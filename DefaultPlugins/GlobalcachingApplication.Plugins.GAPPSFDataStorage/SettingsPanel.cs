using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.GAPPSFDataStorage
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_MAXCOUNT = "Keep maximum backups (0=no maximum)";

        public SettingsPanel()
        {
            InitializeComponent();

            numericUpDown2.Value = Properties.Settings.Default.BackupKeepMaxCount;

            label3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAXCOUNT);
        }

        public void Apply()
        {
            Properties.Settings.Default.BackupKeepMaxCount = (int)numericUpDown2.Value;
            Properties.Settings.Default.Save();
        }
    }
}
