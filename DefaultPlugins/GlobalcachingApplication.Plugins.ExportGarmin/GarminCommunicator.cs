using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.ExportGarmin
{
    public class GarminCommunicator : Utils.BasePlugin.Plugin
    {
        public const string STR_NOGEOCACHESELECTED = "No geocache selected for export";
        public const string STR_ERROR = "Error";

        public const string ACTION_EXPORT_ACTIVE = "Export with Garmin Communicator|Active";
        public const string ACTION_EXPORT_SELECTED = "Export with Garmin Communicator|Selected";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_EXPORT_ACTIVE);
            AddAction(ACTION_EXPORT_SELECTED);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminCommunicatorForm.STR_CANCEL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminCommunicatorForm.STR_START));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminCommunicatorForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminCommunicatorForm.STR_INCLNOTES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminCommunicatorForm.STR_ADDCHILDWAYPOINTS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminCommunicatorForm.STR_USENAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminCommunicatorForm.STR_MAXNAMELENGTH));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminCommunicatorForm.STR_MINSTARTNAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminCommunicatorForm.STR_ADDWPTTODESCR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminCommunicatorForm.STR_USEHINTSDESCR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminCommunicatorForm.STR_GPXVERSION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminCommunicatorForm.STR_MAXLOGS));

            return await base.InitializeAsync(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return  Framework.PluginType.ExportData;
            }
        }


        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_EXPORT_ACTIVE || action == ACTION_EXPORT_SELECTED)
                {
                    List<Framework.Data.Geocache> gcList = null;
                    if (action == ACTION_EXPORT_SELECTED)
                    {
                        gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                    }
                    else
                    {
                        if (Core.ActiveGeocache != null)
                        {
                            gcList = new List<Framework.Data.Geocache>();
                            gcList.Add(Core.ActiveGeocache);
                        }
                    }
                    if (gcList == null || gcList.Count == 0)
                    {
                        System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                    else
                    {

                        using (GarminCommunicatorForm dlg = new GarminCommunicatorForm(Core, gcList))
                        {
                            dlg.ShowDialog();
                        }
                    }
                }
            }
            return result;
        }
    }
}
