using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Data
{
    /// <summary>
    /// Contains the collection of waypoints. The list is not threadsafe and processes changing this collection should be started
    /// with BeginUpdate and end with EndUpdate within the UI context
    /// </summary>
    public class WaypointCollection: ArrayList
    {
        //events in UI context (per geocache)
        public event Framework.EventArguments.WaypointEventHandler WaypointAdded;
        public event Framework.EventArguments.WaypointEventHandler WaypointRemoved;
        public event EventArguments.WaypointEventHandler DataChanged;

        //events in UI context (on the list, after EndUpdate)
        public event EventHandler ListDataChanged;

        private bool _updating = false;
        private bool _dataChanged = false;
        private int _updatingCounter = 0;

        private Hashtable _qaItems = new Hashtable();
        private List<Waypoint> _batchAddition = null;
        private bool _sorted = false;

        //wp grouping by GeocacheCode
        private Hashtable _wpGroups = new Hashtable();

        public Framework.Data.Waypoint getWaypoint(string code)
        {
            return _qaItems[code] as Framework.Data.Waypoint;
        }

        public List<Framework.Data.Waypoint> GetWaypoints(string geocacheCode)
        {
            List<Framework.Data.Waypoint> result;
            Hashtable ht = _wpGroups[geocacheCode] as Hashtable;
            if (ht != null)
            {
                result = (from Waypoint l in ht.Values select l).OrderBy(x => x.Code).ToList();
            }
            else
            {
                return new List<Waypoint>();
            }
            return result;
        }

        public override int Add(object value)
        {
            int result;
            Framework.Data.Waypoint wp = value as Framework.Data.Waypoint;
            if (wp != null)
            {
                //grouping
                if (_wpGroups[wp.GeocacheCode ?? ""] == null)
                {
                    _wpGroups[wp.GeocacheCode ?? ""] = new Hashtable();
                }
                (_wpGroups[wp.GeocacheCode ?? ""] as Hashtable)[wp.Code] = wp;
                //end grouping
                _qaItems[wp.Code] = wp;
                _sorted = false;
                result = base.Add(wp);
                wp.DataChanged += new EventArguments.WaypointEventHandler(gc_DataChanged);
                OnWaypointAdded(wp);
            }
            else 
            {
                //for now, only supported between begin and end update
                List<Waypoint> lgs = value as List<Waypoint>;
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

        void gc_DataChanged(object sender, EventArguments.WaypointEventArgs e)
        {
            OnDataChanged(e.Waypoint);
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
            Framework.Data.Waypoint wp = value as Framework.Data.Waypoint;
            if (wp!=null)
            {
                RemoveAt(IndexOf(wp));
            }
        }
        public override void RemoveAt(int index)
        {
            if (index >= 0 && index < Count)
            {
                Framework.Data.Waypoint wp = this[index] as Framework.Data.Waypoint;
                //grouping
                (_wpGroups[wp.GeocacheCode ?? ""] as Hashtable).Remove(wp.Code);
                //end grouping

                _qaItems.Remove(wp.Code);
                wp.DataChanged -= new EventArguments.WaypointEventHandler(gc_DataChanged);
                base.RemoveAt(index);
                OnWaypointRemoved(wp);
            }
        }
        public override void Clear()
        {
            BeginUpdate();
            if (this.Count > 0)
            {
                //grouping
                _wpGroups.Clear();
                //end grouping

                _qaItems.Clear();
                foreach (Framework.Data.Waypoint gc in this)
                {
                    gc.DataChanged -= new EventArguments.WaypointEventHandler(gc_DataChanged);
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
                        foreach (Waypoint l in _batchAddition)
                        {
                            if (_qaItems[l.ID] == null)
                            {
                                //grouping
                                if (_wpGroups[l.GeocacheCode ?? ""] == null)
                                {
                                    _wpGroups[l.GeocacheCode ?? ""] = new Hashtable();
                                }
                                (_wpGroups[l.GeocacheCode ?? ""] as Hashtable)[l.Code] = l;
                                //end grouping

                                _qaItems[l.Code] = l;
                                _sorted = false;
                                base.Add(l);
                                l.DataChanged += new EventArguments.WaypointEventHandler(gc_DataChanged);
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

        public void OnWaypointAdded(Framework.Data.Waypoint wp)
        {
            if (!_updating && WaypointAdded != null)
            {
                WaypointAdded(this, new Framework.EventArguments.WaypointEventArgs(wp));
            }
            OnListDataChanged();
        }
        public void OnWaypointRemoved(Framework.Data.Waypoint wp)
        {
            if (!_updating && WaypointRemoved != null)
            {
                WaypointRemoved(this, new Framework.EventArguments.WaypointEventArgs(wp));
            }
            OnListDataChanged();
        }

        public void OnDataChanged(Framework.Data.Waypoint wp)
        {
            _dataChanged = true;
            if (!_updating && DataChanged != null)
            {
                DataChanged(this, new Framework.EventArguments.WaypointEventArgs(wp));
                _dataChanged = false;
                wp.IsDataChanged = false;
            }
        }

        public void OnListDataChanged()
        {
            _dataChanged = true;
            if (!_updating && ListDataChanged != null)
            {
                ListDataChanged(this, EventArgs.Empty);
                _dataChanged = false;
                foreach (Waypoint wp in this)
                {
                    wp.IsDataChanged = false;
                }
            }
        }

    }
}
