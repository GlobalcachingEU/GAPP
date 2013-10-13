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

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public partial class SelectByAreaForm : Form
    {
        public const string STR_TITLE = "Import geocaches in area on map";
        public const string STR_SELECTWHOLEAREA = "Select whole area";
        public const string STR_LOCATION = "Location";
        public const string STR_GO = "Go";

        private Framework.Interfaces.ICore _core;
        private double _minLat;
        private double _minLon;
        private double _maxLat;
        private double _maxLon;

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

            Assembly assembly = Assembly.GetExecutingAssembly();
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.OKAPI.ImportByBBoxPage.html")))
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
            webBrowser1.DocumentText = html;
        }

        private void buttonWholeArea_Click(object sender, EventArgs e)
        {
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
    }
}
