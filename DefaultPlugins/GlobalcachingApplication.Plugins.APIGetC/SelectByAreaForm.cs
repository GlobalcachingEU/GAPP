using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace GlobalcachingApplication.Plugins.APIGetC
{
    public partial class SelectByAreaForm : Form
    {
        public const string STR_TITLE = "Select geocaches in area";
        public const string STR_SELECTWITHINREADIUS = "Select within radius";
        public const string STR_SELECTWHOLEAREA = "Select whole area";
        public const string STR_LOCATION = "Location";
        public const string STR_GO = "Go";
        public const string STR_DISTANCE = "Distance";

        private Framework.Interfaces.ICore _core;
        private bool _withinRadius = true;
        private double _minLat;
        private double _minLon;
        private double _maxLat;
        private double _maxLon;
        private Framework.Data.Location _center = null;
        private double _radius;

        public SelectByAreaForm()
        {
            InitializeComponent();
        }

        public SelectByAreaForm(Framework.Interfaces.ICore core)
        {
            InitializeComponent();

            _core = core;

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.buttonWholeArea.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTWHOLEAREA);
            this.buttonWithinRadius.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTWITHINREADIUS);

            Assembly assembly = Assembly.GetExecutingAssembly();
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.APIGetC.page.html")))
            {
                DisplayHtml(textStreamReader.ReadToEnd());
            }
        }

        private void DisplayHtml(string html)
        {
            webBrowser1.Navigate("about:blank");
            if (webBrowser1.Document != null)
            {
                webBrowser1.Document.Write(string.Empty);
            }
            html = html.Replace("google.maps.LatLng(0.0, 0.0)", string.Format("google.maps.LatLng({0}, {1})", _core.CenterLocation.SLat, _core.CenterLocation.SLon));
            html = html.Replace("SLocationS", Utils.LanguageSupport.Instance.GetTranslation(STR_LOCATION));
            html = html.Replace("SGoS", Utils.LanguageSupport.Instance.GetTranslation(STR_GO));
            html = html.Replace("SdistanceS", Utils.LanguageSupport.Instance.GetTranslation(STR_DISTANCE));
            webBrowser1.DocumentText = html;
        }

        private void buttonWithinRadius_Click(object sender, EventArgs e)
        {
            _withinRadius = true;
            try
            {
                if (webBrowser1.ReadyState == WebBrowserReadyState.Complete)
                {
                    object o = webBrowser1.Document.InvokeScript("getCenterPosition");
                    if (o != null && o.GetType() != typeof(DBNull))
                    {
                        string s = o.ToString().Replace("(", "").Replace(")", "");
                        _center = Utils.Conversion.StringToLocation(s);
                    }
                    else
                    {
                        return;
                    }
                    o = webBrowser1.Document.InvokeScript("getRadius");
                    if (o != null && o.GetType() != typeof(DBNull))
                    {
                        string s = o.ToString();
                        _radius = Utils.Conversion.StringToDouble(s);
                    }
                    else
                    {
                        return;
                    }
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    Close();
                }
            }
            catch
            {
            }
        }

        private void buttonWholeArea_Click(object sender, EventArgs e)
        {
            _withinRadius = false;
            try
            {
                if (webBrowser1.ReadyState == WebBrowserReadyState.Complete)
                {
                    object o = webBrowser1.Document.InvokeScript("getBounds");
                    if (o != null && o.GetType() != typeof(DBNull))
                    {
                        string s = o.ToString().Replace("(", "").Replace(")", "");
                        string[] parts = s.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        _minLat = Utils.Conversion.StringToDouble(parts[0]);
                        _minLon = Utils.Conversion.StringToDouble(parts[1]);
                        _maxLat = Utils.Conversion.StringToDouble(parts[2]);
                        _maxLon = Utils.Conversion.StringToDouble(parts[3]);
                    }
                }
            }
            catch
            {
            }
        }

        public bool WithinRadius
        {
            get { return _withinRadius; }
        }
        public double MinLat
        {
            get { return _minLat; }
        }
        public double MinLon
        {
            get { return _minLon; }
        }
        public double MaxLat
        {
            get { return _maxLat; }
        }
        public double MaxLon
        {
            get { return _maxLon; }
        }
        public double Radius
        {
            get { return _radius; }
        }
        public Framework.Data.Location Center
        {
            get { return _center; }
        }
    }

}
