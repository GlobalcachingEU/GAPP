using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.DevelopTests
{
    public class Favorites : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SHOW = "Get Favorites";

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
                    using (var client = new Utils.API.GeocachingLiveV6(Core, true))
                    {
                        var res = client.Client.GetCacheIdsFavoritedByUser(client.Token);
                        if (res.Status.StatusCode == 0)
                        {
                            //res.CacheCodes
                        }
                        var resp2 = client.Client.GetCachesFavoritedByUser(client.Token);
                        if (resp2.Status.StatusCode == 0)
                        {
                            //resp2.
                        }
                        //client.Client.AddFavoritePointToCache(client.Token, "");
                        //client.Client.RemoveFavoritePointFromCache
                    }
                }
            }
            return result;
        }
    }
}
