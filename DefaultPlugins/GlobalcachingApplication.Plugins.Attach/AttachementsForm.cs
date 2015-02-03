using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Attach
{
    public partial class AttachementsForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Geocache Attachements";
        public const string STR_FILE = "File";
        public const string STR_COMMENT = "Comment";
        public const string STR_ADD = "Add";
        public const string STR_DELETE = "Delete";
        public const string STR_OPEN = "OPEN";

        public class AttachementItem
        {
            public string GeocacheCode { get; set; }
            public string FilePath { get; set; }
            public string Comment { get; set; }
        }
        private List<AttachementItem> _activeAttachements = new List<AttachementItem>();

        public class AttachementPoco
        {
            public string code { get; set; }
            public string filepath { get; set; }
            public string comment { get; set; }
        }

        public AttachementsForm()
        {
            InitializeComponent();
        }

        public AttachementsForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            if (PluginSettings.Instance.WindowPos != null && !PluginSettings.Instance.WindowPos.IsEmpty)
            {
                this.Bounds = PluginSettings.Instance.WindowPos;
                this.StartPosition = FormStartPosition.Manual;
            }

            SelectedLanguageChanged(this, EventArgs.Empty);
            core.ActiveGeocacheChanged += new Framework.EventArguments.GeocacheEventHandler(core_ActiveGeocacheChanged);
        }

        void core_ActiveGeocacheChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (this.Visible)
            {
                UpdateView();
            }
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            base.SelectedLanguageChanged(sender, e);
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.listView1.Columns[0].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FILE);
            this.listView1.Columns[1].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COMMENT);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADD);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELETE);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OPEN);
        }

        public void UpdateView()
        {
            _activeAttachements.Clear();
            linkLabelGC.Links.Clear();
            listView1.Items.Clear();
            if (Core.ActiveGeocache != null)
            {
                pictureBoxGC.ImageLocation = Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Default, Core.ActiveGeocache.GeocacheType);
                linkLabelGC.Text = string.Format("{0}, {1}", Core.ActiveGeocache.Code, Core.ActiveGeocache.Name);
                linkLabelGC.Links.Add(0, Core.ActiveGeocache.Code.Length, Core.ActiveGeocache.Url);
                button1.Enabled = true;
                lock (Core.SettingsProvider)
                {
                    var pocos = Core.SettingsProvider.Database.Fetch<AttachementPoco>(string.Format("select * from {1} where code='{0}'", Core.ActiveGeocache.Code, Core.SettingsProvider.GetFullTableName("attachements")));
                    foreach(var poco in pocos)
                    {
                        AttachementItem ai = new AttachementItem();
                        ai.Comment = poco.comment;
                        ai.FilePath = poco.filepath;
                        ai.GeocacheCode = poco.code;
                        _activeAttachements.Add(ai);
                        addAttachementItemToList(ai);
                    }
                }
            }
            else
            {
                pictureBoxGC.Image = null;
                linkLabelGC.Text = "-";
                button1.Enabled = false;
            }
        }

        private void addAttachementItemToList(AttachementItem ai)
        {
            ListViewItem lvi = new ListViewItem(new string[] {ai.FilePath, ai.Comment });
            lvi.Tag = ai;
            listView1.Items.Add(lvi);
        }

        private void AttachementsForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal && this.Visible)
            {
                PluginSettings.Instance.WindowPos = this.Bounds;
            }
        }

        private void AttachementsForm_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal && this.Visible)
            {
                PluginSettings.Instance.WindowPos = this.Bounds;
            }
        }

        private void AttachementsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void linkLabelGC_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (AddFilesForm dlg = new AddFilesForm())
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string[] fp = dlg.FilePaths;
                    string cmnt = dlg.Comment;
                    foreach (string f in fp)
                    {
                        if ((from s in _activeAttachements where s.FilePath.ToLower() == f.ToLower() select s).FirstOrDefault() == null)
                        {
                            AttachementItem ai = new AttachementItem();
                            ai.FilePath = f;
                            ai.Comment = cmnt;
                            ai.GeocacheCode = Core.ActiveGeocache.Code;
                            _activeAttachements.Add(ai);
                            addAttachementItemToList(ai);
                            lock (Core.SettingsProvider)
                            {
                                try
                                {
                                    Core.SettingsProvider.Database.Execute(string.Format("insert into {3} (code, filepath, comment) values ('{2}', '{0}', '{1}')", ai.FilePath.Replace("'", "''"), ai.Comment.Replace("'", "''"), ai.GeocacheCode.Replace("'", "''"), Core.SettingsProvider.GetFullTableName("attachements")));
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                AttachementItem ai = listView1.SelectedItems[0].Tag as AttachementItem;
                lock (Core.SettingsProvider)
                {
                    try
                    {
                        Core.SettingsProvider.Database.Execute(string.Format("delete from {2} where code='{1}' and filepath='{0}'", ai.FilePath.Replace("'", "''"), ai.GeocacheCode.Replace("'", "''"), Core.SettingsProvider.GetFullTableName("attachements")));
                    }
                    catch
                    {
                    }
                }
                _activeAttachements.Remove(ai);
                listView1.Items.Remove(listView1.SelectedItems[0]);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button2.Enabled = listView1.SelectedItems.Count > 0;
            button3.Enabled = listView1.SelectedItems.Count > 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                AttachementItem ai = listView1.SelectedItems[0].Tag as AttachementItem;
                try
                {
                    System.Diagnostics.Process.Start(ai.FilePath);
                }
                catch
                {
                }
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (button3.Enabled)
            {
                button3_Click(sender, e);
            }
        }
    }

    public class Attachements : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Geocache attachements";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            if (PluginSettings.Instance == null)
            {
                var p = new PluginSettings(core);
            }

            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(AttachementsForm.STR_ADD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AttachementsForm.STR_COMMENT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AttachementsForm.STR_DELETE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AttachementsForm.STR_FILE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AttachementsForm.STR_OPEN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AttachementsForm.STR_TITLE));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(AddFilesForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AddFilesForm.STR_ADD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AddFilesForm.STR_COMMENT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AddFilesForm.STR_FILES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AddFilesForm.STR_OK));

            initDatabase(core);

            return await base.InitializeAsync(core);
        }

        private void initDatabase(Framework.Interfaces.ICore core)
        {
            lock(core.SettingsProvider)
            {
                try
                {
                    if (!core.SettingsProvider.TableExists(core.SettingsProvider.GetFullTableName("attachements")))
                    {
                        core.SettingsProvider.Database.Execute(string.Format("create table '{0}' (code text, filepath text, comment text)", core.SettingsProvider.GetFullTableName("attachements")));
                    }
                }
                catch
                {
                }
            }
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.UIChildWindow;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new AttachementsForm(this, core));
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (UIChildWindowForm != null)
                {
                    if (action == ACTION_SHOW)
                    {
                        if (!UIChildWindowForm.Visible)
                        {
                            (UIChildWindowForm as AttachementsForm).UpdateView();
                            UIChildWindowForm.Show();
                        }
                        if (UIChildWindowForm.WindowState == FormWindowState.Minimized)
                        {
                            UIChildWindowForm.WindowState = FormWindowState.Normal;
                        }
                        UIChildWindowForm.BringToFront();
                    }
                }
                result = true;
            }
            return result;
        }
    }
}
