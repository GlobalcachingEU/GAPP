using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace GlobalcachingApplication.Plugins.AutoRgn
{
    public partial class AssignRegionForm : Form
    {
        public const string STR_TITLE = "Assign regions to geocaches";
        public const string STR_COUNTRY = "Country";
        public const string STR_STATE = "State";
        public const string STR_MUNICIPALITY = "Municipality";
        public const string STR_CITY = "City";
        public const string STR_ASSIGNINGREGION = "Assigning regions...";
        public const string STR_LEVEL = "Level";
        public const string STR_NAMEPREFIX = "Name prefix";
        public const string STR_UNASSIGNEDONLY = "Unassigned only";
        public const string STR_SELECTEDONLY = "Selected only";
        public const string STR_START = "Start";

        private Framework.Interfaces.ICore _core = null;
        private Utils.BasePlugin.Plugin _plugin = null;
        private ManualResetEvent _actionReady = null;

        private Framework.Data.AreaType _level;
        private string _prefix = "";
        private List<Framework.Data.Geocache> _gcList = null;

        public AssignRegionForm()
        {
            InitializeComponent();
        }

        public AssignRegionForm(Utils.BasePlugin.Plugin plugin, Framework.Interfaces.ICore core) : this()
        {
            _plugin = plugin;
            _core = core;

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            comboBox1.Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_COUNTRY));
            comboBox1.Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_STATE));
            comboBox1.Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_MUNICIPALITY));
            comboBox1.Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_CITY));

            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LEVEL);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAMEPREFIX);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_UNASSIGNEDONLY);
            this.label8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTEDONLY);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_START);

            comboBox1.SelectedIndex = 1;
            checkBox2_CheckedChanged(this, EventArgs.Empty);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _prefix = textBox1.Text;
            _actionReady = new ManualResetEvent(false);
            _actionReady.Reset();
            using (Utils.FrameworkDataUpdater updater = new Utils.FrameworkDataUpdater(_core))
            {
                Thread thrd = new Thread(new ThreadStart(this.assignRegionsThreadMethod));
                thrd.Start();
                while (!_actionReady.WaitOne(100))
                {
                    System.Windows.Forms.Application.DoEvents();
                }
                thrd.Join();
            }
            _actionReady.Close();
            Close();
        }

        private void assignRegionsThreadMethod()
        {
            try
            {
                using (Utils.ProgressBlock prog = new Utils.ProgressBlock(_plugin, STR_ASSIGNINGREGION, STR_ASSIGNINGREGION, _gcList.Count, 0, true))
                {
                    List<Framework.Data.AreaInfo> areasFilter = Utils.GeometrySupport.Instance.GetAreasByLevel(_level);
                    if (areasFilter != null && areasFilter.Count > 0)
                    {
                        int index = 0;
                        foreach (var gc in _gcList)
                        {
                            List<Framework.Data.AreaInfo> areas = Utils.GeometrySupport.Instance.GetAreasOfLocation(new Framework.Data.Location(gc.Lat, gc.Lon), areasFilter);
                            if (areas != null && areas.Count > 0)
                            {
                                Framework.Data.AreaInfo ai = areas[0];
                                if (_prefix.Length > 0)
                                {
                                    ai = (from g in areas where g.Name.StartsWith(_prefix) select g).FirstOrDefault();
                                }
                                if (ai != null)
                                {
                                    switch (_level)
                                    {
                                        case Framework.Data.AreaType.Country:
                                            gc.Country = ai.Name;
                                            break;
                                        case Framework.Data.AreaType.State:
                                            gc.State = ai.Name;
                                            break;
                                        case Framework.Data.AreaType.Municipality:
                                            gc.Municipality = ai.Name;
                                            break;
                                        case Framework.Data.AreaType.City:
                                            gc.City = ai.Name;
                                            break;
                                    }
                                }
                            }
                            index++;
                            if (index % 50 == 0)
                            {
                                if (!prog.UpdateProgress(STR_ASSIGNINGREGION, STR_ASSIGNINGREGION, _gcList.Count, index))
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            _actionReady.Set();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            label9.Text = string.Format("({0})", (from Framework.Data.Geocache g in _core.Geocaches where (g.Selected || !checkBox2.Checked) select g).Count());
            comboBox1_SelectedIndexChanged(sender, e);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    _level = Framework.Data.AreaType.Country;
                    label10.Text = string.Format("({0})", (from Framework.Data.Geocache g in _core.Geocaches where (string.IsNullOrEmpty(g.Country) || !checkBox1.Checked) select g).Count());
                    _gcList = (from Framework.Data.Geocache g in _core.Geocaches where (string.IsNullOrEmpty(g.Country) || !checkBox1.Checked) && (g.Selected || !checkBox2.Checked) select g).ToList();
                    break;
                case 1:
                    _level = Framework.Data.AreaType.State;
                    label10.Text = string.Format("({0})", (from Framework.Data.Geocache g in _core.Geocaches where (string.IsNullOrEmpty(g.State) || !checkBox1.Checked) select g).Count());
                    _gcList = (from Framework.Data.Geocache g in _core.Geocaches where (string.IsNullOrEmpty(g.State) || !checkBox1.Checked) && (g.Selected || !checkBox2.Checked) select g).ToList();
                    break;
                case 2:
                    _level = Framework.Data.AreaType.Municipality;
                    label10.Text = string.Format("({0})", (from Framework.Data.Geocache g in _core.Geocaches where (string.IsNullOrEmpty(g.Municipality) || !checkBox1.Checked) select g).Count());
                    _gcList = (from Framework.Data.Geocache g in _core.Geocaches where (string.IsNullOrEmpty(g.Municipality) || !checkBox1.Checked) && (g.Selected || !checkBox2.Checked) select g).ToList();
                    break;
                case 3:
                    _level = Framework.Data.AreaType.City;
                    label10.Text = string.Format("({0})", (from Framework.Data.Geocache g in _core.Geocaches where (string.IsNullOrEmpty(g.City) || !checkBox1.Checked) select g).Count());
                    _gcList = (from Framework.Data.Geocache g in _core.Geocaches where (string.IsNullOrEmpty(g.City) || !checkBox1.Checked) && (g.Selected || !checkBox2.Checked) select g).ToList();
                    break;
                default:
                    _level = Framework.Data.AreaType.Other;
                    break;
            }
            label11.Text = string.Format("#{0}", _gcList.Count);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            comboBox1_SelectedIndexChanged(this, EventArgs.Empty);
        }
    }
}
