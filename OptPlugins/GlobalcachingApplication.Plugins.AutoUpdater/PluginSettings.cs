using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.AutoUpdater
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

        public bool ShowSettingsDialog
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("AutoUpdater.ShowSettingsDialog", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("AutoUpdater.ShowSettingsDialog", value); }
        }

        public bool AutomaticDownloadGeocaches
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("AutoUpdater.AutomaticDownloadGeocaches", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("AutoUpdater.AutomaticDownloadGeocaches", value); }
        }

        public bool UpdateNL
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("AutoUpdater.UpdateNL", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("AutoUpdater.UpdateNL", value); }
        }

        public bool UpdateBE
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("AutoUpdater.UpdateBE", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("AutoUpdater.UpdateBE", value); }
        }

        public bool UpdateLU
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("AutoUpdater.UpdateLU", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("AutoUpdater.UpdateLU", value); }
        }

    }
}
