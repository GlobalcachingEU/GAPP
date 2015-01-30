using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GlobalcachingApplication.Plugins.AccountSwitcher
{
    public partial class SettingsFolderForm : Form
    {
        public const string STR_TITLE = "Settings scope";
        public const string STR_SETTINGSFOLDER = "Settings scope";
        public const string STR_TARGETSETTINGSFOLDER = "Target settings scope";
        public const string STR_CURRENT = "Current";
        public const string STR_AVAILABLE = "Available";
        public const string STR_ENABLESTARTUP = "Enable scope selection at startup";
        public const string STR_FOLDER = "Scope";
        public const string STR_COPYCURRENT = "Copy current settings to selected scope";
        public const string STR_COPYDEFAULT = "Create default settings in selected scope";
        public const string STR_SWITCH = "Switch to selected scope";
        public const string STR_OK = "OK";

        private Framework.Interfaces.ICore _core = null;
        private SettingsFolder _plugin = null;

        public SettingsFolderForm()
        {
            InitializeComponent();
        }

        public SettingsFolderForm(SettingsFolder plugin, Framework.Interfaces.ICore core)
            : this()
        {
            _core = core;
            _plugin = plugin;

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SETTINGSFOLDER);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TARGETSETTINGSFOLDER);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CURRENT);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AVAILABLE);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ENABLESTARTUP);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FOLDER);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COPYCURRENT);
            this.button4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COPYDEFAULT);
            this.button5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SWITCH);
            this.button8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);

            textBox1.Text = core.SettingsProvider.GetSettingsScope();
            listBox1.Items.AddRange(core.SettingsProvider.GetSettingsScopes().ToArray());
            checkBox1.Checked = core.EnablePluginDataPathAtStartup;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text.Length == 0)
            {
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
            }
            else if (string.Compare(textBox1.Text, textBox2.Text, true) != 0)
            {
                    button3.Enabled = true;
                    button4.Enabled = true;
                    button5.Enabled = true;
            }
            else
            {
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _plugin.SwitchSettingsFolder(textBox2.Text);
            Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            _core.EnablePluginDataPathAtStartup = checkBox1.Checked;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = (listBox1.SelectedIndex >= 0 && (listBox1.SelectedItem as string)!=_core.SettingsProvider.GetSettingsScope());
            if (listBox1.SelectedIndex >= 0)
            {
                textBox2.Text = listBox1.Items[listBox1.SelectedIndex].ToString();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length > 0)
            {
                _core.SettingsProvider.NewSettingsScope(textBox2.Text, _core.SettingsProvider.GetSettingsScope());
                listBox1.Items.Clear();
                listBox1.Items.AddRange(_core.SettingsProvider.GetSettingsScopes().ToArray());
            }
        }


        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length > 0)
            {
                _core.SettingsProvider.NewSettingsScope(textBox2.Text, null);
                listBox1.Items.Clear();
                listBox1.Items.AddRange(_core.SettingsProvider.GetSettingsScopes().ToArray());
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            textBox2.Text = "default";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                _core.SettingsProvider.DeleteSettingsScope(listBox1.SelectedItem as string);
                listBox1.Items.Clear();
                listBox1.Items.AddRange(_core.SettingsProvider.GetSettingsScopes().ToArray());
            }
        }
    }
}
