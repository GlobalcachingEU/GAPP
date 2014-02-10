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
    public partial class ImportByBBoxWindow : Window
    {
        private bool _webBrowserReady = false;
        private string _html;

        public ImportByBBoxWindow()
        {
            InitializeComponent();

            DataContext = this;

            _html = Utils.ResourceHelper.GetEmbeddedTextFile("/OKAPI/ImportByBBoxWindow.html");

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

        private AsyncDelegateCommand _selectWithinBoundsCommand;
        public AsyncDelegateCommand SelectWithinBoundsCommand
        {
            get
            {
                if (_selectWithinBoundsCommand == null)
                {
                    _selectWithinBoundsCommand = new AsyncDelegateCommand(param => SelectWithinBoundsAsync(),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null && _webBrowserReady);
                }
                return _selectWithinBoundsCommand;
            }
        }
        public async Task SelectWithinBoundsAsync()
        {
            try
            {
                double minLat = 0.0;
                double minLon = 0.0;
                double maxLat = 0.0;
                double maxLon = 0.0;
                
                object o = executeScript("getBounds", null);
                if (o != null && o.GetType() != typeof(DBNull))
                {
                    string s = o.ToString().Replace("(", "").Replace(")", "");
                    string[] parts = s.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    minLat = Utils.Conversion.StringToDouble(parts[0]);
                    minLon = Utils.Conversion.StringToDouble(parts[1]);
                    maxLat = Utils.Conversion.StringToDouble(parts[2]);
                    maxLon = Utils.Conversion.StringToDouble(parts[3]);
                }

                using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
                {
                    await Task.Run(new Action(() =>
                    {
                        try
                        {
                            //first get a list of geocache codes
                            List<string> gcList = OKAPIService.GetGeocachesInBBox(SiteManager.Instance.ActiveSite, minLat, minLon, maxLat, maxLon);
                            using (Utils.ProgressBlock progress = new Utils.ProgressBlock("ImportGeocaches", "ImportGeocaches", gcList.Count, 0, true))
                            {
                                int gcupdatecount = 1;
                                int max = gcList.Count;
                                while (gcList.Count > 0)
                                {
                                    List<string> lcs = gcList.Take(gcupdatecount).ToList();
                                    gcList.RemoveRange(0, lcs.Count);
                                    List<OKAPIService.Geocache> caches = OKAPIService.GetGeocaches(SiteManager.Instance.ActiveSite, lcs);
                                    Import.AddGeocaches(Core.ApplicationData.Instance.ActiveDatabase, caches);

                                    if (!progress.Update("ImportGeocaches", max, max - gcList.Count))
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
