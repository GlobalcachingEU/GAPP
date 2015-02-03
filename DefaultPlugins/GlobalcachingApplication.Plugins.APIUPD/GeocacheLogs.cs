using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.APIUPD
{
    public class GeocacheLogs : Utils.BasePlugin.BaseImportFilter
    {
        public const string STR_NOGEOCACHESELECTED = "No geocache selected for update";
        public const string STR_ERROR = "Error";
        public const string STR_UNABLELIVE = "Unable to access the Live API or process its data";
        public const string STR_UPDATINGGEOCACHES = "Updating geocaches...";
        public const string STR_UPDATINGGEOCACHE = "Updating geocache...";

        public const string ACTION_UPDATE_ALL = "Update geocache logs|All";
        public const string ACTION_UPDATE_SELECTED = "Update geocache logs|Selected";
        public const string ACTION_UPDATE_ACTIVE = "Update geocache logs|Active";

        private List<Framework.Data.Geocache> _gcList = null;
        private string _errormessage = null;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            if (PluginSettings.Instance == null)
            {
                var p = new PluginSettings(core);
            }

            AddAction(ACTION_UPDATE_ALL);
            AddAction(ACTION_UPDATE_SELECTED);
            AddAction(ACTION_UPDATE_ACTIVE);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_UNABLELIVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_UPDATINGGEOCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_UPDATINGGEOCACHE));

            return await base.InitializeAsync(core);
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
            try
            {
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_UPDATINGGEOCACHES, STR_UPDATINGGEOCACHE, _gcList.Count, 0, true))
                {
                    int totalcount = _gcList.Count;
                    using (Utils.API.GeocachingLiveV6 client = new Utils.API.GeocachingLiveV6(Core, string.IsNullOrEmpty(Core.GeocachingComAccount.APIToken)))
                    {
                        int index = 0;
                        TimeSpan interval = new TimeSpan(0, 0, 0, 2, 1);
                        DateTime prevCall = DateTime.MinValue;
                        bool cancel = false;

                        while (_gcList.Count > 0 && !cancel)
                        {
                            int logCount = 0;
                            int maxPerPage = 30;
                            bool done = false;

                            if (PluginSettings.Instance.UpdateLogsMaxLogCount > 0 && PluginSettings.Instance.UpdateLogsMaxLogCount < 30)
                            {
                                maxPerPage = PluginSettings.Instance.UpdateLogsMaxLogCount;
                            }
                            List<string> ids = new List<string>();
                            
                            TimeSpan ts = DateTime.Now - prevCall;
                            if (ts < interval)
                            {
                                Thread.Sleep(interval - ts);
                            }
                            prevCall = DateTime.Now;
                            Thread.Sleep(PluginSettings.Instance.AdditionalDelayBetweenLogImport);
                            var resp = client.Client.GetGeocacheLogsByCacheCode(client.Token, _gcList[0].Code, logCount, maxPerPage);
                            while (resp.Status.StatusCode == 0 && resp.Logs != null && resp.Logs.Count() > 0 && !done)
                            {
                                foreach (var lg in resp.Logs)
                                {
                                    if (!lg.IsArchived)
                                    {
                                        Framework.Data.Log gcLog = Utils.API.Convert.Log(Core, lg);
                                        AddLog(gcLog);
                                        if (PluginSettings.Instance.UpdateLogsMaxLogCount == 0)
                                        {
                                            ids.Add(gcLog.ID);
                                        }
                                    }
                                }

                                logCount += resp.Logs.Count();
                                if (PluginSettings.Instance.UpdateLogsMaxLogCount > 0)
                                {
                                    int left = PluginSettings.Instance.UpdateLogsMaxLogCount - logCount;
                                    if (left < maxPerPage)
                                    {
                                        maxPerPage = left;
                                    }
                                }
                                if (maxPerPage > 0)
                                {
                                    ts = DateTime.Now - prevCall;
                                    if (ts < interval)
                                    {
                                        Thread.Sleep(interval - ts);
                                    }
                                    prevCall = DateTime.Now;
                                    Thread.Sleep(PluginSettings.Instance.AdditionalDelayBetweenLogImport);
                                    resp = client.Client.GetGeocacheLogsByCacheCode(client.Token, _gcList[0].Code, logCount, maxPerPage);
                                }
                                else
                                {
                                    done = true;
                                }
                            }
                            if (resp.Status.StatusCode != 0)
                            {
                                _errormessage = resp.Status.StatusMessage;
                                break;
                            }
                            else
                            {
                                if (PluginSettings.Instance.DeselectGeocacheAfterUpdate)
                                {
                                    _gcList[0].Selected = false;
                                }
                                if (PluginSettings.Instance.UpdateLogsMaxLogCount == 0)
                                {
                                    List<Framework.Data.Log> allLogs = Utils.DataAccess.GetLogs(Core.Logs, _gcList[0].Code);
                                    foreach (Framework.Data.Log gim in allLogs)
                                    {
                                        if (!ids.Contains(gim.ID))
                                        {
                                            Core.Logs.Remove(gim);
                                        }
                                    }
                                }
                            }

                            index++;
                            if (!progress.UpdateProgress(STR_UPDATINGGEOCACHES, STR_UPDATINGGEOCACHE, totalcount, index))
                            {
                                break;
                            }
                            _gcList.RemoveAt(0);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _errormessage = e.Message;
            }
        }

        public async override Task<bool> ActionAsync(string action)
        {
            bool result = base.Action(action);
            if (result &&
                (
                 action == ACTION_UPDATE_ALL ||
                 action == ACTION_UPDATE_SELECTED ||
                 action == ACTION_UPDATE_ACTIVE
                ))
            {
                try
                {
                    //get from goundspeak
                    if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, false))
                    {
                        _gcList = null;
                        if (action == ACTION_UPDATE_ALL)
                        {
                            _gcList = (from Framework.Data.Geocache a in Core.Geocaches select a).ToList();
                        }
                        else if (action == ACTION_UPDATE_SELECTED)
                        {
                            _gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                        }
                        else if (action == ACTION_UPDATE_ACTIVE && Core.ActiveGeocache != null)
                        {
                            _gcList = new List<Framework.Data.Geocache>();
                            _gcList.Add(Core.ActiveGeocache);
                        }
                        if (_gcList != null && _gcList.Count > 0)
                        {
                            _errormessage = null;
                            await PerformImport();
                            if (!string.IsNullOrEmpty(_errormessage))
                            {
                                System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        }
                    }
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_UNABLELIVE), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
            return result;
        }
    }
}
