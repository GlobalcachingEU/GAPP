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

            checkBox1.Checked = PluginSettings.Instance.DeselectGeocacheAfterUpdate;
            checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DESELECT);
            label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXTRADELAY);
            label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXTRADELAYLOGS);
            label5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAXLOGS);
            numericUpDown1.Value = PluginSettings.Instance.AdditionalDelayBetweenImageImport;
            numericUpDown2.Value = PluginSettings.Instance.AdditionalDelayBetweenLogImport;
            numericUpDown3.Value = PluginSettings.Instance.UpdateLogsMaxLogCount;
        }

        public void Apply()
        {
            PluginSettings.Instance.DeselectGeocacheAfterUpdate = checkBox1.Checked;
            PluginSettings.Instance.AdditionalDelayBetweenImageImport = (int)numericUpDown1.Value;
            PluginSettings.Instance.AdditionalDelayBetweenLogImport = (int)numericUpDown2.Value;
            PluginSettings.Instance.UpdateLogsMaxLogCount = (int)numericUpDown3.Value;
        }
    }
}
