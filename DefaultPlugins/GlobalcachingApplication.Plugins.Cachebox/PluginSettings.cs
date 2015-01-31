using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.Cachebox
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

        public int MaxLogs
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("Cachebox.MaxLogs", 5); }
            set { _core.SettingsProvider.SetSettingsValueInt("Cachebox.MaxLogs", value); }
        }

        public bool ExportGrabbedImages
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("Cachebox.ExportGrabbedImages", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("Cachebox.ExportGrabbedImages", value); }
        }

    }
}
