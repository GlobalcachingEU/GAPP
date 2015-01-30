using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.DevelopTests
{
    public class UserWaypoints : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SHOW = "User Waypoints";

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
                    using (var client = new Utils.API.GeocachingLiveV6(Core, true))
                    {
                        var res = client.Client.GetUserWaypoints(client.Token, "GC2CHT2");
                        if (res.Status.StatusCode == 0)
                        {
                        }
                        ///*
                        var req = new Utils.API.LiveV6.SaveUserWaypointRequest();
                        req.AccessToken = client.Token;
                        req.CacheCode = "GC2CHT2";
                        req.Description = "Coordinate Override";
                        var ll = Utils.Conversion.StringToLocation("N 52° 14.431 E 005° 56.159");
                        req.Latitude = ll.Lat;
                        req.Longitude = ll.Lon;
                        var res2 = client.Client.SaveUserWaypoint(req);

                        if (res2.Status.StatusCode == 0)
                        {
                        }

                        res = client.Client.GetUserWaypoints(client.Token, "GC2CHT2");
                        if (res.Status.StatusCode == 0)
                        {
                        }
                         //* */
                    }
                }
            }
            return result;
        }
    }
}
