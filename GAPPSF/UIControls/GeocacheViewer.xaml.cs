using mshtml;
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

namespace GAPPSF.UIControls
{
    /// <summary>
    /// Interaction logic for GeocacheViewer.xaml
    /// </summary>
    public partial class GeocacheViewer : UserControl, IUIControl, IDisposable
    {
        private static string _defaultGeocacheTemplateHtml = null;
        private static string _defaultLogEvenTemplateHtml = null;
        private static string _defaultLogOddTemplateHtml = null;

        public GeocacheViewer()
        {
            InitializeComponent();
            UpdateView();

            if (_defaultGeocacheTemplateHtml==null)
            {
                _defaultGeocacheTemplateHtml = Utils.ResourceHelper.GetEmbeddedTextFile("/UIControls/GeocacheViewerGeocacheTemplate.html");
            }
            if (_defaultLogEvenTemplateHtml == null)
            {
                _defaultLogEvenTemplateHtml = Utils.ResourceHelper.GetEmbeddedTextFile("/UIControls/GeocacheViewerLogTemplateEven.html");
            }
            if (_defaultLogOddTemplateHtml == null)
            {
                _defaultLogOddTemplateHtml = Utils.ResourceHelper.GetEmbeddedTextFile("/UIControls/GeocacheViewerLogTemplateOdd.html");
            }

            Core.ApplicationData.Instance.PropertyChanged += Instance_PropertyChanged;
        }

        public void Dispose()
        {
            Core.ApplicationData.Instance.PropertyChanged -= Instance_PropertyChanged;
        }

        void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveGeocache")
            {
                UpdateView();
            }
        }

        public override string ToString()
        {
            return "Geocache viewer";
        }

