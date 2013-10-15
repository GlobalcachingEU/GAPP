using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Web;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public partial class ImportByRadiusForm : Form
    {
        public const string STR_TITLE = "Import geocaches within radius";
        public const string STR_AREA = "Area";
        public const string STR_LOCATION = "Location";
        public const string STR_RADIUS = "Radius";
        public const string STR_KM = "km";
        public const string STR_MILES = "miles";
        public const string STR_MODIFIEDSINCE = "Modified since";
        public const string STR_OK = "OK";

        private Framework.Interfaces.ICore _core = null;

        public string Filter { get; private set; }
        public Framework.Data.Location Center { get; private set; }
        public double RadiusKm { get; private set; }

        public ImportByRadiusForm()
        {
            InitializeComponent();
        }

        public ImportByRadiusForm(Framework.Interfaces.ICore core):this()
        {
            _core = core;

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AREA);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOCATION);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RADIUS);
            this.radioButtonKm.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_KM);
            this.radioButtonMiles.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MILES);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MODIFIEDSINCE);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);
        }

        private void buttonLocation_Click(object sender, EventArgs e)
        {
            using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(_core, _core.CenterLocation))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Center = dlg.Result;
                    textBoxLocation.Text = Utils.Conversion.GetCoordinatesPresentation(Center);
                }
            }
        }

        private void textBoxLocation_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = textBoxLocation.Text.Length > 0;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            dateTimePicker1.Enabled = checkBox1.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Center != null)
            {
                RadiusKm = (double)numericUpDownRadius.Value;
                if (radioButtonMiles.Checked)
                {
                    RadiusKm *= 1.6214;
                }
                if (checkBox1.Checked)
                {
                    Filter = string.Format("modified_since={0}Z", HttpUtility.UrlEncode(dateTimePicker1.Value.Date.ToUniversalTime().ToString("s")));
                }
                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
        }
    }
}
