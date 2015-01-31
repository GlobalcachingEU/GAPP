using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.GAPPDataStorage
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
            get { return _core.SettingsProvider.GetSettingsValueInt("GAPPDataStorage.BackupKeepMaxCount", 5); }
            set { _core.SettingsProvider.SetSettingsValueInt("GAPPDataStorage.BackupKeepMaxCount", value); }
        }

        public int BackupKeepMaxDays
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("GAPPDataStorage.BackupKeepMaxDays", 0); }
            set { _core.SettingsProvider.SetSettingsValueInt("GAPPDataStorage.BackupKeepMaxDays", value); }
        }

        public string ActiveDataFile
        {
            get { return _core.SettingsProvider.GetSettingsValue("GAPPDataStorage.ActiveDataFile", null); }
            set { _core.SettingsProvider.SetSettingsValue("GAPPDataStorage.ActiveDataFile", value); }
        }

        public string BackupFolder
        {
            get { return _core.SettingsProvider.GetSettingsValue("GAPPDataStorage.BackupFolder", null); }
            set { _core.SettingsProvider.SetSettingsValue("GAPPDataStorage.BackupFolder", value); }
        }
    }
}
