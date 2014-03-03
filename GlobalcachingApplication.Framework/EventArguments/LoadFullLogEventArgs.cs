using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.EventArguments
{
    public class LoadFullLogEventArgs : EventArgs
    {
        public Data.Log RequestForLog { get; private set; }

        public string TBCode { get; set; }
        public string FinderId { get; set; }
        public string Text { get; set; }
        public bool Encoded { get; set; }

        public LoadFullLogEventArgs(Data.Log l)
        {
            RequestForLog = l;
        }
    }

    public delegate void LoadFullLogEventHandler(object sender, LoadFullLogEventArgs e);
}
