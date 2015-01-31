using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.GeoSpy
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


        public string GPXTagCivil
        {
            get { return _core.SettingsProvider.GetSettingsValue("GeoSpy.GPXTagCivil", "Virtual Cache"); }
            set { _core.SettingsProvider.SetSettingsValue("GeoSpy.GPXTagCivil", value); }
        }

        public string GPXTagHistoricAndReligious
        {
            get { return _core.SettingsProvider.GetSettingsValue("GeoSpy.GPXTagHistoricAndReligious", "Virtual Cache"); }
            set { _core.SettingsProvider.SetSettingsValue("GeoSpy.GPXTagHistoricAndReligious", value); }
        }

        public string GPXTagNatural
        {
            get { return _core.SettingsProvider.GetSettingsValue("GeoSpy.GPXTagNatural", "Virtual Cache"); }
            set { _core.SettingsProvider.SetSettingsValue("GeoSpy.GPXTagNatural", value); }
        }

        public string GPXTagTechnical
        {
            get { return _core.SettingsProvider.GetSettingsValue("GeoSpy.GPXTagTechnical", "Virtual Cache"); }
            set { _core.SettingsProvider.SetSettingsValue("GeoSpy.GPXTagTechnical", value); }
        }

        public string GPXTagMilitary
        {
            get { return _core.SettingsProvider.GetSettingsValue("GeoSpy.GPXTagMilitary", "Virtual Cache"); }
            set { _core.SettingsProvider.SetSettingsValue("GeoSpy.GPXTagMilitary", value); }
        }

    }
}
