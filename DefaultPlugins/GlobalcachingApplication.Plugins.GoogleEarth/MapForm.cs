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
using GlobalcachingApplication.Utils.Controls;
using System.Web.Script.Serialization;

namespace GlobalcachingApplication.Plugins.GoogleEarth
{
    public partial class MapForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Google Earth";

        private GAPPWebBrowser _webBrowser = null;
        private bool _webpageLoaded = false;

        public class JSCallBack
        {
            private MapForm _parent;

            public JSCallBack(MapForm parent)
            {
                _parent = parent;
            }

            public void PageReady()
            {
                _parent._webpageLoaded = true;
                _parent.BeginInvoke((Action)(() =>
                {
                    _parent.setActiveGeocacheInMap();
                }));
            }

        }


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

            SelectedLanguageChanged(this, EventArgs.Empty);

            core.ActiveGeocacheChanged += new Framework.EventArguments.GeocacheEventHandler(core_ActiveGeocacheChanged);
            this.VisibleChanged += MapForm_VisibleChanged;
        }

        void MapForm_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                AddWebBrowser();
            }
            else
            {
                RemoveWebBrowser();
            }            
        }

        public void AddWebBrowser()
        {
            if (_webBrowser != null)
            {
                RemoveWebBrowser();
            }
            _webpageLoaded = false;
            _webBrowser = new GAPPWebBrowser("");
            this.Controls.Add(_webBrowser);
            _webBrowser.Browser.RegisterJsObject("bound", new JSCallBack(this));
        }
        public void RemoveWebBrowser()
        {
            if (_webBrowser != null)
            {
                _webpageLoaded = false;
                this.Controls.Remove(_webBrowser);
                _webBrowser.Dispose();
                _webBrowser = null;
            }
        }

        void core_ActiveGeocacheChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (this.Visible && _webpageLoaded)
            {
                setActiveGeocacheInMap();
            }
        }

        private class SetGeocacheInfo
        {
            public string title {get; set; }
            public double lat {get; set; }
            public double lon {get; set; }
            public string icon {get; set; }
            public double speed {get; set; }
            public bool applyV {get; set; }
            public double tilt {get; set; }
            public double altitude {get; set; }
        }
        private void setActiveGeocacheInMap()
        {
            if (this.Visible)
            {
                if (Core.ActiveGeocache != null)
                {
                    SetGeocacheInfo sgi = new SetGeocacheInfo();
                    sgi.title = string.Format("{0}, {1}", Core.ActiveGeocache.Code, Core.ActiveGeocache.Name ?? "");
                    sgi.icon = string.Format("file//", Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Map, Core.ActiveGeocache.GeocacheType));
                    sgi.speed = Properties.Settings.Default.FlyToSpeed;
                    sgi.applyV = Properties.Settings.Default.FixedView;
                    sgi.altitude = Properties.Settings.Default.AltitudeView;
                    sgi.tilt = Properties.Settings.Default.TiltView;
                    if (Core.ActiveGeocache.ContainsCustomLatLon)
                    {
                        sgi.lat = (double)Core.ActiveGeocache.CustomLat;
                        sgi.lon = (double)Core.ActiveGeocache.CustomLon;
                    }
                    else
                    {
                        sgi.lat = Core.ActiveGeocache.Lat;
                        sgi.lon = Core.ActiveGeocache.Lon;
                    }
                    var jsonSerialiser = new JavaScriptSerializer();
                    var json = jsonSerialiser.Serialize(sgi);
                    executeScript(string.Format("setGeocache({0})", json));
                }
            }
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            base.SelectedLanguageChanged(sender, e);
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
        }

        public void UpdateView()
        {
            try
            {
                using (StreamReader textStreamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("GlobalcachingApplication.Plugins.GoogleEarth.Index.html")))
                {
                    string indexhtml = textStreamReader.ReadToEnd();
                    DisplayHtml(indexhtml);
                }
            }
            catch
            {
            }
        }

        private object executeScript(string script)
        {
            return _webBrowser.InvokeScript(script);
        }


        private void MapForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
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

        private void MapForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void DisplayHtml(string html)
        {
            _webBrowser.DocumentText = html;
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser1_DocumentCompleted), new object[] { sender, e });
                return;
            }
            setActiveGeocacheInMap();
        }
    }
}
