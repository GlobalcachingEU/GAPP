using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Attach
{
    public partial class AddFilesForm : Form
    {
        public const string STR_TITLE = "Add attachements";
        public const string STR_FILES = "Files";
        public const string STR_COMMENT = "Comment";
        public const string STR_ADD = "Add";
        public const string STR_OK = "OK";

        public AddFilesForm()
        {
            InitializeComponent();

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FILES);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADD);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COMMENT);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string s in openFileDialog1.FileNames)
                {
                    if (!listBox1.Items.Contains(s))
                    {
                        listBox1.Items.Add(s);
                    }
                }
                button2.Enabled = listBox1.Items.Count > 0;
            }
        }

        public string[] FilePaths
        {
            get { return (from string s in listBox1.Items select s).ToArray(); }
        }
        public string Comment
        {
            get { return textBox1.Text; }
        }
    }
}
