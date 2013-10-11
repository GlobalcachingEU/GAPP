using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace GlobalcachingApplication.Plugins.Munzee
{
    public partial class MunzeeDfxAtForm : Form
    {
        public const string STR_TITLE = "Import Munzees from munzee.dfx.at";
        public const string STR_MUNZEECOMACCOUNT = "Munzee.com account";
        public const string STR_SELECTURL = "Select URL";
        public const string STR_ADD = "Add";
        public const string STR_NAME = "Name";
        public const string STR_URLS = "URLs";
        public const string STR_REMOVE = "Remove";
        public const string STR_OK = "OK";
        public const string STR_URL = "URL";
        public const string STR_COMMENT = "Comment";
        public const string STR_INFO = "Create the URL on munzee.dfx.at and choose JSON.";

        public string SelectedUrl { get; private set; }

        private string _urlsFile = null;
        private Framework.Interfaces.ICore _core = null;

        public class UrlInfo
        {
            public string Url { get; set; }
            public string Comment { get; set; }

            public override string ToString()
            {
                return string.Format("{1} ({0})", Url ?? "", Comment ?? "");
            }
        }
        private List<UrlInfo> _urlInfos = new List<UrlInfo>();

        public MunzeeDfxAtForm()
        {
            InitializeComponent();
        }

        public MunzeeDfxAtForm(Framework.Interfaces.ICore core): this()
        {
            _core = core;

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.groupBox3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MUNZEECOMACCOUNT);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTURL);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADD);
            this.label7.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_URLS);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADD);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_REMOVE);
            this.button4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_URL);
            this.label5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COMMENT);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_INFO);

            textBox3.Text = Properties.Settings.Default.AccountName;

            try
            {
                _urlsFile = System.IO.Path.Combine( _core.PluginDataPath, "MunzeeDfxAt.xml" );

                if (System.IO.File.Exists(_urlsFile))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(_urlsFile);
                    XmlElement root = doc.DocumentElement;

                    XmlNodeList bmNodes = root.SelectNodes("url");
                    if (bmNodes != null)
                    {
                        foreach (XmlNode n in bmNodes)
                        {
                            UrlInfo bm = new UrlInfo();
                            bm.Url = n.SelectSingleNode("Url").InnerText;
                            bm.Comment = n.SelectSingleNode("Comment").InnerText;
                            _urlInfos.Add(bm);
                        }
                    }
                    listBox1.Items.AddRange(_urlInfos.ToArray());
                }
            }
            catch
            {
            }

        }

        private void saveUrls()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("urls");
                doc.AppendChild(root);
                foreach (UrlInfo bmi in _urlInfos)
                {
                    XmlElement bm = doc.CreateElement("url");
                    root.AppendChild(bm);

                    XmlElement el = doc.CreateElement("Url");
                    XmlText txt = doc.CreateTextNode(bmi.Url);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("Comment");
                    txt = doc.CreateTextNode(bmi.Comment);
                    el.AppendChild(txt);
                    bm.AppendChild(el);
                }
                doc.Save(_urlsFile);
            }
            catch
            {
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button4.Enabled = listBox1.SelectedIndex >= 0;
            button3.Enabled = listBox1.SelectedIndex >= 0;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button2.Enabled = (textBox1.Text.StartsWith("http") && textBox2.Text.Length > 0);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            UrlInfo ui = listBox1.SelectedItem as UrlInfo;
            if (ui != null)
            {
                _urlInfos.Remove(ui);
                listBox1.Items.Remove(ui);
                saveUrls();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://munzee.dfx.at");
            }
            catch
            {
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            UrlInfo ui = new UrlInfo();
            ui.Comment = textBox2.Text;
            ui.Url = textBox1.Text;
            _urlInfos.Add(ui);
            listBox1.Items.Add(ui);
            listBox1.SelectedItem = ui;
            saveUrls();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            UrlInfo ui = listBox1.SelectedItem as UrlInfo;
            if (ui != null)
            {
                Properties.Settings.Default.AccountName = textBox3.Text;
                Properties.Settings.Default.Save();
                _core.GeocachingAccountNames.SetAccountName("MZ", Properties.Settings.Default.AccountName);
                SelectedUrl = ui.Url;
                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            button2.Enabled = (textBox1.Text.StartsWith("http") && textBox2.Text.Length > 0);
        }
    }
}
