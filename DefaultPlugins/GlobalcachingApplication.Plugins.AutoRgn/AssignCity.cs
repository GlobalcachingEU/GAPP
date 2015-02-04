using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.AutoRgn
{
    public class AssignCity : Utils.BasePlugin.BaseImportFilter
    {
        public const string ACTION_ACTIVE = "Assign city|Active";
        public const string ACTION_SELECTED = "Assign city|Selected";
        public const string ACTION_ALL = "Assign city|All";

        public const string STR_NOGEOCACHESELECTED = "No geocache selected for export";
        public const string STR_ERROR = "Error";
        public const string STR_ASSIGNCITY = "Get city";

        private List<Framework.Data.Geocache> _gcList = null;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_ACTIVE);
            AddAction(ACTION_SELECTED);
            AddAction(ACTION_ALL);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ASSIGNCITY));

            return await base.InitializeAsync(core);
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_ACTIVE;
            }
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Action;
            }
        }

        protected override void ImportMethod()
        {
            int max = _gcList.Count; //caches, waypoints, logs
            int index = 0;
            DateTime nextUpdate = DateTime.Now.AddSeconds(1);
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_ASSIGNCITY, STR_ASSIGNCITY, max, 0, true))
            {
                foreach (Framework.Data.Geocache gc in _gcList)
                {
                    if (index>0)
                    {
                        System.Threading.Thread.Sleep(1000); //OSM policy
                    }
                    gc.City = Utils.Geocoder.GetCityName(gc.Lat, gc.Lon);

                    index++;
                    if (DateTime.Now >= nextUpdate)
                    {
                        if (!progress.UpdateProgress(STR_ASSIGNCITY, STR_ASSIGNCITY, max, index))
                        {
                            break;
                        }
                    }
                }
            }
        }
        public async override Task<bool> ActionAsync(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_ACTIVE || action == ACTION_SELECTED || action == ACTION_ALL)
                {
                    if (action == ACTION_ALL)
                    {
                        _gcList = (from Framework.Data.Geocache a in Core.Geocaches select a).ToList();
                    }
                    else if (action == ACTION_SELECTED)
                    {
                        _gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                    }
                    else
                    {
                        if (Core.ActiveGeocache != null)
                        {
                            _gcList = new List<Framework.Data.Geocache>();
                            _gcList.Add(Core.ActiveGeocache);
                        }
                    }
                    if (_gcList == null || _gcList.Count == 0)
                    {
                        System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                    else
                    {
                        await PerformImport();
                    }
                }
            }
            return result;
        }

    }
}
