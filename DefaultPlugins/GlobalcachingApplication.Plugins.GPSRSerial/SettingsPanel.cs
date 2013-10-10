using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.GPSRSerial
{
    public partial class SettingsPanel : UserControl
    {
        public SettingsPanel()
        {
            InitializeComponent();

            for (int i = 1; i < 5; i++)
            {
                comboBox1.Items.Add(string.Format("COM{0}", i));
            }
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf(Properties.Settings.Default.ComPort);
            numericUpDown1.Value = Properties.Settings.Default.BaudRate;
            comboBox2.Items.AddRange(Enum.GetNames(typeof(System.IO.Ports.Parity)));
            comboBox2.SelectedIndex = comboBox2.Items.IndexOf(Properties.Settings.Default.Parity);
            numericUpDown2.Value = Properties.Settings.Default.Databits;
            comboBox3.Items.AddRange(Enum.GetNames(typeof(System.IO.Ports.StopBits)));
            comboBox3.SelectedIndex = comboBox3.Items.IndexOf(Properties.Settings.Default.StopBits);
        }

        public void Apply()
        {
            if (comboBox1.SelectedItem != null)
            {
                Properties.Settings.Default.ComPort = comboBox1.SelectedItem.ToString();
            }
            if (comboBox2.SelectedItem != null)
            {
                Properties.Settings.Default.Parity = comboBox2.SelectedItem.ToString();
            }
            if (comboBox3.SelectedItem != null)
            {
                Properties.Settings.Default.StopBits = comboBox3.SelectedItem.ToString();
            }
            Properties.Settings.Default.BaudRate = (int)numericUpDown1.Value;
            Properties.Settings.Default.Databits = (int)numericUpDown2.Value;
            Properties.Settings.Default.Save();
        }
    }
}
