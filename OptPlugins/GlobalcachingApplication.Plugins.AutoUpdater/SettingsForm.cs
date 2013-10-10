using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.AutoUpdater
{
    public partial class SettingsForm : Form
    {
        public const string STR_OK = "OK"
            ;
        public SettingsForm()
        {
            InitializeComponent();

            button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(Updater.ACTION_SHOW);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            settingsPanel1.Apply();
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}
