using GlobalcachingApplication.Framework;
using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.OpenAreas
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
            get { return _core.SettingsProvider.GetSettingsValueRectangle("OpenAreas.WindowPos", Rectangle.Empty); }
            set { _core.SettingsProvider.SetSettingsValueRectangle("OpenAreas.WindowPos", value); }
        }

        public bool MysteryOnlyIfCorrected
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("OpenAreas.MysteryOnlyIfCorrected", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("OpenAreas.MysteryOnlyIfCorrected", value); }
        }

        public bool Waypoints
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("OpenAreas.Waypoints", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("OpenAreas.Waypoints", value); }
        }

        public bool CustomWaypoints
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("OpenAreas.CustomWaypoints", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("OpenAreas.CustomWaypoints", value); }
        }

        public int Radius
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("OpenAreas.Radius", 161); }
            set { _core.SettingsProvider.SetSettingsValueInt("OpenAreas.Radius", value); }
        }

        public int FillOpacity
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("OpenAreas.FillOpacity", 50); }
            set { _core.SettingsProvider.SetSettingsValueInt("OpenAreas.FillOpacity", value); }
        }

        public int StrokeOpacity
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("OpenAreas.StrokeOpacity", 50); }
            set { _core.SettingsProvider.SetSettingsValueInt("OpenAreas.StrokeOpacity", value); }
        }

        public InterceptedStringCollection CustomWaypointsList
        {
            get { return _core.SettingsProvider.GetSettingsValueStringCollection("OpenAreas.CustomWaypointsList", null); }
            set { _core.SettingsProvider.SetSettingsValueStringCollection("OpenAreas.CustomWaypointsList", value); }
        }

        public Color GeocacheColor
        {
            get { return _core.SettingsProvider.GetSettingsValueColor("OpenAreas.GeocacheColor", Color.Red); }
            set { _core.SettingsProvider.SetSettingsValueColor("OpenAreas.GeocacheColor", value); }
        }

        public Color WaypointColor
        {
            get { return _core.SettingsProvider.GetSettingsValueColor("OpenAreas.WaypointColor", Color.FromArgb(255, 128, 255)); }
            set { _core.SettingsProvider.SetSettingsValueColor("OpenAreas.WaypointColor", value); }
        }

        public Color CustomColor
        {
            get { return _core.SettingsProvider.GetSettingsValueColor("OpenAreas.CustomColor", Color.FromArgb(128, 255, 128)); }
            set { _core.SettingsProvider.SetSettingsValueColor("OpenAreas.CustomColor", value); }
        }

    }
}
