using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalcachingApplication.Framework;
using GlobalcachingApplication.Framework.Interfaces;
using GlobalcachingApplication.Utils;
using GlobalcachingApplication.Utils.BasePlugin;
using System.Windows.Forms;

class Script
{
    public static bool Run(Plugin plugin, ICore core)
    {
        IPluginUIChildWindow mapPlugin = (from IPluginUIChildWindow a in core.GetPlugin(PluginType.Map) where a.GetType().ToString() == "GlobalcachingApplication.Plugins.GMap.GMap" select a).FirstOrDefault();
        if (mapPlugin.ChildForm!=null)
        {
            mapPlugin.ChildForm.Enabled = true;
            mapPlugin.ChildForm.MdiParent = null;
        }
        return true;
    }
}
