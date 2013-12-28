using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.MyFinds
{
    public class ImportLiveAPI
    {
        private bool _cancelled = false;

        public async Task ImportMyFindsAsync(Core.Storage.Database db)
        {
            using (Utils.DataUpdater upd = new Utils.DataUpdater(db))
            {
                await Task.Run(() =>
                {
                    try
                    {
                        List<LiveAPI.LiveV6.GeocacheLog> logs = GetLogsOfUser(Core.ApplicationData.Instance.AccountInfos.GetAccountInfo("GC").AccountName, (from a in Core.ApplicationData.Instance.LogTypes where a.AsFound select a).ToList());
                        if (!_cancelled)
                        {
                            List<string> gcCodes = new List<string>();
                            foreach (var l in logs)
                            {
                                if (!l.IsArchived)
                                {
                                    LiveAPI.Import.ImportLog(db, l);
                                    var gc = db.GeocacheCollection.GetGeocache(l.CacheCode);
                                    if (gc == null)
                                    {
                                        gcCodes.Add(l.CacheCode);
                                    }
                                    else
                                    {
                                        gc.Found = true;
                                    }
                                }
                            }
                            if (gcCodes.Count > 0)
                            {
                                LiveAPI.Import.ImportGeocaches(db, gcCodes);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Core.ApplicationData.Instance.Logger.AddLog(this, e);
                    }
                });
            }
        }

        public async Task<List<LiveAPI.LiveV6.GeocacheLog>> GetLogsOfUserAsync(string userName, List<Core.Data.LogType> logTypes)
        {
            List<LiveAPI.LiveV6.GeocacheLog> result = null; 
            await Task.Run(() =>
            {
                try
                {
                    result = GetLogsOfUser(userName, logTypes);
                }
                catch (Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
            });
            return result;
        }

        public List<LiveAPI.LiveV6.GeocacheLog> GetLogsOfUser(string userName, List<Core.Data.LogType> logTypes)
        {
            List<LiveAPI.LiveV6.GeocacheLog> result = new List<LiveAPI.LiveV6.GeocacheLog>();
            using (var api = new LiveAPI.GeocachingLiveV6())
            {
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock("ImportingLogs", "ImportingLogs", 100, 0, true))
                {
                    var req = new LiveAPI.LiveV6.GetUsersGeocacheLogsRequest();
                    req.AccessToken = api.Token;
                    req.ExcludeArchived = false;
                    req.MaxPerPage = Core.Settings.Default.LiveAPIGetUsersGeocacheLogsBatchSize;
                    req.StartIndex = 0;
                    req.LogTypes = (from a in logTypes select (long)a.ID).ToArray();
                    var resp = api.Client.GetUsersGeocacheLogs(req);
                    while (resp.Status.StatusCode == 0)
                    {
                        //logs.AddRange(resp.Logs);
                        //if (resp.Logs.Count() >= req.MaxPerPage)
                        if (resp.Logs.Count() > 0)
                        {
                            result.AddRange(resp.Logs);
                            req.StartIndex = result.Count;
                            if (!progress.Update("ImportingLogs", result.Count + req.MaxPerPage, result.Count))
                            {
                                _cancelled = true;
                                break;
                            }
                            System.Threading.Thread.Sleep(Core.Settings.Default.LiveAPIDelayGetUsersGeocacheLogs);
                            resp = api.Client.GetUsersGeocacheLogs(req);
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (resp.Status.StatusCode != 0)
                    {
                        Core.ApplicationData.Instance.Logger.AddLog(this, Core.Logger.Level.Error, resp.Status.StatusMessage);
                        _cancelled = true;
                    }
                }
            }
            return result;
        }
    }
}
