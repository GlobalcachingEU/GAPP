using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public class Location : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private double _lat = 0.0;
        private double _lon = 0.0;

        public Location()
        {
        }

        public override string ToString()
        {
            return Utils.Conversion.GetCoordinatesPresentation(this);
        }

        public Location(double lat, double lon)
        {
            _lat = lat;
            _lon = lon;
        }

        public double Lat
        {
            get { return _lat; }
            set { SetProperty(ref _lat, value); }
        }
        public double Lon
        {
            get { return _lon; }
            set { SetProperty(ref _lon, value); }
        }

        public string SLat
        {
            get { return _lat.ToString(CultureInfo.InvariantCulture); }
        }
        public string SLon
        {
            get { return _lon.ToString(CultureInfo.InvariantCulture); }
        }
        public string SLatLon
        {
            get { return string.Format("{0},{1}", SLat, SLon); }
        }

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }
    }
}
