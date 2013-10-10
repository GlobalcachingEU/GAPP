using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.EventArguments
{
    public class GeocacheEventArgs: EventArgs
    {
        public Data.Geocache Geocache { get; private set; }

        public GeocacheEventArgs(Data.Geocache gc)
        {
            Geocache = gc;
        }
    }

    public delegate void GeocacheEventHandler(object sender, GeocacheEventArgs e);
}
