using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.LogImagesViewer
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

        public bool CacheImages
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("LogImagesViewer.CacheImages", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("LogImagesViewer.CacheImages", value); }
        }

        public bool Slideshow
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("LogImagesViewer.Slideshow", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("LogImagesViewer.Slideshow", value); }
        }

        public int SlideshowNextDelay
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("LogImagesViewer.SlideshowNextDelay", 3); }
            set { _core.SettingsProvider.SetSettingsValueInt("LogImagesViewer.SlideshowNextDelay", value); }
        }

        public Rectangle WindowPos
        {
            get { return _core.SettingsProvider.GetSettingsValueRectangle("LogImagesViewer.WindowPos", Rectangle.Empty); }
            set { _core.SettingsProvider.SetSettingsValueRectangle("LogImagesViewer.WindowPos", value); }
        }

        public Rectangle WindowPos2
        {
            get { return _core.SettingsProvider.GetSettingsValueRectangle("LogImagesViewer.WindowPos2", Rectangle.Empty); }
            set { _core.SettingsProvider.SetSettingsValueRectangle("LogImagesViewer.WindowPos2", value); }
        }

    }
}
