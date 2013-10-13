using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public class Import
    {
        public static List<Framework.Data.Geocache> AddGeocaches(Framework.Interfaces.ICore core, List<OKAPIService.Geocache> gcList)
        {
            List<Framework.Data.Geocache> result = new List<Framework.Data.Geocache>();
            if (gcList != null)
            {
                foreach (var gcApi in gcList)
                {
                    Framework.Data.Geocache activeGC = Convert.Geocache(core, gcApi);
                    if (Utils.API.Import.AddGeocache(core, activeGC))
                    {
                        if (gcApi.latest_logs != null)
                        {
                            foreach (var l in gcApi.latest_logs)
                            {
                                Framework.Data.Log lg = Convert.Log(core, l, "", "");
                                Utils.API.Import.AddLog(core, lg);
                            }
                        }
                        if (gcApi.alt_wpts != null)
                        {
                            foreach (var wp in gcApi.alt_wpts)
                            {
                                Utils.API.Import.AddWaypoint(core, Convert.Waypoint(core, wp));
                            }
                        }
                        if (gcApi.images != null)
                        {
                            foreach (var imgd in gcApi.images)
                            {
                                Utils.API.Import.AddGeocacheImage(core, Convert.GeocacheImage(core, imgd, activeGC.Code));
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
