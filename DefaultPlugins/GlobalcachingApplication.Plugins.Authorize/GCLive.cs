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

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_AUTHORIZEONLY);
            AddAction(ACTION_AUTHORIZE);
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
            return result;
        }
    }
}
