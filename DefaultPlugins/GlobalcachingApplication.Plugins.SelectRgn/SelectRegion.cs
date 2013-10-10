using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.SelectRgn
{
    public class SelectRegion : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SHOW = "Select geocaches within region";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectRegionForm.STR_ADDTOSELECTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectRegionForm.STR_ALL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectRegionForm.STR_AREA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectRegionForm.STR_CITY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectRegionForm.STR_COUNTRY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectRegionForm.STR_LEVEL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectRegionForm.STR_MUNICIPALITY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectRegionForm.STR_NEWSEARCH));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectRegionForm.STR_OTHER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectRegionForm.STR_PREFIX));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectRegionForm.STR_REGIONSELECTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectRegionForm.STR_SEARCHING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectRegionForm.STR_SEARCHWITHINSELECTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectRegionForm.STR_SELECT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectRegionForm.STR_SELECTIONOPTIONS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectRegionForm.STR_STATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectRegionForm.STR_TITLE));

            return base.Initialize(core);
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
                return Framework.PluginType.GeocacheSelectFilter;
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                using (SelectRegionForm dlg = new SelectRegionForm(this, Core))
                {
                    dlg.ShowDialog();
                }
            }
            return result;
        }
    }
}
