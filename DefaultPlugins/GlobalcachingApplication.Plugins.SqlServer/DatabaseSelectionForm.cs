using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.SqlServer
{
    public partial class DatabaseSelectionForm : Form
    {
        public const string STR_SERVER = "Server";
        public const string STR_DATABASE = "Database";
        public const string STR_TITLE = "Sql server database";
        public const string STR_NTIS = "Use Windows authentification";
        public const string STR_USERNAME = "User name";
        public const string STR_PASSWROD = "Password";

        public DatabaseSelectionForm()
        {
            InitializeComponent();

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SERVER);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DATABASE);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_USERNAME);
            this.label8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PASSWROD);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NTIS);

            textBox1.Text = Properties.Settings.Default.SqlServer;
            textBox2.Text = Properties.Settings.Default.Database;
            textBox3.Text = Properties.Settings.Default.SqlServerUsername;
            textBox4.Text = Properties.Settings.Default.SqlServerPwd;
            checkBox1.Checked = Properties.Settings.Default.SqlServerUseIS;
        }

        public string SqlServer
        {
            get { return textBox1.Text; }
        }
        public string Database
        {
            get { return textBox2.Text; }
        }
        public string Username
        {
            get { return textBox3.Text; }
        }
        public string Password
        {
            get { return textBox4.Text; }
        }
        public bool IntegratedSecurity
        {
            get { return checkBox1.Checked; }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = (textBox1.Text.Length > 0 && textBox2.Text.Length > 0);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox3.Enabled = !checkBox1.Checked;
            textBox4.Enabled = !checkBox1.Checked;
        }
    }
}
