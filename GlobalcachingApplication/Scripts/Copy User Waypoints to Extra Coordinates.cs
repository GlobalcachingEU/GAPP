using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalcachingApplication.Framework.Data;
using GlobalcachingApplication.Framework.Interfaces;
using GlobalcachingApplication.Utils;
using GlobalcachingApplication.Utils.BasePlugin;
using System.Windows.Forms;

class Script
{
    public static bool Run(Plugin plugin, ICore core)
    {

        using (FrameworkDataUpdater upd = new FrameworkDataUpdater(core))
        {
            //reset current selection
            //foreach (Geocache gc in core.Geocaches)
            foreach(Geocache gc in DataAccess.GetSelectedGeocaches(core.Geocaches))
            {
                //you can have more than one, then just pick the last one
                UserWaypoint uwp = DataAccess.GetUserWaypointsFromGeocache(core.UserWaypoints, gc.Code).LastOrDefault();
                if (uwp != null)
                {
                    gc.CustomLat = uwp.Lat;
                    gc.CustomLon = uwp.Lon;
                }
            }

        }
        return true;
    }
}
