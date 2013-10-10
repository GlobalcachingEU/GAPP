using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.APILogT
{
    public class BatchLog : Utils.BasePlugin.Plugin
    {
        public const string ACTION_BATCH = "Log trackables";
        public const string ACTION_VISITFINDS = "Log trackable visits My Finds";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_BATCH);
            AddAction(ACTION_VISITFINDS);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(BatchLogForm.STR_ADD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BatchLogForm.STR_CANCEL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BatchLogForm.STR_DATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BatchLogForm.STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BatchLogForm.STR_LOG));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BatchLogForm.STR_LOGTYPE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BatchLogForm.STR_NOTE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BatchLogForm.STR_STOPATLOG));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BatchLogForm.STR_TBLIST));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BatchLogForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BatchLogForm.STR_TRACKABLES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BatchLogForm.STR_GEOCACHE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BatchLogForm.STR_CHECK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(BatchLogForm.STR_INPOSSESSION));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_AVAILABLELOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_BETWEEN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_CHECK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_CODE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_DOLOG));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_EXPLANATION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_EXPLANATIONTXT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_GETALLLOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_GETOWNED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_GETTINGLOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_ICON));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_LOGDATEUNKNOWN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_LOGGING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_LOGGINGOK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_LOGTEXT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_MESSAGES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_MISSING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_OR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_PRESENT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_SELECTTRACKABLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(VisitMyFindsForm.STR_TRACKINGNUMBER));

            return base.Initialize(core);
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_BATCH;
            }
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.LiveAPI;
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result && action == ACTION_BATCH)
            {
                if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, false))
                {
                    using (BatchLogForm dlg = new BatchLogForm(this, Core))
                    {
                        dlg.ShowDialog();
                    }
                }
            }
            else if (result && action == ACTION_VISITFINDS)
            {
                if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, false))
                {
                    using (VisitMyFindsForm dlg = new VisitMyFindsForm(this, Core))
                    {
                        dlg.ShowDialog();
                    }
                }
            }
            return result;
        }
    }
}
