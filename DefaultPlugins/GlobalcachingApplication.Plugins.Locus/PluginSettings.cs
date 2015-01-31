using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.Locus
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

        public bool ExportGrabbedImages
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("Locus.ExportGrabbedImages", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("Locus.ExportGrabbedImages", value); }
        }

        public int MaxLogs
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("Locus.MaxLogs", 5); }
            set { _core.SettingsProvider.SetSettingsValueInt("Locus.MaxLogs", value); }
        }

        public int MaxFilesInFolder
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("Locus.MaxFilesInFolder", 0); }
            set { _core.SettingsProvider.SetSettingsValueInt("Locus.MaxFilesInFolder", value); }
        }

    }
}
