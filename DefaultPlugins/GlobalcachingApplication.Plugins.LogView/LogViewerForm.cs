using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.LogView
{
    public partial class LogViewerForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Log viewer";
        public const string STR_SHOWFORACTIVE = "Show logs for active geocache";
        public const string STR_SHOWFORUSER = "Show logs of user";
        public const string STR_USERNAME = "User name";
        public const string STR_SHOW = "Show";
        public const string STR_GEOCACHECODE = "Geocache Code";
        public const string STR_DATE = "Date";
        public const string STR_USER = "User";
        public const string STR_TEXT = "Text";
        public const string STR_DELETE = "Delete";

        public static Framework.Interfaces.ICore FixedCore = null;

        public LogViewerForm()
        {
            InitializeComponent();
        }

        public LogViewerForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            FixedCore = core;
            InitializeComponent();

            core.ActiveGeocacheChanged += new Framework.EventArguments.GeocacheEventHandler(core_ActiveGeocacheChanged);
            core.Logs.DataChanged += new Framework.EventArguments.LogEventHandler(Logs_DataChanged);
            core.Logs.ListDataChanged += new EventHandler(Logs_ListDataChanged);
            core.Logs.LogAdded += new Framework.EventArguments.LogEventHandler(Logs_LogAdded);
            core.Logs.LogRemoved += new Framework.EventArguments.LogEventHandler(Logs_LogRemoved);

            logViewControl1.logList.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(logList_SelectionChanged);

            SelectedLanguageChanged(this, EventArgs.Empty);
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.radioButton1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHOWFORACTIVE);
            this.radioButton2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHOWFORUSER);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_USERNAME);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHOW);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELETE);
            logViewControl1.logList.Columns[1].Header = Utils.LanguageSupport.Instance.GetTranslation(STR_DATE);
            logViewControl1.logList.Columns[2].Header = Utils.LanguageSupport.Instance.GetTranslation(STR_GEOCACHECODE);
            logViewControl1.logList.Columns[3].Header = Utils.LanguageSupport.Instance.GetTranslation(STR_USER);
            logViewControl1.logList.Columns[4].Header = Utils.LanguageSupport.Instance.GetTranslation(STR_TEXT);
        }

        void logList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            button2.Enabled = (logViewControl1.logList.SelectedItems != null && logViewControl1.logList.SelectedItems.Count > 0);
            if (radioButton2.Checked)
            {
                if (logViewControl1.logList.SelectedItem != null)
                {
                    Framework.Data.Log l = logViewControl1.logList.SelectedItem as Framework.Data.Log;
                    if (l != null && l.GeocacheCode!=null)
                    {
                        Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(Core.Geocaches, l.GeocacheCode);
                        if (gc != null)
                        {
                            Core.ActiveGeocache = gc;
                        }
                    }
                }
            }
        }

        void Logs_LogRemoved(object sender, Framework.EventArguments.LogEventArgs e)
        {
            if (this.Visible)
            {
                UpdateView(true);
            }
        }

        void Logs_LogAdded(object sender, Framework.EventArguments.LogEventArgs e)
        {
            if (this.Visible)
            {
                UpdateView(true);
            }
        }

        void Logs_ListDataChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                UpdateView(true);
            }
        }

        void Logs_DataChanged(object sender, Framework.EventArguments.LogEventArgs e)
        {
            if (this.Visible)
            {
                UpdateView(true);
            }
        }

        void core_ActiveGeocacheChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (this.Visible)
            {
                UpdateView(radioButton1.Checked);
            }
        }

        public void UpdateView()
        {
            UpdateView(true);
        }

        public void UpdateView(bool reloadList)
        {
            linkLabelGC.Links.Clear();
            if (Core.ActiveGeocache != null)
            {
                pictureBoxGC.ImageLocation = Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Default, Core.ActiveGeocache.GeocacheType);
                linkLabelGC.Text = string.Format("{0}, {1}", Core.ActiveGeocache.Code, Core.ActiveGeocache.Name);
                linkLabelGC.Links.Add(0, Core.ActiveGeocache.Code.Length, Core.ActiveGeocache.Url);
            }
            else
            {
                pictureBoxGC.Image = null;
                linkLabelGC.Text = "-";
            }

            if (reloadList)
            {
                if (radioButton1.Checked)
                {
                    if (Core.ActiveGeocache != null)
                    {
                        logViewControl1.logList.DataContext = Utils.DataAccess.GetLogs(Core.Logs, Core.ActiveGeocache.Code);
                    }
                    else
                    {
                        logViewControl1.logList.DataContext = null;
                    }
                }
                else
                {
                    string s = textBox1.Text.Trim();
                    logViewControl1.logList.DataContext = (from Framework.Data.Log l in Core.Logs
                                                           where l.Finder != null && string.Compare(l.Finder, s, true)==0
                                                           orderby l.Date descending
                                                           select l).ToList();
                }
            }
        }

        private void LogViewerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!radioButton2.Checked)
            {
                radioButton2.Checked = true;
            }
            else
            {
                UpdateView(true);
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            UpdateView(true);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            UpdateView(true);
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar=='\x0D')
            {
                e.Handled = true;
                button1_Click(this, EventArgs.Empty);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (logViewControl1.logList.SelectedItems != null && logViewControl1.logList.SelectedItems.Count > 0)
            {
                this.Cursor = Cursors.WaitCursor;
                using (Utils.FrameworkDataUpdater upd = new Utils.FrameworkDataUpdater(Core))
                {
                    foreach (Framework.Data.Log l in logViewControl1.logList.SelectedItems)
                    {
                        if (l != null)
                        {
                            Utils.DataAccess.DeleteLog(Core, l);
                        }
                    }
                }
                UpdateView(true);
                this.Cursor = Cursors.Default;
            }
        }

    }
}
