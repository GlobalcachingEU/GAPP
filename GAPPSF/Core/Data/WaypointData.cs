using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public class WaypointData: IWaypointData
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string UrlName { get; set; }
        public WaypointType WPType { get; set; }
        public string Code { get; set; }
        public string GeocacheCode { get; set; }
        public DateTime DataFromDate { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public DateTime Time { get; set; }

        public static void Copy(WaypointData src, WaypointData dest)
        {
            dest.ID = src.ID;
            dest.Name = src.Name;
            dest.Comment = src.Comment;
            dest.Description = src.Description;
            dest.Url = src.Url;
            dest.UrlName = src.UrlName;
            dest.WPType = src.WPType;
            dest.Code = src.Code;
            dest.GeocacheCode = src.GeocacheCode;
            dest.DataFromDate = src.DataFromDate;
            dest.Lat = src.Lat;
            dest.Lon = src.Lon;
            dest.Time = src.Time;
        }
    }
}
