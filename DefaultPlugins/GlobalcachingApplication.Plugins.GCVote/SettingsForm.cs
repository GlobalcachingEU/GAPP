using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.GCVote
{
    public partial class SettingsForm : Form
    {
        private SettingsPanel settingsPanel1 = null;

        public SettingsForm()
        {
            InitializeComponent();
        }

        public SettingsForm(Framework.Interfaces.ICore core)
            : this()
        {
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(DashboardForm.STR_TITLE);

            settingsPanel1 = new SettingsPanel(core);
            Controls.Add(settingsPanel1);
            settingsPanel1.Location = new Point(0, 0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            settingsPanel1.Apply();
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}
