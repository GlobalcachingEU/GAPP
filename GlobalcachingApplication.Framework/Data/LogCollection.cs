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
    public class LogCollection: ArrayList
    {
        //events in UI context (per geocache)
        public event Framework.EventArguments.LogEventHandler LogAdded;
        public event Framework.EventArguments.LogEventHandler LogRemoved;
        public event EventArguments.LogEventHandler DataChanged;
        public event EventArguments.LoadFullLogEventHandler LoadFullData;

        //events in UI context (on the list, after EndUpdate)
        public event EventHandler ListDataChanged;

        private bool _updating = false;
        private bool _dataChanged = false;
        private int _updatingCounter = 0;

        private Hashtable _qaItems = new Hashtable();
        private List<Log> _batchAddition = null;
        private bool _sorted = false;

        //log grouping by GeocacheCode
        private Hashtable _logGroups = new Hashtable();

        public LogCollection()
        {
        }

        public Framework.Data.Log GetLog(string id)
        {
            return _qaItems[id] as Framework.Data.Log;
        }

        public List<Framework.Data.Log> GetLogs(string geocacheCode)
        {
            List<Framework.Data.Log> result;
            Hashtable ht = _logGroups[geocacheCode] as Hashtable;
            if (ht != null)
            {
                result = (from Log l in ht.Values select l).OrderByDescending(x=>x.Date).ThenByDescending(x=>x.ID).ToList();
            }
            else
            {
                return new List<Log>();
            }
            return result;
        }

        public override int Add(object value)
        {
            int result;
            Framework.Data.Log wp = value as Framework.Data.Log;
            if (wp != null)
            {
                _qaItems[wp.ID] = wp;
                //grouping
                if (_logGroups[wp.GeocacheCode ?? ""] == null)
                {
                    _logGroups[wp.GeocacheCode ?? ""] = new Hashtable();
                }
                (_logGroups[wp.GeocacheCode ?? ""] as Hashtable)[wp.ID] = wp;
                //end grouping
                _sorted = false;
                result = base.Add(wp);
                wp.DataChanged += new EventArguments.LogEventHandler(gc_DataChanged);
                wp.LoadFullData += new EventArguments.LoadFullLogEventHandler(wp_LoadFullData);
                OnLogAdded(wp);
            }
            else 
            {
                //for now, only supported between begin and end update
                List<Log> lgs = value as List<Log>;
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

        void wp_LoadFullData(object sender, EventArguments.LoadFullLogEventArgs e)
        {
            OnLoadFullData(e);
        }

        void gc_DataChanged(object sender, EventArguments.LogEventArgs e)
        {
            OnDataChanged(e.Log);
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
            Framework.Data.Log wp = value as Framework.Data.Log;
            if (wp!=null)
            {
                RemoveAt(IndexOf(wp));
            }
        }
        public override void RemoveAt(int index)
        {
            if (index >= 0 && index < Count)
            {
                Framework.Data.Log wp = this[index] as Framework.Data.Log;
                //grouping
                (_logGroups[wp.GeocacheCode ?? ""] as Hashtable).Remove(wp.ID);
                //end grouping
                _qaItems.Remove(wp.ID);
                wp.DataChanged -= new EventArguments.LogEventHandler(gc_DataChanged);
                wp.LoadFullData -= new EventArguments.LoadFullLogEventHandler(wp_LoadFullData);
                base.RemoveAt(index);
                OnLogRemoved(wp);
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
                foreach (Framework.Data.Log gc in this)
                {
                    gc.DataChanged -= new EventArguments.LogEventHandler(gc_DataChanged);
                    gc.LoadFullData -= new EventArguments.LoadFullLogEventHandler(wp_LoadFullData);
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
                        foreach (Log l in _batchAddition)
                        {
                            if (_qaItems[l.ID] == null)
                            {
                                _qaItems[l.ID] = l;
                                //grouping
                                if (_logGroups[l.GeocacheCode ?? ""] == null)
                                {
                                    _logGroups[l.GeocacheCode ?? ""] = new Hashtable();
                                }
                                (_logGroups[l.GeocacheCode ?? ""] as Hashtable)[l.ID] = l;
                                //end grouping
                                _sorted = false;
                                base.Add(l);
                                l.DataChanged += new EventArguments.LogEventHandler(gc_DataChanged);
                                l.LoadFullData += new EventArguments.LoadFullLogEventHandler(wp_LoadFullData);
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

        public void OnLogAdded(Framework.Data.Log wp)
        {
            if (!_updating && LogAdded != null)
            {
                LogAdded(this, new Framework.EventArguments.LogEventArgs(wp));
            }
            OnListDataChanged();
        }
        public void OnLogRemoved(Framework.Data.Log wp)
        {
            if (!_updating && LogRemoved != null)
            {
                LogRemoved(this, new Framework.EventArguments.LogEventArgs(wp));
            }
            OnListDataChanged();
        }

        public void OnDataChanged(Framework.Data.Log wp)
        {
            _dataChanged = true;
            if (!_updating && DataChanged != null)
            {
                DataChanged(this, new Framework.EventArguments.LogEventArgs(wp));
                _dataChanged = false;
                wp.IsDataChanged = false;
            }
        }

        public void OnLoadFullData(Framework.EventArguments.LoadFullLogEventArgs e)
        {
            if (LoadFullData != null)
            {
                LoadFullData(this, e);
            }
        }

        public void OnListDataChanged()
        {
            _dataChanged = true;
            if (!_updating && ListDataChanged != null)
            {
                ListDataChanged(this, EventArgs.Empty);
                _dataChanged = false;
                foreach(Log l in this)
                {
                    l.IsDataChanged = false;
                }
            }
        }

    }
}
