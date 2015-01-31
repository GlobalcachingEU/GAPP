using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.APILOGC
{
    public class PluginSettings
    {
        public static PluginSettings _uniqueInstance = null;
        private ICore _core = null;

        public PluginSettings(ICore core)
        {
            _uniqueInstance = this;
            _core = core;
        }

        public static PluginSettings Instance
        {
            get { return _uniqueInstance; }
        }

        public double MaxImageSizeMB
        {
            get { return _core.SettingsProvider.GetSettingsValueDouble("APILOGC.MaxImageSizeMB", 2); }
            set { _core.SettingsProvider.SetSettingsValueDouble("APILOGC.MaxImageSizeMB", value); }
        }

        public int MaxImageWidth
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("APILOGC.MaxImageWidth", 600); }
            set { _core.SettingsProvider.SetSettingsValueInt("APILOGC.MaxImageWidth", value); }
        }

        public int MaxImageHeight
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("APILOGC.MaxImageHeight", 600); }
            set { _core.SettingsProvider.SetSettingsValueInt("APILOGC.MaxImageHeight", value); }
        }

        public int ImageQuality
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("APILOGC.ImageQuality", 75); }
            set { _core.SettingsProvider.SetSettingsValueInt("APILOGC.ImageQuality", value); }
        }

    }
}
