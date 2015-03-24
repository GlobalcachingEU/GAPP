using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.APIMyF
{
    public class MyFinds : Utils.BasePlugin.BaseImportFilter
    {
        public const string STR_IMPORTINGMYF = "Importing geocaching.com my finds...";
        public const string STR_IMPORTINGLOGS = "Importing geocaching.com logs...";
        public const string STR_IMPORTINGCACHES = "Importing geocaching.com geocaches...";
        public const string STR_ERROR = "Error";

        public const string ACTION_IMPORT = "Import My Finds";

        private string _errormessage = null;
        private int _apiLimit = -1;
        private int _apiLeft = -1;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_IMPORT);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGMYF));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGLOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));

            return await base.InitializeAsync(core);
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_IMPORT;
            }
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.LiveAPI;
            }
        }

        protected override void ImportMethod()
        {
            bool cancelled = false;
            using (Utils.ProgressBlock blockprogress = new Utils.ProgressBlock(this, STR_IMPORTINGMYF, STR_IMPORTINGMYF, 1, 0))
            {
                try
                {
                    var logs = new List<Utils.API.LiveV6.GeocacheLog>();

                    using (var api = new Utils.API.GeocachingLiveV6(Core))
                    {
                        using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_IMPORTINGMYF, STR_IMPORTINGLOGS, 100, 0, true))
                        {
                            var req = new Utils.API.LiveV6.GetUsersGeocacheLogsRequest();
                            req.AccessToken = api.Token;
                            req.ExcludeArchived = false;
                            req.MaxPerPage = 30;
                            req.StartIndex = 0;
                            req.LogTypes = (from a in Core.LogTypes where a.AsFound select (long)a.ID).ToArray();
                            var resp = api.Client.GetUsersGeocacheLogs(req);
                            while (resp.Status.StatusCode == 0)
                            {
                                //logs.AddRange(resp.Logs);
                                //if (resp.Logs.Count() >= req.MaxPerPage)
                                if (resp.Logs.Count() > 0)
                                {
                                    logs.AddRange(resp.Logs);
                                    req.StartIndex = logs.Count;
                                    if (!progress.UpdateProgress(STR_IMPORTINGMYF, STR_IMPORTINGLOGS, logs.Count + req.MaxPerPage, logs.Count))
                                    {
                                        cancelled = true;
                                        break;
                                    }
                                    Thread.Sleep(4000);
                                    resp = api.Client.GetUsersGeocacheLogs(req);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if (resp.Status.StatusCode != 0)
                            {
                                _errormessage = resp.Status.StatusMessage;
                            }
                        }

                        //ok, we have the logs
                        //get the geocaches
                        if (!cancelled && string.IsNullOrEmpty(_errormessage))
                        {
                            //we download the geocaches that are not present. But for the ones we have, we need to add the log and mark it as found
                            List<string> gcList = (from a in logs where Utils.DataAccess.GetGeocache(Core.Geocaches, a.CacheCode) != null select a.CacheCode).ToList();
                            foreach (string s in gcList)
                            {
                                Utils.DataAccess.GetGeocache(Core.Geocaches, s).Found = true;
                                var ls = (from a in logs where a.CacheCode == s && !a.IsArchived select a).ToList();
                                foreach (var l in ls)
                                {
                                    AddLog(Utils.API.Convert.Log(Core, l));
                                }
                            }

                            gcList = (from a in logs where Utils.DataAccess.GetGeocache(Core.Geocaches, a.CacheCode) == null && !a.IsArchived select a.CacheCode).ToList();
                            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_IMPORTINGMYF, STR_IMPORTINGCACHES, gcList.Count, 0, true))
                            {
                                int index = 0;
                                int gcupdatecount = 20;

                                while (gcList.Count > 0)
                                {
                                    Utils.API.LiveV6.SearchForGeocachesRequest req = new Utils.API.LiveV6.SearchForGeocachesRequest();
                                    req.IsLite = Core.GeocachingComAccount.MemberTypeId == 1;
                                    req.AccessToken = api.Token;
                                    req.CacheCode = new Utils.API.LiveV6.CacheCodeFilter();
                                    req.CacheCode.CacheCodes = (from a in gcList select a).Take(gcupdatecount).ToArray();
                                    req.MaxPerPage = gcupdatecount;
                                    req.GeocacheLogCount = 0;
                                    index += req.CacheCode.CacheCodes.Length;
                                    gcList.RemoveRange(0, req.CacheCode.CacheCodes.Length);
                                    var resp = api.Client.SearchForGeocaches(req);
                                    if (resp.Status.StatusCode == 0 && resp.Geocaches != null)
                                    {
                                        if (resp.CacheLimits != null)
                                        {
                                            _apiLimit = resp.CacheLimits.MaxCacheCount;
                                            _apiLeft = resp.CacheLimits.CachesLeft;
                                        }
                                        Utils.API.Import.AddGeocaches(Core, resp.Geocaches);

                                        foreach (var g in resp.Geocaches)
                                        {
                                            var ls = (from a in logs where a.CacheCode == g.Code && !a.IsArchived select a).ToList();
                                            foreach (var l in ls)
                                            {
                                                AddLog(Utils.API.Convert.Log(Core, l));
                                            }
                                        }

                                        if (!progress.UpdateProgress(STR_IMPORTINGMYF, STR_IMPORTINGCACHES, logs.Count, logs.Count - gcList.Count))
                                        {
                                            cancelled = true;
                                            break;
                                        }

                                        if (gcList.Count > 0)
                                        {
                                            Thread.Sleep(3000);
                                        }
                                    }
                                    else
                                    {
                                        _errormessage = resp.Status.StatusMessage;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _errormessage = e.Message;
                }
            }
        }

        public async override Task<bool> ActionAsync(string action)
        {
            bool result = base.Action(action);
            if (result && action == ACTION_IMPORT)
            {
                if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, false))
                {
                    _errormessage = null;
                    _apiLimit = -1;
                    await PerformImport();
                    if (!string.IsNullOrEmpty(_errormessage))
                    {
                        System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                    if (_apiLimit >= 0)
                    {
                        Utils.Dialogs.LiveAPICachesLeftForm.ShowMessage(_apiLimit, _apiLeft);
                    }
                }
            }
            return result;
        }

    }
}
