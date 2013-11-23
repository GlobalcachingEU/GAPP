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

namespace GAPPSF.LiveAPI
{
    /// <summary>
    /// Interaction logic for AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        private bool _testSite = false;
        public string Token { get; private set; }

        public AuthorizationWindow()
        {
            InitializeComponent();
        }

        void webBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri==null)
            {
                if (_testSite)
                {
                    webBrowser.Navigate("http://application.globalcaching.eu/TokenRequest.aspx?target=staging");
                }
                else
                {
                    webBrowser.Navigate("http://application.globalcaching.eu/TokenRequest.aspx");
                }

            }
            else if (e.Uri.AbsoluteUri.ToLower().StartsWith("http://application.globalcaching.eu/tokenresult.aspx"))
            {
                try
                {
                    Token = webBrowser.InvokeScript("getToken") as string;
                }
                catch
                {
                }
                DialogResult = true;
                Close();
            }            
        }

        public AuthorizationWindow(bool useTestSite): this()
        {
            _testSite = useTestSite;

            webBrowser.LoadCompleted += webBrowser_LoadCompleted;
            webBrowser.NavigateToString("<html><body><h1>Loading authorization page, please wait...</h1></body></html>");

        }
    }
}
