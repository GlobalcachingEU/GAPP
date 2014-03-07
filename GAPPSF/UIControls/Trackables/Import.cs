using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.UIControls.Trackables
{
    public class Import
    {
        public async Task AddOwnTrackablesAsync(TrackableGroup grp)
        {
            await Task.Run(() =>
            {
                try
                {
                    AddOwnTrackables(grp);
                }
                catch (Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
            });
        }

        public async Task AddUpdateTrackablesAsync(TrackableGroup grp, List<string> trkList)
        {
            await Task.Run(() =>
            {
                try
                {
                    AddUpdateTrackables(grp, trkList);
                }
                catch(Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
            });
        }

        public void AddOwnTrackables(TrackableGroup grp)
        {
            bool canceled = false;
            using (Utils.ProgressBlock progr = new Utils.ProgressBlock("GetTrackableData", "GetTrackableData", 1, 0, true))
            using (var api = new LiveAPI.GeocachingLiveV6())
            {
                List<string> trkCodes = new List<string>();

                var req = new LiveAPI.LiveV6.GetTrackablesByOwnerRequest();
                req.AccessToken = api.Token;
                req.TrackableLogsCount = 0;
                req.StartIndex = 0;
                req.MaxPerPage = Core.Settings.Default.LiveAPIGetOwnedTrackablesBatchSize;
                int total = 0;
                while (true)
                {
                    var resp = api.Client.GetOwnedTrackables(req);
                    if (resp.Status.StatusCode == 0)
                    {
                        if (resp.Trackables != null)
                        {
                            foreach (var t in resp.Trackables)
                            {
                                trkCodes.Add(t.Code);
                                Core.Settings.Default.AddUpdateTrackable(grp, GetTrackableItemFromLiveAPI(t));
                                total++;
                            }
                            if (!progr.Update("GetTrackableData", total, 2 * total))
                            {
                                canceled = true;
                                break;
                            }
                        }
                        if (resp.Trackables.Count() < req.MaxPerPage)
                        {
                            break;
                        }
                        else
                        {
                            req.StartIndex = total;
                            System.Threading.Thread.Sleep(Core.Settings.Default.LiveAPIDelayGetOwnedTrackables);
                        }
                    }
                    else
                    {
                        Core.ApplicationData.Instance.Logger.AddLog(this, Core.Logger.Level.Error, resp.Status.StatusMessage);
                        break;
                    }
                }

                if (!canceled)
                {
                    progr.Update("GetTrackableData", total, total);
                    AddUpdateTrackables(grp, trkCodes);
                }
            }
        }

        public void AddUpdateTrackables(TrackableGroup grp, List<string> trkList)
        {
            using (Utils.ProgressBlock progr = new Utils.ProgressBlock("GetTrackableData", "GetTrackableData", trkList.Count, 0, true))
            using (var api = new LiveAPI.GeocachingLiveV6())
            {
                int index = 0;
                while (index < trkList.Count && AddUpdateTrackable(api, grp, trkList[index]))
                {
                    index++;
                    if (progr.Update("GetTrackableData", trkList.Count, index))
                    {
                        break;
                    }
                    else if (index < trkList.Count)
                    {
                        System.Threading.Thread.Sleep(Core.Settings.Default.LiveAPIDelayGetTrackablesByTBCode);
                    }
                }
            }
        }

        private bool AddUpdateTrackable(LiveAPI.GeocachingLiveV6 api, TrackableGroup grp, string trkCode)
        {
            bool result = false;
            if (trkCode.ToUpper().StartsWith("TB"))
            {
                try
                {
                    var resp = api.Client.GetTrackablesByTBCode(api.Token, trkCode.ToUpper(), 0);
                    if (resp.Status.StatusCode == 0)
                    {
                        if (resp.Trackables != null)
                        {
                            foreach (var t in resp.Trackables)
                            {
                                TrackableItem trk = GetTrackableItemFromLiveAPI(t);
                                Core.Settings.Default.AddUpdateTrackable(grp, trk);

                                var resp2 = api.Client.GetTrackableTravelList(api.Token, trk.Code);
                                if (resp2.Status.StatusCode == 0)
                                {
                                    if (resp2.TrackableTravels != null)
                                    {
                                        List<TravelItem> travelList = new List<TravelItem>();
                                        foreach (var tt in resp2.TrackableTravels)
                                        {
                                            if (tt.Latitude != null && tt.Longitude != null)
                                            {
                                                TravelItem ti = new TravelItem();
                                                ti.TrackableCode = trk.Code;
                                                if (tt.CacheID != null)
                                                {
                                                    ti.GeocacheCode = Utils.Conversion.GetCacheCodeFromCacheID((int)tt.CacheID);
                                                }
                                                else
                                                {
                                                    ti.GeocacheCode = "";
                                                }
                                                ti.DateLogged = tt.DateLogged;
                                                ti.Lat = (double)tt.Latitude;
                                                ti.Lon = (double)tt.Longitude;
                                                travelList.Add(ti);
                                            }
                                        }
                                        Core.Settings.Default.UpdateTrackableTravels(trk, travelList);
                                    }

                                    //get all logs
                                    List<LogItem> logs = new List<LogItem>();
                                    int maxPageSize = Core.Settings.Default.LiveAPIGetTrackableLogsByTBCodeBatchSize;
                                    while (true)
                                    {
                                        var resp3 = api.Client.GetTrackableLogsByTBCode(api.Token, trk.Code, logs.Count, maxPageSize);
                                        if (resp3.Status.StatusCode == 0)
                                        {
                                            if (resp3.TrackableLogs != null)
                                            {
                                                foreach (var tl in resp3.TrackableLogs)
                                                {
                                                    LogItem li = new LogItem();
                                                    li.TrackableCode = trk.Code;
                                                    if (tl.CacheID != null)
                                                    {
                                                        li.GeocacheCode = Utils.Conversion.GetCacheCodeFromCacheID((int)tl.CacheID);
                                                    }
                                                    else
                                                    {
                                                        li.GeocacheCode = "";
                                                    }
                                                    li.LogCode = tl.Code;
                                                    li.ID = tl.ID;
                                                    li.IsArchived = tl.IsArchived;
                                                    li.LoggedBy = tl.LoggedBy == null ? "" : tl.LoggedBy.UserName;
                                                    li.LogGuid = tl.LogGuid.ToString();
                                                    li.LogIsEncoded = tl.LogIsEncoded;
                                                    li.LogText = tl.LogText;
                                                    li.WptLogTypeId = tl.LogType == null ? -1 : (int)tl.LogType.WptLogTypeId;
                                                    li.Url = tl.Url;
                                                    li.UTCCreateDate = tl.UTCCreateDate;
                                                    li.VisitDate = tl.VisitDate;
                                                    logs.Add(li);
                                                }
                                                if (resp3.TrackableLogs.Count() < maxPageSize)
                                                {
                                                    break;
                                                }
                                                System.Threading.Thread.Sleep(Core.Settings.Default.LiveAPIDelayGetTrackableLogsByTBCode);
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            logs = null;
                                            Core.ApplicationData.Instance.Logger.AddLog(this, Core.Logger.Level.Error, resp3.Status.StatusMessage);
                                            break;
                                        }
                                    }
                                    if (logs!=null)
                                    {
                                        Core.Settings.Default.UpdateTrackableLogs(trk, logs);
                                    }
                                }
                                else
                                {
                                    Core.ApplicationData.Instance.Logger.AddLog(this, Core.Logger.Level.Error, resp2.Status.StatusMessage);
                                }
                            }
                        }
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
            }
            return result;
        }

        private TrackableItem GetTrackableItemFromLiveAPI(LiveAPI.LiveV6.Trackable t)
        {
            TrackableItem trk = new TrackableItem();
            trk.Code = t.Code;
            trk.AllowedToBeCollected = t.AllowedToBeCollected;
            trk.Archived = t.Archived;
            trk.BugTypeID = t.BugTypeID;
            trk.CurrentGeocacheCode = t.CurrentGeocacheCode;
            trk.CurrentGoal = t.CurrentGoal;
            trk.DateCreated = t.DateCreated;
            trk.Description = t.Description;
            trk.IconUrl = t.IconUrl;
            trk.IconData = Core.Settings.Default.GetTrackableIconData(trk.IconUrl);
            if (trk.IconData==null && !string.IsNullOrEmpty(t.IconUrl))
            {
                try
                {
                    using (System.Net.WebClient wc = new System.Net.WebClient())
                    {
                        trk.IconData = wc.DownloadData(t.IconUrl);
                    }
                }
                catch(Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
            }
            trk.Id = t.Id;
            trk.InCollection = t.InCollection;
            trk.Name = t.Name;
            trk.TBTypeName = t.TBTypeName;
            trk.Url = t.Url;
            trk.WptTypeID = t.WptTypeID;
            if (t.OriginalOwner != null)
            {
                trk.Owner = t.OriginalOwner.UserName;
            }
            else
            {
                trk.Owner = "";
            }
            return trk;
        }

    }
}
