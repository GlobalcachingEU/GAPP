using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.GDirections
{
    public class GoogleDirections : Utils.BasePlugin.Plugin
    {
        public const string ACTION_START = "Create route for selected geocaches";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_START);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(GoogleDirectionsForm.STR_ADDTOSTOPS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GoogleDirectionsForm.STR_ADDWAYPOINT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GoogleDirectionsForm.STR_AUTOMATICROUTING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GoogleDirectionsForm.STR_AVWAYPOINTS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GoogleDirectionsForm.STR_CENTER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GoogleDirectionsForm.STR_CREATEROUTE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GoogleDirectionsForm.STR_CUSTOMWP));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GoogleDirectionsForm.STR_END));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GoogleDirectionsForm.STR_HOME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GoogleDirectionsForm.STR_PRINT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GoogleDirectionsForm.STR_START));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GoogleDirectionsForm.STR_STOPS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GoogleDirectionsForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GoogleDirectionsForm.STR_TOTALDISTANCE));

            return base.Initialize(core);
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_START;
            }
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return  Framework.PluginType.Action;
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_START)
                {
                    using (GoogleDirectionsForm dlg = new GoogleDirectionsForm(this, Core))
                    {
                        dlg.ShowDialog();
                    }
                }
            }
            return result;
        }
    }
}
