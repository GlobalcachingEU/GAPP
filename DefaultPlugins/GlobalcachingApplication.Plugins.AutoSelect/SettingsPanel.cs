using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.AutoSelect
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_AUTO_SELECTNEW = "Automatic selection of new geocaches";

        public SettingsPanel()
        {
            InitializeComponent();

            checkBox1.Checked = Properties.Settings.Default.AutoSelectNewGeocaches;

            checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AUTO_SELECTNEW);
        }

        public void Apply()
        {
            Properties.Settings.Default.AutoSelectNewGeocaches = checkBox1.Checked;
            Properties.Settings.Default.Save();
        }
    }

}
