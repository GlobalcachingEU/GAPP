using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public partial class SettingsForm : Form
    {
        public const string STR_OK = "OK";
        public const string STR_TITLE = "Settings";

        SettingsPanel _sp = null;

        public SettingsForm()
        {
            InitializeComponent();
        }

        public SettingsForm(Framework.Interfaces.ICore core)
            : this()
        {
            _sp = new SettingsPanel(core);
            _sp.Location = new Point(12, 12);
            this.Controls.Add(_sp);

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _sp.Apply();
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}
