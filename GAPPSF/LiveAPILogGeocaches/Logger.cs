using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.LiveAPILogGeocaches
{
    public class Logger
    {
        public async Task<List<LogInfo>> LogGeocachesAsync(List<LogInfo> logInfos)
        {
            List<LogInfo> result = new List<LogInfo>();
            Utils.DataUpdater upd = null;
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase);
            }
            using (Utils.ProgressBlock prog = new Utils.ProgressBlock("LogGeocache", "Logging", logInfos.Count, 0, true))
            {
                using (var api = new LiveAPI.GeocachingLiveV6())
                {
                    foreach (LogInfo li in logInfos)
                    {
                        int index = 0;
                        List<LiveAPI.LiveV6.Trackable> dropTbs = null;
                        List<string> retrieveTbs = null;

                        //todo: check if trackable dialog is needed
                        //fetch in background

                        bool ok = false;
                        await Task.Run(() =>
                        {
                            if (index > 0 && dropTbs == null && retrieveTbs == null)
                            {
                                System.Threading.Thread.Sleep(Core.Settings.Default.LiveAPIDelayCreateFieldNoteAndPublish);
                            }
                            ok = LogGeocache(api, li, dropTbs, retrieveTbs);
                        });
                        if (ok)
                        {
                            result.Add(li);
                            index++;
                            if (!prog.Update("Logging", logInfos.Count, index))
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            if (upd!=null)
            {
                upd.Dispose();
                upd = null;
            }
            return result;
        }

        public bool LogGeocache(LiveAPI.GeocachingLiveV6 api, LogInfo logInfo, List<LiveAPI.LiveV6.Trackable> dropTbs, List<string> retrieveTbs)
        {
            bool result = false;
            try
            {
                var req = new LiveAPI.LiveV6.CreateFieldNoteAndPublishRequest();
                req.AccessToken = api.Token;
                req.CacheCode = logInfo.GeocacheCode;
                req.EncryptLogText = false;
                req.FavoriteThisCache = logInfo.AddToFavorites;
                req.Note = logInfo.LogText;
                req.PromoteToLog = true;
                req.WptLogTypeId = logInfo.LogType.ID;
                req.UTCDateLogged = logInfo.VisitDate.Date.AddHours(12).ToUniversalTime();
                var resp = api.Client.CreateFieldNoteAndPublish(req);
                if (resp.Status.StatusCode == 0)
                {

                    //todo
                    result = true;
                }
                else
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, Core.Logger.Level.Error, resp.Status.StatusMessage);
                }
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
            return result;
        }
    }
}
