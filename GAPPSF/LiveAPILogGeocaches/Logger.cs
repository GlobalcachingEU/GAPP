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
                    bool error = false;

                    if (Core.ApplicationData.Instance.ActiveDatabase!=null)
                    {
                        var gc = Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection.GetGeocache(logInfo.GeocacheCode);
                        if (gc != null)
                        {
                            if (logInfo.LogType.AsFound)
                            {
                                gc.Found = true;
                            }
                        }
                        LiveAPI.Import.ImportLog(Core.ApplicationData.Instance.ActiveDatabase, resp.Log);
                        if (gc!=null)
                        {
                            gc.ResetCachedLogData();
                        }
                    }

                    //log trackables (14=drop off, 13=retrieve from cache, 75=visited)
                    if (dropTbs != null && dropTbs.Count > 0)
                    {
                        var reqT = new LiveAPI.LiveV6.CreateTrackableLogRequest();
                        reqT.AccessToken = api.Token;
                        reqT.LogType = 14;
                        reqT.UTCDateLogged = logInfo.VisitDate.Date.AddHours(12).ToUniversalTime();
                        foreach (var tb in dropTbs)
                        {
                            reqT.TrackingNumber = tb.TrackingCode;
                            reqT.CacheCode = logInfo.GeocacheCode;
                            var resp2 = api.Client.CreateTrackableLog(reqT);
                            if (resp2.Status.StatusCode != 0)
                            {
                                Core.ApplicationData.Instance.Logger.AddLog(this, Core.Logger.Level.Error, resp2.Status.StatusMessage);
                                error = true;
                                //break;
                            }
                        }
                    }

                    if (retrieveTbs != null && retrieveTbs.Count > 0)
                    {
                        var reqT = new LiveAPI.LiveV6.CreateTrackableLogRequest();
                        reqT.AccessToken = api.Token;
                        reqT.LogType = 13;
                        reqT.UTCDateLogged = logInfo.VisitDate.Date.AddHours(12).ToUniversalTime();
                        foreach (var tb in retrieveTbs)
                        {
                            reqT.TrackingNumber = tb;
                            reqT.CacheCode = logInfo.GeocacheCode;
                            var resp2 = api.Client.CreateTrackableLog(reqT);
                            if (resp2.Status.StatusCode != 0)
                            {
                                Core.ApplicationData.Instance.Logger.AddLog(this, Core.Logger.Level.Error, resp2.Status.StatusMessage);
                                error = true;
                                //break;
                            }
                        }
                    }

                    //add log images
                    foreach (var li in logInfo.Images)
                    {
                        var uplReq = new LiveAPI.LiveV6.UploadImageToGeocacheLogRequest();
                        uplReq.AccessToken = api.Token;
                        uplReq.LogGuid = resp.Log.Guid;
                        uplReq.ImageData = new LiveAPI.LiveV6.UploadImageData();
                        uplReq.ImageData.FileCaption = li.Caption;
                        uplReq.ImageData.FileDescription = li.Description;
                        uplReq.ImageData.FileName = li.Uri;
                        //todo: scale image to comply to limits
                        uplReq.ImageData.base64ImageData = System.Convert.ToBase64String(System.IO.File.ReadAllBytes(li.Uri));
                        var resp2 = api.Client.UploadImageToGeocacheLog(uplReq);
                        if (resp2.Status.StatusCode != 0)
                        {
                            Core.ApplicationData.Instance.Logger.AddLog(this, Core.Logger.Level.Error, resp2.Status.StatusMessage);
                            error = true;
                            //break;
                        }
                    }

                    if (logInfo.AddToFavorites)
                    {
                        Favorites.Manager.Instance.AddFavoritedGeocache(logInfo.GeocacheCode);
                    }

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
