using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.OKAPI
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
            Username = Core.ApplicationData.Instance.AccountInfos.GetAccountInfo(GeocodePrefix).AccountName ?? "";
            UserID = Core.Settings.Default.OKAPISiteInfoGermanyUserID ?? "";
            Token = Core.Settings.Default.OKAPISiteInfoGermanyToken ?? "";
            TokenSecret = Core.Settings.Default.OKAPISiteInfoGermanyTokenSecret ?? "";

            base.LoadSettings();
        }

        public override void SaveSettings()
        {
            Core.ApplicationData.Instance.AccountInfos.GetAccountInfo(GeocodePrefix).AccountName = Username ?? "";
            Core.Settings.Default.OKAPISiteInfoGermanyUserID = UserID ?? "";
            Core.Settings.Default.OKAPISiteInfoGermanyToken = Token ?? "";
            Core.Settings.Default.OKAPISiteInfoGermanyTokenSecret = TokenSecret ?? "";
        }

    }
}
