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
    public class LogImageCollection : List<LogImage>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private Database _db = null;
        private int _updateCounter = 0;
        private bool _dataHasChanged = false;

        private Hashtable _qaItems = new Hashtable();
        private bool _sorted = false;

        //log image grouping by log
        private Hashtable _logGroups = new Hashtable();

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event EventHandler LogImageDataChanged; //data of a log has changed after log image .EndUpdate()
        public event PropertyChangedEventHandler LogImagePropertyChanged; //property of a log has changed
        public event PropertyChangedEventHandler PropertyChanged;

        public LogImageCollection(Database db)
        {
            _db = db;
        }

        public LogImage GetLogImage(string id)
        {
            return _qaItems[id] as LogImage;
        }

        public List<LogImage> GetLogImages(string logId)
        {
            List<LogImage> result;
            Hashtable ht = _logGroups[logId] as Hashtable;
            if (ht != null)
            {
                result = (from LogImage l in ht.Values select l).OrderBy(x => x.ID).ToList();
            }
            else
            {
                return new List<LogImage>();
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

        public new void Add(LogImage gc)
        {
            _qaItems[gc.ID] = gc;
            _sorted = false;

            //grouping
            if (_logGroups[gc.LogId ?? ""] == null)
            {
                _logGroups[gc.LogId ?? ""] = new Hashtable();
            }
            (_logGroups[gc.LogId ?? ""] as Hashtable)[gc.ID] = gc;
            //end grouping

            gc.DataChanged += gc_DataChanged;
            gc.PropertyChanged += gc_PropertyChanged;
            base.Add(gc);

            if (_updateCounter <= 0)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            else
            {
                _dataHasChanged = true;
            }
        }

        public new int IndexOf(LogImage gc)
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
            LogImage gc = this[index];
            _qaItems.Remove(gc.ID);
            //grouping
            (_logGroups[gc.LogId ?? ""] as Hashtable).Remove(gc.ID);
            //end grouping

            gc.DataChanged -= gc_DataChanged;
            gc.PropertyChanged -= gc_PropertyChanged;
            base.RemoveAt(index);

            if (_updateCounter <= 0)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            else
            {
                _dataHasChanged = true;
            }

        }

        public new void Remove(LogImage gc)
        {
            RemoveAt(this.IndexOf(gc));
        }

        public new void Clear()
        {
            foreach (LogImage gc in this)
            {
                gc.DataChanged -= gc_DataChanged;
                gc.PropertyChanged -= gc_PropertyChanged;
            }
            _qaItems.Clear();
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
                if (LogImagePropertyChanged != null)
                {
                    LogImagePropertyChanged(sender, e);
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
                if (LogImageDataChanged != null)
                {
                    LogImageDataChanged(sender, e);
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
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Count"));
            }
            _dataHasChanged = false;
        }
    }
}
