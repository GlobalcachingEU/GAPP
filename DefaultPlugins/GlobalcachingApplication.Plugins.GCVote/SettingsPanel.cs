using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.GCVote
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_USERNAME = "User name";
        public const string STR_PASSWORD = "Password";
        public const string STR_LOADATSTARTUP = "Load at startup";

        public SettingsPanel()
        {
            InitializeComponent();

        }

        public SettingsPanel(Framework.Interfaces.ICore core)
            : this()
        {

            if (string.IsNullOrEmpty(Properties.Settings.Default.GCVoteUsername))
            {
                textBox1.Text = core.GeocachingComAccount.AccountName ?? "";
            }
            else
            {
                textBox1.Text = Properties.Settings.Default.GCVoteUsername;
            }
            textBox2.Text = Properties.Settings.Default.GCVotePassword ?? "";
            checkBox1.Checked = Properties.Settings.Default.ActivateAtAtartup;

            label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_USERNAME);
            label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PASSWORD);
            label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOADATSTARTUP);
        }

        public void Apply()
        {
            Properties.Settings.Default.GCVoteUsername = textBox1.Text;
            Properties.Settings.Default.GCVotePassword = textBox2.Text;
            Properties.Settings.Default.ActivateAtAtartup = checkBox1.Checked;
            Properties.Settings.Default.Save();
        }
    }
}
