using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace GlobalcachingApplication.Plugins.ExportGarmin
{
    public partial class GarminCommunicatorForm : Form
    {
        public const string STR_TITLE = "Export to Garmin Device";
        public const string STR_START = "Start";
        public const string STR_CANCEL = "Cancel";
        public const string STR_INCLNOTES = "Include notes in description";
        public const string STR_ADDCHILDWAYPOINTS = "Add child waypoints";
        public const string STR_USENAME = "Use name and not geocache code";
        public const string STR_MAXNAMELENGTH = "Maximum geocache name length";
        public const string STR_MINSTARTNAME = "Minimum start of name length";
        public const string STR_ADDWPTTODESCR = "Add additional waypoints to description";
        public const string STR_USEHINTSDESCR = "Use the hints for description";
        public const string STR_GPXVERSION = "GPX version";
        public const string STR_EXTRACOORDNAMEPREFIX = "Extra coord. name prefix";

        private TemporaryFile tmpFile = null;
        private Framework.Interfaces.ICore _core = null;
        private List<Framework.Data.Geocache> _gcList = null;
        private bool _running = false;
        private int _nextIndex = 0;

        public GarminCommunicatorForm()
        {
            InitializeComponent();
        }

        public GarminCommunicatorForm(Framework.Interfaces.ICore core, List<Framework.Data.Geocache> gcList)
        {
            InitializeComponent();

            _core = core;
            _gcList = gcList;

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }
            numericUpDown1.Value = Properties.Settings.Default.MaxGeocacheNameLength;
            numericUpDown2.Value = Properties.Settings.Default.MinStartOfGeocacheName;
            checkBox1.Checked = Properties.Settings.Default.AddFieldNotesToDescription;
            checkBox2.Checked = Properties.Settings.Default.AddChildWaypoints;
            checkBox3.Checked = Properties.Settings.Default.UseNameAndNotCode;
            checkBox4.Checked = Properties.Settings.Default.AddWaypointsToDescription;
            checkBox5.Checked = Properties.Settings.Default.UseHintsForDescription;
            comboBox2.Items.Add(Utils.GPXGenerator.V100);
            comboBox2.Items.Add(Utils.GPXGenerator.V101);
            comboBox2.Items.Add(Utils.GPXGenerator.V102);
            textBox1.Text = Properties.Settings.Default.CorrectedNamePrefix ?? "";
            if (!string.IsNullOrEmpty(Properties.Settings.Default.GPXVersionStr))
            {
                comboBox2.SelectedItem = Version.Parse(Properties.Settings.Default.GPXVersionStr);
            }
            else
            {
                comboBox2.SelectedIndex = 0;
            }

            button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_START);
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_INCLNOTES);
            this.checkBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDCHILDWAYPOINTS);
            this.checkBox3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_USENAME);
            this.checkBox4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDWPTTODESCR);
            this.checkBox5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_USEHINTSDESCR);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAXNAMELENGTH);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MINSTARTNAME);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GPXVERSION);
            this.label10.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXTRACOORDNAMEPREFIX);

            tmpFile = new TemporaryFile(true);
            using (StreamReader textStreamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("GlobalcachingApplication.Plugins.ExportGarmin.GarminCommunicator.html")))
            {
                File.WriteAllText(tmpFile.Path, textStreamReader.ReadToEnd());
            }
            webBrowser1.Navigate(string.Format("file://{0}", tmpFile.Path));

            toolStripStatusLabel1.Text = string.Format("0/{0}", gcList.Count);
            toolStripProgressBar1.Maximum = gcList.Count;
        }

        private object executeScript(string script, object[] pars)
        {
            if (pars == null)
            {
                return webBrowser1.Document.InvokeScript(script);
            }
            else
            {
                return webBrowser1.Document.InvokeScript(script, pars);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _running = !_running;
            if (_running)
            {
                Properties.Settings.Default.AddFieldNotesToDescription = checkBox1.Checked;
                Properties.Settings.Default.AddChildWaypoints = checkBox2.Checked;
                Properties.Settings.Default.UseNameAndNotCode = checkBox3.Checked;
                Properties.Settings.Default.AddWaypointsToDescription = checkBox4.Checked;
                Properties.Settings.Default.UseHintsForDescription = checkBox5.Checked;
                Properties.Settings.Default.CorrectedNamePrefix = textBox1.Text;
                if (comboBox2.SelectedItem as Version == null)
                {
                    Properties.Settings.Default.GPXVersionStr = "";
                }
                else
                {
                    Properties.Settings.Default.GPXVersionStr = (comboBox2.SelectedItem as Version).ToString();
                }
                Properties.Settings.Default.Save();
                button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CANCEL);
            }
            else
            {
                button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_START);
                //forget it
                timer1.Enabled = false;
                Close();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Visible)
            {
                try
                {
                    bool ready = (bool)executeScript("isReady", null);
                    if (ready)
                    {
                        if (_running)
                        {
                            if (_nextIndex < _gcList.Count)
                            {
                                List<Framework.Data.Geocache> gcList = new List<Framework.Data.Geocache>();
                                gcList.Add(_gcList[_nextIndex]);

                                Utils.GPXGenerator gpxGenerator = new Utils.GPXGenerator(_core, gcList, string.IsNullOrEmpty(Properties.Settings.Default.GPXVersionStr) ? Utils.GPXGenerator.V101 : Version.Parse(Properties.Settings.Default.GPXVersionStr));
                                gpxGenerator.AddFieldnotesToDescription = Properties.Settings.Default.AddFieldNotesToDescription;
                                gpxGenerator.MaxNameLength = Properties.Settings.Default.MaxGeocacheNameLength;
                                gpxGenerator.MinStartOfname = Properties.Settings.Default.MinStartOfGeocacheName;
                                gpxGenerator.UseNameForGCCode = Properties.Settings.Default.UseNameAndNotCode;
                                gpxGenerator.AddAdditionWaypointsToDescription = Properties.Settings.Default.AddWaypointsToDescription;
                                gpxGenerator.UseHintsForDescription = Properties.Settings.Default.UseHintsForDescription;
                                gpxGenerator.ExtraCoordPrefix = Properties.Settings.Default.CorrectedNamePrefix;
                                
                                using (System.IO.TemporaryFile gpxFile = new System.IO.TemporaryFile(true))
                                {
                                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(gpxFile.Path, false, Encoding.UTF8))
                                    {
                                        int block = 0;
                                        //generate header
                                        sw.Write(gpxGenerator.Start());
                                        //preserve mem and do for each cache the export
                                        for (int i = 0; i < gpxGenerator.Count; i++)
                                        {
                                            sw.WriteLine(gpxGenerator.Next());
                                            if (Properties.Settings.Default.AddChildWaypoints)
                                            {
                                                string s = gpxGenerator.WaypointData();
                                                if (!string.IsNullOrEmpty(s))
                                                {
                                                    sw.WriteLine(s);
                                                }
                                            }
                                            block++;
                                            if (block > 10)
                                            {
                                                block = 0;
                                            }
                                        }
                                        //finalize
                                        sw.Write(gpxGenerator.Finish());
                                    }
                                    executeScript("uploadGpx", new object[] { System.IO.File.ReadAllText(gpxFile.Path), string.Format("{0}.gpx", gcList[0].Code) });
                                }
                                _nextIndex++;
                                toolStripStatusLabel1.Text = string.Format("{0}/{1}", _nextIndex, _gcList.Count);
                                toolStripProgressBar1.Value = _nextIndex;
                            }
                            else
                            {
                                //done, close
                                timer1.Enabled = false;
                                Close();
                            }
                        }
                        else
                        {
                            button1.Enabled = true;
                        }
                    }
                }
                catch
                {
                }
            }
            else
            {
                timer1.Enabled = false;
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            timer1.Enabled = true;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MaxGeocacheNameLength = (int)numericUpDown1.Value;
            Properties.Settings.Default.Save();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MinStartOfGeocacheName = (int)numericUpDown2.Value;
            Properties.Settings.Default.Save();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AddFieldNotesToDescription = checkBox1.Checked;
            Properties.Settings.Default.Save();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AddWaypointsToDescription = checkBox4.Checked;
            Properties.Settings.Default.Save();
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.UseHintsForDescription = checkBox5.Checked;
            Properties.Settings.Default.Save();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.Text = Properties.Settings.Default.CorrectedNamePrefix;
            Properties.Settings.Default.Save();
        }

    }
}
