using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.MapProviders.OSMBinMap
{
    public class Way
    {
        public class WayCoordinate
        {
            public int Latitude { get; set; }
            public int Longitude { get; set; }
        }
        public class WayCoordinateBlock
        {
            public List<WayCoordinate> CoordBlock { get; set; }
        }

        public class WayData
        {
            public List<WayCoordinateBlock> DataBlock { get; set; }
        }

        public List<int> TagIDs { get; set; }
        public int Layer { get; set; }
        public string Name { get; set; }
        public string HouseNumber { get; set; }
        public string Reference { get; set; }
        public int? LabelLatitude { get; set; }
        public int? LabelLongitude { get; set; }
        public List<WayData> WayDataBlocks { get; set; }

        //from theme
        public List<RenderInfo> RenderInfos { get; set; }
    }
}
