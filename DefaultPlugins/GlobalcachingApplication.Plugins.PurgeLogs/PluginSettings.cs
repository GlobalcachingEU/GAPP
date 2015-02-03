using GlobalcachingApplication.Framework;
using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.PurgeLogs
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

        public int DaysMonths
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("PurgeLogs.DaysMonths", 1); }
            set { _core.SettingsProvider.SetSettingsValueInt("PurgeLogs.DaysMonths", value); }
        }

        public int DaysMonthsCount
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("PurgeLogs.DaysMonthsCount", 3); }
            set { _core.SettingsProvider.SetSettingsValueInt("PurgeLogs.DaysMonthsCount", value); }
        }

        public int KeepAtLeast
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("PurgeLogs.KeepAtLeast", 5); }
            set { _core.SettingsProvider.SetSettingsValueInt("PurgeLogs.KeepAtLeast", value); }
        }

        public bool KeepAllOfOwned
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("PurgeLogs.KeepAllOfOwned", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("PurgeLogs.KeepAllOfOwned", value); }
        }

        public bool KeepOwnLogs
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("PurgeLogs.KeepOwnLogs", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("PurgeLogs.KeepOwnLogs", value); }
        }

        public InterceptedStringCollection KeepLogsOf
        {
            get { return _core.SettingsProvider.GetSettingsValueStringCollection("PurgeLogs.KeepLogsOf", null); }
            set { _core.SettingsProvider.SetSettingsValueStringCollection("PurgeLogs.KeepLogsOf", value); }
        }

        public InterceptedStringCollection RemoveAllLogsFrom
        {
            get { return _core.SettingsProvider.GetSettingsValueStringCollection("PurgeLogs.RemoveAllLogsFrom", null); }
            set { _core.SettingsProvider.SetSettingsValueStringCollection("PurgeLogs.RemoveAllLogsFrom", value); }
        }

    }
}
