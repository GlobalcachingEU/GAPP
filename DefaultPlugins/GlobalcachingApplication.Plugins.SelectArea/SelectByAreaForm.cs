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
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.SelectArea
{
    public partial class SelectByAreaForm : Form
    {
        public const string STR_TITLE = "Select geocaches in area";
        public const string STR_NEWSEARCH = "New search";
        public const string STR_SEARCHWITHINSELECTION = "Search within selection";
        public const string STR_ADDTOSELECTION = "Add to current selection";
        public const string STR_SELECTWITHINREADIUS = "Select within radius";
        public const string STR_SELECTWHOLEAREA = "Select whole area";
        public const string STR_LOCATION = "Location";
        public const string STR_GO = "Go";
        public const string STR_DISTANCE = "Distance";

        public enum SelectionSelect
        {
            NewSearch,
            WithinSelection,
            AddToSelection
        }

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
            this.radioButtonAddToCurrent.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDTOSELECTION);
            this.radioButtonWithinSelection.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SEARCHWITHINSELECTION);
            this.radioButtonNewSearch.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NEWSEARCH);
            this.buttonWholeArea.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTWHOLEAREA);
            this.buttonWithinRadius.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTWITHINREADIUS);

            Assembly assembly = Assembly.GetExecutingAssembly();
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.SelectArea.page.html")))
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
                    o = webBrowser1.Document.InvokeScript("getRadius");
                    if (o != null && o.GetType() != typeof(DBNull))
                    {
                        string s = o.ToString();
                        _radius = Utils.Conversion.StringToDouble(s);
                    }
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

        public SelectionSelect SearchOption
        {
            get
            {
                if (radioButtonNewSearch.Checked) return SelectionSelect.NewSearch;
                else if (radioButtonWithinSelection.Checked) return SelectionSelect.WithinSelection;
                else return SelectionSelect.AddToSelection;
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

    public class GeocacheViewer : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SELECT = "Select geocaches by area";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SELECT);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectByAreaForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectByAreaForm.STR_ADDTOSELECTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectByAreaForm.STR_NEWSEARCH));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectByAreaForm.STR_SEARCHWITHINSELECTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectByAreaForm.STR_SELECTWHOLEAREA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectByAreaForm.STR_SELECTWITHINREADIUS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectByAreaForm.STR_GO));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectByAreaForm.STR_LOCATION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectByAreaForm.STR_DISTANCE));

            return await base.InitializeAsync(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return  Framework.PluginType.GeocacheSelectFilter;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SELECT;
            }
        }


        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                using (SelectByAreaForm dlg = new SelectByAreaForm(Core))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Core.Geocaches.BeginUpdate();

                        SelectByAreaForm.SelectionSelect searchOption = dlg.SearchOption;
                        bool withinRadius = dlg.WithinRadius;
                        double radius = 0.0;
                        double minlat = 0.0;
                        double minlon = 0.0;
                        double maxlat = 0.0;
                        double maxlon = 0.0;
                        Framework.Data.Location center = null;
                        if (withinRadius)
                        {
                            radius = dlg.Radius * 1000.0;
                            center = dlg.Center;
                        }
                        else{
                            minlat = dlg.MinLat;
                            minlon = dlg.MinLon;
                            maxlat = dlg.MaxLat;
                            maxlon = dlg.MaxLon;
                        }

                        if (searchOption == SelectByAreaForm.SelectionSelect.NewSearch)
                        {

                            //reset current
                            foreach (Framework.Data.Geocache gc in Core.Geocaches)
                            {
                                gc.Selected = false;
                            }
                        }

                        var gcList = (from Framework.Data.Geocache wp in Core.Geocaches
                                     where ((searchOption != SelectByAreaForm.SelectionSelect.WithinSelection) || wp.Selected)
                                            && (!withinRadius || Utils.Calculus.CalculateDistance(wp, center).EllipsoidalDistance<=radius)
                                            && (withinRadius || ( wp.Lat >= minlat && wp.Lat <= maxlat && wp.Lon >= minlon && wp.Lon <= maxlon))
                                     select wp).ToList();

                        if (searchOption == SelectByAreaForm.SelectionSelect.WithinSelection)
                        {
                            //reset current
                            foreach (Framework.Data.Geocache gc in Core.Geocaches)
                            {
                                gc.Selected = false;
                            }
                        }

                        foreach (Framework.Data.Geocache gc in gcList)
                        {
                            gc.Selected = true;
                        }

                        Core.Geocaches.EndUpdate();
                    }
                }
                result = true;
            }
            return result;
        }

    }

}
