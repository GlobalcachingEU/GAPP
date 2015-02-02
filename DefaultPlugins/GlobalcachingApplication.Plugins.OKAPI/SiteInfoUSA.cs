using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public class SiteInfoUSA: SiteInfo
    {
        public const string STR_INFO = "opencaching.us";

        public SiteInfoUSA()
        {
            ID = "4";
            Info = STR_INFO;
            OKAPIBaseUrl = "http://www.opencaching.us/okapi/";
            GeocodePrefix = "OU";
        }

        public override void LoadSettings()
        {
            Username = PluginSettings.Instance.SiteInfoUSAUsername ?? "";
            UserID = PluginSettings.Instance.SiteInfoUSAUserID ?? "";
            Token = PluginSettings.Instance.SiteInfoUSAToken ?? "";
            TokenSecret = PluginSettings.Instance.SiteInfoUSATokenSecret ?? "";

            base.LoadSettings();
        }

        public override void SaveSettings()
        {
            PluginSettings.Instance.SiteInfoUSAUsername = Username;
            PluginSettings.Instance.SiteInfoUSAUserID = UserID;
            PluginSettings.Instance.SiteInfoUSAToken = Token;
            PluginSettings.Instance.SiteInfoUSATokenSecret = TokenSecret;
        }

    }
}
