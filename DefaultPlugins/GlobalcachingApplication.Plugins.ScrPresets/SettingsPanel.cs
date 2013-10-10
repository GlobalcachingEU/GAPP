using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.ScrPresets
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_PRESETS = "Presets";
        public const string STR_DELETE = "Delete";

        public SettingsPanel()
        {
            InitializeComponent();
        }

        public SettingsPanel(List<string> presetnames)
        {
            InitializeComponent();

            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PRESETS);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELETE);
            listBox1.Items.AddRange(presetnames.ToArray());
        }

        public List<string> PresetNames
        {
            get { return (from string s in listBox1.Items select s).ToList(); }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = listBox1.SelectedIndex >= 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            }
        }
    }
}
