using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Data
{
    public class UserWaypointCollection: ArrayList
    {
        public event Framework.EventArguments.UserWaypointEventHandler UserWaypointAdded;
        public event Framework.EventArguments.UserWaypointEventHandler UserWaypointRemoved;
        public event EventArguments.UserWaypointEventHandler DataChanged;

        //events in UI context (on the list, after EndUpdate)
        public event EventHandler ListDataChanged;

        private bool _updating = false;
        private bool _dataChanged = false;
        private int _updatingCounter = 0;

        private Hashtable _qaItems = new Hashtable();
        private List<UserWaypoint> _batchAddition = null;

        public Framework.Data.UserWaypoint getWaypoint(int id)
        {
            return _qaItems[id] as Framework.Data.UserWaypoint;
        }

        public override int Add(object value)
        {
            int result;
            Framework.Data.UserWaypoint wp = value as Framework.Data.UserWaypoint;
            if (wp != null)
            {
                _qaItems[wp.ID] = wp;
                result = base.Add(wp);
                wp.DataChanged += new EventArguments.UserWaypointEventHandler(gc_DataChanged);
                OnUserWaypointAdded(wp);
            }
            else 
            {
                //for now, only supported between begin and end update
                List<UserWaypoint> lgs = value as List<UserWaypoint>;
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

        void gc_DataChanged(object sender, EventArguments.UserWaypointEventArgs e)
        {
            OnDataChanged(e.Waypoint);
        }

        public override void Remove(object value)
        {
            Framework.Data.UserWaypoint wp = value as Framework.Data.UserWaypoint;
            if (wp!=null)
            {
                _qaItems.Remove(wp.ID);
                wp.DataChanged -= new EventArguments.UserWaypointEventHandler(gc_DataChanged);

                base.Remove(wp);
                OnUserWaypointRemoved(wp);
            }
        }
        public override void RemoveAt(int index)
        {
            if (index >= 0 && index < Count)
            {
                Framework.Data.UserWaypoint wp = this[index] as Framework.Data.UserWaypoint;
                _qaItems.Remove(wp.ID);
                wp.DataChanged -= new EventArguments.UserWaypointEventHandler(gc_DataChanged);
                base.RemoveAt(index);
                OnUserWaypointRemoved(wp);
            }
        }
        public override void Clear()
        {
            BeginUpdate();
            if (this.Count > 0)
            {
                _qaItems.Clear();
                foreach (Framework.Data.UserWaypoint gc in this)
                {
                    gc.DataChanged -= new EventArguments.UserWaypointEventHandler(gc_DataChanged);
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
                        foreach (UserWaypoint l in _batchAddition)
                        {
                            if (_qaItems[l.ID] == null)
                            {
                                _qaItems[l.ID] = l;
                                base.Add(l);
                                l.DataChanged += new EventArguments.UserWaypointEventHandler(gc_DataChanged);
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

        public void OnUserWaypointAdded(Framework.Data.UserWaypoint wp)
        {
            if (!_updating && UserWaypointAdded != null)
            {
                UserWaypointAdded(this, new Framework.EventArguments.UserWaypointEventArgs(wp));
            }
            OnListDataChanged();
        }
        public void OnUserWaypointRemoved(Framework.Data.UserWaypoint wp)
        {
            if (!_updating && UserWaypointRemoved != null)
            {
                UserWaypointRemoved(this, new Framework.EventArguments.UserWaypointEventArgs(wp));
            }
            OnListDataChanged();
        }

        public void OnDataChanged(Framework.Data.UserWaypoint wp)
        {
            _dataChanged = true;
            if (!_updating && DataChanged != null)
            {
                DataChanged(this, new Framework.EventArguments.UserWaypointEventArgs(wp));
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
                foreach (UserWaypoint wp in this)
                {
                    wp.IsDataChanged = false;
                }
            }
        }
    }
}
