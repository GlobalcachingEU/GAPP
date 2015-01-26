using CefSharp;
using CefSharp.WinForms;
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
        private IWinFormsWebBrowser Browser = null;

        public KeyRequestForm(): this( Framework.Data.APIKey.GeocachingLive)
        {
        }

        public KeyRequestForm(Framework.Data.APIKey keyType)
        {
            InitializeComponent();
            string url;
            if (_apiKeyType == Framework.Data.APIKey.GeocachingLive)
            {
                url = "http://application.globalcaching.eu/TokenRequest.aspx";
            }
            else
            {
                url = "http://application.globalcaching.eu/TokenRequest.aspx?target=staging";
            }

            var browser = new ChromiumWebBrowser(url)
            {
                Dock = DockStyle.Fill
            };
            panel1.Controls.Add(browser);
            browser.IsLoadingChanged += browser_IsLoadingChanged;
            Browser = browser;
            _apiKeyType = keyType;

            Disposed += KeyRequestForm_Disposed;
        }

        void browser_IsLoadingChanged(object sender, IsLoadingChangedEventArgs e)
        {
            if (!e.IsLoading)
            {
                if (Browser.Address.ToLower().StartsWith("http://application.globalcaching.eu/tokenresult.aspx"))
                {
                    try
                    {
                        var task = Browser.EvaluateScriptAsync("getToken()");
                        task.Wait();
                        _token = task.Result.Result as string;
                    }
                    catch
                    {
                    }
                    this.BeginInvoke(new Action(this.Close));
                }
            }
        }


        void KeyRequestForm_Disposed(object sender, EventArgs e)
        {
            Disposed -= KeyRequestForm_Disposed;
            if (Browser != null)
            {
                var browser = (ChromiumWebBrowser)Browser;
                browser.IsLoadingChanged -= browser_IsLoadingChanged;
                Browser.Dispose();
                Browser = null;
            }
        }

        public string Token
        {
            get { return _token; }
        }
    }
}
