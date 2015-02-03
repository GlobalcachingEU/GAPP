using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.PurgeLogs
{
    public partial class LogPurgerForm : Form
    {
        public const string STR_TITLE = "Purge logs";
        public const string STR_DAYS = "Days";
        public const string STR_MONTHS = "Months";
        public const string STR_OLDERTHAN = "Older than";
        public const string STR_KEEPATLEAST = "Keep at least";
        public const string STR_KEEPOFOWNED = "Keep all of owned caches";
        public const string STR_KEEPOWNLOGS = "Keep all own logs";
        public const string STR_KEEPLOGSOF = "Keep logs of";
        public const string STR_REMOVELOGSFROM = "Remove logs from";

        public LogPurgerForm()
        {
            InitializeComponent();

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            comboBox1.Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_DAYS));
            comboBox1.Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_MONTHS));
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OLDERTHAN);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_KEEPATLEAST);
            this.label10.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_KEEPOFOWNED);
            this.label12.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_KEEPOWNLOGS);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_KEEPLOGSOF);
            this.label8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_REMOVELOGSFROM);


            numericUpDown1.Value = PluginSettings.Instance.DaysMonthsCount;
            comboBox1.SelectedIndex = PluginSettings.Instance.DaysMonths;
            numericUpDown2.Value = PluginSettings.Instance.KeepAtLeast;
            checkBox1.Checked = PluginSettings.Instance.KeepAllOfOwned;
            checkBox2.Checked = PluginSettings.Instance.KeepOwnLogs;
            foreach (string s in PluginSettings.Instance.KeepLogsOf)
            {
                listBox1.Items.Add(s);
            }
            foreach (string s in PluginSettings.Instance.RemoveAllLogsFrom)
            {
                listBox2.Items.Add(s);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            PluginSettings.Instance.DaysMonthsCount = (int)numericUpDown1.Value;
            PluginSettings.Instance.DaysMonths = comboBox1.SelectedIndex;
            PluginSettings.Instance.KeepAtLeast = (int)numericUpDown2.Value;
            PluginSettings.Instance.KeepAllOfOwned = checkBox1.Checked;
            PluginSettings.Instance.KeepOwnLogs = checkBox2.Checked;
            PluginSettings.Instance.KeepLogsOf.Clear();
            foreach (string s in listBox1.Items)
            {
                PluginSettings.Instance.KeepLogsOf.Add(s);
            }
            PluginSettings.Instance.RemoveAllLogsFrom.Clear();
            foreach (string s in listBox2.Items)
            {
                PluginSettings.Instance.RemoveAllLogsFrom.Add(s);
            }
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = textBox1.Text.Length > 0;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            button4.Enabled = textBox2.Text.Length > 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0)
            {
                if (listBox1.Items.IndexOf(textBox1.Text)<0)
                {
                    listBox1.Items.Add(textBox1.Text);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length > 0)
            {
                if (listBox2.Items.IndexOf(textBox2.Text) < 0)
                {
                    listBox2.Items.Add(textBox2.Text);
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button2.Enabled = listBox1.SelectedIndex >= 0;
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            button3.Enabled = listBox2.SelectedIndex >= 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex >= 0)
            {
                listBox2.Items.RemoveAt(listBox2.SelectedIndex);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            }
        }
    }
}
