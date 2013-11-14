using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public class UserWaypointData: IUserWaypointData
    {
        public string ID { get; set; }
        public string GeocacheCode { get; set; }
        public string Description { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public DateTime Date { get; set; }

        public static void Copy(IUserWaypointData src, IUserWaypointData dest)
        {
            dest.ID = src.ID;
            dest.Description = src.Description;
            dest.GeocacheCode = src.GeocacheCode;
            dest.Lat = src.Lat;
            dest.Lon = src.Lon;
            dest.Date = src.Date;
        }

    }
}
