using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.ExportGarmin
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
            get { return _core.SettingsProvider.GetSettingsValueInt("ExportGarmin.MaximumNumberOfLogs", 5); }
            set { _core.SettingsProvider.SetSettingsValueInt("ExportGarmin.MaximumNumberOfLogs", value); }
        }

        public int MaxGeocacheNameLength
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("ExportGarmin.MaxGeocacheNameLength", 255); }
            set { _core.SettingsProvider.SetSettingsValueInt("ExportGarmin.MaxGeocacheNameLength", value); }
        }

        public int MinStartOfGeocacheName
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("ExportGarmin.MinStartOfGeocacheName", 255); }
            set { _core.SettingsProvider.SetSettingsValueInt("ExportGarmin.MinStartOfGeocacheName", value); }
        }

        public bool AddFieldNotesToDescription
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGarmin.AddFieldNotesToDescription", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGarmin.AddFieldNotesToDescription", value); }
        }

        public bool AddChildWaypoints
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGarmin.AddChildWaypoints", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGarmin.AddChildWaypoints", value); }
        }

        public bool SeperateFilePerGeocache
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGarmin.SeperateFilePerGeocache", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGarmin.SeperateFilePerGeocache", value); }
        }

        public bool UseNameAndNotCode
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGarmin.UseNameAndNotCode", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGarmin.UseNameAndNotCode", value); }
        }

        public bool AddWaypointsToDescription
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGarmin.AddWaypointsToDescription", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGarmin.AddWaypointsToDescription", value); }
        }

        public bool UseHintsForDescription
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGarmin.UseHintsForDescription", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGarmin.UseHintsForDescription", value); }
        }

        public bool UseDatabaseNameForFileName
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGarmin.UseDatabaseNameForFileName", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGarmin.UseDatabaseNameForFileName", value); }
        }

        public bool CreateGGZFile
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGarmin.CreateGGZFile", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGarmin.CreateGGZFile", value); }
        }

        public bool AddImages
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGarmin.AddImages", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGarmin.AddImages", value); }
        }

        public bool AddExtraInfoToDescription
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ExportGarmin.AddExtraInfoToDescription", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("ExportGarmin.AddExtraInfoToDescription", value); }
        }

        public string GPXVersionStr
        {
            get { return _core.SettingsProvider.GetSettingsValue("ExportGarmin.GPXVersionStr", null); }
            set { _core.SettingsProvider.SetSettingsValue("ExportGarmin.GPXVersionStr", value); }
        }

        public string CorrectedNamePrefix
        {
            get { return _core.SettingsProvider.GetSettingsValue("ExportGarmin.CorrectedNamePrefix", ""); }
            set { _core.SettingsProvider.SetSettingsValue("ExportGarmin.CorrectedNamePrefix", value); }
        }


    }
}
