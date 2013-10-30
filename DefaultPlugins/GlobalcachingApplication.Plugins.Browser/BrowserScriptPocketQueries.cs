using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Browser
{
    public class BrowserScriptPocketQueries : BrowserScript
    {
        public const string STR_SAVE = "Save settings";
        public const string STR_RESTORE = "Restore settings...";
        public const string STR_CREATE = "Create series...";
        public const string STR_SETTINGSSAVED = "Pocket Query settings has been saved";
        public const string STR_ERROR = "Error";
        public const string STR_DELETE = "Delete _gcpqgen_";

        private Button saveButton = null;
        private Button resoreButton = null;
        private Button createButton = null;
        private Button deleteButton = null;
        private string _settingsFileName = null;
        private string _seriesFileName = null;

        public BrowserScriptPocketQueries(WebbrowserForm.BrowserTab browserTab, Utils.BasePlugin.Plugin ownerPlugin, WebBrowser webBrowser, Framework.Interfaces.ICore core)
            : base(browserTab, ownerPlugin, "Pocket Queries", webBrowser, core, true)
        {
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SAVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_RESTORE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CREATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SETTINGSSAVED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_DELETE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSettingForm.STR_OK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSettingForm.STR_REMOVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSettingForm.STR_SAVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSettingForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_AUTODATERANGES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_AUTOMATIC));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_BELGIUM));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_CANCEL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_CREATEPQ));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_DATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_DELAY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_END));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_FROMSELECTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_ITALY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_MARGIN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_MAXINLASTPQ));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_MAXINPQ));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_NETHERLANDS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_NEW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_REMOVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_RENAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_SAVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_SERIE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_SETTINGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_START));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_TEMPLATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_AUTOMATICACTIVATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesForm.STR_FROMGCPROJECT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesGCProjectForm.STR_DESCRIPTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesGCProjectForm.STR_OK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BrowserScriptPocketQueriesPQSeriesGCProjectForm.STR_TITLE));

            webBrowser.Navigating += new WebBrowserNavigatingEventHandler(webBrowser_Navigating);
            webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser_DocumentCompleted);

            try
            {
                string p = core.PluginDataPath;
                _settingsFileName = System.IO.Path.Combine(new string[] { p, "pqsettings.xml" });
                _seriesFileName = System.IO.Path.Combine(new string[] { p, "pqseries.xml" });
            }
            catch
            {
            }
        }

        public override void LanguageChanged()
        {
            if (saveButton != null)
            {
                saveButton.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SAVE);
                resoreButton.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RESTORE);
                createButton.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CREATE);
                deleteButton.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELETE);
            }
        }

        void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (saveButton != null)
            {
                saveButton.Enabled = !string.IsNullOrEmpty(_settingsFileName) && Browser.Url != null && Browser.Url.OriginalString.ToLower().IndexOf("geocaching.com/pocket/gcquery.aspx") >= 0;
                resoreButton.Enabled = saveButton.Enabled;
                createButton.Enabled = !string.IsNullOrEmpty(_settingsFileName) && Browser.Url != null && Browser.Url.OriginalString.ToLower().IndexOf("geocaching.com/pocket/") >= 0 && Browser.Document != null && Browser.Document.GetElementById("ctl00_ContentBody_PQListControl1_btnScheduleNow") != null;
                deleteButton.Enabled = createButton.Enabled;
            }
        }

        void webBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (saveButton != null)
            {
                saveButton.Enabled = false;
                resoreButton.Enabled = false;
                createButton.Enabled = false;
                deleteButton.Enabled = false;
            }
        }

        public override void CreateControls(Control.ControlCollection collection)
        {
            Panel p = new Panel();
            p.Width = 240;
            p.Height = 110;

            saveButton = new Button();
            saveButton.Width = 190;
            saveButton.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SAVE);
            saveButton.Enabled = false;
            saveButton.Click += new EventHandler(saveButton_Click);
            p.Controls.Add(saveButton);

            resoreButton = new Button();
            resoreButton.Location = new System.Drawing.Point(saveButton.Left, saveButton.Bottom + 5);
            resoreButton.Width = saveButton.Width;
            resoreButton.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RESTORE);
            resoreButton.Enabled = false;
            resoreButton.Click += new EventHandler(resoreButton_Click);
            p.Controls.Add(resoreButton);

            createButton = new Button();
            createButton.Location = new System.Drawing.Point(resoreButton.Left, resoreButton.Bottom + 5);
            createButton.Width = saveButton.Width;
            createButton.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CREATE);
            createButton.Enabled = false;
            createButton.Click += new EventHandler(createButton_Click);
            p.Controls.Add(createButton);

            deleteButton = new Button();
            deleteButton.Location = new System.Drawing.Point(createButton.Left, createButton.Bottom + 5);
            deleteButton.Width = saveButton.Width;
            deleteButton.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELETE);
            deleteButton.Enabled = false;
            deleteButton.Click += new EventHandler(deleteButton_Click);
            p.Controls.Add(deleteButton);

            collection.Add(p);
        }

        void saveButton_Click(object sender, EventArgs e)
        {
            BrowserScriptPocketQueriesPQSetting pq = BrowserScriptPocketQueriesPQSetting.FromPQPage(Core, this.OwnerPlugin, Browser);
            if (pq != null)
            {
                try
                {
                    List<BrowserScriptPocketQueriesPQSetting> l = BrowserScriptPocketQueriesPQSetting.Load(_settingsFileName);
                    BrowserScriptPocketQueriesPQSetting pqs = (from p in l where p.Name == pq.Name select p).FirstOrDefault();
                    if (pqs != null)
                    {
                        pq.ID = pqs.ID;
                        l.Remove(pqs);
                    }
                    l.Add(pq);
                    BrowserScriptPocketQueriesPQSetting.Save(_settingsFileName, l);
                    MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_SETTINGSSAVED));
                }
                catch
                {
                    MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                }
            }
            else
            {
                MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
            }
        }

        void resoreButton_Click(object sender, EventArgs e)
        {
            using (BrowserScriptPocketQueriesPQSettingForm dlg = new BrowserScriptPocketQueriesPQSettingForm(_settingsFileName))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if (dlg.SelectedSetting != null)
                    {
                        if (!dlg.SelectedSetting.SetPQPage(Browser))
                        {
                            MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                        }
                    }
                }
            }
        }

        void createButton_Click(object sender, EventArgs e)
        {
            using (BrowserScriptPocketQueriesPQSeriesForm dlg = new BrowserScriptPocketQueriesPQSeriesForm(Core, this.OwnerPlugin, _settingsFileName, _seriesFileName, Browser))
            {
                dlg.ShowDialog();
            }
        }

        void deleteButton_Click(object sender, EventArgs e)
        {
            try
            {
                bool autoQueriesFound = false;
                //ok, now we need to check the checkboxes where the name starts with _gcpqgen_
                bool usedQuotes = true;
                string docText = Browser.Document.Body.InnerHtml;
                int pos;
                pos = docText.IndexOf("id=\"chk", StringComparison.OrdinalIgnoreCase);
                if (pos < 0)
                {
                    pos = docText.IndexOf("id=chk", StringComparison.OrdinalIgnoreCase);
                    usedQuotes = false;
                }
                while (pos > 0)
                {
                    int pos2;
                    string elementId;

                    if (usedQuotes)
                    {
                        pos = docText.IndexOf("\"", pos, StringComparison.OrdinalIgnoreCase)+1;
                        pos2 = docText.IndexOf("\"", pos, StringComparison.OrdinalIgnoreCase);
                        elementId = docText.Substring(pos, pos2 - pos);
                    }
                    else
                    {
                        pos += 3;
                        pos2 = docText.IndexOf(" ", pos + 1, StringComparison.OrdinalIgnoreCase);
                        elementId = docText.Substring(pos, pos2 - pos);
                    }

                    //now get the name of the PQ
                    //first look for "(500)" ALERT! tricky!
                    int namePos;
                    int namePos1 = docText.IndexOf("(500)", pos, StringComparison.OrdinalIgnoreCase);
                    int namePos2 = docText.IndexOf("(1000)", pos, StringComparison.OrdinalIgnoreCase);
                    if (namePos1 > 0 && (namePos2 < 0 || namePos1 < namePos2))
                    {
                        namePos = namePos1;
                    }
                    else
                    {
                        namePos = namePos2;
                    }

                    pos = docText.IndexOf("id=\"chk", pos2, StringComparison.OrdinalIgnoreCase);
                    if (pos < 0)
                    {
                        pos = docText.IndexOf("id=chk", pos2, StringComparison.OrdinalIgnoreCase);
                        usedQuotes = false;
                    }
                    else
                    {
                        usedQuotes = true;
                    }

                    if (namePos > 0 && (namePos < pos || pos < 0))
                    {
                        //this means that the (500) belongs to current PQ
                        //now look for the name;
                        namePos = docText.IndexOf(">", namePos, StringComparison.OrdinalIgnoreCase);
                        pos2 = docText.IndexOf("<", namePos, StringComparison.OrdinalIgnoreCase);
                        string theName = docText.Substring(namePos + 1, pos2 - namePos - 1).Trim();

                        if (theName.IndexOf("_gcpqgen_") >= 0)
                        {
                            autoQueriesFound = true;
                            Browser.Document.GetElementById(elementId).InvokeMember("Click");
                        }
                    }
                }
                if (autoQueriesFound)
                {
                    pos = docText.IndexOf("lnkDeleteSelected", StringComparison.OrdinalIgnoreCase);
                    string elId = string.Format("ctl00_ContentBody_PQListControl1_PQRepeater_ctl{0}_lnkDeleteSelected", docText.Substring(pos - 3, 2));
                    HtmlElement he = Browser.Document.GetElementById(elId);
                    he.InvokeMember("Click");
                }
            }
            catch
            {
            }
        }

    }
}
