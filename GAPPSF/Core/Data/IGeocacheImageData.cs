using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public interface IGeocacheImageData
    {
        string ID { get; set; } //guid
        DateTime DataFromDate { get; set; }
        string GeocacheCode { get; set; }
        string Url { get; set; }
        string Name { get; set; }
        string MobileUrl { get; set; }
        string ThumbUrl { get; set; }
        string Description { get; set; }
    }
}
