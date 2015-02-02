using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.OKAPI
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

        public string ActiveSiteID
        {
            get { return _core.SettingsProvider.GetSettingsValue("OKAPI.ActiveSiteID", null); }
            set { _core.SettingsProvider.SetSettingsValue("OKAPI.ActiveSiteID", value); }
        }

        public string SiteInfoGermanyUsername
        {
            get { return _core.SettingsProvider.GetSettingsValue("OKAPI.SiteInfoGermanyUsername", null); }
            set { _core.SettingsProvider.SetSettingsValue("OKAPI.SiteInfoGermanyUsername", value); }
        }

        public string SiteInfoGermanyUserID
        {
            get { return _core.SettingsProvider.GetSettingsValue("OKAPI.SiteInfoGermanyUserID", null); }
            set { _core.SettingsProvider.SetSettingsValue("OKAPI.SiteInfoGermanyUserID", value); }
        }

        public string SiteInfoNetherlandsUsername
        {
            get { return _core.SettingsProvider.GetSettingsValue("OKAPI.SiteInfoNetherlandsUsername", null); }
            set { _core.SettingsProvider.SetSettingsValue("OKAPI.SiteInfoNetherlandsUsername", value); }
        }

        public string SiteInfoNetherlandsUserID
        {
            get { return _core.SettingsProvider.GetSettingsValue("OKAPI.SiteInfoNetherlandsUserID", null); }
            set { _core.SettingsProvider.SetSettingsValue("OKAPI.SiteInfoNetherlandsUserID", value); }
        }

        public string SiteInfoPolandUsername
        {
            get { return _core.SettingsProvider.GetSettingsValue("OKAPI.SiteInfoPolandUsername", null); }
            set { _core.SettingsProvider.SetSettingsValue("OKAPI.SiteInfoPolandUsername", value); }
        }

        public string SiteInfoPolandUserID
        {
            get { return _core.SettingsProvider.GetSettingsValue("OKAPI.SiteInfoPolandUserID", null); }
            set { _core.SettingsProvider.SetSettingsValue("OKAPI.SiteInfoPolandUserID", value); }
        }

        public string SiteInfoUSAUsername
        {
            get { return _core.SettingsProvider.GetSettingsValue("OKAPI.SiteInfoUSAUsername", null); }
            set { _core.SettingsProvider.SetSettingsValue("OKAPI.SiteInfoUSAUsername", value); }
        }

        public string SiteInfoUSAUserID
        {
            get { return _core.SettingsProvider.GetSettingsValue("OKAPI.SiteInfoUSAUserID", null); }
            set { _core.SettingsProvider.SetSettingsValue("OKAPI.SiteInfoUSAUserID", value); }
        }

        public string SiteInfoUKUsername
        {
            get { return _core.SettingsProvider.GetSettingsValue("OKAPI.SiteInfoUKUsername", null); }
            set { _core.SettingsProvider.SetSettingsValue("OKAPI.SiteInfoUKUsername", value); }
        }

        public string SiteInfoUKUserID
        {
            get { return _core.SettingsProvider.GetSettingsValue("OKAPI.SiteInfoUKUserID", null); }
            set { _core.SettingsProvider.SetSettingsValue("OKAPI.SiteInfoUKUserID", value); }
        }

        public string SiteInfoGermanyToken
        {
            get { return _core.SettingsProvider.GetSettingsValue("OKAPI.SiteInfoGermanyToken", null); }
            set { _core.SettingsProvider.SetSettingsValue("OKAPI.SiteInfoGermanyToken", value); }
        }

        public string SiteInfoGermanyTokenSecret
        {
            get { return _core.SettingsProvider.GetSettingsValue("OKAPI.SiteInfoGermanyTokenSecret", null); }
            set { _core.SettingsProvider.SetSettingsValue("OKAPI.SiteInfoGermanyTokenSecret", value); }
        }

        public string SiteInfoNetherlandsToken
        {
            get { return _core.SettingsProvider.GetSettingsValue("OKAPI.SiteInfoNetherlandsToken", null); }
            set { _core.SettingsProvider.SetSettingsValue("OKAPI.SiteInfoNetherlandsToken", value); }
        }
        public string SiteInfoNetherlandsTokenSecret
        {
            get { return _core.SettingsProvider.GetSettingsValue("OKAPI.SiteInfoNetherlandsTokenSecret", null); }
            set { _core.SettingsProvider.SetSettingsValue("OKAPI.SiteInfoNetherlandsTokenSecret", value); }
        }

        public string SiteInfoPolandToken
        {
            get { return _core.SettingsProvider.GetSettingsValue("OKAPI.SiteInfoPolandToken", null); }
            set { _core.SettingsProvider.SetSettingsValue("OKAPI.SiteInfoPolandToken", value); }
        }

        public string SiteInfoPolandTokenSecret
        {
            get { return _core.SettingsProvider.GetSettingsValue("OKAPI.SiteInfoPolandTokenSecret", null); }
            set { _core.SettingsProvider.SetSettingsValue("OKAPI.SiteInfoPolandTokenSecret", value); }
        }

        public string SiteInfoUSAToken
        {
            get { return _core.SettingsProvider.GetSettingsValue("OKAPI.SiteInfoUSAToken", null); }
            set { _core.SettingsProvider.SetSettingsValue("OKAPI.SiteInfoUSAToken", value); }
        }

        public string SiteInfoUSATokenSecret
        {
            get { return _core.SettingsProvider.GetSettingsValue("OKAPI.SiteInfoUSATokenSecret", null); }
            set { _core.SettingsProvider.SetSettingsValue("OKAPI.SiteInfoUSATokenSecret", value); }
        }

        public string SiteInfoUKToken
        {
            get { return _core.SettingsProvider.GetSettingsValue("OKAPI.SiteInfoUKToken", null); }
            set { _core.SettingsProvider.SetSettingsValue("OKAPI.SiteInfoUKToken", value); }
        }

        public string SiteInfoUKTokenSecret
        {
            get { return _core.SettingsProvider.GetSettingsValue("OKAPI.SiteInfoUKTokenSecret", null); }
            set { _core.SettingsProvider.SetSettingsValue("OKAPI.SiteInfoUKTokenSecret", value); }
        }

    }
}
