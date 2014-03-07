using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.UIControls.Trackables
{
    public class TravelItem
    {
        public string TrackableCode { get; set; }
        public string GeocacheCode { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public DateTime DateLogged { get; set; }
    }
}
