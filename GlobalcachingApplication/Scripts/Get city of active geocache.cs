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
        if (core.ActiveGeocache == null)
        {
            System.Windows.Forms.MessageBox.Show(LanguageSupport.Instance.GetTranslation("No active geocache"));
        }
        else
        {
            core.ActiveGeocache.City = Geocoder.GetCityName(core.ActiveGeocache.Lat, core.ActiveGeocache.Lon);
        }
        return true;
    }
}
