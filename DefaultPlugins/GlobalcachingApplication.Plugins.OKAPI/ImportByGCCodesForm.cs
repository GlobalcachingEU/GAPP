using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public partial class ImportByGCCodesForm : Form
    {
        public const string STR_TITLE = "Import geocaches by codes";
        public const string STR_LIST = "Provide list of geocache codes";
        public const string STR_OK = "OK";

        public ImportByGCCodesForm()
        {
            InitializeComponent();

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LIST);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);
        }

        public List<string> GCCodes
        {
            get
            {
                List<string> result = new List<string>();
                string[] parts = textBox1.Text.Split(new char[] {' ', ',','.','\r','\n','\t',';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in parts)
                {
                    result.Add(s.ToUpper());
                }
                return result;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}
