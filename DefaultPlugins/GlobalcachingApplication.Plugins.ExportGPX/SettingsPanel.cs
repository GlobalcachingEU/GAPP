using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.ExportGPX
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_GPXVERSION = "GPX version";
        public const string STR_ZIPFILE = "Zip file";
        public const string STR_USENAME = "Use name and not geocache code";
        public const string STR_MAXNAMELENGTH = "Maximum geocache name length";
        public const string STR_MINSTARTNAME = "Minimum start of name length";
        public const string STR_ADDWPTTODESCR = "Add additional waypoints to description";
        public const string STR_USEHINTSDESCR = "Use the hints for description";
        public const string STR_INCLNOTES = "Include notes in description";
        public const string STR_ADDWAYPOINTS = "Add waypoints";
        public const string STR_EXTRACOORDNAMEPREFIX = "Extra coord. name prefix";
        public const string STR_EXTRAINFO = "Add extra information to description";
        public const string STR_MAXLOGS = "Maximum number of logs";
        public const string STR_MAXGPXINGGZ = "Maximum size GPX in GGZ (bytes)";

        public SettingsPanel()
        {
            InitializeComponent();

            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GPXVERSION);
            this.checkBoxZipFile.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ZIPFILE);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAXNAMELENGTH);
            this.label5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MINSTARTNAME);
            this.label8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXTRACOORDNAMEPREFIX);
            this.checkBox3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_USENAME);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDWPTTODESCR);
            this.checkBox5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_USEHINTSDESCR);
            this.checkBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_INCLNOTES);
            this.checkBox4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDWAYPOINTS);
            this.checkBox6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXTRAINFO);
            this.label10.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAXLOGS);
            this.label12.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAXGPXINGGZ);

            numericUpDown1.Value = PluginSettings.Instance.MaxGeocacheNameLength;
            numericUpDown2.Value = PluginSettings.Instance.MinStartOfGeocacheName;
            numericUpDown3.Value = PluginSettings.Instance.MaximumNumberOfLogs;
            numericUpDown4.Value = PluginSettings.Instance.MaximumSizeGpxInGgz;
            checkBox3.Checked = PluginSettings.Instance.UseNameAndNotCode;
            checkBox1.Checked = PluginSettings.Instance.AddWaypointsToDescription;
            checkBox2.Checked = PluginSettings.Instance.AddFieldNotesToDescription;
            checkBox4.Checked = PluginSettings.Instance.AddWaypoints;
            checkBox6.Checked = PluginSettings.Instance.AddExtraInfoToDescription;
            checkBoxZipFile.Checked = PluginSettings.Instance.ZipFile;
            textBox1.Text = PluginSettings.Instance.CorrectedNamePrefix ?? "";
            checkBox5.Checked = PluginSettings.Instance.UseHintsForDescription;
            comboBox1.Items.Add(Utils.GPXGenerator.V100);
            comboBox1.Items.Add(Utils.GPXGenerator.V101);
            comboBox1.Items.Add(Utils.GPXGenerator.V102);
            if (!string.IsNullOrEmpty(PluginSettings.Instance.GPXVersionStr))
            {
                comboBox1.SelectedItem = Version.Parse(PluginSettings.Instance.GPXVersionStr);
            }
            else
            {
                comboBox1.SelectedIndex = 1;
            }
        }

        public void Apply()
        {
            PluginSettings.Instance.UseNameAndNotCode = checkBox3.Checked;
            PluginSettings.Instance.AddWaypointsToDescription = checkBox1.Checked;
            PluginSettings.Instance.MaxGeocacheNameLength = (int)numericUpDown1.Value;
            PluginSettings.Instance.MinStartOfGeocacheName = (int)numericUpDown2.Value;
            PluginSettings.Instance.MaximumNumberOfLogs = (int)numericUpDown3.Value;
            PluginSettings.Instance.MaximumSizeGpxInGgz = (int)numericUpDown4.Value;
            PluginSettings.Instance.ZipFile = checkBoxZipFile.Checked;
            PluginSettings.Instance.UseHintsForDescription = checkBox5.Checked;
            PluginSettings.Instance.AddFieldNotesToDescription = checkBox2.Checked;
            PluginSettings.Instance.AddWaypoints = checkBox4.Checked;
            PluginSettings.Instance.AddExtraInfoToDescription = checkBox6.Checked;
            PluginSettings.Instance.CorrectedNamePrefix = textBox1.Text;
            if (comboBox1.SelectedItem as Version == null)
            {
                PluginSettings.Instance.GPXVersionStr = "";
            }
            else
            {
                PluginSettings.Instance.GPXVersionStr = (comboBox1.SelectedItem as Version).ToString();
            }
        }
    }
}
