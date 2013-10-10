using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.Maps.MapControl
{
    public class MarkerClusterer
    {
        public class Bucket
        {
            public double Latitude {get; private set;}
            public double Longitude {get; private set;}

            public Framework.Data.Geocache Geocache { get; private set; }
            public int Count { get; private set; }

            public Bucket(Framework.Data.Geocache gc)
            {
                Geocache = gc;
                Count = 1;
                if (gc.ContainsCustomLatLon)
                {
                    Latitude = (double)gc.CustomLat;
                    Longitude = (double)gc.CustomLon;
                }
                else
                {
                    Latitude = gc.Lat;
                    Longitude = gc.Lon;
                }
            }

            public void AddMarker(Framework.Data.Geocache m)
            {
                Count++;
            }
        }

        public List<Bucket> Buckets { get; private set; }
        private double _latDelta;
        private double _lonDelta;

        public MarkerClusterer(double latDelta, double lonDelta)
        {
            _latDelta = latDelta;
            _lonDelta = lonDelta;
            Buckets = new List<Bucket>();
        }

        public void AddMarker(Framework.Data.Geocache m, bool enableCluster)
        {
            double lat;
            double lon;
            if (m.ContainsCustomLatLon)
            {
                lat = (double)m.CustomLat;
                lon = (double)m.CustomLon;
            }
            else
            {
                lat = m.Lat;
                lon = m.Lon;
            }
            if (enableCluster)
            {
                Bucket bucket = (from b in Buckets
                                 where
                                lat >= (b.Latitude - _latDelta) && lat <= (b.Latitude + _latDelta) &&
                                lon >= (b.Longitude - _lonDelta) && lon <= (b.Longitude + _lonDelta)
                                 select b).FirstOrDefault();
                if (bucket == null)
                {
                    Buckets.Add(new Bucket(m));
                }
                else
                {
                    bucket.AddMarker(m);
                }
            }
            else
            {
                Buckets.Add(new Bucket(m));
            }
        }
    }
}
