using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Core
{
    public partial class KeyRequestForm : Form
    {
        private Framework.Data.APIKey _apiKeyType;
        private string _token = null;

        public KeyRequestForm(): this( Framework.Data.APIKey.GeocachingLive)
        {
        }

        public KeyRequestForm(Framework.Data.APIKey keyType)
        {
            InitializeComponent();
            _apiKeyType = keyType;
        }

        public string Token
        {
            get { return _token; }
        }

        private void KeyRequestForm_Shown(object sender, EventArgs e)
        {
            if (_apiKeyType == Framework.Data.APIKey.GeocachingLive)
            {
                webBrowser1.Navigate("http://application.globalcaching.eu/TokenRequest.aspx");
            }
            else
            {
                webBrowser1.Navigate("http://application.globalcaching.eu/TokenRequest.aspx?target=staging");
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            toolStripProgressBar1.Visible = false;
            toolStripStatusLabel1.Text = "idle";
            if (e.Url.AbsoluteUri.ToLower().StartsWith("http://application.globalcaching.eu/tokenresult.aspx"))
            {
                try
                {
                    _token = webBrowser1.Document.InvokeScript("getToken") as string;
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
