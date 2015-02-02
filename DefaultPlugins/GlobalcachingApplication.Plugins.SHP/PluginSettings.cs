using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.SHP
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

        public string DefaultShapeFilesFolder
        {
            get { return _core.SettingsProvider.GetSettingsValue("SHP.DefaultShapeFilesFolder", null); }
            set { _core.SettingsProvider.SetSettingsValue("SHP.DefaultShapeFilesFolder", value); }
        }

        public System.Collections.Specialized.StringCollection ShapeFiles
        {
            get { return _core.SettingsProvider.GetSettingsValueStringCollection("SHP.ShapeFiles", null); }
            set { _core.SettingsProvider.SetSettingsValueStringCollection("SHP.ShapeFiles", value); }
        }

    }
}
