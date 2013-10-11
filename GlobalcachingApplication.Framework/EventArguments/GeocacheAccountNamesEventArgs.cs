using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.EventArguments
{
    public class GeocachingAccountNamesEventArgs: EventArgs
    {
        public Data.GeocachingAccountNames AccountNames { get; private set; }

        public GeocachingAccountNamesEventArgs(Data.GeocachingAccountNames an)
        {
            AccountNames = an;
        }
    }

    public delegate void GeocachingAccountNamesEventHandler(object sender, GeocachingAccountNamesEventArgs e);
}
