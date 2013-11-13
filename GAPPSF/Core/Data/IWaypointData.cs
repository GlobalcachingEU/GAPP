using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public interface IWaypointData
    {
        string ID { get; set; }
        string Name { get; set; }
        string Comment { get; set; }
        string Description { get; set; }
        string Url { get; set; }
        string UrlName { get; set; }
        WaypointType WPType { get; set; }
        string Code { get; set; }
        string GeocacheCode { get; set; }
        DateTime DataFromDate { get; set; }
        double? Lat { get; set; }
        double? Lon { get; set; }
        DateTime Time { get; set; }
    }
}
