using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace GAPPSF.UIControls.OpenAreas
{
    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl, IUIControl, IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<CheckedListItem<Core.Data.WaypointType>> AvailableWaypointTypes { get; private set; }
        public ObservableCollection<string> CustomLocations { get; private set; }

        private bool _webBrowserReady = false;

        private string _selectedCustomLocation;
        public string SelectedCustomLocation
        {
            get { return _selectedCustomLocation; }
            set 
            { 
                SetProperty(ref _selectedCustomLocation, value);
                IsCustomLocationSelected = !string.IsNullOrEmpty(_selectedCustomLocation);
            }
        }

        private bool _isCustomLocationSelected;
        public bool IsCustomLocationSelected
        {
            get { return _isCustomLocationSelected; }
            set { SetProperty(ref _isCustomLocationSelected, value); }
        }
        
        public Control()
        {
            AvailableWaypointTypes = new ObservableCollection<CheckedListItem<Core.Data.WaypointType>>();
            var wpts = from a in Core.ApplicationData.Instance.WaypointTypes where a.ID>=0 select a;
            foreach (var wp in wpts)
            {
                AvailableWaypointTypes.Add(new CheckedListItem<Core.Data.WaypointType>(wp, true));
            }
            CustomLocations = new ObservableCollection<string>();
            if (!string.IsNullOrEmpty(Core.Settings.Default.OpenAreasCustomLocations))
            {
                string[] lines = Core.Settings.Default.OpenAreasCustomLocations.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach(string l in lines)
                {
                    CustomLocations.Add(l);
                }
            }

            InitializeComponent();

            webBrowser1.LoadCompleted += webBrowser1_LoadCompleted;
            webBrowser1.Navigated += webBrowser1_Navigated;

            ColorPickerGeocache.SelectedColor = (Color)ColorConverter.ConvertFromString(Core.Settings.Default.OpenAreasGeocacheColor);
            ColorPickerWaypoint.SelectedColor = (Color)ColorConverter.ConvertFromString(Core.Settings.Default.OpenAreasWaypointColor);
            ColorPickerCustom.SelectedColor = (Color)ColorConverter.ConvertFromString(Core.Settings.Default.OpenAreasCustomColor);

            DataContext = this;
            CustomLocations.CollectionChanged += CustomLocations_CollectionChanged;
        }

        void CustomLocations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in CustomLocations)
            {
                sb.AppendLine(s);
            }
            Core.Settings.Default.OpenAreasCustomLocations = sb.ToString();
        }

        public void Dispose()
        {

        }

        void webBrowser1_LoadCompleted(object sender, NavigationEventArgs e)
        {
            _webBrowserReady = true;
        }

        void webBrowser1_Navigated(object sender, NavigationEventArgs e)
        {
            Utils.ResourceHelper.HideScriptErrors(webBrowser1, true);
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

        private string addWaypointsToMap(string template, List<Core.Data.Waypoint> wpList)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("var wpList = [");
            bool first = true;
            if (wpList != null)
            {
                foreach (var wp in wpList)
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
                        bln.AppendFormat("{0}<br />", HttpUtility.HtmlEncode(Localization.TranslationManager.Instance.Translate(wp.WPType.Name)));
                        bln.AppendFormat("{0}<br />", HttpUtility.HtmlEncode(wp.Description)).Replace("\r\n", "<br />");
                        bln.AppendFormat("{0}", HttpUtility.HtmlEncode(wp.Comment).Replace("\r\n", "<br />"));
                        sb.AppendLine(string.Format("{{a: '{0}', b: {1}, c: {2}, d: wpt{3}Icon, e: '{4}'}}", wp.Code, wp.Lat.ToString().Replace(',', '.'), wp.Lon.ToString().Replace(',', '.'), wp.WPType.ID.ToString().Replace("-", "_"), bln.ToString().Replace("'", "").Replace("\r", "").Replace("\n", "")));
                    }
                }
            }
            sb.Append("];");
            return template.Replace("//waypoints", sb.ToString());
        }

        private string addGeocachesToMap(string template, List<Core.Data.Geocache> gcList)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("var gcList = [");
            bool first = true;
            foreach (var gc in gcList)
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

        private string addCircelsToMap(string template, List<Core.Data.Geocache> gcList, List<Core.Data.Waypoint> wpList, List<Core.Data.Location> custList)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("var circList = [");
            bool first = true;
            string radius = Core.Settings.Default.OpenAreasRadius.ToString().Replace(',', '.');
            string fillOpacity = ((double)Core.Settings.Default.OpenAreasFillOpacity / 100.0).ToString("0.##").Replace(',', '.');
            string strokeOpacity = ((double)Core.Settings.Default.OpenAreasStrokeOpacity / 100.0).ToString("0.##").Replace(',', '.');
            string gcColor = string.Format("#{0}{1}{2}", ColorPickerGeocache.SelectedColor.R.ToString("x2"), ColorPickerGeocache.SelectedColor.G.ToString("x2"), ColorPickerGeocache.SelectedColor.B.ToString("x2"));
            string wpColor = string.Format("#{0}{1}{2}", ColorPickerWaypoint.SelectedColor.R.ToString("x2"), ColorPickerWaypoint.SelectedColor.G.ToString("x2"), ColorPickerWaypoint.SelectedColor.B.ToString("x2"));
            string cuColor = string.Format("#{0}{1}{2}", ColorPickerCustom.SelectedColor.R.ToString("x2"), ColorPickerCustom.SelectedColor.G.ToString("x2"), ColorPickerCustom.SelectedColor.B.ToString("x2"));
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
                if (gc.ContainsCustomLatLon)
                {
                    sb.AppendLine(string.Format("{{a: {0}, b: {1}, c: {2}, d: {3}, e: {4}, f: '{5}'}}", gc.CustomLat.ToString().Replace(',', '.'), gc.CustomLon.ToString().Replace(',', '.'), radius, fillOpacity, strokeOpacity, gcColor));
                }
                else
                {
                    sb.AppendLine(string.Format("{{a: {0}, b: {1}, c: {2}, d: {3}, e: {4}, f: '{5}'}}", gc.Lat.ToString().Replace(',', '.'), gc.Lon.ToString().Replace(',', '.'), radius, fillOpacity, strokeOpacity, gcColor));
                }
            }
            foreach (Core.Data.Waypoint wp in wpList)
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
            foreach (Core.Data.Location wp in custList)
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

        private string positionMap(string template, List<Core.Data.Geocache> gcList)
        {
            double maxLat = gcList.Max(x => x.Lat);
            double minLat = gcList.Min(x => x.Lat);
            double maxLon = gcList.Max(x => x.Lon);
            double minLon = gcList.Min(x => x.Lon);

            string s = string.Format("map.fitBounds(new google.maps.LatLngBounds(new google.maps.LatLng({0},{1}), new google.maps.LatLng({2},{3})));", minLat.ToString().Replace(',', '.'), minLon.ToString().Replace(',', '.'), maxLat.ToString().Replace(',', '.'), maxLon.ToString().Replace(',', '.'));

            return template.Replace("//panToBounds", s);
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
            catch (Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
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
                return Core.Settings.Default.OpenAreasWindowWidth;
            }
            set
            {
                Core.Settings.Default.OpenAreasWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.OpenAreasWindowHeight;
            }
            set
            {
                Core.Settings.Default.OpenAreasWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.OpenAreasWindowLeft;
            }
            set
            {
                Core.Settings.Default.OpenAreasWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.OpenAreasWindowTop;
            }
            set
            {
                Core.Settings.Default.OpenAreasWindowTop = value;
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

        private void ColorPickerGeocache_ColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            Core.Settings.Default.OpenAreasGeocacheColor = ColorPickerGeocache.SelectedColor.ToString();
        }

        private void ColorPickerWaypoint_ColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            Core.Settings.Default.OpenAreasWaypointColor = ColorPickerWaypoint.SelectedColor.ToString();
        }

        private void ColorPickerCustom_ColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            Core.Settings.Default.OpenAreasCustomColor = ColorPickerCustom.SelectedColor.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                bool allUnknowns = !Core.Settings.Default.OpenAreasMysteryOnlyIfCorrected;
                List<Core.Data.Geocache> gcList = (from g in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where g.Selected && (g.GeocacheType.ID != 8 || allUnknowns || g.ContainsCustomLatLon) select g).ToList();
                if (gcList.Count > 0)
                {
                    List<Core.Data.Waypoint> wpList = new List<Core.Data.Waypoint>();
                    List<Core.Data.Location> custList = new List<Core.Data.Location>();
                    if (Core.Settings.Default.OpenAreasAddWaypoints)
                    {
                        int[] wpFilter = (from a in AvailableWaypointTypes where a.IsChecked select a.Item.ID).ToArray();
                        if (wpFilter.Length > 0)
                        {
                            foreach (var gc in gcList)
                            {
                                List<Core.Data.Waypoint> wpl = Utils.DataAccess.GetWaypointsFromGeocache(Core.ApplicationData.Instance.ActiveDatabase, gc.Code);
                                foreach (var wp in wpl)
                                {
                                    if (wpFilter.Contains(wp.WPType.ID) && wp.Lat != null && wp.Lon != null)
                                    {
                                        wpList.Add(wp);
                                    }
                                }
                            }
                        }
                    }
                    if (Core.Settings.Default.OpenAreasCustomWaypoints)
                    {
                        custList = (from string sl in CustomLocations select Utils.Conversion.StringToLocation(sl)).ToList();
                    }
                    string s = setIcons(Utils.ResourceHelper.GetEmbeddedTextFile("/UIControls/OpenAreas/MultipleGeocache.js"));
                    s = addWaypointsToMap(s, wpList);
                    s = addGeocachesToMap(s, gcList);
                    s = addCircelsToMap(s, gcList, wpList, custList);
                    s = positionMap(s, gcList);
                    s = Utils.ResourceHelper.GetEmbeddedTextFile("/UIControls/OpenAreas/MultipleGeocache.html").Replace("<!-- %MultipleGeocache.js% -->", s);
                    DisplayHtml(s);
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var dlg = new Dialogs.GetLocationWindow();
            if (dlg.ShowDialog()==true)
            {
                if (dlg.Location!=null)
                {
                    CustomLocations.Add(Utils.Conversion.GetCoordinatesPresentation(dlg.Location));
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SelectedCustomLocation))
            {
                CustomLocations.Remove(SelectedCustomLocation);
            }
        }

    }
}
