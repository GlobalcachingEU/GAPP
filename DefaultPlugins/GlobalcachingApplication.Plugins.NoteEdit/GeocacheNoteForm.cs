using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.NoteEdit
{
    public partial class GeocacheNoteForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_CACHENOTEEDITOR = "Geocache note editor";
        public const string STR_NOTESFOR = "Notes for";
        public const string STR_PERSONALNOTE = "Personal note on geocaching.com";
        public const string STR_NOTE = "Note";
        public const string STR_SAVE = "Save";

        public GeocacheNoteForm()
        {
            InitializeComponent();
        }

        public GeocacheNoteForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CACHENOTEEDITOR));

            if (PluginSettings.Instance.WindowPos != null && !PluginSettings.Instance.WindowPos.IsEmpty)
            {
                this.Bounds = PluginSettings.Instance.WindowPos;
                this.StartPosition = FormStartPosition.Manual;
            }

            core.ActiveGeocacheChanged += new Framework.EventArguments.GeocacheEventHandler(core_ActiveGeocacheChanged);
            SelectedLanguageChanged(this, EventArgs.Empty);
            core.Geocaches.DataChanged += new Framework.EventArguments.GeocacheEventHandler(Geocaches_DataChanged);
            core.Geocaches.ListDataChanged += new EventHandler(Geocaches_ListDataChanged);
        }

        void Geocaches_ListDataChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                UpdateView();
            }
        }

        void Geocaches_DataChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (Visible)
            {
                if (e.Geocache == Core.ActiveGeocache)
                {
                    UpdateView();
                }
            }
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CACHENOTEEDITOR);
            this.label3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NOTESFOR);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PERSONALNOTE);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NOTE);
            this.buttonSaveNote.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SAVE);
        }


        void core_ActiveGeocacheChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (Visible)
            {
                UpdateView();
            }
        }

        public void UpdateView()
        {
            if (Core.ActiveGeocache == null)
            {
                labelActiveGeocache.Text = "-";
                textBox1.Text = "";
                htmlEditorControl1.Text = "";
            }
            else
            {
                labelActiveGeocache.Text = string.Format("{0} - {1}", Core.ActiveGeocache.Code, Core.ActiveGeocache.Name);
                textBox1.Text = Core.ActiveGeocache.PersonaleNote;
                htmlEditorControl1.Text = Core.ActiveGeocache.Notes;
            }
        }

        private void GeocacheNoteForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                Core.ActiveGeocacheChanged -= new Framework.EventArguments.GeocacheEventHandler(core_ActiveGeocacheChanged);
                Core.Geocaches.DataChanged -= new Framework.EventArguments.GeocacheEventHandler(Geocaches_DataChanged);
                Core.Geocaches.ListDataChanged -= new EventHandler(Geocaches_ListDataChanged);
            }
        }

        private void buttonSaveNote_Click(object sender, EventArgs e)
        {
            if (Core.ActiveGeocache != null)
            {
                Core.ActiveGeocache.Notes = htmlEditorControl1.Text;
            }
        }

        private void GeocacheNoteForm_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal && this.Visible)
            {
                PluginSettings.Instance.WindowPos = this.Bounds;
            }
        }

        private void GeocacheNoteForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal && this.Visible)
            {
                PluginSettings.Instance.WindowPos = this.Bounds;
            }
        }

    }

    public class GeocacheNoteEditor : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Edit Geocache notes";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            var p = new PluginSettings(core);

            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheNoteForm.STR_CACHENOTEEDITOR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheNoteForm.STR_NOTESFOR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheNoteForm.STR_PERSONALNOTE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheNoteForm.STR_NOTE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheNoteForm.STR_SAVE));

            return await base.InitializeAsync(core);
        }

        public override string FriendlyName
        {
            get
            {
                return "Geocache note editor";
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
            return (new GeocacheNoteForm(this, core));
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
                            (UIChildWindowForm as GeocacheNoteForm).UpdateView();
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
