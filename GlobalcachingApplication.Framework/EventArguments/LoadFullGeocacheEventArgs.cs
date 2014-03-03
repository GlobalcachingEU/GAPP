using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.EventArguments
{
    public class LoadFullGeocacheEventArgs : EventArgs
    {
        public Data.Geocache RequestedForGeocache { get; private set; }
        public string ShortDescription { get; set; }
        public bool ShortDescriptionInHtml { get; set; }
        public string LongDescription { get; set; }
        public bool LongDescriptionInHtml { get; set; }

        public LoadFullGeocacheEventArgs(Data.Geocache gc)
        {
            RequestedForGeocache = gc;
        }
    }

    public delegate void LoadFullGeocacheEventHandler(object sender, LoadFullGeocacheEventArgs e);
}
