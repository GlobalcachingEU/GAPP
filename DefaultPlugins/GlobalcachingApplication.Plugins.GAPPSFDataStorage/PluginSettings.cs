using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.GAPPSFDataStorage
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

        public int BackupKeepMaxCount
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("GAPPSFDataStorage.BackupKeepMaxCount", 10); }
            set { _core.SettingsProvider.SetSettingsValueInt("GAPPSFDataStorage.BackupKeepMaxCount", value); }
        }

        public string ActiveDataFile
        {
            get { return _core.SettingsProvider.GetSettingsValue("GAPPSFDataStorage.ActiveDataFile", null); }
            set { _core.SettingsProvider.SetSettingsValue("GAPPSFDataStorage.ActiveDataFile", value); }
        }
    }
}
