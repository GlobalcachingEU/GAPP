using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace GlobalcachingApplication.Plugins.ActionSequence
{
    public partial class ActionListForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Action sequence";
        public const string STR_EXECUTING = "Executing...";
        public const string STR_EXECUTE = "Execute";
        public const string STR_DELETE = "Delete";
        public const string STR_NEW = "New";
        public const string STR_RENAME = "Rename";
        public const string STR_SEQUENCE = "Sequence";
        public const string STR_PLUGIN = "Plugin";
        public const string STR_ACTION = "Action";
        public const string STR_SUBACTION = "Sub action";

        private string _settingsFile = null;
        private List<SequenceInfo> _sequenceInfos = null;

        public ActionListForm()
        {
            InitializeComponent();
        }

        public ActionListForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            if (Properties.Settings.Default.WindowPos != null && !Properties.Settings.Default.WindowPos.IsEmpty)
            {
                this.Bounds = Properties.Settings.Default.WindowPos;
                this.StartPosition = FormStartPosition.Manual;
            }

            try
            {
                _settingsFile = System.IO.Path.Combine(core.PluginDataPath, "actionseq.xml" );
            }
            catch
            {
            }

            SelectedLanguageChanged(this, EventArgs.Empty);

            _sequenceInfos = SequenceInfo.GetActionSequences(_settingsFile);
            comboBox1.Items.AddRange(_sequenceInfos.ToArray());
        }

        public void ApplicationInitialized()
        {
            Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
            foreach (SequenceInfo af in _sequenceInfos)
            {
                (this.OwnerPlugin as ActionList).AddNewAction(af.Name);
                main.AddAction(OwnerPlugin, "Action sequence", af.Name);
            }
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            base.SelectedLanguageChanged(sender, e);
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SEQUENCE);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXECUTE);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELETE);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NEW);
            this.button4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RENAME);
            this.listView1.Columns[0].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PLUGIN);
            this.listView1.Columns[1].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ACTION);
            this.listView1.Columns[2].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SUBACTION);
        }

        public void Execute(string actionSequence)
        {
            Execute((from a in _sequenceInfos where a.Name == actionSequence select a).FirstOrDefault());
        }

        public void Execute(SequenceInfo si)
        {
            if (si != null)
            {
                try
                {
                    foreach (SequenceInfo.ActionInfo ai in si.Actions)
                    {
                        var p = Utils.PluginSupport.PluginByName(Core, ai.Plugin);
                        if (p != null)
                        {
                            if (string.IsNullOrEmpty(ai.SubAction))
                            {
                                p.Action(ai.Action);
                            }
                            else
                            {
                                p.Action(string.Format("{0}|{1}", ai.Action, ai.SubAction));
                            }
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private void ActionListForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void ActionListForm_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void ActionListForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            SequenceInfo si = comboBox1.SelectedItem as SequenceInfo;
            if (si != null)
            {
                button1.Enabled = true;
                button2.Enabled = true;
                button7.Enabled = true;
                button3.Enabled = textBox1.Text.Length > 0 && (from a in _sequenceInfos where a.Name == textBox1.Text select a).FirstOrDefault() == null;
                button4.Enabled = textBox1.Text.Length > 0 && (from a in _sequenceInfos where a.Name == textBox1.Text select a).FirstOrDefault() == null;

                foreach (var a in si.Actions)
                {
                    ListViewItem lvi = new ListViewItem(new string[]{
                                Utils.LanguageSupport.Instance.GetTranslation(a.Plugin),
                                Utils.LanguageSupport.Instance.GetTranslation(a.Action),
                                Utils.LanguageSupport.Instance.GetTranslation(a.SubAction)
                            });
                    lvi.Tag = a;
                    listView1.Items.Add(lvi);
                }
            }
            else
            {
                button1.Enabled = false;
                button2.Enabled = false;
                button7.Enabled = false;
                button4.Enabled = false;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            SequenceInfo si = comboBox1.SelectedItem as SequenceInfo;
            if (si != null)
            {
                using (SelectActionForm dlg = new SelectActionForm(Core))
                {
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        if (dlg.SelectedAction != null)
                        {
                            si.Actions.Add(dlg.SelectedAction);
                            ListViewItem lvi = new ListViewItem(new string[]{
                                Utils.LanguageSupport.Instance.GetTranslation(dlg.SelectedAction.Plugin),
                                Utils.LanguageSupport.Instance.GetTranslation(dlg.SelectedAction.Action),
                                Utils.LanguageSupport.Instance.GetTranslation(dlg.SelectedAction.SubAction)
                            });
                            lvi.Tag = dlg.SelectedAction;
                            listView1.Items.Add(lvi);
                            SequenceInfo.SaveActionSequences(_settingsFile, _sequenceInfos);
                        }
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0)
            {
                SequenceInfo si = new SequenceInfo();
                si.Name = textBox1.Text;
                _sequenceInfos.Add(si);

                Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
                (this.OwnerPlugin as ActionList).AddNewAction(si.Name);
                main.AddAction(OwnerPlugin, "Action sequence", si.Name);

                comboBox1.Items.Add(si);
                comboBox1.SelectedItem = si;

                comboBox1_SelectedIndexChanged(this, EventArgs.Empty);
                SequenceInfo.SaveActionSequences(_settingsFile, _sequenceInfos);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0)
            {
                button3.Enabled = (from a in _sequenceInfos where a.Name == textBox1.Text select a).FirstOrDefault() == null;
                button4.Enabled = (comboBox1.SelectedItem as SequenceInfo) != null && (from a in _sequenceInfos where a.Name == textBox1.Text select a).FirstOrDefault() == null;
            }
            else
            {
                button3.Enabled = false;
                button4.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SequenceInfo si = comboBox1.SelectedItem as SequenceInfo;
            if (si != null)
            {
                Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
                (this.OwnerPlugin as ActionList).DeleteAction(si.Name);
                main.RemoveAction(OwnerPlugin, "Action sequence", si.Name);

                comboBox1.Items.Remove(si);
                comboBox1_SelectedIndexChanged(this, EventArgs.Empty);

                _sequenceInfos.Remove(si);
                SequenceInfo.SaveActionSequences(_settingsFile, _sequenceInfos);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SequenceInfo si = comboBox1.SelectedItem as SequenceInfo;
            if (si != null && listView1.SelectedItems.Count > 0)
            {
                si.Actions.Remove(listView1.SelectedItems[0].Tag as SequenceInfo.ActionInfo);
                listView1.Items.Remove(listView1.SelectedItems[0]);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SequenceInfo si = comboBox1.SelectedItem as SequenceInfo;
            if (si != null)
            {
                this.Enabled = false;
                toolStripStatusLabel1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXECUTING);
                statusStrip1.Refresh();
                Application.DoEvents();
                Execute(si);
                this.Enabled = true;
                toolStripStatusLabel1.Text = "";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SequenceInfo si = comboBox1.SelectedItem as SequenceInfo;
            if (si != null)
            {
                Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
                (this.OwnerPlugin as ActionList).DeleteAction(si.Name);
                main.RemoveAction(OwnerPlugin, "Action sequence", si.Name);

                si.Name = textBox1.Text;
                (this.OwnerPlugin as ActionList).AddNewAction(si.Name);
                main.AddAction(OwnerPlugin, "Action sequence", si.Name);

                typeof(ComboBox).InvokeMember("RefreshItems", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, comboBox1, new object[] { });
                comboBox1_SelectedIndexChanged(this, EventArgs.Empty);

                SequenceInfo.SaveActionSequences(_settingsFile, _sequenceInfos);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SequenceInfo si = comboBox1.SelectedItem as SequenceInfo;
            if (si != null && listView1.SelectedItems.Count > 0)
            {
                button5.Enabled = true;
                button6.Enabled = listView1.SelectedIndices[0] > 0;
                button8.Enabled = listView1.SelectedIndices[0] < listView1.Items.Count - 1;
            }
            else
            {
                button5.Enabled = false;
                button6.Enabled = false;
                button8.Enabled = false;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            SequenceInfo si = comboBox1.SelectedItem as SequenceInfo;
            if (si != null && listView1.SelectedItems.Count > 0 && listView1.SelectedIndices[0] > 0)
            {
                SequenceInfo.ActionInfo ai = listView1.SelectedItems[0].Tag as SequenceInfo.ActionInfo;
                if (ai != null)
                {
                    int index = listView1.SelectedIndices[0] - 1;
                    si.Actions.Remove(ai);
                    si.Actions.Insert(index, ai);

                    comboBox1_SelectedIndexChanged(this, EventArgs.Empty);
                    listView1.Items[index].Selected = true;
                    listView1_SelectedIndexChanged(this, EventArgs.Empty);

                    SequenceInfo.SaveActionSequences(_settingsFile, _sequenceInfos);
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            SequenceInfo si = comboBox1.SelectedItem as SequenceInfo;
            if (si != null && listView1.SelectedItems.Count > 0 && listView1.SelectedIndices[0] < listView1.Items.Count - 1)
            {
                SequenceInfo.ActionInfo ai = listView1.SelectedItems[0].Tag as SequenceInfo.ActionInfo;
                if (ai != null)
                {
                    int index = listView1.SelectedIndices[0] + 1;
                    si.Actions.Remove(ai);
                    si.Actions.Insert(index, ai);

                    comboBox1_SelectedIndexChanged(this, EventArgs.Empty);
                    listView1.Items[index].Selected = true;
                    listView1_SelectedIndexChanged(this, EventArgs.Empty);

                    SequenceInfo.SaveActionSequences(_settingsFile, _sequenceInfos);
                }
            }
        }

    }
}
