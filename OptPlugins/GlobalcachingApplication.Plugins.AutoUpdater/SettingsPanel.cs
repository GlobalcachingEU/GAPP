using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.AutoUpdater
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_SHOWDIALOG = "Show settings dialog before start";
        public const string STR_AUTODOWNLOAD = "Automatic download new geocaches (no prompt)";
        public const string STR_UPDATEANDDOWNLOAD = "Update and download caches for";
        public const string STR_NL = "Netherlands";
        public const string STR_BE = "Belgium";
        public const string STR_LU = "Luxembourg";
        public const string STR_IT = "Italy";

        public SettingsPanel()
        {
            InitializeComponent();

            checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHOWDIALOG);
            checkBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AUTODOWNLOAD);
            groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_UPDATEANDDOWNLOAD);
            checkBox3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NL);
            checkBox4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_BE);
            checkBox5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LU);
            checkBox6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IT);

            checkBox1.Checked = Properties.Settings.Default.ShowSettingsDialog;
            checkBox2.Checked = Properties.Settings.Default.AutomaticDownloadGeocaches;
            checkBox3.Checked = Properties.Settings.Default.UpdateNL;
            checkBox4.Checked = Properties.Settings.Default.UpdateBE;
            checkBox5.Checked = Properties.Settings.Default.UpdateLU;
            checkBox6.Checked = Properties.Settings.Default.UpdateIT;
        }

        public void Apply()
        {
            Properties.Settings.Default.ShowSettingsDialog = checkBox1.Checked;
            Properties.Settings.Default.AutomaticDownloadGeocaches = checkBox2.Checked;
            Properties.Settings.Default.UpdateNL = checkBox3.Checked;
            Properties.Settings.Default.UpdateBE = checkBox4.Checked;
            Properties.Settings.Default.UpdateLU = checkBox5.Checked;
            Properties.Settings.Default.UpdateIT = checkBox6.Checked;
            Properties.Settings.Default.Save();
        }
    }
}
