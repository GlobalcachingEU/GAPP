using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.APIGetC
{
    public partial class PresetsForm : Form
    {
        public const string STR_TITLE = "Filter presets";
        public const string STR_SAVED = "Saved presets";
        public const string STR_DELETE = "Delete";
        public const string STR_CURRENT = "Current filter settings";
        public const string STR_NAME = "Name";
        public const string STR_SAVE = "Save";
        public const string STR_OK = "OK";

        public List<PresetSettings> Presets { get; private set; }
        private PresetSettings _activePreset;

        public PresetsForm()
        {
            InitializeComponent();
        }
        public PresetsForm(List<PresetSettings> allPresets, PresetSettings activePreset)
            : this()
        {
            Presets = new List<PresetSettings>();
            Presets.AddRange(allPresets);
            _activePreset = activePreset;

            listBox1.Items.AddRange(allPresets.ToArray());
            if (_activePreset != null)
            {
                textBox1.Text = _activePreset.Name;
            }
            else
            {
                button2.Enabled = false;
            }

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SAVED);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELETE);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CURRENT);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SAVE);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = listBox1.SelectedItem != null;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button2.Enabled = _activePreset != null && textBox1.Text.Length > 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Presets.Remove(listBox1.SelectedItem as PresetSettings);
            listBox1.Items.Remove(listBox1.SelectedItem);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _activePreset.Name = textBox1.Text;
            Presets.Add(_activePreset);
            listBox1.Items.Add(_activePreset);
            _activePreset = null;
            textBox1.Text = "";
            textBox1.Enabled = false;
            button2.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}
