using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.GAPPDataStorage
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_BACKUPFOLDER = "Backup folder";
        public const string STR_MAXDAYS = "Remove older then days (0=don't remove)";
        public const string STR_MAXCOUNT = "Keep maximum backups (0=no maximum)";

        public SettingsPanel()
        {
            InitializeComponent();

            textBox1.Text = Properties.Settings.Default.BackupFolder ?? "";
            numericUpDown1.Value = Properties.Settings.Default.BackupKeepMaxDays;
            numericUpDown2.Value = Properties.Settings.Default.BackupKeepMaxCount;

            label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_BACKUPFOLDER);
            label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAXDAYS);
            label3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAXCOUNT);
        }

        public void Apply()
        {
            Properties.Settings.Default.BackupFolder = textBox1.Text;
            Properties.Settings.Default.BackupKeepMaxDays = (int)numericUpDown1.Value;
            Properties.Settings.Default.BackupKeepMaxCount = (int)numericUpDown2.Value;
            Properties.Settings.Default.Save();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog()== DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }
    }
}
