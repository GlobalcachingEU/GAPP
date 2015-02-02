using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public class SiteInfoNetherlands: SiteInfo
    {
        public const string STR_INFO = "opencaching.nl";

        public SiteInfoNetherlands()
        {
            ID = "2";
            Info = STR_INFO;
            OKAPIBaseUrl = "http://www.opencaching.nl/okapi/";
            GeocodePrefix = "OB";
        }

        public override void LoadSettings()
        {
            Username = PluginSettings.Instance.SiteInfoNetherlandsUsername ?? "";
            UserID = PluginSettings.Instance.SiteInfoNetherlandsUserID ?? "";
            Token = PluginSettings.Instance.SiteInfoNetherlandsToken ?? "";
            TokenSecret = PluginSettings.Instance.SiteInfoNetherlandsTokenSecret ?? "";

            base.LoadSettings();
        }

        public override void SaveSettings()
        {
            PluginSettings.Instance.SiteInfoNetherlandsUsername = Username;
            PluginSettings.Instance.SiteInfoNetherlandsUserID = UserID;
            PluginSettings.Instance.SiteInfoNetherlandsToken = Token;
            PluginSettings.Instance.SiteInfoNetherlandsTokenSecret = TokenSecret;
        }

    }
}
