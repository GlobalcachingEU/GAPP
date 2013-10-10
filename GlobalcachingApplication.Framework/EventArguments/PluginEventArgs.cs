using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.EventArguments
{
    public class PluginEventArgs: EventArgs
    {
        public Framework.Interfaces.IPlugin Plugin { get; private set; }

        public PluginEventArgs(Framework.Interfaces.IPlugin plugin)
        {
            Plugin = plugin;
        }
    }

    public delegate void PluginEventHandler(object sender, PluginEventArgs e);
}
