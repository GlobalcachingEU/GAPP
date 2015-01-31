using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.iGeoKnife
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_MAXLOGS = "Maximum logs";

        public SettingsPanel()
        {
            InitializeComponent();

            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAXLOGS);
            numericUpDown1.Value = PluginSettings.Instance.MaxLogs;
        }
    }
}
