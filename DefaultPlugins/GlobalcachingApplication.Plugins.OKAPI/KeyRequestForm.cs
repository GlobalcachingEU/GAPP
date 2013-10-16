using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public partial class KeyRequestForm : Form
    {
        private SiteInfo _siteInfo = null;
        private string _token = null;
        private string _tokenSecret = null;

        public KeyRequestForm()
        {
            InitializeComponent();
        }

        public KeyRequestForm(SiteInfo siteInfo):this()
        {
            _siteInfo = siteInfo;
        }

        public string Token
        {
            get { return _token; }
        }
        public string TokenSecret
        {
            get { return _tokenSecret; }
        }

        private void KeyRequestForm_Shown(object sender, EventArgs e)
        {
            webBrowser1.Navigate(string.Format("http://application.globalcaching.eu/TokenRequest{0}.aspx", _siteInfo.GeocodePrefix));
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            toolStripProgressBar1.Visible = false;
            toolStripStatusLabel1.Text = "idle";
            if (e.Url.AbsoluteUri.ToLower().StartsWith(string.Format("http://application.globalcaching.eu/tokenresult{0}.aspx", _siteInfo.GeocodePrefix.ToLower())))
            {
                try
                {
                    _token = webBrowser1.Document.InvokeScript("getToken") as string;
                    _tokenSecret = webBrowser1.Document.InvokeScript("getTokenSecret") as string;
                }
                catch
                {
                }
                Close();
            }
        }

        private void webBrowser1_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
        {
            try
            {
                toolStripProgressBar1.Maximum = (int)e.MaximumProgress;
                toolStripProgressBar1.Value = (int)e.CurrentProgress;
            }
            catch
            {
            }
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            toolStripProgressBar1.Visible = true;
            //toolStripStatusLabel1.Text = e.Url.AbsoluteUri;
            toolStripStatusLabel1.Text = "Loading...";
        }
    }
}
