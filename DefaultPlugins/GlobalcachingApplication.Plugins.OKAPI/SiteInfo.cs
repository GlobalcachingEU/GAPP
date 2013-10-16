using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public class SiteInfo
    {
        public string ID { get; set; }
        public string Info { get; set; }

        public string OKAPIBaseUrl { get; set; } //e.g. http://www.opencaching.de/okapi/
        public string GeocodePrefix { get; set; } //e.g. OP
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string Token { get; set; }
        public string TokenSecret { get; set; }

        public string Username { get; set; } //like jonny gogo
        public string UserID { get; set; } //obtained from OKAPI with method services/users/by_username

        public virtual void LoadSettings()
        {
            try
            {
                string doc;
                using (StreamReader textStreamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("GlobalcachingApplication.Plugins.OKAPI.ConsumerKeys.xml")))
                {
                    doc = textStreamReader.ReadToEnd();
                }
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(doc);
                XmlNode n = xdoc.SelectSingleNode(string.Format("/keys/{0}", GeocodePrefix));
                if (n != null)
                {
                    ConsumerKey = n.SelectSingleNode("ConsumerKey").InnerText;
                    ConsumerSecret = n.SelectSingleNode("ConsumerSecret").InnerText;
                }

                if (ConsumerKey == null || ConsumerKey.IndexOf(' ') > 0)
                {
                    ConsumerKey = "";
                }
                if (ConsumerSecret == null || ConsumerSecret.IndexOf(' ') > 0)
                {
                    ConsumerSecret = "";
                }
            }
            catch
            {
            }
        }

        public virtual void SaveSettings()
        {
        }

        public override string ToString()
        {
            return Utils.LanguageSupport.Instance.GetTranslation(Info ?? "");
        }

        public bool IsAuthorized
        {
            get { return (!string.IsNullOrEmpty(Token) && !string.IsNullOrEmpty(TokenSecret)); }
        }
    }
}
