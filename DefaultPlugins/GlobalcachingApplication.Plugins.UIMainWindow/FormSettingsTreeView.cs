using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.UIMainWindow
{
    public partial class FormSettingsTreeView : Form
    {
        private const string STR_YES = "Yes";
        private const string STR_NO = "No";
        private const string STR_TITLE = "Settings";
        private const string STR_GENERAL = "General";
        private const string STR_GCCOMACCOUNT = "Geocaching.com account";
        private const string STR_AUTHORIZE = "Authorize";
        private const string STR_PLUGINS = "Plugins";
        private const string STR_APPLY = "Apply";
        private const string STR_NAME = "Name";
        private const string STR_MENBERSHIP = "Membership";
        private const string STR_API = "API";
        private const string STR_INTERNALSTORAGE = "Internal storage";
        private const string STR_APPLYINGSETTINGS = "Applying settings...";
        private const string STR_LOADINBACKGROUND = "Load logs in background if supported";
        private const string STR_AUTOSAVEONCLOSE = "Automatic save data on closing";
        private const string STR_CLEARAUTHORIZE = "Clear authorization";
        private const string STR_OTHERACCOUNTNAMES = "Account names on other geocaching sites";
        private const string STR_CODEPREFIX = "Code prefix";
        private const string STR_ACCOUNTNAME = "Account name";

        private Framework.Interfaces.ICore _core = null;
        private List<Framework.Interfaces.IPlugin> _pluginList = null;
        private List<UserControl> _ucList = new List<UserControl>();
        private Utils.BasePlugin.Plugin _ownerPlugin = null;

        public FormSettingsTreeView()
        {
            InitializeComponent();
        }

        public static Framework.Data.LanguageItem[] LanguageItems
        {
            get
            {
                List<Framework.Data.LanguageItem> result = new List<Framework.Data.LanguageItem>();
                result.AddRange(new Framework.Data.LanguageItem[] 
                    { 
                        new Framework.Data.LanguageItem(STR_YES), 
                        new Framework.Data.LanguageItem(STR_NO), 
                        new Framework.Data.LanguageItem(STR_TITLE), 
                        new Framework.Data.LanguageItem(STR_GENERAL), 
                        new Framework.Data.LanguageItem(STR_GCCOMACCOUNT), 
                        new Framework.Data.LanguageItem(STR_AUTHORIZE), 
                        new Framework.Data.LanguageItem(STR_PLUGINS), 
                        new Framework.Data.LanguageItem(STR_APPLY), 
                        new Framework.Data.LanguageItem(STR_NAME), 
                        new Framework.Data.LanguageItem(STR_INTERNALSTORAGE), 
                        new Framework.Data.LanguageItem(STR_API), 
                        new Framework.Data.LanguageItem(STR_INTERNALSTORAGE), 
                        new Framework.Data.LanguageItem(STR_APPLYINGSETTINGS), 
                        new Framework.Data.LanguageItem(STR_LOADINBACKGROUND), 
                        new Framework.Data.LanguageItem(STR_AUTOSAVEONCLOSE), 
                        new Framework.Data.LanguageItem(STR_CLEARAUTHORIZE), 
                        new Framework.Data.LanguageItem(STR_OTHERACCOUNTNAMES), 
                        new Framework.Data.LanguageItem(STR_CODEPREFIX), 
                        new Framework.Data.LanguageItem(STR_ACCOUNTNAME), 
                    });
                foreach (var t in Enum.GetNames(typeof(Framework.PluginType)))
                {
                    result.Add(new Framework.Data.LanguageItem(PluginTypeToString(t)));
                }                
                return result.ToArray();
            }
        }

        private static string PluginTypeToString(string ptName)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Char c in ptName)
            {
                if (Char.IsUpper(c))
                    sb.Append(' ');
                sb.Append(c);
            }
            return sb.ToString().TrimStart();
        }

        public FormSettingsTreeView(Framework.Interfaces.ICore core, Utils.BasePlugin.Plugin ownerPlugin)
            : this()
        {
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GCCOMACCOUNT);
            this.buttonAuthorize.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AUTHORIZE);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_INTERNALSTORAGE);
            this.buttonApply.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_APPLY);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_API);
            this.label3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MENBERSHIP);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOADINBACKGROUND);
            this.checkBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AUTOSAVEONCLOSE);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CLEARAUTHORIZE);
            this.groupBox3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OTHERACCOUNTNAMES);
            this.listView1.Columns[0].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CODEPREFIX);
            this.listView1.Columns[1].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ACCOUNTNAME);
            this.label7.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CODEPREFIX);
            this.label8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ACCOUNTNAME);

            TreeNode tn;
            tn = new TreeNode(Utils.LanguageSupport.Instance.GetTranslation(STR_GENERAL));
            tn.Tag = panelGeneral;
            treeView1.Nodes.Add(tn);

            foreach (var t in Enum.GetNames(typeof(Framework.PluginType)))
            {
                tn = new TreeNode(Utils.LanguageSupport.Instance.GetTranslation(PluginTypeToString(t)));
                tn.Tag = (Framework.PluginType)Enum.Parse(typeof(Framework.PluginType), t);
                treeView1.Nodes.Add(tn);
            }

            _core = core;
            _ownerPlugin = ownerPlugin;
            _pluginList = core.GetPlugins();
            List<string> allPlugins = core.GetAllDetectedPlugins();
            checkedListBoxPlugins.Items.AddRange(allPlugins.ToArray());
            foreach (Framework.Interfaces.IPlugin p in _pluginList)
            {
                int index = checkedListBoxPlugins.Items.IndexOf(p.GetType().FullName);
                if (index >= 0)
                {
                    checkedListBoxPlugins.SetItemChecked(index, true);
                }
                List<UserControl> pnls = p.CreateConfigurationPanels();
                if (pnls != null && pnls.Count > 0)
                {
                    _ucList.AddRange(pnls.ToArray());
                    int yPos = 0;
                    foreach (UserControl uc in pnls)
                    {
                        uc.Location = new Point(0, yPos);
                        yPos += uc.Height;
                        uc.Visible = false;
                        splitContainer1.Panel2.Controls.Add(uc);
                    }                    

                    foreach (TreeNode t in treeView1.Nodes)
                    {
                        if (t.Tag!=null && t.Tag.GetType()==typeof( Framework.PluginType))
                        {
                            if ((Framework.PluginType)t.Tag == p.PluginType)
                            {
                                tn = new TreeNode(Utils.LanguageSupport.Instance.GetTranslation(p.FriendlyName));
                                tn.Tag = pnls;
                                t.Nodes.Add(tn);
                                break;
                            }
                        }
                    }
                }
            }
            //delete the plugin nodes with no childs (settings)
            int i = 1;
            while ( i < treeView1.Nodes.Count)
            {
                TreeNode t = treeView1.Nodes[i];
                if (t.Tag != null && t.Tag.GetType() == typeof(Framework.PluginType))
                {
                    if (t.Nodes.Count == 0)
                    {
                        treeView1.Nodes.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            treeView1.ExpandAll();
            treeView1.SelectedNode = treeView1.Nodes[0];

            comboBoxInternalStorage.Items.AddRange(core.GetAvailableInternalStoragePlugins().ToArray());
            comboBoxInternalStorage.SelectedItem = core.ActiveInternalStoragePlugin;
            textBoxUsername.Text = core.GeocachingComAccount.AccountName;
            labelApiEnabled.Text = string.IsNullOrEmpty(core.GeocachingComAccount.APIToken) ? Utils.LanguageSupport.Instance.GetTranslation(STR_NO) : Utils.LanguageSupport.Instance.GetTranslation(STR_YES);
            labelApiMembership.Text = string.IsNullOrEmpty(core.GeocachingComAccount.MemberType) ? "-" : core.GeocachingComAccount.MemberType;
            checkBox1.Checked = core.LoadLogsInBackground;
            checkBox2.Checked = core.AutoSaveOnClose;

            fillGeocacheAccounts();
        }

        private void fillGeocacheAccounts()
        {
            listView1.Items.Clear();
            string[] pf = _core.GeocachingAccountNames.GeocachePrefixes;
            foreach (string s in pf)
            {
                if (s != "GC")
                {
                    listView1.Items.Add(new ListViewItem(new string[] { s, _core.GeocachingAccountNames.GetAccountName(s) }));
                }
            }
        }

        private void treeView1_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = (e.Node != null && e.Node.Tag != null && e.Node.Tag.GetType() == typeof(Framework.PluginType));
            if (!e.Cancel)
            {
                if (treeView1.SelectedNode != null)
                {
                    if (treeView1.SelectedNode == treeView1.Nodes[0])
                    {
                        panelGeneral.Visible = false;
                    }
                    else if (treeView1.SelectedNode.Tag is List<UserControl>)
                    {
                        List<UserControl> ucl = treeView1.SelectedNode.Tag as List<UserControl>;
                        foreach (UserControl uc in ucl)
                        {
                            uc.Visible = false;
                        }
                    }
                }
                if (e.Node != null)
                {
                    if (e.Node == treeView1.Nodes[0])
                    {
                        panelGeneral.Visible = true;
                    }
                    else if (e.Node.Tag is List<UserControl>)
                    {
                        List<UserControl> ucl = e.Node.Tag as List<UserControl>;
                        foreach (UserControl uc in ucl)
                        {
                            uc.Visible = true;
                        }
                    }
                }
            }
        }

        private void buttonAuthorize_Click(object sender, EventArgs e)
        {
            if (Utils.API.GeocachingLiveV6.Authorize(_core, false))
            {
                textBoxUsername.Text = _core.GeocachingComAccount.AccountName;
                labelApiEnabled.Text = string.IsNullOrEmpty(_core.GeocachingComAccount.APIToken) ? Utils.LanguageSupport.Instance.GetTranslation(STR_NO) : Utils.LanguageSupport.Instance.GetTranslation(STR_YES);
                labelApiMembership.Text = string.IsNullOrEmpty(_core.GeocachingComAccount.MemberType) ? "-" : _core.GeocachingComAccount.MemberType;
            }
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            bool needRestart = false;
            if (_core != null)
            {
                _core.GeocachingComAccount.AccountName = textBoxUsername.Text;
                _core.LoadLogsInBackground = checkBox1.Checked;
                _core.AutoSaveOnClose = checkBox2.Checked;
                if (_ucList.Count > 0)
                {
                    using (Utils.ProgressBlock prog = new Utils.ProgressBlock(this._ownerPlugin, Utils.LanguageSupport.Instance.GetTranslation(STR_APPLYINGSETTINGS), Utils.LanguageSupport.Instance.GetTranslation(STR_APPLYINGSETTINGS), _pluginList.Count, 0))
                    {
                        Application.DoEvents();
                        int index = 0;
                        foreach (Framework.Interfaces.IPlugin p in _pluginList)
                        {
                            prog.UpdateProgress(Utils.LanguageSupport.Instance.GetTranslation(STR_APPLYINGSETTINGS), Utils.LanguageSupport.Instance.GetTranslation(p.FriendlyName), _pluginList.Count, index);
                            Application.DoEvents();
                            p.ApplySettings(_ucList);
                            index++;
                        }
                    }
                }
                List<string> sl = new List<string>();
                for (int index = 0; index < checkedListBoxPlugins.Items.Count; index++)
                {
                    if (!checkedListBoxPlugins.CheckedIndices.Contains(index))
                    {
                        sl.Add(checkedListBoxPlugins.Items[index].ToString());
                    }
                }
                needRestart = _core.SetDisabledPlugins(sl.ToArray());
                if ((string)comboBoxInternalStorage.SelectedItem != _core.ActiveInternalStoragePlugin)
                {
                    _core.ActiveInternalStoragePlugin = (string)comboBoxInternalStorage.SelectedItem;
                    needRestart = true;
                }
            }
            Close();
            DialogResult = System.Windows.Forms.DialogResult.OK;
            if (needRestart)
            {
                Application.Restart();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _core.GeocachingComAccount.APIToken = "";
            _core.GeocachingComAccount.MemberType = "";
            _core.GeocachingComAccount.MemberTypeId = 0;
            labelApiEnabled.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NO);
            labelApiMembership.Text = "-";
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button2.Enabled = listView1.SelectedItems.Count > 0;
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            button3.Enabled = comboBox1.Text.Length == 2 && textBox1.Text.Length > 0;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button3.Enabled = comboBox1.Text.Length == 2 && textBox1.Text.Length > 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text.Length == 2 && textBox1.Text.Length > 0)
            {
                _core.GeocachingAccountNames.SetAccountName(comboBox1.Text.ToUpper(), textBox1.Text);
                fillGeocacheAccounts();
            }
        }
    }
}
