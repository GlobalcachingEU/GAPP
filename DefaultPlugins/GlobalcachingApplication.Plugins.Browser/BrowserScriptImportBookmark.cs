using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Browser
{
    public class BrowserScriptImportBookmark : BrowserScript
    {
        public const string STR_IMPORT = "Import";

        private Button importButton = null;
        private Utils.BasePlugin.Plugin _bookmarkImportPlugin = null;

        public BrowserScriptImportBookmark(WebbrowserForm.BrowserTab browserTab, Utils.BasePlugin.Plugin ownerPlugin, WebBrowser webBrowser, Framework.Interfaces.ICore core)
            : base(browserTab, ownerPlugin, "Import bookmark", webBrowser, core, true)
        {
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORT));
            webBrowser.Navigating += new WebBrowserNavigatingEventHandler(webBrowser_Navigating);
            webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser_DocumentCompleted);

            _bookmarkImportPlugin = Utils.PluginSupport.PluginByName(core, "GlobalcachingApplication.Plugins.APIBookmark.Import") as Utils.BasePlugin.Plugin;
        }

        void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (importButton != null)
            {
                importButton.Enabled = Browser.Url != null && Browser.Url.OriginalString.ToLower().IndexOf("www.geocaching.com/bookmarks/view.aspx") >= 0;
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
            try
            {
                string name = null;
                HtmlElement el = Browser.Document.GetElementById("ctl00_ContentBody_lbHeading");
                if (el != null && Browser.Url!=null)
                {
                    name = el.InnerText.Trim();
                    string url = Browser.Url.OriginalString;
                    var m = _bookmarkImportPlugin.GetType().GetMethod("ImportBookmark");
                    m.Invoke(_bookmarkImportPlugin, new object[] {name, url });
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
