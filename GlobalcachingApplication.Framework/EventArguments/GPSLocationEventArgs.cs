using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.EventArguments
{
    public class GPSLocationEventArgs
    {
        public Data.GPSLocation Location { get; private set; }

        public GPSLocationEventArgs(Data.GPSLocation lc)
        {
            Location = lc;
        }
    }

    public delegate void GPSLocationEventHandler(object sender, GPSLocationEventArgs e);
}
