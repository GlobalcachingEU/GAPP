using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public class AreaInfo
    {
        public object ID { get; set; }
        public object ParentID { get; set; }
        public AreaType Level { get; set; }
        public string Name { get; set; }
        public double MinLat { get; set; }
        public double MinLon { get; set; }
        public double MaxLat { get; set; }
        public double MaxLon { get; set; }
        public List<Polygon> Polygons { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
