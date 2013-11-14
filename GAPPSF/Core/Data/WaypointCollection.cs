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
    public class WaypointCollection : List<Waypoint>, INotifyCollectionChanged
    {
        private Database _db = null;
        private int _updateCounter = 0;
        private bool _dataHasChanged = false;

        private Hashtable _qaItems = new Hashtable();
        private bool _sorted = false;

        //wp grouping by GeocacheCode
        private Hashtable _wpGroups = new Hashtable();

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event EventHandler WaypointDataChanged; //data of a wp has changed after Geocache.EndUpdate()
        public event PropertyChangedEventHandler WaypointPropertyChanged; //property of a wp has changed

        public WaypointCollection(Database db)
        {
            _db = db;
        }

        public Waypoint GetWaypoint(string id)
        {
            return _qaItems[id] as Waypoint;
        }

        public List<Waypoint> GetWaypoints(string geocacheCode)
        {
            List<Waypoint> result;
            Hashtable ht = _wpGroups[geocacheCode] as Hashtable;
            if (ht != null)
            {
                result = (from Waypoint l in ht.Values select l).OrderBy(x=>x.ID).ToList();
            }
            else
            {
                return new List<Waypoint>();
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

        public new void Add(Waypoint gc)
        {
            _qaItems[gc.ID] = gc;
            _sorted = false;

            //grouping
            if (_wpGroups[gc.GeocacheCode ?? ""] == null)
            {
                _wpGroups[gc.GeocacheCode ?? ""] = new Hashtable();
            }
            (_wpGroups[gc.GeocacheCode ?? ""] as Hashtable)[gc.ID] = gc;
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

        public new int IndexOf(Waypoint gc)
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
            Waypoint gc = this[index];
            //grouping
            (_wpGroups[gc.GeocacheCode ?? ""] as Hashtable).Remove(gc.ID);
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

        public new void Remove(Waypoint gc)
        {
            RemoveAt(this.IndexOf(gc));
        }

        public new void Clear()
        {
            foreach (Waypoint gc in this)
            {
                gc.DataChanged -= gc_DataChanged;
                gc.PropertyChanged -= gc_PropertyChanged;
            }
            base.Clear();
            _wpGroups.Clear();

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
                if (WaypointPropertyChanged != null)
                {
                    WaypointPropertyChanged(sender, e);
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
                if (WaypointDataChanged != null)
                {
                    WaypointDataChanged(sender, e);
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
