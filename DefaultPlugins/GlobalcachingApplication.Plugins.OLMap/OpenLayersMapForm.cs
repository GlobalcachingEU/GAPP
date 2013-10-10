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

namespace GlobalcachingApplication.Plugins.OLMap
{
    public partial class OpenLayersMapForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "OpenLayers map";
        public const string STR_ACTIVE = "Active";
        public const string STR_SELECTED = "Selected";
        public const string STR_ALL = "All";
        public const string STR_SHOWGEOCACHES = "Load geocaches";

        private bool _mapUpdated = false;

        public OpenLayersMapForm()
        {
            InitializeComponent();
        }

        public OpenLayersMapForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
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

            core.ActiveGeocacheChanged += new Framework.EventArguments.GeocacheEventHandler(core_ActiveGeocacheChanged);
            core.Geocaches.SelectedChanged += new Framework.EventArguments.GeocacheEventHandler(Geocaches_SelectedChanged);
            core.Geocaches.ListSelectionChanged += new EventHandler(Geocaches_ListSelectionChanged);
            core.Geocaches.DataChanged += new Framework.EventArguments.GeocacheEventHandler(Geocaches_DataChanged);
            core.Geocaches.GeocacheAdded += new Framework.EventArguments.GeocacheEventHandler(Geocaches_GeocacheAdded);
            core.Geocaches.GeocacheRemoved += new Framework.EventArguments.GeocacheEventHandler(Geocaches_GeocacheRemoved);
            core.Geocaches.ListDataChanged += new EventHandler(Geocaches_ListDataChanged);

