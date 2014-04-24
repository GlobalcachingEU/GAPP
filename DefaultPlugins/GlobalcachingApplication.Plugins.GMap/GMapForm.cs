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

namespace GlobalcachingApplication.Plugins.GMap
{
    public partial class GMapForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Google map";
        public const string STR_ACTIVE = "Active";
        public const string STR_SELECTED = "Selected";
        public const string STR_SELECTEDMARKERS = "Selected markers";
        public const string STR_ALL = "All";
        public const string STR_SHOWGEOCACHES = "Show geocaches";
        public const string STR_AREAS = "Areas";
        public const string STR_LEVEL = "Level";
        public const string STR_NAME = "Name";
        public const string STR_ADD = "Add";
        public const string STR_LOADING = "Loading";
        public const string STR_LOCATION = "Location";
        public const string STR_GO = "Go";

        private string _defaultSingleGeocachehtml = "";
        private string _defaultSingleGeocachejs = "";
        private string _defaultMultipleGeocachehtml = "";
        private string _defaultMultipleGeocachejs = "";

        private enum MapType
        {
            None,
            SingleGeocache,
            SelectedGeocaches,
            Allgeocaches,
        }
        private MapType _activeMapType = MapType.None;

        public enum MapUpdateReason
        {
            Init,
            ActiveGeocacheChanged,
            SelectedChanged,
            DataChanged,
            MapSettingsChanged,
        }

        public GMapForm()
        {
            InitializeComponent();
        }

        public GMapForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            checkBox1.Checked = Properties.Settings.Default.AddSelectedMarkers;

            if (Properties.Settings.Default.WindowPos != null && !Properties.Settings.Default.WindowPos.IsEmpty)
            {
                this.Bounds = Properties.Settings.Default.WindowPos;
                this.StartPosition = FormStartPosition.Manual;
            }

            Assembly assembly = Assembly.GetExecutingAssembly();
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.GMap.SingleGeocache.html")))
            {
                _defaultSingleGeocachehtml = textStreamReader.ReadToEnd();
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.GMap.SingleGeocache.js")))
            {
                _defaultSingleGeocachejs = textStreamReader.ReadToEnd();
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.GMap.MultipleGeocache.html")))
            {
                _defaultMultipleGeocachehtml = textStreamReader.ReadToEnd();
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.GMap.MultipleGeocache.js")))
            {
                _defaultMultipleGeocachejs = textStreamReader.ReadToEnd();
            }

            core.ActiveGeocacheChanged += new Framework.EventArguments.GeocacheEventHandler(core_ActiveGeocacheChanged);
            core.Geocaches.SelectedChanged += new Framework.EventArguments.GeocacheEventHandler(Geocaches_SelectedChanged);
            core.Geocaches.ListSelectionChanged += new EventHandler(Geocaches_ListSelectionChanged);
            core.Geocaches.DataChanged += new Framework.EventArguments.GeocacheEventHandler(Geocaches_DataChanged);
            core.Geocaches.GeocacheAdded += new Framework.EventArguments.GeocacheEventHandler(Geocaches_GeocacheAdded);
            core.Geocaches.GeocacheRemoved += new Framework.EventArguments.GeocacheEventHandler(Geocaches_GeocacheAdded);
            core.Geocaches.ListDataChanged += new EventHandler(Geocaches_ListDataChanged);
            core.GeocachingComAccountChanged += new Framework.EventArguments.GeocacheComAccountEventHandler(core_GeocachingComAccountChanged);
            core.GPSLocation.Changed += new Framework.EventArguments.GPSLocationEventHandler(GPSLocation_Changed);
            core.Waypoints.ListDataChanged += new EventHandler(Waypoints_ListDataChanged);
            core.Waypoints.DataChanged += new Framework.EventArguments.WaypointEventHandler(Waypoints_DataChanged);

            SelectedLanguageChanged(this, EventArgs.Empty);
        }

        void Waypoints_DataChanged(object sender, Framework.EventArguments.WaypointEventArgs e)
        {
            if (Visible)
            {
                UpdateView(MapUpdateReason.SelectedChanged);
            }
        }

        void Waypoints_ListDataChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                UpdateView(MapUpdateReason.SelectedChanged);
            }            
        }

        void GPSLocation_Changed(object sender, Framework.EventArguments.GPSLocationEventArgs e)
        {
            if (Visible)
            {
                executeScript("setCurrentPosition", new object[] { e.Location.Valid, e.Location.Position.Lat, e.Location.Position.Lon });
                if (button3.Visible != e.Location.Valid)
                {
                    button3.Visible = e.Location.Valid;
                }
            }
        }

