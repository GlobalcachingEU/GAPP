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

namespace GAPPSF.Dialogs
{
    /// <summary>
    /// Interaction logic for SelectAreaWindow.xaml
    /// </summary>
    public partial class SelectAreaWindow : Window
    {
        private bool _webBrowserReady = false;
        private string _html;

        public SelectAreaWindow()
        {
            InitializeComponent();

            DataContext = this;

            _html = Utils.ResourceHelper.GetEmbeddedTextFile("/Dialogs/SelectAreaWindow.html");

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
                var sc = selectionContext.GeocacheSelectionContext;                

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
                                List<Core.Data.Geocache> gcList;
                                if (sc == GAPPSF.UIControls.SelectionContext.Context.NewSelection)
                                {
                                    gcList = Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection;
                                }
                                else if (sc == GAPPSF.UIControls.SelectionContext.Context.WithinSelection)
                                {
                                    gcList = (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Selected select a).ToList();
                                }
                                else
                                {
                                    gcList = (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where !a.Selected select a).ToList();
                                }
                                DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                                using (Utils.ProgressBlock prog = new Utils.ProgressBlock("Searching", "Searching", gcList.Count, 0, true))
                                {
                                    int index = 0;
                                    foreach (var gc in gcList)
                                    {
                                        gc.Selected = Utils.Calculus.CalculateDistance(gc, center).EllipsoidalDistance <= radius;

                                        index++;
                                        if (DateTime.Now >= nextUpdate)
                                        {
                                            if (!prog.Update("Searching", gcList.Count, index))
                                            {
                                                break;
                                            }
                                            nextUpdate = DateTime.Now.AddSeconds(1);
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
                var sc = selectionContext.GeocacheSelectionContext;
                
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
                            List<Core.Data.Geocache> gcList;
                            if (sc == GAPPSF.UIControls.SelectionContext.Context.NewSelection)
                            {
                                gcList = Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection;
                            }
                            else if (sc == GAPPSF.UIControls.SelectionContext.Context.WithinSelection)
                            {
                                gcList = (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Selected select a).ToList();
                            }
                            else
                            {
                                gcList = (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where !a.Selected select a).ToList();
                            }
                            DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                            using (Utils.ProgressBlock prog = new Utils.ProgressBlock("Searching", "Searching", gcList.Count, 0, true))
                            {
                                int index = 0;
                                foreach (var gc in gcList)
                                {
                                    gc.Selected = (gc.Lat >= minLat && gc.Lat <= maxLat && gc.Lon >= minLon && gc.Lon <= maxLon);

                                    index++;
                                    if (DateTime.Now >= nextUpdate)
                                    {
                                        if (!prog.Update("Searching", gcList.Count, index))
                                        {
                                            break;
                                        }
                                        nextUpdate = DateTime.Now.AddSeconds(1);
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
