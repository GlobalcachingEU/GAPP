using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.ExpExcel
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

        public System.Collections.Specialized.StringCollection ExportFields
        {
            get { return _core.SettingsProvider.GetSettingsValueStringCollection("ExpExcel.ExportFields", null); }
            set { _core.SettingsProvider.SetSettingsValueStringCollection("ExpExcel.ExportFields", value); }
        }

        public string FilePath
        {
            get { return _core.SettingsProvider.GetSettingsValue("ExpExcel.FilePath", null); }
            set { _core.SettingsProvider.SetSettingsValue("ExpExcel.FilePath", value); }
        }

    }
}
