using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
            textBox2.Text = "";
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
                    string username = textBox1.Text;
                    string userid = OKAPIService.GetUserID(si, ref username);
                    textBox1.Text = username;
                    textBox2.Text = userid;
                }
                catch
                {
                }
                Cursor = Cursors.Default;
            }
        }
    }
}
