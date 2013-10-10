using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.LogImagesViewer
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_CACHEDATA = "Store offline";
        public const string STR_DOWNLOADALL = "Download all log images...";

        private Viewer _owner = null;

        public SettingsPanel()
        {
            InitializeComponent();
        }

        public SettingsPanel(Viewer owner): this()
        {
            _owner = owner;

            label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CACHEDATA);
            button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DOWNLOADALL);

            checkBox1.Checked = Properties.Settings.Default.CacheImages;
        }

        public void Apply()
        {
            Properties.Settings.Default.CacheImages = checkBox1.Checked;
            Properties.Settings.Default.Save();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _owner.DownloadAllLogImages();
        }
    }
}
