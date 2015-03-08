using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.DevelopTests
{
    public class GetOtherUserProfileTest : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SHOW = "Get user profile of thex";

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
                try
                {
                    using (var api = new Utils.API.GeocachingLiveV6(Core))
                    {
                        var req = new Utils.API.LiveV6.GetAnotherUsersProfileRequest();
                        req.AccessToken = api.Token;
                        req.UserID = 164977;
                        req.ProfileOptions = new Utils.API.LiveV6.UserProfileOptions();
                        req.ProfileOptions.PublicProfileData = true;
                        var p = api.Client.GetAnotherUsersProfile(req);
                    }
                }
                catch(Exception e)
                {
                }
            }
            return result;
        }
    }
}
