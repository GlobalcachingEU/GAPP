using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Data
{
    /// <summary>
    /// Contains the collection of geocache images. The list is not threadsafe and processes changing this collection should be started
    /// with BeginUpdate and end with EndUpdate within the UI context
    /// </summary>
    public class GeocacheImageCollection: ArrayList
    {
        //events in UI context (per geocache)
        public event Framework.EventArguments.GeocacheImageEventHandler GeocacheImageAdded;
        public event Framework.EventArguments.GeocacheImageEventHandler GeocacheImageRemoved;
        public event EventArguments.GeocacheImageEventHandler DataChanged;

        //events in UI context (on the list, after EndUpdate)
        public event EventHandler ListDataChanged;

        private bool _updating = false;
        private bool _dataChanged = false;
        private int _updatingCounter = 0;

        private Hashtable _qaItems = new Hashtable();
        private List<GeocacheImage> _batchAddition = null;
        private bool _sorted = false;

        //image grouping by GeocacheCode
        private Hashtable _geocacheImageGroups = new Hashtable();

        public GeocacheImageCollection()
        {
        }

        public Framework.Data.GeocacheImage GetGeocacheImage(string id)
        {
            return _qaItems[id] as Framework.Data.GeocacheImage;
        }

        public List<Framework.Data.GeocacheImage> GetGeocacheImages(string geocacheCode)
        {
            List<Framework.Data.GeocacheImage> result;
            Hashtable ht = _geocacheImageGroups[geocacheCode] as Hashtable;
            if (ht != null)
            {
                result = (from GeocacheImage l in ht.Values select l).ToList();
            }
            else
            {
                return new List<GeocacheImage>();
            }
            return result;
        }

        public override int Add(object value)
        {
            int result;
            Framework.Data.GeocacheImage wp = value as Framework.Data.GeocacheImage;
            if (wp != null)
            {
                _qaItems[wp.ID] = wp;
                //grouping
                if (_geocacheImageGroups[wp.GeocacheCode ?? ""] == null)
                {
                    _geocacheImageGroups[wp.GeocacheCode ?? ""] = new Hashtable();
                }
                (_geocacheImageGroups[wp.GeocacheCode ?? ""] as Hashtable)[wp.ID] = wp;
                //end grouping
                _sorted = false;
                result = base.Add(wp);
                wp.DataChanged += new EventArguments.GeocacheImageEventHandler(gc_DataChanged);
                OnGeocacheImageAdded(wp);
            }
            else 
            {
                //for now, only supported between begin and end update
                List<GeocacheImage> lgs = value as List<GeocacheImage>;
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

        void gc_DataChanged(object sender, EventArguments.GeocacheImageEventArgs e)
        {
            OnDataChanged(e.GeocacheImage);
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
            Framework.Data.GeocacheImage wp = value as Framework.Data.GeocacheImage;
            if (wp!=null)
            {
                RemoveAt(IndexOf(wp));
            }
        }
        public override void RemoveAt(int index)
        {
            if (index >= 0 && index < Count)
            {
                Framework.Data.GeocacheImage wp = this[index] as Framework.Data.GeocacheImage;
                //grouping
                (_geocacheImageGroups[wp.GeocacheCode ?? ""] as Hashtable).Remove(wp.ID);
                //end grouping
                _qaItems.Remove(wp.ID);
                wp.DataChanged -= new EventArguments.GeocacheImageEventHandler(gc_DataChanged);
                base.RemoveAt(index);
                OnGeocacheImageRemoved(wp);
            }
        }
        public override void Clear()
        {
            BeginUpdate();
            if (this.Count > 0)
            {
                //grouping
                _geocacheImageGroups.Clear();
                //end grouping
                _qaItems.Clear();
                foreach (Framework.Data.GeocacheImage gc in this)
                {
                    gc.DataChanged -= new EventArguments.GeocacheImageEventHandler(gc_DataChanged);
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
                        foreach (GeocacheImage l in _batchAddition)
                        {
                            if (_qaItems[l.ID] == null)
                            {
                                _qaItems[l.ID] = l;
                                //grouping
                                if (_geocacheImageGroups[l.GeocacheCode ?? ""] == null)
                                {
                                    _geocacheImageGroups[l.GeocacheCode ?? ""] = new Hashtable();
                                }
                                (_geocacheImageGroups[l.GeocacheCode ?? ""] as Hashtable)[l.ID] = l;
                                //end grouping
                                _sorted = false;
                                base.Add(l);
                                l.DataChanged += new EventArguments.GeocacheImageEventHandler(gc_DataChanged);
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

        public void OnGeocacheImageAdded(Framework.Data.GeocacheImage wp)
        {
            if (!_updating && GeocacheImageAdded != null)
            {
                GeocacheImageAdded(this, new Framework.EventArguments.GeocacheImageEventArgs(wp));
            }
            OnListDataChanged();
        }
        public void OnGeocacheImageRemoved(Framework.Data.GeocacheImage wp)
        {
            if (!_updating && GeocacheImageRemoved != null)
            {
                GeocacheImageRemoved(this, new Framework.EventArguments.GeocacheImageEventArgs(wp));
            }
            OnListDataChanged();
        }

        public void OnDataChanged(Framework.Data.GeocacheImage wp)
        {
            _dataChanged = true;
            if (!_updating && DataChanged != null)
            {
                DataChanged(this, new Framework.EventArguments.GeocacheImageEventArgs(wp));
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
                foreach (GeocacheImage l in this)
                {
                    l.IsDataChanged = false;
                }
            }
        }

    }
}
