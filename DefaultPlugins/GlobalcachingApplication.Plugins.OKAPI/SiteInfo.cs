using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public class SiteInfo
    {
        public string ID { get; set; }
        public string Info { get; set; }

        public string OKAPIBaseUrl { get; set; } //e.g. http://www.opencaching.de/okapi/
        public string GeocodePrefix { get; set; } //e.g. OP
        public string ConsumerKey { get; set; }

        public string Username { get; set; } //like jonny gogo
        public string UserID { get; set; } //obtained from OKAPI with method services/users/by_username

        public virtual void LoadSettings()
        {
        }

        public virtual void SaveSettings()
        {
        }

        public override string ToString()
        {
            return Utils.LanguageSupport.Instance.GetTranslation(Info ?? "");
        }
    }
}
