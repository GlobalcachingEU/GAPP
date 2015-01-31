using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.ExpExcel
{
    public partial class ExportForm : Form
    {
        public const string STR_TITLE = "Export to Excel";
        public const string STR_FILE = "FILE";
        public const string STR_SHEETS = "Sheets";
        public const string STR_FIELDS = "Fields";
        public const string STR_NAME = "Name";
        public const string STR_EXPORT = "Export";
        public const string STR_DELETE = "Delete";
        public const string STR_ADD = "Add";

        public class Sheet
        {
            public string Name { get; set; }
            public List<PropertyItem> SelectedItems { get; private set; }

            public Sheet()
            {
                SelectedItems = new List<PropertyItem>();
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public List<Sheet> Sheets
        {
            get
            {
                List<Sheet> result = new List<Sheet>();
                foreach (Sheet s in comboBox1.Items)
                {
                    result.Add(s);
                }
                return result;
            }
        }

        public ExportForm()
        {
            InitializeComponent();

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FILE);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHEETS);
            this.label5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FIELDS);
            this.listView1.Columns[0].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXPORT);
            this.button4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELETE);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADD);

            foreach (PropertyItem pi in PropertyItem.PropertyItems)
            {
                ListViewItem lvi = new ListViewItem(pi.ToString());
                lvi.Tag = pi;
                listView1.Items.Add(lvi);
            }
            _cntdown = listView1.Items.Count;

            textBox1.Text = PluginSettings.Instance.FilePath ?? "";
            foreach (string s in PluginSettings.Instance.ExportFields)
            {
                //fielscount
                //fields
                //name
                string[] parts = s.Split(new char[] { '|' }, 2);
                int cnt = int.Parse(parts[0]);
                parts = s.Split(new char[] { '|' }, cnt+2);
                Sheet sheet = new Sheet();
                sheet.Name = parts[parts.Length - 1];
                for (int i = 1; i < parts.Length - 1; i++)
                {
                    sheet.SelectedItems.Add((from p in PropertyItem.PropertyItems where p.GetType().ToString() == parts[i] select p).FirstOrDefault());
                }
                comboBox1.Items.Add(sheet);
            }
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
                button1.Enabled = textBox1.Text.Length > 0;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PluginSettings.Instance.FilePath = textBox1.Text;
            System.Collections.Specialized.StringCollection sc = new System.Collections.Specialized.StringCollection();
            foreach (Sheet sheet in comboBox1.Items)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(sheet.SelectedItems.Count.ToString());
                foreach(PropertyItem pi in sheet.SelectedItems)
                {
                    sb.AppendFormat("|{0}",pi.GetType().ToString());
                }
                sb.AppendFormat("|{0}",sheet.Name);
                sc.Add(sb.ToString());
            }
            PluginSettings.Instance.ExportFields = sc;
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = saveFileDialog1.FileName;
            }
        }

        private void checkOK()
        {
            button1.Enabled = textBox1.TextLength > 0 && comboBox1.Items.Count>0;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            checkOK();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button4.Enabled = comboBox1.SelectedIndex >= 0;
            listView1.Enabled = comboBox1.SelectedIndex >= 0;
            Sheet sheet = comboBox1.SelectedItem as Sheet;
            if (sheet != null)
            {
                updateSelectedItems(sheet);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            button3.Enabled = textBox2.TextLength > 0;
        }

        private void updateSelectedItems(Sheet sheet)
        {
            listView1.ItemChecked -= listView1_ItemChecked;
            //maintain selected order, so first
            button5.Enabled = false;
            button6.Enabled = false;
            listView1.Items.Clear();
            foreach (PropertyItem pi in sheet.SelectedItems)
            {
                ListViewItem lvi = new ListViewItem(pi.ToString());
                lvi.Tag = pi;
                lvi.Checked = true;
                listView1.Items.Add(lvi);
            }
            foreach (PropertyItem pi in PropertyItem.PropertyItems)
            {
                if (!sheet.SelectedItems.Contains(pi))
                {
                    ListViewItem lvi = new ListViewItem(pi.ToString());
                    lvi.Tag = pi;
                    listView1.Items.Add(lvi);
                }
            }
            listView1.ItemChecked += listView1_ItemChecked;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Sheet sheet = new Sheet();
            sheet.Name = textBox2.Text;
            comboBox1.Items.Add(sheet);
            comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
            textBox2.Text = "";
            checkOK();
        }

        private void updateSelectedItemsForSheet()
        {
            Sheet sheet = comboBox1.SelectedItem as Sheet;
            if (sheet != null)
            {
                sheet.SelectedItems.Clear();
                for (int i = 0; i < listView1.Items.Count; i++ )
                {
                    ListViewItem ci = listView1.Items[i];
                    if (ci.Checked)
                    {
                        sheet.SelectedItems.Add(ci.Tag as PropertyItem);
                    }
                }
            }
        }

        private int _cntdown;
        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (_cntdown>0)
            {
                _cntdown--;
                return;
            }
            Sheet sheet = comboBox1.SelectedItem as Sheet;
            if (sheet != null)
            {
                if (e.Item.Checked != sheet.SelectedItems.Contains((e.Item.Tag as PropertyItem)))
                {
                    updateSelectedItemsForSheet();
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            comboBox1.Items.RemoveAt(comboBox1.SelectedIndex);
            checkOK();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 0)
            {
                button6.Enabled = false;
                button5.Enabled = false;
            }
            else
            {
                button6.Enabled = listView1.SelectedIndices[0] > 0;
                button5.Enabled = listView1.SelectedIndices[0] < listView1.Items.Count-1;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //up
            int index = listView1.SelectedIndices[0];
            ListViewItem lvi = listView1.Items[index];
            listView1.Items.Remove(lvi);
            listView1.Items.Insert(index - 1, lvi);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //down
            int index = listView1.SelectedIndices[0];
            ListViewItem lvi = listView1.Items[index];
            listView1.Items.Remove(lvi);
            listView1.Items.Insert(index + 1, lvi);
        }
    }
}
