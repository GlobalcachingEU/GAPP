using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.EventArguments
{
    public class LogEventArgs: EventArgs
    {
        public Data.Log Log { get; private set; }

        public LogEventArgs(Data.Log l)
        {
            Log = l;
        }
    }

    public delegate void LogEventHandler(object sender, LogEventArgs e);
}
