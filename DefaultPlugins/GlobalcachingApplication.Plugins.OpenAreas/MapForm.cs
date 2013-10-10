using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Web;

namespace GlobalcachingApplication.Plugins.OpenAreas
{
    public partial class MapForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Open areas";
        public const string STR_UPDATEMAP = "Update map";
        public const string STR_SELECTEDGEOCACHES = "Selected geocaches";
        public const string STR_MYSTERYIFCORRECTED = "Mystery, only if corrected";
        public const string STR_WAYPOINTS = "Waypoints";
        public const string STR_CUSTOMWAYPOINTS = "Custom waypoints";
        public const string STR_RADIUS = "Radius";
        public const string STR_FILLOPACITY = "Fill opacity";
        public const string STR_STROKEOPACITY = "Stroke opacity";
        public const string STR_GEOCACHE = "Geocache";
        public const string STR_WAYPOINT = "Waypoint";
        public const string STR_CUSTOM = "Custom";

        private string _defaultMultipleGeocachehtml = "";
        private string _defaultMultipleGeocachejs = "";

        public MapForm()
        {
            InitializeComponent();
        }

        public MapForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
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

            foreach (var cnt in core.WaypointTypes)
            {
                if (cnt.ID > 0)
                {
                    imageListWaypointType.Images.Add(cnt.ID.ToString(), Image.FromFile(Utils.ImageSupport.Instance.GetImagePath(core, Framework.Data.ImageSize.Small, cnt)));
                    listViewWaypointType.Items.Add(new ListViewItem(cnt.Name, imageListWaypointType.Images.Count - 1));
                }
            }

            Assembly assembly = Assembly.GetExecutingAssembly();
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.OpenAreas.MultipleGeocache.html")))
            {
                _defaultMultipleGeocachehtml = textStreamReader.ReadToEnd();
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.OpenAreas.MultipleGeocache.js")))
            {
                _defaultMultipleGeocachejs = textStreamReader.ReadToEnd();
            }

            checkBox3.Checked = Properties.Settings.Default.MysteryOnlyIfCorrected;
            checkBox2.Checked = Properties.Settings.Default.Waypoints;
            checkBox4.Checked = Properties.Settings.Default.CustomWaypoints;
            if (Properties.Settings.Default.CustomWaypointsList == null)
            {
                Properties.Settings.Default.CustomWaypointsList = new System.Collections.Specialized.StringCollection();
            }
            foreach (string s in Properties.Settings.Default.CustomWaypointsList)
            {
                listBox1.Items.Add(s);
            }
            numericUpDown1.Value = Properties.Settings.Default.Radius;
            numericUpDown2.Value = Properties.Settings.Default.FillOpacity;
            numericUpDown3.Value = Properties.Settings.Default.StrokeOpacity;
            label12.BackColor = Properties.Settings.Default.GeocacheColor;
            label13.BackColor = Properties.Settings.Default.WaypointColor;
            label16.BackColor = Properties.Settings.Default.CustomColor;

            SelectedLanguageChanged(this, EventArgs.Empty);
        }

