using GAPPSF.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GAPPSF.UIControls.CAR
{
    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl, IUIControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        private bool _isRouteAvailable = false;
        public bool IsRouteAvailable
        {
            get { return _isRouteAvailable; }
            set { SetProperty(ref _isRouteAvailable, value); }
        }

        public Control()
        {
            InitializeComponent();

            webBrowser1.ObjectForScripting = new webBrowserScriptingCallback(this);
            webBrowser1.Navigated += webBrowser1_Navigated;

            DataContext = this;

            DisplayHtml();
        }

        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate("CachesAlongARoute") as string;
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
            return template.Replace("//icons", sb.ToString());
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

        private void DisplayHtml()
        {
            IsRouteAvailable = false;
            string html = Utils.ResourceHelper.GetEmbeddedTextFile("/UIControls/CAR/route.html");
            html = setIcons(html.Replace("google.maps.LatLng(0.0, 0.0)", string.Format("google.maps.LatLng({0})", Core.ApplicationData.Instance.CenterLocation.SLatLon)));
            html = html.Replace("SRoute fromS", Localization.TranslationManager.Instance.Translate("RouteFrom") as string);
            html = html.Replace("Sroute toS", Localization.TranslationManager.Instance.Translate("RouteTo") as string);
            webBrowser1.NavigateToString(html);
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

        public int WindowWidth
        {
            get
            {
                return Core.Settings.Default.CARWindowWidth;
            }
            set
            {
                Core.Settings.Default.CARWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.CARWindowHeight;
            }
            set
            {
                Core.Settings.Default.CARWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.CARWindowLeft;
            }
            set
            {
                Core.Settings.Default.CARWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.CARWindowTop;
            }
            set
            {
                Core.Settings.Default.CARWindowTop = value;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DisplayHtml();
        }

        private AsyncDelegateCommand _selectCommand;
        public AsyncDelegateCommand SelectCommand
        {
            get
            {
                if (_selectCommand==null)
                {
                    _selectCommand = new AsyncDelegateCommand(param => SelectGeocaches(), 
                        param=>IsRouteAvailable && Core.ApplicationData.Instance.ActiveDatabase!=null);
                }
                return _selectCommand;
            }
        }
        public async Task SelectGeocaches()
        {
            try
            {
                if (IsRouteAvailable && Core.ApplicationData.Instance.ActiveDatabase != null)
                {
                    string s = executeScript("getRoute", null).ToString();
                    string[] parts = s.Split(new char[] { ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    double[] lat = new double[parts.Length / 2];
                    double[] lon = new double[parts.Length / 2];
                    int index = 0;
                    for (int i = 0; i < parts.Length; i += 2)
                    {
                        lat[index] = Utils.Conversion.StringToDouble(parts[i]);
                        lon[index] = Utils.Conversion.StringToDouble(parts[i + 1]);
                        index++;
                    }
                    double kmOfRoute = Core.Settings.Default.CARRadius;
                    if (Core.Settings.Default.CARKm == GeocacheFilter.BooleanEnum.False)
                    {
                        kmOfRoute *= 1.6214;
                    }
                    double difLat = 0.009 * kmOfRoute;
                    double difLon = 0.012 * kmOfRoute;

                    using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
                    {
                        List<Core.Data.Geocache> gcAlongRoute = new List<Core.Data.Geocache>();
                        await Task.Run(() =>
                        {
                            try
                            {
                                using (Utils.ProgressBlock progress = new Utils.ProgressBlock("GeocacheAlongARoute", "ProcessingStep", lat.Length, 0))
                                {
                                    List<Core.Data.Geocache> gcList;
                                    if (selectionContext.GeocacheSelectionContext == SelectionContext.Context.NewSelection)
                                    {
                                        gcList = Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection;
                                    }
                                    else if (selectionContext.GeocacheSelectionContext == SelectionContext.Context.WithinSelection)
                                    {
                                        gcList = (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Selected select a).ToList();
                                    }
                                    else
                                    {
                                        gcList = (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where !a.Selected select a).ToList();
                                    }
                                    foreach (var gc in gcList)
                                    {
                                        gc.Selected = false;
                                    }
                                    try
                                    {
                                        DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                                        for (int i = 0; i < lat.Length - 1; i += 2)
                                        {
                                            //process line
                                            double minLat = Math.Min(lat[i], lat[i + 1]) - difLat;
                                            double maxLat = Math.Max(lat[i], lat[i + 1]) + difLat;
                                            double minLon = Math.Min(lon[i], lon[i + 1]) - difLon;
                                            double maxLon = Math.Max(lon[i], lon[i + 1]) + difLon;

                                            gcAlongRoute.AddRange((from Core.Data.Geocache wp in gcList
                                                                   where !gcAlongRoute.Contains(wp) && wp.Lat >= minLat && wp.Lat <= maxLat && wp.Lon >= minLon && wp.Lon <= maxLon
                                                                   select wp).ToList());

                                            if (DateTime.Now >= nextUpdate)
                                            {
                                                progress.Update("ProcessingStep", lat.Length, i);
                                                nextUpdate = DateTime.Now.AddSeconds(1);
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Core.ApplicationData.Instance.Logger.AddLog(this, e);
                                    }
                                }
                                foreach (var gc in gcAlongRoute)
                                {
                                    gc.Selected = true;
                                }
                            }
                            catch (Exception e)
                            {
                                Core.ApplicationData.Instance.Logger.AddLog(this, e);
                            }
                        });
                        addGeocachesToMap(gcAlongRoute);
                    }
                }
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
        }

        public void routeAvailable(bool avail)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                IsRouteAvailable = avail;
                CommandManager.InvalidateRequerySuggested();
            }));
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
    }

    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class webBrowserScriptingCallback
    {
        private Control _gm;
        public webBrowserScriptingCallback(Control gm)
        {
            _gm = gm;
        }
        public void geocacheClick(string code)
        {
            _gm.geocacheClick(code);
        }
        public void routeAvailable(bool avail)
        {
            _gm.routeAvailable(avail);
        }
    }

}
