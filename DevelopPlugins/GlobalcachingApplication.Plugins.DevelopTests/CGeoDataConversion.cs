using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.DevelopTests
{
    public class CGeoDataConversion : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SHOW = "Get CGeo data conversion";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            //AddAction(ACTION_SHOW);

            return await base.InitializeAsync(core);
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
                    using (var client = new Utils.API.GeocachingLiveV6(Core))
                    {
                        StringBuilder sb = new StringBuilder();
                        var res = client.Client.GetAttributeTypesData(client.Token);
                        if (res.Status.StatusCode == 0)
                        {
                            foreach (var at in res.AttributeTypes)
                            {
                                sb.AppendLine(string.Format("htAttributes[{0}] = \"{1}\";", at.ID, at.IconName));
                            }
                        }
                        string fill = sb.ToString();
                    }
                }
            }
            return result;
        }
    }
}
