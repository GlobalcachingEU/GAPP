using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.AccountSwitcher
{
    public partial class AccountsForm : Form
    {
        public const string STR_YES = "Yes";
        public const string STR_NO = "No";
        public const string STR_TITLE = "Accounts";
        public const string STR_ACCOUNTS = "Accounts";
        public const string STR_GCCOMACCOUNT = "Geocaching.com account";
        public const string STR_NAME = "Name";
        public const string STR_MENBERSHIP = "Membership";
        public const string STR_API = "API";
        public const string STR_CLEARAUTHORIZE = "Clear authorization";
        public const string STR_ACTIVATE = "Activate";
        public const string STR_REMOVE = "Remove";
        public const string STR_FROMCURRENT = "From current settings";
        public const string STR_ENABLE = "Enable";
        public const string STR_CREATEACCOUNT = "Create account";
        public const string STR_OK = "OK";
        public const string STR_NEWACCOUNT = "New account";

        private Framework.Interfaces.ICore _core = null;
        public List<AccountInfo> AccountInfoSettings = null;
        private bool _updating = false;

        public AccountsForm()
        {
            InitializeComponent();
        }

        public AccountsForm(Framework.Interfaces.ICore core, List<AccountInfo> ail)
            : this()
        {
            _core = core;
            AccountInfoSettings = ail;

            comboBox1.Items.AddRange(ail.ToArray());
            textBox1.Text = core.GeocachingComAccount.AccountName;
            comboBox1_SelectedIndexChanged(this, EventArgs.Empty);

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ACCOUNTS);
            this.groupBox3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GCCOMACCOUNT);
            this.label10.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.label9.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_API);
            this.label8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MENBERSHIP);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CLEARAUTHORIZE);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.button4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ACTIVATE);
            this.button5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_REMOVE);
            this.button6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FROMCURRENT);
            this.label12.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ENABLE);
            this.label14.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ENABLE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.label16.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CREATEACCOUNT);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NEWACCOUNT);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = textBox1.Text.Length>0 && (from a in AccountInfoSettings where a.Name == textBox1.Text select a).FirstOrDefault() == null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0)
            {
                AccountInfo ai = new AccountInfo();
                ai.Name = textBox1.Text;
                ai.SwitchDatabase = true;
                ai.SwitchGeocachingComAccount = true;
                ai.SaveSettings(_core);
                AccountInfoSettings.Add(ai);
                comboBox1.Items.Add(ai);
                comboBox1.SelectedItem = ai;
                comboBox1_SelectedIndexChanged(this, EventArgs.Empty);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            AccountInfo ai = comboBox1.SelectedItem as AccountInfo;
            if (ai != null)
            {
                ai.RestoreSettings(_core);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _updating = true;
            AccountInfo ai = comboBox1.SelectedItem as AccountInfo;
            if (ai != null)
            {
                button4.Enabled = true;
                button5.Enabled = true;
                button6.Enabled = true;
                groupBox3.Enabled = true;
                groupBox4.Enabled = true;
                checkBox1.Checked = ai.SwitchGeocachingComAccount;
                checkBox2.Checked = ai.SwitchDatabase;
                textBox2.Text = ai.InternalStorageInfo == null ? "" : ai.InternalStorageInfo.Name ?? "";
                textBoxUsername.Text = ai.GeocachingComAccount == null ? "" : ai.GeocachingComAccount.AccountName ?? "";
                labelApiEnabled.Text = ai.GeocachingComAccount == null ? Utils.LanguageSupport.Instance.GetTranslation(STR_NO) : string.IsNullOrEmpty(ai.GeocachingComAccount.APIToken) ? Utils.LanguageSupport.Instance.GetTranslation(STR_NO) : Utils.LanguageSupport.Instance.GetTranslation(STR_YES);
                labelApiMembership.Text = ai.GeocachingComAccount == null ? "-" : string.IsNullOrEmpty(ai.GeocachingComAccount.MemberType) ? "-" : ai.GeocachingComAccount.MemberType;
            }
            else
            {
                button4.Enabled = false;
                button5.Enabled = false;
                button6.Enabled = false;
                groupBox3.Enabled = false;
                groupBox4.Enabled = false;
                checkBox1.Checked = false;
                checkBox2.Checked = false;
                textBox2.Text = "";
                textBoxUsername.Text = "";
                labelApiEnabled.Text = "";
                labelApiMembership.Text = "";
            }
            _updating = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            AccountInfo ai = comboBox1.SelectedItem as AccountInfo;
            if (ai != null)
            {
                AccountInfoSettings.Remove(ai);
                comboBox1.Items.Remove(ai);
                comboBox1_SelectedIndexChanged(this, EventArgs.Empty);
                textBox1_TextChanged(this, EventArgs.Empty);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            AccountInfo ai = comboBox1.SelectedItem as AccountInfo;
            if (ai != null)
            {
                if (ai.GeocachingComAccount != null)
                {
                    ai.GeocachingComAccount.MemberType = "";
                    ai.GeocachingComAccount.MemberTypeId = 0;
                    ai.GeocachingComAccount.APIToken = "";

                    comboBox1_SelectedIndexChanged(this, EventArgs.Empty);
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!_updating)
            {
                AccountInfo ai = comboBox1.SelectedItem as AccountInfo;
                if (ai != null)
                {
                    ai.SwitchGeocachingComAccount = checkBox1.Checked;
                }
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (!_updating)
            {
                AccountInfo ai = comboBox1.SelectedItem as AccountInfo;
                if (ai != null)
                {
                    ai.SwitchDatabase = checkBox2.Checked;
                }
            }
        }

        private void textBoxUsername_TextChanged(object sender, EventArgs e)
        {
            if (!_updating)
            {
                AccountInfo ai = comboBox1.SelectedItem as AccountInfo;
                if (ai != null && ai.GeocachingComAccount!=null)
                {
                    ai.GeocachingComAccount.AccountName = textBoxUsername.Text;
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            AccountInfo ai = comboBox1.SelectedItem as AccountInfo;
            if (ai != null && ai.GeocachingComAccount != null)
            {
                ai.SaveSettings(_core);
                comboBox1_SelectedIndexChanged(this, EventArgs.Empty);
            }
        }
    }
}
