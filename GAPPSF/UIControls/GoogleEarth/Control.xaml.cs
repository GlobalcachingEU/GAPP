using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

namespace GAPPSF.UIControls.GoogleEarth
{
    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl, IUIControl, IDisposable
    {
        private bool _webBrowserReady = false;
        private string _html;

        public Control()
        {
            InitializeComponent();

            _html = Utils.ResourceHelper.GetEmbeddedTextFile("/UIControls/GoogleEarth/Index.html");

            Core.ApplicationData.Instance.PropertyChanged += Instance_PropertyChanged;
            Core.Settings.Default.PropertyChanged += Default_PropertyChanged;
            webBrowser1.LoadCompleted += webBrowser1_LoadCompleted;
            webBrowser1.Navigated += webBrowser1_Navigated;

            LoadHtml();
        }

        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate("GoogleEarth") as string;
        }

        void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName=="ActiveGeocache")
            {
                UpdateView();
            }
        }

        void Default_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.StartsWith("GoogleEarth") &&
                !e.PropertyName.StartsWith("GoogleEarthWindow"))
            {
                //LoadHtml();
                UpdateView();
            }
        }

        public void LoadHtml()
        {
            DisplayHtml(_html);
        }


        public void Dispose()
        {
            Core.Settings.Default.PropertyChanged -= Default_PropertyChanged;
            Core.ApplicationData.Instance.PropertyChanged -= Instance_PropertyChanged;
        }

        void webBrowser1_Navigated(object sender, NavigationEventArgs e)
        {
            Utils.ResourceHelper.HideScriptErrors(webBrowser1, true);
        }

        void webBrowser1_LoadCompleted(object sender, NavigationEventArgs e)
        {
            _webBrowserReady = true;
            UpdateView();
        }


        private void UpdateView()
        {
            if (_webBrowserReady)
            {
                if (Core.ApplicationData.Instance.ActiveGeocache != null)
                {
                    if (Core.ApplicationData.Instance.ActiveGeocache.ContainsCustomLatLon)
                    {
                        executeScript("setGeocache", new object[] { string.Format("{0}, {1}", Core.ApplicationData.Instance.ActiveGeocache.Code, Core.ApplicationData.Instance.ActiveGeocache.Name ?? ""), (double)Core.ApplicationData.Instance.ActiveGeocache.CustomLat, (double)Core.ApplicationData.Instance.ActiveGeocache.CustomLon, "", Core.Settings.Default.GoogleEarthFlyToSpeed, Core.Settings.Default.GoogleEarthFixedView, Core.Settings.Default.GoogleEarthTiltView, Core.Settings.Default.GoogleEarthAltitudeView });
                    }
                    else
                    {
                        executeScript("setGeocache", new object[] { string.Format("{0}, {1}", Core.ApplicationData.Instance.ActiveGeocache.Code, Core.ApplicationData.Instance.ActiveGeocache.Name ?? ""), Core.ApplicationData.Instance.ActiveGeocache.Lat, Core.ApplicationData.Instance.ActiveGeocache.Lon, "", Core.Settings.Default.GoogleEarthFlyToSpeed, Core.Settings.Default.GoogleEarthFixedView, Core.Settings.Default.GoogleEarthTiltView, Core.Settings.Default.GoogleEarthAltitudeView });
                    }
                }
            }
        }

        private void DisplayHtml(string html)
        {
            _webBrowserReady = false;
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


        public int WindowWidth
        {
            get
            {
                return Core.Settings.Default.GoogleEarthWindowWidth;
            }
            set
            {
                Core.Settings.Default.GoogleEarthWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.GoogleEarthWindowHeight;
            }
            set
            {
                Core.Settings.Default.GoogleEarthWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.GoogleEarthWindowLeft;
            }
            set
            {
                Core.Settings.Default.GoogleEarthWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.GoogleEarthWindowTop;
            }
            set
            {
                Core.Settings.Default.GoogleEarthWindowTop = value;
            }
        }

    }
}
