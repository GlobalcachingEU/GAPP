using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace GlobalcachingApplication.Plugins.Browser
{
    public partial class BrowserScriptPocketQueriesPQSeriesGCProjectForm : Form
    {
        public const string STR_TITLE = "From Project-GC...";
        public const string STR_DESCRIPTION = "Copy the PQ split result as shown in picture\r\nand paste it into the textbox";
        public const string STR_OK = "OK";

        public List<DateTime> DateList { get; private set; }

        public BrowserScriptPocketQueriesPQSeriesGCProjectForm()
        {
            InitializeComponent();

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DESCRIPTION);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                DateList = new List<DateTime>();
                string[] lines = textBox1.Lines;
                foreach (string s in lines)
                {
                    int index;
                    string[] parts = s.Split(new char[] {' ',',','\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1)
                    {
                        if (int.TryParse(parts[0], out index))
                        {
                            DateTime dt;
                            if (DateTime.TryParseExact(parts[1], "MMMM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                            {
                                DateList.Add(dt.Date);
                            }
                            else if (DateTime.TryParseExact(parts[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                            {
                                DateList.Add(dt.Date);
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            button1.Enabled = DateList.Count > 1;
        }
    }
}
