using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.ActBuilder
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_SHOWCONNECTIONLABEL = "Show label on connection lines";

        public SettingsPanel()
        {
            InitializeComponent();

            checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(ActionBuilderForm.STR_DONOTSHOWAGAIN);
            checkBox1.Checked = !Properties.Settings.Default.ShowFlowCompletedMessage;

            checkBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHOWCONNECTIONLABEL);
            checkBox2.Checked = Properties.Settings.Default.ShowConnectionLabel;
        }

        public void Apply()
        {
            Properties.Settings.Default.ShowFlowCompletedMessage = !checkBox1.Checked;
            Properties.Settings.Default.ShowConnectionLabel = checkBox2.Checked;
            Properties.Settings.Default.Save();
        }
    }
}
