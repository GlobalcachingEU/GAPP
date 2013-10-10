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
    public partial class FormSettings : Form
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

        private Framework.Interfaces.ICore _core = null;
        private List<Framework.Interfaces.IPlugin> _pluginList = null;
        private List<UserControl> _ucList = new List<UserControl>();
        private Utils.BasePlugin.Plugin _ownerPlugin = null;

        public static Framework.Data.LanguageItem[] LanguageItems
        {
            get
            {
                return new Framework.Data.LanguageItem[] 
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
                        new Framework.Data.LanguageItem(STR_MENBERSHIP), 
                        new Framework.Data.LanguageItem(STR_API), 
                        new Framework.Data.LanguageItem(STR_INTERNALSTORAGE), 
                        new Framework.Data.LanguageItem(STR_APPLYINGSETTINGS), 
                        new Framework.Data.LanguageItem(STR_LOADINBACKGROUND), 
                    };
            }
        }

        public FormSettings()
        {
            InitializeComponent();
        }

        public FormSettings(Framework.Interfaces.ICore core, Utils.BasePlugin.Plugin ownerPlugin)
            : this()
        {
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.tabPageGeneral.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GENERAL);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GCCOMACCOUNT);
            this.buttonAuthorize.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AUTHORIZE);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PLUGINS);
            this.buttonApply.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_APPLY);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_API);
            this.label3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MENBERSHIP);
            this.label7.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_INTERNALSTORAGE);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOADINBACKGROUND);

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

                    //create tab
                    TabPage tp = new TabPage(Utils.LanguageSupport.Instance.GetTranslation(p.FriendlyName));
                    tp.AutoScroll = true;
                    tabControlSettings.TabPages.Add(tp);
                    //add controls
                    FlowLayoutPanel fp = new FlowLayoutPanel();
                    tp.Controls.Add(fp);
                    fp.Dock = DockStyle.Fill;
                    fp.Controls.AddRange(pnls.ToArray());
                }
            }
            comboBoxInternalStorage.Items.AddRange(core.GetAvailableInternalStoragePlugins().ToArray());
            comboBoxInternalStorage.SelectedItem = core.ActiveInternalStoragePlugin;
            textBoxUsername.Text = core.GeocachingComAccount.AccountName;
            labelApiEnabled.Text = string.IsNullOrEmpty(core.GeocachingComAccount.APIToken) ? Utils.LanguageSupport.Instance.GetTranslation(STR_NO) : Utils.LanguageSupport.Instance.GetTranslation(STR_YES);
            labelApiMembership.Text = string.IsNullOrEmpty(core.GeocachingComAccount.MemberType) ? "-" : core.GeocachingComAccount.MemberType;
            checkBox1.Checked = core.LoadLogsInBackground;
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            bool needRestart = false;
            if (_core != null)
            {
                _core.GeocachingComAccount.AccountName = textBoxUsername.Text;
                _core.LoadLogsInBackground = checkBox1.Checked;
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
                for (int index=0; index<checkedListBoxPlugins.Items.Count; index++)
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

        private void buttonAuthorize_Click(object sender, EventArgs e)
        {
            if (Utils.API.GeocachingLiveV6.Authorize(_core, false))
            {
                textBoxUsername.Text = _core.GeocachingComAccount.AccountName;
                labelApiEnabled.Text = string.IsNullOrEmpty(_core.GeocachingComAccount.APIToken) ? Utils.LanguageSupport.Instance.GetTranslation(STR_NO) : Utils.LanguageSupport.Instance.GetTranslation(STR_YES);
                labelApiMembership.Text = string.IsNullOrEmpty(_core.GeocachingComAccount.MemberType) ? "-" : _core.GeocachingComAccount.MemberType;
            }
        }
    }
}
