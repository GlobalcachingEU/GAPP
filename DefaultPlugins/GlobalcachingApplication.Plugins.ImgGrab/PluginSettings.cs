using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.ImgGrab
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

        public bool DownloadBeforeCreate
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ImgGrab.DownloadBeforeCreate", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("ImgGrab.DownloadBeforeCreate", value); }
        }

        public bool CopyNotInDescription
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ImgGrab.CopyNotInDescription", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("ImgGrab.CopyNotInDescription", value); }
        }

        public bool ClearBeforeCopy
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("ImgGrab.ClearBeforeCopy", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("ImgGrab.ClearBeforeCopy", value); }
        }

        public string ActiveDataPath
        {
            get { return _core.SettingsProvider.GetSettingsValue("ImgGrab.ActiveDataPath", null); }
            set { _core.SettingsProvider.SetSettingsValue("ImgGrab.ActiveDataPath", value); }
        }

        public string CreateFolderPath
        {
            get { return _core.SettingsProvider.GetSettingsValue("ImgGrab.CreateFolderPath", null); }
            set { _core.SettingsProvider.SetSettingsValue("ImgGrab.CreateFolderPath", value); }
        }

    }
}
