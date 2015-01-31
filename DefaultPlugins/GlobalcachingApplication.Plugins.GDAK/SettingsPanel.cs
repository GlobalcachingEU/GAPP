using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.GDAK
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_MAXLOGS = "Maximum logs";
        public const string STR_GRABBEDIMG = "Export grabbed images";
        public const string STR_MAXFILESINFOLDER = "Max. in folder";
        public const string STR_MAXFILESINFOLDERNULL = "(0 = unlimited)";

        public SettingsPanel()
        {
            InitializeComponent();

            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAXLOGS);
            numericUpDown1.Value = PluginSettings.Instance.MaxLogs;

            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GRABBEDIMG);
            this.checkBox1.Checked = PluginSettings.Instance.ExportGrabbedImages;

            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAXFILESINFOLDER);
            this.label5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAXFILESINFOLDERNULL);
            numericUpDown2.Value = PluginSettings.Instance.MaxFilesInFolder;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDown2.Enabled = checkBox1.Checked;
        }
    }
}
