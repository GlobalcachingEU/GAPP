using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.GCVote
{
    public partial class DashboardForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "GCVote dashboard";
        public const string STR_WARNDELETE = "Are you sure you want to delete all GCVote data?";
        public const string STR_WARNING = "Warning";
        public const string STR_GEOCACHE = "Geocache";
        public const string STR_MAINTENANCE = "Maintenance";
        public const string STR_AVEGARE = "Average";
        public const string STR_MEDIAN = "Median";
        public const string STR_COUNT = "Count";
        public const string STR_YOURVOTE = "Your vote";
        public const string STR_LOADALLVOTES = "Load all votes";
        public const string STR_CLEARALLVOTES = "Clear all votes";
        public const string STR_SETTINGS = "Settings";
        public const string STR_CLEARSAVEDDATA = "Clear saved data";

        public DashboardForm()
        {
            InitializeComponent();
        }

        public DashboardForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            core.ActiveGeocacheChanged += new Framework.EventArguments.GeocacheEventHandler(core_ActiveGeocacheChanged);
            SelectedLanguageChanged(this, EventArgs.Empty);
            core.Geocaches.DataChanged += new Framework.EventArguments.GeocacheEventHandler(Geocaches_DataChanged);
            core.Geocaches.ListDataChanged += new EventHandler(Geocaches_ListDataChanged);
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GEOCACHE);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAINTENANCE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AVEGARE);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MEDIAN);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COUNT);
            this.label8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_YOURVOTE);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOADALLVOTES);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CLEARALLVOTES);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SETTINGS);
            this.button4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CLEARSAVEDDATA);
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

        void core_ActiveGeocacheChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (Visible)
            {
                UpdateView();
            }
        }

        public void UpdateView()
        {
            linkLabelGC.Links.Clear();
            if (Core.ActiveGeocache != null)
            {
                pictureBoxGC.ImageLocation = Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Default, Core.ActiveGeocache.GeocacheType);
                linkLabelGC.Text = string.Format("{0}, {1}", Core.ActiveGeocache.Code, Core.ActiveGeocache.Name);
                linkLabelGC.Links.Add(0, Core.ActiveGeocache.Code.Length, Core.ActiveGeocache.Url);

                double VoteMedian = 0;
                double VoteAvg = 0;
                int VoteCnt = 0;
                double VoteUser = 0;
                if (Repository.Instance.GetGCVote(Core.ActiveGeocache.Code, out VoteMedian, out VoteAvg, out VoteCnt, out VoteUser))
                {
                    textBox1.Text = VoteMedian.ToString("0.0");
                    textBox2.Text = VoteAvg.ToString("0.0");
                    textBox3.Text = VoteCnt.ToString();
                    if (VoteUser < 1)
                    {
                        textBox4.Text = "-";
                    }
                    else
                    {
                        textBox4.Text = VoteUser.ToString("0.0");
                    }
                }
                else
                {
                    textBox1.Text = "-";
                    textBox2.Text = "-";
                    textBox3.Text = "-";
                    textBox4.Text = "-";
                }
            }
            else
            {
                pictureBoxGC.Image = null;
                linkLabelGC.Text = "-";
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
                textBox4.Text = "";
            }
        }

        private void DashboardForm_FormClosing(object sender, FormClosingEventArgs e)
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

        private async void button3_Click(object sender, EventArgs e)
        {
            await Repository.Instance.ActivateGCVote(OwnerPlugin as Utils.BasePlugin.Plugin);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Repository.Instance.DeactivateGCVote();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_WARNDELETE), Utils.LanguageSupport.Instance.GetTranslation(STR_WARNING), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
            {
                Cursor = Cursors.WaitCursor;
                Repository.Instance.ClearAllData();
                Cursor = Cursors.Default;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (SettingsForm dlg = new SettingsForm(Core))
            {
                dlg.ShowDialog();
            }
        }

        private void linkLabelGC_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }
    }
}
