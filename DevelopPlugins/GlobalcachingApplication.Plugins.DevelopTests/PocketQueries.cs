using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.DevelopTests
{
    public class PocketQueries : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SHOW = "Get realtime pocket query data";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            //AddAction(ACTION_SHOW);

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
                return ACTION_SHOW;
            }
        }


        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result && action == ACTION_SHOW)
            {
                if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, true))
                {
                    using (var client = new Utils.API.GeocachingLiveV6(Core, false))
                    {
                        var res = client.Client.GetPocketQueryList(client.Token);
                        if (res.Status.StatusCode == 0)
                        {
                            //res.CacheCodes
                            Guid g = res.PocketQueryList[0].GUID;
                            //g = Guid.Parse("c42cf092-28e8-4781-b62e-0de0e3e1a11d");

                            var resp2 = client.Client.GetPocketQueryData(client.Token, g, 0, 10, true);
                            if (resp2.Status.StatusCode == 0)
                            {
                                //resp2.
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
