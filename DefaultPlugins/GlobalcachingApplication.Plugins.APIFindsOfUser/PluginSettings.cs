using GlobalcachingApplication.Framework;
using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.APIFindsOfUser
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

        public InterceptedStringCollection Usernames
        {
            get { return _core.SettingsProvider.GetSettingsValueStringCollection("APIFindsOfUser.Usernames", null); }
            set { _core.SettingsProvider.SetSettingsValueStringCollection("APIFindsOfUser.Usernames", value); }
        }

        public InterceptedStringCollection LogTypes
        {
            get { return _core.SettingsProvider.GetSettingsValueStringCollection("APIFindsOfUser.LogTypes", null); }
            set { _core.SettingsProvider.SetSettingsValueStringCollection("APIFindsOfUser.LogTypes", value); }
        }

        public bool ImportMissingCaches
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("APIFindsOfUser.ImportMissingCaches", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("APIFindsOfUser.ImportMissingCaches", value); }
        }

        public bool BetweenDates
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("APIFindsOfUser.BetweenDates", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("APIFindsOfUser.BetweenDates", value); }
        }
    }
}
