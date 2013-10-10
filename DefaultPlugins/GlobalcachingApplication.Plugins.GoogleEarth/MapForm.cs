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

namespace GlobalcachingApplication.Plugins.GoogleEarth
{
    public partial class MapForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Google Earth";

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
        }

        void core_ActiveGeocacheChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (this.Visible && webBrowser1.ReadyState == WebBrowserReadyState.Complete)
            {
                setActiveGeocacheInMap();
            }
        }

        private void setActiveGeocacheInMap()
        {
            if (this.Visible)
            {
                if (Core.ActiveGeocache != null)
                {
                    if (Core.ActiveGeocache.ContainsCustomLatLon)
                    {
                        executeScript("setGeocache", new object[] { string.Format("{0}, {1}", Core.ActiveGeocache.Code, Core.ActiveGeocache.Name ?? ""), (double)Core.ActiveGeocache.CustomLat, (double)Core.ActiveGeocache.CustomLon, string.Format("file//", Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Map, Core.ActiveGeocache.GeocacheType)), Properties.Settings.Default.FlyToSpeed, Properties.Settings.Default.FixedView, Properties.Settings.Default.TiltView, Properties.Settings.Default.AltitudeView });
                    }
                    else
                    {
                        executeScript("setGeocache", new object[] { string.Format("{0}, {1}", Core.ActiveGeocache.Code, Core.ActiveGeocache.Name ?? ""), Core.ActiveGeocache.Lat, Core.ActiveGeocache.Lon, string.Format("file//", Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Map, Core.ActiveGeocache.GeocacheType)), Properties.Settings.Default.FlyToSpeed, Properties.Settings.Default.FixedView, Properties.Settings.Default.TiltView, Properties.Settings.Default.AltitudeView });
                    }
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
            webBrowser1.Navigate("about:blank");
            if (webBrowser1.Document != null)
            {
                webBrowser1.Document.Write(string.Empty);
            }
            webBrowser1.DocumentText = html;
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
