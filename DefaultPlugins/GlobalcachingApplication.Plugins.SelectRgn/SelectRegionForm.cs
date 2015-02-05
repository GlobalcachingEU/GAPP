using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.SelectRgn
{
    public partial class SelectRegionForm : Form
    {
        public const string STR_TITLE = "Select geocaches within region";
        public const string STR_NEWSEARCH = "New search";
        public const string STR_SEARCHWITHINSELECTION = "Search within selection";
        public const string STR_ADDTOSELECTION = "Add to current selection";
        public const string STR_SELECTIONOPTIONS = "Selection options";
        public const string STR_REGIONSELECTION = "Region selection";
        public const string STR_LEVEL = "Level";
        public const string STR_AREA = "Area";
        public const string STR_PREFIX = "Prefix";
        public const string STR_SELECT = "Select";
        public const string STR_COUNTRY = "Country";
        public const string STR_STATE = "State";
        public const string STR_MUNICIPALITY = "Municipality";
        public const string STR_CITY = "City";
        public const string STR_OTHER = "Other";
        public const string STR_ALL = "-- All --";
        public const string STR_SEARCHING = "Searching...";
        public const string STR_INENVELOPE = "In envelope";

        private Framework.Interfaces.ICore _core = null;
        private SelectRegion _plugin = null;
        private List<Framework.Data.Geocache> _gcList = null;

        private Framework.Data.AreaType _level;
        private string _prefix = "";
        private string _areaName = "";
        private bool _inEnvelope = false;

        public SelectRegionForm()
        {
            InitializeComponent();
        }

        public SelectRegionForm(SelectRegion plugin, Framework.Interfaces.ICore core)
        {
            InitializeComponent();

            _plugin = plugin;
            _core = core;

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.radioButtonAddToCurrent.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDTOSELECTION);
            this.radioButtonWithinSelection.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SEARCHWITHINSELECTION);
            this.radioButtonNewSearch.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NEWSEARCH);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTIONOPTIONS);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_REGIONSELECTION);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LEVEL);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AREA);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PREFIX);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECT);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_INENVELOPE);

            comboBox1.Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_COUNTRY));
            comboBox1.Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_STATE));
            comboBox1.Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_MUNICIPALITY));
            comboBox1.Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_CITY));
            comboBox1.Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_OTHER));

            comboBox1.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();
            comboBox2.Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_ALL));
            Framework.Data.AreaType level;
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    level = Framework.Data.AreaType.Country;
                    break;
                case 1:
                    level = Framework.Data.AreaType.State;
                    break;
                case 2:
                    level = Framework.Data.AreaType.Municipality;
                    break;
                case 3:
                    level = Framework.Data.AreaType.City;
                    break;
                default:
                    level = Framework.Data.AreaType.Other;
                    break;
            }
            comboBox2.Items.AddRange(Utils.GeometrySupport.Instance.GetAreasByLevel(level).OrderBy(x => x.Name).ToArray());
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            _inEnvelope = checkBox1.Checked;
            if (radioButtonNewSearch.Checked)
            {
                _gcList = (from Framework.Data.Geocache g in _core.Geocaches select g).ToList();
            }
            else if (radioButtonAddToCurrent.Checked)
            {
                _gcList = (from Framework.Data.Geocache g in _core.Geocaches where !g.Selected select g).ToList();
            }
            else //within current
            {
                _gcList = (from Framework.Data.Geocache g in _core.Geocaches where g.Selected select g).ToList();
            }
            using (Utils.FrameworkDataUpdater updater = new Utils.FrameworkDataUpdater(_core))
            {
                if (radioButtonNewSearch.Checked || radioButtonWithinSelection.Checked)
                {
                    foreach (Framework.Data.Geocache g in _core.Geocaches)
                    {
                        g.Selected = false;
                    }
                }
                if (comboBox2.SelectedIndex < 1)
                {
                    _areaName = "";
                }
                else
                {
                    _areaName = comboBox2.Text;
                }
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        _level = Framework.Data.AreaType.Country;
                        break;
                    case 1:
                        _level = Framework.Data.AreaType.State;
                        break;
                    case 2:
                        _level = Framework.Data.AreaType.Municipality;
                        break;
                    case 3:
                        _level = Framework.Data.AreaType.City;
                        break;
                    default:
                        _level = Framework.Data.AreaType.Other;
                        break;
                }
                _prefix = textBox1.Text;

                await Task.Run(() =>
                {
                    this.performSelectionMethod();
                });
                Close();
            }
        }

        private void performSelectionMethod()
        {
            try
            {
                DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                using (Utils.ProgressBlock prog = new Utils.ProgressBlock(_plugin, STR_SEARCHING, STR_SEARCHING, _gcList.Count, 0, true))
                {
                    int index = 0;

                    //select the available areas
                    List<Framework.Data.AreaInfo> areas;
                    if (string.IsNullOrEmpty(_areaName))
                    {
                        areas = Utils.GeometrySupport.Instance.GetAreasByLevel(_level);
                    }
                    else
                    {
                        areas = Utils.GeometrySupport.Instance.GetAreasByName(_areaName, _level);
                    }
                    if (areas != null && areas.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(_prefix))
                        {
                            areas = (from a in areas where a.Name.StartsWith(_prefix, StringComparison.OrdinalIgnoreCase) select a).ToList();
                        }
                    }
                    if (areas != null && areas.Count > 0)
                    {
                        foreach (Framework.Data.Geocache g in _gcList)
                        {
                            /* //the lazy way, but not efficient
                            List<Framework.Data.AreaInfo> pointInAreas = Utils.GeometrySupport.Instance.GetAreasOfLocation(new Framework.Data.Location(g.Lat, g.Lon));

                            g.Selected = (pointInAreas != null &&
                                (from a in areas join b in pointInAreas on a equals b select a).Count() > 0);
                            */
                            if (_inEnvelope)
                            {
                                g.Selected = Utils.GeometrySupport.Instance.GetEnvelopAreasOfLocation(new Framework.Data.Location(g.Lat, g.Lon), areas).Count > 0;
                            }
                            else
                            {
                                g.Selected = Utils.GeometrySupport.Instance.GetAreasOfLocation(new Framework.Data.Location(g.Lat, g.Lon), areas).Count > 0;
                            }

                            index++;
                            if (DateTime.Now>=nextUpdate)
                            {
                                if (!prog.UpdateProgress(STR_SEARCHING, STR_SEARCHING, _gcList.Count, index))
                                {
                                    break;
                                }
                                nextUpdate = DateTime.Now.AddSeconds(1);
                            }
                        }
                    }
                    else
                    {
                        foreach (Framework.Data.Geocache g in _gcList)
                        {
                            g.Selected = false;
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }
}
