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
            numericUpDown1.Value = PluginSettings.Instance.MaxLogs;

            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GRABBEDIMG);
            this.checkBox1.Checked = PluginSettings.Instance.ExportGrabbedImages;
        }

        public void Apply()
        {
            PluginSettings.Instance.MaxLogs = (int)numericUpDown1.Value;
            PluginSettings.Instance.ExportGrabbedImages = this.checkBox1.Checked;
        }
    }
}
