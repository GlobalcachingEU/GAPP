using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Browser
{
    public class BrowserScriptImportAllGeocaches : BrowserScript
    {
        public const string STR_IMPORT_ALL = "Import all";
        public const string STR_IMPORT_MISSING = "Import missing";

        private Button importButtonAll = null;
        private Label importLabelAll = null;
        private Button importButtonMissing = null;
        private Label importLabelMissing = null;

        public BrowserScriptImportAllGeocaches(WebbrowserForm.BrowserTab browserTab, Utils.BasePlugin.Plugin ownerPlugin, WebBrowser webBrowser, Framework.Interfaces.ICore core)
            : base(browserTab, ownerPlugin, "Import geocaches", webBrowser, core, true)
        {
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORT_ALL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORT_MISSING));
            webBrowser.Navigating += new WebBrowserNavigatingEventHandler(webBrowser_Navigating);
            webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser_DocumentCompleted);
        }

        void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            List<string> gcList = GetAllGCCodes();
            if (importButtonAll != null)
            {
                importButtonAll.Enabled = gcList.Count>0;
                importLabelAll.Text = string.Format("({0})", gcList.Count);

                List<string> gcCodes = GetAllGCCodes();
                List<string> gcCodesMissing = new List<string>();
                foreach (string c in gcCodes)
                {
                    if (Utils.DataAccess.GetGeocache(Core.Geocaches, c) == null)
                    {
                        gcCodesMissing.Add(c);
                    }
                }
                importButtonMissing.Enabled = gcCodesMissing.Count>0;
                importLabelMissing.Text = string.Format("({0})", gcCodesMissing.Count);

            }
        }

        public override void Rework()
        {
            webBrowser_DocumentCompleted(null, null);
        }

        void webBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (importButtonAll != null)
            {
                importButtonAll.Enabled = false;
                importLabelAll.Text = "(-)";
                importButtonMissing.Enabled = false;
                importLabelMissing.Text = "(-)";
            }
        }

        public override void CreateControls(Control.ControlCollection collection)
        {
            Panel p = new Panel();
            p.Width = 240;
            p.Height = 50;

            importButtonAll = new Button();
            importButtonAll.Width = 190;
            importButtonAll.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IMPORT_ALL);
            importButtonAll.Enabled = false;
            importButtonAll.Click += new EventHandler(importButton_Click);
            p.Controls.Add(importButtonAll);

            importLabelAll = new Label();
            importLabelAll.Location = new System.Drawing.Point(importButtonAll.Width + 5, importButtonAll.Top);
            importLabelAll.Text = "(-)";
            p.Controls.Add(importLabelAll);

            importButtonMissing = new Button();
            importButtonMissing.Location = new System.Drawing.Point(importButtonAll.Left, importButtonAll.Bottom + 5);
            importButtonMissing.Width = 190;
            importButtonMissing.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IMPORT_MISSING);
            importButtonMissing.Enabled = false;
            importButtonMissing.Click += new EventHandler(importButtonMissing_Click);
            p.Controls.Add(importButtonMissing);

            importLabelMissing = new Label();
            importLabelMissing.Location = new System.Drawing.Point(importButtonMissing.Width + 5, importButtonMissing.Top);
            importLabelMissing.Text = "(-)";
            p.Controls.Add(importLabelMissing);

            collection.Add(p);
        }

        void importButton_Click(object sender, EventArgs e)
        {
            ImportGeocaches(GetAllGCCodes());
        }
        void importButtonMissing_Click(object sender, EventArgs e)
        {
            List<string> gcCodes = GetAllGCCodes();
            List<string> gcCodesMissing = new List<string>();
            foreach (string c in gcCodes)
            {
                if (Utils.DataAccess.GetGeocache(Core.Geocaches, c) == null)
                {
                    gcCodesMissing.Add(c);
                }
            }
            ImportGeocaches(gcCodesMissing);
        }

        public override void LanguageChanged()
        {
            if (importButtonAll != null)
            {
                importButtonAll.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IMPORT_ALL);
                importButtonMissing.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IMPORT_MISSING);
            }
        }
    }
}
