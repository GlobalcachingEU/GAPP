using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.EventArguments
{
    public class ProgressEventArgs
    {
        public string ActionTitle { get; private set; }
        public string ActionText { get; private set; }
        public int Max { get; private set; }
        public int Position { get; private set; }
        public bool CanCancel { get; private set; }
        public bool Cancel { get; set; }
        public Framework.Interfaces.IPlugin Plugin { get; private set; }

        public ProgressEventArgs(Framework.Interfaces.IPlugin plugin, string actionTitle, string actionText, int max, int position)
        {
            Plugin = plugin;
            ActionTitle = actionTitle;
            ActionText = actionText;
            Max = max;
            Position = position;
        }

        public ProgressEventArgs(Framework.Interfaces.IPlugin plugin, string actionTitle, string actionText, int max, int position, bool canCancel)
            :this(plugin, actionTitle, actionText,max,position)
        {
            CanCancel = canCancel;
        }
    }

    public delegate void ProgressEventHandler(object sender, ProgressEventArgs e);
}
