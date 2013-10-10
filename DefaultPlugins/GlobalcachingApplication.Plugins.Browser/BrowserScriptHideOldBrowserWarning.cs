using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Browser
{
    public class BrowserScriptHideOldBrowserWarning : BrowserScript
    {
        public BrowserScriptHideOldBrowserWarning(WebbrowserForm.BrowserTab browserTab, Utils.BasePlugin.Plugin ownerPlugin, WebBrowser webBrowser, Framework.Interfaces.ICore core)
            : base(browserTab, ownerPlugin, "Hide old browser warning", webBrowser, core, false)
        {
            webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser_DocumentCompleted);
        }

        void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            HtmlElement el = SearchDiv(Browser.Document.Body);
            if (el != null)
            {
                el.Style = "visibility:hidden;display:none";
            }
        }

        private HtmlElement SearchDiv(HtmlElement el)
        {
            if (el == null)
            {
                return null;
            }
            if (el.GetAttribute("className") == "WarningMessage PhaseOut")
            {
                return el;
            }
            HtmlElementCollection elc = el.GetElementsByTagName("div");
            if (elc != null)
            {
                foreach (HtmlElement e in elc)
                {
                    HtmlElement r = SearchDiv(e);
                    if (r != null)
                    {
                        return r;
                    }
                }
            }
            return null;
        }

    }
}
