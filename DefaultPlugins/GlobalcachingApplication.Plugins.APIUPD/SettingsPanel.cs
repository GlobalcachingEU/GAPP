using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.APIUPD
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_DESELECT = "Deselect geocaches after update";
        public const string STR_EXTRADELAY = "Extra delay between get geocache images call";
        public const string STR_EXTRADELAYLOGS = "Extra delay between get geocache logs call";
        public const string STR_MAXLOGS = "Maximum number of logs with update (0=all)";

        public SettingsPanel()
        {
            InitializeComponent();

            checkBox1.Checked = Properties.Settings.Default.DeselectGeocacheAfterUpdate;
            checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DESELECT);
            label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXTRADELAY);
            label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXTRADELAYLOGS);
            label5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAXLOGS);
            numericUpDown1.Value = Properties.Settings.Default.AdditionalDelayBetweenImageImport;
            numericUpDown2.Value = Properties.Settings.Default.AdditionalDelayBetweenLogImport;
            numericUpDown3.Value = Properties.Settings.Default.UpdateLogsMaxLogCount;
        }

        public void Apply()
        {
            Properties.Settings.Default.DeselectGeocacheAfterUpdate = checkBox1.Checked;
            Properties.Settings.Default.AdditionalDelayBetweenImageImport = (int)numericUpDown1.Value;
            Properties.Settings.Default.AdditionalDelayBetweenLogImport = (int)numericUpDown2.Value;
            Properties.Settings.Default.UpdateLogsMaxLogCount = (int)numericUpDown3.Value;
            Properties.Settings.Default.Save();
        }
    }
}
