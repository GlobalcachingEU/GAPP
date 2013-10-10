using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Demo
{
    public partial class SettingsPanel : UserControl
    {
        public SettingsPanel()
        {
            InitializeComponent();

            numericUpDown1.Value = Properties.Settings.Default.Interval;
            checkBox1.Checked = Properties.Settings.Default.SelectedGeocachesOnly;
            checkBox2.Checked = Properties.Settings.Default.GetLoggerAvatar;
        }

        public void Apply()
        {
            Properties.Settings.Default.Interval = (int)numericUpDown1.Value;
            Properties.Settings.Default.SelectedGeocachesOnly = checkBox1.Checked;
            Properties.Settings.Default.GetLoggerAvatar = checkBox2.Checked;
            Properties.Settings.Default.Save();
        }

    }
}
