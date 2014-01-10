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
    public class GeocacheCollection: List<Geocache>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private Database _db = null;
        private int _updateCounter = 0;
        private bool _dataHasChanged = false;

        private Hashtable _qaItems = new Hashtable();
        private bool _sorted = false;

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event EventHandler GeocacheDataChanged; //data of a geocache has changed after Geocache.EndUpdate()
        public event PropertyChangedEventHandler GeocachePropertyChanged; //property of a geocache has changed
        public event PropertyChangedEventHandler PropertyChanged;


        public GeocacheCollection(Database db)
        {
            _db = db;

            _db.LogCollection.CollectionChanged += LogCollection_CollectionChanged;
            _db.LogCollection.LogDataChanged += LogCollection_LogDataChanged;
            _db.LogCollection.LogPropertyChanged += LogCollection_LogPropertyChanged;

            _db.UserWaypointCollection.CollectionChanged += UserWaypointCollection_CollectionChanged;
            _db.UserWaypointCollection.WaypointDataChanged += UserWaypointCollection_WaypointDataChanged;
            _db.UserWaypointCollection.WaypointPropertyChanged += UserWaypointCollection_WaypointPropertyChanged;

            _db.WaypointCollection.CollectionChanged += WaypointCollection_CollectionChanged;
            _db.WaypointCollection.WaypointDataChanged += WaypointCollection_WaypointDataChanged;
            _db.WaypointCollection.WaypointPropertyChanged += WaypointCollection_WaypointPropertyChanged;
        }

        void WaypointCollection_WaypointPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            WaypointCollection_WaypointDataChanged(sender, EventArgs.Empty);
        }

        void WaypointCollection_WaypointDataChanged(object sender, EventArgs e)
        {
            Data.Waypoint l = sender as Data.Waypoint;
            if (l != null)
            {
                if (!string.IsNullOrEmpty(l.GeocacheCode))
                {
                    Data.Geocache gc = GetGeocache(l.GeocacheCode);
                    if (gc != null)
                    {
                        gc.ResetWaypointsData();
                    }
                }
            }
        }

        void WaypointCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (Data.Geocache gc in this)
            {
                gc.ResetWaypointsData();
            }
        }

        void UserWaypointCollection_WaypointPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UserWaypointCollection_WaypointDataChanged(sender, EventArgs.Empty);
        }

        void UserWaypointCollection_WaypointDataChanged(object sender, EventArgs e)
        {
            Data.UserWaypoint l = sender as Data.UserWaypoint;
            if (l != null)
            {
                if (!string.IsNullOrEmpty(l.GeocacheCode))
                {
                    Data.Geocache gc = GetGeocache(l.GeocacheCode);
                    if (gc != null)
                    {
                        gc.ResetCachedUserWaypointsData();
                    }
                }
            }
        }

        void UserWaypointCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (Data.Geocache gc in this)
            {
                gc.ResetCachedUserWaypointsData();
            }
        }

        void LogCollection_LogPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            LogCollection_LogDataChanged(sender, EventArgs.Empty);
        }

        void LogCollection_LogDataChanged(object sender, EventArgs e)
        {
            Data.Log l = sender as Data.Log;
            if (l != null)
            {
                if (!string.IsNullOrEmpty(l.GeocacheCode))
                {
                    Data.Geocache gc = GetGeocache(l.GeocacheCode);
                    if (gc != null)
                    {
                        gc.ResetCachedLogData();
                    }
                }
            }
        }

        void LogCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (Data.Geocache gc in this)
            {
                gc.ResetCachedLogData();
            }
        }

        public Geocache GetGeocache(string code)
        {
            return _qaItems[code] as Geocache;
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

        public new void Add(Geocache gc)
        {
            _qaItems[gc.Code] = gc;
            _sorted = false;

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

        public new int IndexOf(Geocache gc)
        {
            if (!_sorted)
            {
                Sort();
                _sorted = true;
            }
            return BinarySearch(gc);
        }

        public new void Remove(Geocache gc)
        {
            _qaItems.Remove(gc.Code);
            gc.DataChanged -= gc_DataChanged;
            gc.PropertyChanged -= gc_PropertyChanged;
            base.RemoveAt(this.IndexOf(gc));

            if (_updateCounter <= 0)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            else
            {
                _dataHasChanged = true;
            }
        }

        public new void Clear()
        {
            foreach (Geocache gc in this)
            {
                gc.DataChanged -= gc_DataChanged;
                gc.PropertyChanged -= gc_PropertyChanged;
            }
            _qaItems.Clear();
            base.Clear();

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
                if (GeocachePropertyChanged != null)
                {
                    GeocachePropertyChanged(sender, e);
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
                if (GeocacheDataChanged != null)
                {
                    GeocacheDataChanged(sender, e);
                }
            }
            else
            {
                _dataHasChanged = true;
            }
        }

        public virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
            if (PropertyChanged!=null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Count"));
            }
            _dataHasChanged = false;
        }


    }
}
