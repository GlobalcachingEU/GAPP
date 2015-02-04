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
    public partial class SettingsForm : Form
    {
        public const string STR_TITLE = "Web browser settings";
        public const string STR_NAMESPACE = "Namespaces";
        public const string STR_HOMEPAGE = "Homepage";
        public const string STR_SYSTEMSCRIPTS = "System scripts";
        public const string STR_NAME = "Name";
        public const string STR_USERSCRIPTS = "User scripts";
        public const string STR_DELETE = "Delete";
        public const string STR_NEW = "New";
        public const string STR_RENAME = "Rename";
        public const string STR_OK = "OK";
        public const string STR_SUPPRESSSCRIPTERRORS = "Suppress script errors";
        public const string STR_COMPATIBILITY = "Compatibility";

        public class UserScriptInfo
        {
            public string Name { get; set; }
            public string ClassCode { get; set; }
            public UserScripts.Script OriginalScript { get; set; }
        }

        private int[] _browserCompatibilityValues = new int[]
        {
            7,
            8,
            9,
            10,
        };

        private UserScripts _userScripts = null;

        public SettingsForm()
        {
            InitializeComponent();
        }

        public SettingsForm(List<string> systemScripts, UserScripts userScripts)
            : this()
        {
            _userScripts = userScripts;

            textBox1.Text = PluginSettings.Instance.HomePage;

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_HOMEPAGE);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SYSTEMSCRIPTS);
            this.listView1.Columns[0].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.listView2.Columns[0].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.groupBox3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_USERSCRIPTS);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELETE);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NEW);
            this.button4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RENAME);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SUPPRESSSCRIPTERRORS);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COMPATIBILITY);

            checkBox1.Checked = PluginSettings.Instance.ScriptErrorsSuppressed;
            comboBox1.Items.AddRange((from a in _browserCompatibilityValues select a.ToString()).ToArray());
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf((PluginSettings.Instance.CompatibilityMode / 1000).ToString());

            ListViewItem lvi;
            foreach (string s in systemScripts)
            {
                lvi = new ListViewItem(Utils.LanguageSupport.Instance.GetTranslation(s));
                lvi.Tag = s;
                lvi.Checked = !PluginSettings.Instance.DisabledSystemScripts.Contains(s);
                listView1.Items.Add(lvi);
            }

            lvi = new ListViewItem(Utils.LanguageSupport.Instance.GetTranslation(STR_NAMESPACE));
            lvi.Tag = userScripts.UsingNamespaces;
            lvi.Checked = true;
            listView2.Items.Add(lvi);

            foreach (UserScripts.Script scr in userScripts.Scripts)
            {
                UserScriptInfo scri = new UserScriptInfo();
                scri.Name = scr.Name;
                scri.ClassCode = scr.ClassCode;
                scri.OriginalScript = scr;

                lvi = new ListViewItem(Utils.LanguageSupport.Instance.GetTranslation(scr.Name));
                lvi.Tag = scri;
                lvi.Checked = scr.Enabled;
                listView2.Items.Add(lvi);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PluginSettings.Instance.ScriptErrorsSuppressed = checkBox1.Checked;
            if (comboBox1.SelectedItem != null)
            {
                PluginSettings.Instance.CompatibilityMode = int.Parse(comboBox1.SelectedItem.ToString()) * 1000;
            }
            PluginSettings.Instance.HomePage = textBox1.Text;
            foreach (ListViewItem lvi in listView1.Items)
            {
                if (!lvi.Checked)
                {
                    PluginSettings.Instance.DisabledSystemScripts.Add(lvi.Tag.ToString());
                }
            }

            //user scripts
            bool changed = false;
            if (listView2.Items[0].Tag.ToString() != _userScripts.UsingNamespaces)
            {
                changed = true;
                _userScripts.UsingNamespaces = listView2.Items[0].Tag.ToString();
            }
            if (listView2.Items.Count - 1 != _userScripts.Scripts.Count)
            {
                changed = true;
            }
            if (!changed)
            {
                for (int i = 1; i < listView2.Items.Count; i++)
                {
                    UserScriptInfo scri = listView2.Items[i].Tag as UserScriptInfo;
                    if (scri.OriginalScript == null || scri.OriginalScript.Enabled != listView2.Items[i].Checked || scri.Name != scri.OriginalScript.Name || scri.ClassCode != scri.OriginalScript.ClassCode)
                    {
                        changed = true;
                        break;
                    }
                }
            }
            if (changed)
            {
                _userScripts.Scripts.Clear();
                for (int i = 1; i < listView2.Items.Count; i++)
                {
                    UserScriptInfo scri = listView2.Items[i].Tag as UserScriptInfo;
                    UserScripts.Script s = new UserScripts.Script();
                    s.ClassCode = scri.ClassCode;
                    s.Enabled = listView2.Items[i].Checked;
                    s.Name = scri.Name;
                    _userScripts.Scripts.Add(s);
                }
                _userScripts.Invalidate();
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedIndices.Count == 0)
            {
                textBox2.Text = "";
                button2.Enabled = false;
            }
            else
            {
                int index = listView2.SelectedIndices[0];
                if (index == 0)
                {
                    textBox2.Text = listView2.Items[index].Tag.ToString();
                    button2.Enabled = false;
                }
                else
                {
                    textBox2.Text = (listView2.Items[index].Tag as UserScriptInfo).ClassCode;
                    button2.Enabled = true;
                }
            }
            textBox3_TextChanged(this, EventArgs.Empty);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            UserScriptInfo scri = new UserScriptInfo();
            scri.Name = textBox3.Text;
            scri.ClassCode = _userScripts.GetTemplate(scri.Name);

            ListViewItem lvi = new ListViewItem(Utils.LanguageSupport.Instance.GetTranslation(scri.Name));
            lvi.Tag = scri;
            lvi.Checked = false;
            listView2.Items.Add(lvi);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedIndices.Count > 0)
            {
                int index = listView2.SelectedIndices[0];
                if (index == 0)
                {
                    listView2.Items[index].Tag = textBox2.Text;
                }
                else
                {
                    (listView2.Items[index].Tag as UserScriptInfo).ClassCode = textBox2.Text;
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedIndices.Count > 0)
            {
                int index = listView2.SelectedIndices[0];
                listView2.Items.RemoveAt(index);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedIndices.Count > 0)
            {
                int index = listView2.SelectedIndices[0];
                if (index > 0)
                {
                    (listView2.Items[index].Tag as UserScriptInfo).Name = textBox3.Text;
                    listView2.Items[index].SubItems[0].Text = textBox3.Text;
                }
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            button3.Enabled = textBox3.Text.Length > 0;
            bool canRename = false;
            if (textBox3.Text.Length > 0 && listView2.SelectedIndices.Count > 0 && listView2.SelectedIndices[0] > 0)
            {
                canRename = true;
            }
            button4.Enabled = canRename;
        }
    }
}
