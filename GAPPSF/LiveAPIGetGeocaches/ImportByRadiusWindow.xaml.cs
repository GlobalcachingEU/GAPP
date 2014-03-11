using GAPPSF.Commands;
using System;
using System.Collections.Generic;
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

namespace GAPPSF.LiveAPIGetGeocaches
{
    /// <summary>
    /// Interaction logic for SelectAreaWindow.xaml
    /// </summary>
    public partial class ImportByRadiusWindow : Window
    {
        private bool _webBrowserReady = false;
        private string _html;

        public Core.Data.Location Center { get; set; }
        public double Radius { get; set; }

        public ImportByRadiusWindow()
        {
            InitializeComponent();

            DataContext = this;

            _html = Utils.ResourceHelper.GetEmbeddedTextFile("/LiveAPIGetGeocaches/ImportByRadiusWindow.html");

            webBrowser1.LoadCompleted += webBrowser1_LoadCompleted;
            webBrowser1.Navigated += webBrowser1_Navigated;

            LoadHtml();
        }

        public void LoadHtml()
        {
            DisplayHtml(_html);
        }

        void webBrowser1_Navigated(object sender, NavigationEventArgs e)
        {
            Utils.ResourceHelper.HideScriptErrors(webBrowser1, true);
        }

        void webBrowser1_LoadCompleted(object sender, NavigationEventArgs e)
        {
            _webBrowserReady = true;
        }

        private void DisplayHtml(string html)
        {
            _webBrowserReady = false;
            html = html.Replace("google.maps.LatLng(0.0, 0.0)", string.Format("google.maps.LatLng({0}, {1})", Core.ApplicationData.Instance.CenterLocation.SLat, Core.ApplicationData.Instance.CenterLocation.SLon));
            html = html.Replace("SLocationS", Localization.TranslationManager.Instance.Translate("Location") as string);
            html = html.Replace("SGoS", Localization.TranslationManager.Instance.Translate("GO") as string);
            html = html.Replace("SdistanceS", Localization.TranslationManager.Instance.Translate("Distance") as string);
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                object o = executeScript("getCenterPosition", null);
                if (o != null && o.GetType() != typeof(DBNull))
                {
                    string s = o.ToString().Replace("(", "").Replace(")", "");
                    Center = Utils.Conversion.StringToLocation(s);
                }
                else
                {
                    return;
                }
                o = executeScript("getRadius", null);
                if (o != null && o.GetType() != typeof(DBNull))
                {
                    string s = o.ToString();
                    Radius = Utils.Conversion.StringToDouble(s);
                }
                else
                {
                    return;
                }
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, ex);
            }
        }

    }
}
