using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.EventArguments
{
    public class DebugLogEventArgs: EventArgs
    {
        public Interfaces.IPlugin Plugin { get; private set; }
        public string Message { get; private set; }
        public Data.DebugLogLevel Level { get; private set; }
        public Exception Exception { get; private set; }

        public DebugLogEventArgs(Data.DebugLogLevel level, Interfaces.IPlugin p, Exception e, string msg)
        {
            Plugin = p;
            Message = msg;
            Level = level;
            Exception = e;
        }
    }

    public delegate void DebugLogEventHandler(object sender, DebugLogEventArgs e);
}
