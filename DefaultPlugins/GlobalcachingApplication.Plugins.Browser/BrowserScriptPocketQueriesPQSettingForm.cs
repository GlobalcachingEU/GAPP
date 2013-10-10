using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Browser
{
    public partial class BrowserScriptPocketQueriesPQSettingForm : Form
    {
        public const string STR_TITLE = "Available pocket query settings";
        public const string STR_REMOVE = "Remove";
        public const string STR_SAVE = "Save";
        public const string STR_OK = "OK";

        public BrowserScriptPocketQueriesPQSetting SelectedSetting { get; set; }
        private string _filename = "";

        public BrowserScriptPocketQueriesPQSettingForm()
        {
            InitializeComponent();
        }

        public BrowserScriptPocketQueriesPQSettingForm(string filename)
            : this()
        {
            _filename = filename;
            List<BrowserScriptPocketQueriesPQSetting> l = BrowserScriptPocketQueriesPQSetting.Load(_filename);
            if (l != null)
            {
                listBox1.Items.AddRange(l.ToArray());
            }

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_REMOVE);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SAVE);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                button1.Enabled = true;
                button3.Enabled = true;
            }
            else
            {
                button1.Enabled = true;
                button3.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BrowserScriptPocketQueriesPQSetting.Save(_filename, (from BrowserScriptPocketQueriesPQSetting a in listBox1.Items select a).ToList());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SelectedSetting = listBox1.SelectedItem as BrowserScriptPocketQueriesPQSetting;
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                BrowserScriptPocketQueriesPQSetting.Save(_filename, (from BrowserScriptPocketQueriesPQSetting a in listBox1.Items select a).ToList());
            }
        }
    }
}
