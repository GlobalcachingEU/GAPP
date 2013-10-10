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
using System.Threading;

namespace GlobalcachingApplication.Plugins.CAR
{
    public partial class CachesAlongRouteForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Geocaches along a route";
        public const string STR_NEWSEARCH = "New search";
        public const string STR_SEARCHWITHINSELECTION = "Search within selection";
        public const string STR_ADDTOSELECTION = "Add to current selection";
        public const string STR_RESTART = "Restart";
        public const string STR_SELECT = "Select";
        public const string STR_KM = "km";
        public const string STR_MILES = "miles";
        public const string STR_SEARCHING = "Geocache along a route...";
        public const string STR_PROCESSINGSTEP = "Processing step...";
        public const string STR_ROUTEFROM = "Route from";
        public const string STR_ROUTETO = "Route to";

        private ManualResetEvent _threadReady = null;
        private double[] _lat = null;
        private double[] _lon = null;
        private double _difLat;
        private double _difLon;
        private List<Framework.Data.Geocache> _gcAlongRoute;

        public CachesAlongRouteForm()
        {
            InitializeComponent();
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            base.SelectedLanguageChanged(sender, e);
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.radioButtonAddToCurrent.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDTOSELECTION);
            this.radioButtonWithinSelection.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SEARCHWITHINSELECTION);
            this.radioButtonNewSearch.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NEWSEARCH);
            this.buttonReload.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RESTART);
            this.buttonSelect.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECT);
            this.radioButtonKm.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_KM);
            this.radioButtonMiles.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MILES);
            if (Visible)
            {
                buttonReload_Click(this, EventArgs.Empty);
            }
        }

        public CachesAlongRouteForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
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

            this.radioButtonKm.Checked = Properties.Settings.Default.UseMetric;
            this.radioButtonMiles.Checked = !Properties.Settings.Default.UseMetric;

            SelectedLanguageChanged(this, EventArgs.Empty);
        }


        private void CachesAlongRouteForm_FormClosing(object sender, FormClosingEventArgs e)
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
            sb.AppendLine(string.Format("var foundIcon = new google.maps.MarkerImage(\"{0}\");", System.IO.Path.Combine(new string[] { System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Images", "Map", "cachetypes", "gevonden.png" }).Replace("\\", "\\\\")));
            foreach (Framework.Data.GeocacheType gctype in Core.GeocacheTypes)
            {
                sb.AppendLine(string.Format("var gct{0}Icon = new google.maps.MarkerImage(\"{1}\");", gctype.ID.ToString().Replace("-", "_"), Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Map, gctype).Replace("\\", "\\\\")));
            }
            return template.Replace("//icons", sb.ToString());
        }

        private void addGeocachesToMap(List<Framework.Data.Geocache> gcList)
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
                if (gc.Found)
                {
                    sb.AppendLine(string.Format("{{a: '{0}', b: {1}, c: {2}, d: foundIcon}}", gc.Code, gc.Lat.ToString().Replace(',', '.'), gc.Lon.ToString().Replace(',', '.')));
                }
                else
                {
                    sb.AppendLine(string.Format("{{a: '{0}', b: {1}, c: {2}, d: gct{3}Icon}}", gc.Code, gc.Lat.ToString().Replace(',', '.'), gc.Lon.ToString().Replace(',', '.'), gc.GeocacheType.ID.ToString().Replace("-", "_")));
                }
            }
            sb.Append("]");
            executeScript("updateGeocaches", new object[] { sb.ToString() });
        }

        private void CachesAlongRouteForm_Shown(object sender, EventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.CAR.route.html")))
            {
                webBrowser1.Navigate("about:blank");
                if (webBrowser1.Document != null)
                {
                    webBrowser1.Document.Write(string.Empty);
                }
                string html = setIcons(textStreamReader.ReadToEnd().Replace("google.maps.LatLng(0.0, 0.0)", string.Format("google.maps.LatLng({0})", Core.CenterLocation.SLatLon)));
                html = html.Replace("SRoute fromS",Utils.LanguageSupport.Instance.GetTranslation(STR_ROUTEFROM));
                html = html.Replace("Sroute toS",Utils.LanguageSupport.Instance.GetTranslation(STR_ROUTETO));
                webBrowser1.DocumentText = html;
            }
            timer1.Enabled = true;
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

        private void buttonReload_Click(object sender, EventArgs e)
        {
            CachesAlongRouteForm_Shown(sender, e);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                bool v = buttonSelect.Enabled;
                try
                {
                    if (webBrowser1.ReadyState == WebBrowserReadyState.Complete)
                    {
                        v = bool.Parse(executeScript("isRouteAvailable", null).ToString());
                    }
                    else
                    {
                        v = false;
                    }

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
                catch
                {
                    v = false;
                }
                if (v != buttonSelect.Enabled)
                {
                    buttonSelect.Enabled = v;
                }
            }
            else
            {
                timer1.Enabled = false;
            }
        }

        private void CachesAlongRouteForm_Leave(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }

        private void CachesAlongRouteForm_Enter(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void buttonSelect_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.UseMetric = this.radioButtonKm.Checked;
            Properties.Settings.Default.Save();

            try
            {
                string s = executeScript("getRoute", null).ToString();
                string[] parts = s.Split(new char[] { ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                _lat = new double[parts.Length/2];
                _lon = new double[parts.Length / 2];
                int index = 0;
                for (int i = 0; i < parts.Length; i += 2)
                {
                    _lat[index] = Utils.Conversion.StringToDouble(parts[i]);
                    _lon[index] = Utils.Conversion.StringToDouble(parts[i+1]);
                    index++;
                }
                double kmOfRoute = (double)numericUpDownDist.Value;
                if (radioButtonMiles.Checked)
                {
                    kmOfRoute *= 1.6214;
                }
                _difLat = 0.009 * kmOfRoute;
                _difLon = 0.012 * kmOfRoute;

                _threadReady = new ManualResetEvent(false);
                Thread thrd = new Thread(new ThreadStart(this.performSelection));
                thrd.Start();
                while (!_threadReady.WaitOne(100))
                {
                    System.Windows.Forms.Application.DoEvents();
                }
                thrd.Join();

                Core.Geocaches.BeginUpdate();
                if (radioButtonAddToCurrent.Checked)
                {
                    foreach (var gc in _gcAlongRoute)
                    {
                        gc.Selected = true;
                    }
                }
                else if (radioButtonNewSearch.Checked)
                {
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        gc.Selected = false;
                    }
                    foreach (var gc in _gcAlongRoute)
                    {
                        gc.Selected = true;
                    }
                }
                else //within selection
                {
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        if (gc.Selected && !_gcAlongRoute.Contains(gc))
                        {
                            gc.Selected = false;
                        }
                    }
                }
                Core.Geocaches.EndUpdate();
                addGeocachesToMap((from wp in _gcAlongRoute where wp.Selected select wp).ToList());
            }
            catch
            {
            }
        }

        private void performSelection()
        {
            CachesAlongRoute pin = (OwnerPlugin as CachesAlongRoute);
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(pin, STR_SEARCHING, STR_PROCESSINGSTEP, _lat.Length, 0))
            {
                _gcAlongRoute = new List<Framework.Data.Geocache>();
                try
                {
                    int step = 0;
                    for (int i = 0; i < _lat.Length - 1; i += 2)
                    {
                        //process line
                        double minLat = Math.Min(_lat[i], _lat[i + 1]) - _difLat;
                        double maxLat = Math.Max(_lat[i], _lat[i + 1]) + _difLat;
                        double minLon = Math.Min(_lon[i], _lon[i + 1]) - _difLon;
                        double maxLon = Math.Max(_lon[i], _lon[i + 1]) + _difLon;

                        _gcAlongRoute.AddRange((from Framework.Data.Geocache wp in Core.Geocaches
                                                where !_gcAlongRoute.Contains(wp) && wp.Lat >= minLat && wp.Lat <= maxLat && wp.Lon >= minLon && wp.Lon <= maxLon
                                                select wp).ToList());

                        step++;
                        if (step > 50)
                        {
                            step = 0;
                            progress.UpdateProgress(STR_SEARCHING, STR_PROCESSINGSTEP, _lat.Length, i);
                        }
                    }
                }
                catch
                {
                }
            }
            _threadReady.Set();
        }

        private void CachesAlongRouteForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void CachesAlongRouteForm_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

    }

    public class CachesAlongRoute : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Search geocaches along a route";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(CachesAlongRouteForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CachesAlongRouteForm.STR_SEARCHWITHINSELECTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CachesAlongRouteForm.STR_NEWSEARCH));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CachesAlongRouteForm.STR_ADDTOSELECTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CachesAlongRouteForm.STR_RESTART));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CachesAlongRouteForm.STR_SELECT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CachesAlongRouteForm.STR_MILES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CachesAlongRouteForm.STR_KM));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CachesAlongRouteForm.STR_PROCESSINGSTEP));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CachesAlongRouteForm.STR_SEARCHING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CachesAlongRouteForm.STR_ROUTEFROM));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CachesAlongRouteForm.STR_ROUTETO));

            return base.Initialize(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.GeocacheSelectFilter;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new CachesAlongRouteForm(this, core));
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
