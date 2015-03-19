using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.DevelopTests
{
    class GetOtherUserLogs : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SHOW = "Get user logs of SKAMS";

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
                    var logs = new List<Utils.API.LiveV6.GeocacheLog>();

                    using (var api = new Utils.API.GeocachingLiveV6(Core))
                    {
                        using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, "Importing logs", "Importing logs...", 100, 0, true))
                        {
                            var req = new Utils.API.LiveV6.GetUsersGeocacheLogsRequest();
                            req.AccessToken = api.Token;
                            req.Username = "SKAMS";
                            req.ExcludeArchived = true;
                            req.MaxPerPage = 30;
                            req.StartIndex = 0;
                            req.LogTypes = (from a in Core.LogTypes where a.AsFound select (long)a.ID).ToArray();
                            var resp = api.Client.GetUsersGeocacheLogs(req);
                            while (resp.Status.StatusCode == 0)
                            {
                                logs.AddRange(resp.Logs);

                                if (resp.Logs.Count() >= req.MaxPerPage)
                                {
                                    req.StartIndex = logs.Count;
                                    if (!progress.UpdateProgress("Importing logs", "Importing logs...", logs.Count + req.MaxPerPage, logs.Count))
                                    {
                                        break;
                                    }
                                    resp = api.Client.GetUsersGeocacheLogs(req);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if (resp.Status.StatusCode != 0)
                            {
                                //_errormessage = resp.Status.StatusMessage;
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            return result;
        }
    }
}
