using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Browser
{
    public class BrowserScriptRework:BrowserScript
    {
        public const string STR_REWORK = "Force Rework";

        private Button reworkButton = null;

        public BrowserScriptRework(WebbrowserForm.BrowserTab browserTab, Utils.BasePlugin.Plugin ownerPlugin, WebBrowser webBrowser, Framework.Interfaces.ICore core)
            : base(browserTab, ownerPlugin, "Rework", webBrowser, core, true)
        {
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_REWORK));
            webBrowser.Navigating += new WebBrowserNavigatingEventHandler(webBrowser_Navigating);
            webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser_DocumentCompleted);
        }

        void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            reworkButton.Enabled = true;
        }

        void webBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
        }

        public override void CreateControls(Control.ControlCollection collection)
        {
            reworkButton = new Button();
            reworkButton.Width = 150;
            reworkButton.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_REWORK);
            reworkButton.Enabled = false;
            reworkButton.Click += new EventHandler(reworkButton_Click);

            collection.Add(reworkButton);
        }

        void reworkButton_Click(object sender, EventArgs e)
        {
            if (this.BrowserTab != null && this.BrowserTab.Scripts != null)
            {
                foreach (BrowserScript scr in this.BrowserTab.Scripts)
                {
                    try
                    {
                        scr.Rework();
                    }
                    catch
                    {
                    }
                }
            }
        }

    }
}
