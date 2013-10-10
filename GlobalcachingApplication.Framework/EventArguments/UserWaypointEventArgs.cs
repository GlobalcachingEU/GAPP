using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.EventArguments
{
    public class UserWaypointEventArgs: EventArgs
    {
        public Data.UserWaypoint Waypoint { get; private set; }

        public UserWaypointEventArgs(Data.UserWaypoint wp)
        {
            Waypoint = wp;
        }
    }

    public delegate void UserWaypointEventHandler(object sender, UserWaypointEventArgs e);
}
