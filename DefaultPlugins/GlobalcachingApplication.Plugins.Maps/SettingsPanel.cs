using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Maps
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_CLEARCACHE = "Clear cache";
        public const string STR_OFFLINEOSMMAPS = "Offline OSM maps";
        public const string STR_LOCATION = "Folder";
        public const string STR_MAPS = "maps";
        public const string STR_GETMORE = "Get more...";

        private MapsPlugin _mapsPlugin = null;

        public SettingsPanel()
        {
            InitializeComponent();
        }

        public SettingsPanel(MapsPlugin mapsPlugin) : this()
        {
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CLEARCACHE);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OFFLINEOSMMAPS);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOCATION);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAPS);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GETMORE);

            _mapsPlugin = mapsPlugin;
            textBox1.Text = PluginSettings.Instance.OSMOfflineMapFolder;
            loadMaps();
        }

        public void Apply()
        {
            PluginSettings.Instance.OSMOfflineMapFolder = textBox1.Text;
            PluginSettings.Instance.DisabledMaps.Clear();
            for (int i = 0; i < checkedListBox1.Items.Count; i++ )
            {
                if (!checkedListBox1.GetItemChecked(i))
                {
                    PluginSettings.Instance.DisabledMaps.Add(checkedListBox1.Items[i].ToString());
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                _mapsPlugin.ClearCache();
            }
            catch
            {
            }
            this.Cursor = Cursors.Default;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog()== DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
                loadMaps();
            }
        }

        private void loadMaps()
        {
            checkedListBox1.Items.Clear();
            try
            {
                string[] fls = System.IO.Directory.GetFiles(textBox1.Text, "*.map");
                foreach (string s in fls)
                {
                    string fn = System.IO.Path.GetFileName(s);
                    checkedListBox1.Items.Add(fn, !PluginSettings.Instance.DisabledMaps.Contains(fn));
                }
            }
            catch
            {
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (GetMapsForm dlg = new GetMapsForm(_mapsPlugin))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if (dlg.DownloadedFile != null)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        try
                        {
                            string dst = System.IO.Path.Combine(textBox1.Text, dlg.DownloadedFileName);
                            if (System.IO.File.Exists(dst))
                            {
                                System.IO.File.Delete(dst);
                            }
                            System.IO.File.Move(dlg.DownloadedFile.Path, dst);
                            loadMaps();
                        }
                        catch
                        {
                        }
                        this.Cursor = Cursors.Default;
                    }
                }
            }
        }
    }
}
