using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.EventArguments
{
    public class LogImageEventArgs: EventArgs
    {
        public Data.LogImage LogImage { get; private set; }

        public LogImageEventArgs(Data.LogImage li)
        {
            LogImage = li;
        }
    }

    public delegate void LogImageEventHandler(object sender, LogImageEventArgs e);
}
