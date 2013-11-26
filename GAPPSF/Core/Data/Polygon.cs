using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.Core.Data
{
    public class Polygon: List<Location>
    {
        public double MinLat { get; set; }
        public double MinLon { get; set; }
        public double MaxLat { get; set; }
        public double MaxLon { get; set; }

        public void AddLocation(Location loc)
        {
            if (this.Count == 0)
            {
                MinLat = loc.Lat;
                MinLon = loc.Lon;
                MaxLat = loc.Lat;
                MaxLon = loc.Lon;
            }
            else
            {
                if (loc.Lat < MinLat)
                {
                    MinLat = loc.Lat;
                }
                else if (loc.Lat > MaxLat)
                {
                    MaxLat = loc.Lat;
                }
                if (loc.Lon < MinLon)
                {
                    MinLon = loc.Lon;
                }
                else if (loc.Lon > MaxLon)
                {
                    MaxLon = loc.Lon;
                }
            }
            Add(loc);
        }
    }
}
