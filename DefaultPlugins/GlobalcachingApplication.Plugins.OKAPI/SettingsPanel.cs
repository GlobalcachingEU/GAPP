using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Web;
using System.IO;
using System.Xml;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_SITES = "Opencaching sites";
        public const string STR_ACTIVE = "Active";
        public const string STR_USERNAME = "User name";
        public const string STR_USERID = "User ID";
        public const string STR_RETRIEVE = "Retrieve";
        public const string STR_SAVE = "Save";

        private Framework.Interfaces.ICore _core = null;
        private bool _manualUpdate = false;

        public SettingsPanel()
        {
            InitializeComponent();
        }

        public SettingsPanel(Framework.Interfaces.ICore core)
            : this()
        {
            _core = core;

            comboBox1.Items.AddRange(SiteManager.Instance.AvailableSites.ToArray());
            comboBox1.SelectedItem = SiteManager.Instance.ActiveSite;

            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SITES);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ACTIVE);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_USERNAME);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_USERID);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RETRIEVE);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SAVE);
        }

        public void Apply()
        {
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SiteInfo si = comboBox1.SelectedItem as SiteInfo;
            if (si != null)
            {
                textBox1.Text = si.Username;
                textBox2.Text = si.UserID;
                textBox1.Enabled = true;
                button2.Enabled = true;
                SiteManager.Instance.ActiveSite = si;
            }
            else
            {
                textBox1.Text = "";
                textBox2.Text = "";
                textBox1.Enabled = false;
                button2.Enabled = false;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!_manualUpdate)
            {
                textBox2.Text = "";
            }
            button1.Enabled = textBox1.Text.Length > 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SiteInfo si = comboBox1.SelectedItem as SiteInfo;
            if (si != null)
            {
                si.Username = textBox1.Text;
                si.UserID = textBox2.Text;
                si.SaveSettings();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SiteInfo si = comboBox1.SelectedItem as SiteInfo;
            if (si != null)
            {
                textBox2.Text = "";
                Cursor = Cursors.WaitCursor;
                try
                {
                    //services/users/by_username
                    HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(string.Format("{0}services/users/by_username?format=xmlmap2&username={1}&fields=uuid|username&consumer_key={2}", si.OKAPIBaseUrl, HttpUtility.UrlEncode(textBox1.Text), HttpUtility.UrlEncode(si.ConsumerKey)));
                    wr.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
                    wr.Method = WebRequestMethods.Http.Get;
                    using (HttpWebResponse webResponse = (HttpWebResponse)wr.GetResponse())
                    using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        //<object><string key="uuid">50e9442e-...934</string></object>
                        string doc = reader.ReadToEnd();

                        XmlDocument xdoc = new XmlDocument();
                        xdoc.LoadXml(doc);
                        XmlNodeList nl = xdoc.SelectNodes("/object/string");
                        if (nl != null)
                        {
                            foreach (XmlNode n in nl)
                            {
                                XmlAttribute attr = n.Attributes["key"];
                                if (attr != null && attr.Value == "uuid")
                                {
                                    textBox2.Text = n.InnerText;
                                }
                                else if (attr != null && attr.Value == "username")
                                {
                                    _manualUpdate = true;
                                    textBox1.Text = n.InnerText;
                                    _manualUpdate = false;
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
                Cursor = Cursors.Default;
            }
        }
    }
}
