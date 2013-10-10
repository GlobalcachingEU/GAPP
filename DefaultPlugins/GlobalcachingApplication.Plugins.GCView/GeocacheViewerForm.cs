using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Permissions;

namespace GlobalcachingApplication.Plugins.GCView
{
    public partial class GeocacheViewerForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Geocache viewer";
        public const string STR_USEOFFLINEIMG = "Use offline images if available";
        public const string STR_BY = "by";
        public const string STR_NOGEOCACHESELECTED = "No geocache selected";
        public const string STR_OPENININTERNALBROWSER = "Open in internal web browser";
        public const string STR_MAXLOGS = "Max. logs";
        public const string STR_SHOWWPTS = "Show additional waypoints";

        internal string _defaultGeocacheTemplateHtml = "";
        internal string _defaultLogEvenTemplateHtml = "";
        internal string _defaultLogOddTemplateHtml = "";
        private bool _loadingPage = true;

        public GeocacheViewerForm()
        {
            InitializeComponent();
        }

        public GeocacheViewerForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            if (Properties.Settings.Default.WindowPos != null && !Properties.Settings.Default.WindowPos.IsEmpty)
            {
                this.Bounds = Properties.Settings.Default.WindowPos;
                this.StartPosition = FormStartPosition.Manual;
            }

