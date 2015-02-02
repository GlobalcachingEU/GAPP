using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.TrkGroup
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

        public int TimeBetweenTrackableUpdates
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("TrkGroup.TimeBetweenTrackableUpdates", 500); }
            set { _core.SettingsProvider.SetSettingsValueInt("TrkGroup.TimeBetweenTrackableUpdates", value); }
        }

        public string DatabaseFileName
        {
            get { return _core.SettingsProvider.GetSettingsValue("TrkGroup.DatabaseFileName", null); }
            set { _core.SettingsProvider.SetSettingsValue("TrkGroup.DatabaseFileName", value); }
        }

    }
}
