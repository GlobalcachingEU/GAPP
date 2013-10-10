using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Browser
{
    public class BrowserScriptSelectAllGeocaches : BrowserScript
    {
        public const string STR_SELECT = "Select all";

        private Button selectButton = null;

        public BrowserScriptSelectAllGeocaches(WebbrowserForm.BrowserTab browserTab, Utils.BasePlugin.Plugin ownerPlugin, WebBrowser webBrowser, Framework.Interfaces.ICore core)
            : base(browserTab, ownerPlugin, "Select all geocaches", webBrowser, core, true)
        {
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SELECT));
            webBrowser.Navigating += new WebBrowserNavigatingEventHandler(webBrowser_Navigating);
            webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser_DocumentCompleted);
        }

        void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (selectButton != null)
            {
                selectButton.Enabled = GetAllGCCodes().Count>0;
            }
        }

        public override void Rework()
        {
            webBrowser_DocumentCompleted(null, null);
        }

        void webBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (selectButton != null)
            {
                selectButton.Enabled = false;
            }
        }

        public override void CreateControls(Control.ControlCollection collection)
        {
            selectButton = new Button();
            selectButton.Width = 150;
            selectButton.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECT);
            selectButton.Enabled = false;
            selectButton.Click += new EventHandler(importButton_Click);

            collection.Add(selectButton);
        }

        void importButton_Click(object sender, EventArgs e)
        {
            List<string> gcCodes = GetAllGCCodes();
            var lst = from s in gcCodes
                      join Framework.Data.Geocache g in Core.Geocaches on s equals g.Code
                      select g;
            foreach (Framework.Data.Geocache c in lst)
            {
                c.Selected = true;
            }
        }

        public override void LanguageChanged()
        {
            if (selectButton != null)
            {
                selectButton.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECT);
            }
        }
    }
}
