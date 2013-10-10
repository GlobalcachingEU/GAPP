using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Data
{
    /// <summary>
    /// Contains the collection of Logs. The list is not threadsafe and processes changing this collection should be started
    /// with BeginUpdate and end with EndUpdate within the UI context
    /// </summary>
    public class LogImageCollection: ArrayList
    {
        //events in UI context (per geocache)
        public event Framework.EventArguments.LogImageEventHandler LogImageAdded;
        public event Framework.EventArguments.LogImageEventHandler LogImageRemoved;
        public event EventArguments.LogImageEventHandler DataChanged;

        //events in UI context (on the list, after EndUpdate)
        public event EventHandler ListDataChanged;

        private bool _updating = false;
        private bool _dataChanged = false;
        private int _updatingCounter = 0;

        private Hashtable _qaItems = new Hashtable();
        private List<LogImage> _batchAddition = null;
        private bool _sorted = false;

        //log grouping by GeocacheCode
        private Hashtable _logGroups = new Hashtable();

        public Framework.Data.LogImage GetLogImage(string id)
        {
            return _qaItems[id] as Framework.Data.LogImage;
        }

        public List<Framework.Data.LogImage> GetLogImages(string logId)
        {
            List<Framework.Data.LogImage> result;
            Hashtable ht = _logGroups[logId] as Hashtable;
            if (ht != null)
            {
                result = (from LogImage l in ht.Values select l).ToList();
            }
            else
            {
                return new List<LogImage>();
            }
            return result;
        }

        public override int Add(object value)
        {
            int result;
            Framework.Data.LogImage li = value as Framework.Data.LogImage;
            if (li != null)
            {
                _qaItems[li.ID] = li;
                //grouping
                if (_logGroups[li.LogID ?? ""] == null)
                {
                    _logGroups[li.LogID ?? ""] = new Hashtable();
                }
                (_logGroups[li.LogID ?? ""] as Hashtable)[li.ID] = li;
                //end grouping
                _sorted = false;
                result = base.Add(li);
                li.DataChanged += new EventArguments.LogImageEventHandler(gc_DataChanged);
                OnLogImageAdded(li);
            }
            else 
            {
                //for now, only supported between begin and end update
                List<LogImage> lgs = value as List<LogImage>;
                if (lgs != null)
                {
                    _batchAddition = lgs;
                    result = 0;
                }
                else
                {
                    result = -1;
                }
            }
            return result;
        }

        void gc_DataChanged(object sender, EventArguments.LogImageEventArgs e)
        {
            OnDataChanged(e.LogImage);
        }

        public override int IndexOf(object value)
        {
            if (!_sorted)
            {
                Sort();
                _sorted = true;
            }
            return BinarySearch(value);
        }

        public override void Remove(object value)
        {
            Framework.Data.LogImage li = value as Framework.Data.LogImage;
            if (li!=null)
            {
                RemoveAt(IndexOf(li));
            }
        }
        public override void RemoveAt(int index)
        {
            if (index >= 0 && index < Count)
            {
                Framework.Data.LogImage li = this[index] as Framework.Data.LogImage;
                //grouping
                (_logGroups[li.LogID ?? ""] as Hashtable).Remove(li.ID);
                //end grouping
                _qaItems.Remove(li.ID);
                li.DataChanged -= new EventArguments.LogImageEventHandler(gc_DataChanged);
                base.RemoveAt(index);
                OnLogImageRemoved(li);
            }
        }
        public override void Clear()
        {
            BeginUpdate();
            if (this.Count > 0)
            {
                //grouping
                _logGroups.Clear();
                //end grouping
                _qaItems.Clear();
                foreach (Framework.Data.LogImage gc in this)
                {
                    gc.DataChanged -= new EventArguments.LogImageEventHandler(gc_DataChanged);
                }
                base.Clear();
                _dataChanged = true;
            }
            EndUpdate();
        }
        public override void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }
        public override void AddRange(ICollection c)
        {
            throw new NotImplementedException();
        }

        public void BeginUpdate()
        {
            _updating = true;
            _updatingCounter++;
        }
        public void EndUpdate()
        {
            _updatingCounter--;
            if (_updatingCounter <= 0)
            {
                if (_batchAddition != null)
                {
                    if (_batchAddition.Count > 0)
                    {
                        foreach (LogImage l in _batchAddition)
                        {
                            if (_qaItems[l.ID] == null)
                            {
                                _qaItems[l.ID] = l;
                                //grouping
                                if (_logGroups[l.LogID ?? ""] == null)
                                {
                                    _logGroups[l.LogID ?? ""] = new Hashtable();
                                }
                                (_logGroups[l.LogID ?? ""] as Hashtable)[l.ID] = l;
                                //end grouping

                                _sorted = false;
                                base.Add(l);
                                l.DataChanged += new EventArguments.LogImageEventHandler(gc_DataChanged);
                            }
                        }
                        _dataChanged = true;
                    }
                    _batchAddition.Clear();
                    _batchAddition = null;
                }

                _updating = false;
                _updatingCounter = 0;
                if (_dataChanged)
                {
                    OnListDataChanged();
                }
            }
        }

        public void OnLogImageAdded(Framework.Data.LogImage li)
        {
            if (!_updating && LogImageAdded != null)
            {
                LogImageAdded(this, new Framework.EventArguments.LogImageEventArgs(li));
            }
            OnListDataChanged();
        }
        public void OnLogImageRemoved(Framework.Data.LogImage li)
        {
            if (!_updating && LogImageRemoved != null)
            {
                LogImageRemoved(this, new Framework.EventArguments.LogImageEventArgs(li));
            }
            OnListDataChanged();
        }

        public void OnDataChanged(Framework.Data.LogImage li)
        {
            _dataChanged = true;
            if (!_updating && DataChanged != null)
            {
                DataChanged(this, new Framework.EventArguments.LogImageEventArgs(li));
                _dataChanged = false;
                li.IsDataChanged = false;
            }
        }

        public void OnListDataChanged()
        {
            _dataChanged = true;
            if (!_updating && ListDataChanged != null)
            {
                ListDataChanged(this, EventArgs.Empty);
                _dataChanged = false;
                foreach(LogImage l in this)
                {
                    l.IsDataChanged = false;
                }
            }
        }

    }
}
