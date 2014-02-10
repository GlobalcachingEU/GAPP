using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.OKAPI
{
    public class Import
    {
        public static List<Core.Data.Geocache> AddGeocaches(Core.Storage.Database db, List<OKAPIService.Geocache> gcList)
        {
            List<Core.Data.Geocache> result = new List<Core.Data.Geocache>();
            if (gcList != null)
            {
                foreach (var gcApi in gcList)
                {
                    Core.Data.Geocache activeGC = Convert.AddGeocache(db, gcApi);
                    if (activeGC != null)
                    {
                        if (gcApi.latest_logs != null)
                        {
                            foreach (var l in gcApi.latest_logs)
                            {
                                Convert.AddLog(db, l, "", "");
                            }
                        }
                        if (gcApi.alt_wpts != null)
                        {
                            foreach (var wp in gcApi.alt_wpts)
                            {
                                Convert.AddWaypoint(db, wp);
                            }
                        }
                        if (gcApi.images != null)
                        {
                            foreach (var imgd in gcApi.images)
                            {
                                Convert.AddGeocacheImage(db, imgd, activeGC.Code);
                            }
                        }
                    }
                }
            }
            return result;
        }

        public async Task ImportMyLogsWithCachesAsync(Core.Storage.Database db, SiteInfo activeSite)
        {
            using (Utils.DataUpdater upd = new Utils.DataUpdater(db))
            {
                await Task.Run(new Action(() => ImportMyLogsWithCaches(db, activeSite)));
            }
        }
        public void ImportMyLogsWithCaches(Core.Storage.Database db, SiteInfo activeSite)
        {
            using (Utils.ProgressBlock blockprogress = new Utils.ProgressBlock("ImportingOpencachingLogs", "ImportingOpencachingLogs", 2, 0, true))
            {
                bool cancelled = false;
                try
                {
                    int stepSize = 100;
                    List<OKAPIService.Log> logs = new List<OKAPIService.Log>();

                    using (Utils.ProgressBlock progress = new Utils.ProgressBlock("ImportingOpencachingLogs", "ImportingOpencachingLogs", 100, 0, true))
                    {
                        List<OKAPIService.Log> pageLogs = OKAPIService.GetLogsOfUserID(activeSite, activeSite.UserID, stepSize, 0);

                        while (pageLogs.Count() > 0)
                        {
                            logs.AddRange(pageLogs);
                            if (!progress.Update("ImportingOpencachingLogs", logs.Count + stepSize, logs.Count))
                            {
                                cancelled = true;
                                break;
                            }
                            else if (pageLogs.Count() < stepSize)
                            {
                                break;
                            }
                            pageLogs = OKAPIService.GetLogsOfUserID(activeSite, activeSite.UserID, stepSize, logs.Count);
                        }
                    }

                    if (!cancelled)
                    {
                        blockprogress.Update("ImportingOpencachingLogs", 2, 1);

                        //we download the geocaches that are not present. But for the ones we have, we need to add the log and mark it as found
                        List<string> gcList = (from a in logs where db.GeocacheCollection.GetGeocache(a.cache_code) != null select a.cache_code).ToList();
                        foreach (string s in gcList)
                        {
                            db.GeocacheCollection.GetGeocache(s).Found = true;
                            var ls = (from a in logs where a.cache_code == s select a).ToList();
                            foreach (var l in ls)
                            {
                                Convert.AddLog(db, l, activeSite.Username, activeSite.UserID);
                            }
                        }

                        gcList = (from a in logs where db.GeocacheCollection.GetGeocache(a.cache_code) == null select a.cache_code).ToList();
                        using (Utils.ProgressBlock progress = new Utils.ProgressBlock("ImportingOpencachingLogs", "ImportingOpencachingGeocaches", gcList.Count, 0, true))
                        {
                            int gcupdatecount = 1;

                            while (gcList.Count > 0)
                            {
                                List<string> lcs = gcList.Take(gcupdatecount).ToList();
                                gcList.RemoveRange(0, lcs.Count);
                                List<OKAPIService.Geocache> caches = OKAPIService.GetGeocaches(activeSite, lcs);
                                Import.AddGeocaches(db, caches);

                                foreach (var g in caches)
                                {
                                    var ls = (from a in logs where a.cache_code == g.code select a).ToList();
                                    foreach (var l in ls)
                                    {
                                        Convert.AddLog(db, l, activeSite.Username, activeSite.UserID);
                                    }
                                }

                                if (!progress.Update("ImportingOpencachingGeocaches", logs.Count, logs.Count - gcList.Count))
                                {
                                    cancelled = true;
                                    break;
                                }
                            }

                        }

                    }
                }
                catch (Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
            }
        }
    }
}
