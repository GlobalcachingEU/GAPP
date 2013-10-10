using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.EventArguments
{
    public class GeocacheComAccountEventArgs: EventArgs
    {
        public Data.GeocachingComAccountInfo AccountInfo { get; private set; }

        public GeocacheComAccountEventArgs(Data.GeocachingComAccountInfo ai)
        {
            AccountInfo = ai;
        }
    }

    public delegate void GeocacheComAccountEventHandler(object sender, GeocacheComAccountEventArgs e);
}
