using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.PkgMng
{
    public partial class NewCoreVersionAvailableNotification : Utils.Controls.NotificationMessageBox
    {
        public const string STR_NEWVERSIONAVAILABLE = "A new version is available";
        public const string STR_DOWNLOADPAGE = "See website";
        public const string STR_DOWNLOADINSTALL = "Download and install";
        public const string STR_DOWNLOADING = "Downloading...";

        public event EventHandler<EventArgs> DownloadAndInstall;
        public string DownloadLink { get; set; }

        public NewCoreVersionAvailableNotification()
        {
            InitializeComponent();

            label1.Visible = false;
            label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DOWNLOADING);
            linkLabel1.Text = string.Format("{0} ({1})", Utils.LanguageSupport.Instance.GetTranslation(STR_NEWVERSIONAVAILABLE), Utils.LanguageSupport.Instance.GetTranslation(STR_DOWNLOADPAGE));
            linkLabel1.Links.Add(new LinkLabel.Link(Utils.LanguageSupport.Instance.GetTranslation(STR_NEWVERSIONAVAILABLE).Length + 2, Utils.LanguageSupport.Instance.GetTranslation(STR_DOWNLOADPAGE).Length));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://application.globalcaching.eu");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            if (DownloadAndInstall != null)
            {
                label1.Visible = true;
                label1.Refresh();
                DownloadAndInstall(this, EventArgs.Empty);
            }
        }
    }
}
