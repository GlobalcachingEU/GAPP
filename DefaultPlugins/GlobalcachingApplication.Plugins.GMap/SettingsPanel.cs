using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.GMap
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_CLUSTERMARKER = "Cluster Marker";
        public const string STR_ENABLEABOVETHRESHOLD = "Enable above # geocaches";
        public const string STR_MAXZOOMLEVEL = "Maximum zoom level";
        public const string STR_GRIDSIZE = "Grid size";
        public const string STR_SHOWNAME = "Show name of geocache in tool tip";
        public const string STR_AUTOTOPPANEL = "Automatic top panel visibility";

        public bool SettingsChanged { get; private set; }

        public SettingsPanel()
        {
            InitializeComponent();

            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CLUSTERMARKER);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ENABLEABOVETHRESHOLD);
            this.label3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAXZOOMLEVEL);
            this.label5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GRIDSIZE);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHOWNAME);
            this.checkBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AUTOTOPPANEL);

            SettingsChanged = false;
            numericUpDown1.Value = PluginSettings.Instance.ClusterMarkerThreshold;
            numericUpDown2.Value = PluginSettings.Instance.ClusterMarkerMaxZoomLevel;
            numericUpDown3.Value = PluginSettings.Instance.ClusterMarkerGridSize;
            checkBox1.Checked = PluginSettings.Instance.ShowNameInToolTip;
            checkBox2.Checked = PluginSettings.Instance.AutoTopPanel;
        }

        public void Apply()
        {
            SettingsChanged = false;
            SettingsChanged |= (int)numericUpDown1.Value != PluginSettings.Instance.ClusterMarkerThreshold;
            SettingsChanged |= (int)numericUpDown2.Value != PluginSettings.Instance.ClusterMarkerMaxZoomLevel;
            SettingsChanged |= (int)numericUpDown3.Value != PluginSettings.Instance.ClusterMarkerGridSize;
            SettingsChanged |= checkBox1.Checked != PluginSettings.Instance.ShowNameInToolTip;

            PluginSettings.Instance.ClusterMarkerThreshold = (int)numericUpDown1.Value;
            PluginSettings.Instance.ClusterMarkerMaxZoomLevel = (int)numericUpDown2.Value;
            PluginSettings.Instance.ClusterMarkerGridSize = (int)numericUpDown3.Value;
            PluginSettings.Instance.ShowNameInToolTip = checkBox1.Checked;
            PluginSettings.Instance.AutoTopPanel = checkBox2.Checked;
        }
    }
}
