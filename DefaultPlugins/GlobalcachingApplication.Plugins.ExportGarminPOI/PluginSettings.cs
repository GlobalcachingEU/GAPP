using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.ExportGarminPOI
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

        public bool ClearExportDirectory
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGarminPOI.ClearExportDirectory", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGarminPOI.ClearExportDirectory", value); }
        }

        public bool ExportGeocachePOIs
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGarminPOI.ExportGeocachePOIs", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGarminPOI.ExportGeocachePOIs", value); }
        }

        public bool ExportWaypointPOIs
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGarminPOI.ExportWaypointPOIs", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGarminPOI.ExportWaypointPOIs", value); }
        }

        public bool RunPOILoader
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGarminPOI.RunPOILoader", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGarminPOI.RunPOILoader", value); }
        }

        public bool PassDirectoryToPOILoader
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGarminPOI.PassDirectoryToPOILoader", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGarminPOI.PassDirectoryToPOILoader", value); }
        }

        public bool RunPOILoaderSilently
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGarminPOI.RunPOILoaderSilently", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGarminPOI.RunPOILoaderSilently", value); }
        }

        public int NameLengthLimit
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("ExportGarminPOI.NameLengthLimit", 22); }
            set { _core.SettingsProvider.SetSettingsValueInt("ExportGarminPOI.NameLengthLimit", value); }
        }

        public int DescriptionLengthLimit
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("ExportGarminPOI.DescriptionLengthLimit", 84); }
            set { _core.SettingsProvider.SetSettingsValueInt("ExportGarminPOI.DescriptionLengthLimit", value); }
        }

        public string POIExportPath
        {
            get { return _core.SettingsProvider.GetSettingsValue("ExportGarminPOI.POIExportPath", null); }
            set { _core.SettingsProvider.SetSettingsValue("ExportGarminPOI.POIExportPath", value); }
        }

        public string POILoaderFilename
        {
            get { return _core.SettingsProvider.GetSettingsValue("ExportGarminPOI.POILoaderFilename", null); }
            set { _core.SettingsProvider.SetSettingsValue("ExportGarminPOI.POILoaderFilename", value); }
        }

        public string POINameType
        {
            get { return _core.SettingsProvider.GetSettingsValue("ExportGarminPOI.POINameType", "N"); }
            set { _core.SettingsProvider.SetSettingsValue("ExportGarminPOI.POINameType", value); }
        }

    }
}
