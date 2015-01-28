using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

namespace GlobalcachingApplication.Utils.Controls
{
    public partial class GAPPWebBrowser : UserControl
    {
        public IWinFormsWebBrowser Browser { get; private set; }
        public ChromiumWebBrowser ChromiumBrowser { get { return Browser as ChromiumWebBrowser; } }

        public GAPPWebBrowser()
        {
            InitializeComponent();
        }

        public GAPPWebBrowser(string url): this()
        {
            Dock = DockStyle.Fill;
            var browser = new ChromiumWebBrowser(url)
            {
                Dock = DockStyle.Fill
            };
            panel1.Controls.Add(browser);
            Browser = browser;

            Disposed += GAPPWebBrowser_Disposed;
        }

        void GAPPWebBrowser_Disposed(object sender, EventArgs e)
        {
            Disposed -= GAPPWebBrowser_Disposed;
            if (Browser != null)
            {
                var browser = ChromiumBrowser;
                Browser.Dispose();
                Browser = null;
            }            
        }

        public string DocumentText
        {
            get { return null; }
            set
            {
                //ChromiumBrowser.LoadHtml(value, @"c:\temp.html");
                ChromiumBrowser.LoadHtml(value, @"http://www.google.com/");
            }
        }

        public bool IsReady
        {
            get { return Browser.IsBrowserInitialized && !Browser.IsLoading; }
        }

        public object InvokeScript(string script)
        {
            object result = null;
            try
            {
                var task = Browser.EvaluateScriptAsync(script);
                task.Wait();
                result = task.Result.Result;
            }
            catch
            {
            }
            return result;
        }
    }
}
