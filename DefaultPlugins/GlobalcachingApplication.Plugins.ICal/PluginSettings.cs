using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.ICal
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

        public bool AddToOutlook
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ICal.AddToOutlook", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("ICal.AddToOutlook", value); }
        }

        public bool AddToGoogleCalendar
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ICal.AddToGoogleCalendar", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("ICal.AddToGoogleCalendar", value); }
        }

        public bool OpenGoogleCalendar
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ICal.OpenGoogleCalendar", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("ICal.OpenGoogleCalendar", value); }
        }

    }
}