            SelectedLanguageChanged(this, EventArgs.Empty);
        }

        void Geocaches_ListDataChanged(object sender, EventArgs e)
        {
            if (this.Visible && _mapUpdated)
            {
                UpdateMap();
            }
        }

        void Geocaches_GeocacheRemoved(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (this.Visible && _mapUpdated)
            {
                UpdateMap();
            }
        }

        void Geocaches_GeocacheAdded(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (this.Visible && _mapUpdated)
            {
                UpdateMap();
            }
        }

        void Geocaches_DataChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (this.Visible && _mapUpdated)
            {
                UpdateMap();
            }
        }

        void Geocaches_ListSelectionChanged(object sender, EventArgs e)
        {
            if (this.Visible && _mapUpdated)
            {
                UpdateSelectedMarkers();
            }
        }

        void Geocaches_SelectedChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (this.Visible && _mapUpdated)
            {
                UpdateSelectedMarkers();
            }
        }

        void core_ActiveGeocacheChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (this.Visible && _mapUpdated)
            {
                setActiveGeocache(true);
            }
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            base.SelectedLanguageChanged(sender, e);
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.radioButtonActive.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ACTIVE);
            this.radioButtonAll.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ALL);
            this.radioButtonSelected.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTED);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHOWGEOCACHES);
        }

        private void OpenLayersMapForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private string setIcons(string template)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("var iconSize = new OpenLayers.Size(27, 34);"));
            sb.AppendLine(string.Format("var iconOffset = new OpenLayers.Pixel(-(iconSize.w / 2), -iconSize.h);"));
            sb.AppendLine(string.Format("var foundIcon = new OpenLayers.Icon('{0}',iconSize,iconOffset);", System.IO.Path.Combine(new string[] { System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Images", "Map", "cachetypes", "gevonden.png" }).Replace("\\", "\\\\")));
            foreach (Framework.Data.GeocacheType gctype in Core.GeocacheTypes)
            {
                sb.AppendLine(string.Format("var gct{0}Icon = new OpenLayers.Icon('{1}',iconSize,iconOffset);", gctype.ID.ToString().Replace("-", "_"), Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Map, gctype).Replace("\\", "\\\\")));
                sb.AppendLine(string.Format("var gct{0}IconC = new OpenLayers.Icon('{1}',iconSize,iconOffset);", gctype.ID.ToString().Replace("-", "_"), Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Map, gctype, true).Replace("\\", "\\\\")));
            }
            foreach (Framework.Data.WaypointType wptype in Core.WaypointTypes)
            {
                sb.AppendLine(string.Format("var wpt{0}Icon = new OpenLayers.Icon('{1}',iconSize,iconOffset);", wptype.ID.ToString().Replace("-", "_"), Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Map, wptype).Replace("\\", "\\\\")));
            }
            return template.Replace("//icons", sb.ToString());
        }

        public void UpdateView()
        {
            using (StreamReader textStreamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("GlobalcachingApplication.Plugins.OLMap.template.html")))
            {
                DisplayHtml(setIcons(textStreamReader.ReadToEnd()));
            }
            _mapUpdated = false;
        }

        private void setActiveGeocache(bool panTo)
        {
            if (Core.ActiveGeocache != null)
            {
                executeScript("setActiveMarker", new object[] { Core.ActiveGeocache.Lat, Core.ActiveGeocache.Lon, string.Format("gct{0}Icon", Core.ActiveGeocache.GeocacheType.ID.ToString().Replace("-", "_")), panTo });
            }
        }

        private void UpdateMap()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                _mapUpdated = true;
                setActiveGeocache(false);
                StringBuilder sb = new StringBuilder();
                sb.Append("[");
                if (!radioButtonActive.Checked)
                {
                    bool first = true;
                    string icon;
                    bool selectedOnly = radioButtonSelected.Checked;
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        if (gc.Selected || !selectedOnly)
                        {
                            if (!first)
                            {
                                sb.Append(",");
                            }
                            else
                            {
                                first = false;
                            }
                            if (gc.Found)
                            {
                                icon = "foundIcon";
                            }
                            else if (gc.CustomCoords)
                            {
                                icon = string.Format("gct{0}IconC", gc.GeocacheType.ID);
                            }
                            else
                            {
                                icon = string.Format("gct{0}Icon", gc.GeocacheType.ID);
                            }
                            sb.AppendFormat("{{c:'{0}',a:{1},b:{2},i:{4},g:{3}}}", gc.Code, gc.Lat.ToString().Replace(',', '.'), gc.Lon.ToString().Replace(',', '.'), gc.GeocacheType.ID, icon);
                        }
                    }
                }
                sb.Append("]");
                executeScript("setAllMarkers", new object[] { sb.ToString() });
                UpdateSelectedMarkers();
            }
            catch
            {
            }
            this.Cursor = Cursors.Default;
        }

        private void UpdateSelectedMarkers()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            if (!radioButtonActive.Checked)
            {
                bool first = true;
                string icon;
                foreach (Framework.Data.Geocache gc in Core.Geocaches)
                {
                    if (gc.Selected)
                    {
                        if (!first)
                        {
                            sb.Append(",");
                        }
                        else
                        {
                            first = false;
                        }
                        if (gc.Found)
                        {
                            icon = "foundIcon";
                        }
                        else if (gc.CustomCoords)
                        {
                            icon = string.Format("gct{0}IconC", gc.GeocacheType.ID);
                        }
                        else
                        {
                            icon = string.Format("gct{0}Icon", gc.GeocacheType.ID);
                        }
                        sb.AppendFormat("{{c:'{0}',a:{1},b:{2},i:{4},g:{3}}}", gc.Code, gc.Lat.ToString().Replace(',', '.'), gc.Lon.ToString().Replace(',', '.'), gc.GeocacheType.ID, icon);
                    }
                }
            }
            sb.Append("]");
            executeScript("setSelectedMarkers", new object[] { sb.ToString() });
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

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser1_DocumentCompleted), new object[] { sender, e });
                return;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (webBrowser1.ReadyState == WebBrowserReadyState.Complete && webBrowser1.Url != null)
            {
                if (!_mapUpdated)
                {
                    if ((bool)executeScript("isReady", null))
                    {
                        UpdateMap();
                    }
                }
                else
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
        }

        private void OpenLayersMapForm_Enter(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void OpenLayersMapForm_Leave(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }

        private void OpenLayersMapForm_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void OpenLayersMapForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void radioButtonActive_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null && rb.Checked)
            {
                if (_mapUpdated)
                {
                    UpdateMap();
                }
            }
        }

    }
}
