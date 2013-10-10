using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.EventArguments
{
    public class GeocacheImageEventArgs: EventArgs
    {
        public Data.GeocacheImage GeocacheImage { get; private set; }

        public GeocacheImageEventArgs(Data.GeocacheImage li)
        {
            GeocacheImage = li;
        }
    }

    public delegate void GeocacheImageEventHandler(object sender, GeocacheImageEventArgs e);
}
