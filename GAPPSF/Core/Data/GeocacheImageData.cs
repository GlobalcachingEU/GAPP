using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public class GeocacheImageData : IGeocacheImageData
    {
        public string ID { get; set; } //guid
        public DateTime DataFromDate { get; set; }
        public string GeocacheCode { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string MobileUrl { get; set; }
        public string ThumbUrl { get; set; }
        public string Description { get; set; }

        public static void Copy(IGeocacheImageData src, IGeocacheImageData dest)
        {
            dest.ID = src.ID;
            dest.DataFromDate = src.DataFromDate;
            dest.GeocacheCode = src.GeocacheCode;
            dest.Url = src.Url;
            dest.Name = src.Name;
            dest.MobileUrl = src.MobileUrl;
            dest.ThumbUrl = src.ThumbUrl;
            dest.Description = src.Description;
        }

    }
}
