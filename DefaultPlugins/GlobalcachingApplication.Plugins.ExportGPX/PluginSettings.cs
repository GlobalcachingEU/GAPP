using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.ExportGPX
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

        public int MaximumNumberOfLogs
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("ExportGPX.MaximumNumberOfLogs", 5); }
            set { _core.SettingsProvider.SetSettingsValueInt("ExportGPX.MaximumNumberOfLogs", value); }
        }

        public int MaxGeocacheNameLength
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("ExportGPX.MaxGeocacheNameLength", 255); }
            set { _core.SettingsProvider.SetSettingsValueInt("ExportGPX.MaxGeocacheNameLength", value); }
        }

        public int MinStartOfGeocacheName
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("ExportGPX.MinStartOfGeocacheName", 255); }
            set { _core.SettingsProvider.SetSettingsValueInt("ExportGPX.MinStartOfGeocacheName", value); }
        }

        public bool ZipFile
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGPX.ZipFile", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGPX.ZipFile", value); }
        }

        public bool AddFieldNotesToDescription
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGPX.AddFieldNotesToDescription", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGPX.AddFieldNotesToDescription", value); }
        }

        public bool UseNameAndNotCode
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGPX.UseNameAndNotCode", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGPX.UseNameAndNotCode", value); }
        }

        public bool AddWaypointsToDescription
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGPX.AddWaypointsToDescription", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGPX.AddWaypointsToDescription", value); }
        }

        public bool AddWaypoints
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGPX.AddWaypoints", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGPX.AddWaypoints", value); }
        }

        public bool UseHintsForDescription
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGPX.UseHintsForDescription", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGPX.UseHintsForDescription", value); }
        }

        public bool UseDatabaseNameForFileName
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGPX.UseDatabaseNameForFileName", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGPX.UseDatabaseNameForFileName", value); }
        }

        public bool CreateGGZFile
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGPX.CreateGGZFile", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGPX.CreateGGZFile", value); }
        }

        public bool AddImages
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGPX.AddImages", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGPX.AddImages", value); }
        }

        public bool AddExtraInfoToDescription
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGPX.AddExtraInfoToDescription", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGPX.AddExtraInfoToDescription", value); }
        }

        public string CorrectedNamePrefix
        {
            get { return _core.SettingsProvider.GetSettingsValue("ExportGPX.CorrectedNamePrefix", ""); }
            set { _core.SettingsProvider.SetSettingsValue("ExportGPX.CorrectedNamePrefix", value); }
        }

        public string GPXVersionStr
        {
            get { return _core.SettingsProvider.GetSettingsValue("ExportGPX.GPXVersionStr", null); }
            set { _core.SettingsProvider.SetSettingsValue("ExportGPX.GPXVersionStr", value); }
        }

    }
}
