using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Utils
{
    public class PluginSupport
    {
        public static Framework.Interfaces.IPlugin PluginByName(Framework.Interfaces.ICore core, string pluginName)
        {
            List<Framework.Interfaces.IPlugin> pins = core.GetPlugins();
            return (from a in pins where a.GetType().ToString().ToLower() == pluginName.ToLower() select a).FirstOrDefault();
        }

        public static bool ExecuteDefaultAction(Framework.Interfaces.ICore core, string pluginName)
        {
            bool result = false;
            Framework.Interfaces.IPlugin pi = PluginByName(core, pluginName);
            if (pi != null)
            {
                if (!string.IsNullOrEmpty(pi.DefaultAction))
                {
                    result = pi.Action(pi.DefaultAction);
                }
            }
            return result;
        }
    }
}
