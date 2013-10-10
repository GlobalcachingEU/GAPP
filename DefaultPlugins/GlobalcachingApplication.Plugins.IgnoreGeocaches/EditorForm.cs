using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace GlobalcachingApplication.Plugins.IgnoreGeocaches
{
    public partial class EditorForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Ignore geocaches";
        public const string STR_GCCODE = "Code";
        public const string STR_GCNAME = "Name contains";
        public const string STR_GCOWNER = "Owner";
        public const string STR_PROPERTY = "Property";
        public const string STR_VALUES = "Filters";
        public const string STR_REMOVE = "Remove";
        public const string STR_ADD = "Add";
        public const string STR_EXPLAIN = "New filters only apply for new imports";
        public const string STR_CLEARALL = "Clear all filters";

        public class IgnorFields
        {
            public IgnoreService.FilterField Field { get; private set; }
            public string Text { get; private set; }

            public IgnorFields(IgnoreService.FilterField field, string text)
            {
                Field = field;
                Text = text;
            }

            public override string ToString()
            {
                return Utils.LanguageSupport.Instance.GetTranslation(Text);
            }
        }

        public EditorForm()
        {
            InitializeComponent();
        }

        public EditorForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            if (Properties.Settings.Default.WindowPos != null && !Properties.Settings.Default.WindowPos.IsEmpty)
            {
                this.Location = Properties.Settings.Default.WindowPos.Location;
                this.StartPosition = FormStartPosition.Manual;
            }

            comboBox1.Items.Add(new IgnorFields(IgnoreService.FilterField.GeocacheCode, STR_GCCODE));
            comboBox1.Items.Add(new IgnorFields(IgnoreService.FilterField.GeocacheName, STR_GCNAME));
            comboBox1.Items.Add(new IgnorFields(IgnoreService.FilterField.GeocacheOwner, STR_GCOWNER));
            comboBox1.SelectedIndex = 0;

            SelectedLanguageChanged(this, EventArgs.Empty);
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PROPERTY);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_VALUES);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADD);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_REMOVE);
            this.label5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXPLAIN);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CLEARALL);

            typeof(ComboBox).InvokeMember("RefreshItems", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, comboBox1, new object[] { });
        }

        public void UpdateView()
        {
            comboBox1_SelectedIndexChanged(this, EventArgs.Empty);
        }

        private void EditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void EditorForm_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void EditorForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            IgnorFields igf = comboBox1.SelectedItem as IgnorFields;
            if (igf != null)
            {
                listBox1.Items.AddRange(IgnoreService.Instance(Core).GetFilters(igf.Field));
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            IgnorFields igf = comboBox1.SelectedItem as IgnorFields;
            if (listBox1.SelectedIndex < 0 || igf==null)
            {
                button2.Enabled = false;
            }
            else
            {
                button2.Enabled = true;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            IgnorFields igf = comboBox1.SelectedItem as IgnorFields;
            if (textBox1.Text.Length > 0 && igf != null)
            {
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
            }
        }

        public void AddCodes(List<string> codes)
        {
            IgnoreService.Instance(Core).AddCodes(codes);
            IgnorFields igf = comboBox1.SelectedItem as IgnorFields;
            if (igf != null && this.Visible && igf.Field ==  IgnoreService.FilterField.GeocacheCode)
            {
                comboBox1_SelectedIndexChanged(this, EventArgs.Empty);
            }
        }

        public void AddFilter(IgnoreService.FilterField field, string text)
        {
            string[] filters = IgnoreService.Instance(Core).GetFilters(field);
            string cs = (from s in filters where string.Compare(s, text, true) == 0 select s).FirstOrDefault();
            if (string.IsNullOrEmpty(cs))
            {
                IgnoreService.Instance(Core).AddFilter(field, field == IgnoreService.FilterField.GeocacheCode ? text.ToUpper() : text);
                IgnorFields igf = comboBox1.SelectedItem as IgnorFields;
                if (igf != null && this.Visible && igf.Field == field)
                {
                    comboBox1_SelectedIndexChanged(this, EventArgs.Empty);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IgnorFields igf = comboBox1.SelectedItem as IgnorFields;
            if (textBox1.Text.Length > 0 && igf != null)
            {
                AddFilter(igf.Field, textBox1.Text);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            IgnorFields igf = comboBox1.SelectedItem as IgnorFields;
            if (igf != null && listBox1.SelectedItem != null)
            {
                IgnoreService.Instance(Core).RemoveFilter(igf.Field, listBox1.SelectedItem.ToString());
                listBox1.Items.Remove(listBox1.SelectedItem);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            IgnoreService.Instance(Core).Clear();
            comboBox1_SelectedIndexChanged(this, EventArgs.Empty);
        }

    }
}
