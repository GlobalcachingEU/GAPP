using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace RestoreDefaultSettings
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete all your settings and data of GAPP?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
            {
                string baseFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                try
                {
                    bool settingsFolderFound = false;
                    bool defaultFolderFound = false;
                    string settingsFolder = Path.Combine(baseFolder, "Globalcaching.eu");
                    if (Directory.Exists(settingsFolder))
                    {
                        Directory.Delete(settingsFolder, true);
                        settingsFolderFound = true;
                    }
                    string defaultFolder = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GAPP");
                    if (Directory.Exists(defaultFolder))
                    {
                        Directory.Delete(defaultFolder, true);
                        defaultFolderFound = true;
                    }
                    if (settingsFolderFound && defaultFolderFound)
                    {
                        MessageBox.Show("The settings and data folder has been deleted. You can now (re)start GAPP.", "Ready");
                    }
                    else
                    {
                        MessageBox.Show("One or both folders are missing. Not everything might have been removed.", "Ready");
                    }
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Failed");
                }
            }
            if (MessageBox.Show("Are you sure you want to delete all your settings and data of GAPP SF?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
            {
                string baseFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                try
                {
                    bool settingsFolderFound = false;
                    bool defaultFolderFound = false;
                    string settingsFolder = Path.Combine(baseFolder, "Globalcaching");
                    if (Directory.Exists(settingsFolder))
                    {
                        Directory.Delete(settingsFolder, true);
                        settingsFolderFound = true;
                    }
                    string defaultFolder = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GAPPSF");
                    if (Directory.Exists(defaultFolder))
                    {
                        Directory.Delete(defaultFolder, true);
                        defaultFolderFound = true;
                    }
                    if (settingsFolderFound && defaultFolderFound)
                    {
                        MessageBox.Show("The settings and data folder has been deleted. You can now (re)start GAPP SF.", "Ready");
                    }
                    else
                    {
                        MessageBox.Show("One or both folders are missing. Not everything might have been removed.", "Ready");
                    }
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Failed");
                }
            }
        }
    }
}
