using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Data
{
    public class Location : DataObject
    {
        public event EventArguments.LocationEventHandler Changed;

        private double _lat = 0.0;
        private double _lon = 0.0;

        public Location()
        {
        }

        public Location(double lat, double lon)
        {
            _lat = lat;
            _lon = lon;
        }

        public double Lat
        {
            get { return _lat; }
        }
        public double Lon
        {
            get { return _lon; }
        }

        public string SLat
        {
            get { return _lat.ToString().Replace(',','.'); }
        }
        public string SLon
        {
            get { return _lon.ToString().Replace(',', '.'); }
        }
        public string SLatLon
        {
            get { return string.Format("{0},{1}",SLat,SLon); }
        }

        public void SetLocation(double lat, double lon)
        {
            _lat = lat;
            _lon = lon;
            OnChanged(this);
        }

        public void OnChanged(object sender)
        {
            if (Changed != null)
            {
                Changed(sender, new EventArguments.LocationEventArgs(this));
            }
        }
    }
}
