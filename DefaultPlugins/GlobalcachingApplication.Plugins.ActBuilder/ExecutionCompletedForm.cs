using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.ActBuilder
{
    public partial class ExecutionCompletedForm : Form
    {
        public ExecutionCompletedForm()
        {
            InitializeComponent();

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(ActionBuilderForm.STR_TITLE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(ActionBuilderForm.STR_FLOWEXECUTED);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(ActionBuilderForm.STR_OK);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(ActionBuilderForm.STR_DONOTSHOWAGAIN);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowFlowCompletedMessage = !checkBox1.Checked;
            Properties.Settings.Default.Save();
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}