        private void DisplayHtml(string html)
        {
            webBrowser1.Navigate("about:blank");
            if (webBrowser1.Document != null)
            {
                webBrowser1.Document.Write(string.Empty);
            }
            webBrowser1.DocumentText = html;
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

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_UPDATEMAP);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTEDGEOCACHES);
            this.checkBox3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MYSTERYIFCORRECTED);
            this.checkBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_WAYPOINTS);
            this.checkBox4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CUSTOMWAYPOINTS);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RADIUS);
            this.label5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FILLOPACITY);
            this.label9.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_STROKEOPACITY);
            this.label11.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GEOCACHE);
            this.label15.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_WAYPOINT);
            this.label18.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CUSTOM);
        }

        private void MapForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void MapForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void MapForm_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private string setIcons(string template)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("var foundIcon = new google.maps.MarkerImage(\"{0}\");", System.IO.Path.Combine(new string[] { System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Images", "Map", "cachetypes", "gevonden.png" }).Replace("\\", "\\\\")));
            sb.AppendLine(string.Format("var curposIcon = new google.maps.MarkerImage(\"{0}\");", System.IO.Path.Combine(new string[] { System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Images", "Map", "curpos.png" }).Replace("\\", "\\\\")));
            foreach (Framework.Data.GeocacheType gctype in Core.GeocacheTypes)
            {
                sb.AppendLine(string.Format("var gct{0}Icon = new google.maps.MarkerImage(\"{1}\");", gctype.ID.ToString().Replace("-", "_"), Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Map, gctype).Replace("\\", "\\\\")));
                sb.AppendLine(string.Format("var gct{0}IconC = new google.maps.MarkerImage(\"{1}\");", gctype.ID.ToString().Replace("-", "_"), Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Map, gctype, true).Replace("\\", "\\\\")));
            }
            foreach (Framework.Data.WaypointType wptype in Core.WaypointTypes)
            {
                sb.AppendLine(string.Format("var wpt{0}Icon = new google.maps.MarkerImage(\"{1}\");", wptype.ID.ToString().Replace("-", "_"), Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Map, wptype).Replace("\\", "\\\\")));
            }
            return template.Replace("//icons", sb.ToString());
        }

        private string addWaypointsToMap(string template, List<Framework.Data.Waypoint> wpList)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("var wpList = [");
            bool first = true;
            if (wpList != null)
            {
                foreach (Framework.Data.Waypoint wp in wpList)
                {
                    if (wp.Lat != null && wp.Lon != null)
                    {
                        if (!first)
                        {
                            sb.Append(",");
                        }
                        else
                        {
                            first = false;
                        }
                        StringBuilder bln = new StringBuilder();
                        bln.AppendFormat("{0}<br />", Utils.Conversion.GetCoordinatesPresentation((double)wp.Lat, (double)wp.Lon));
                        bln.AppendFormat("{0}<br />", wp.Code);
                        bln.AppendFormat("{0}<br />", HttpUtility.HtmlEncode(Utils.LanguageSupport.Instance.GetTranslation(wp.WPType.Name)));
                        bln.AppendFormat("{0}<br />", HttpUtility.HtmlEncode(wp.Description)).Replace("\r\n", "<br />");
                        bln.AppendFormat("{0}", HttpUtility.HtmlEncode(wp.Comment).Replace("\r\n", "<br />"));
                        sb.AppendLine(string.Format("{{a: '{0}', b: {1}, c: {2}, d: wpt{3}Icon, e: '{4}'}}", wp.Code, wp.Lat.ToString().Replace(',', '.'), wp.Lon.ToString().Replace(',', '.'), wp.WPType.ID.ToString().Replace("-", "_"), bln.ToString().Replace("'", "").Replace("\r", "").Replace("\n", "")));
                    }
                }
            }
            sb.Append("];");
            return template.Replace("//waypoints", sb.ToString());
        }

        private string addGeocachesToMap(string template, List<Framework.Data.Geocache> gcList)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("var gcList = [");
            bool first = true;
            foreach (Framework.Data.Geocache gc in gcList)
            {
                if (!first)
                {
                    sb.Append(",");
                }
                else
                {
                    first = false;
                }
                //if (gc.Found)
                //{
                //    sb.AppendLine(string.Format("{{a: '{0}', b: {1}, c: {2}, d: foundIcon}}", gc.Code, gc.Lat.ToString().Replace(',', '.'), gc.Lon.ToString().Replace(',', '.')));
                //}
                //else
                //{
                    if (gc.ContainsCustomLatLon)
                    {
                        sb.AppendLine(string.Format("{{a: '{0}', b: {1}, c: {2}, d: gct{3}IconC}}", gc.Code, gc.CustomLat.ToString().Replace(',', '.'), gc.CustomLon.ToString().Replace(',', '.'), gc.GeocacheType.ID.ToString().Replace("-", "_")));
                    }
                    else
                    {
                        sb.AppendLine(string.Format("{{a: '{0}', b: {1}, c: {2}, d: gct{3}Icon}}", gc.Code, gc.Lat.ToString().Replace(',', '.'), gc.Lon.ToString().Replace(',', '.'), gc.GeocacheType.ID.ToString().Replace("-", "_")));
                    }
                //}
            }
            sb.Append("];");
            return template.Replace("//geocaches", sb.ToString());
        }

        private string addCircelsToMap(string template, List<Framework.Data.Geocache> gcList, List<Framework.Data.Waypoint> wpList, List<Framework.Data.Location> custList)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("var circList = [");
            bool first = true;
            string radius = ((int)numericUpDown1.Value).ToString();
            string fillOpacity = ((double)numericUpDown2.Value / 100.0).ToString("0.##").Replace(',','.');
            string strokeOpacity = ((double)numericUpDown3.Value / 100.0).ToString("0.##").Replace(',', '.');
            string gcColor = string.Format("#{0}{1}{2}", label12.BackColor.R.ToString("x2"), label12.BackColor.G.ToString("x2"), label12.BackColor.B.ToString("x2"));
            string wpColor = string.Format("#{0}{1}{2}", label13.BackColor.R.ToString("x2"), label13.BackColor.G.ToString("x2"), label13.BackColor.B.ToString("x2"));
            string cuColor = string.Format("#{0}{1}{2}", label16.BackColor.R.ToString("x2"), label16.BackColor.G.ToString("x2"), label16.BackColor.B.ToString("x2"));
            foreach (Framework.Data.Geocache gc in gcList)
            {
                if (!first)
                {
                    sb.Append(",");
                }
                else
                {
                    first = false;
                }
                if (gc.ContainsCustomLatLon)
                {
                    sb.AppendLine(string.Format("{{a: {0}, b: {1}, c: {2}, d: {3}, e: {4}, f: '{5}'}}", gc.CustomLat.ToString().Replace(',', '.'), gc.CustomLon.ToString().Replace(',', '.'), radius, fillOpacity, strokeOpacity, gcColor));
                }
                else
                {
                    sb.AppendLine(string.Format("{{a: {0}, b: {1}, c: {2}, d: {3}, e: {4}, f: '{5}'}}", gc.Lat.ToString().Replace(',', '.'), gc.Lon.ToString().Replace(',', '.'), radius, fillOpacity, strokeOpacity, gcColor));
                }
            }
            foreach (Framework.Data.Waypoint wp in wpList)
            {
                if (!first)
                {
                    sb.Append(",");
                }
                else
                {
                    first = false;
                }
                sb.AppendLine(string.Format("{{a: {0}, b: {1}, c: {2}, d: {3}, e: {4}, f: '{5}'}}", wp.Lat.ToString().Replace(',', '.'), wp.Lon.ToString().Replace(',', '.'), radius, fillOpacity, strokeOpacity, wpColor));
            }
            foreach (Framework.Data.Location wp in custList)
            {
                if (!first)
                {
                    sb.Append(",");
                }
                else
                {
                    first = false;
                }
                sb.AppendLine(string.Format("{{a: {0}, b: {1}, c: {2}, d: {3}, e: {4}, f: '{5}'}}", wp.Lat.ToString().Replace(',', '.'), wp.Lon.ToString().Replace(',', '.'), radius, fillOpacity, strokeOpacity, cuColor));
            }
            sb.Append("];");
            return template.Replace("//circels", sb.ToString());
        }

        private string positionMap(string template, List<Framework.Data.Geocache> gcList)
        {
            double maxLat = gcList.Max(x => x.Lat);
            double minLat = gcList.Min(x => x.Lat);
            double maxLon = gcList.Max(x => x.Lon);
            double minLon = gcList.Min(x => x.Lon);

            string s = string.Format("map.fitBounds(new google.maps.LatLngBounds(new google.maps.LatLng({0},{1}), new google.maps.LatLng({2},{3})));", minLat.ToString().Replace(',', '.'), minLon.ToString().Replace(',', '.'), maxLat.ToString().Replace(',', '.'), maxLon.ToString().Replace(',', '.'));

            return template.Replace("//panToBounds", s);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool allUnknowns = !checkBox3.Checked;
            List<Framework.Data.Geocache> gcList = (from Framework.Data.Geocache g in Core.Geocaches where g.Selected && (g.GeocacheType.ID!=8 || allUnknowns ||g.ContainsCustomLatLon) select g).ToList();
            if (gcList.Count > 0)
            {
                List<Framework.Data.Waypoint> wpList = new List<Framework.Data.Waypoint>();
                List<Framework.Data.Location> custList = new List<Framework.Data.Location>();
                if (checkBox2.Checked)
                {
                    int[] wpFilter = (from ListViewItem ls in listViewWaypointType.Items where ls.Checked select (int)Utils.DataAccess.GetWaypointType(Core.WaypointTypes, ls.SubItems[0].Text).ID).ToArray();
                    if (wpFilter.Length > 0)
                    {
                        foreach (var gc in gcList)
                        {
                            List<Framework.Data.Waypoint> wpl = Utils.DataAccess.GetWaypointsFromGeocache(Core.Waypoints, gc.Code);
                            foreach (var wp in wpl)
                            {
                                if (wpFilter.Contains(wp.WPType.ID) && wp.Lat!=null && wp.Lon!=null)
                                {
                                    wpList.Add(wp);
                                }
                            }
                        }
                    }
                }
                if (checkBox4.Checked)
                {
                    custList = (from string sl in listBox1.Items select Utils.Conversion.StringToLocation(sl)).ToList();
                }
                string s = setIcons(_defaultMultipleGeocachejs);
                s = addWaypointsToMap(s, wpList);
                s = addGeocachesToMap(s, gcList);
                s = addCircelsToMap(s, gcList, wpList, custList);
                s = positionMap(s, gcList);
                s = _defaultMultipleGeocachehtml.Replace("<!-- %MultipleGeocache.js% -->", s);
                DisplayHtml(s);
                timer1.Enabled = true;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button3.Enabled = (listBox1.SelectedIndex >= 0);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                Properties.Settings.Default.CustomWaypointsList.Remove(listBox1.Items[listBox1.SelectedIndex].ToString());
                Properties.Settings.Default.Save();
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(Core, Core.CenterLocation))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string s = Utils.Conversion.GetCoordinatesPresentation(dlg.Result.Lat, dlg.Result.Lon);
                    listBox1.Items.Add(s);
                    Properties.Settings.Default.CustomWaypointsList.Add(s);
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MysteryOnlyIfCorrected = checkBox3.Checked;
            Properties.Settings.Default.Save();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Waypoints = checkBox2.Checked;
            Properties.Settings.Default.Save();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.CustomWaypoints = checkBox4.Checked;
            Properties.Settings.Default.Save();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Radius = (int)numericUpDown1.Value;
            Properties.Settings.Default.Save();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.FillOpacity = (int)numericUpDown2.Value;
            Properties.Settings.Default.Save();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.StrokeOpacity = (int)numericUpDown3.Value;
            Properties.Settings.Default.Save();
        }

        private void label12_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = label12.BackColor;
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                label12.BackColor = colorDialog1.Color;
                Properties.Settings.Default.GeocacheColor = colorDialog1.Color;
                Properties.Settings.Default.Save();
            }
        }

        private void label13_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = label13.BackColor;
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                label13.BackColor = colorDialog1.Color;
                Properties.Settings.Default.WaypointColor = colorDialog1.Color;
                Properties.Settings.Default.Save();
            }
        }

        private void label16_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = label16.BackColor;
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                label16.BackColor = colorDialog1.Color;
                Properties.Settings.Default.CustomColor = colorDialog1.Color;
                Properties.Settings.Default.Save();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (webBrowser1.ReadyState == WebBrowserReadyState.Complete)
                {
                    object o = executeScript("getSelectedGeocache", null);
                    if (o != null)
                    {
                        string s = (string)o;
                        if (s.Length > 0 && (Core.ActiveGeocache == null || Core.ActiveGeocache.Code != s))
                        {
                            Core.ActiveGeocache = Utils.DataAccess.GetGeocache(Core.Geocaches, s);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void MapForm_Leave(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }

        private void MapForm_Enter(object sender, EventArgs e)
        {
            if (webBrowser1.ReadyState == WebBrowserReadyState.Complete)
            {
                timer1.Enabled = true;
            }
        }

    }
}
