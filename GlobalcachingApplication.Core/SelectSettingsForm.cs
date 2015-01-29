using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Core
{
    public partial class SelectSettingsForm : Form
    {
        public SelectSettingsForm(Engine eng)
        {
            InitializeComponent();

            checkBox1.Checked = Properties.Settings.Default.EnablePluginDataPathAtStartup;

            try
            {
                //no check if valid

                listBox1.Items.Add(eng.PluginDataPath);
                string[] lst = eng.AvailablePluginDataPaths;
                if (lst != null && lst.Length > 0)
                {
                    listBox1.Items.AddRange((from a in lst where string.Compare(a, eng.PluginDataPath, true) != 0 select a).Distinct().ToArray());
                }
            }
            catch
            {
                //oeps
                //fall back to factory default
                string p = System.IO.Path.Combine(new string[] { System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GAPP" });
                Properties.Settings.Default.PluginDataPath = p.TrimEnd(new char[] { '\\', '/' });
                listBox1.Items.Add(eng.PluginDataPath);
            }
            listBox1.SelectedItem = eng.PluginDataPath;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.EnablePluginDataPathAtStartup = checkBox1.Checked;
            if (listBox1.SelectedItem != null)
            {
                Properties.Settings.Default.PluginDataPath = listBox1.SelectedItem.ToString();
            }
            Properties.Settings.Default.Save();
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = listBox1.SelectedIndex >= 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = Properties.Settings.Default.PluginDataPath;
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Properties.Settings.Default.PluginDataPath = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.Save();
                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.PluginDataPath = "";
            Properties.Settings.Default.Save();
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}
