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

            if (_defaultGeocacheTemplateHtml==null)
            {
                _defaultGeocacheTemplateHtml = Utils.ResourceHelper.GetEmbeddedTextFile("/UIControls/GeocacheViewer/GeocacheViewerGeocacheTemplate.html");
            }
            if (_defaultLogEvenTemplateHtml == null)
            {
                _defaultLogEvenTemplateHtml = Utils.ResourceHelper.GetEmbeddedTextFile("/UIControls/GeocacheViewer/GeocacheViewerLogTemplateEven.html");
            }
            if (_defaultLogOddTemplateHtml == null)
            {
                _defaultLogOddTemplateHtml = Utils.ResourceHelper.GetEmbeddedTextFile("/UIControls/GeocacheViewer/GeocacheViewerLogTemplateOdd.html");
            }

            Core.ApplicationData.Instance.PropertyChanged += Instance_PropertyChanged;
            Core.Settings.Default.PropertyChanged += Default_PropertyChanged;

            UpdateView();
        }

        void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.StartsWith("GCViewer"))
            {
                UpdateView();
            }
        }

        public void Dispose()
        {
            Core.Settings.Default.PropertyChanged -= Default_PropertyChanged;
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
                DisplayHtml(string.Format("<html><head></head><body>{0}</body></html>", Localization.TranslationManager.Instance.TranslateText("No geocache selected")));
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
                s = s.Replace("<!--userwaypoints_available-->", Core.ApplicationData.Instance.ActiveGeocache.HasUserWaypoints.ToString().ToLower());
                List<Core.Data.GeocacheImage> imgList = Utils.DataAccess.GetGeocacheImages(Core.ApplicationData.Instance.ActiveGeocache.Database, Core.ApplicationData.Instance.ActiveGeocache.Code);
                StringBuilder sbImgs = new StringBuilder();
                if (imgList != null && imgList.Count > 0)
                {
                    sbImgs.Append("<table>");
                    foreach (Core.Data.GeocacheImage img in imgList)
                    {
                        sbImgs.Append("<tr>");
                        sbImgs.Append("<td>");
                        sbImgs.Append(string.Format("<img src=\"{0}\" />", Utils.ResourceHelper.GetEmbeddedHtmlImageData("/Resources/General/images.gif")));
                        string link = img.Url;
                        sbImgs.Append(string.Format(" <a href=\"{0}\">{1}</a>", link, HttpUtility.HtmlEncode(img.Name)));
                        sbImgs.Append("</td>");
                        sbImgs.Append("</tr>");
                    }
                    sbImgs.Append("</table>");
                }
                s = s.Replace("<!--imagelist-->", sbImgs.ToString());
                if (Core.ApplicationData.Instance.ActiveGeocache.HasUserWaypoints)
                {
                    StringBuilder uwp = new StringBuilder();
                    List<Core.Data.UserWaypoint> wpList = Utils.DataAccess.GetUserWaypointsFromGeocache(Core.ApplicationData.Instance.ActiveGeocache.Database, Core.ApplicationData.Instance.ActiveGeocache.Code);
                    foreach (Core.Data.UserWaypoint wp in wpList)
                    {
                        if (uwp.Length > 0)
                        {
                            uwp.Append("<br />");
                        }
                        uwp.AppendFormat(HttpUtility.HtmlEncode(string.Format("{0} - {1} - {2}", Utils.Conversion.GetCoordinatesPresentation(wp.Lat, wp.Lon), wp.Description, wp.Date)));
                    }
                    s = s.Replace("<!--userwaypoints-->", uwp.ToString());
                }
                else
                {
                    s = s.Replace("<!--userwaypoints-->", "");
                }

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
                List<Core.Data.Waypoint> wpts = Utils.DataAccess.GetWaypointsFromGeocache(Core.ApplicationData.Instance.ActiveGeocache.Database, Core.ApplicationData.Instance.ActiveGeocache.Code);
                if (Core.Settings.Default.GCViewerShowAdditionalWaypoints && wpts != null && wpts.Count > 0)
                {
                    StringBuilder awp = new StringBuilder();
                    awp.Append("<p>");
                    foreach (Core.Data.Waypoint wp in wpts)
                    {
                        awp.AppendFormat("{0} - {1} ({2})<br />", HttpUtility.HtmlEncode(wp.ID ?? ""), HttpUtility.HtmlEncode(wp.Description ?? ""), HttpUtility.HtmlEncode(wp.WPType.Name));
                        if (wp.Lat != null && wp.Lon != null)
                        {
                            awp.AppendFormat("{0}<br />", HttpUtility.HtmlEncode(Utils.Conversion.GetCoordinatesPresentation((double)wp.Lat, (double)wp.Lon)));
                        }
                        else
                        {
                            awp.Append("???<br />");
                        }
                        awp.AppendFormat("{0}<br /><br />", HttpUtility.HtmlEncode(wp.Comment ?? ""));
                    }
                    awp.Append("</p>");
                    s = s.Replace("<!--waypoints-->", awp.ToString());
                }
                else
                {
                    s = s.Replace("<!--waypoints-->", "");
                }
                List<int> attr = Core.ApplicationData.Instance.ActiveGeocache.AttributeIds;
                if (attr != null)
                {
                    for (int i = 0; i < attr.Count; i++)
                    {
                        s = s.Replace(string.Format("<!--attribute{0}-->", i), string.Format("<img src=\"{0}\" />", Utils.ResourceHelper.GetEmbeddedHtmlImageData(string.Format("/Resources/Attributes/{0}.gif",attr[i].ToString().Replace("-","_")))));
                    }
                }

                List<Core.Data.Log> logs = Utils.DataAccess.GetLogs(Core.ApplicationData.Instance.ActiveGeocache.Database, Core.ApplicationData.Instance.ActiveGeocache.Code);
                StringBuilder sb = new StringBuilder();
                if (logs != null)
                {
                    bool odd = true;
                    foreach (Core.Data.Log l in logs.Take(Core.Settings.Default.GCViewerShowLogs).ToList())
                    {
                        string lgtxt = "";
                        if (odd)
                        {
                            lgtxt = _defaultLogOddTemplateHtml;
                        }
                        else
                        {
                            lgtxt = _defaultLogEvenTemplateHtml;
                        }
                        lgtxt = lgtxt.Replace("SlogtypeimgS", Utils.ResourceHelper.GetEmbeddedHtmlImageData(string.Format("/Resources/LogTypes/{0}.gif",l.LogType.ID)));
                        lgtxt = lgtxt.Replace("<!--date-->", l.Date.ToString("d"));
                        lgtxt = lgtxt.Replace("<!--username-->", HttpUtility.HtmlEncode(l.Finder));
                        if (Core.ApplicationData.Instance.ActiveGeocache.Code.StartsWith("OC") || Core.ApplicationData.Instance.ActiveGeocache.Code.StartsWith("OB"))
                        {
                            //no parsing, opencaching.de/nl provides html
                            lgtxt = lgtxt.Replace("<!--logtext-->", l.Text);
                        }
                        else
                        {
                            lgtxt = lgtxt.Replace("<!--logtext-->", HttpUtility.HtmlEncode(l.Text).Replace("\r\n", "<br />"));
                        }

                        sb.Append(lgtxt);

                        odd = !odd;
                    }
                }
                s = s.Replace("<!--logs-->", sb.ToString());

                if (Core.Settings.Default.GCViewerUseOfflineImages)
                {
                    Dictionary<string, string> offimgl = GAPPSF.ImageGrabber.OfflineImagesManager.Instance.GetImages(Core.ApplicationData.Instance.ActiveGeocache);
                    foreach(var kp in offimgl)
                    {
                        s = s.Replace(kp.Key, GAPPSF.ImageGrabber.OfflineImagesManager.Instance.GetImageUri(Core.ApplicationData.Instance.ActiveGeocache, kp.Key));
                    }
                }

                DisplayHtml(s);
            }
        }

        private void DisplayHtml(string html)
        {
            html = html.Replace("SbyS", Localization.TranslationManager.Instance.TranslateText("by"));
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
