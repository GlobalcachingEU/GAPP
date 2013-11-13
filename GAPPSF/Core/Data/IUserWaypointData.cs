using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public interface IUserWaypointData
    {
        string ID { get; set; }
        string GeocacheCode { get; set; }
        string Description { get; set; }
        double Lat { get; set; }
        double Lon { get; set; }
        DateTime Date { get; set; }
    }
}
