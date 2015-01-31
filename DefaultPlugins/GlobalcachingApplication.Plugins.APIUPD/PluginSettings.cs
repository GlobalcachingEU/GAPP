using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.APIUPD
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

        public int AdditionalDelayBetweenImageImport
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("APIUPD.AdditionalDelayBetweenImageImport", 0); }
            set { _core.SettingsProvider.SetSettingsValueInt("APIUPD.AdditionalDelayBetweenImageImport", value); }
        }

        public int UpdateLogsMaxLogCount
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("APIUPD.UpdateLogsMaxLogCount", 5); }
            set { _core.SettingsProvider.SetSettingsValueInt("APIUPD.UpdateLogsMaxLogCount", value); }
        }

        public int AdditionalDelayBetweenLogImport
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("APIUPD.AdditionalDelayBetweenLogImport", 0); }
            set { _core.SettingsProvider.SetSettingsValueInt("APIUPD.AdditionalDelayBetweenLogImport", value); }
        }

        public bool DeselectGeocacheAfterUpdate
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("APIUPD.DeselectGeocacheAfterUpdate", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("APIUPD.DeselectGeocacheAfterUpdate", value); }
        }

    }
}
