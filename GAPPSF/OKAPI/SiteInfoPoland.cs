using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.OKAPI
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
            Username = Core.ApplicationData.Instance.AccountInfos.GetAccountInfo(GeocodePrefix).AccountName ?? "";
            UserID = Core.Settings.Default.OKAPISiteInfoPolandUserID ?? "";
            Token = Core.Settings.Default.OKAPISiteInfoPolandToken ?? "";
            TokenSecret = Core.Settings.Default.OKAPISiteInfoPolandTokenSecret ?? "";

            base.LoadSettings();
        }

        public override void SaveSettings()
        {
            Core.ApplicationData.Instance.AccountInfos.GetAccountInfo(GeocodePrefix).AccountName = Username ?? "";
            Core.Settings.Default.OKAPISiteInfoPolandUserID = UserID ?? "";
            Core.Settings.Default.OKAPISiteInfoPolandToken = Token ?? "";
            Core.Settings.Default.OKAPISiteInfoPolandTokenSecret = TokenSecret ?? "";
        }

    }
}
