using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.MapProviders.OSMBinMap
{
    public class POI
    {
        public int Latitude { get; set; }
        public int Longitude { get; set; }
        public List<int> TagIDs { get; set; }
        public int Layer { get; set; }
        public string Name { get; set; }
        public string HouseNumber { get; set; }
        public int? Elevation { get; set; }
    }
}
