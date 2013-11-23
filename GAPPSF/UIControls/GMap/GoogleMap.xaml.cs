using System;
using System.Collections.Generic;
using System.Linq;
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
    public partial class GoogleMap : UserControl,IUIControl, IDisposable
    {
        private static string _multipleGeocacheHtml = null;
        private static string _multipleGeocacheJs = null;
        private static string _singleGeocacheHtml = null;
        private static string _singleGeocacheJs = null;

        public enum GeocachesOnMap
        {
            None,
            Active,
            Selected,
            All
        };

        private bool _webBrowserReady = false;
        private GeocachesOnMap _targetGeocaches = GeocachesOnMap.None;
        public GeocachesOnMap TargetGeocaches 
        {
            get { return _targetGeocaches; } 
            set
            {
                if (_targetGeocaches!=value)
                {
                    _targetGeocaches = value;
                    LoadHtml();
                    UpdateView();
                }
            }
        }

        public GoogleMap()
        {
            InitializeComponent();

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

        void webBrowser1_LoadCompleted(object sender, NavigationEventArgs e)
        {
            _webBrowserReady = true;
            UpdateView();
        }

        void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveGeocache")
            {
                UpdateView();
            }
        }

        public void Dispose()
        {
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

        private void addWaypointsToMap(List<Core.Data.Waypoint> wpList)
        {
        }

        public void UpdateView()
        {
            if (_webBrowserReady)
            {
                if (Core.ApplicationData.Instance.ActiveGeocache == null)
                {
                    executeScript("setGeocache", new object[] { "", "", "", Core.ApplicationData.Instance.CenterLocation.Lat, Core.ApplicationData.Instance.CenterLocation.Lon, "" });
                    addWaypointsToMap(null);
                }
                else
                {
                    if (Core.ApplicationData.Instance.ActiveGeocache.ContainsCustomLatLon)
                    {
                        executeScript("setGeocache", new object[] { Utils.ResourceHelper.GetEmbeddedHtmlImageData(string.Format("/Resources/CacheTypes/{0}", Core.ApplicationData.Instance.ActiveGeocache.GeocacheType.ID)), Core.ApplicationData.Instance.ActiveGeocache.Code, HttpUtility.HtmlEncode(Core.ApplicationData.Instance.ActiveGeocache.Name), Core.ApplicationData.Instance.ActiveGeocache.CustomLat, Core.ApplicationData.Instance.ActiveGeocache.CustomLon, string.Format("gct{0}IconC", Core.ApplicationData.Instance.ActiveGeocache.GeocacheType.ID.ToString().Replace("-", "_")) });
                    }
                    else
                    {
                        executeScript("setGeocache", new object[] { Utils.ResourceHelper.GetEmbeddedHtmlImageData(string.Format("/Resources/CacheTypes/{0}", Core.ApplicationData.Instance.ActiveGeocache.GeocacheType.ID)), Core.ApplicationData.Instance.ActiveGeocache.Code, HttpUtility.HtmlEncode(Core.ApplicationData.Instance.ActiveGeocache.Name), Core.ApplicationData.Instance.ActiveGeocache.Lat, Core.ApplicationData.Instance.ActiveGeocache.Lon, string.Format("gct{0}Icon", Core.ApplicationData.Instance.ActiveGeocache.GeocacheType.ID.ToString().Replace("-", "_")) });
                    }
                    addWaypointsToMap(Utils.DataAccess.GetWaypointsFromGeocache(Core.ApplicationData.Instance.ActiveGeocache.Database, Core.ApplicationData.Instance.ActiveGeocache.Code));

                }
            }
        }

        private object executeScript(string script, object[] pars)
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
    }
}
