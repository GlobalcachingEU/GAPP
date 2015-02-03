using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.APIFindsOfUser
{
    public partial class ImportForm : Form
    {
        public const string STR_TITLE = "Import logs of other users";
        public const string STR_COPYTOCLIPBOARD = "Copy to clipboard";
        public const string STR_USERS = "Users";
        public const string STR_GETLOGS = "Get logs";
        public const string STR_REMOVE = "Remove";
        public const string STR_REMOVEANDLOGS = "Remove and remove logs";
        public const string STR_USERNAME = "User name";
        public const string STR_ADD = "Add";
        public const string STR_IMPORTMISSING = "Import missing geocaches";
        public const string STR_IMPORTSELECTED = "Import logs of selected user";
        public const string STR_IMPORTALL = "Import logs of all users";
        public const string STR_BETWEENDATES = "Between dates";
        public const string STR_LOGTYPES = "Log types";
        public const string STR_SELECTALL = "Select all";
        public const string STR_DESELECTALL = "Deselect all";

        private Framework.Interfaces.ICore _core = null;

        public List<string> SelectedUsers { get; private set; }
        public List<long> SelectedLogTypes { get; private set; }
        public DateTime DateFrom { get; private set; }
        public DateTime DateTo { get; private set; }

        public ImportForm()
        {
            InitializeComponent();
        }

        public ImportForm(Framework.Interfaces.ICore core)
            : this()
        {
            _core = core;
            checkBox1.Checked = PluginSettings.Instance.ImportMissingCaches;
            checkBox2.Checked = PluginSettings.Instance.BetweenDates;

            dateTimePicker1.Value = DateTime.Now.AddDays(-7);
            dateTimePicker2.Value = DateTime.Now;

            foreach (string s in PluginSettings.Instance.Usernames)
            {
                listBox1.Items.Add(s);
            }
            button5.Enabled = listBox1.Items.Count > 0;

            toolTip1.SetToolTip(button6, Utils.LanguageSupport.Instance.GetTranslation(STR_COPYTOCLIPBOARD));
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_USERS);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GETLOGS);
            this.groupBox3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOGTYPES);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_REMOVE);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_REMOVEANDLOGS);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_USERNAME);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADD);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IMPORTMISSING);
            this.button4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IMPORTSELECTED);
            this.button5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IMPORTALL);
            this.checkBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_BETWEENDATES);
            this.button7.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTALL);
            this.button8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DESELECTALL);

            long[] lt = new long[] { 1, 2, 3, 4, 5, 6, 7, 9, 10, 11, 12, 22, 23, 24, 45, 46, 47 };
            var ltl = from a in core.LogTypes where lt.Contains(a.ID) select a;
            foreach (var gt in ltl)
            {
                imageList1.Images.Add(gt.ID.ToString(), Image.FromFile(Utils.ImageSupport.Instance.GetImagePath(core, Framework.Data.ImageSize.Small, gt)));
                ListViewItem lvt = new ListViewItem(Utils.LanguageSupport.Instance.GetTranslation(gt.Name), imageList1.Images.Count - 1);
                lvt.Checked = PluginSettings.Instance.LogTypes.Count == 0 || PluginSettings.Instance.LogTypes.Contains(gt.ID.ToString());
                lvt.Tag = gt;
                listView1.Items.Add(lvt);
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            DateFrom = dateTimePicker1.Value;
            DateTo = dateTimePicker2.Value;
            SelectedUsers = (from string s in listBox1.Items select s).ToList();
            SelectedLogTypes = new List<long>();
            PluginSettings.Instance.LogTypes.Clear();
            foreach(ListViewItem lvt in listView1.CheckedItems)
            {
                PluginSettings.Instance.LogTypes.Add((lvt.Tag as Framework.Data.LogType).ID.ToString());
                SelectedLogTypes.Add((lvt.Tag as Framework.Data.LogType).ID);
            }
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DateFrom = dateTimePicker1.Value;
            DateTo = dateTimePicker2.Value;
            SelectedUsers = new List<string>();
            SelectedLogTypes = new List<long>();
            PluginSettings.Instance.LogTypes.Clear();
            foreach (ListViewItem lvt in listView1.CheckedItems)
            {
                PluginSettings.Instance.LogTypes.Add((lvt.Tag as Framework.Data.LogType).ID.ToString());
                SelectedLogTypes.Add((lvt.Tag as Framework.Data.LogType).ID);
            }
            if (listBox1.SelectedItem != null)
            {
                SelectedUsers.Add((string)listBox1.SelectedItem);
                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            PluginSettings.Instance.ImportMissingCaches = checkBox1.Checked;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim().Length > 0)
            {
                PluginSettings.Instance.Usernames.Add(textBox1.Text.Trim());
                listBox1.Items.Add(textBox1.Text.Trim());
                button5.Enabled = true;
            }
            textBox1.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                PluginSettings.Instance.Usernames.Remove((string)listBox1.Items[listBox1.SelectedIndex]);
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                button5.Enabled = listBox1.Items.Count > 0;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                string usr = (string)listBox1.Items[listBox1.SelectedIndex];
                PluginSettings.Instance.Usernames.Remove(usr);
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                _core.Logs.BeginUpdate();

                List<Framework.Data.Log> logs = null;
                usr = usr.ToLower();
                logs = (from Framework.Data.Log l in _core.Logs
                          where l.Finder.ToLower()==usr
                          select l).ToList();
                foreach (Framework.Data.Log l in logs)
                {
                    _core.Logs.Remove(l);
                }

                _core.Logs.EndUpdate();
                button5.Enabled = listBox1.Items.Count > 0;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                button1.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button2.Enabled = textBox1.Text.Length > 0;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (PluginSettings.Instance.Usernames.Count == 0)
            {
                Clipboard.SetText("");
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(PluginSettings.Instance.Usernames[0]);
                for (int i = 1; i < PluginSettings.Instance.Usernames.Count; i++)
                {
                    sb.Append(",");
                    sb.Append(PluginSettings.Instance.Usernames[i]);
                }
                Clipboard.SetText(sb.ToString());
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            PluginSettings.Instance.BetweenDates = checkBox2.Checked;
            dateTimePicker1.Enabled = checkBox2.Checked;
            dateTimePicker2.Enabled = checkBox2.Checked;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvt in listView1.Items)
            {
                lvt.Checked = true;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvt in listView1.Items)
            {
                lvt.Checked = false;
            }
        }
    }
}
