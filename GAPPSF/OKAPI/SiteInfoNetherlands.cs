using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.OKAPI
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
            Username = Core.ApplicationData.Instance.AccountInfos.GetAccountInfo(GeocodePrefix).AccountName ?? "";
            UserID = Core.Settings.Default.OKAPISiteInfoNetherlandsUserID ?? "";
            Token = Core.Settings.Default.OKAPISiteInfoNetherlandsToken ?? "";
            TokenSecret = Core.Settings.Default.OKAPISiteInfoNetherlandsTokenSecret ?? "";

            base.LoadSettings();
        }

        public override void SaveSettings()
        {
            Core.ApplicationData.Instance.AccountInfos.GetAccountInfo(GeocodePrefix).AccountName = Username ?? "";
            Core.Settings.Default.OKAPISiteInfoNetherlandsUserID = UserID ?? "";
            Core.Settings.Default.OKAPISiteInfoNetherlandsToken = Token ?? "";
            Core.Settings.Default.OKAPISiteInfoNetherlandsTokenSecret = TokenSecret ?? "";
        }

    }
}
