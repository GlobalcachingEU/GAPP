using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.APIFindsOfUser
{
    public class Import: Utils.BasePlugin.BaseImportFilter
    {
        public const string STR_IMPORTINGMYF = "Importing geocaching.com logs...";
        public const string STR_IMPORTINGLOGS = "Importing geocaching.com logs...";
        public const string STR_IMPORTINGCACHES = "Importing geocaching.com geocaches...";
        public const string STR_ERROR = "Error";

        public const string ACTION_IMPORT = "Import logs of other users";

        private List<string> _users = null;
        private List<long> _logTypes = null;
        private DateTime _fromDate = DateTime.Now;
        private DateTime _toDate = DateTime.Now;
        private string _errormessage = null;
        private int _apiLimit = -1;
        private int _apiLeft = -1;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            var p = new PluginSettings(core);

            AddAction(ACTION_IMPORT);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGMYF));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGLOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_COPYTOCLIPBOARD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_ADD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_GETLOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_IMPORTALL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_IMPORTMISSING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_IMPORTSELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_REMOVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_REMOVEANDLOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_USERNAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_USERS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_BETWEENDATES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_LOGTYPES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_SELECTALL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_DESELECTALL));

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

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_IMPORT)
                {
                    using (ImportForm dlg = new ImportForm(Core))
                    {
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            _users = dlg.SelectedUsers;
                            _logTypes = dlg.SelectedLogTypes;
                            _fromDate = dlg.DateFrom;
                            _toDate = dlg.DateTo;
                            if (_users != null && _users.Count > 0)
                            {
                                _errormessage = null;
                                _apiLimit = -1;
                                PerformImport();
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
                    }
                }
            }
            return result;
        }

        protected override void ImportMethod()
        {
            bool cancelled = false;
            using (Utils.ProgressBlock blockprogress = new Utils.ProgressBlock(this, STR_IMPORTINGMYF, STR_IMPORTINGMYF, _users.Count, 0))
            {
                try
                {
                    using (var api = new Utils.API.GeocachingLiveV6(Core))
                    {
                        foreach (string usr in _users)
                        {
                            var logs = new List<Utils.API.LiveV6.GeocacheLog>();
                            int page = 0;
                            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_IMPORTINGMYF, STR_IMPORTINGLOGS, 100, 0, true))
                            {
                                var req = new Utils.API.LiveV6.GetUsersGeocacheLogsRequest();
                                req.AccessToken = api.Token;
                                req.ExcludeArchived = true;
                                req.Username = usr;
                                req.MaxPerPage = 100;
                                req.StartIndex = 0;
                                if (PluginSettings.Instance.BetweenDates)
                                {
                                    req.Range = new Utils.API.LiveV6.DateRange();
                                    req.Range.StartDate = _fromDate < _toDate? _fromDate:_toDate;
                                    req.Range.EndDate = _toDate > _fromDate ? _toDate : _fromDate;
                                }
                                req.LogTypes = _logTypes.ToArray();
                                //req.LogTypes = new long[] { 2 };
                                var resp = api.Client.GetUsersGeocacheLogs(req);
                                while (resp.Status.StatusCode == 0)
                                {
                                    logs.AddRange(resp.Logs);

                                    //if (resp.Logs.Count() >= req.MaxPerPage)
                                    if (resp.Logs.Count() > 0)
                                    {
                                        page++;
                                        //req.StartIndex = logs.Count;
                                        req.StartIndex = page * req.MaxPerPage;
                                        if (!progress.UpdateProgress(STR_IMPORTINGMYF, STR_IMPORTINGLOGS, logs.Count + req.MaxPerPage, logs.Count))
                                        {
                                            cancelled = true;
                                            break;
                                        }
                                        System.Threading.Thread.Sleep(2100);
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

                            if (!cancelled)
                            {
                                foreach (var l in logs)
                                {
                                    AddLog(Utils.API.Convert.Log(Core, l));
                                }
                            }

                            //ok, we have the logs
                            //get the geocaches
                            if (PluginSettings.Instance.ImportMissingCaches && !cancelled && string.IsNullOrEmpty(_errormessage))
                            {
                                List<string> gcList = (from a in logs where a.CacheCode!=null && Utils.DataAccess.GetGeocache(Core.Geocaches, a.CacheCode) == null select a.CacheCode).ToList();
                                int maxToGet = gcList.Count;
                                using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_IMPORTINGMYF, STR_IMPORTINGCACHES, maxToGet, 0, true))
                                {
                                    int index = 0;
                                    int gcupdatecount;
                                    TimeSpan interval = new TimeSpan(0, 0, 0, 2, 100);
                                    DateTime prevCall = DateTime.MinValue;
                                    bool dodelay;

                                    gcupdatecount = 50;
                                    dodelay = (gcList.Count > 30);

                                    while (gcList.Count > 0)
                                    {
                                        if (dodelay)
                                        {
                                            TimeSpan ts = DateTime.Now - prevCall;
                                            if (ts < interval)
                                            {
                                                System.Threading.Thread.Sleep(interval - ts);
                                            }
                                        }

                                        Utils.API.LiveV6.SearchForGeocachesRequest req = new Utils.API.LiveV6.SearchForGeocachesRequest();
                                        req.IsLite = Core.GeocachingComAccount.MemberTypeId == 1;
                                        req.AccessToken = api.Token;
                                        req.CacheCode = new Utils.API.LiveV6.CacheCodeFilter();
                                        req.CacheCode.CacheCodes = (from a in gcList select a).Take(gcupdatecount).ToArray();
                                        req.MaxPerPage = gcupdatecount;
                                        req.GeocacheLogCount = 0;
                                        index += req.CacheCode.CacheCodes.Length;
                                        gcList.RemoveRange(0, req.CacheCode.CacheCodes.Length);
                                        prevCall = DateTime.Now;
                                        var resp = api.Client.SearchForGeocaches(req);
                                        if (resp.Status.StatusCode == 0 && resp.Geocaches != null)
                                        {
                                            if (resp.CacheLimits != null)
                                            {
                                                _apiLimit = resp.CacheLimits.MaxCacheCount;
                                                _apiLeft = resp.CacheLimits.CachesLeft;
                                            }
                                            Utils.API.Import.AddGeocaches(Core, resp.Geocaches);

                                            if (!progress.UpdateProgress(STR_IMPORTINGMYF, STR_IMPORTINGCACHES, maxToGet, maxToGet - gcList.Count))
                                            {
                                                cancelled = true;
                                                break;
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
                }
                catch (Exception e)
                {
                    _errormessage = e.Message;
                }
            }
        }
    }
}
