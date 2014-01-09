using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.PurgeLogs
{
    public class Purger
    {
        public async Task PurgeWithDefaultSettings(Core.Storage.Database db)
        {
            using (Utils.DataUpdater upd = new Utils.DataUpdater(db))
            {
                await Task.Run(()=>{
                    try
                    {
                        List<string> removeFromUsers = new List<string>();
                        List<string> keepFromUsers = new List<string>();

                        if (!string.IsNullOrEmpty(Core.Settings.Default.PurgeLogsKeepLogsOfUsers))
                        {
                            string[] usrs = Core.Settings.Default.PurgeLogsKeepLogsOfUsers.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string s in usrs)
                            {
                                keepFromUsers.Add(s);
                            }
                        }
                        if (!string.IsNullOrEmpty(Core.Settings.Default.PurgeLogsRemoveAllLogsOfUsers))
                        {
                            string[] usrs = Core.Settings.Default.PurgeLogsRemoveAllLogsOfUsers.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string s in usrs)
                            {
                                removeFromUsers.Add(s);
                            }
                        }

                        DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                        using (Utils.ProgressBlock prog = new Utils.ProgressBlock("PurgeLogs", "PurgeLogs", db.GeocacheCollection.Count, 0, true))
                        {
                            int index = 0;
                            DateTime n = DateTime.Now.Date;
                            foreach (var gc in db.GeocacheCollection)
                            {
                                bool ignoreMe = false;
                                string me = null;
                                if (Core.Settings.Default.PurgeLogsKeepOwnLogs)
                                {
                                    me = Core.ApplicationData.Instance.AccountInfos.GetAccountInfo(gc.Code.Substring(0, 2)).AccountName;
                                    if (!string.IsNullOrEmpty(me))
                                    {
                                        me = me.ToLower();
                                        ignoreMe = true;
                                    }
                                }
                                if (!Core.Settings.Default.PurgeLogsKeepOfOwnedCaches || !gc.IsOwn)
                                {
                                    List<Core.Data.Log> lgs = Utils.DataAccess.GetLogs(db, gc.Code);
                                    List<Core.Data.Log> dls = (from a in lgs where
                                                                   ((removeFromUsers.Contains(a.Finder.ToLower()) && !keepFromUsers.Contains(a.Finder.ToLower())) ||
                                                                   (n - a.Date.Date).TotalDays > Core.Settings.Default.PurgeLogsOlderThanDays) &&
                                                                   (!ignoreMe || a.Finder.ToLower()!=me)
                                                               select a).Skip(Core.Settings.Default.PurgeLogsKeepLogCount).ToList();


                                    foreach (var l in dls)
                                    {
                                        Utils.DataAccess.DeleteLog(db, l);
                                    }
                                }

                                index++;
                                if (DateTime.Now >= nextUpdate)
                                {
                                    if (!prog.Update("PurgeLogs", db.GeocacheCollection.Count, index))
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
            }
        }
    }
}