        void Geocaches_ListSelectionChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                UpdateView(MapUpdateReason.SelectedChanged);
            }
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            base.SelectedLanguageChanged(sender, e);
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.radioButtonActive.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ACTIVE);
            this.radioButtonAll.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ALL);
            this.radioButtonSelected.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTED);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHOWGEOCACHES);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AREAS);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LEVEL);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTEDMARKERS);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADD);
            if (Visible)
            {
                _activeMapType = MapType.None;
                UpdateView(MapUpdateReason.DataChanged);
            }
        }

        void core_GeocachingComAccountChanged(object sender, Framework.EventArguments.GeocacheComAccountEventArgs e)
        {
            if (Visible)
            {
                UpdateView(MapUpdateReason.DataChanged);
            }
        }

        void Geocaches_ListDataChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                UpdateView(MapUpdateReason.DataChanged);
            }
        }

        void Geocaches_GeocacheAdded(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (Visible)
            {
                UpdateView(MapUpdateReason.DataChanged);
            }
        }

        void Geocaches_DataChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (Visible)
            {
                UpdateView(MapUpdateReason.DataChanged);
            }
        }

        void Geocaches_SelectedChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (Visible)
            {
                UpdateView(MapUpdateReason.SelectedChanged);
            }
        }

        void core_ActiveGeocacheChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (Visible)
            {
                UpdateView(MapUpdateReason.ActiveGeocacheChanged);
            }
        }

        private void GMapForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                _activeMapType = MapType.None;
                e.Cancel = true;
                Hide();
            }
            else
            {
                Core.ActiveGeocacheChanged -= new Framework.EventArguments.GeocacheEventHandler(core_ActiveGeocacheChanged);
                Core.Geocaches.SelectedChanged -= new Framework.EventArguments.GeocacheEventHandler(Geocaches_SelectedChanged);
                Core.Geocaches.DataChanged -= new Framework.EventArguments.GeocacheEventHandler(Geocaches_DataChanged);
                Core.Geocaches.GeocacheAdded -= new Framework.EventArguments.GeocacheEventHandler(Geocaches_GeocacheAdded);
                Core.Geocaches.GeocacheRemoved -= new Framework.EventArguments.GeocacheEventHandler(Geocaches_GeocacheAdded);
                Core.Geocaches.ListDataChanged -= new EventHandler(Geocaches_ListDataChanged);
                Core.GeocachingComAccountChanged -= new Framework.EventArguments.GeocacheComAccountEventHandler(core_GeocachingComAccountChanged);
            }
        }

        private string setIcons(string template)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("var foundIcon = new google.maps.MarkerImage(\"{0}\");", System.IO.Path.Combine(new string[] { System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Images", "Map", "cachetypes", "gevonden.png" }).Replace("\\", "\\\\")));
            sb.AppendLine(string.Format("var curposIcon = new google.maps.MarkerImage(\"{0}\");", System.IO.Path.Combine(new string[] { System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Images", "Map", "curpos.png" }).Replace("\\", "\\\\")));
            sb.AppendLine(string.Format("var selectedIcon = new google.maps.MarkerImage(\"{0}\");", System.IO.Path.Combine(new string[] { System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Images", "Map", "selected.png" }).Replace("\\", "\\\\")));
            foreach (Framework.Data.GeocacheType gctype in Core.GeocacheTypes)
            {
                sb.AppendLine(string.Format("var gct{0}Icon = new google.maps.MarkerImage(\"{1}\");", gctype.ID.ToString().Replace("-","_"), Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Map, gctype).Replace("\\","\\\\")));
                sb.AppendLine(string.Format("var gct{0}IconC = new google.maps.MarkerImage(\"{1}\");", gctype.ID.ToString().Replace("-", "_"), Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Map, gctype, true).Replace("\\", "\\\\")));
            }
            foreach (Framework.Data.WaypointType wptype in Core.WaypointTypes)
            {
                sb.AppendLine(string.Format("var wpt{0}Icon = new google.maps.MarkerImage(\"{1}\");", wptype.ID.ToString().Replace("-", "_"), Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Map, wptype).Replace("\\", "\\\\")));
            }
            return template.Replace("//icons", sb.ToString());
        }

        private void addWaypointsToMap(List<Framework.Data.Waypoint> wpList)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
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
                        bln.AppendFormat("{0}<br />", Utils.Conversion.GetCoordinatesPresentation((double)wp.Lat,(double)wp.Lon));
                        bln.AppendFormat("{0}<br />", wp.Code);
                        bln.AppendFormat("{0}<br />", HttpUtility.HtmlEncode(Utils.LanguageSupport.Instance.GetTranslation(wp.WPType.Name)));
                        bln.AppendFormat("{0}<br />", HttpUtility.HtmlEncode(wp.Description)).Replace("\r\n", "<br />").Replace("\n", "");
                        bln.AppendFormat("{0}", HttpUtility.HtmlEncode(wp.Comment).Replace("\r\n", "<br />").Replace("\n", ""));
                        sb.AppendLine(string.Format("{{a: '{0}', b: {1}, c: {2}, d: wpt{3}Icon, e: '{4}'}}", wp.Code, wp.Lat.ToString().Replace(',', '.'), wp.Lon.ToString().Replace(',', '.'), wp.WPType.ID.ToString().Replace("-", "_"), bln.ToString().Replace("'", "")));
                    }
                }
            }
            sb.Append("]");
            executeScript("updateWaypoints", new object[] { sb.ToString() });
        }

        private void markSelectedGeocaches(List<Framework.Data.Geocache> gcList)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
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
                sb.Append(gc.Selected ? "1" : "0");
            }
            sb.Append("]");
            executeScript("setSelectedGeocaches", new object[] { sb.ToString() });
        }

        private void addGeocachesToMap(List<Framework.Data.Geocache> gcList)
        {
            string coordAccuracy = "0.00000";

            StringBuilder sb = new StringBuilder();
            sb.Append("[");
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
                string tt;
                if (Properties.Settings.Default.ShowNameInToolTip && gc.Name!=null)
                {
                    tt = string.Format("{0}, {1}", gc.Code, gc.Name.Replace('"', ' ').Replace('\'', ' ').Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ').Replace('\\', ' '));
                }
                else
                {
                    tt = gc.Code;
                }
                if (gc.Found)
                {
                    if (gc.ContainsCustomLatLon)
                    {
                        sb.AppendLine(string.Format("{{\"a\":\"{0}\",\"b\":{1},\"c\":{2},\"d\":\"foundIcon\"}}", tt, ((double)gc.CustomLat).ToString(coordAccuracy).Replace(',', '.'), ((double)gc.CustomLon).ToString(coordAccuracy).Replace(',', '.')));
                    }
                    else
                    {
                        sb.AppendLine(string.Format("{{\"a\":\"{0}\",\"b\":{1},\"c\":{2},\"d\":\"foundIcon\"}}", tt, gc.Lat.ToString(coordAccuracy).Replace(',', '.'), gc.Lon.ToString(coordAccuracy).Replace(',', '.')));
                    }
                }
                else
                {
                    if (gc.ContainsCustomLatLon)
                    {
                        sb.AppendLine(string.Format("{{\"a\":\"{0}\",\"b\":{1},\"c\":{2},\"d\":\"gct{3}IconC\"}}", tt, ((double)gc.CustomLat).ToString(coordAccuracy).Replace(',', '.'), ((double)gc.CustomLon).ToString(coordAccuracy).Replace(',', '.'), gc.GeocacheType.ID.ToString().Replace("-", "_")));
                    }
                    else
                    {
                        sb.AppendLine(string.Format("{{\"a\":\"{0}\",\"b\":{1},\"c\":{2},\"d\":\"gct{3}Icon\"}}", tt, gc.Lat.ToString(coordAccuracy).Replace(',', '.'), gc.Lon.ToString(coordAccuracy).Replace(',', '.'), gc.GeocacheType.ID.ToString().Replace("-", "_")));
                    }
                }
            }
            sb.Append("]");
            executeScript("updateGeocaches", new object[] { sb.ToString() });
        }

        public void UpdateView(MapUpdateReason reason)
        {
            Core.DebugLog(Framework.Data.DebugLogLevel.Info, OwnerPlugin, null, string.Format("GMap: UpdateView({0})", reason));
            timer1.Enabled = false;
            if (radioButtonActive.Checked && (_activeMapType != MapType.SingleGeocache || reason== MapUpdateReason.MapSettingsChanged))
            {
                _activeMapType = MapType.SingleGeocache;
                DisplayHtml(_defaultSingleGeocachehtml.Replace("<!-- %SingleGeocache.js% -->", setIcons(_defaultSingleGeocachejs)));
                reason = MapUpdateReason.Init;
            }
            else if (radioButtonSelected.Checked && (_activeMapType != MapType.SelectedGeocaches || reason == MapUpdateReason.MapSettingsChanged))
            {
                _activeMapType = MapType.SelectedGeocaches;
                DisplayHtml(_defaultMultipleGeocachehtml.Replace("<!-- %MultipleGeocache.js% -->", setIcons(_defaultMultipleGeocachejs)));
                reason = MapUpdateReason.Init;
            }
            else if (radioButtonAll.Checked && (_activeMapType != MapType.Allgeocaches || reason == MapUpdateReason.MapSettingsChanged))
            {
                _activeMapType = MapType.Allgeocaches;
                DisplayHtml(_defaultMultipleGeocachehtml.Replace("<!-- %MultipleGeocache.js% -->", setIcons(_defaultMultipleGeocachejs)));
                reason = MapUpdateReason.Init;
            }
            if (webBrowser1.ReadyState == WebBrowserReadyState.Complete)
            {
                switch (_activeMapType)
                {
                    case MapType.SingleGeocache:
                        break;
                    case MapType.Allgeocaches:
                        if (reason == MapUpdateReason.Init || reason == MapUpdateReason.DataChanged)
                        {
                            addGeocachesToMap((from Framework.Data.Geocache wp in Core.Geocaches
                                               select wp).OrderBy(x=>x.Code).ToList());
                            if (Properties.Settings.Default.AddSelectedMarkers)
                            {
                                markSelectedGeocaches((from Framework.Data.Geocache wp in Core.Geocaches
                                                       select wp).OrderBy(x => x.Code).ToList());
                            }
                        }
                        else if (reason == MapUpdateReason.SelectedChanged)
                        {
                            if (Properties.Settings.Default.AddSelectedMarkers)
                            {
                                markSelectedGeocaches((from Framework.Data.Geocache wp in Core.Geocaches
                                                       select wp).OrderBy(x => x.Code).ToList());
                            }
                        }
                        timer1.Enabled = true;
                        break;
                    case MapType.SelectedGeocaches:
                        if (reason == MapUpdateReason.Init || reason == MapUpdateReason.DataChanged || reason == MapUpdateReason.SelectedChanged)
                        {
                            addGeocachesToMap(Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches));
                        }
                        timer1.Enabled = true;
                        break;
                }

                //and always do:
                if (Core.ActiveGeocache == null)
                {
                    executeScript("setGeocache", new object[] { "", "", "", Core.CenterLocation.Lat, Core.CenterLocation.Lon, "" });
                    addWaypointsToMap(null);
                }
                else
                {
                    if (Core.ActiveGeocache.ContainsCustomLatLon)
                    {
                        executeScript("setGeocache", new object[] { Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Medium, Core.ActiveGeocache.GeocacheType), Core.ActiveGeocache.Code, HttpUtility.HtmlEncode(Core.ActiveGeocache.Name), Core.ActiveGeocache.CustomLat, Core.ActiveGeocache.CustomLon, string.Format("gct{0}IconC", Core.ActiveGeocache.GeocacheType.ID.ToString().Replace("-", "_")) });
                    }
                    else
                    {
                        executeScript("setGeocache", new object[] { Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Medium, Core.ActiveGeocache.GeocacheType), Core.ActiveGeocache.Code, HttpUtility.HtmlEncode(Core.ActiveGeocache.Name), Core.ActiveGeocache.Lat, Core.ActiveGeocache.Lon, string.Format("gct{0}Icon", Core.ActiveGeocache.GeocacheType.ID.ToString().Replace("-", "_")) });
                    }
                    addWaypointsToMap(Utils.DataAccess.GetWaypointsFromGeocache(Core.Waypoints,Core.ActiveGeocache.Code));
                }
            }
        }

        private void DisplayHtml(string html)
        {
            webBrowser1.Navigate("about:blank");
            if (webBrowser1.Document != null)
            {
                webBrowser1.Document.Write(string.Empty);
            }
            html = html.Replace("SLoadingS", Utils.LanguageSupport.Instance.GetTranslation(STR_SHOWGEOCACHES));
            html = html.Replace("SLocationS", Utils.LanguageSupport.Instance.GetTranslation(STR_LOCATION));
            html = html.Replace("SGoS", Utils.LanguageSupport.Instance.GetTranslation(STR_GO));
            html = html.Replace("//enableClusterMarkerAboveCount", string.Format("enableClusterMarkerAboveCount = {0};",Properties.Settings.Default.ClusterMarkerThreshold));
            html = html.Replace("//clusterOptions.maxZoom", string.Format("clusterOptions.maxZoom = {0};", Properties.Settings.Default.ClusterMarkerMaxZoomLevel));
            html = html.Replace("//clusterOptions.gridSize", string.Format("clusterOptions.gridSize = {0};", Properties.Settings.Default.ClusterMarkerGridSize));

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

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser1_DocumentCompleted), new object[] { sender, e });
                return;
            }
            UpdateView(MapUpdateReason.Init);
            if (webBrowser1.Document != null)
            {
                foreach(HtmlElement el in webBrowser1.Document.GetElementsByTagName("div"))
                {
                    el.AttachEventHandler("onmouseenter", delegate { mousemoveEventHandler(el, EventArgs.Empty); });
                }
            }
        }
        public void mousemoveEventHandler(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.AutoTopPanel && groupBox1.Visible)
            {
                button2_Click(sender, e);
            }
        }

        private void radioButtonActive_CheckedChanged(object sender, EventArgs e)
        {
            UpdateView(MapUpdateReason.Init);
        }

        protected override void CouplingToMainScreenChanged()
        {
            base.CouplingToMainScreenChanged();
            this.BeginInvoke(new EventHandler(this.reloadMap), new object[] { this, EventArgs.Empty });
        }
        private void reloadMap(object sender, EventArgs e)
        {
            UpdateView(MapUpdateReason.MapSettingsChanged);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (webBrowser1.ReadyState == WebBrowserReadyState.Complete)
            {
                object o = executeScript("getSelectedGeocache", null);
                if (o != null)
                {
                    string s = (string)o;
                    if (s.Length > 0)
                    {
                        s = s.Split(new char[] { ',' }, 2)[0];
                        if (Core.ActiveGeocache == null || Core.ActiveGeocache.Code != s)
                        {
                            Core.ActiveGeocache = Utils.DataAccess.GetGeocache(Core.Geocaches, s);
                        }
                    }
                }
            }
        }

        private void GMapForm_Leave(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }

        private void GMapForm_Enter(object sender, EventArgs e)
        {
            comboBoxAreaLevel.Items.Clear();
            comboBoxAreaName.Items.Clear();
            comboBoxAreaLevel.Items.AddRange(Enum.GetNames(typeof(Framework.Data.AreaType)));
            comboBoxAreaLevel.SelectedIndex = 0;

            if (webBrowser1.ReadyState == WebBrowserReadyState.Complete && (_activeMapType == MapType.Allgeocaches || _activeMapType == MapType.SelectedGeocaches))
            {
                timer1.Enabled = true;
            }
        }

        private void comboBoxAreaLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBoxAreaName.Items.Clear();
            comboBoxAreaName.Items.AddRange(Utils.GeometrySupport.Instance.GetAreasByLevel((Framework.Data.AreaType)Enum.Parse(typeof(Framework.Data.AreaType), comboBoxAreaLevel.SelectedItem.ToString())).OrderBy(x => x.Name).ToArray());
        }

        private void comboBoxAreaName_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = (comboBoxAreaName.SelectedIndex >= 0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                Framework.Data.AreaInfo ai = comboBoxAreaName.SelectedItem as Framework.Data.AreaInfo;
                if (ai != null)
                {
                    Utils.GeometrySupport.Instance.GetPolygonOfArea(ai);
                    if (ai.Polygons != null)
                    {
                        int count = 0;
                        while (count < ai.Polygons.Count)
                        {
                            addPolygons(ai, ai.Polygons.Skip(count).Take(20).ToList());
                            count += 100;
                        }
                        executeScript("zoomToBounds", new object[] { ai.MinLat, ai.MinLon, ai.MaxLat, ai.MaxLon });
                    }
                }
            }
            catch
            {
            }
            this.Cursor = Cursors.Default;
        }

        private void addPolygons(Framework.Data.AreaInfo ai, List<Framework.Data.Polygon> polys)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            bool firstPoly = true;
            bool firstPoint;
            foreach (Framework.Data.Polygon ps in polys)
            {
                //[{points:[{lat, lon},{lat, lon}]},{points:[{lat, lon},{lat, lon}]}]
                if (!firstPoly)
                {
                    sb.Append(",");
                }
                else
                {
                    firstPoly = false;
                }
                sb.Append("{points:[");
                firstPoint = true;
                //List < Framework.Data.Location> reduced = Utils.DouglasPeucker.DouglasPeuckerReduction(ps, 0.00001);
                //foreach (Framework.Data.Location l in reduced)
                foreach (Framework.Data.Location l in ps)
                {
                    if (!firstPoint)
                    {
                        sb.Append(",");
                    }
                    else
                    {
                        firstPoint = false;
                    }
                    sb.AppendFormat("{{lat: {0}, lon: {1}}}", l.Lat.ToString().Replace(',', '.'), l.Lon.ToString().Replace(',', '.'));
                }
                sb.Append("]}");
            }
            sb.Append("]");
            executeScript("addPolygons", new object[] { sb.ToString() });
        }

        private void GMapForm_Shown(object sender, EventArgs e)
        {
        }

        private void GMapForm_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void GMapForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (groupBox1.Visible)
            {
                groupBox1.Visible = false;
                groupBox2.Visible = false;
                panel1.Height = 30;
                button2.Text = "v";
            }
            else
            {
                groupBox1.Visible = true;
                groupBox2.Visible = true;
                panel1.Height = 132;
                button2.Text = "^";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Core.GPSLocation.Valid)
            {
                executeScript("setMapCenter", new object[] { Core.GPSLocation.Position.Lat, Core.GPSLocation.Position.Lon });
            }
        }

        private void panel1_MouseEnter(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.AutoTopPanel && !groupBox1.Visible)
            {
                button2_Click(sender, e);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AddSelectedMarkers = checkBox1.Checked;
            Properties.Settings.Default.Save();
            if (_activeMapType== MapType.Allgeocaches)
            {
                if (Properties.Settings.Default.AddSelectedMarkers)
                {
                    markSelectedGeocaches((from Framework.Data.Geocache wp in Core.Geocaches
                                           select wp).OrderBy(x => x.Code).ToList());
                }
                else
                {
                    executeScript("setSelectedGeocaches", new object[] { "[]" });
                }
            }
        }
    }

    public class GMap : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Google Map";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(GMapForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GMapForm.STR_SELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GMapForm.STR_SELECTEDMARKERS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GMapForm.STR_ALL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GMapForm.STR_ACTIVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GMapForm.STR_AREAS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GMapForm.STR_SHOWGEOCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GMapForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GMapForm.STR_LEVEL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GMapForm.STR_ADD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GMapForm.STR_LOADING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GMapForm.STR_LOCATION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GMapForm.STR_GO));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_CLUSTERMARKER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_ENABLEABOVETHRESHOLD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_GRIDSIZE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_MAXZOOMLEVEL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_SHOWNAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_AUTOTOPPANEL));

            return base.Initialize(core);
        }

        public override string FriendlyName
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new GMapForm(this, core));
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Map;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            foreach (System.Windows.Forms.UserControl uc in configPanels)
            {
                if (uc is SettingsPanel)
                {
                    (uc as SettingsPanel).Apply();
                    if ((uc as SettingsPanel).SettingsChanged)
                    {
                        if (UIChildWindowForm != null)
                        {
                            if (UIChildWindowForm.Visible)
                            {
                                UIChildWindowForm.Show();
                                (UIChildWindowForm as GMapForm).UpdateView(GMapForm.MapUpdateReason.MapSettingsChanged);
                            }
                        }
                    }
                    break;
                }
            }
            return true;
        }

        public override List<System.Windows.Forms.UserControl> CreateConfigurationPanels()
        {
            List<System.Windows.Forms.UserControl> pnls = base.CreateConfigurationPanels();
            if (pnls == null) pnls = new List<System.Windows.Forms.UserControl>();
            pnls.Add(new SettingsPanel());
            return pnls;
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (UIChildWindowForm != null)
                {
                    if (action == ACTION_SHOW)
                    {
                        if (!UIChildWindowForm.Visible)
                        {
                            UIChildWindowForm.Show();
                            (UIChildWindowForm as GMapForm).UpdateView(GMapForm.MapUpdateReason.Init);
                        }
                        if (UIChildWindowForm.WindowState == FormWindowState.Minimized)
                        {
                            UIChildWindowForm.WindowState = FormWindowState.Normal;
                        }
                        UIChildWindowForm.BringToFront();
                    }
                }
                result = true;
            }
            return result;
        }
    }

}
