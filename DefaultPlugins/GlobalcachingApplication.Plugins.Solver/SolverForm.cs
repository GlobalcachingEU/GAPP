using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gavaghan.Geodesy;

namespace GlobalcachingApplication.Plugins.Solver
{
    public partial class SolverForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Solver";
        public const string STR_TEXT = "Text";
        public const string STR_PROJECTION = "Projection";
        public const string STR_SOLVER = "Solver";
        public const string STR_RESULT = "Result";
        public const string STR_PROJFROMCOOR = "Projection from coordinate";
        public const string STR_DISTANCE = "Distance";
        public const string STR_ANGLE = "Angle";
        public const string STR_PROJLOC = "Projected location";
        public const string STR_MILES = "miles";
        public const string STR_METERS = "meters";
        public const string STR_DEGREES = "degrees";
        public const string STR_COORDINATE = "Coordinate";
        public const string STR_ANGLEANDDEGREES = "Angle and distance";
        public const string STR_DUTCHGRID = "Dutch grid";
        public const string STR_CONVERSION = "Conversion";
        public const string STR_DUTCHFRIDINFO = "Coordinate in WGS84 (latitude, longitude) or Dutch grid (x y)";

        public SolverForm()
        {
            InitializeComponent();
        }

        public SolverForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            if (Properties.Settings.Default.WindowPos != null && !Properties.Settings.Default.WindowPos.IsEmpty)
            {
                this.Bounds = Properties.Settings.Default.WindowPos;
                this.StartPosition = FormStartPosition.Manual;
            }

            SelectedLanguageChanged(this, EventArgs.Empty);

            comboBox1.Items.Add(new TextSolverRot13());
            comboBox1.Items.Add(new TextSolverWordValue());
            comboBox1.Items.Add(new TextSolverASCII());
            comboBox1.Items.Add(new TextSolverFrequency());
            comboBox1.Items.Add(new TextSolverCipher());

            comboBox1.SelectedItem = comboBox1.Items[0];
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.tabPage1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TEXT);
            this.tabPage2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PROJECTION);
            this.tabPage3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ANGLEANDDEGREES);
            this.tabPage4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DUTCHGRID);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TEXT);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SOLVER);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RESULT);
            this.label5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PROJFROMCOOR);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DISTANCE);
            this.label7.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ANGLE);
            this.radioButton1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_METERS);
            this.radioButton2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MILES);
            this.label9.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PROJLOC);
            this.label8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DEGREES);
            this.label14.Text = string.Format("{0} 1", Utils.LanguageSupport.Instance.GetTranslation(STR_COORDINATE));
            this.label17.Text = string.Format("{0} 2", Utils.LanguageSupport.Instance.GetTranslation(STR_COORDINATE));
            this.label19.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DISTANCE);
            this.label24.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ANGLE);
            this.label20.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_METERS);
            this.label21.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MILES);
            this.label22.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DEGREES);
            this.label25.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COORDINATE);
            this.label29.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CONVERSION);
            this.label27.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DUTCHFRIDINFO);
        }

        private void SolverForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void SolverForm_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void SolverForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox2.Text = (comboBox1.SelectedItem as TextSolver).Process(textBox1.Text);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox2.Text = (comboBox1.SelectedItem as TextSolver).Process(textBox1.Text);
        }

        private void calculateProjection()
        {
            Framework.Data.Location ll = Utils.Conversion.StringToLocation(textBox3.Text);
            if (ll != null)
            {
                GeodeticCalculator gc = new GeodeticCalculator();
                GlobalCoordinates p = gc.CalculateEndingGlobalCoordinates(Ellipsoid.WGS84, new GlobalCoordinates(new Angle(ll.Lat), new Angle(ll.Lon)), new Angle((double)numericUpDown2.Value), radioButton1.Checked ? (double)numericUpDown1.Value : 1609.26939 * (double)numericUpDown2.Value);
                textBox4.Text = Utils.Conversion.GetCoordinatesPresentation(p.Latitude.Degrees, p.Longitude.Degrees);
            }
            else
            {
                textBox4.Text = "";
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            calculateProjection();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            calculateProjection();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            calculateProjection();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            calculateProjection();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            calculateProjection();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(Core, null))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textBox3.Text = Utils.Conversion.GetCoordinatesPresentation(dlg.Result.Lat, dlg.Result.Lon);
                }
            }
        }

        private void calculateAngleAndDegrees()
        {
            Framework.Data.Location ll1 = Utils.Conversion.StringToLocation(textBox5.Text);
            Framework.Data.Location ll2 = Utils.Conversion.StringToLocation(textBox6.Text);
            if (ll1 != null && ll2 != null)
            {
                GeodeticMeasurement gm = Utils.Calculus.CalculateDistance(ll1.Lat, ll1.Lon, ll2.Lat, ll2.Lon);
                textBox7.Text = gm.EllipsoidalDistance.ToString("0");
                textBox8.Text = (0.0006214 * gm.EllipsoidalDistance).ToString("0.000");
                textBox9.Text = gm.Azimuth.Degrees.ToString("0");
            }
            else
            {
                textBox7.Text = "";
                textBox8.Text = "";
                textBox9.Text = "";
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            calculateAngleAndDegrees();
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            calculateAngleAndDegrees();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(Core, null))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textBox5.Text = Utils.Conversion.GetCoordinatesPresentation(dlg.Result.Lat, dlg.Result.Lon);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(Core, null))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textBox6.Text = Utils.Conversion.GetCoordinatesPresentation(dlg.Result.Lat, dlg.Result.Lon);
                }
            }
        }

        private void convertCoordinate()
        {
            Framework.Data.Location ll = Utils.Conversion.StringToLocation(textBox10.Text);
            if (ll != null)
            {
                double x;
                double y;
                if (Utils.Calculus.RDFromLatLong(ll.Lat, ll.Lon, out x, out y))
                {
                    textBox11.Text = string.Format("{0:0} {1:0}",x,y);;
                }
                else
                {
                    textBox11.Text = "";
                }
            }
            else
            {
                try
                {
                    string[] parts = textBox10.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    double x = Utils.Conversion.StringToDouble(parts[0]);
                    double y = Utils.Conversion.StringToDouble(parts[1]);
                    ll = Utils.Calculus.LocationFromRD(x, y);
                    if (ll != null)
                    {
                        textBox11.Text = Utils.Conversion.GetCoordinatesPresentation(ll.Lat, ll.Lon); ;
                    }
                    else
                    {
                        textBox11.Text = "";
                    }
                }
                catch
                {
                    textBox11.Text = "";
                }
            }
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            convertCoordinate();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(Core, null))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textBox10.Text = Utils.Conversion.GetCoordinatesPresentation(dlg.Result.Lat, dlg.Result.Lon);
                }
            }
        }

    }
}
