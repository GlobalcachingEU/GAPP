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
            Username = Properties.Settings.Default.SiteInfoPolandUsername ?? "";
            UserID = Properties.Settings.Default.SiteInfoPolandUserID ?? "";
            Token = Properties.Settings.Default.SiteInfoPolandToken ?? "";
            TokenSecret = Properties.Settings.Default.SiteInfoPolandTokenSecret ?? "";

            base.LoadSettings();
        }

        public override void SaveSettings()
        {
            Properties.Settings.Default.SiteInfoPolandUsername = Username;
            Properties.Settings.Default.SiteInfoPolandUserID = UserID;
            Properties.Settings.Default.SiteInfoPolandToken = Token;
            Properties.Settings.Default.SiteInfoPolandTokenSecret = TokenSecret;
            Properties.Settings.Default.Save();
        }

    }
}
