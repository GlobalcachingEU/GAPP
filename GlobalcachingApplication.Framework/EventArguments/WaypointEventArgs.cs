using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.EventArguments
{
    public class WaypointEventArgs: EventArgs
    {
        public Data.Waypoint Waypoint { get; private set; }

        public WaypointEventArgs(Data.Waypoint wp)
        {
            Waypoint = wp;
        }
    }

    public delegate void WaypointEventHandler(object sender, WaypointEventArgs e);
}
