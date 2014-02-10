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

namespace GAPPSF.OKAPI
{
    /// <summary>
    /// Interaction logic for SelectAreaWindow.xaml
    /// </summary>
    public partial class ImportByRadiusWindow : Window
    {
        private bool _webBrowserReady = false;
        private string _html;

        public ImportByRadiusWindow()
        {
            InitializeComponent();

            DataContext = this;

            _html = Utils.ResourceHelper.GetEmbeddedTextFile("/OKAPI/ImportByRadiusWindow.html");

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

        private AsyncDelegateCommand _selectWithinRadiusCommand;
        public AsyncDelegateCommand SelectWithinRadiusCommand
        {
            get
            {
                if (_selectWithinRadiusCommand==null)
                {
                    _selectWithinRadiusCommand = new AsyncDelegateCommand(param => SelectWithinRadiusAsync(),
                        param => Core.ApplicationData.Instance.ActiveDatabase!=null && _webBrowserReady);
                }
                return _selectWithinRadiusCommand;
            }
        }
        public async Task SelectWithinRadiusAsync()
        {
            try
            {
                Core.Data.Location center = null;
                double radius = 0;
                string filter = null;

                object o = executeScript("getCenterPosition", null);
                if (o != null && o.GetType() != typeof(DBNull))
                {
                    string s = o.ToString().Replace("(", "").Replace(")", "");
                    center = Utils.Conversion.StringToLocation(s);
                }
                o = executeScript("getRadius", null);
                if (o != null && o.GetType() != typeof(DBNull))
                {
                    string s = o.ToString();
                    radius = Utils.Conversion.StringToDouble(s) * 1000.0;
                }

                using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
                {
                    await Task.Run(new Action(() =>
                        {
                            try
                            {
                                //first get a list of geocache codes
                                List<string> gcList = OKAPIService.GetGeocachesWithinRadius(SiteManager.Instance.ActiveSite, center, radius, filter);
                                using (Utils.ProgressBlock progress = new Utils.ProgressBlock("ImportingOpencachingGeocaches", "ImportingOpencachingGeocaches", gcList.Count, 0, true))
                                {
                                    int gcupdatecount = 1;
                                    int max = gcList.Count;
                                    while (gcList.Count > 0)
                                    {
                                        List<string> lcs = gcList.Take(gcupdatecount).ToList();
                                        gcList.RemoveRange(0, lcs.Count);
                                        List<OKAPIService.Geocache> caches = OKAPIService.GetGeocaches(SiteManager.Instance.ActiveSite, lcs);
                                        Import.AddGeocaches(Core.ApplicationData.Instance.ActiveDatabase, caches);

                                        if (!progress.Update("ImportingOpencachingGeocaches", max, max - gcList.Count))
                                        {
                                            break;
                                        }
                                    }

                                }
                            }
                            catch (Exception e)
                            {
                                Core.ApplicationData.Instance.Logger.AddLog(this, e);
                            }
                        }));
                }
            }
            catch (Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
        }

    }
}
