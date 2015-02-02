using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.SimpleCacheList
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_BACKGROUND = "Background";
        public const string STR_ARCHIVED = "Archived";
        public const string STR_NOTAVAILABLE = "Not available";
        public const string STR_AVAILABLE = "Available";
        public const string STR_OWN = "Own";
        public const string STR_FOUND = "Found";
        public const string STR_DEFERREDSCROLLING = "Use deferred scrolling";
        public const string STR_ENABLEAUTOSORT = "Enable automatic sorting";
        public const string STR_AUTOTOPPANEL = "Automatic top panel visibility";
        public const string STR_EXTRACOORD = "Extr. coord.";

        public SettingsPanel()
        {
            InitializeComponent();

            labelArchivedColor.BackColor = PluginSettings.Instance.BkColorArchived;
            labelNotAvailable.BackColor = PluginSettings.Instance.BkColorNotAvailable;
            labelAvailable.BackColor = PluginSettings.Instance.BkColorAvailable;
            labelFound.BackColor = PluginSettings.Instance.BkColorFound;
            labelOwn.BackColor = PluginSettings.Instance.BkColorOwned;
            label12.BackColor = PluginSettings.Instance.BkColorExtraCoord;
            checkBoxDefferredScrolling.Checked = PluginSettings.Instance.DeferredScrolling;
            checkBox1.Checked = PluginSettings.Instance.EnableAutomaticSorting;
            checkBox2.Checked = PluginSettings.Instance.AutoTopPanel;

            button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_BACKGROUND);
            button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_BACKGROUND);
            button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_BACKGROUND);
            button4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_BACKGROUND);
            buttonBkArchived.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_BACKGROUND);

            label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ARCHIVED);
            label3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NOTAVAILABLE);
            label5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AVAILABLE);
            label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FOUND);
            label11.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OWN);
            label13.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXTRACOORD);

            checkBoxDefferredScrolling.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DEFERREDSCROLLING);
            checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ENABLEAUTOSORT);
            checkBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AUTOTOPPANEL);
        }

        public System.Drawing.Color ArchivedBkColor
        {
            get { return labelArchivedColor.BackColor; }
        }
        public System.Drawing.Color NotAvailableBkColor
        {
            get { return labelNotAvailable.BackColor; }
        }
        public System.Drawing.Color AvailableBkColor
        {
            get { return labelAvailable.BackColor; }
        }
        public System.Drawing.Color FoundBkColor
        {
            get { return labelFound.BackColor; }
        }
        public System.Drawing.Color OwnBkColor
        {
            get { return labelOwn.BackColor; }
        }
        public System.Drawing.Color ExtraCoordBkColor
        {
            get { return label12.BackColor; }
        }
        public bool DeferredScrolling
        {
            get { return checkBoxDefferredScrolling.Checked; }
        }
        public bool EnableAutomaticSorting
        {
            get { return checkBox1.Checked; }
        }
        public bool AutoTopPanel
        {
            get { return checkBox2.Checked; }
        }

        private void buttonBkArchived_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = labelArchivedColor.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                labelArchivedColor.BackColor = colorDialog1.Color;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = labelNotAvailable.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                labelNotAvailable.BackColor = colorDialog1.Color;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = labelAvailable.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                labelAvailable.BackColor = colorDialog1.Color;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = labelFound.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                labelFound.BackColor = colorDialog1.Color;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = labelOwn.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                labelOwn.BackColor = colorDialog1.Color;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = label11.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                label12.BackColor = colorDialog1.Color;
            }
        }
    }
}
