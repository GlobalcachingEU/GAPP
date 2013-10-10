using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.ScrPresets
{
    public partial class PresetNameForm : Form
    {
        public const string STR_TITLE = "Preset name";
        public const string STR_NAME = "Name";
        public const string STR_OK = "OK";

        public PresetNameForm()
        {
            InitializeComponent();
        }

        public PresetNameForm(List<string> presets)
        {
            InitializeComponent();

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);

            comboBox1.Items.AddRange(presets.ToArray());
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = comboBox1.Text.Length > 0;
        }

        public string PresetName
        {
            get { return comboBox1.Text; ;}
        }

        private void comboBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '|' || e.KeyChar == '\'')
            {
                e.Handled = true;
            }
        }
    }
}