            Assembly assembly = Assembly.GetExecutingAssembly();
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.GCView.GeocacheTemplate.html")))
            {
                _defaultGeocacheTemplateHtml = textStreamReader.ReadToEnd();
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.GCView.LogTemplateEven.html")))
            {
                _defaultLogEvenTemplateHtml = textStreamReader.ReadToEnd();
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.GCView.LogTemplateOdd.html")))
            {
                _defaultLogOddTemplateHtml = textStreamReader.ReadToEnd();
            }

            checkBox1.Checked = Properties.Settings.Default.OpenInInternalBrowser;
            checkBox2.Checked = Properties.Settings.Default.ShowAdditionalWaypoints;
            checkBoxOfflineImages.Checked = Properties.Settings.Default.UseOfflineImagesIfAvailable;
            numericUpDown1.Value = Properties.Settings.Default.ShowLogs;

            core.ActiveGeocacheChanged += new Framework.EventArguments.GeocacheEventHandler(core_ActiveGeocacheChanged);
            SelectedLanguageChanged(this, EventArgs.Empty);
            core.Geocaches.DataChanged += new Framework.EventArguments.GeocacheEventHandler(Geocaches_DataChanged);
            core.Geocaches.ListDataChanged += new EventHandler(Geocaches_ListDataChanged);
        }

        void Geocaches_ListDataChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                UpdateView();
            }
        }

        void Geocaches_DataChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (Visible)
            {
                if (e.Geocache == Core.ActiveGeocache)
                {
                    UpdateView();
                }
            }
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.checkBoxOfflineImages.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_USEOFFLINEIMG);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OPENININTERNALBROWSER);
            this.checkBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHOWWPTS);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAXLOGS);
            if (Visible)
            {
                UpdateView();
            }
        }


        void core_ActiveGeocacheChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (Visible)
            {
                UpdateView();
            }
        }

        public void UpdateView()
        {
            if (Core.ActiveGeocache == null)
            {
                DisplayHtml(string.Format("<html><head></head><body>{0}</body></html>", Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED)));
            }
            else
            {
                string s;
                if (string.IsNullOrEmpty(Properties.Settings.Default.GeocacheTemplateHtml))
                {
                    s = _defaultGeocacheTemplateHtml;
                }
                else
                {
                    s = Properties.Settings.Default.GeocacheTemplateHtml;
                }
                s = s.Replace("<!--code-->", Core.ActiveGeocache.Code);
                s = s.Replace("<!--name-->", HttpUtility.HtmlEncode(Core.ActiveGeocache.Name));
                s = s.Replace("<!--url-->", HttpUtility.HtmlEncode(Core.ActiveGeocache.Url));
                s = s.Replace("<!--hint-->", HttpUtility.HtmlEncode(Core.ActiveGeocache.EncodedHints));
                s = s.Replace("<!--personalnote-->", HttpUtility.HtmlEncode(Core.ActiveGeocache.PersonaleNote??""));
                s = s.Replace("<!--note-->", Core.ActiveGeocache.Notes ?? "");
                s = s.Replace("<!--personalnote_available-->", (!string.IsNullOrEmpty(Core.ActiveGeocache.PersonaleNote)).ToString().ToLower());
                s = s.Replace("<!--note_available-->", (!string.IsNullOrEmpty(Core.ActiveGeocache.Notes)).ToString().ToLower());
                s = s.Replace("SapppathS", System.IO.Path.GetDirectoryName(Application.ExecutablePath).Replace('\\', '/'));
                s = s.Replace("<!--available-->", Core.ActiveGeocache.Available.ToString().ToLower());
                s = s.Replace("<!--archived-->", Core.ActiveGeocache.Archived.ToString().ToLower());
                s = s.Replace("<!--coord-->", HttpUtility.HtmlEncode(Utils.Conversion.GetCoordinatesPresentation(Core.ActiveGeocache.Lat, Core.ActiveGeocache.Lon)));
                s = s.Replace("ScoordLatS", Core.ActiveGeocache.Lat.ToString().Replace(',', '.'));
                s = s.Replace("ScoordLonS", Core.ActiveGeocache.Lon.ToString().Replace(',', '.'));
                s = s.Replace("<!--custcoord_available-->", (Core.ActiveGeocache.CustomLat != null && Core.ActiveGeocache.CustomLon!=null).ToString().ToLower());
                s = s.Replace("<!--custcoord-->", (Core.ActiveGeocache.CustomLat != null && Core.ActiveGeocache.CustomLon != null) ? HttpUtility.HtmlEncode(Utils.Conversion.GetCoordinatesPresentation((double)Core.ActiveGeocache.CustomLat, (double)Core.ActiveGeocache.CustomLon)) : "");
                s = s.Replace("<!--userwaypoints_available-->", Core.ActiveGeocache.HasUserWaypoints.ToString().ToLower());
                List<Framework.Data.GeocacheImage> imgList = Utils.DataAccess.GetGeocacheImages(Core.GeocacheImages, Core.ActiveGeocache.Code);
                StringBuilder sbImgs = new StringBuilder();
                if (imgList != null && imgList.Count > 0)
                {
                    sbImgs.Append("<table>");
                    foreach (Framework.Data.GeocacheImage img in imgList)
                    {
                        sbImgs.Append("<tr>");
                        sbImgs.Append("<td>");
                        sbImgs.Append(string.Format("<img src=\"{0}\" />", System.IO.Path.Combine(new string[] { System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Images", "Default", "images.gif" })));
                        string link = img.Url;
                        if (Properties.Settings.Default.UseOfflineImagesIfAvailable)
                        {
                            string p = Utils.ImageSupport.Instance.GetImagePath(link);
                            if (!string.IsNullOrEmpty(p))
                            {
                                //link = string.Format("file://{0}",HttpUtility.UrlEncode(p));
                                //link = p.Replace("\\", "/");
                                link = string.Format("file:///{0}", p);
                            }
                        }
                        sbImgs.Append(string.Format(" <a href=\"{0}\">{1}</a>",link, HttpUtility.HtmlEncode(img.Name)));
                        sbImgs.Append("</td>");
                        sbImgs.Append("</tr>");
                    }
                    sbImgs.Append("</table>");
                }
                s = s.Replace("<!--imagelist-->", sbImgs.ToString());
                if (Core.ActiveGeocache.HasUserWaypoints)
                {
                    StringBuilder uwp = new StringBuilder();
                    List<Framework.Data.UserWaypoint> wpList = Utils.DataAccess.GetUserWaypointsFromGeocache(Core.UserWaypoints, Core.ActiveGeocache.Code);
                    foreach (Framework.Data.UserWaypoint wp in wpList)
                    {
                        if (uwp.Length > 0)
                        {
                            uwp.Append("<br />");
                        }
                        uwp.AppendFormat(HttpUtility.HtmlEncode(string.Format("{0} - {1} - {2}",Utils.Conversion.GetCoordinatesPresentation(wp.Lat,wp.Lon), wp.Description, wp.Date)));
                    }
                    s = s.Replace("<!--userwaypoints-->", uwp.ToString());
                }
                else
                {
                    s = s.Replace("<!--userwaypoints-->", "");
                }
                if (Core.ActiveGeocache.ShortDescriptionInHtml)
                {
                    if (Core.ActiveGeocache.Code.StartsWith("OC"))
                    {
                        s = s.Replace("<!--shortdescr-->", (Core.ActiveGeocache.ShortDescription ?? "").Replace("\"images/uploads/", "\"http://www.opencaching.de/images/uploads/"));
                    }
                    else if (Core.ActiveGeocache.Code.StartsWith("OB"))
                    {
                        s = s.Replace("<!--shortdescr-->", (Core.ActiveGeocache.ShortDescription ?? "").Replace("\"images/uploads/", "\"http://www.opencaching.nl/images/uploads/"));
                    }
                    else
                    {
                        s = s.Replace("<!--shortdescr-->", Core.ActiveGeocache.ShortDescription ?? "");
                    }
                }
                else
                {
                    s = s.Replace("<!--shortdescr-->", HttpUtility.HtmlEncode(Core.ActiveGeocache.ShortDescription ?? "").Replace("\r\n", "<br />"));
                }
                if (Core.ActiveGeocache.LongDescriptionInHtml)
                {
                    if (Core.ActiveGeocache.Code.StartsWith("OC"))
                    {
                        s = s.Replace("<!--longdescr-->", (Core.ActiveGeocache.LongDescription ?? "").Replace("\"images/uploads/", "\"http://www.opencaching.de/images/uploads/"));
                    }
                    else if (Core.ActiveGeocache.Code.StartsWith("OB"))
                    {
                        s = s.Replace("<!--longdescr-->", (Core.ActiveGeocache.LongDescription ?? "").Replace("\"images/uploads/", "\"http://www.opencaching.nl/images/uploads/"));
                    }
                    else
                    {
                        s = s.Replace("<!--longdescr-->", Core.ActiveGeocache.LongDescription ?? "");
                    }
                }
                else
                {
                    s = s.Replace("<!--longdescr-->", HttpUtility.HtmlEncode(Core.ActiveGeocache.LongDescription ?? "").Replace("\r\n", "<br />"));
                }

                List<Framework.Data.Waypoint> wpts = Utils.DataAccess.GetWaypointsFromGeocache(Core.Waypoints, Core.ActiveGeocache.Code);
                if (Properties.Settings.Default.ShowAdditionalWaypoints && wpts != null && wpts.Count > 0)
                {
                    StringBuilder awp = new StringBuilder();
                    awp.Append("<p>");
                    foreach (Framework.Data.Waypoint wp in wpts)
                    {
                        awp.AppendFormat("{0} - {1} ({2})<br />", HttpUtility.HtmlEncode(wp.ID ?? ""), HttpUtility.HtmlEncode(wp.Description ?? ""), HttpUtility.HtmlEncode(Utils.LanguageSupport.Instance.GetTranslation(wp.WPType.Name)));
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

                if (Properties.Settings.Default.UseOfflineImagesIfAvailable)
                {
                    s = checkForOfflineImages(s);
                }

                for (int i = 0; i < Core.ActiveGeocache.AttributeIds.Count; i++)
                {
                    s = s.Replace(string.Format("<!--attribute{0}-->", i), string.Format("<img src=\"{0}\" />", Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Medium, Utils.DataAccess.GetGeocacheAttribute(Core.GeocacheAttributes, Core.ActiveGeocache.AttributeIds[i]), Core.ActiveGeocache.AttributeIds[i] > 0 ? Framework.Data.GeocacheAttribute.State.Yes : Framework.Data.GeocacheAttribute.State.No)));
                }

                List<Framework.Data.Log> logs = Utils.DataAccess.GetLogs(Core.Logs, Core.ActiveGeocache.Code);
                StringBuilder sb = new StringBuilder();
                if (logs != null)
                {
                    bool odd = true;
                    foreach (Framework.Data.Log l in logs.Take(Properties.Settings.Default.ShowLogs).ToList())
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
                        lgtxt = lgtxt.Replace("SlogtypeimgS", Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Default, l.LogType));
                        lgtxt = lgtxt.Replace("<!--date-->", l.Date.ToString("d"));
                        lgtxt = lgtxt.Replace("<!--username-->", HttpUtility.HtmlEncode(l.Finder));
                        if (Core.ActiveGeocache.Code.StartsWith("OC") || Core.ActiveGeocache.Code.StartsWith("OB"))
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

                DisplayHtml(s);
            }
        }

        private string checkForOfflineImages(string html)
        {
            string result = html;
            try
            {
                Regex r = new Regex(@"</?\w+\s+[^>]*>", RegexOptions.Multiline);
                MatchCollection mc = r.Matches(html);
                foreach (Match m in mc)
                {
                    string s = m.Value.Substring(1).Replace('\r', ' ').Replace('\n', ' ').Trim();
                    if (s.StartsWith("img ", StringComparison.OrdinalIgnoreCase))
                    {
                        int pos = s.IndexOf(" src", StringComparison.OrdinalIgnoreCase);
                        pos = s.IndexOfAny(new char[] { '\'', '"' }, pos);
                        int pos2 = s.IndexOfAny(new char[] { '\'', '"' }, pos + 1);
                        string link = s.Substring(pos + 1, pos2 - pos - 1);

                        string p = Utils.ImageSupport.Instance.GetImagePath(link);
                        if (!string.IsNullOrEmpty(p))
                        {
                            result = result.Replace(link, p);
                        }
                    }
                }
            }
            catch
            {
            }
            return result;
        }

        private void GeocacheViewerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                Core.ActiveGeocacheChanged -= new Framework.EventArguments.GeocacheEventHandler(core_ActiveGeocacheChanged);
                Core.Geocaches.DataChanged -= new Framework.EventArguments.GeocacheEventHandler(Geocaches_DataChanged);
                Core.Geocaches.ListDataChanged -= new EventHandler(Geocaches_ListDataChanged);
            }
        }

        private void DisplayHtml(string html)
        {
            _loadingPage = true;
            webBrowser1.Navigate("about:blank");
            _loadingPage = true;
            if (webBrowser1.Document != null)
            {
                webBrowser1.Document.Write(string.Empty);
            }
            html = html.Replace("SbyS", Utils.LanguageSupport.Instance.GetTranslation(STR_BY));
            webBrowser1.DocumentText = html;
        }

        private void GeocacheViewerForm_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void GeocacheViewerForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void checkBoxOfflineImages_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.UseOfflineImagesIfAvailable = checkBoxOfflineImages.Checked;
            Properties.Settings.Default.Save();
            if (Visible)
            {
                UpdateView();
            }
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (!_loadingPage)
            {
                try
                {
                    if (Properties.Settings.Default.OpenInInternalBrowser)
                    {
                        Utils.BasePlugin.Plugin p = Utils.PluginSupport.PluginByName(Core, "GlobalcachingApplication.Plugins.Browser.BrowserPlugin") as Utils.BasePlugin.Plugin;
                        if (p != null)
                        {
                            var m = p.GetType().GetMethod("OpenNewBrowser");
                            m.Invoke(p, new object[] { e.Url.ToString() });
                        }
                    }
                    else
                    {
                        System.Diagnostics.Process.Start(e.Url.ToString());
                    }
                }
                catch
                {
                }
                e.Cancel = true;
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            _loadingPage = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.OpenInInternalBrowser = checkBox1.Checked;
            Properties.Settings.Default.Save();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowLogs = (int)numericUpDown1.Value;
            Properties.Settings.Default.Save();
            if (Visible)
            {
                UpdateView();
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowAdditionalWaypoints = checkBox2.Checked;
            Properties.Settings.Default.Save();
            if (Visible)
            {
                UpdateView();
            }
        }

    }

    public class GeocacheViewer : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "View Geocache";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheViewerForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheViewerForm.STR_USEOFFLINEIMG));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheViewerForm.STR_BY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheViewerForm.STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheViewerForm.STR_OPENININTERNALBROWSER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheViewerForm.STR_MAXLOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheViewerForm.STR_SHOWWPTS));

            return base.Initialize(core);
        }

        public override string FriendlyName
        {
            get
            {
                return "Geocache viewer";
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        public bool OpenInInternalBrowser()
        {
            return Properties.Settings.Default.OpenInInternalBrowser;
        }

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new GeocacheViewerForm(this, core));
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (UIChildWindowForm != null)
                {
                    if (action == ACTION_SHOW)
                    {
                        if (!UIChildWindowForm.Visible)
                        {
                            (UIChildWindowForm as GeocacheViewerForm).UpdateView();
                            UIChildWindowForm.Show();
                        }
                        if (UIChildWindowForm.WindowState == FormWindowState.Minimized)
                        {
                            UIChildWindowForm.WindowState = FormWindowState.Normal;
                        }
                        UIChildWindowForm.BringToFront();
                    }
                }
                result = true;
            }
            return result;
        }

        public override bool ApplySettings(List<UserControl> configPanels)
        {
            if (UIChildWindowForm.Visible)
            {
                (UIChildWindowForm as GeocacheViewerForm).UpdateView();
            }
            return true;
        }

        public override List<System.Windows.Forms.UserControl> CreateConfigurationPanels()
        {
            List<System.Windows.Forms.UserControl> pnls = base.CreateConfigurationPanels();
            if (pnls == null) pnls = new List<System.Windows.Forms.UserControl>();
            pnls.Add(new SettingsPanel(UIChildWindowForm as GeocacheViewerForm));
            return pnls;
        }

    }

}
