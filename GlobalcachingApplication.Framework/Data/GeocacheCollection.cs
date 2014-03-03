using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Data
{
    /// <summary>
    /// Contains the collection of geocaches. The list is not threadsafe and processes changing this collection should be started
    /// with BeginUpdate and end with EndUpdate within the UI context
    /// </summary>
    public class GeocacheCollection: ArrayList
    {
        //events in UI context (per geocache)
        public event Framework.EventArguments.GeocacheEventHandler GeocacheAdded;
        public event Framework.EventArguments.GeocacheEventHandler GeocacheRemoved;
        public event Framework.EventArguments.GeocacheEventHandler GeocacheSelectedChanged;
        public event EventArguments.GeocacheEventHandler SelectedChanged;
        public event EventArguments.GeocacheEventHandler DataChanged;
        public event EventArguments.LoadFullGeocacheEventHandler LoadFullData;

        //events in UI context (on the list, after EndUpdate)
        public event EventHandler ListSelectionChanged;
        public event EventHandler ListDataChanged;

        private bool _updating = false;
        private bool _dataChanged = false;
        private bool _selectedChanged = false;
        private int _updatingCounter = 0;
        private List<string> _customAttributes = new List<string>();

        private Interfaces.ICore _core = null;

        private Hashtable _qaItems = new Hashtable();
        private bool _sorted = false;

        public GeocacheCollection(Framework.Interfaces.ICore core)
        {
            _core = core;
            if (_core != null && _core.Logs != null)
            {
                _core.Logs.ListDataChanged += new EventHandler(Logs_ListDataChanged);
                _core.Logs.LogAdded += new EventArguments.LogEventHandler(Logs_LogAdded);
                _core.Logs.LogRemoved += new EventArguments.LogEventHandler(Logs_LogRemoved);

                _core.UserWaypoints.ListDataChanged += new EventHandler(UserWaypoints_ListDataChanged);
                _core.UserWaypoints.UserWaypointAdded += new EventArguments.UserWaypointEventHandler(UserWaypoints_UserWaypointAdded);
                _core.UserWaypoints.UserWaypointRemoved += new EventArguments.UserWaypointEventHandler(UserWaypoints_UserWaypointRemoved);

                _core.Waypoints.ListDataChanged += new EventHandler(Waypoints_ListDataChanged);
                _core.Waypoints.WaypointAdded += new EventArguments.WaypointEventHandler(Waypoints_WaypointAdded);
                _core.Waypoints.WaypointRemoved += new EventArguments.WaypointEventHandler(Waypoints_WaypointRemoved);
            }
        }

        void Waypoints_WaypointRemoved(object sender, EventArguments.WaypointEventArgs e)
        {
            foreach (Framework.Data.Geocache gc in this)
            {
                if (gc.Code == e.Waypoint.GeocacheCode)
                {
                    gc.ResetWaypointsData();
                    break;
                }
            }
        }

        void Waypoints_WaypointAdded(object sender, EventArguments.WaypointEventArgs e)
        {
            foreach (Framework.Data.Geocache gc in this)
            {
                if (gc.Code == e.Waypoint.GeocacheCode)
                {
                    gc.ResetWaypointsData();
                    break;
                }
            }
        }

        void Waypoints_ListDataChanged(object sender, EventArgs e)
        {
            foreach (Framework.Data.Geocache gc in this)
            {
                gc.ResetWaypointsData();
            }
        }

        void UserWaypoints_UserWaypointRemoved(object sender, EventArguments.UserWaypointEventArgs e)
        {
            foreach (Framework.Data.Geocache gc in this)
            {
                if (gc.Code == e.Waypoint.GeocacheCode)
                {
                    gc.ResetCachedUserWaypointsData();
                    break;
                }
            }            
        }

        void UserWaypoints_UserWaypointAdded(object sender, EventArguments.UserWaypointEventArgs e)
        {
            foreach (Framework.Data.Geocache gc in this)
            {
                if (gc.Code == e.Waypoint.GeocacheCode)
                {
                    if (!gc.HasUserWaypoints)
                    {
                        gc.ResetCachedUserWaypointsData();
                    }
                    break;
                }
            }
        }

        void UserWaypoints_ListDataChanged(object sender, EventArgs e)
        {
            foreach (Framework.Data.Geocache gc in this)
            {
                gc.ResetCachedUserWaypointsData();
            }            
        }

        void Logs_LogRemoved(object sender, EventArguments.LogEventArgs e)
        {
            foreach (Framework.Data.Geocache gc in this)
            {
                if (gc.Code == e.Log.GeocacheCode)
                {
                    gc.ResetCachedLogData();
                }
            }
        }

        void Logs_LogAdded(object sender, EventArguments.LogEventArgs e)
        {
            foreach (Framework.Data.Geocache gc in this)
            {
                if (gc.Code == e.Log.GeocacheCode)
                {
                    gc.ResetCachedLogData();
                }
            }            
        }

        void Logs_ListDataChanged(object sender, EventArgs e)
        {
            foreach (Framework.Data.Geocache gc in this)
            {
                gc.ResetCachedLogData();
            }            
        }

        public string[] CustomAttributes
        {
            get { return _customAttributes.ToArray(); }
        }

        public void AddCustomAttribute(string name)
        {
            if (!_customAttributes.Contains(name))
            {
                Geocache.CustomAttributesKeys.Add(name);
                _customAttributes.Add(name);
                OnListDataChanged();
            }
        }

        public void DeleteCustomAttribute(string name)
        {
            if (_customAttributes.Contains(name))
            {
                Geocache.CustomAttributesKeys.Remove(name);
                _customAttributes.Remove(name);
                foreach (Framework.Data.Geocache gc in this)
                {
                    gc.DeleteCustomAttribute(name);
                }
                OnListDataChanged();
            }
        }

        public Framework.Data.Geocache GetGeocache(string code)
        {
            return _qaItems[code] as Framework.Data.Geocache;
        }

        public override int Add(object value)
        {
            int result;
            Framework.Data.Geocache gc = value as Framework.Data.Geocache;
            if (gc!=null)
            {
                _qaItems[gc.Code] = gc;
                gc.Core = _core;
                _sorted = false;
                result = base.Add(gc);
                gc.DataChanged += new EventArguments.GeocacheEventHandler(gc_DataChanged);
                gc.SelectedChanged += new EventArguments.GeocacheEventHandler(gc_SelectedChanged);
                gc.LoadFullData += new EventArguments.LoadFullGeocacheEventHandler(gc_LoadFullData);
                OnCacheAdded(gc);
            }
            else 
            {
                //for now, only supported between begin and end update
                List<Geocache> lgs = value as List<Geocache>;
                if (lgs != null)
                {
                    if (lgs.Count > 0)
                    {
                        foreach (Geocache l in lgs)
                        {
                            if (_qaItems[l.Code] == null)
                            {
                                l.Core = _core;
                                _qaItems[l.Code] = l;
                                _sorted = false;
                                base.Add(l);
                                l.DataChanged += new EventArguments.GeocacheEventHandler(gc_DataChanged);
                                l.SelectedChanged += new EventArguments.GeocacheEventHandler(gc_SelectedChanged);
                                l.LoadFullData += new EventArguments.LoadFullGeocacheEventHandler(gc_LoadFullData);
                            }
                        }
                        _dataChanged = true;
                    }
                    result = 0;
                }
                else
                {
                    result = -1;
                }
            }
            return result;
        }

        void gc_LoadFullData(object sender, EventArguments.LoadFullGeocacheEventArgs e)
        {
            OnLoadFullData(e);
        }

        void gc_SelectedChanged(object sender, EventArguments.GeocacheEventArgs e)
        {
            OnGeocacheSelectedChanged(e.Geocache);
        }

        void gc_DataChanged(object sender, EventArguments.GeocacheEventArgs e)
        {
            OnDataChanged(e.Geocache);
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
            Framework.Data.Geocache gc = value as Framework.Data.Geocache;
            if (gc!=null)
            {
                RemoveAt(IndexOf(gc));
            }
        }
        public override void RemoveAt(int index)
        {
            if (index >= 0 && index < Count)
            {
                Framework.Data.Geocache gc = this[index] as Framework.Data.Geocache;
                _qaItems.Remove(gc.Code);
                gc.DataChanged -= new EventArguments.GeocacheEventHandler(gc_DataChanged);
                gc.SelectedChanged -= new EventArguments.GeocacheEventHandler(gc_SelectedChanged);
                gc.LoadFullData -= new EventArguments.LoadFullGeocacheEventHandler(gc_LoadFullData);
                base.RemoveAt(index);
                OnCacheRemoved(gc);
            }
        }
        public override void Clear()
        {
            BeginUpdate();
            if (this.Count>0)
            {
                _qaItems.Clear();
                foreach (Framework.Data.Geocache gc in this)
                {
                    gc.DataChanged -= new EventArguments.GeocacheEventHandler(gc_DataChanged);
                    gc.SelectedChanged -= new EventArguments.GeocacheEventHandler(gc_SelectedChanged);
                    gc.LoadFullData -= new EventArguments.LoadFullGeocacheEventHandler(gc_LoadFullData);
                }
                _dataChanged = true;
                base.Clear();
            }
            Geocache.CustomAttributesKeys.Clear();
            _customAttributes.Clear();
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
                _updating = false;
                _updatingCounter = 0;
                if (_dataChanged)
                {
                    OnListDataChanged();
                }
                else if (_selectedChanged) //only want one event, data means anything, cache added, removed, prop changed
                {
                    OnListSelectionChanged();
                }
            }
        }

        public void OnCacheAdded(Framework.Data.Geocache gc)
        {
            if (!_updating && GeocacheAdded != null)
            {
                GeocacheAdded(this, new Framework.EventArguments.GeocacheEventArgs(gc));
            }
            OnListDataChanged();
        }
        public void OnCacheRemoved(Framework.Data.Geocache gc)
        {
            if (!_updating && GeocacheRemoved != null)
            {
                GeocacheRemoved(this, new Framework.EventArguments.GeocacheEventArgs(gc));
            }
            if (gc.Selected)
            {
                OnListSelectionChanged();
            }
            OnListDataChanged();
        }

        public void OnGeocacheSelectedChanged(Framework.Data.Geocache gc)
        {
            if (_updating)
            {
                _selectedChanged = true;
            }
            else
            {
                if (GeocacheSelectedChanged != null)
                {
                    GeocacheSelectedChanged(this, new Framework.EventArguments.GeocacheEventArgs(gc));
                }
                if (SelectedChanged != null)
                {
                    SelectedChanged(this, new Framework.EventArguments.GeocacheEventArgs(gc));
                }
            }
        }

        public void OnSelectedChanged(Framework.Data.Geocache gc)
        {
            _selectedChanged = true;
            if (!_updating && SelectedChanged != null)
            {
                SelectedChanged(this, new Framework.EventArguments.GeocacheEventArgs(gc));
                _selectedChanged = false;
            }
        }
        public void OnDataChanged(Framework.Data.Geocache gc)
        {
            _dataChanged = true;
            if (!_updating && DataChanged != null)
            {
                DataChanged(this, new Framework.EventArguments.GeocacheEventArgs(gc));
                _dataChanged = false;
                gc.IsDataChanged = false;
            }
        }

        public void OnListSelectionChanged()
        {
            if (!_updating && ListSelectionChanged != null)
            {
                ListSelectionChanged(this, EventArgs.Empty);
                _selectedChanged = true;
            }
        }
        public void OnListDataChanged()
        {
            _dataChanged = true;
            if (!_updating && ListDataChanged != null)
            {
                ListDataChanged(this, EventArgs.Empty);
                _dataChanged = false;
                foreach (Geocache gc in this)
                {
                    gc.IsDataChanged = false;
                }
            }
        }

        public void OnLoadFullData(Framework.EventArguments.LoadFullGeocacheEventArgs e)
        {
            if (LoadFullData != null)
            {
                LoadFullData(this, e);
            }
        }

    }
}
