using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.OV2
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

        public string LastSavedFile
        {
            get { return _core.SettingsProvider.GetSettingsValue("OV2.LastSavedFile", null); }
            set { _core.SettingsProvider.SetSettingsValue("OV2.LastSavedFile", value); }
        }

        public bool gcName
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("OV2.gcName", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("OV2.gcName", value); }
        }

        public bool gcCode
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("OV2.gcCode", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("OV2.gcCode", value); }
        }

        public bool gcCoord
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("OV2.gcCoord", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("OV2.gcCoord", value); }
        }

        public bool gcOwner
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("OV2.gcOwner", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("OV2.gcOwner", value); }
        }

        public bool gcCacheType
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("OV2.gcCacheType", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("OV2.gcCacheType", value); }
        }

        public bool gcHint
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("OV2.gcHint", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("OV2.gcHint", value); }
        }

        public bool gcTerrain
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("OV2.gcTerrain", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("OV2.gcTerrain", value); }
        }

        public bool gcDifficulty
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("OV2.gcDifficulty", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("OV2.gcDifficulty", value); }
        }

        public bool gcContainer
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("OV2.gcContainer", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("OV2.gcContainer", value); }
        }

        public bool gcFavorites
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("OV2.gcFavorites", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("OV2.gcFavorites", value); }
        }

    }
}
