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

        public SettingsPanel()
        {
            InitializeComponent();

            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHOWDIALOG);
                checkBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AUTODOWNLOAD);
                groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_UPDATEANDDOWNLOAD);
                checkBox3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NL);
                checkBox4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_BE);
                checkBox5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LU);

                checkBox1.Checked = PluginSettings.Instance.ShowSettingsDialog;
                checkBox2.Checked = PluginSettings.Instance.AutomaticDownloadGeocaches;
                checkBox3.Checked = PluginSettings.Instance.UpdateNL;
                checkBox4.Checked = PluginSettings.Instance.UpdateBE;
                checkBox5.Checked = PluginSettings.Instance.UpdateLU;
            }
        }

        public void Apply()
        {
            PluginSettings.Instance.ShowSettingsDialog = checkBox1.Checked;
            PluginSettings.Instance.AutomaticDownloadGeocaches = checkBox2.Checked;
            PluginSettings.Instance.UpdateNL = checkBox3.Checked;
            PluginSettings.Instance.UpdateBE = checkBox4.Checked;
            PluginSettings.Instance.UpdateLU = checkBox5.Checked;
        }
    }
}
