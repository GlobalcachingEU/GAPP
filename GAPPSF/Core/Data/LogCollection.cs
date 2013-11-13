using GAPPSF.Core.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public class LogCollection : List<Log>, INotifyCollectionChanged
    {
        private Database _db = null;
        private int _updateCounter = 0;
        private bool _dataHasChanged = false;

        private Hashtable _qaItems = new Hashtable();
        private bool _sorted = false;

        //log grouping by GeocacheCode
        private Hashtable _logGroups = new Hashtable();

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event EventHandler LogDataChanged; //data of a log has changed after Geocache.EndUpdate()
        public event PropertyChangedEventHandler LogPropertyChanged; //property of a log has changed

        public LogCollection(Database db)
        {
            _db = db;
        }

        public Log GetLog(string id)
        {
            return _qaItems[id] as Log;
        }

        public List<Log> GetLogs(string geocacheCode)
        {
            List<Log> result;
            Hashtable ht = _logGroups[geocacheCode] as Hashtable;
            if (ht != null)
            {
                result = (from Log l in ht.Values select l).OrderByDescending(x => x.Date).ThenByDescending(x => x.ID).ToList();
            }
            else
            {
                return new List<Log>();
            }
            return result;
        }

        public void BeginUpdate()
        {
            _updateCounter++;
        }
        public void EndUpdate()
        {
            _updateCounter--;
            if (_updateCounter <= 0)
            {
                if (_dataHasChanged)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }
        }

        public new void Add(Log gc)
        {
            _qaItems[gc.ID] = gc;
            _sorted = false;

            //grouping
            if (_logGroups[gc.GeocacheCode ?? ""] == null)
            {
                _logGroups[gc.GeocacheCode ?? ""] = new Hashtable();
            }
            (_logGroups[gc.GeocacheCode ?? ""] as Hashtable)[gc.ID] = gc;
            //end grouping

            gc.DataChanged += gc_DataChanged;
            gc.PropertyChanged += gc_PropertyChanged;
            base.Add(gc);

            if (_updateCounter <= 0)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
            }
            else
            {
                _dataHasChanged = true;
            }
        }

        public new int IndexOf(Log gc)
        {
            if (!_sorted)
            {
                Sort();
                _sorted = true;
            }
            return BinarySearch(gc);
        }

        public new void RemoveAt(int index)
        {
            Log gc = this[index];
            //grouping
            (_logGroups[gc.GeocacheCode ?? ""] as Hashtable).Remove(gc.ID);
            //end grouping

            gc.DataChanged -= gc_DataChanged;
            gc.PropertyChanged -= gc_PropertyChanged;
            base.RemoveAt(index);

            if (_updateCounter <= 0)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));
            }
            else
            {
                _dataHasChanged = true;
            }

        }

        public new void Remove(Log gc)
        {
            RemoveAt(this.IndexOf(gc));
        }

        public new void Clear()
        {
            foreach (Log gc in this)
            {
                gc.DataChanged -= gc_DataChanged;
                gc.PropertyChanged -= gc_PropertyChanged;
            }
            base.Clear();
            _logGroups.Clear();

            if (_updateCounter <= 0)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            else
            {
                _dataHasChanged = true;
            }
        }

        void gc_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_updateCounter <= 0)
            {
                if (LogPropertyChanged != null)
                {
                    LogPropertyChanged(sender, e);
                }
            }
            else
            {
                _dataHasChanged = true;
            }
        }

        void gc_DataChanged(object sender, EventArgs e)
        {
            if (_updateCounter <= 0)
            {
                if (LogDataChanged != null)
                {
                    LogDataChanged(sender, e);
                }
            }
            else
            {
                _dataHasChanged = true;
            }
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
            _dataHasChanged = false;
        }
    }
}
