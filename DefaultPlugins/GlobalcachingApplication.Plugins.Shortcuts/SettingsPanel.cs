using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Shortcuts
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_CLEAR = "Clear";
        public const string STR_INFO = "Select a plugin with action and press the key to use as a shortcut.";

        public SettingsPanel()
        {
            InitializeComponent();
        }

        public void ApplyShortcuts(Framework.Interfaces.ICore core)
        {
            core.ShortcutInfo.Clear();
            foreach (ListViewItem lvi in listView1.Items)
            {
                if (lvi.SubItems[3].Tag != null)
                {
                    Framework.Interfaces.IPlugin p = lvi.Tag as Framework.Interfaces.IPlugin;
                    if (p != null)
                    {
                        core.ShortcutInfo.Add(new Framework.Data.ShortcutInfo() { PluginType = lvi.SubItems[0].Tag.ToString(), PluginAction = lvi.SubItems[1].Tag.ToString(), PluginSubAction = lvi.SubItems[2].Tag.ToString(), ShortcutKeys = (Keys)(int)lvi.SubItems[3].Tag, ShortcutKeyString = lvi.SubItems[3].Text });
                    }
                }
            }
        }

        public SettingsPanel(Framework.Interfaces.ICore core)
        {
            InitializeComponent();

            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_INFO);
            this.buttonClear.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CLEAR);

            List<Framework.Data.ShortcutInfo> scuts = core.ShortcutInfo;
            List<Framework.Interfaces.IPlugin> pins = core.GetPlugins();
            foreach (Framework.Interfaces.IPlugin p in pins)
            {
                List<string> actions = p.GetActionSubactionList('@');
                foreach (string act in actions)
                {
                    string[] parts = act.Split(new char[] { '@' });
                    ListViewItem lvi = new ListViewItem(new string[] { p.GetType().ToString(), Utils.LanguageSupport.Instance.GetTranslation(parts[0]), parts.Length==1?"": Utils.LanguageSupport.Instance.GetTranslation(parts[1]), "" });
                    lvi.Tag = p;
                    listView1.Items.Add(lvi);
                    Framework.Data.ShortcutInfo sc = (from t in scuts where t.PluginType == p.GetType().ToString() && t.PluginAction == parts[0] && (parts.Length==1 || t.PluginSubAction == parts[1]) select t).FirstOrDefault();
                    lvi.SubItems[0].Tag = p.GetType().ToString();
                    lvi.SubItems[1].Tag = parts[0];
                    lvi.SubItems[2].Tag = parts.Length==1?"":parts[1];
                    if (sc != null)
                    {
                        lvi.SubItems[3].Text = sc.ShortcutKeyString;
                        lvi.SubItems[3].Tag = sc.ShortcutKeys;
                    }
                    else
                    {
                        lvi.SubItems[3].Tag = null;
                    }
                }
            }
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                KeysConverter kc = new KeysConverter();

                ListViewItem lvi = listView1.Items[listView1.SelectedIndices[0]];
                lvi.SubItems[3].Text = kc.ConvertToString(e.KeyData);
                lvi.SubItems[3].Tag = e.KeyData;
                e.Handled = true;
            }
        }

        private void listView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonClear.Enabled = listView1.SelectedIndices.Count > 0;
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                ListViewItem lvi = listView1.Items[listView1.SelectedIndices[0]];
                lvi.SubItems[3].Text = "";
                lvi.SubItems[3].Tag = null;
            }
        }
    }
}
