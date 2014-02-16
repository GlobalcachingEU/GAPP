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

namespace GAPPSF.GPX
{
    /// <summary>
    /// Interaction logic for ExportGarminCommunicatorWindow.xaml
    /// </summary>
    public partial class ExportGarminCommunicatorWindow : Window
    {
        private List<Core.Data.Geocache> _gcList;
        private bool _cancled = false;
        private bool _started = false;
        private bool _pluginready = false;
        private int _index = 0;

        public ExportGarminCommunicatorWindow()
        {
            InitializeComponent();
        }

        public ExportGarminCommunicatorWindow(List<Core.Data.Geocache> gcList)
            : this()
        {
            _gcList = gcList;
            progBar.Maximum = gcList.Count();
            webBrowser1.ObjectForScripting = new webBrowserScriptingCallback(this);
            webBrowser1.LoadCompleted += webBrowser1_LoadCompleted;
            webBrowser1.Navigated += webBrowser1_Navigated;

            //DisplayHtml(Utils.ResourceHelper.GetEmbeddedTextFile("/GPX/ExportGarminCommunicatorWindow.html"));
            try
            {
                System.IO.TemporaryFile tmpFile = new System.IO.TemporaryFile(true);
                Utils.ResourceHelper.SaveToFile("/GPX/ExportGarminCommunicatorWindow.html", tmpFile.Path, true);
                webBrowser1.Navigate(string.Format("file://{0}", tmpFile.Path));
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
        }

        void webBrowser1_Navigated(object sender, NavigationEventArgs e)
        {
            Utils.ResourceHelper.HideScriptErrors(webBrowser1, true);
        }

        void webBrowser1_LoadCompleted(object sender, NavigationEventArgs e)
        {
        }

        private void DisplayHtml(string html)
        {
            _pluginready = false;
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

        public void garminPluginReady()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                _pluginready = true;
                if (_started && !_cancled)
                {
                    if (_index >= _gcList.Count)
                    {
                        Close();
                    }
                    else
                    {
                        List<Core.Data.Geocache> gcList = new List<Core.Data.Geocache>();
                        gcList.Add(_gcList[_index]);

                        GPX.Export gpxGenerator = new Export(gcList, Version.Parse(Core.Settings.Default.GPXVersion));
                        using (System.IO.TemporaryFile gpxFile = new System.IO.TemporaryFile(true))
                        {
                            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(gpxFile.Path, false, Encoding.UTF8))
                            {
                                //generate header
                                sw.Write(gpxGenerator.Start());
                                //preserve mem and do for each cache the export
                                for (int i = 0; i < gpxGenerator.Count; i++)
                                {
                                    sw.WriteLine(gpxGenerator.Next());
                                    if (Core.Settings.Default.GPXAddChildWaypoints)
                                    {
                                        string s = gpxGenerator.WaypointData();
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            sw.WriteLine(s);
                                        }
                                    }
                                }
                                //finalize
                                sw.Write(gpxGenerator.Finish());
                            }
                            _index++;
                            progBar.Value = _index;
                            _pluginready = false;
                            executeScript("uploadGpx", new object[] { System.IO.File.ReadAllText(gpxFile.Path), string.Format("{0}.gpx", gcList[0].Code) });
                        }
                    }
                }
            })); 
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cancled = true;
            Close();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            _started = true;
            startButton.Visibility = System.Windows.Visibility.Collapsed;
            cancelButton.Visibility = System.Windows.Visibility.Visible;
            if (_pluginready)
            {
                garminPluginReady();
            }
        }
    }

    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class webBrowserScriptingCallback
    {
        private ExportGarminCommunicatorWindow _gm;
        public webBrowserScriptingCallback(ExportGarminCommunicatorWindow gm)
        {
            _gm = gm;
        }
        public void garminPluginReady()
        {
            _gm.garminPluginReady();
        }
    }

}
