using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.LiveAPILogGeocaches
{
    public class GeocacheVisitsItem
    {
        public string Code { get; set; }
        public DateTime LogDate { get; set; }
        public Core.Data.LogType LogType { get; set; }
        public string Comment { get; set; }
    }
}
