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

            numericUpDown1.Value = Properties.Settings.Default.MaxGeocacheNameLength;
            numericUpDown2.Value = Properties.Settings.Default.MinStartOfGeocacheName;
            checkBox3.Checked = Properties.Settings.Default.UseNameAndNotCode;
            checkBox1.Checked = Properties.Settings.Default.AddWaypointsToDescription;
            checkBox2.Checked = Properties.Settings.Default.AddFieldnotesToDescription;
            checkBox4.Checked = Properties.Settings.Default.AddWaypoints;
            checkBox6.Checked = Properties.Settings.Default.AddExtraInfoToDescription;
            checkBoxZipFile.Checked = Properties.Settings.Default.ZipFile;
            textBox1.Text = Properties.Settings.Default.CorrectedNamePrefix ?? "";
            checkBox5.Checked = Properties.Settings.Default.UseHintsForDescription;
            comboBox1.Items.Add(Utils.GPXGenerator.V100);
            comboBox1.Items.Add(Utils.GPXGenerator.V101);
            comboBox1.Items.Add(Utils.GPXGenerator.V102);
            if (!string.IsNullOrEmpty(Properties.Settings.Default.GPXVersionStr))
            {
                comboBox1.SelectedItem = Version.Parse(Properties.Settings.Default.GPXVersionStr);
            }
            else
            {
                comboBox1.SelectedIndex = 1;
            }
        }

        public void Apply()
        {
            Properties.Settings.Default.UseNameAndNotCode = checkBox3.Checked;
            Properties.Settings.Default.AddWaypointsToDescription = checkBox1.Checked;
            Properties.Settings.Default.MaxGeocacheNameLength = (int)numericUpDown1.Value;
            Properties.Settings.Default.MinStartOfGeocacheName = (int)numericUpDown2.Value;
            Properties.Settings.Default.ZipFile = checkBoxZipFile.Checked;
            Properties.Settings.Default.UseHintsForDescription = checkBox5.Checked;
            Properties.Settings.Default.AddFieldnotesToDescription = checkBox2.Checked;
            Properties.Settings.Default.AddWaypoints = checkBox4.Checked;
            Properties.Settings.Default.AddExtraInfoToDescription = checkBox6.Checked;
            Properties.Settings.Default.CorrectedNamePrefix = textBox1.Text;
            if (comboBox1.SelectedItem as Version == null)
            {
                Properties.Settings.Default.GPXVersionStr = "";
            }
            else
            {
                Properties.Settings.Default.GPXVersionStr = (comboBox1.SelectedItem as Version).ToString();
            }
            Properties.Settings.Default.Save();
        }
    }
}
