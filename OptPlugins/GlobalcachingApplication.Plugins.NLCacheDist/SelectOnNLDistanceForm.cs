using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.NLCacheDist
{
    public partial class SelectOnNLDistanceForm : Form
    {
        public const string STR_TITLE = "Search for geocache distance";
        public const string STR_SELECTION = "Selection";
        public const string STR_FIND = "Find";
        public const string STR_NEWSEARCH = "New search";
        public const string STR_SEARCHWITHINSELECTION = "Search within selection";
        public const string STR_ADDTOSELECTION = "Add to current selection";
        public const string STR_MINDIST = "Min. distance";
        public const string STR_MAXDIST = "Max. distance";

        public SelectOnNLDistanceForm()
        {
            InitializeComponent();

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTION);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FIND);
            this.radioButtonNewSearch.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NEWSEARCH);
            this.radioButtonWithinSelection.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SEARCHWITHINSELECTION);
            this.radioButtonAddToCurrent.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDTOSELECTION);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MINDIST);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MAXDIST);
        }
    }

    public class GeocacheTextSearch : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SEARCH = "NL geocache distance";
        private double _minValue = 0.0;
        private double _maxValue = 0.0;

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.GeocacheSelectFilter;
            }
        }

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SEARCH);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectOnNLDistanceForm.STR_ADDTOSELECTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectOnNLDistanceForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectOnNLDistanceForm.STR_SELECTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectOnNLDistanceForm.STR_FIND));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectOnNLDistanceForm.STR_NEWSEARCH));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectOnNLDistanceForm.STR_SEARCHWITHINSELECTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectOnNLDistanceForm.STR_MINDIST));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectOnNLDistanceForm.STR_MAXDIST));

            return await base.InitializeAsync(core);
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                using (SelectOnNLDistanceForm dlg = new SelectOnNLDistanceForm())
                {
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Core.Geocaches.BeginUpdate();

                        if (dlg.radioButtonNewSearch.Checked)
                        {
                            //reset current
                            foreach (Framework.Data.Geocache gc in Core.Geocaches)
                            {
                                gc.Selected = false;
                            }
                        }

                        List<Framework.Data.Geocache> gcList;
                        if (dlg.radioButtonNewSearch.Checked)
                        {
                            gcList = (from Framework.Data.Geocache a in Core.Geocaches select a).ToList();
                        }
                        else if (dlg.radioButtonWithinSelection.Checked)
                        {
                            gcList = (from Framework.Data.Geocache a in Core.Geocaches where a.Selected select a).ToList();
                        }
                        else
                        {
                            gcList = (from Framework.Data.Geocache a in Core.Geocaches where !a.Selected select a).ToList();
                        }

                        _minValue = (double)dlg.numericUpDownMin.Value;
                        _maxValue = (double)dlg.numericUpDownMax.Value;
                        foreach(var gc in gcList)
                        {
                            gc.Selected = inRange(gc.GetCustomAttribute(GeocacheDistance.CUSTOM_ATTRIBUTE) as string);
                        }

                        Core.Geocaches.EndUpdate();
                    }
                }
            }
            return result;
        }

        private bool inRange(string actualValue)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(actualValue))
            {
                double v = Utils.Conversion.StringToDouble(actualValue);
                result = (v >= _minValue && v <= _maxValue);
            }
            return result;
        }
    }

}
