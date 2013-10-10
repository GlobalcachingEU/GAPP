using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.APILOGC
{
    public partial class OfflineLogForm : Form
    {
        public const string STR_TITLE = "Offline logging";
        public const string STR_ERROR = "Error";
        public const string STR_ADDGEOCACHES = "Add geocaches";
        public const string STR_CODE = "Code";
        public const string STR_ADD = "Add";
        public const string STR_ADDALLSELECTED = "Add all selected";
        public const string STR_OFFLINELOGS = "Offline logs";
        public const string STR_GEOCACHES = "Geocaches";
        public const string STR_LOGTYPE = "Log type";
        public const string STR_LOGDATE = "Log date";
        public const string STR_LOGTEXT = "Log text";
        public const string STR_LOGSELECTED = "Log selected";
        public const string STR_LOGALL = "Log all";
        public const string STR_OK = "OK";
        public const string STR_LOGONLINE = "Log online";

        public class OfflineLogInfo
        {
            public string GeocacheCode { get; set; }
            public string GeocacheName { get; set; }
            public Framework.Data.LogType LogType { get; set; }
            public DateTime LogDate { get; set; }
            public string LogText { get; set; }

            public override string ToString()
            {
                return string.Format("{0} - {1}", GeocacheCode ?? "", GeocacheName ?? "");
            }
        }

        private Framework.Interfaces.ICore _core = null;
        private Utils.API.GeocachingLiveV6 _client = null;
        private Utils.BasePlugin.Plugin _plugin = null;
        private string _offlineLogsFile = null;

        public OfflineLogForm()
        {
            InitializeComponent();
        }

        public OfflineLogForm(Utils.BasePlugin.Plugin plugin, Framework.Interfaces.ICore core, Utils.API.GeocachingLiveV6 client) :
            this()
        {
            _core = core;
            _client = client;
            _plugin = plugin;

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDGEOCACHES);
            this.label5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CODE);
            this.button6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADD);
            this.button7.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDALLSELECTED);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OFFLINELOGS);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GEOCACHES);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOGTYPE);
            this.label3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOGDATE);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOGTEXT);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOGSELECTED);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOGALL);
            this.button8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);
            this.groupBox3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOGONLINE);

            int[] ids = new int[] { 2, 3, 4, 7, 45 };
            comboBoxLogType1.SetLogTypes(core, (from Framework.Data.LogType l in core.LogTypes join a in ids on l.ID equals a select l).ToList());

            if (core.ActiveGeocache != null)
            {
                textBox2.Text = core.ActiveGeocache.Code;
            }
            button7.Enabled = Utils.DataAccess.GetSelectedGeocaches(core.Geocaches).Count > 0;

            loadOfflineLogs();
        }

        private void loadOfflineLogs()
        {
            try
            {
                _offlineLogsFile = System.IO.Path.Combine(new string[] { _core.PluginDataPath, "OfflineLogs.xml" });

                if (System.IO.File.Exists(_offlineLogsFile))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(_offlineLogsFile);
                    XmlElement root = doc.DocumentElement;

                    XmlNodeList logNodes = root.SelectNodes("log");
                    if (logNodes != null)
                    {
                        foreach (XmlNode n in logNodes)
                        {
                            OfflineLogInfo oi = new OfflineLogInfo();
                            oi.GeocacheCode = n.SelectSingleNode("Code").InnerText;
                            oi.GeocacheName = n.SelectSingleNode("Name").InnerText;
                            oi.LogDate = DateTime.Parse(n.SelectSingleNode("Date").InnerText);
                            oi.LogText = n.SelectSingleNode("Text").InnerText;
                            oi.LogType = Utils.DataAccess.GetLogType(_core.LogTypes, int.Parse(n.SelectSingleNode("Type").InnerText));
                            listBox1.Items.Add(oi);
                        }
                    }
                    listBox1_SelectedIndexChanged(this, EventArgs.Empty);
                    button2.Enabled = listBox1.Items.Count > 0;
                }
            }
            catch
            {
            }
        }

        private void saveOfflineLogs()
        {
            _core.Geocaches.BeginUpdate();
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("logs");
                doc.AppendChild(root);
                foreach (OfflineLogInfo oi in listBox1.Items)
                {
                    XmlElement log = doc.CreateElement("log");
                    root.AppendChild(log);

                    XmlElement el = doc.CreateElement("Code");
                    XmlText txt = doc.CreateTextNode(oi.GeocacheCode);
                    el.AppendChild(txt);
                    log.AppendChild(el);

                    el = doc.CreateElement("Name");
                    txt = doc.CreateTextNode(oi.GeocacheName??"");
                    el.AppendChild(txt);
                    log.AppendChild(el);

                    el = doc.CreateElement("Date");
                    txt = doc.CreateTextNode(oi.LogDate.ToString("s"));
                    el.AppendChild(txt);
                    log.AppendChild(el);

                    el = doc.CreateElement("Text");
                    txt = doc.CreateTextNode(oi.LogText);
                    el.AppendChild(txt);
                    log.AppendChild(el);

                    el = doc.CreateElement("Type");
                    txt = doc.CreateTextNode(oi.LogType.ID.ToString());
                    el.AppendChild(txt);
                    log.AppendChild(el);

                    if (oi.LogType.AsFound)
                    {
                        Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(_core.Geocaches, oi.GeocacheCode);
                        if (gc != null)
                        {
                            gc.Found = true;
                        }
                    }
                }
                doc.Save(_offlineLogsFile);
            }
            catch
            {
            }
            _core.Geocaches.EndUpdate();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            button6.Enabled = textBox2.Text.Length > 2 && textBox2.Text.ToUpper().StartsWith("GC");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OfflineLogInfo oi = new OfflineLogInfo();
            oi.GeocacheCode = textBox2.Text.ToUpper().Trim();
            Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(_core.Geocaches, oi.GeocacheCode);
            if (gc != null)
            {
                oi.GeocacheName = gc.Name;
            }
            oi.LogDate = DateTime.Now;
            oi.LogType = Utils.DataAccess.GetLogType(_core.LogTypes, 2);
            oi.LogText = "";
            listBox1.Items.Add(oi);
            listBox1.SelectedItem = oi;
            button2.Enabled = true;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            List<Framework.Data.Geocache> gcList = Utils.DataAccess.GetSelectedGeocaches(_core.Geocaches);
            foreach (Framework.Data.Geocache g in gcList)
            {
                OfflineLogInfo oi = new OfflineLogInfo();
                oi.GeocacheCode = g.Code;
                oi.GeocacheName = g.Name;
                oi.LogDate = DateTime.Now;
                oi.LogType = Utils.DataAccess.GetLogType(_core.LogTypes, 2);
                oi.LogText = "";
                listBox1.Items.Add(oi);
            }
            listBox1_SelectedIndexChanged(this, EventArgs.Empty);
            button7.Enabled = false;
            button2.Enabled = listBox1.Items.Count>0;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            OfflineLogInfo oi = listBox1.SelectedItem as OfflineLogInfo;
            if (oi != null)
            {
                button1.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = listBox1.SelectedIndex < listBox1.Items.Count-1;
                button5.Enabled = listBox1.SelectedIndex>0;
                comboBoxLogType1.Enabled = true;
                dateTimePicker1.Enabled = true;
                textBox1.Enabled = true;

                comboBoxLogType1.SelectedItem = oi.LogType;
                dateTimePicker1.Value = oi.LogDate;
                textBox1.Text = oi.LogText;
            }
            else
            {
                button1.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
                comboBoxLogType1.Enabled = false;
                dateTimePicker1.Enabled = false;
                textBox1.Enabled = false;
                textBox1.Text = "";
                dateTimePicker1.Value = DateTime.Now;
            }
        }

        private void comboBoxLogType1_SelectedIndexChanged(object sender, EventArgs e)
        {
            OfflineLogInfo oi = listBox1.SelectedItem as OfflineLogInfo;
            if (oi != null)
            {
                oi.LogType = comboBoxLogType1.SelectedItem as Framework.Data.LogType;
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            OfflineLogInfo oi = listBox1.SelectedItem as OfflineLogInfo;
            if (oi != null)
            {
                oi.LogDate = dateTimePicker1.Value;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            OfflineLogInfo oi = listBox1.SelectedItem as OfflineLogInfo;
            if (oi != null)
            {
                oi.LogText = textBox1.Text;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            saveOfflineLogs();
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex>=0)
            {
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 1)
            {
                int index = listBox1.SelectedIndex;
                object o = listBox1.Items[listBox1.SelectedIndex];
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                listBox1.Items.Insert(index - 1, o);
                listBox1.SelectedIndex = index - 1;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0 && listBox1.SelectedIndex<listBox1.Items.Count-1)
            {
                int index = listBox1.SelectedIndex;
                object o = listBox1.Items[listBox1.SelectedIndex];
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                listBox1.Items.Insert(index + 1, o);
                listBox1.SelectedIndex = index + 1;
            }
        }

        private Framework.Data.Geocache getGeocache(string code)
        {
            Cursor = Cursors.WaitCursor;
            Framework.Data.Geocache result = null;
            try
            {
                Utils.API.LiveV6.SearchForGeocachesRequest req = new Utils.API.LiveV6.SearchForGeocachesRequest();
                req.IsLite = _core.GeocachingComAccount.MemberTypeId == 1;
                req.AccessToken = _client.Token;
                req.CacheCode = new Utils.API.LiveV6.CacheCodeFilter();
                req.CacheCode.CacheCodes = new string[] { code };
                req.MaxPerPage = 1;
                req.GeocacheLogCount = 5;
                var resp = _client.Client.SearchForGeocaches(req);
                if (resp.Status.StatusCode == 0 && resp.Geocaches != null)
                {
                    Utils.API.Import.AddGeocaches(_core, resp.Geocaches);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(resp.Status.StatusMessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
            catch
            {
            }
            Cursor = Cursors.Default;
            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OfflineLogInfo oi = listBox1.SelectedItem as OfflineLogInfo;
            if (oi != null)
            {
                Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(_core.Geocaches, oi.GeocacheCode);
                if (gc == null)
                {
                    gc = getGeocache(oi.GeocacheCode);
                }
                if (gc != null)
                {
                    using (GeocacheLogForm dlg = new GeocacheLogForm(_core, _client, gc, oi))
                    {
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            listBox1.Items.Remove(oi);
                        }
                    }
                }
            }            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<OfflineLogInfo> processed = new List<OfflineLogInfo>();
            foreach (OfflineLogInfo oi in listBox1.Items)
            {
                Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(_core.Geocaches, oi.GeocacheCode);
                if (gc == null)
                {
                    gc = getGeocache(oi.GeocacheCode);
                }
                if (gc != null)
                {
                    using (GeocacheLogForm dlg = new GeocacheLogForm(_core, _client, gc, oi))
                    {
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            processed.Add(oi);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            foreach (OfflineLogInfo oi in processed)
            {
                listBox1.Items.Remove(oi);
            }
        }

    }
}
