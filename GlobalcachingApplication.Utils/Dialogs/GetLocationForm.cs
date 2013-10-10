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

namespace GlobalcachingApplication.Utils.Dialogs
{
    public partial class GetLocationForm : Form
    {
        public const string STR_GET_LOCATION = "Get location";
        public const string STR_LOCATION = "Location";
        public const string STR_OK = "OK";
        public const string STR_MAP = "Map";
        public const string STR_FROMHOMELOC = "From home location";
        public const string STR_FROMCENTERLOC = "From center location";

        private Framework.Data.Location _loc = null;
        private Framework.Interfaces.ICore _core = null;

        public GetLocationForm()
        {
            InitializeComponent();
        }

        public Framework.Data.Location Result
        {
            get { return _loc; }
        }

        public GetLocationForm(Framework.Interfaces.ICore core, Framework.Data.Location defaultLoc)
        {
            InitializeComponent();

            this.Text = LanguageSupport.Instance.GetTranslation(STR_GET_LOCATION);
            this.labelLocation.Text = LanguageSupport.Instance.GetTranslation(STR_LOCATION);
            this.buttonOK.Text = LanguageSupport.Instance.GetTranslation(STR_OK);
            this.buttonMap.Text = LanguageSupport.Instance.GetTranslation(STR_MAP);
            this.buttonFromHome.Text = LanguageSupport.Instance.GetTranslation(STR_FROMHOMELOC);
            this.buttonFromCenter.Text = LanguageSupport.Instance.GetTranslation(STR_FROMCENTERLOC);

            _core = core;
            if (defaultLoc != null)
            {
                _loc = new Framework.Data.Location(defaultLoc.Lat, defaultLoc.Lon);
                textBoxLocation.Text = Utils.Conversion.GetCoordinatesPresentation(_loc);
            }
        }

        private void buttonMap_Click(object sender, EventArgs e)
        {
            webBrowser1.Visible = true;
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Utils.Dialogs.GetLocationForm.html")))
            {
                webBrowser1.Navigate("about:blank");
                if (webBrowser1.Document != null)
                {
                    webBrowser1.Document.Write(string.Empty);
                }
                Framework.Data.Location ll = _loc;
                if (ll == null)
                {
                    ll = Utils.Conversion.StringToLocation(textBoxLocation.Text);
                    if (ll == null)
                    {
                        ll = _core.CenterLocation;
                    }
                }
                webBrowser1.DocumentText = textStreamReader.ReadToEnd().Replace("$Location$", LanguageSupport.Instance.GetTranslation(STR_LOCATION)).Replace("google.maps.LatLng(0.0, 0.0)", string.Format("google.maps.LatLng({0})", ll.SLatLon));
                timer1.Enabled = true;
            }
        }

        private void textBoxLocation_TextChanged(object sender, EventArgs e)
        {
            Framework.Data.Location l = Utils.Conversion.StringToLocation(textBoxLocation.Text);
            if (l == null)
            {
                buttonOK.Enabled = false;
            }
            else
            {
                buttonOK.Enabled = true;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _loc = Utils.Conversion.StringToLocation(textBoxLocation.Text);
        }

        private void buttonFromHome_Click(object sender, EventArgs e)
        {
            textBoxLocation.Text = Utils.Conversion.GetCoordinatesPresentation(_core.HomeLocation);
            if (webBrowser1.Visible && webBrowser1.ReadyState == WebBrowserReadyState.Complete)
            {
                webBrowser1.Document.InvokeScript("setCenter", new object[] { _core.HomeLocation.Lat, _core.HomeLocation.Lon });
            }
        }

        private void buttonFromCenter_Click(object sender, EventArgs e)
        {
            textBoxLocation.Text = Utils.Conversion.GetCoordinatesPresentation(_core.CenterLocation);
            if (webBrowser1.Visible && webBrowser1.ReadyState == WebBrowserReadyState.Complete)
            {
                webBrowser1.Document.InvokeScript("setCenter", new object[] { _core.CenterLocation.Lat, _core.CenterLocation.Lon });
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (webBrowser1.Visible)
            {
                if (webBrowser1.ReadyState == WebBrowserReadyState.Complete)
                {
                    object o = webBrowser1.Document.InvokeScript("getNewLocation");
                    if (o != null && o.GetType()!=typeof(DBNull))
                    {
                        string s = o.ToString().Replace("(", "").Replace(")", "");
                        Framework.Data.Location l = Utils.Conversion.StringToLocation(s);
                        if (l != null)
                        {
                            textBoxLocation.Text = Utils.Conversion.GetCoordinatesPresentation(l);
                        }
                    }
                }
            }
            else
            {
                timer1.Enabled = false;
            }
        }
    }
}
