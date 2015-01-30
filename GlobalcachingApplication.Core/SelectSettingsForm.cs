using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Core
{
    public partial class SelectSettingsForm : Form
    {
        private Engine _eng;

        public SelectSettingsForm(Engine eng)
        {
            InitializeComponent();

            _eng = eng;
            checkBox1.Checked = eng.EnablePluginDataPathAtStartup;

            listBox1.Items.AddRange(eng.SettingsProvider.GetSettingsScopes().ToArray());
            listBox1.SelectedItem = eng.SettingsProvider.GetSettingsScope();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _eng.EnablePluginDataPathAtStartup = checkBox1.Checked;
            if (listBox1.SelectedItem != null)
            {
                _eng.SettingsProvider.SetSettingsScope(listBox1.SelectedItem.ToString(), true);
            }
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = listBox1.SelectedIndex >= 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _eng.SettingsProvider.SetSettingsScope(null, true);
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button2.Enabled = textBox1.Text.Length > 0 && !listBox1.Items.Contains(textBox1.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _eng.SettingsProvider.NewSettingsScope(textBox1.Text, null);
            listBox1.Items.Clear();
            listBox1.Items.AddRange(_eng.SettingsProvider.GetSettingsScopes().ToArray());
            listBox1.SelectedItem = _eng.SettingsProvider.GetSettingsScope();
        }
    }
}
