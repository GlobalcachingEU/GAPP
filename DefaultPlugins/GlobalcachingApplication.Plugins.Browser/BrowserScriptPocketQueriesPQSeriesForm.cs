using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Xml;
using System.Threading;

namespace GlobalcachingApplication.Plugins.Browser
{
    public partial class BrowserScriptPocketQueriesPQSeriesForm : Form
    {
        public const string STR_TITLE = "Create pocket query series";
        public const string STR_CANCEL = "Cancel";
        public const string STR_CREATEPQ = "Create Pocket Queries";
        public const string STR_ERROR = "Error";
        public const string STR_SERIE = "Serie";
        public const string STR_NAME = "Name";
        public const string STR_SETTINGS = "Settings";
        public const string STR_TEMPLATE = "Template";
        public const string STR_START = "Start";
        public const string STR_END = "End";
        public const string STR_DATE = "Date";
        public const string STR_REMOVE = "Remove";
        public const string STR_RENAME = "Rename";
        public const string STR_SAVE = "Save";
        public const string STR_NEW = "New";
        public const string STR_AUTODATERANGES = "Automatic date ranges";
        public const string STR_MAXINPQ = "Max. in PQ";
        public const string STR_MAXINLASTPQ = "Max. in last PQ";
        public const string STR_FROMSELECTION = "From selection";
        public const string STR_NETHERLANDS = "Netherlands";
        public const string STR_BELGIUM = "Belgium";
        public const string STR_ITALY = "Italy";
        public const string STR_DELAY = "Delay (ms)";
        public const string STR_MARGIN = "Margin";
        public const string STR_AUTOMATIC = "Automatic";
        public const string STR_AUTOMATICACTIVATE = "Automatically activate";
        public const string STR_FROMGCPROJECT = "From Project-GC...";

        private string _settingsFilename = "";
        private string _seriesFilename = "";
        private bool _init = false;
        private Framework.Interfaces.ICore _core = null;
        private WebBrowser _wb = null;
        private bool _cancel = false;
        private ManualResetEvent _browserReady = null;
        private Framework.Interfaces.IPlugin _plugin = null;

        public BrowserScriptPocketQueriesPQSeriesForm()
        {
            InitializeComponent();
        }

