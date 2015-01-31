using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.GCView
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

        public Rectangle WindowPos
        {
            get { return _core.SettingsProvider.GetSettingsValueRectangle("GCView.WindowPos", Rectangle.Empty); }
            set { _core.SettingsProvider.SetSettingsValueRectangle("GCView.WindowPos", value); }
        }

        public int ShowLogs
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("GCView.ShowLogs", 5); }
            set { _core.SettingsProvider.SetSettingsValueInt("GCView.ShowLogs", value); }
        }

        public bool ShowAdditionalWaypoints
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("GCView.ShowAdditionalWaypoints", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("GCView.ShowAdditionalWaypoints", value); }
        }

        public bool UseOfflineImagesIfAvailable
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("GCView.UseOfflineImagesIfAvailable", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("GCView.UseOfflineImagesIfAvailable", value); }
        }

        public bool OpenInInternalBrowser
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("GCView.OpenInInternalBrowser", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("GCView.OpenInInternalBrowser", value); }
        }

        public string GeocacheTemplateHtml
        {
            get { return _core.SettingsProvider.GetSettingsValue("GCView.GeocacheTemplateHtml", null); }
            set { _core.SettingsProvider.SetSettingsValue("GCView.GeocacheTemplateHtml", value); }
        }

        public string LogTemplateEvenHtml
        {
            get { return _core.SettingsProvider.GetSettingsValue("GCView.LogTemplateEvenHtml", null); }
            set { _core.SettingsProvider.SetSettingsValue("GCView.LogTemplateEvenHtml", value); }
        }

        public string LogTemplateOddHtml
        {
            get { return _core.SettingsProvider.GetSettingsValue("GCView.LogTemplateOddHtml", null); }
            set { _core.SettingsProvider.SetSettingsValue("GCView.LogTemplateOddHtml", value); }
        }

    }
}
