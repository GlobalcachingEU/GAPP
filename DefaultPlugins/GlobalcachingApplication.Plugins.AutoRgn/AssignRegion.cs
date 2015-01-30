using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.AutoRgn
{
    public class AssignRegion : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SHOW = "Assign regions to geocaches";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(AssignRegionForm.STR_ASSIGNINGREGION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AssignRegionForm.STR_CITY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AssignRegionForm.STR_COUNTRY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AssignRegionForm.STR_MUNICIPALITY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AssignRegionForm.STR_STATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AssignRegionForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AssignRegionForm.STR_UNASSIGNEDONLY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AssignRegionForm.STR_START));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AssignRegionForm.STR_SELECTEDONLY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AssignRegionForm.STR_NAMEPREFIX));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AssignRegionForm.STR_LEVEL));

            return await base.InitializeAsync(core);
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Action;
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                using (AssignRegionForm dlg = new AssignRegionForm(this, Core))
                {
                    dlg.ShowDialog();
                }
            }
            return result;
        }
    }
}
