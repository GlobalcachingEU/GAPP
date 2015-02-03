using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.APIFavorites
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


        public string Favorites
        {
            get { return _core.SettingsProvider.GetSettingsValue("APIFavorites.Favorites", null); }
            set { _core.SettingsProvider.SetSettingsValue("APIFavorites.Favorites", value); }
        }
    }
}
