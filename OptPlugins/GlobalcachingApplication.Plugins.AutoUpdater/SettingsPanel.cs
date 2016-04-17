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
        public const string STR_COUNTRY = "Country";
        public const string STR_STATE = "State";
        public const string STR_LOCATION = "Location";
        public const string STR_RADIUS = "Radius";
        public const string STR_KM = "km";
        public const string STR_CLEAR = "Clear";

        public SettingsPanel()
        {
            InitializeComponent();

            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHOWDIALOG);
                checkBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AUTODOWNLOAD);
                groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_UPDATEANDDOWNLOAD);
                label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOCATION);
                label5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RADIUS);
                label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_KM);
                button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CLEAR);

                checkBox1.Checked = PluginSettings.Instance.ShowSettingsDialog;
                checkBox2.Checked = PluginSettings.Instance.AutomaticDownloadGeocaches;
                textBox1.Text = PluginSettings.Instance.FromLocation ?? "";
                numericUpDown1.Value = PluginSettings.Instance.FromLocationRadius;

                var l = new Label();
                l.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COUNTRY);
                flowLayoutPanel1.Controls.Add(l);
                var cntrs = PluginSettings.Instance.AreaInfoItems.Where(x => x.Level=="Land");
                foreach (var item in cntrs)
                {
                    var c = new CheckBox();
                    c.Tag = item;
                    c.AutoSize = true;
                    c.Text = item.ToString();
                    c.Checked = PluginSettings.Instance.GetUpdateAreaInfo(item);
                    flowLayoutPanel1.Controls.Add(c);
                }

                l = new Label();
                l.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_STATE);
                flowLayoutPanel1.Controls.Add(l);
                var states = PluginSettings.Instance.AreaInfoItems.Where(x => x.Level == "Provincie");
                foreach (var item in states)
                {
                    var c = new CheckBox();
                    c.Tag = item;
                    c.AutoSize = true;
                    c.Text = item.ToString();
                    c.Checked = PluginSettings.Instance.GetUpdateAreaInfo(item);
                    flowLayoutPanel1.Controls.Add(c);
                }

            }
        }

        public void Apply()
        {
            PluginSettings.Instance.ShowSettingsDialog = checkBox1.Checked;
            PluginSettings.Instance.AutomaticDownloadGeocaches = checkBox2.Checked;
            PluginSettings.Instance.FromLocation = textBox1.Text;
            PluginSettings.Instance.FromLocationRadius = (int)numericUpDown1.Value;
            foreach (var c in flowLayoutPanel1.Controls)
            {
                if (c is CheckBox)
                {
                    var cb = c as CheckBox;
                    if (cb.Tag is AreaItemInfo)
                    {
                        PluginSettings.Instance.SetUpdateAreaInfo(cb.Tag as AreaItemInfo, cb.Checked);
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(Updater._core, string.IsNullOrEmpty(textBox1.Text) ? Updater._core.CenterLocation : Utils.Conversion.StringToLocation(textBox1.Text)))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textBox1.Text = Utils.Conversion.GetCoordinatesPresentation(dlg.Result);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }
    }
}
