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
            Username = Properties.Settings.Default.SiteInfoNetherlandsUsername ?? "";
            UserID = Properties.Settings.Default.SiteInfoNetherlandsUserID ?? "";
            Token = Properties.Settings.Default.SiteInfoNetherlandsToken ?? "";
            TokenSecret = Properties.Settings.Default.SiteInfoNetherlandsTokenSecret ?? "";

            base.LoadSettings();
        }

        public override void SaveSettings()
        {
            Properties.Settings.Default.SiteInfoNetherlandsUsername = Username;
            Properties.Settings.Default.SiteInfoNetherlandsUserID = UserID;
            Properties.Settings.Default.SiteInfoNetherlandsToken = Token;
            Properties.Settings.Default.SiteInfoNetherlandsTokenSecret = TokenSecret;
            Properties.Settings.Default.Save();
        }

    }
}
