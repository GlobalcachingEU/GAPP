using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.EventArguments
{
    public class LocationEventArgs
    {
        public Data.Location Location { get; private set; }

        public LocationEventArgs(Data.Location lc)
        {
            Location = lc;
        }
    }

    public delegate void LocationEventHandler(object sender, LocationEventArgs e);
}