        public BrowserScriptPocketQueriesPQSeriesForm(Framework.Interfaces.ICore core, Framework.Interfaces.IPlugin plugin, string settingsFilename, string seriesFilename, WebBrowser wb)
            : this()
        {
            _settingsFilename = settingsFilename;
            _seriesFilename = seriesFilename;
            _core = core;
            _wb = wb;
            _plugin = plugin;

            List<BrowserScriptPocketQueriesPQSetting> s = BrowserScriptPocketQueriesPQSetting.Load(settingsFilename);
            if (s != null)
            {
                comboBox2.Items.AddRange(s.ToArray());
            }

            List<BrowserScriptPocketQueriesPQSeries> l = BrowserScriptPocketQueriesPQSeries.Load(_seriesFilename);
            if (l != null)
            {
                comboBox1.Items.AddRange(l.ToArray());
            }

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.button5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CREATEPQ);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SERIE);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SETTINGS);
            this.label12.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TEMPLATE);
            this.label8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_START);
            this.label10.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_END);
            this.listView1.Columns[0].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DATE);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_REMOVE);
            this.button12.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RENAME);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SAVE);
            this.button4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NEW);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AUTODATERANGES);
            this.label14.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAXINPQ);
            this.label16.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAXINLASTPQ);
            this.button7.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FROMSELECTION);
            this.button8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NETHERLANDS);
            this.button9.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_BELGIUM);
            this.button10.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ITALY);
            this.label21.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELAY);
            this.label19.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MARGIN);
            this.button11.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AUTOMATIC);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AUTOMATICACTIVATE);
            this.button13.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FROMGCPROJECT);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex < 0)
            {
                button1.Enabled = false;
                textBox1.Text = "";
                textBox2.Text = "";
                textBox2.Enabled = false;
                comboBox2.SelectedItem = null;
                comboBox2.Enabled = false;
                dateTimePicker1.Enabled = false;
                dateTimePicker2.Enabled = false;
                dateTimePicker3.Enabled = false;
                listView1.Items.Clear();
                listView1.Enabled = false;
                button3.Enabled = false;
                groupBox1.Enabled = false;
                button5.Enabled = false;
                button11.Enabled = false;
            }
            else
            {
                _init = true;
                BrowserScriptPocketQueriesPQSeries pqs = comboBox1.SelectedItem as BrowserScriptPocketQueriesPQSeries;
                button1.Enabled = true;
                textBox1.Text = pqs.Name;
                textBox2.Text = pqs.NameTemplate;
                textBox2.Enabled = true;
                comboBox2.Enabled = true;
                comboBox2.SelectedItem = (from BrowserScriptPocketQueriesPQSetting p in comboBox2.Items where p.ID == pqs.SettingsID select p).FirstOrDefault();
                if (comboBox2.SelectedItem == null)
                {
                    pqs.SettingsID = "";
                }
                dateTimePicker1.Enabled = true;
                dateTimePicker2.Enabled = true;
                dateTimePicker3.Enabled = true;
                listView1.Enabled = true;
                if (pqs.Dates.Count > 1)
                {
                    dateTimePicker1.Value = pqs.Dates.Min();
                    dateTimePicker2.Value = pqs.Dates.Max();
                    listView1.Items.Clear();
                    pqs.Dates.Sort();
                    for (int i = 1; i < pqs.Dates.Count - 1; i++)
                    {
                        ListViewItem lvi = new ListViewItem(pqs.Dates[i].ToString("d"));
                        lvi.Tag = pqs.Dates[i];
                        listView1.Items.Add(lvi);
                    }
                }
                button3.Enabled = true;
                groupBox1.Enabled = true;
                checkReadyToCreate();
                checkReadyToGetDates();
                _init = false;
            }
        }

        private void checkReadyToCreate()
        {
            BrowserScriptPocketQueriesPQSeries pqs = comboBox1.SelectedItem as BrowserScriptPocketQueriesPQSeries;
            button5.Enabled = (pqs!=null &&
                pqs.Dates.Count>1 &&
                !string.IsNullOrEmpty(pqs.SettingsID) &&
                pqs.NameTemplate.Trim().Length>0);
        }

        private void checkReadyToGetDates()
        {
            BrowserScriptPocketQueriesPQSeries pqs = comboBox1.SelectedItem as BrowserScriptPocketQueriesPQSeries;
            button11.Enabled = (pqs != null &&
                !string.IsNullOrEmpty(pqs.SettingsID) &&
                pqs.Dates.Count > 1);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button6.Enabled = listView1.SelectedIndices.Count > 0;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button4.Enabled = textBox1.Text.Trim().Length > 0;
            button12.Enabled = comboBox1.SelectedItem!=null && textBox1.Text.Trim().Length > 0 && (from BrowserScriptPocketQueriesPQSeries p in comboBox1.Items where p.Name == textBox1.Text select p).FirstOrDefault() == null;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            BrowserScriptPocketQueriesPQSeries pqs = comboBox1.SelectedItem as BrowserScriptPocketQueriesPQSeries;
            if (pqs != null)
            {
                BrowserScriptPocketQueriesPQSetting s = comboBox2.SelectedItem as BrowserScriptPocketQueriesPQSetting;
                if (s != null)
                {
                    pqs.SettingsID = s.ID;
                }
                else
                {
                    pqs.SettingsID = "";
                }
            }
            checkReadyToCreate();
            checkReadyToGetDates();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            BrowserScriptPocketQueriesPQSeries pqs = comboBox1.SelectedItem as BrowserScriptPocketQueriesPQSeries;
            if (pqs != null)
            {
                pqs.NameTemplate = textBox2.Text;
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if (!_init)
            {
                BrowserScriptPocketQueriesPQSeries pqs = comboBox1.SelectedItem as BrowserScriptPocketQueriesPQSeries;
                if (pqs != null)
                {
                    if (pqs.Dates.Count > 1)
                    {
                        //select the min date that is not in the list
                        DateTime dtl = DateTime.MaxValue;
                        int index = -1;
                        int i = 0;
                        foreach (DateTime dt in pqs.Dates)
                        {
                            bool inList = false;
                            foreach (ListViewItem lvi in listView1.Items)
                            {
                                if ((DateTime)lvi.Tag == dt)
                                {
                                    inList = true;
                                    break;
                                }
                            }
                            if (!inList)
                            {
                                if (dtl > dt)
                                {
                                    dtl = dt;
                                    index = i;
                                }
                            }
                            i++;
                        }
                        if (index >= 0)
                        {
                            pqs.Dates[index] = dateTimePicker1.Value;
                        }
                    }
                }
            }
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            if (!_init)
            {
                BrowserScriptPocketQueriesPQSeries pqs = comboBox1.SelectedItem as BrowserScriptPocketQueriesPQSeries;
                if (pqs != null)
                {
                    if (pqs.Dates.Count > 1)
                    {
                        //select the min date that is not in the list
                        DateTime dtl = DateTime.MinValue;
                        int index = -1;
                        int i = 0;
                        foreach (DateTime dt in pqs.Dates)
                        {
                            bool inList = false;
                            foreach (ListViewItem lvi in listView1.Items)
                            {
                                if ((DateTime)lvi.Tag == dt)
                                {
                                    inList = true;
                                    break;
                                }
                            }
                            if (!inList)
                            {
                                if (dtl < dt)
                                {
                                    dtl = dt;
                                    index = i;
                                }
                            }
                            i++;
                        }
                        if (index >= 0)
                        {
                            pqs.Dates[index] = dateTimePicker2.Value;
                        }
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            BrowserScriptPocketQueriesPQSeries pqs = comboBox1.SelectedItem as BrowserScriptPocketQueriesPQSeries;
            if (pqs != null)
            {
                pqs.Dates.Add(dateTimePicker3.Value);
                ListViewItem lvi = new ListViewItem(dateTimePicker3.Value.ToString("d"));
                lvi.Tag = dateTimePicker3.Value;
                listView1.Items.Add(lvi);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                BrowserScriptPocketQueriesPQSeries pqs = comboBox1.SelectedItem as BrowserScriptPocketQueriesPQSeries;
                if (pqs != null)
                {
                    DateTime dt = (DateTime)listView1.SelectedItems[0].Tag;
                    for (int i = 0; i < pqs.Dates.Count; i++)
                    {
                        if (pqs.Dates[i] == dt)
                        {
                            pqs.Dates.RemoveAt(i);
                            break;
                        }
                    }
                    listView1.Items.RemoveAt(listView1.SelectedIndices[0]);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            BrowserScriptPocketQueriesPQSeries pqs = comboBox1.SelectedItem as BrowserScriptPocketQueriesPQSeries;
            if (pqs != null)
            {
                comboBox1.Items.Remove(pqs);
            }
            comboBox1_SelectedIndexChanged(this, EventArgs.Empty);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            BrowserScriptPocketQueriesPQSeries pqs = comboBox1.SelectedItem as BrowserScriptPocketQueriesPQSeries;
            if (pqs != null)
            {
                pqs.Name = textBox1.Text;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BrowserScriptPocketQueriesPQSeries.Save(_seriesFilename, (from BrowserScriptPocketQueriesPQSeries p in comboBox1.Items select p).ToList());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            BrowserScriptPocketQueriesPQSeries pqs = new BrowserScriptPocketQueriesPQSeries();
            pqs.ID = Guid.NewGuid().ToString("N");
            pqs.Name = textBox1.Text;
            pqs.NameTemplate = "_gcpqgen_<index>_DL_XX_<begindate>_<enddate>";
            pqs.SettingsID = "";
            pqs.Dates.Add(new DateTime(2001, 1, 1));
            pqs.Dates.Add(DateTime.Now.AddMonths(6));
            comboBox1.Items.Add(pqs);
            comboBox1.SelectedItem = pqs;
        }

        private void addFromGlobalcaching(string page)
        {
            BrowserScriptPocketQueriesPQSeries pqs = comboBox1.SelectedItem as BrowserScriptPocketQueriesPQSeries;
            if (pqs != null)
            {
                Cursor = Cursors.WaitCursor;

                _init = true;
                pqs.Dates.Clear();
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        string xmldoc = wc.DownloadString(string.Format("http://www.globalcaching.eu/gcpqgen/{2}.aspx?max={0}&mlpq={1}", (int)numericUpDown1.Value, (int)numericUpDown2.Value, page));
                        if (xmldoc != null)
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(xmldoc);
                            XmlElement root = doc.DocumentElement;

                            XmlNodeList bmNodes = root.SelectNodes("Set");
                            if (bmNodes != null)
                            {
                                foreach (XmlNode n in bmNodes)
                                {
                                    pqs.Dates.Add(DateTime.ParseExact(n.Attributes["FromDate"].Value, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
                                }
                                if (bmNodes.Count > 0)
                                {
                                    pqs.Dates.Add(DateTime.ParseExact(bmNodes[bmNodes.Count - 1].Attributes["ToDate"].Value, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
                if (pqs.Dates.Count == 0)
                {
                    pqs.Dates.Add(new DateTime(2001, 1, 1));
                    pqs.Dates.Add(DateTime.Now.AddMonths(6));
                }
                _init = false;
                comboBox1_SelectedIndexChanged(this, EventArgs.Empty);
                Cursor = Cursors.Default;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            addFromGlobalcaching("pqset");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            addFromGlobalcaching("pqsetbe");
        }

        private void button10_Click(object sender, EventArgs e)
        {
            addFromGlobalcaching("pqsetit");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            BrowserScriptPocketQueriesPQSeries pqs = comboBox1.SelectedItem as BrowserScriptPocketQueriesPQSeries;
            if (pqs != null)
            {
                Cursor = Cursors.WaitCursor;
                _init = true;
                pqs.Dates.Clear();
                pqs.Dates.Add(new DateTime(2001, 1, 1));
                pqs.Dates.Add(DateTime.Now.AddMonths(6));

                int maxInPq = (int)numericUpDown1.Value;
                int maxInLastPq = (int)numericUpDown2.Value;

                int inPq = 0;
                DateTime prevDate = DateTime.MinValue;
                DateTime maxLastPqDate = DateTime.MinValue;
                int numInPrevDate = 0;
                List<Framework.Data.Geocache> gcList = (from Framework.Data.Geocache g in _core.Geocaches where g.Selected orderby g.PublishedTime select g).ToList();
                foreach (Framework.Data.Geocache g in gcList)
                {
                    if (prevDate == g.PublishedTime)
                    {
                        numInPrevDate++;
                    }
                    else
                    {
                        prevDate = g.PublishedTime;
                        numInPrevDate = 1;
                    }
                    inPq++;
                    if (inPq > maxInPq)
                    {
                        pqs.Dates.Add(g.PublishedTime.AddDays(-1));
                        inPq = numInPrevDate;
                        maxLastPqDate = DateTime.MinValue;
                    }
                    if (inPq > maxInLastPq && maxLastPqDate == DateTime.MinValue)
                    {
                        maxLastPqDate = prevDate;
                    }
                }
                //the last pq
                if (inPq > maxInLastPq && maxLastPqDate != DateTime.MinValue)
                {
                    pqs.Dates.Add(maxLastPqDate);
                }

                _init = false;
                comboBox1_SelectedIndexChanged(this, EventArgs.Empty);
                Cursor = Cursors.Default;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (panel1.Enabled)
            {
                string message = "";
                string inArea = "";
                string msgid = "";
                _cancel = false;
                BrowserScriptPocketQueriesPQSeries pqs = comboBox1.SelectedItem as BrowserScriptPocketQueriesPQSeries;
                BrowserScriptPocketQueriesPQSetting setting = comboBox2.SelectedItem as BrowserScriptPocketQueriesPQSetting;
                if (pqs != null && pqs.Dates.Count > 1 && setting!=null)
                {
                    this.button5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CANCEL);
                    panel1.Enabled = false;
                    this.ControlBox = false;
                    _browserReady = new ManualResetEvent(false);
                    _wb.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(_wb_DocumentCompleted);

                    try
                    {
                        string baseUrl = string.Format("http://{0}", _wb.Url.Host);
                        pqs.Dates.Sort();
                        if (setToEnglish())
                        {
                            int pqIndex = 0;
                            int retryCount = 0;
                            int activationDay = 0;
                            while (pqIndex < pqs.Dates.Count - 1)
                            {
                                message = "";
                                msgid = "";
                                try
                                {
                                    inArea = "1";
                                    if (gotoPage(string.Format("{0}/pocket/default.aspx", baseUrl)))
                                    {
                                        inArea = "2";
                                        _wb.Document.GetElementById("uxCreateNewPQ").SetAttribute("Target", "_self");
                                        _browserReady.Reset();
                                        _wb.Document.GetElementById("uxCreateNewPQ").InvokeMember("Click");
                                        if (waitForPageReady(string.Format("{0}/pocket/gcquery.aspx", baseUrl)))
                                        {
                                            inArea = "3";
                                            if (pqIndex == 0)
                                            {
                                                setting.PlacedDuringFromDate = pqs.Dates[pqIndex];
                                            }
                                            else
                                            {
                                                setting.PlacedDuringFromDate = pqs.Dates[pqIndex].AddDays(1);
                                            }
                                            setting.PlacedDuringToDate = pqs.Dates[pqIndex + 1];
                                            setting.PlacedDuring = "ctl00_ContentBody_rbPlacedBetween";
                                            setting.Name = pqs.NameTemplate.Replace("<index>", pqIndex.ToString("00")).Replace("<begindate>", setting.PlacedDuringFromDate.ToString("yyyyMMdd")).Replace("<enddate>", setting.PlacedDuringToDate.ToString("yyyyMMdd"));
                                            inArea = "4";
                                            if (checkBox1.Checked)
                                            {
                                                setting.SetPQPage(_wb, activationDay);
                                            }
                                            else
                                            {
                                                setting.SetPQPage(_wb);
                                            }
                                            inArea = "5";
                                            _browserReady.Reset();
                                            _wb.Document.GetElementById("ctl00_ContentBody_btnSubmit").InvokeMember("Click");
                                            inArea = "6";
                                            if (waitForPageReady(string.Format("{0}/pocket/gcquery.aspx", baseUrl)))
                                            {
                                                inArea = "7";
                                                if (!_wb.Document.Body.InnerHtml.Contains("Thanks! Your pocket query has been saved and currently results in"))
                                                {
                                                    message = STR_ERROR;
                                                    msgid = "1";
                                                }
                                            }
                                            else
                                            {
                                                message = STR_ERROR;
                                                msgid = "2";
                                            }
                                        }
                                        else
                                        {
                                            message = STR_ERROR;
                                            msgid = "3";
                                        }
                                        if (_cancel)
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        msgid = "4";
                                        message = STR_ERROR;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    msgid = string.Format("6i - {0}", inArea);
                                    message = STR_ERROR;
                                }
                                if (!string.IsNullOrEmpty(message))
                                {
                                    if (retryCount < 6)
                                    {
                                        _core.DebugLog(Framework.Data.DebugLogLevel.Error, _plugin, null, string.Format("ERROR: Setting PQ settings - retry ({0})", retryCount));
                                        //check if PQ has been created:
                                        if (gotoPage(string.Format("{0}/pocket/default.aspx", baseUrl)))
                                        {
                                            if (_wb.Document.Body.InnerHtml.IndexOf(setting.Name) > 0)
                                            {
                                                _core.DebugLog(Framework.Data.DebugLogLevel.Error, _plugin, null, "But was created");
                                                //created, goto next
                                                retryCount = 0;
                                                pqIndex++;

                                                activationDay++;
                                                if (activationDay > 6)
                                                {
                                                    activationDay = 0;
                                                }
                                            }
                                            else
                                            {
                                                _core.DebugLog(Framework.Data.DebugLogLevel.Error, _plugin, null, "Was not created");
                                                retryCount++;
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    activationDay++;
                                    if (activationDay > 6)
                                    {
                                        activationDay = 0;
                                    }

                                    retryCount = 0;
                                    pqIndex++;
                                }
                            }
                        }
                        else
                        {
                            msgid = "5";
                            message = STR_ERROR;
                        }
                    }
                    catch (Exception ex)
                    {
                        msgid = string.Format("6 - {0}", inArea);
                        message = STR_ERROR;
                    }

                    _wb.DocumentCompleted -= new WebBrowserDocumentCompletedEventHandler(_wb_DocumentCompleted);
                    _browserReady.Dispose();
                    _browserReady = null;

                    this.ControlBox = true;
                    panel1.Enabled = true;
                    this.button5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CREATEPQ);
                    if (!string.IsNullOrEmpty(message))
                    {
                        MessageBox.Show(string.Format("{0} ({1} - {2})", Utils.LanguageSupport.Instance.GetTranslation(message), msgid, inArea), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                    }
                }
            }
            else
            {
                _cancel = true;
            }
        }

        void _wb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (_wb.Document != null && _wb.ReadyState == WebBrowserReadyState.Complete)
            {
                if (_browserReady != null)
                {
                    _browserReady.Set();
                }
            }
        }

        private bool setToEnglish()
        {
            bool result = false;
            try
            {
                _browserReady.Reset();
                _wb.Document.GetElementById("ctl00_uxLocaleListTop_uxLocaleList_ctl00_uxLocaleItem").InvokeMember("Click");
                result = waitForPageReady();
            }
            catch
            {
            }
            return result;
        }

        private bool gotoPage(string url)
        {
            bool result = false;
            if (_wb.Url.OriginalString != url)
            {
                if (_wb.ReadyState != WebBrowserReadyState.Complete)
                {
                    _wb.Stop();
                }
                _browserReady.Reset();
                _wb.Navigate(url);
                result = waitForPageReady(url);
            }
            else
            {
                return true;
            }
            return result;
        }

        private bool waitForPageReady()
        {
            return waitForPageReady(null);
        }
        private bool waitForPageReady(string url)
        {
            bool result = false;
            int i = 600;
            while (!_browserReady.WaitOne(100, true) && i > 0 && !_cancel)
            {
                Application.DoEvents();
                i--;
            }
            if (i > 0)
            {
                if (!_cancel)
                {
                    if ((_wb.Document!=null && _wb.Document.Body != null) && string.IsNullOrEmpty(url) || _wb.Url.ToString() == url)
                    {
                        result = true;
                    }
                    else
                    {
                        _browserReady.Reset();
                        result = waitForPageReady(url);
                    }
                }
            }
            else
            {
                _wb.Stop();
            }
            return result;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (panel1.Enabled)
            {
                string message = "";
                _cancel = false;
                BrowserScriptPocketQueriesPQSeries pqs = comboBox1.SelectedItem as BrowserScriptPocketQueriesPQSeries;
                BrowserScriptPocketQueriesPQSetting setting = comboBox2.SelectedItem as BrowserScriptPocketQueriesPQSetting;
                if (pqs != null && setting != null)
                {
                    _init = true;
                    listView1.Items.Clear();
                    pqs.Dates.Clear();
                    pqs.Dates.Add(dateTimePicker1.Value < dateTimePicker2.Value ? dateTimePicker1.Value : dateTimePicker2.Value);
                    pqs.Dates.Add(dateTimePicker2.Value > dateTimePicker1.Value ? dateTimePicker2.Value : dateTimePicker1.Value);

                    this.button5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CANCEL);
                    panel1.Enabled = false;
                    this.ControlBox = false;
                    _browserReady = new ManualResetEvent(false);
                    _wb.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(_wb_DocumentCompleted);

                    try
                    {
                        string baseUrl = string.Format("http://{0}", _wb.Url.Host);

                        if (setToEnglish())
                        {
                            if (gotoPage(string.Format("{0}/pocket/default.aspx", baseUrl)))
                            {
                                _wb.Document.GetElementById("uxCreateNewPQ").SetAttribute("Target", "_self");
                                _browserReady.Reset();
                                _wb.Document.GetElementById("uxCreateNewPQ").InvokeMember("Click");
                                if (waitForPageReady(string.Format("{0}/pocket/gcquery.aspx", baseUrl)))
                                {
                                    //now, use this page to get the ranges
                                    int maxInPq = (int)numericUpDown1.Value;
                                    int maxInLastPq = (int)numericUpDown2.Value;
                                    int margin = (int)numericUpDown3.Value;
                                    DateTime lastPqEnd = pqs.Dates[1];
                                    DateTime minOfInterval = pqs.Dates[0];
                                    DateTime maxOfInterval = pqs.Dates[1];
                                    DateTime lastAcceptableDate = DateTime.MinValue;

                                    //with every pq, start with begin date (is begin or end date of prev pq
                                    //and have end date
                                    //devide number of days by 2 and add to begin date if too many
                                    //until days=0 or number in pq < desired
                                    setting.PlacedDuring = "ctl00_ContentBody_rbPlacedBetween";
                                    setting.Name = "_gcpqgen_ranges_";
                                    setting.PlacedDuringFromDate = pqs.Dates[0];
                                    setting.PlacedDuringToDate = pqs.Dates[1];

                                    while (!_cancel && string.IsNullOrEmpty(message))
                                    {
                                        setting.SetPQPage(_wb);
                                        _browserReady.Reset();
                                        _wb.Document.GetElementById("ctl00_ContentBody_btnSubmit").InvokeMember("Click");
                                        if (waitForPageReady(string.Format("{0}/pocket/gcquery.aspx", baseUrl)))
                                        {
                                            Thread.Sleep((int)numericUpDown4.Value);

                                            //check result
                                            int count = getNumberOfCachesInPQ();
                                            if (count >= 0)
                                            {
                                                _core.DebugLog(Framework.Data.DebugLogLevel.Info, _plugin, null, string.Format("PQ => {0} - {1} ({2})", setting.PlacedDuringFromDate.ToString("d"), setting.PlacedDuringToDate.ToString("d"), count));
                                                if (count > maxInPq)
                                                {
                                                    //go between min and current end
                                                    maxOfInterval = setting.PlacedDuringToDate;
                                                    int days = (int)(maxOfInterval - minOfInterval).TotalDays / 2;
                                                    if (days < 2)
                                                    {
                                                        if (lastAcceptableDate >= setting.PlacedDuringFromDate)
                                                        {
                                                            //take this
                                                            //ready for this one!
                                                            pqs.Dates.Add(lastAcceptableDate);

                                                            //update ui
                                                            ListViewItem lvi = new ListViewItem(lastAcceptableDate.ToString("d"));
                                                            lvi.Tag = lastAcceptableDate;
                                                            listView1.Items.Add(lvi);

                                                            setting.PlacedDuringFromDate = lastAcceptableDate.AddDays(1);
                                                            setting.PlacedDuringToDate = lastPqEnd;

                                                            lastAcceptableDate = DateTime.MinValue;
                                                        }
                                                        else
                                                        {
                                                            message = STR_ERROR;
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        setting.PlacedDuringToDate = minOfInterval.AddDays(days);
                                                    }
                                                }
                                                else if (setting.PlacedDuringToDate >= lastPqEnd)
                                                {
                                                    //ready!
                                                    break;
                                                }
                                                else if (count <= maxInPq && count >= (maxInPq - margin))
                                                {
                                                    //ready for this one!
                                                    pqs.Dates.Add(setting.PlacedDuringToDate);

                                                    //update ui
                                                    ListViewItem lvi = new ListViewItem(setting.PlacedDuringToDate.ToString("d"));
                                                    lvi.Tag = setting.PlacedDuringToDate;
                                                    listView1.Items.Add(lvi);

                                                    setting.PlacedDuringFromDate = setting.PlacedDuringToDate.AddDays(1);
                                                    setting.PlacedDuringToDate = lastPqEnd;

                                                    lastAcceptableDate = DateTime.MinValue;
                                                }
                                                else //count too small
                                                {
                                                    if (setting.PlacedDuringToDate > lastAcceptableDate)
                                                    {
                                                        lastAcceptableDate = setting.PlacedDuringToDate;
                                                    }
                                                    minOfInterval = setting.PlacedDuringToDate;
                                                    int days = (int)(maxOfInterval - minOfInterval).TotalDays / 2;
                                                    if (days < 2)
                                                    {
                                                        //take this
                                                        //ready for this one!
                                                        pqs.Dates.Add(setting.PlacedDuringToDate);

                                                        //update ui
                                                        ListViewItem lvi = new ListViewItem(setting.PlacedDuringToDate.ToString("d"));
                                                        lvi.Tag = setting.PlacedDuringToDate;
                                                        listView1.Items.Add(lvi);

                                                        setting.PlacedDuringFromDate = setting.PlacedDuringToDate.AddDays(1);
                                                        setting.PlacedDuringToDate = lastPqEnd;

                                                        lastAcceptableDate = DateTime.MinValue;
                                                    }
                                                    else
                                                    {
                                                        setting.PlacedDuringToDate = minOfInterval.AddDays(days);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                message = STR_ERROR;
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    message = STR_ERROR;
                                }
                            }
                            else
                            {
                                message = STR_ERROR;
                            }
                        }
                        else
                        {
                            message = STR_ERROR;
                        }
                    }
                    catch
                    {
                        message = STR_ERROR;
                    }

                    _wb.DocumentCompleted -= new WebBrowserDocumentCompletedEventHandler(_wb_DocumentCompleted);
                    _browserReady.Dispose();
                    _browserReady = null;

                    this.ControlBox = true;
                    panel1.Enabled = true;
                    _init = false;
                    comboBox1_SelectedIndexChanged(this, EventArgs.Empty);
                    this.button5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CREATEPQ);
                    if (!string.IsNullOrEmpty(message))
                    {
                        MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(message), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                    }

                }
            }
        }

        private int getNumberOfCachesInPQ()
        {
            //look for the number
            int result = -1;
            HtmlElementCollection elc = _wb.Document.Body.GetElementsByTagName("p");
            if (elc != null)
            {
                foreach (HtmlElement el in elc)
                {
                    string className = el.GetAttribute("classname");
                    if (className != null && className.ToLower() == "success")
                    {
                        //look for the one number within the text
                        string[] parts = el.InnerText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string s in parts)
                        {
                            int currentCount = -1;
                            bool success = Int32.TryParse(s, out currentCount);
                            if (success)
                            {
                                result = currentCount;
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            return result;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            using (BrowserScriptPocketQueriesPQSeriesGCProjectForm dlg = new BrowserScriptPocketQueriesPQSeriesGCProjectForm())
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    BrowserScriptPocketQueriesPQSeries pqs = comboBox1.SelectedItem as BrowserScriptPocketQueriesPQSeries;
                    if (pqs != null)
                    {
                        _init = true;
                        pqs.Dates.Clear();
                        pqs.Dates.AddRange(dlg.DateList);
                        if (pqs.Dates.Count == 0)
                        {
                            pqs.Dates.Add(new DateTime(2001, 1, 1));
                        }
                        pqs.Dates.Add(DateTime.Now.AddMonths(6));
                        _init = false;
                        comboBox1_SelectedIndexChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

    }
}
