using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalcachingApplication.Framework.Interfaces;
using GlobalcachingApplication.Utils;
using GlobalcachingApplication.Utils.BasePlugin;

class Script
{
    public static bool Run(Plugin plugin, ICore core)
    {
        System.Windows.Forms.MessageBox.Show(core.ActiveGeocache == null ? LanguageSupport.Instance.GetTranslation("No active geocache") : core.ActiveGeocache.Code);
        return true;
    }
}
