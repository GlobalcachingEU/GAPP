using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GlobalcachingApplication.Plugins.AccountSwitcher
{
    public partial class SettingsFolderForm : Form
    {
        public const string STR_TITLE = "Settings folder";
        public const string STR_SAMEASCURRENT = "The folder is the active settings folder";
        public const string STR_COPYINGFOLDER = "Copying current settings folder to target settings folder...";
        public const string STR_CLEANINGFOLDER = "Clearing target settings folder...";
        public const string STR_SETTINGSFOLDER = "Settings folder";
        public const string STR_TARGETSETTINGSFOLDER = "Target settings folder";
        public const string STR_CURRENT = "Current";
        public const string STR_AVAILABLE = "Available";
        public const string STR_ENABLESTARTUP = "Enable folder selection at startup";
        public const string STR_FOLDER = "Folder";
        public const string STR_COPYCURRENT = "Copy current settings to selected folder";
        public const string STR_COPYDEFAULT = "Create default settings in selected folder";
        public const string STR_SWITCH = "Switch to selected folder";
        public const string STR_OK = "OK";
        public const string STR_ASKDELETEFILES = "The folder is not empty.\r\nAre you sure you want to delete the existing files?";

        private Framework.Interfaces.ICore _core = null;
        private SettingsFolder _plugin = null;

        public SettingsFolderForm()
        {
            InitializeComponent();
        }

        public SettingsFolderForm(SettingsFolder plugin, Framework.Interfaces.ICore core)
            : this()
        {
            _core = core;
            _plugin = plugin;

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SETTINGSFOLDER);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TARGETSETTINGSFOLDER);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CURRENT);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AVAILABLE);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ENABLESTARTUP);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FOLDER);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COPYCURRENT);
            this.button4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COPYDEFAULT);
            this.button5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SWITCH);
            this.button8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);

            textBox1.Text = core.PluginDataPath;
            string[] lst = core.AvailablePluginDataPaths;
            listBox1.Items.AddRange((from a in lst where string.Compare(a, core.PluginDataPath, true) != 0 select a).ToArray());
            checkBox1.Checked = core.EnablePluginDataPathAtStartup;
        }

        private bool IsSubOrParentFolder(string p1, string p2)
        {
            bool result = (p1.Length!=p2.Length &&
                    (p1.StartsWith(p2, StringComparison.InvariantCultureIgnoreCase) ||
                    p2.StartsWith(p1, StringComparison.InvariantCultureIgnoreCase)));
            if (result && string.Compare(p1,p2,true)!=0)
            {
                //check if parant is the same
                //if yes, then false possitive
                try
                {
                    DirectoryInfo di1 = new DirectoryInfo(p1);
                    DirectoryInfo di2 = new DirectoryInfo(p2);
                    result = (di1.Parent.FullName.ToLower() != di2.Parent.FullName.ToLower());
                }
                catch
                {
                }
            }
            return result;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text.Length>0 && string.Compare(textBox1.Text, textBox2.Text, true) != 0)
            {
                //one cannot be a sub of the other
                if ((from string sf in listBox1.Items where IsSubOrParentFolder(sf,textBox2.Text)  select sf).Count()==0 &&
                    !IsSubOrParentFolder(textBox1.Text, textBox2.Text))
                {
                    button3.Enabled = true;
                    button4.Enabled = true;

                    //switch can only be done if a valid folder
                    //right now, we just check if the GlobalcachingApplication.Core.Properties.Settings.xml is present
                    button5.Enabled = _plugin.IsValidSettingsFolder(textBox2.Text);
                }
                else
                {
                    button3.Enabled = false;
                    button4.Enabled = false;
                    button5.Enabled = false;
                }
            }
            else
            {
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(textBox1.Text);
            }
            catch
            {
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = textBox1.Text;
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string s = folderBrowserDialog1.SelectedPath.TrimEnd(new char[] { '\\', '/' });
                if (s.Length > 2)
                {
                    if (string.Compare(textBox1.Text, s,true)!=0 &&
                        (from string sf in listBox1.Items where IsSubOrParentFolder(sf, s) select sf).Count() == 0 &&
                        !IsSubOrParentFolder(textBox1.Text, s))
                    {
                        textBox2.Text = s;
                        if (_plugin.IsValidSettingsFolder(s))
                        {
                            string f = (from string sf in listBox1.Items where string.Compare(sf, s, true) == 0 select sf).FirstOrDefault();
                            if (string.IsNullOrEmpty(f))
                            {
                                _plugin.AddToMenu(s);
                                listBox1.Items.Add(s);
                                listBox1.SelectedIndex = listBox1.Items.IndexOf(s);
                                listBox1_SelectedIndexChanged(this, EventArgs.Empty);
                                savePathList();
                                textBox2_TextChanged(this, EventArgs.Empty);
                            }
                        }
                    }
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _plugin.SwitchSettingsFolder(textBox2.Text);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            _core.EnablePluginDataPathAtStartup = checkBox1.Checked;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                button6.Enabled = true;
                button7.Enabled = true;

                textBox2.Text = listBox1.Items[listBox1.SelectedIndex].ToString();
            }
            else
            {
                button6.Enabled = false;
                button7.Enabled = false;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                try
                {
                    System.Diagnostics.Process.Start(listBox1.Items[listBox1.SelectedIndex].ToString());
                }
                catch
                {
                }
            }
        }

        private void savePathList()
        {
            string[] lst = new string[1 + listBox1.Items.Count];
            lst[0] = _core.PluginDataPath;
            if (listBox1.Items.Count > 0)
            {
                listBox1.Items.CopyTo(lst, 1);
            }

            _core.AvailablePluginDataPaths = lst;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                _plugin.RemoveFromMenu(listBox1.Items[listBox1.SelectedIndex].ToString());
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                savePathList();
            }
        }

        private bool checkAndCleanSettingsFolder(string folder)
        {
            bool result = false;
            try
            {
                if (folder.ToLower() != _core.PluginDataPath.ToLower())
                {
                    if (System.IO.Directory.Exists(folder))
                    {
                        //either clean and empty or valid settings folder
                        string[] dirs = Directory.GetDirectories(folder);
                        string[] fls = Directory.GetFiles(folder);
                        if (dirs.Length == 0 && fls.Length == 0)
                        {
                            result = true;
                        }
                        else if (_plugin.IsValidSettingsFolder(folder))
                        {
                            if (System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_ASKDELETEFILES), Utils.LanguageSupport.Instance.GetTranslation(SettingsFolder.STR_WARNING), System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
                            {
                                using (Utils.ProgressBlock prg = new Utils.ProgressBlock(_plugin, STR_CLEANINGFOLDER, STR_CLEANINGFOLDER, 1, 0))
                                {
                                    Application.DoEvents();
                                    //clean
                                    foreach (string s in fls)
                                    {
                                        File.Delete(s);
                                    }
                                    foreach (string s in dirs)
                                    {
                                        Directory.Delete(s, true);
                                    }
                                }
                                result = true;
                            }
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(SettingsFolder.STR_FOLDERNOTVALID), Utils.LanguageSupport.Instance.GetTranslation(SettingsFolder.STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(SettingsFolder.STR_FOLDERNOTEXIST), Utils.LanguageSupport.Instance.GetTranslation(SettingsFolder.STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_SAMEASCURRENT), Utils.LanguageSupport.Instance.GetTranslation(SettingsFolder.STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message, Utils.LanguageSupport.Instance.GetTranslation(SettingsFolder.STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            return result;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length > 0)
            {
                string folder = textBox2.Text;
                //only if folder is empty or is a valid settings folder
                if (checkAndCleanSettingsFolder(folder))
                {
                    //copy existing data
                    bool ok;
                    using (Utils.ProgressBlock prg = new Utils.ProgressBlock(_plugin, STR_COPYINGFOLDER, STR_COPYINGFOLDER, 1, 0))
                    {
                        Application.DoEvents();
                        List<Framework.Interfaces.IPlugin> p = _core.GetPlugin(Framework.PluginType.InternalStorage);
                        if (p != null && p.Count == 1)
                        {
                            (p[0] as Framework.Interfaces.IPluginInternalStorage).StartReleaseForCopy();
                        }
                        ok = DirectoryCopy(_core.PluginDataPath, folder, true);
                        if (p != null && p.Count == 1)
                        {
                            (p[0] as Framework.Interfaces.IPluginInternalStorage).EndReleaseForCopy();
                        }
                    }
                    if (ok)
                    {
                        if (_core.CreateSettingsInFolder(folder, true))
                        {
                            //add it to the list if not already exists
                            string f = (from string s in listBox1.Items where string.Compare(s, folder, true) == 0 select s).FirstOrDefault();
                            if (string.IsNullOrEmpty(f))
                            {
                                _plugin.AddToMenu(folder);
                                listBox1.Items.Add(folder);
                                listBox1.SelectedIndex = listBox1.Items.IndexOf(folder);
                                listBox1_SelectedIndexChanged(this, EventArgs.Empty);
                                savePathList();
                                textBox2_TextChanged(this, EventArgs.Empty);
                            }
                        }
                    }
                }
            }
        }

        private static bool DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            bool result = false;

            try
            {
                DirectoryInfo dir = new DirectoryInfo(sourceDirName);
                DirectoryInfo[] dirs = dir.GetDirectories();

                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                }

                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string temppath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(temppath, false);
                }

                if (copySubDirs)
                {
                    foreach (DirectoryInfo subdir in dirs)
                    {
                        string temppath = Path.Combine(destDirName, subdir.Name);
                        DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                    }
                }
                result = true;
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message, Utils.LanguageSupport.Instance.GetTranslation(SettingsFolder.STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            return result;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length > 0)
            {
                string folder = textBox2.Text;
                //only if folder is empty or is a valid settings folder
                if (checkAndCleanSettingsFolder(folder))
                {
                    //copy existing data
                    if (_core.CreateSettingsInFolder(folder, false))
                    {
                        //add it to the list if not already exists
                        string f = (from string s in listBox1.Items where string.Compare(s, folder, true) == 0 select s).FirstOrDefault();
                        if (string.IsNullOrEmpty(f))
                        {
                            _plugin.AddToMenu(folder);
                            listBox1.Items.Add(folder);
                            listBox1.SelectedIndex = listBox1.Items.IndexOf(folder);
                            listBox1_SelectedIndexChanged(this, EventArgs.Empty);
                            savePathList();
                            textBox2_TextChanged(this, EventArgs.Empty);
                        }
                    }
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            string p = System.IO.Path.Combine(new string[] { System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GAPP" });
            string s = p.TrimEnd(new char[] { '\\', '/' });
            if (s.Length > 2)
            {
                if (string.Compare(textBox1.Text, s, true) != 0 &&
                    (from string sf in listBox1.Items where IsSubOrParentFolder(sf, s) select sf).Count() == 0 &&
                    !IsSubOrParentFolder(textBox1.Text, s))
                {
                    textBox2.Text = s;
                    if (_plugin.IsValidSettingsFolder(s))
                    {
                        string f = (from string sf in listBox1.Items where string.Compare(sf, s, true) == 0 select sf).FirstOrDefault();
                        if (string.IsNullOrEmpty(f))
                        {
                            _plugin.AddToMenu(s);
                            listBox1.Items.Add(s);
                            listBox1.SelectedIndex = listBox1.Items.IndexOf(s);
                            listBox1_SelectedIndexChanged(this, EventArgs.Empty);
                            savePathList();
                            textBox2_TextChanged(this, EventArgs.Empty);
                        }
                    }
                }
            }
        }
    }
}
