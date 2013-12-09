using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.SHP
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_WGS84 = "WGS84";
        public const string STR_DUTCHGRID = "Dutch Grid";
        public const string STR_COUNTRY = "Country";
        public const string STR_STATE = "State";
        public const string STR_MUNICIPALITY = "Municipality";
        public const string STR_CITY = "City";
        public const string STR_OTHER = "Other";
        public const string STR_ERROR = "Error";
        public const string STR_PARSEERROR = "This file cannot be processed";
        public const string STR_SHAPEFILESTOUSE = "Shapefiles in use";
        public const string STR_DELETE = "Delete";
        public const string STR_SHAPEFILE = "Shapefile";
        public const string STR_NAMEPREFIX = "Name prefix";
        public const string STR_NAMEFIELD = "Name field";
        public const string STR_FORMAT = "Format";
        public const string STR_TYPE = "Type";
        public const string STR_ADD = "Add / Update";
        public const string STR_DOWNLOADMORE = "Download more...";
        public const string STR_ENCODING = "Encoding";

        private Utils.BasePlugin.Plugin _plugin = null;

        public SettingsPanel()
        {
            InitializeComponent();

            comboBox2.Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_WGS84));
            comboBox2.Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_DUTCHGRID));

            comboBox3.Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_COUNTRY));
            comboBox3.Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_STATE));
            comboBox3.Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_MUNICIPALITY));
            comboBox3.Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_CITY));
            comboBox3.Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_OTHER));

            label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHAPEFILESTOUSE);
            button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELETE);
            label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHAPEFILE);
            label11.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAMEPREFIX);
            label5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAMEFIELD);
            label7.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FORMAT);
            label9.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TYPE);
            button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADD);
            button4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DOWNLOADMORE);
            label13.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ENCODING);

            listView1.Columns[0].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHAPEFILE);
            listView1.Columns[1].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAMEFIELD);
            listView1.Columns[2].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FORMAT);
            listView1.Columns[3].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TYPE);
            listView1.Columns[4].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAMEPREFIX);
            listView1.Columns[5].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ENCODING);

            foreach (string s in Properties.Settings.Default.ShapeFiles)
            {
                string[] parts = s.Split(new char[] { '|' }, 7).ToArray();
                ListViewItem lvi = new ListViewItem(parts.Skip(1).ToArray());
                if (lvi.SubItems.Count < 6)
                {
                    lvi.SubItems.Add("");
                }
                lvi.Checked = bool.Parse(parts[0]);
                listView1.Items.Add(lvi);
            }

            try
            {
                openFileDialog1.InitialDirectory = Properties.Settings.Default.DefaultShapeFilesFolder;
            }
            catch
            {
            }
        }

        public SettingsPanel(Utils.BasePlugin.Plugin plugin): this()
        {
            _plugin = plugin;
        }

        public void Apply()
        {
            Properties.Settings.Default.ShapeFiles.Clear();
            foreach (ListViewItem lvi in listView1.Items)
            {
                string s = lvi.Checked.ToString();
                foreach (ListViewItem.ListViewSubItem p in lvi.SubItems)
                {
                    s = string.Concat(s, "|", p.Text);
                }
                if (lvi.SubItems.Count < 5)
                {
                    s = string.Concat(s, "|");
                    s = string.Concat(s, "|");
                }
                else if (lvi.SubItems.Count < 6)
                {
                    s = string.Concat(s, "|");
                }
                Properties.Settings.Default.ShapeFiles.Add(s);
            }
            Properties.Settings.Default.Save();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            comboBox2.SelectedIndex = -1;
            comboBox3.SelectedIndex = -1;
            comboBox2.Enabled = false;
            comboBox3.Enabled = false;
            bool error = false;
            if (textBox1.Text.Length > 0)
            {
                try
                {
                    using (ShapeFile sf = new ShapeFile(textBox1.Text))
                    {
                        comboBox1.Items.AddRange(sf.GetFields());
                    }
                }
                catch
                {
                    error = true;
                }
            }
            if (error)
            {
                MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_PARSEERROR), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
            }
            else if (comboBox1.Items.Count>0)
            {
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
            }
        }

        private void checkAddChangeButton()
        {
            button2.Enabled = (textBox1.Text.Length > 0 && comboBox1.SelectedIndex >= 0 && comboBox2.SelectedIndex >= 0 && comboBox3.SelectedIndex >= 0);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkAddChangeButton();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkAddChangeButton();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkAddChangeButton();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //build the string
            string t;
            switch (comboBox3.SelectedIndex)
            {
                case 0:
                    t = Framework.Data.AreaType.Country.ToString();
                    break;
                case 1:
                    t = Framework.Data.AreaType.State.ToString();
                    break;
                case 2:
                    t = Framework.Data.AreaType.Municipality.ToString();
                    break;
                case 3:
                    t = Framework.Data.AreaType.City.ToString();
                    break;
                default:
                    t = Framework.Data.AreaType.Other.ToString();
                    break;
            }
            string c;
            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    c = ShapeFile.CoordType.WGS84.ToString();
                    break;
                case 1:
                    c = ShapeFile.CoordType.DutchGrid.ToString();
                    break;
                default:
                    c = ShapeFile.CoordType.WGS84.ToString();
                    break;
            }
            ListViewItem lvi = null;
            foreach (ListViewItem l in listView1.Items)
            {
                if (l.SubItems[0].Text == textBox1.Text)
                {
                    lvi = l;
                    lvi.SubItems[1].Text = comboBox1.Text;
                    lvi.SubItems[2].Text = c;
                    lvi.SubItems[3].Text = t;
                    if (lvi.SubItems.Count < 5)
                    {
                        lvi.SubItems.Add(textBox2.Text);
                        lvi.SubItems.Add(textBox3.Text);
                    }
                    else
                    {
                        lvi.SubItems[4].Text = textBox2.Text;
                        if (lvi.SubItems.Count < 6)
                        {
                            lvi.SubItems.Add(textBox3.Text);
                        }
                        else
                        {
                            lvi.SubItems[5].Text = textBox3.Text;
                        }
                    }
                    break;
                }
            }
            if (lvi == null)
            {
                lvi = new ListViewItem(new string[6]);
                lvi.SubItems[0].Text = textBox1.Text;
                lvi.SubItems[1].Text = comboBox1.Text;
                lvi.SubItems[2].Text = c;
                lvi.SubItems[3].Text = t;
                lvi.SubItems[4].Text = textBox2.Text;
                lvi.SubItems[5].Text = textBox3.Text;
                lvi.Checked = true;
                listView1.Items.Add(lvi);
                lvi.Selected = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                listView1.Items.RemoveAt(listView1.SelectedIndices[0]);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                button3.Enabled = true;

                ListViewItem lvi = listView1.Items[listView1.SelectedIndices[0]];
                textBox1.Text = lvi.SubItems[0].Text;
                comboBox1.SelectedIndex = comboBox1.Items.IndexOf(lvi.SubItems[1].Text);
                Framework.Data.AreaType at = (Framework.Data.AreaType)Enum.Parse(typeof(Framework.Data.AreaType), lvi.SubItems[3].Text);
                switch (at)
                {
                    case Framework.Data.AreaType.Country:
                        comboBox3.SelectedIndex = 0;
                        break;
                    case Framework.Data.AreaType.State:
                        comboBox3.SelectedIndex = 1;
                        break;
                    case Framework.Data.AreaType.Municipality:
                        comboBox3.SelectedIndex = 2;
                        break;
                    case Framework.Data.AreaType.City:
                        comboBox3.SelectedIndex = 3;
                        break;
                    case Framework.Data.AreaType.Other:
                        comboBox3.SelectedIndex = 4;
                        break;
                }
                ShapeFile.CoordType ct = (ShapeFile.CoordType)Enum.Parse(typeof(ShapeFile.CoordType), lvi.SubItems[2].Text);
                switch (ct)
                {
                    case ShapeFile.CoordType.WGS84:
                        comboBox2.SelectedIndex = 0;
                        break;
                    case ShapeFile.CoordType.DutchGrid:
                        comboBox2.SelectedIndex = 1;
                        break;
                }
                textBox2.Text = lvi.SubItems.Count > 4 ? lvi.SubItems[4].Text : "";
                textBox3.Text = lvi.SubItems.Count > 5 ? lvi.SubItems[5].Text : "";
            }
            else
            {
                button3.Enabled = false;
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (DownloadShapefileForm dlg = new DownloadShapefileForm(_plugin))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    ListViewItem lvi = new ListViewItem(new string[6]);
                    lvi.SubItems[0].Text = dlg.ShapeFilePath;
                    lvi.SubItems[1].Text = dlg.ShapeFileFieldName;
                    lvi.SubItems[2].Text = dlg.ShapeFileFormat;
                    lvi.SubItems[3].Text = dlg.ShapeFileType;
                    lvi.SubItems[4].Text = dlg.ShapeFilePrefix;
                    lvi.SubItems[5].Text = dlg.ShapeFileDbfEncoding;
                    lvi.Checked = true;
                    listView1.Items.Add(lvi);
                    lvi.Selected = true;
                }
            }
        }
    }
}
