using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.MapProviders
{
    public class Marker
    {
        public string ImagePath { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public object Tag { get; set; }
    }
}
