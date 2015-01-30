using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Utils.Dialogs
{
    public partial class GeocachesIgnoredMessageForm : Form
    {
        public const string STR_WARNING = "Warning";
        public const string STR_XIGNORED = "{0} geocaches have been ignored during import.";
        public const string STR_ACTION_EDIT = "Edit ignored geocaches";
        public const string STR_OK = "OK";

        private Framework.Interfaces.ICore _core = null;

        public GeocachesIgnoredMessageForm()
        {
            InitializeComponent();
        }

        public GeocachesIgnoredMessageForm(Framework.Interfaces.ICore core, int cnt): this()
        {
            _core = core;

            this.Text = LanguageSupport.Instance.GetTranslation(STR_WARNING);
            this.label1.Text = string.Format(LanguageSupport.Instance.GetTranslation(STR_XIGNORED), cnt);
            this.button1.Text = LanguageSupport.Instance.GetTranslation(STR_ACTION_EDIT);
            this.button2.Text = LanguageSupport.Instance.GetTranslation(STR_OK);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            await PluginSupport.ExecuteDefaultActionAsync(_core, "GlobalcachingApplication.Plugins.IgnoreGeocaches.Editor");
            Close();
        }
    }
}
