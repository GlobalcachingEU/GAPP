using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Data
{
    public class GPSLocation
    {
        public event EventArguments.GPSLocationEventHandler Changed;
        public event EventArguments.GPSLocationEventHandler Updated;

        private DateTime _lastUpdate = DateTime.MinValue;
        private bool _valid = false;
        private Location _location;

        public GPSLocation()
        {
            _location = new Location();
        }

        public bool Valid
        {
            get { return _valid; }
        }
        public Location Position
        {
            get { return _location; }
        }
        public DateTime LastUpdate
        {
            get { return _lastUpdate; }
        }

        public void UpdateGPSLocation(bool valid, double lat, double lon)
        {
            _lastUpdate = DateTime.Now;
            bool changed = (valid != _valid || 
                            Math.Abs(lat - _location.Lat) > 0.000001 || 
                            Math.Abs(lon - _location.Lon) > 0.000001
                            );
            if (changed)
            {
                _valid = valid;
                _location.SetLocation(lat, lon);
                OnChanged();
            }
            OnUpdated();
        }

        public void OnUpdated()
        {
            if (Updated != null)
            {
                Updated(this, new EventArguments.GPSLocationEventArgs(this));
            }
        }
        public void OnChanged()
        {
            if (Changed != null)
            {
                Changed(this, new EventArguments.GPSLocationEventArgs(this));
            }
        }

    }
}
