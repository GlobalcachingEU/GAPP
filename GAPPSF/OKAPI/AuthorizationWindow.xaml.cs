using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GAPPSF.OKAPI
{
    /// <summary>
    /// Interaction logic for AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        private SiteInfo _siteInfo = null;
        private string _token = null;
        private string _tokenSecret = null;

        public AuthorizationWindow()
        {
            InitializeComponent();
        }

        public AuthorizationWindow(SiteInfo siteInfo)
            : this()
        {
            _siteInfo = siteInfo;

            webBrowser.LoadCompleted += webBrowser_LoadCompleted;
            webBrowser.NavigateToString("<html><body><h1>Loading authorization page, please wait...</h1></body></html>");
        }

        public string Token
        {
            get { return _token; }
        }
        public string TokenSecret
        {
            get { return _tokenSecret; }
        }


        void webBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri == null)
            {
                webBrowser.Navigate(string.Format("http://application.globalcaching.eu/TokenRequest{0}.aspx", _siteInfo.GeocodePrefix));
            }
            else if (e.Uri.AbsoluteUri.ToLower().StartsWith(string.Format("http://application.globalcaching.eu/tokenresult{0}.aspx", _siteInfo.GeocodePrefix.ToLower())))
            {
                try
                {
                    _token = webBrowser.InvokeScript("getToken") as string;
                    _tokenSecret = webBrowser.InvokeScript("getTokenSecret") as string;
                }
                catch (Exception ex)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, ex);
                }
                DialogResult = true;
                Close();
            }
        }
    }
}
