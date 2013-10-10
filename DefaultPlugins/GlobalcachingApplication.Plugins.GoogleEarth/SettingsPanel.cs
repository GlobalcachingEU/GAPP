using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.GoogleEarth
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_FLYTOSPEED = "Fly to speed";
        public const string STR_FIXEDVIEW = "Fixed view";
        public const string STR_TILT = "View angle";
        public const string STR_ALTITUDE = "View altitude";

        public SettingsPanel()
        {
            InitializeComponent();

            numericUpDown1.Value = (Decimal)Properties.Settings.Default.FlyToSpeed;
            numericUpDown2.Value = (Decimal)Properties.Settings.Default.TiltView;
            numericUpDown3.Value = (Decimal)Properties.Settings.Default.AltitudeView;
            checkBox1.Checked = Properties.Settings.Default.FixedView;

            label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FLYTOSPEED);
            label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FIXEDVIEW);
            label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TILT);
            label8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ALTITUDE);
        }

        public void Apply()
        {
            Properties.Settings.Default.FixedView = checkBox1.Checked;
            Properties.Settings.Default.FlyToSpeed = (double)numericUpDown1.Value;
            Properties.Settings.Default.TiltView = (int)numericUpDown2.Value;
            Properties.Settings.Default.AltitudeView = (int)numericUpDown3.Value;
            Properties.Settings.Default.Save();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDown2.Enabled = checkBox1.Checked;
            numericUpDown3.Enabled = checkBox1.Checked;
        }
    }
}
