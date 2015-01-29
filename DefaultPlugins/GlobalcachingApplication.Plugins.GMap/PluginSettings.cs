using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.GMap
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
            get { return _core.SettingsProvider.GetSettingsValueRectangle("GMap.WindowPos", Rectangle.Empty); }
            set { _core.SettingsProvider.SetSettingsValueRectangle("GMap.WindowPos", value); }
        }

        public int ClusterMarkerThreshold
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("GMap.ClusterMarkerThreshold", 500); }
            set { _core.SettingsProvider.SetSettingsValueInt("GMap.ClusterMarkerThreshold", value); }
        }

        public int ClusterMarkerMaxZoomLevel
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("GMap.ClusterMarkerMaxZoomLevel", 13); }
            set { _core.SettingsProvider.SetSettingsValueInt("GMap.ClusterMarkerMaxZoomLevel", value); }
        }

        public int ClusterMarkerGridSize
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("GMap.ClusterMarkerGridSize", 50); }
            set { _core.SettingsProvider.SetSettingsValueInt("GMap.ClusterMarkerGridSize", value); }
        }

        public bool ShowNameInToolTip
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("GMap.ShowNameInToolTip", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("GMap.ShowNameInToolTip", value); }
        }

        public bool AutoTopPanel
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("GMap.AutoTopPanel", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("GMap.AutoTopPanel", value); }
        }

        public bool AddSelectedMarkers
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("GMap.AddSelectedMarkers", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("GMap.AddSelectedMarkers", value); }
        }
    }
}
