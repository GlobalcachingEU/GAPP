using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GAPPSF.OKAPI
{
    public class SiteInfo : INotifyPropertyChanged
    {
        public string ID { get; set; }
        public string Info { get; set; }

        public string OKAPIBaseUrl { get; set; } //e.g. http://www.opencaching.de/okapi/
        public string GeocodePrefix { get; set; } //e.g. OP
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }

        private string _token;
        public string Token 
        {
            get { return _token; }
            set { SetProperty(ref _token, value); SignalAuthorized(); } 
        }
        private string _tokenSecret;
        public string TokenSecret
        {
            get { return _tokenSecret; }
            set { SetProperty(ref _tokenSecret, value); SignalAuthorized(); }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }
        private string _userID;
        public string UserID //obtained from OKAPI with method services/users/by_username
        {
            get { return _userID; }
            set { SetProperty(ref _userID, value); }
        }

        public virtual void LoadSettings()
        {
            try
            {
                string doc = Utils.ResourceHelper.GetEmbeddedTextFile("/OKAPI/ConsumerKeys.xml");
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
            return Localization.TranslationManager.Instance.Translate(Info ?? "") as string;
        }

        public bool IsAuthorized
        {
            get { return (!string.IsNullOrEmpty(Token) && !string.IsNullOrEmpty(TokenSecret)); }
        }
        public void SignalAuthorized()
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs("IsAuthorized"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }

    }
}
