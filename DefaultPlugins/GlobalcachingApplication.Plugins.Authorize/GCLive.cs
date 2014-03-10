using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.Authorize
{
    public class GCLive: Utils.BasePlugin.Plugin
    {
        public const string ACTION_AUTHORIZEONLY = "Authorize|Get access only";
        public const string ACTION_AUTHORIZE = "Authorize|Get access and sync. settings";
        public const string ACTION_AUTHORIZE_MANUAL = "Authorize|Manual Live API Authorization";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_AUTHORIZEONLY);
            AddAction(ACTION_AUTHORIZE);
            AddAction(ACTION_AUTHORIZE_MANUAL);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCLiveManualForm.STR_AUTHORIZE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCLiveManualForm.STR_CONFIRM));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCLiveManualForm.STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCLiveManualForm.STR_STEP1));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCLiveManualForm.STR_STEP2));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCLiveManualForm.STR_STEP3));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCLiveManualForm.STR_TITLE));

            return base.Initialize(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.LiveAPI;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_AUTHORIZEONLY;
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result && action == ACTION_AUTHORIZEONLY)
            {
                Utils.API.GeocachingLiveV6.Authorize(Core, false);
            }
            else if (result && action == ACTION_AUTHORIZE)
            {
                Utils.API.GeocachingLiveV6.Authorize(Core, true);
            }
            else if (result && action == ACTION_AUTHORIZE_MANUAL)
            {
                using (GCLiveManualForm dlg = new GCLiveManualForm(Core))
                {
                    dlg.ShowDialog();
                }
            }
            return result;
        }
    }
}