        public void UpdateView()
        {
            if (Core.ApplicationData.Instance.ActiveGeocache == null)
            {
                DisplayHtml(string.Format("<html><head></head><body>{0}</body></html>", "No geocache selected"));
            }
            else
            {
                string s;
                s = _defaultGeocacheTemplateHtml;
                s = s.Replace("<!--code-->", Core.ApplicationData.Instance.ActiveGeocache.Code);
                s = s.Replace("<!--name-->", HttpUtility.HtmlEncode(Core.ApplicationData.Instance.ActiveGeocache.Name));
                s = s.Replace("<!--url-->", HttpUtility.HtmlEncode(Core.ApplicationData.Instance.ActiveGeocache.Url));
                s = s.Replace("<!--hint-->", HttpUtility.HtmlEncode(Core.ApplicationData.Instance.ActiveGeocache.EncodedHints));
                s = s.Replace("<!--personalnote-->", HttpUtility.HtmlEncode(Core.ApplicationData.Instance.ActiveGeocache.PersonalNote ?? ""));
                s = s.Replace("<!--note-->", Core.ApplicationData.Instance.ActiveGeocache.Notes ?? "");
                s = s.Replace("<!--personalnote_available-->", (!string.IsNullOrEmpty(Core.ApplicationData.Instance.ActiveGeocache.PersonalNote)).ToString().ToLower());
                s = s.Replace("<!--note_available-->", (!string.IsNullOrEmpty(Core.ApplicationData.Instance.ActiveGeocache.Notes)).ToString().ToLower());
                //s = s.Replace("SapppathS", System.IO.Path.GetDirectoryName(Application.ExecutablePath).Replace('\\', '/'));
                s = s.Replace("<!--available-->", Core.ApplicationData.Instance.ActiveGeocache.Available.ToString().ToLower());
                s = s.Replace("<!--archived-->", Core.ApplicationData.Instance.ActiveGeocache.Archived.ToString().ToLower());
                s = s.Replace("<!--coord-->", HttpUtility.HtmlEncode(Utils.Conversion.GetCoordinatesPresentation(Core.ApplicationData.Instance.ActiveGeocache.Lat, Core.ApplicationData.Instance.ActiveGeocache.Lon)));
                s = s.Replace("ScoordLatS", Core.ApplicationData.Instance.ActiveGeocache.Lat.ToString().Replace(',', '.'));
                s = s.Replace("ScoordLonS", Core.ApplicationData.Instance.ActiveGeocache.Lon.ToString().Replace(',', '.'));
                s = s.Replace("<!--custcoord_available-->", (Core.ApplicationData.Instance.ActiveGeocache.CustomLat != null && Core.ApplicationData.Instance.ActiveGeocache.CustomLon != null).ToString().ToLower());
                s = s.Replace("<!--custcoord-->", (Core.ApplicationData.Instance.ActiveGeocache.CustomLat != null && Core.ApplicationData.Instance.ActiveGeocache.CustomLon != null) ? HttpUtility.HtmlEncode(Utils.Conversion.GetCoordinatesPresentation((double)Core.ApplicationData.Instance.ActiveGeocache.CustomLat, (double)Core.ApplicationData.Instance.ActiveGeocache.CustomLon)) : "");
                //s = s.Replace("<!--userwaypoints_available-->", Core.ApplicationData.Instance.ActiveGeocache.HasUserWaypoints.ToString().ToLower());

                if (Core.ApplicationData.Instance.ActiveGeocache.ShortDescriptionInHtml)
                {
                    if (Core.ApplicationData.Instance.ActiveGeocache.Code.StartsWith("OC"))
                    {
                        s = s.Replace("<!--shortdescr-->", (Core.ApplicationData.Instance.ActiveGeocache.ShortDescription ?? "").Replace("\"images/uploads/", "\"http://www.opencaching.de/images/uploads/"));
                    }
                    else if (Core.ApplicationData.Instance.ActiveGeocache.Code.StartsWith("OB"))
                    {
                        s = s.Replace("<!--shortdescr-->", (Core.ApplicationData.Instance.ActiveGeocache.ShortDescription ?? "").Replace("\"images/uploads/", "\"http://www.opencaching.nl/images/uploads/"));
                    }
                    else
                    {
                        s = s.Replace("<!--shortdescr-->", Core.ApplicationData.Instance.ActiveGeocache.ShortDescription ?? "");
                    }
                }
                else
                {
                    s = s.Replace("<!--shortdescr-->", HttpUtility.HtmlEncode(Core.ApplicationData.Instance.ActiveGeocache.ShortDescription ?? "").Replace("\r\n", "<br />"));
                }
                if (Core.ApplicationData.Instance.ActiveGeocache.LongDescriptionInHtml)
                {
                    if (Core.ApplicationData.Instance.ActiveGeocache.Code.StartsWith("OC"))
                    {
                        s = s.Replace("<!--longdescr-->", (Core.ApplicationData.Instance.ActiveGeocache.LongDescription ?? "").Replace("\"images/uploads/", "\"http://www.opencaching.de/images/uploads/"));
                    }
                    else if (Core.ApplicationData.Instance.ActiveGeocache.Code.StartsWith("OB"))
                    {
                        s = s.Replace("<!--longdescr-->", (Core.ApplicationData.Instance.ActiveGeocache.LongDescription ?? "").Replace("\"images/uploads/", "\"http://www.opencaching.nl/images/uploads/"));
                    }
                    else
                    {
                        s = s.Replace("<!--longdescr-->", Core.ApplicationData.Instance.ActiveGeocache.LongDescription ?? "");
                    }
                }
                else
                {
                    s = s.Replace("<!--longdescr-->", HttpUtility.HtmlEncode(Core.ApplicationData.Instance.ActiveGeocache.LongDescription ?? "").Replace("\r\n", "<br />"));
                }

                DisplayHtml(s);
            }
        }

        private void DisplayHtml(string html)
        {
            html = html.Replace("SbyS", "by");
            webBrowser1.NavigateToString(html);
        }



        public int WindowWidth
        {
            get
            {
                return Core.Settings.Default.GCViewerWindowWidth;
            }
            set
            {
                Core.Settings.Default.GCViewerWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.GCViewerWindowHeight;
            }
            set
            {
                Core.Settings.Default.GCViewerWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.GCViewerWindowLeft;
            }
            set
            {
                Core.Settings.Default.GCViewerWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.GCViewerWindowTop;
            }
            set
            {
                Core.Settings.Default.GCViewerWindowTop = value;
            }
        }

    }
}
