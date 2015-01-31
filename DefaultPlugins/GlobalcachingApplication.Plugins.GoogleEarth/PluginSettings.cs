using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.GoogleEarth
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
            get { return _core.SettingsProvider.GetSettingsValueRectangle("GoogleEarth.WindowPos", Rectangle.Empty); }
            set { _core.SettingsProvider.SetSettingsValueRectangle("GoogleEarth.WindowPos", value); }
        }

        public int TiltView
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("GoogleEarth.TiltView", 45); }
            set { _core.SettingsProvider.SetSettingsValueInt("GoogleEarth.TiltView", value); }
        }

        public int AltitudeView
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("GoogleEarth.AltitudeView", 500); }
            set { _core.SettingsProvider.SetSettingsValueInt("GoogleEarth.AltitudeView", value); }
        }


        public bool FixedView
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("GoogleEarth.FixedView", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("GoogleEarth.FixedView", value); }
        }

        public double FlyToSpeed
        {
            get { return _core.SettingsProvider.GetSettingsValueDouble("GoogleEarth.FlyToSpeed", 0.5); }
            set { _core.SettingsProvider.SetSettingsValueDouble("GoogleEarth.FlyToSpeed", value); }
        }
    }
}
