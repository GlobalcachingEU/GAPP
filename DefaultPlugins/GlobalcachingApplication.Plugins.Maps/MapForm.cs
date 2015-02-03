using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Maps
{
    public partial class MapForm : Form
    {
        private string STR_TITLE = "Open Street Map";
        public const string STR_SEARCH = "Search";
        public const string STR_ACTIVE = "Active";
        public const string STR_SELECTED = "Selected";
        public const string STR_ALL = "All";
        public const string STR_SHOWGEOCACHES = "Show geocaches";
        public const string STR_DECOUPLE_WINDOW = "Decouple from main screen";
        public const string STR_DOCK_WINDOW = "Couple to main screen";
        public const string STR_TOPMOST_WINDOW = "Keep window in front";
        public const string STR_NOTTOPMOST_WINDOW = "Do not keep window in front";

        private const int DECOUPLE_WINDOW_ID = 0x100;
        private const int DOCK_WINDOW_ID = 0x101;
        private const int TOPMOST_WINDOW_ID = 0x102;
        private const int NOTTOPMOST_WINDOW_ID = 0x103;

        private Framework.Interfaces.ICore _core = null;
        private MapControl.MarkerClusterer _clusterGeocaches = null;
        private Framework.Data.Geocache _activeGeocache = null;
        private bool _firstShow = true;
        private bool _updateSystemMenu = false;
        private MapControl.MapControlFactory _mapControlFactory = null;

        public MapForm()
        {
            InitializeComponent();
        }

        public MapForm(Framework.Interfaces.ICore core, string title)
        {
            InitializeComponent();

            STR_TITLE = title;
            _core = core;
            _core.ActiveGeocacheChanged += new Framework.EventArguments.GeocacheEventHandler(_core_ActiveGeocacheChanged);
            _core.SelectedLanguageChanged += new EventHandler(_core_SelectedLanguageChanged);
            _core.Geocaches.SelectedChanged += new Framework.EventArguments.GeocacheEventHandler(Geocaches_SelectedChanged);
            _core.Geocaches.ListSelectionChanged += new EventHandler(Geocaches_ListSelectionChanged);
            _core.Geocaches.DataChanged += new Framework.EventArguments.GeocacheEventHandler(Geocaches_DataChanged);
            _core.Geocaches.GeocacheAdded += new Framework.EventArguments.GeocacheEventHandler(Geocaches_GeocacheAdded);
            _core.Geocaches.GeocacheRemoved += new Framework.EventArguments.GeocacheEventHandler(Geocaches_GeocacheRemoved);
            _core.Geocaches.ListDataChanged += new EventHandler(Geocaches_ListDataChanged);
            _core.GPSLocation.Updated += new Framework.EventArguments.GPSLocationEventHandler(GPSLocation_Updated);
            _core.Waypoints.DataChanged += new Framework.EventArguments.WaypointEventHandler(Waypoints_DataChanged);
            _core.Waypoints.ListDataChanged += new EventHandler(Waypoints_ListDataChanged);

            _core_SelectedLanguageChanged(this, EventArgs.Empty);
            this.mapContainerControl1.tileCanvas.ZoomChanged += new EventHandler<EventArgs>(tileCanvas_ZoomChanged);
            this.mapContainerControl1.GeocacheClick += new Framework.EventArguments.GeocacheEventHandler(mapContainerControl1_GeocacheClick);

            /*
            if (PluginSettings.Instance.WindowPos != null && !PluginSettings.Instance.WindowPos.IsEmpty)
            {
                this.Bounds = PluginSettings.Instance.WindowPos;
                this.StartPosition = FormStartPosition.Manual;
            }
            */
            _mapControlFactory = MapControl.MapCanvas.MapControlFactoryToUse;
            if (_mapControlFactory != null)
            {
                if (PluginSettings.Instance.SpecifiedWindowPos != null)
                {
                    for (int i = 0; i < PluginSettings.Instance.SpecifiedWindowPos.Count; i++)
                    {
                        if (PluginSettings.Instance.SpecifiedWindowPos[i].StartsWith(string.Concat(_mapControlFactory.ID, "|")))
                        {
                            try
                            {
                                string[] parts = PluginSettings.Instance.SpecifiedWindowPos[i].Split(new char[] { '|' });
                                this.Bounds = new Rectangle(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]));
                                this.StartPosition = FormStartPosition.Manual;
                                break;
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                else
                {
                    PluginSettings.Instance.SpecifiedWindowPos = new System.Collections.Specialized.StringCollection();
                }
            }
        }

        void Waypoints_ListDataChanged(object sender, EventArgs e)
        {
            if (this.Visible && _core.ActiveGeocache!=null)
            {
                List<Framework.Data.Waypoint> wps = Utils.DataAccess.GetWaypointsFromGeocache(_core.Waypoints, _core.ActiveGeocache.Code);
                List<MapControl.Marker> wpmarkers = new List<MapControl.Marker>();
                foreach (Framework.Data.Waypoint wp in wps)
                {
                    if (wp.Lat != null && wp.Lon != null)
                    {
                        MapControl.Marker wpm = new MapControl.Marker();
                        wpm.Latitude = (double)wp.Lat;
                        wpm.Longitude = (double)wp.Lon;
                        wpm.ImagePath = Utils.ImageSupport.Instance.GetImagePath(_core, Framework.Data.ImageSize.Map, wp.WPType);
                        wpmarkers.Add(wpm);
                    }
                }
                this.mapContainerControl1.SetWaypointMarkers(wpmarkers);
                this.mapContainerControl1.MapCanvas.RepositionChildren();
            }            
        }

        void Waypoints_DataChanged(object sender, Framework.EventArguments.WaypointEventArgs e)
        {
            Waypoints_ListDataChanged(this, EventArgs.Empty);           
        }

        void GPSLocation_Updated(object sender, Framework.EventArguments.GPSLocationEventArgs e)
        {
            if (this.Visible)
            {
                if (e.Location.Valid)
                {
                    MapControl.Marker wpm = new MapControl.Marker();
                    wpm.Latitude = e.Location.Position.Lat;
                    wpm.Longitude = e.Location.Position.Lon;
                    wpm.ImagePath = System.IO.Path.Combine(new string[] { System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Images", "Map", "curpos.png" });
                    this.mapContainerControl1.SetCurposMarker(wpm);
                }
                else
                {
                    this.mapContainerControl1.SetCurposMarker(null);
                }
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            //not critical
            try
            {
                Utils.SystemMenu sm = Utils.SystemMenu.FromForm(this);
                if (sm != null)
                {
                    if (sm.AppendSeparator())
                    {
                        sm.Append(DECOUPLE_WINDOW_ID, Utils.LanguageSupport.Instance.GetTranslation(STR_DECOUPLE_WINDOW));
                        sm.Append(DOCK_WINDOW_ID, Utils.LanguageSupport.Instance.GetTranslation(STR_DOCK_WINDOW));
                        sm.Append(TOPMOST_WINDOW_ID, Utils.LanguageSupport.Instance.GetTranslation(STR_TOPMOST_WINDOW));
                        sm.Append(NOTTOPMOST_WINDOW_ID, Utils.LanguageSupport.Instance.GetTranslation(STR_NOTTOPMOST_WINDOW));
                    }
                }
            }
            catch
            {
            }
        }

        private void Decouple()
        {
            this.MdiParent = null;
            if (PluginSettings.Instance.DecoupledChildWindows == null)
            {
                PluginSettings.Instance.DecoupledChildWindows = new System.Collections.Specialized.StringCollection();
            }
            if (!PluginSettings.Instance.DecoupledChildWindows.Contains(STR_TITLE))
            {
                PluginSettings.Instance.DecoupledChildWindows.Add(STR_TITLE);
            }
        }

        protected override void WndProc(ref Message msg)
        {
            if (msg.Msg == (int)Utils.SystemMenu.WindowMessages.wmSysCommand)
            {
                switch (msg.WParam.ToInt32())
                {
                    case DECOUPLE_WINDOW_ID:
                        Decouple();
                        break;
                    case DOCK_WINDOW_ID:
                        Framework.Interfaces.IPluginUIMainWindow mainPlugin = (from Framework.Interfaces.IPlugin a in _core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault() as Framework.Interfaces.IPluginUIMainWindow;
                        if (mainPlugin != null)
                        {
                            this.MdiParent = mainPlugin.MainForm;
                        }
                        if (PluginSettings.Instance.DecoupledChildWindows == null)
                        {
                            PluginSettings.Instance.DecoupledChildWindows = new System.Collections.Specialized.StringCollection();
                        }
                        if (PluginSettings.Instance.DecoupledChildWindows.Contains(STR_TITLE))
                        {
                            PluginSettings.Instance.DecoupledChildWindows.Remove(STR_TITLE);
                        }
                        break;
                    case TOPMOST_WINDOW_ID:
                        //can only be topmost if decoupled
                        Decouple();
                        this.TopMost = true;
                        if (PluginSettings.Instance.TopMostWindows == null)
                        {
                            PluginSettings.Instance.TopMostWindows = new System.Collections.Specialized.StringCollection();
                        }
                        if (!PluginSettings.Instance.TopMostWindows.Contains(STR_TITLE))
                        {
                            PluginSettings.Instance.TopMostWindows.Add(STR_TITLE);
                        }
                        break;
                    case NOTTOPMOST_WINDOW_ID:
                        this.TopMost = false;
                        if (PluginSettings.Instance.TopMostWindows == null)
                        {
                            PluginSettings.Instance.TopMostWindows = new System.Collections.Specialized.StringCollection();
                        }
                        if (PluginSettings.Instance.TopMostWindows.Contains(STR_TITLE))
                        {
                            PluginSettings.Instance.TopMostWindows.Remove(STR_TITLE);
                        }
                        break;
                }
            }
            base.WndProc(ref msg);
        }

        public void SettingsChanged()
        {
            this.mapContainerControl1.MapCanvas.MapControlFactory.SettingsChanged();
        }

        void Geocaches_ListDataChanged(object sender, EventArgs e)
        {
            if (this.Visible && !radioButtonActive.Checked)
            {
                clusterGeocaches(true);
                this.mapContainerControl1.MapCanvas.CheckMassMarkers();
                this.mapContainerControl1.MapCanvas.RepositionChildren();
            }
        }

        void Geocaches_GeocacheRemoved(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (this.Visible && !radioButtonActive.Checked)
            {
                clusterGeocaches(true);
                this.mapContainerControl1.MapCanvas.CheckMassMarkers();
                this.mapContainerControl1.MapCanvas.RepositionChildren();
            }
        }

        void Geocaches_GeocacheAdded(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (this.Visible && !radioButtonActive.Checked)
            {
                clusterGeocaches(true);
                this.mapContainerControl1.MapCanvas.CheckMassMarkers();
                this.mapContainerControl1.MapCanvas.RepositionChildren();
            }
        }

        void Geocaches_DataChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (this.Visible)
            {
                _activeGeocache = null;
                clusterGeocaches(true);
                UpdateView(true);
            }
        }

        void Geocaches_ListSelectionChanged(object sender, EventArgs e)
        {
            if (this.Visible && radioButtonSelected.Checked)
            {
                clusterGeocaches(true);
                this.mapContainerControl1.MapCanvas.CheckMassMarkers();
                this.mapContainerControl1.MapCanvas.RepositionChildren();
            }
        }

        private void reloadSystemMenu()
        {
            //not critical
            try
            {
                Utils.SystemMenu.ResetSystemMenu(this);
                Utils.SystemMenu sm = Utils.SystemMenu.FromForm(this);
                if (sm != null)
                {
                    if (sm.AppendSeparator())
                    {
                        sm.Append(DECOUPLE_WINDOW_ID, Utils.LanguageSupport.Instance.GetTranslation(STR_DECOUPLE_WINDOW));
                        sm.Append(DOCK_WINDOW_ID, Utils.LanguageSupport.Instance.GetTranslation(STR_DOCK_WINDOW));
                        sm.Append(TOPMOST_WINDOW_ID, Utils.LanguageSupport.Instance.GetTranslation(STR_TOPMOST_WINDOW));
                        sm.Append(NOTTOPMOST_WINDOW_ID, Utils.LanguageSupport.Instance.GetTranslation(STR_NOTTOPMOST_WINDOW));
                    }
                }
            }
            catch
            {
            }
        }

        void Geocaches_SelectedChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (this.Visible && radioButtonSelected.Checked)
            {
                clusterGeocaches(true);
                this.mapContainerControl1.MapCanvas.CheckMassMarkers();
                this.mapContainerControl1.MapCanvas.RepositionChildren();
            }
        }

        void mapContainerControl1_GeocacheClick(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            _activeGeocache = e.Geocache;
            _core.ActiveGeocache = e.Geocache;
        }

        void tileCanvas_ZoomChanged(object sender, EventArgs e)
        {
            if (!radioButtonActive.Checked)
            {
                clusterGeocaches(true);
            }
        }

        private void clusterGeocaches(bool recluster)
        {
            List<MapControl.Marker> mlist = new List<MapControl.Marker>();
            if (!radioButtonActive.Checked)
            {
                double lat1 = this.mapContainerControl1.tileCanvas.MapControlFactory.TileGenerator.GetLatitude(0, this.mapContainerControl1.MapCanvas.Zoom);
                double lat2 = this.mapContainerControl1.tileCanvas.MapControlFactory.TileGenerator.GetLatitude(1, this.mapContainerControl1.MapCanvas.Zoom);
                double deltaLat = 200 * (lat1 - lat2) / 256.0; //200px
                double lon1 = this.mapContainerControl1.tileCanvas.MapControlFactory.TileGenerator.GetLongitude(0, this.mapContainerControl1.MapCanvas.Zoom);
                double lon2 = this.mapContainerControl1.tileCanvas.MapControlFactory.TileGenerator.GetLongitude(1, this.mapContainerControl1.MapCanvas.Zoom);
                double deltaLon = 200 * (lon2 - lon1) / 256.0; //200px
                if (_clusterGeocaches == null || recluster)
                {
                    _clusterGeocaches = new MapControl.MarkerClusterer(deltaLat, deltaLon);

                    bool addAll = radioButtonAll.Checked;
                    bool doCluster = this.mapContainerControl1.MapCanvas.Zoom<12;
                    foreach (Framework.Data.Geocache gc in _core.Geocaches)
                    {
                        if (gc != _core.ActiveGeocache && (addAll || gc.Selected))
                        {
                            _clusterGeocaches.AddMarker(gc, doCluster);
                        }
                    }
                }
                //add markers
                foreach (var b in _clusterGeocaches.Buckets)
                {
                    MapControl.Marker m = new MapControl.Marker();
                    m.Latitude = b.Latitude;
                    m.Longitude = b.Longitude;
                    if (b.Count == 1)
                    {
                        m.ImagePath = Utils.ImageSupport.Instance.GetImagePath(_core, Framework.Data.ImageSize.Map, b.Geocache.GeocacheType, b.Geocache.ContainsCustomLatLon);
                        m.Tag = b.Geocache;
                    }
                    else
                    {
                        m.ImagePath = "";
                        m.Tag = b.Count;
                    }
                    mlist.Add(m);
                }
            }
            this.mapContainerControl1.SetGeocacheMarkers(mlist);
        }

        void _core_SelectedLanguageChanged(object sender, EventArgs e)
        {
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.mapContainerControl1.searchBar.searchBtn.Content = Utils.LanguageSupport.Instance.GetTranslation(STR_SEARCH);
            this.radioButtonActive.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ACTIVE);
            this.radioButtonAll.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ALL);
            this.radioButtonSelected.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTED);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHOWGEOCACHES);
            if (this.Visible)
            {
                reloadSystemMenu();
            }
            else
            {
                _updateSystemMenu = true;
            }
        }

        void _core_ActiveGeocacheChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (this.Visible)
            {
                if (_activeGeocache != _core.ActiveGeocache)
                {
                    if (_core.ActiveGeocache != null)
                    {
                        MapControl.Marker m = new MapControl.Marker();
                        if (_core.ActiveGeocache.ContainsCustomLatLon)
                        {
                            this.mapContainerControl1.MapCanvas.Center((double)_core.ActiveGeocache.CustomLat, (double)_core.ActiveGeocache.CustomLon, this.mapContainerControl1.MapCanvas.Zoom);
                            m.Latitude = (double)_core.ActiveGeocache.CustomLat;
                            m.Longitude = (double)_core.ActiveGeocache.CustomLon;
                        }
                        else
                        {
                            this.mapContainerControl1.MapCanvas.Center(_core.ActiveGeocache.Lat, _core.ActiveGeocache.Lon, this.mapContainerControl1.MapCanvas.Zoom);
                            m.Latitude = _core.ActiveGeocache.Lat;
                            m.Longitude = _core.ActiveGeocache.Lon;
                        }
                        m.ImagePath = Utils.ImageSupport.Instance.GetImagePath(_core, Framework.Data.ImageSize.Map, _core.ActiveGeocache.GeocacheType, _core.ActiveGeocache.ContainsCustomLatLon);

                        List<Framework.Data.Waypoint> wps = Utils.DataAccess.GetWaypointsFromGeocache(_core.Waypoints, _core.ActiveGeocache.Code);
                        List<MapControl.Marker> wpmarkers = new List<MapControl.Marker>();
                        foreach (Framework.Data.Waypoint wp in wps)
                        {
                            if (wp.Lat != null && wp.Lon != null)
                            {
                                MapControl.Marker wpm = new MapControl.Marker();
                                wpm.Latitude = (double)wp.Lat;
                                wpm.Longitude = (double)wp.Lon;
                                wpm.ImagePath = Utils.ImageSupport.Instance.GetImagePath(_core, Framework.Data.ImageSize.Map, wp.WPType);
                                wpmarkers.Add(wpm);
                            }
                        }
                        this.mapContainerControl1.SetWaypointMarkers(wpmarkers);
                        this.mapContainerControl1.SetCacheMarker(m);
                    }
                    else
                    {
                        this.mapContainerControl1.SetCacheMarker(null);
                    }
                }
                UpdateView(false);
            }
        }

        public void UpdateView()
        {
            UpdateView(true);
        }

        public void UpdateView(bool init)
        {
            if (this.Visible)
            {
                if (init)
                {
                    _core_ActiveGeocacheChanged(this, null);
                }
                linkLabelGC.Links.Clear();
                if (_core.ActiveGeocache != null)
                {
                    pictureBoxGC.ImageLocation = Utils.ImageSupport.Instance.GetImagePath(_core, Framework.Data.ImageSize.Default, _core.ActiveGeocache.GeocacheType);
                    linkLabelGC.Text = string.Format("{0}, {1}", _core.ActiveGeocache.Code, _core.ActiveGeocache.Name);
                    linkLabelGC.Links.Add(0, _core.ActiveGeocache.Code.Length, _core.ActiveGeocache.Url);
                    if (_firstShow)
                    {
                        this.mapContainerControl1.MapCanvas.Center(_core.ActiveGeocache.Lat, _core.ActiveGeocache.Lon, this.mapContainerControl1.MapCanvas.MapControlFactory.TileGenerator.MaxZoom - 3);
                    }
                }
                else
                {
                    pictureBoxGC.Image = null;
                    linkLabelGC.Text = "-";

                    this.mapContainerControl1.MapCanvas.Center(_core.CenterLocation.Lat, _core.CenterLocation.Lon, this.mapContainerControl1.MapCanvas.MapControlFactory.TileGenerator.MaxZoom-3);
                }
                _firstShow = false;
            }
        }

        private void MapForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void linkLabelGC_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }

        private void radioButtonSelected_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton cb = sender as RadioButton;
            if (cb != null)
            {
                if (cb.Checked)
                {
                    clusterGeocaches(true);
                    this.mapContainerControl1.MapCanvas.CheckMassMarkers();
                    this.mapContainerControl1.MapCanvas.RepositionChildren();
                }
            }
        }

        private void MapForm_LocationChanged(object sender, EventArgs e)
        {
            if (PluginSettings.Instance.SpecifiedWindowPos == null)
            {
                PluginSettings.Instance.SpecifiedWindowPos = new System.Collections.Specialized.StringCollection();
            }
            if (WindowState == FormWindowState.Normal && _mapControlFactory != null && this.Visible)
            {
                bool done = false;
                string s = string.Format("{0}|{1}|{2}|{3}|{4}", _mapControlFactory.ID, this.Bounds.X, this.Bounds.Y, this.Bounds.Width, this.Bounds.Height);
                for (int i = 0; i < PluginSettings.Instance.SpecifiedWindowPos.Count; i++)
                {
                    if (PluginSettings.Instance.SpecifiedWindowPos[i].StartsWith(string.Concat(_mapControlFactory.ID, "|")))
                    {
                        PluginSettings.Instance.SpecifiedWindowPos[i] = s;
                        done = true;
                        break;
                    }
                }
                if (!done)
                {
                    PluginSettings.Instance.SpecifiedWindowPos.Add(s);
                }

                this.Refresh();
            }
        }

        private void MapForm_SizeChanged(object sender, EventArgs e)
        {
            if (PluginSettings.Instance.SpecifiedWindowPos == null)
            {
                PluginSettings.Instance.SpecifiedWindowPos = new System.Collections.Specialized.StringCollection();
            }
            if (WindowState == FormWindowState.Normal && _mapControlFactory != null && this.Visible)
            {
                bool done = false;
                string s = string.Format("{0}|{1}|{2}|{3}|{4}", _mapControlFactory.ID, this.Bounds.X, this.Bounds.Y, this.Bounds.Width, this.Bounds.Height);
                for (int i = 0; i < PluginSettings.Instance.SpecifiedWindowPos.Count; i++)
                {
                    if (PluginSettings.Instance.SpecifiedWindowPos[i].StartsWith(string.Concat(_mapControlFactory.ID, "|")))
                    {
                        PluginSettings.Instance.SpecifiedWindowPos[i] = s;
                        done = true;
                        break;
                    }
                }
                if (!done)
                {
                    PluginSettings.Instance.SpecifiedWindowPos.Add(s);
                }
            }
        }

        private void MapForm_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible && _updateSystemMenu)
            {
                reloadSystemMenu();
            }
        }

        private void MapForm_Activated(object sender, EventArgs e)
        {
            this.Opacity = 1.0;
        }

        private void MapForm_Deactivate(object sender, EventArgs e)
        {
            if (PluginSettings.Instance.DecoupledChildWindows != null && PluginSettings.Instance.DecoupledChildWindows.Contains(STR_TITLE) &&
                PluginSettings.Instance.TopMostWindows != null && PluginSettings.Instance.TopMostWindows.Contains(STR_TITLE))
            {
                this.Opacity = (double)Utils.BasePlugin.BaseUIChildWindowForm.TopMostOpaque / 100.0;
            }
        }
    }
}
