using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public class SiteInfoPoland: SiteInfo
    {
        public const string STR_INFO = "opencaching.pl";

        public SiteInfoPoland()
        {
            ID = "3";
            Info = STR_INFO;
            OKAPIBaseUrl = "http://opencaching.pl/okapi/";
            GeocodePrefix = "OP";
        }

        public override void LoadSettings()
        {
            Username = PluginSettings.Instance.SiteInfoPolandUsername ?? "";
            UserID = PluginSettings.Instance.SiteInfoPolandUserID ?? "";
            Token = PluginSettings.Instance.SiteInfoPolandToken ?? "";
            TokenSecret = PluginSettings.Instance.SiteInfoPolandTokenSecret ?? "";

            base.LoadSettings();
        }

        public override void SaveSettings()
        {
            PluginSettings.Instance.SiteInfoPolandUsername = Username;
            PluginSettings.Instance.SiteInfoPolandUserID = UserID;
            PluginSettings.Instance.SiteInfoPolandToken = Token;
            PluginSettings.Instance.SiteInfoPolandTokenSecret = TokenSecret;
        }

    }
}
