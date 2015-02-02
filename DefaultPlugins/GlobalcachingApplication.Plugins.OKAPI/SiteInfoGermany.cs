using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public class SiteInfoGermany: SiteInfo
    {
        public const string STR_INFO = "opencaching.de";

        public SiteInfoGermany()
        {
            ID = "1";
            Info = STR_INFO;
            OKAPIBaseUrl = "http://www.opencaching.de/okapi/";
            GeocodePrefix = "OC";
        }

        public override void LoadSettings()
        {
            Username = PluginSettings.Instance.SiteInfoGermanyUsername ?? "";
            UserID = PluginSettings.Instance.SiteInfoGermanyUserID ?? "";
            Token = PluginSettings.Instance.SiteInfoGermanyToken ?? "";
            TokenSecret = PluginSettings.Instance.SiteInfoGermanyTokenSecret ?? "";

            base.LoadSettings();
        }

        public override void SaveSettings()
        {
            PluginSettings.Instance.SiteInfoGermanyUsername = Username;
            PluginSettings.Instance.SiteInfoGermanyUserID = UserID;
            PluginSettings.Instance.SiteInfoGermanyToken = Token;
            PluginSettings.Instance.SiteInfoGermanyTokenSecret = TokenSecret;
        }

    }
}
