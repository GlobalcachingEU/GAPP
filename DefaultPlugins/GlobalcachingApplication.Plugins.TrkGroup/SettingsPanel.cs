using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.TrkGroup
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_DELAY = "Delay between trackable updates";

        public SettingsPanel()
        {
            InitializeComponent();
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELAY);
            this.numericUpDown1.Value = PluginSettings.Instance.TimeBetweenTrackableUpdates;
        }

        public void Apply()
        {
            PluginSettings.Instance.TimeBetweenTrackableUpdates = (int)this.numericUpDown1.Value;
        }
    }
}
