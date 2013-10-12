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
            ConsumerKey = "hCYQD3KLqSCc3wMKFqKZ";
        }

        public override void LoadSettings()
        {
            Username = Properties.Settings.Default.SiteInfoGermanyUsername ?? "";
            UserID = Properties.Settings.Default.SiteInfoGermanyUserID ?? "";
        }

        public override void SaveSettings()
        {
            Properties.Settings.Default.SiteInfoGermanyUsername = Username;
            Properties.Settings.Default.SiteInfoGermanyUserID = UserID;
            Properties.Settings.Default.Save();
        }

    }
}
