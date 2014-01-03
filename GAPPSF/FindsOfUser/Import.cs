using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.FindsOfUser
{
    public class Import
    {
        public async Task ImportLogsOfUsers(Core.Storage.Database db, 
            List<string> usrs,
            bool betweenDates,
            DateTime minDate,
            DateTime maxDate,
            List<Core.Data.LogType> logTypes,
            bool importMissingGeocaches)
        {
            using (Utils.DataUpdater upd = new Utils.DataUpdater(db))
            {
                await Task.Run(() =>
                {
                    try
                    {
                        bool cancelled = false;
                        using (var api = new LiveAPI.GeocachingLiveV6())
                        {
                            using (Utils.ProgressBlock prog = new Utils.ProgressBlock("ImportingLogs", usrs[0], usrs.Count, 0, true))
                            {
                                for (int i = 0; !cancelled && (i < usrs.Count); i++)
                                {
                                    prog.Update(usrs[i], usrs.Count, i);

                                    List<LiveAPI.LiveV6.GeocacheLog> logs = new List<LiveAPI.LiveV6.GeocacheLog>();
                                    using (Utils.ProgressBlock progress = new Utils.ProgressBlock("ImportingLogs", 100, 0))
                                    {
                                        var req = new LiveAPI.LiveV6.GetUsersGeocacheLogsRequest();
                                        req.Username = usrs[i];
                                        req.AccessToken = api.Token;
                                        req.ExcludeArchived = false;
                                        req.MaxPerPage = Core.Settings.Default.LiveAPIGetUsersGeocacheLogsBatchSize;
                                        req.StartIndex = 0;
                                        req.LogTypes = (from a in logTypes select (long)a.ID).ToArray();
                                        if (betweenDates)
                                        {
                                            req.Range = new LiveAPI.LiveV6.DateRange();
                                            req.Range.StartDate = minDate < maxDate ? minDate : maxDate;
                                            req.Range.EndDate = maxDate > minDate ? maxDate : minDate;
                                        }
                                        var resp = api.Client.GetUsersGeocacheLogs(req);
                                        while (resp.Status.StatusCode == 0)
                                        {
                                            //logs.AddRange(resp.Logs);
                                            //if (resp.Logs.Count() >= req.MaxPerPage)
                                            if (resp.Logs.Count() > 0)
                                            {
                                                logs.AddRange(resp.Logs);
                                                req.StartIndex = logs.Count;
                                                if (!progress.Update("ImportingLogs", logs.Count + req.MaxPerPage, logs.Count))
                                                {
                                                    cancelled = true;
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
                                            cancelled = true;
                                        }
                                    }
                                    if (!cancelled && importMissingGeocaches && logs.Count > 0)
                                    {
                                        List<string> gcList = new List<string>();
                                        foreach (var l in logs)
                                        {
                                            if (!gcList.Contains(l.CacheCode) && db.GeocacheCollection.GetGeocache(l.CacheCode) == null)
                                            {
                                                gcList.Add(l.CacheCode);
                                            }
                                        }
                                        LiveAPI.Import.ImportGeocaches(db, gcList);
                                    }

                                    cancelled = !prog.Update(usrs[i], usrs.Count, i);
                                }
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        Core.ApplicationData.Instance.Logger.AddLog(this, e);
                    }
                });
            }
        }

        public async Task<List<Core.Data.Log>> GetLogsOfUser(Core.Storage.Database db, string usrName)
        {
            List<Core.Data.Log> result = new List<Core.Data.Log>();
            await Task.Run(() =>
            {
                try
                {
                    using (Utils.ProgressBlock prog = new Utils.ProgressBlock("SearchingForLogs", "SearchingForLogs", db.LogCollection.Count,0, true))
                    {
                        int index = 0;
                        DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                        foreach(var l in db.LogCollection)
                        {
                            string f = l.Finder;
                            if (string.Compare(f,usrName,true)==0)
                            {
                                result.Add(l);
                            }
                            index++;
                            if (DateTime.Now>=nextUpdate)
                            {
                                if (!prog.Update("SearchingForLogs", db.LogCollection.Count, index))
                                {
                                    break;
                                }
                                nextUpdate = DateTime.Now.AddSeconds(1);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
            });
            return result;
        }
    }
}
