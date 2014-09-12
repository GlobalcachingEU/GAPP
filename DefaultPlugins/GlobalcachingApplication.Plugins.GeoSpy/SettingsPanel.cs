using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.GeoSpy
{
    public partial class SettingsPanel : UserControl
    {
        public const string STR_SETTYPE = "Set GeoSpy type in GPX export";

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

            comboBox1.Text = Properties.Settings.Default.GPXTagCivil;
            comboBox2.Text = Properties.Settings.Default.GPXTagHistoricAndReligious;
            comboBox3.Text = Properties.Settings.Default.GPXTagNatural;
            comboBox4.Text = Properties.Settings.Default.GPXTagTechnical;
            comboBox5.Text = Properties.Settings.Default.GPXTagMilitary;
        }

        public void Apply()
        {
            Properties.Settings.Default.GPXTagCivil = comboBox1.Text;
            Properties.Settings.Default.GPXTagHistoricAndReligious = comboBox2.Text;
            Properties.Settings.Default.GPXTagNatural = comboBox3.Text;
            Properties.Settings.Default.GPXTagTechnical = comboBox4.Text;
            Properties.Settings.Default.GPXTagMilitary = comboBox5.Text;
            Properties.Settings.Default.Save();
        }
    }
}
