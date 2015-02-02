using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public class SiteInfoUK: SiteInfo
    {
        public const string STR_INFO = "opencaching.org.uk";

        public SiteInfoUK()
        {
            ID = "5";
            Info = STR_INFO;
            OKAPIBaseUrl = "http://www.opencaching.org.uk/okapi/";
            GeocodePrefix = "OK";
        }

        public override void LoadSettings()
        {
            Username = PluginSettings.Instance.SiteInfoUKUsername ?? "";
            UserID = PluginSettings.Instance.SiteInfoUKUserID ?? "";
            Token = PluginSettings.Instance.SiteInfoUKToken ?? "";
            TokenSecret = PluginSettings.Instance.SiteInfoUKTokenSecret ?? "";

            base.LoadSettings();
        }

        public override void SaveSettings()
        {
            PluginSettings.Instance.SiteInfoUKUsername = Username;
            PluginSettings.Instance.SiteInfoUKUserID = UserID;
            PluginSettings.Instance.SiteInfoUKToken = Token;
            PluginSettings.Instance.SiteInfoUKTokenSecret = TokenSecret;
        }

    }
}
