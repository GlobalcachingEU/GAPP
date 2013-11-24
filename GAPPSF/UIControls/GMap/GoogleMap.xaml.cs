using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GAPPSF.UIControls.GMap
{
    /// <summary>
    /// Interaction logic for GoogleMap.xaml
    /// </summary>
    public partial class GoogleMap : UserControl,IUIControl, IDisposable, INotifyPropertyChanged
    {
        private static string _multipleGeocacheHtml = null;
        private static string _multipleGeocacheJs = null;
        private static string _singleGeocacheHtml = null;
        private static string _singleGeocacheJs = null;

        public event PropertyChangedEventHandler PropertyChanged;

        public enum GeocachesOnMap
        {
            None,
            Active,
            Selected,
            All
        };

        private Core.Storage.Database _currentConnectedDatabase = null;
        private bool _webBrowserReady = false;
        private GeocachesOnMap _targetGeocaches = GeocachesOnMap.None;
        public GeocachesOnMap TargetGeocaches 
        {
            get { return _targetGeocaches; } 
            set
            {
                if (_targetGeocaches!=value)
                {
                    SetProperty(ref _targetGeocaches, value);
                    LoadHtml();
                    UpdateView(false);
                }
            }
        }

        public GoogleMap()
        {
            DataContext = this;

            InitializeComponent();

            webBrowser1.ObjectForScripting = new webBrowserScriptingCallback(this);

            if (_multipleGeocacheHtml == null)
            {
                _multipleGeocacheHtml = Utils.ResourceHelper.GetEmbeddedTextFile("/UIControls/GMap/MultipleGeocache.html");
            }
            if (_multipleGeocacheJs == null)
            {
                _multipleGeocacheJs = Utils.ResourceHelper.GetEmbeddedTextFile("/UIControls/GMap/MultipleGeocache.js");
            }
            if (_singleGeocacheHtml == null)
            {
                _singleGeocacheHtml = Utils.ResourceHelper.GetEmbeddedTextFile("/UIControls/GMap/SingleGeocache.html");
            }
            if (_singleGeocacheJs == null)
            {
                _singleGeocacheJs = Utils.ResourceHelper.GetEmbeddedTextFile("/UIControls/GMap/SingleGeocache.js");
            }

            webBrowser1.LoadCompleted += webBrowser1_LoadCompleted;
            Core.ApplicationData.Instance.PropertyChanged += Instance_PropertyChanged;

            TargetGeocaches = GeocachesOnMap.Active;
        }

        public override string ToString()
        {
            return "Google map";
        }

        public void geocacheClick(string code)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (!string.IsNullOrEmpty(code) && Core.ApplicationData.Instance.ActiveDatabase != null)
                {
                    string[] parts = code.Split(new char[] { ',', ' ' });
                    Core.Data.Geocache gc = Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection.GetGeocache(parts[0]);
                    if (gc != null)
                    {
                        Core.ApplicationData.Instance.ActiveGeocache = gc;
                    }
                }
            })); 
        }

        private Core.Storage.Database CurrentConnectedDatabase
        {
            get { return _currentConnectedDatabase; }
            set
            {
                if (_currentConnectedDatabase != value)
                {
                    if (_currentConnectedDatabase!=null)
                    {
                        _currentConnectedDatabase.GeocacheCollection.CollectionChanged -= GeocacheCollection_CollectionChanged;
                        _currentConnectedDatabase.GeocacheCollection.GeocacheDataChanged -= GeocacheCollection_GeocacheDataChanged;
                        _currentConnectedDatabase.GeocacheCollection.GeocachePropertyChanged -= GeocacheCollection_GeocachePropertyChanged;
                    }
                    _currentConnectedDatabase = value;
                    if (_currentConnectedDatabase != null)
                    {
                        _currentConnectedDatabase.GeocacheCollection.CollectionChanged += GeocacheCollection_CollectionChanged;
                        _currentConnectedDatabase.GeocacheCollection.GeocacheDataChanged += GeocacheCollection_GeocacheDataChanged;
                        _currentConnectedDatabase.GeocacheCollection.GeocachePropertyChanged += GeocacheCollection_GeocachePropertyChanged;
                    }
                    UpdateView(false);
                }
            }
        }

        void GeocacheCollection_GeocachePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Core.Data.Geocache gc = sender as Core.Data.Geocache;
            if (gc != null)
            {
                if (gc == Core.ApplicationData.Instance.ActiveGeocache)
                {
                    UpdateView(true);
                }
                else if (TargetGeocaches == GeocachesOnMap.All || TargetGeocaches == GeocachesOnMap.Selected)
                {
                    UpdateView(false);
                }
            }
            else
            {
                UpdateView(true);
            }
        }

        void GeocacheCollection_GeocacheDataChanged(object sender, EventArgs e)
        {
            Core.Data.Geocache gc = sender as Core.Data.Geocache;
            if (gc != null)
            {
                if (gc == Core.ApplicationData.Instance.ActiveGeocache)
                {
                    UpdateView(true);
                }
                else if (TargetGeocaches == GeocachesOnMap.All || TargetGeocaches == GeocachesOnMap.Selected)
                {
                    UpdateView(false);
                }
            }
            else
            {
                UpdateView(true);
            }
        }

        void GeocacheCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (TargetGeocaches== GeocachesOnMap.All || TargetGeocaches== GeocachesOnMap.Selected)
            {
                UpdateView(false);
            }
        }

        void webBrowser1_LoadCompleted(object sender, NavigationEventArgs e)
        {
            _webBrowserReady = true;
            UpdateView(false);
        }

        void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveGeocache")
            {
                UpdateView(true);
            }
            else if (e.PropertyName == "ActiveDatabase")
            {
                CurrentConnectedDatabase = Core.ApplicationData.Instance.ActiveDatabase;
            }
        }

        public void Dispose()
        {
            CurrentConnectedDatabase = null;
            Core.ApplicationData.Instance.PropertyChanged -= Instance_PropertyChanged;
        }

        private string setIcons(string template)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("var foundIcon = new google.maps.MarkerImage(\"{0}\");", Utils.ResourceHelper.GetEmbeddedHtmlImageData("/Resources/CacheTypes/Map/gevonden.png")));
            sb.AppendLine(string.Format("var curposIcon = new google.maps.MarkerImage(\"{0}\");", Utils.ResourceHelper.GetEmbeddedHtmlImageData("/Resources/CacheTypes/Map/curpos.png")));
            foreach (Core.Data.GeocacheType gctype in Core.ApplicationData.Instance.GeocacheTypes)
            {
                sb.AppendLine(string.Format("var gct{0}Icon = new google.maps.MarkerImage(\"{1}\");", gctype.ID.ToString().Replace("-", "_"), Utils.ResourceHelper.GetEmbeddedHtmlImageData(string.Format("/Resources/CacheTypes/Map/{0}.png", gctype.ID))));
                sb.AppendLine(string.Format("var gct{0}IconC = new google.maps.MarkerImage(\"{1}\");", gctype.ID.ToString().Replace("-", "_"), Utils.ResourceHelper.GetEmbeddedHtmlImageData(string.Format("/Resources/CacheTypes/Map/c{0}.png", gctype.ID))));
            }
            foreach (Core.Data.WaypointType wptype in Core.ApplicationData.Instance.WaypointTypes)
            {
                sb.AppendLine(string.Format("var wpt{0}Icon = new google.maps.MarkerImage(\"{1}\");", wptype.ID.ToString().Replace("-", "_"), Utils.ResourceHelper.GetEmbeddedHtmlImageData(string.Format("/Resources/WPTypes/{0}.gif", wptype.ID))));
            }
            return template.Replace("//icons", sb.ToString());
        }

        public void LoadHtml()
        {
            if (TargetGeocaches == GeocachesOnMap.Active)
            {
                DisplayHtml(_singleGeocacheHtml.Replace("<!-- %SingleGeocache.js% -->", setIcons(_singleGeocacheJs)));
            }
            else
            {
                DisplayHtml(_multipleGeocacheHtml.Replace("<!-- %MultipleGeocache.js% -->", setIcons(_multipleGeocacheJs)));
            }
        }


        private void addGeocachesToMap(List<Core.Data.Geocache> gcList)
        {
            string coordAccuracy = "0.00000";

            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            bool first = true;
            foreach (Core.Data.Geocache gc in gcList)
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
                tt = string.Format("{0}, {1}", gc.Code, gc.Name.Replace('"', ' ').Replace('\'', ' ').Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ').Replace('\\', ' '));
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

        private void addWaypointsToMap(List<Core.Data.Waypoint> wpList)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            bool first = true;
            if (wpList != null)
            {
                foreach (Core.Data.Waypoint wp in wpList)
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
                        bln.AppendFormat("{0}<br />", HttpUtility.HtmlEncode(wp.WPType.Name));
                        bln.AppendFormat("{0}<br />", HttpUtility.HtmlEncode(wp.Description)).Replace("\r\n", "<br />");
                        bln.AppendFormat("{0}", HttpUtility.HtmlEncode(wp.Comment).Replace("\r\n", "<br />"));
                        sb.AppendLine(string.Format("{{a: '{0}', b: {1}, c: {2}, d: wpt{3}Icon, e: '{4}'}}", wp.Code, wp.Lat.ToString().Replace(',', '.'), wp.Lon.ToString().Replace(',', '.'), wp.WPType.ID.ToString().Replace("-", "_"), bln.ToString().Replace("'", "")));
                    }
                }
            }
            sb.Append("]");
            executeScript("updateWaypoints", new object[] { sb.ToString().Replace('\r', ' ').Replace('\n', ' ') });
        }

        public void UpdateView(bool activeGeocacheOnly)
        {
            if (_webBrowserReady)
            {
                if (!activeGeocacheOnly && Core.ApplicationData.Instance.ActiveDatabase != null)
                {
                    if (this._targetGeocaches == GeocachesOnMap.Selected)
                    {
                        addGeocachesToMap((from Core.Data.Geocache wp in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection
                                           select wp).ToList());
                    }
                    else if (this._targetGeocaches == GeocachesOnMap.All)
                    {
                        addGeocachesToMap(Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection);
                    }
                }
                if (Core.ApplicationData.Instance.ActiveGeocache == null)
                {
                    executeScript("setGeocache", new object[] { "", "", "", Core.ApplicationData.Instance.CenterLocation.Lat, Core.ApplicationData.Instance.CenterLocation.Lon, "" });
                    addWaypointsToMap(null);
                }
                else
                {
                    if (Core.ApplicationData.Instance.ActiveGeocache.ContainsCustomLatLon)
                    {
                        executeScript("setGeocache", new object[] { Utils.ResourceHelper.GetEmbeddedHtmlImageData(string.Format("/Resources/CacheTypes/{0}.gif", Core.ApplicationData.Instance.ActiveGeocache.GeocacheType.ID)), Core.ApplicationData.Instance.ActiveGeocache.Code, HttpUtility.HtmlEncode(Core.ApplicationData.Instance.ActiveGeocache.Name), Core.ApplicationData.Instance.ActiveGeocache.CustomLat, Core.ApplicationData.Instance.ActiveGeocache.CustomLon, string.Format("gct{0}IconC", Core.ApplicationData.Instance.ActiveGeocache.GeocacheType.ID.ToString().Replace("-", "_")) });
                    }
                    else
                    {
                        executeScript("setGeocache", new object[] { Utils.ResourceHelper.GetEmbeddedHtmlImageData(string.Format("/Resources/CacheTypes/{0}.gif", Core.ApplicationData.Instance.ActiveGeocache.GeocacheType.ID)), Core.ApplicationData.Instance.ActiveGeocache.Code, HttpUtility.HtmlEncode(Core.ApplicationData.Instance.ActiveGeocache.Name), Core.ApplicationData.Instance.ActiveGeocache.Lat, Core.ApplicationData.Instance.ActiveGeocache.Lon, string.Format("gct{0}Icon", Core.ApplicationData.Instance.ActiveGeocache.GeocacheType.ID.ToString().Replace("-", "_")) });
                    }
                    addWaypointsToMap((from a in Utils.DataAccess.GetWaypointsFromGeocache(Core.ApplicationData.Instance.ActiveGeocache.Database, Core.ApplicationData.Instance.ActiveGeocache.Code) where a.Lat!=null && a.Lon!=null select a).ToList());
                }
            }
        }

        private object executeScript(string script, object[] pars)
        {
            try
            {
                if (pars == null)
                {
                    return webBrowser1.InvokeScript(script);
                }
                else
                {
                    return webBrowser1.InvokeScript(script, pars);
                }
            }
            catch
            {
            }
            return null;
        }

        private void DisplayHtml(string html)
        {
            _webBrowserReady = false;
            webBrowser1.NavigateToString(html);
        }


        public int WindowWidth
        {
            get
            {
                return Core.Settings.Default.GoogleMapWindowWidth;
            }
            set
            {
                Core.Settings.Default.GoogleMapWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.GoogleMapWindowHeight;
            }
            set
            {
                Core.Settings.Default.GoogleMapWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.GoogleMapWindowLeft;
            }
            set
            {
                Core.Settings.Default.GoogleMapWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.GoogleMapWindowTop;
            }
            set
            {
                Core.Settings.Default.GoogleMapWindowTop = value;
            }
        }

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }

    }

    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class webBrowserScriptingCallback
    {
        private GoogleMap _gm;
        public webBrowserScriptingCallback(GoogleMap gm)
        {
            _gm = gm;
        }
        public void geocacheClick(string code)
        {
            _gm.geocacheClick(code);
        }
    }
}
