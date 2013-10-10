using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Munzee
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_SETTYPE = "Set Munzee type in GPX export";

        public SettingsPanel()
        {
            InitializeComponent();
        }

        public SettingsPanel(Framework.Interfaces.ICore core): this()
        {

            label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SETTYPE);

            string[] list = (from Framework.Data.GeocacheType gt in core.GeocacheTypes where gt.ID > 0 && gt.ID < 9000 select gt.GPXTag).ToArray();
            comboBox1.Items.AddRange(list);
            comboBox2.Items.AddRange(list);
            comboBox3.Items.AddRange(list);
            comboBox4.Items.AddRange(list);
            comboBox5.Items.AddRange(list);
            comboBox6.Items.AddRange(list);
            comboBox7.Items.AddRange(list);

            comboBox1.Text = Properties.Settings.Default.GPXTagMunzee;
            comboBox2.Text = Properties.Settings.Default.GPXTagVirtual;
            comboBox3.Text = Properties.Settings.Default.GPXTagMaintenance;
            comboBox4.Text = Properties.Settings.Default.GPXTagMystery;
            comboBox5.Text = Properties.Settings.Default.GPXTagNFC;
            comboBox6.Text = Properties.Settings.Default.GPXTagPremium;
            comboBox7.Text = Properties.Settings.Default.GPXTagBusiness;
        }

        public void Apply()
        {
            Properties.Settings.Default.GPXTagMunzee = comboBox1.Text;
            Properties.Settings.Default.GPXTagVirtual = comboBox2.Text;
            Properties.Settings.Default.GPXTagMaintenance = comboBox3.Text;
            Properties.Settings.Default.GPXTagMystery = comboBox4.Text;
            Properties.Settings.Default.GPXTagNFC = comboBox5.Text;
            Properties.Settings.Default.GPXTagPremium = comboBox6.Text;
            Properties.Settings.Default.GPXTagBusiness = comboBox7.Text;
            Properties.Settings.Default.Save();
        }
    }
}
