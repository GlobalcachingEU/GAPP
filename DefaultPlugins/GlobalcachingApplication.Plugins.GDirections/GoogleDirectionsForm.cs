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

namespace GlobalcachingApplication.Plugins.GDirections
{
    public partial class GoogleDirectionsForm : Form
    {
        public const string STR_TITLE = "Create route for selected geocaches";
        public const string STR_CUSTOMWP = "User waypoint";
        public const string STR_HOME = "Home";
        public const string STR_CENTER = "Center";
        public const string STR_TOTALDISTANCE = "Total Distance";
        public const string STR_START = "Start";
        public const string STR_END = "End";
        public const string STR_STOPS = "Stops (max {0})";
        public const string STR_AUTOMATICROUTING = "Automatic routing";
        public const string STR_AVWAYPOINTS = "Available waypoints (max {0})";
        public const string STR_ADDTOSTOPS = "Add to stops";
        public const string STR_ADDWAYPOINT = "Add waypoint...";
        public const string STR_CREATEROUTE = "Create route";
        public const string STR_PRINT = "Print";

        private Framework.Interfaces.ICore _core = null;
        private Utils.BasePlugin.Plugin _plugin = null;

        public class LocationInfo
        {
            public Framework.Data.Location Location { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        public GoogleDirectionsForm()
        {
            InitializeComponent();
        }

        public GoogleDirectionsForm(Utils.BasePlugin.Plugin plugin, Framework.Interfaces.ICore core)
            : this()
        {
            _core = core;
            _plugin = plugin;

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_START);
            this.label3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_END);
            this.label2.Text = string.Format(Utils.LanguageSupport.Instance.GetTranslation(STR_STOPS), 8);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AUTOMATICROUTING);
            this.label4.Text = string.Format(Utils.LanguageSupport.Instance.GetTranslation(STR_AVWAYPOINTS), 200);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDTOSTOPS);
            this.button5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDWAYPOINT);
            this.button6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CREATEROUTE);
            this.button7.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PRINT);

            Assembly assembly = Assembly.GetExecutingAssembly();
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.GDirections.content.html")))
            {
                string doc = textStreamReader.ReadToEnd();
                doc = doc.Replace("//defaultCenter", string.Format("var startCenter = new google.maps.LatLng({0});",_core.CenterLocation.SLatLon));
                doc = doc.Replace("Total Distance", Utils.LanguageSupport.Instance.GetTranslation(STR_TOTALDISTANCE));
                webBrowser1.Navigate("about:blank");
                if (webBrowser1.Document != null)
                {
                    webBrowser1.Document.Write(string.Empty);
                }
                webBrowser1.DocumentText = doc;
            }

            LocationInfo li = new LocationInfo();
            li.Location = new Framework.Data.Location(_core.HomeLocation.Lat, _core.HomeLocation.Lon);
            li.Name = string.Format("{0}", Utils.LanguageSupport.Instance.GetTranslation(STR_HOME));
            listBox2.Items.Add(li);
            comboBox1.Items.Add(li);
            comboBox2.Items.Add(li);

            li = new LocationInfo();
            li.Location = new Framework.Data.Location(_core.CenterLocation.Lat, _core.CenterLocation.Lon);
            li.Name = string.Format("{0}", Utils.LanguageSupport.Instance.GetTranslation(STR_CENTER));
            listBox2.Items.Add(li);
            comboBox1.Items.Add(li);
            comboBox2.Items.Add(li);

            var geocaches = Utils.DataAccess.GetSelectedGeocaches(_core.Geocaches).Take(200);
            foreach (Framework.Data.Geocache gc in geocaches)
            {
                li = new LocationInfo();
                li.Location = new Framework.Data.Location(gc.Lat, gc.Lon);
                li.Name = gc.Name ?? "";

                listBox2.Items.Add(li);
                comboBox1.Items.Add(li);
                comboBox2.Items.Add(li);
            }
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

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = (listBox1.Items.Count < 8 && listBox2.SelectedIndex >= 0);
        }

        private void checkCanCalculate()
        {
            button6.Enabled = (comboBox1.SelectedItem != null && comboBox2.SelectedItem != null && (comboBox1.SelectedItem != comboBox2.SelectedItem || listBox1.Items.Count>0));
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //webBrowser1.Print();
            webBrowser1.ShowPrintDialog();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(_core, _core.CenterLocation))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LocationInfo li = new LocationInfo();
                    li.Location = new Framework.Data.Location(dlg.Result.Lat, dlg.Result.Lon);
                    li.Name = string.Format("{0} - {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_CUSTOMWP), listBox2.Items.Count);
                    listBox2.Items.Add(li);
                    comboBox1.Items.Add(li);
                    comboBox2.Items.Add(li);
                }
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkCanCalculate();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkCanCalculate();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button2.Enabled = listBox1.SelectedIndex >= 0;
            checkUpDownStops();
        }

        private void checkUpDownStops()
        {
            button4.Enabled = listBox1.SelectedIndex > 0;
            button3.Enabled = listBox1.Items.Count > 1 && listBox1.SelectedIndex>=0 && listBox1.SelectedIndex < listBox1.Items.Count - 1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            }
            checkCanCalculate();
            checkUpDownStops();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem !=null)
            {
                if (listBox1.Items.IndexOf(listBox2.SelectedItem) < 0)
                {
                    listBox1.Items.Add(listBox2.SelectedItem);
                }
            }
            checkCanCalculate();
            checkUpDownStops();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("var request = {");
            sb.AppendFormat("origin: '{0}',", (comboBox1.SelectedItem as LocationInfo).Location.SLatLon);
            sb.AppendFormat("destination: '{0}',", (comboBox2.SelectedItem as LocationInfo).Location.SLatLon);
            if (listBox1.Items.Count > 0)
            {
                sb.AppendFormat("waypoints:[{{location: '{0}'}}", (listBox1.Items[0] as LocationInfo).Location.SLatLon);
                for (int i=1; i<listBox1.Items.Count; i++)
                {
                    sb.AppendFormat(",{{location: '{0}'}}", (listBox1.Items[i] as LocationInfo).Location.SLatLon);
                }
                sb.Append("],");
            }
            sb.AppendFormat("optimizeWaypoints: {0},", checkBox1.Checked ? "true" : "false");
            sb.Append("travelMode: google.maps.DirectionsTravelMode.DRIVING");
            sb.Append("};");

            executeScript("calcRoute", new object[] { sb.ToString() });
        }

        private void listBox2_DoubleClick(object sender, EventArgs e)
        {
            if (button1.Enabled)
            {
                button1_Click(this, EventArgs.Empty);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count > 1 && listBox1.SelectedIndex >= 0 && listBox1.SelectedIndex < listBox1.Items.Count - 1)
            {
                int index = listBox1.SelectedIndex;
                object o = listBox1.Items[index];
                listBox1.Items.RemoveAt(index);
                listBox1.Items.Insert(index + 1, o);
                listBox1.SelectedIndex = index + 1;
                checkUpDownStops();
                checkBox1.Checked = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex > 0)
            {
                int index = listBox1.SelectedIndex;
                object o = listBox1.Items[index];
                listBox1.Items.RemoveAt(index);
                listBox1.Items.Insert(index - 1, o);
                listBox1.SelectedIndex = index - 1;
                checkUpDownStops();
                checkBox1.Checked = false;
            }
        }
    }
}
