using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GlobalcachingApplication.Plugins.GAPPDataStorage
{
    public partial class RestoreForm : Form
    {
        public const string STR_TITLE = "Restore";
        public const string STR_BACKUPFOLDER = "Backup folder";
        public const string STR_BACKUPS = "Backups";
        public const string STR_RESTOREFOLDER = "Restore folder";
        public const string STR_OK = "OK";
        public const string STR_FILE = "File";
        public const string STR_DATE = "Date";
        public const string STR_PATH = "Path";
        public const string STR_WARNING = "Warning";
        public const string STR_OVERWRITE = "This will overwrite an existing file\r\nOverwrite this file?";

        private InternalStorage _storage;

        public string RestoreFolder { get; private set; }
        public InternalStorage.BackupItem SelectedBackupItem { get; private set; }

        public RestoreForm()
        {
            InitializeComponent();
        }

        public RestoreForm(InternalStorage storage):this()
        {
            _storage = storage;

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_BACKUPFOLDER);
            label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_BACKUPS);
            label3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RESTOREFOLDER);
            button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);
            listView1.Columns[0].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FILE);
            listView1.Columns[1].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DATE);
            listView1.Columns[2].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PATH);

            textBox1.Text = Properties.Settings.Default.BackupFolder ?? "";
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            listView1.Items.Clear();

            _storage.ReadBackupItemList();
            InternalStorage.BackupItemList bil = _storage._backupItemList;
            if (bil != null && bil.BackupItems!=null)
            {
                List<InternalStorage.BackupItem> sbil = (from b in bil.BackupItems select b).OrderBy(x => x.BackupFile).ThenByDescending(x => x.BackupDate).ToList();
                foreach (InternalStorage.BackupItem bi in bil.BackupItems)
                {
                    ListViewItem lvi = new ListViewItem(new string[] {Path.GetFileNameWithoutExtension(bi.OriginalPath), bi.BackupDate.ToString(), bi.OriginalPath });
                    lvi.Tag = bi;
                    listView1.Items.Add(lvi);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text) && Directory.Exists(textBox1.Text))
            {
                folderBrowserDialog1.SelectedPath = textBox1.Text;
            }
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Properties.Settings.Default.BackupFolder = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.Save();
                textBox1.Text = Properties.Settings.Default.BackupFolder;
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button2.Enabled = listView1.SelectedItems.Count > 0;

            if (listView1.SelectedItems.Count > 0)
            {
                textBox2.Text = Path.GetDirectoryName((listView1.SelectedItems[0].Tag as InternalStorage.BackupItem).OriginalPath);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                SelectedBackupItem = listView1.SelectedItems[0].Tag as InternalStorage.BackupItem;
                RestoreFolder = textBox2.Text;
                if (File.Exists(Path.Combine(RestoreFolder, Path.GetFileName(SelectedBackupItem.OriginalPath))))
                {
                    System.Windows.Forms.DialogResult dr = MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_OVERWRITE), Utils.LanguageSupport.Instance.GetTranslation(STR_WARNING), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button3);
                    if (dr == System.Windows.Forms.DialogResult.Yes)
                    {
                    }
                    else if (dr == System.Windows.Forms.DialogResult.No)
                    {
                        DialogResult = System.Windows.Forms.DialogResult.Cancel;
                        Close();
                        return;
                    }
                    else if (dr == System.Windows.Forms.DialogResult.Cancel)
                    {
                        return;
                    }
                }
                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox2.Text) && Directory.Exists(textBox2.Text))
            {
                folderBrowserDialog1.SelectedPath = textBox1.Text;
            }
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog1.SelectedPath;
            }
        }
    }
}
