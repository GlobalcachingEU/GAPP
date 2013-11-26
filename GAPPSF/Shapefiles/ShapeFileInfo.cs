using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Shapefiles
{
    public class ShapeFileInfo
    {
        public bool Enabled { get; set; }
        public string Filename { get; set; }
        public string TableName { get; set; }
        public List<string> TableNames { get; set; }
        public ShapeFile.CoordType TCoord { get; set; }
        public ShapeFile.CoordType TCoords { get; set; }
        public Core.Data.AreaType TArea { get; set; }
        public Core.Data.AreaType TAreas { get; set; }
        public string Prefix { get; set; }
    }
}
