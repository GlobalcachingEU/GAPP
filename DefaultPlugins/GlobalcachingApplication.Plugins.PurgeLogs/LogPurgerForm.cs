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


            numericUpDown1.Value = Properties.Settings.Default.DaysMonthsCount;
            comboBox1.SelectedIndex = Properties.Settings.Default.DaysMonths;
            numericUpDown2.Value = Properties.Settings.Default.KeepAtLeast;
            checkBox1.Checked = Properties.Settings.Default.KeepAllOfOwned;
            checkBox2.Checked = Properties.Settings.Default.KeepOwnLogs;
            if (Properties.Settings.Default.KeepLogsOf == null)
            {
                Properties.Settings.Default.KeepLogsOf = new System.Collections.Specialized.StringCollection();
            }
            else
            {
                foreach (string s in Properties.Settings.Default.KeepLogsOf)
                {
                    listBox1.Items.Add(s);
                }
            }
            if (Properties.Settings.Default.RemoveAllLogsFrom == null)
            {
                Properties.Settings.Default.RemoveAllLogsFrom = new System.Collections.Specialized.StringCollection();
            }
            else
            {
                foreach (string s in Properties.Settings.Default.RemoveAllLogsFrom)
                {
                    listBox2.Items.Add(s);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.DaysMonthsCount = (int)numericUpDown1.Value;
            Properties.Settings.Default.DaysMonths = comboBox1.SelectedIndex;
            Properties.Settings.Default.KeepAtLeast = (int)numericUpDown2.Value;
            Properties.Settings.Default.KeepAllOfOwned = checkBox1.Checked;
            Properties.Settings.Default.KeepOwnLogs = checkBox2.Checked;
            Properties.Settings.Default.KeepLogsOf.Clear();
            foreach (string s in listBox1.Items)
            {
                Properties.Settings.Default.KeepLogsOf.Add(s);
            }
            Properties.Settings.Default.RemoveAllLogsFrom.Clear();
            foreach (string s in listBox2.Items)
            {
                Properties.Settings.Default.RemoveAllLogsFrom.Add(s);
            }
            Properties.Settings.Default.Save();
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
