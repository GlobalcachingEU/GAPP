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
    public class GeocacheCollection: List<Geocache>, INotifyCollectionChanged
    {
        private Database _db = null;
        private int _updateCounter = 0;
        private bool _dataHasChanged = false;

        private Hashtable _qaItems = new Hashtable();
        private bool _sorted = false;

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event EventHandler GeocacheDataChanged; //data of a geocache has changed after Geocache.EndUpdate()
        public event PropertyChangedEventHandler GeocachePropertyChanged; //property of a geocache has changed


        public GeocacheCollection(Database db)
        {
            _db = db;
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
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
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
            gc.DataChanged -= gc_DataChanged;
            gc.PropertyChanged -= gc_PropertyChanged;
            base.RemoveAt(this.IndexOf(gc));

            if (_updateCounter <= 0)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));
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
