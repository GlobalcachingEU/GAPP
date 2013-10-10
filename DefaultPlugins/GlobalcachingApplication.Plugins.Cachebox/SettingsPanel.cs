using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Cachebox
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_MAXLOGS = "Maximum logs";
        public const string STR_GRABBEDIMG = "Export grabbed images";

        public SettingsPanel()
        {
            InitializeComponent();

            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAXLOGS);
            numericUpDown1.Value = Properties.Settings.Default.MaxLogs;

            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GRABBEDIMG);
            this.checkBox1.Checked = Properties.Settings.Default.ExportGrabbedImages;
        }

        public void Apply()
        {
            Properties.Settings.Default.MaxLogs = (int)numericUpDown1.Value;
            Properties.Settings.Default.ExportGrabbedImages = this.checkBox1.Checked;
            Properties.Settings.Default.Save();
        }
    }
}
