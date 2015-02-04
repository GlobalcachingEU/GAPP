using GlobalcachingApplication.Framework;
using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.Browser
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

        public Rectangle WindowPos
        {
            get { return _core.SettingsProvider.GetSettingsValueRectangle("Browser.WindowPos", Rectangle.Empty); }
            set { _core.SettingsProvider.SetSettingsValueRectangle("Browser.WindowPos", value); }
        }

        public string HomePage
        {
            get { return _core.SettingsProvider.GetSettingsValue("Browser.HomePage", "http://www.geocaching.com/"); }
            set { _core.SettingsProvider.SetSettingsValue("Browser.HomePage", value); }
        }

        public InterceptedStringCollection Favorites
        {
            get { return _core.SettingsProvider.GetSettingsValueStringCollection("Browser.Favorites", null); }
            set { _core.SettingsProvider.SetSettingsValueStringCollection("Browser.Favorites", value); }
        }

        public InterceptedStringCollection DisabledSystemScripts
        {
            get { return _core.SettingsProvider.GetSettingsValueStringCollection("Browser.DisabledSystemScripts", null); }
            set { _core.SettingsProvider.SetSettingsValueStringCollection("Browser.DisabledSystemScripts", value); }
        }

        public bool ScriptErrorsSuppressed
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("Browser.ScriptErrorsSuppressed", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("Browser.ScriptErrorsSuppressed", value); }
        }

        public int CompatibilityMode
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("Browser.CompatibilityMode", 8000); }
            set { _core.SettingsProvider.SetSettingsValueInt("Browser.CompatibilityMode", value); }
        }

    }
}
