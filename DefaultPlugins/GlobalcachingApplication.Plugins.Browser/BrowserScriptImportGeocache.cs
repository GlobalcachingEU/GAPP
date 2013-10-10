using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Browser
{
    public class BrowserScriptImportGeocache : BrowserScript
    {
        public const string STR_IMPORT = "Import";

        private Button importButton = null;

        public BrowserScriptImportGeocache(WebbrowserForm.BrowserTab browserTab, Utils.BasePlugin.Plugin ownerPlugin, WebBrowser webBrowser, Framework.Interfaces.ICore core)
            : base(browserTab, ownerPlugin, "Import geocache", webBrowser, core, true)
        {
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORT));
            webBrowser.Navigating += new WebBrowserNavigatingEventHandler(webBrowser_Navigating);
            webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser_DocumentCompleted);
        }

        void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (importButton != null)
            {
                importButton.Enabled = Browser.Url!=null && Browser.Url.OriginalString.ToLower().IndexOf("www.geocaching.com/seek/cache_details.aspx") >= 0;
            }
        }

        void webBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (importButton != null)
            {
                importButton.Enabled = false;
            }
        }

        public override void CreateControls(Control.ControlCollection collection)
        {
            importButton = new Button();
            importButton.Width = 150;
            importButton.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IMPORT);
            importButton.Enabled = false;
            importButton.Click += new EventHandler(importButton_Click);

            collection.Add(importButton);
        }

        void importButton_Click(object sender, EventArgs e)
        {
            //<span id="ctl00_ContentBody_CoordInfoLinkControl1_uxCoordInfoCode" class="CoordInfoCode">GC2FE26</span>
            try
            {
                string gcCode = null;
                HtmlElement el = Browser.Document.GetElementById("ctl00_ContentBody_CoordInfoLinkControl1_uxCoordInfoCode");
                if (el != null)
                {
                    gcCode = el.InnerText.Trim();
                }
                else
                {
                    //PM? alternative in meta data
                    //<meta name="og:url" content="http://www.geocaching.com/seek/cache_details.aspx?wp=GC3YTQC" property="og:url" />
                    el = Browser.Document.GetElementById("og:url");
                    if (el != null)
                    {
                        string url = el.GetAttribute("content");
                        string[] parts = url.Split(new char[] { '=' });
                        if (parts.Length == 2)
                        {
                            gcCode = parts[1];
                        }
                    }
                }
                if (!string.IsNullOrEmpty(gcCode) && gcCode.StartsWith("GC"))
                {
                    List<string> gcList = new List<string>();
                    gcList.Add(gcCode);
                    ImportGeocaches(gcList);
                }
            }
            catch
            {
            }
        }

        public override void LanguageChanged()
        {
            if (importButton != null)
            {
                importButton.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IMPORT);
            }
        }

    }
}
