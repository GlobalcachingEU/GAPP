using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.DevelopTests
{
    public partial class WebBrowserForm : Form
    {
        public WebBrowserForm()
        {
            InitializeComponent();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //if (e.Url.AbsoluteUri == "http://wwww.geocaching.com")
            {
                HtmlDocument doc = webBrowser1.Document;
                HtmlElement head = doc.GetElementsByTagName("head")[0];
                HtmlElement s = doc.CreateElement("script");
                s.SetAttribute("text", "function sayHello() { alert('hello') }");
                head.AppendChild(s);
                webBrowser1.Document.InvokeScript("sayHello");
            }
        }

        private void WebBrowserForm_Shown(object sender, EventArgs e)
        {
            webBrowser1.Navigate("http://www.geocaching.com");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            //webBrowser1.Navigate("http://www.geocaching.com");
        }
    }

    public class WebBrowserTest : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SHOW = "WebBrowser test";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            //AddAction(ACTION_SHOW);

            return base.Initialize(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Debug;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }


        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result && action == ACTION_SHOW)
            {
                WebBrowserForm dlg = new WebBrowserForm();
                dlg.Show();
            }
            return result;
        }
    }

}
