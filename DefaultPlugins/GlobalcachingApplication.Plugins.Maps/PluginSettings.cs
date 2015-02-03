using GlobalcachingApplication.Framework;
using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.Maps
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

        public InterceptedStringCollection DisabledMaps
        {
            get { return _core.SettingsProvider.GetSettingsValueStringCollection("Maps.DisabledMaps", null); }
            set { _core.SettingsProvider.SetSettingsValueStringCollection("Maps.DisabledMaps", value); }
        }

        public InterceptedStringCollection DecoupledChildWindows
        {
            get { return _core.SettingsProvider.GetSettingsValueStringCollection("Maps.DecoupledChildWindows", null); }
            set { _core.SettingsProvider.SetSettingsValueStringCollection("Maps.DecoupledChildWindows", value); }
        }

        public InterceptedStringCollection SpecifiedWindowPos
        {
            get { return _core.SettingsProvider.GetSettingsValueStringCollection("Maps.SpecifiedWindowPos", null); }
            set { _core.SettingsProvider.SetSettingsValueStringCollection("Maps.SpecifiedWindowPos", value); }
        }

        public InterceptedStringCollection TopMostWindows
        {
            get { return _core.SettingsProvider.GetSettingsValueStringCollection("Maps.TopMostWindows", null); }
            set { _core.SettingsProvider.SetSettingsValueStringCollection("Maps.TopMostWindows", value); }
        }

        public string OSMOfflineMapFolder
        {
            get { return _core.SettingsProvider.GetSettingsValue("Maps.OSMOfflineMapFolder", null); }
            set { _core.SettingsProvider.SetSettingsValue("Maps.OSMOfflineMapFolder", value); }
        }

        public int TileServerPort
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("Maps.TileServerPort", 6231); }
            set { _core.SettingsProvider.SetSettingsValueInt("Maps.TileServerPort", value); }
        }

        public Rectangle WindowPos
        {
            get { return _core.SettingsProvider.GetSettingsValueRectangle("Maps.WindowPos", Rectangle.Empty); }
            set { _core.SettingsProvider.SetSettingsValueRectangle("Maps.WindowPos", value); }
        }

    }
}
