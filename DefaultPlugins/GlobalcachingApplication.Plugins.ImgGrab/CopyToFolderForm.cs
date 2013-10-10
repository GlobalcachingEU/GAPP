using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.ImgGrab
{
    public partial class CopyToFolderForm : Form
    {
        public const string STR_TITLE = "Copy images to folder";
        public const string STR_FOLDER = "Folder";
        public const string STR_DOWNLOAD = "Download before creating folder";
        public const string STR_NOTINDESCR = "Only images not in geocache description";
        public const string STR_OK = "OK";
        public const string STR_CLEAR = "Clear folder before copy";

        public CopyToFolderForm()
        {
            InitializeComponent();

            checkBox1.Checked = Properties.Settings.Default.DownloadBeforeCreate;
            checkBox2.Checked = Properties.Settings.Default.CopyNotInDescription;
            checkBox3.Checked = Properties.Settings.Default.ClearBeforeCopy;

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FOLDER);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DOWNLOAD);
            this.checkBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NOTINDESCR);
            this.checkBox3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CLEAR);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);

            folderBrowserDialog1.SelectedPath = Properties.Settings.Default.CreateFolderPath;
            textBox1.Text = Properties.Settings.Default.CreateFolderPath;
        }

        public CopyToFolderForm(string folder):this()
        {
            textBox1.Text = folder;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button2.Enabled = textBox1.Text.Length > 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog()== System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.DownloadBeforeCreate = checkBox1.Checked;
            Properties.Settings.Default.CopyNotInDescription = checkBox2.Checked;
            Properties.Settings.Default.ClearBeforeCopy = checkBox3.Checked;
            Properties.Settings.Default.CreateFolderPath = textBox1.Text;
            Properties.Settings.Default.Save();
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}
