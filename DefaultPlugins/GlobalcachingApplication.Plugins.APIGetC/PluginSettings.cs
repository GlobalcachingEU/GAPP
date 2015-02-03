using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.APIGetC
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

        public int TotalMaximum
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("APIGetC.TotalMaximum", 100); }
            set { _core.SettingsProvider.SetSettingsValueInt("APIGetC.TotalMaximum", value); }
        }

        public int MaxPerRequest
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("APIGetC.MaxPerRequest", 10); }
            set { _core.SettingsProvider.SetSettingsValueInt("APIGetC.MaxPerRequest", value); }
        }

        public int MaxLogs
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("APIGetC.MaxLogs", 5); }
            set { _core.SettingsProvider.SetSettingsValueInt("APIGetC.MaxLogs", value); }
        }

        public bool UseMetric
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("APIGetC.UseMetric", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("APIGetC.UseMetric", value); }
        }

        public string ApiImportPresets
        {
            get { return _core.SettingsProvider.GetSettingsValue("APIGetC.ApiImportPresets", null); }
            set { _core.SettingsProvider.SetSettingsValue("APIGetC.ApiImportPresets", value); }
        }
    }
}
