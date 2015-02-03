using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Bookmark
{
    public partial class EditorForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Geocache collections";
        public const string STR_COLLECTION = "Collection";
        public const string STR_NAME = "Name";
        public const string STR_NEW = "New";
        public const string STR_DELETE = "Delete";
        public const string STR_SELECTALL = "Select all";
        public const string STR_REMOVEFROMCOLLECTION = "Remove from collection";
        public const string STR_ADDACTIVE = "Add active geocache to collection";
        public const string STR_ADDSELECTED = "Add selected geocaches to collection";

        private bool _ignoreDataChanged = false;
        private bool _ignoreSelection = false;

        public EditorForm()
        {
            InitializeComponent();
        }

        public EditorForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            var p = PluginSettings.Instance.WindowPos;
            if (p != null && !p.IsEmpty)
            {
                this.Bounds = p;
                this.StartPosition = FormStartPosition.Manual;
            }

            SelectedLanguageChanged(this, EventArgs.Empty);

            Repository.Instance.Initialize(core);
            comboBox1.Items.AddRange(Repository.Instance.AvailableBookmarks.ToArray());

            Repository.Instance.DataChanged += new EventHandler(Instance_DataChanged);

            core.ActiveGeocacheChanged += new Framework.EventArguments.GeocacheEventHandler(core_ActiveGeocacheChanged);
            core.Geocaches.DataChanged += new Framework.EventArguments.GeocacheEventHandler(Geocaches_DataChanged);
            core.Geocaches.ListDataChanged += new EventHandler(Geocaches_ListDataChanged);
            core.Geocaches.SelectedChanged += new Framework.EventArguments.GeocacheEventHandler(Geocaches_SelectedChanged);
            core.Geocaches.ListSelectionChanged += new EventHandler(Geocaches_ListSelectionChanged);
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COLLECTION);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NEW);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELETE);
            this.button5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTALL);
            this.button4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_REMOVEFROMCOLLECTION);
            this.button6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDACTIVE);
            this.button7.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDSELECTED);
        }

        void Geocaches_ListSelectionChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                checkButtons();
            }
        }

        void Geocaches_SelectedChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (Visible)
            {
                checkButtons();
            }
        }

        void Instance_DataChanged(object sender, EventArgs e)
        {
            if (!_ignoreDataChanged && Visible)
            {
                comboBox1.SelectedIndex = -1;
                comboBox1_SelectedIndexChanged(this, EventArgs.Empty);
            }
        }

        void Geocaches_ListDataChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                checkButtons();
            }
        }

        void Geocaches_DataChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (Visible)
            {
                if (e.Geocache == Core.ActiveGeocache)
                {
                    checkButtons();
                }
            }
        }

        void core_ActiveGeocacheChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (Visible)
            {
                checkButtons();
                checkActiveGeocache();
            }
        }

        private void checkActiveGeocache()
        {
            if (!_ignoreSelection && Core.ActiveGeocache != null && listBox1.Items.Count > 0)
            {
                _ignoreSelection = true;
                listBox1.ClearSelected();
                listBox1.SelectedIndex = listBox1.Items.IndexOf(Core.ActiveGeocache.Code);
                _ignoreSelection = false;
            }
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
            if (WindowState == FormWindowState.Normal && this.Visible)
            {
                PluginSettings.Instance.WindowPos = this.Bounds;
            }
        }

        private void EditorForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal && this.Visible)
            {
                PluginSettings.Instance.WindowPos = this.Bounds;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0)
            {
                string s = textBox1.Text.ToLower();
                BookmarkInfo bmi = (from BookmarkInfo b in comboBox1.Items where b.Name.ToLower() == s select b).FirstOrDefault();
                button1.Enabled = bmi == null;
            }
            else
            {
                button1.Enabled = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0)
            {
                _ignoreDataChanged = true;
                BookmarkInfo bmi = Repository.Instance.AddCollection(textBox1.Text);
                _ignoreDataChanged = false;
                comboBox1.Items.Add(bmi);
                comboBox1.SelectedItem = bmi;
                comboBox1_SelectedIndexChanged(sender, e);
                button1.Enabled = false;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            BookmarkInfo bmi = comboBox1.SelectedItem as BookmarkInfo;
            if (bmi != null)
            {
                button2.Enabled = true;
                listBox1.Items.AddRange((from string s in bmi.GeocacheCodes.Keys select s).OrderBy(x => x).ToArray());
            }
            else
            {
                button2.Enabled = false;

            }
            checkButtons();
            checkActiveGeocache();
        }

        private void checkButtons()
        {
            BookmarkInfo bmi = comboBox1.SelectedItem as BookmarkInfo;
            if (bmi != null)
            {
                button5.Enabled = listBox1.Items.Count > 0;
                button6.Enabled = Core.ActiveGeocache != null && !Repository.Instance.InCollection(bmi.Name, Core.ActiveGeocache.Code);
                button7.Enabled = (from Framework.Data.Geocache gc in Core.Geocaches where gc.Selected select gc).FirstOrDefault() != null;
            }
            else
            {
                button6.Enabled = false;
                button7.Enabled = false;
            }
            listBox1_SelectedIndexChanged(null, EventArgs.Empty);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BookmarkInfo bmi = comboBox1.SelectedItem as BookmarkInfo;
            if (bmi != null)
            {
                _ignoreDataChanged = true;
                Repository.Instance.DeleteCollection(bmi.Name);
                _ignoreDataChanged = false;
                comboBox1.Items.Remove(bmi);
                comboBox1_SelectedIndexChanged(sender, e);

                textBox1_TextChanged(sender, e);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button4.Enabled = listBox1.SelectedItems.Count > 0;
            if (!_ignoreSelection && sender !=null && listBox1.SelectedItem != null)
            {
                Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(Core.Geocaches, listBox1.SelectedItem.ToString());
                if (gc != null && (Core.ActiveGeocache==null || Core.ActiveGeocache!=gc))
                {
                    _ignoreSelection = true;
                    Core.ActiveGeocache = gc;
                    _ignoreSelection = false;
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            BookmarkInfo bmi = comboBox1.SelectedItem as BookmarkInfo;
            if (bmi != null && Core.ActiveGeocache!=null)
            {
                if (bmi.GeocacheCodes[Core.ActiveGeocache.Code] == null)
                {
                    _ignoreDataChanged = true;
                    Repository.Instance.AddToCollection(bmi.Name, Core.ActiveGeocache.Code);
                    _ignoreDataChanged = false;
                    listBox1.Items.Add(Core.ActiveGeocache.Code);
                }
            }
            button6.Enabled = false;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            BookmarkInfo bmi = comboBox1.SelectedItem as BookmarkInfo;
            if (bmi != null)
            {
                _ignoreDataChanged = true;
                List<Framework.Data.Geocache> gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                foreach (Framework.Data.Geocache gc in gcList)
                {
                    Repository.Instance.AddToCollection(bmi.Name, gc.Code);
                }
                _ignoreDataChanged = false;
                comboBox1_SelectedIndexChanged(sender, e);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            BookmarkInfo bmi = comboBox1.SelectedItem as BookmarkInfo;
            if (bmi != null)
            {
                using (Utils.FrameworkDataUpdater upd = new Utils.FrameworkDataUpdater(Core))
                {
                    foreach (string s in bmi.GeocacheCodes.Keys)
                    {
                        Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(Core.Geocaches, s);
                        if (gc != null)
                        {
                            gc.Selected = true;
                        }
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            BookmarkInfo bmi = comboBox1.SelectedItem as BookmarkInfo;
            if (bmi != null && listBox1.SelectedItems.Count>0)
            {
                _ignoreDataChanged = true;
                foreach (string s in listBox1.SelectedItems)
                {
                    Repository.Instance.RemoveFromCollection(bmi.Name, s);
                }
                _ignoreDataChanged = false;
                comboBox1_SelectedIndexChanged(sender, e);
            }
        }

    }
}
