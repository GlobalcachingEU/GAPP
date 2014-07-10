using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GAPPSF.LiveAPI
{
    public class Import
    {

        public static async Task ImportGeocacheLogsAsync(Core.Storage.Database db, List<Core.Data.Geocache> gcList)
        {
            using (Utils.DataUpdater upd = new Utils.DataUpdater(db))
            {
                await Task.Run(() =>
                {
                    ImportGeocacheLogs(db, gcList);
                });
            }
        }


        public static void ImportGeocacheLogs(Core.Storage.Database db, List<Core.Data.Geocache> gcList)
        {
            try
            {
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock("UpdatingGeocaches", "UpdatingGeocache", gcList.Count, 0, true))
                {
                    int totalcount = gcList.Count;
                    using (LiveAPI.GeocachingLiveV6 client = new LiveAPI.GeocachingLiveV6())
                    {
                        int index = 0;

                        while (gcList.Count > 0)
                        {
                            int logCount = 0;
                            int maxPerPage = Core.Settings.Default.LiveAPIGetGeocacheLogsByCacheCodeBatchSize;
                            bool done = false;

                            if (Core.Settings.Default.LiveAPIUpdateGeocacheLogsMax > 0 && Core.Settings.Default.LiveAPIUpdateGeocacheLogsMax < 30)
                            {
                                maxPerPage = Core.Settings.Default.LiveAPIUpdateGeocacheLogsMax;
                            }
                            List<string> ids = new List<string>();

                            if (index > 0)
                            {
                                Thread.Sleep(Core.Settings.Default.LiveAPIDelayGetGeocacheLogsByCacheCode);
                            }
                            var resp = client.Client.GetGeocacheLogsByCacheCode(client.Token, gcList[0].Code, logCount, maxPerPage);
                            while (resp.Status.StatusCode == 0 && resp.Logs != null && resp.Logs.Count() > 0 && !done)
                            {
                                foreach (var lg in resp.Logs)
                                {
                                    if (!lg.IsArchived)
                                    {
                                        Core.Data.Log gcLog = ImportLog(db, lg);
                                        if (Core.Settings.Default.LiveAPIUpdateGeocacheLogsMax == 0 && gcLog != null)
                                        {
                                            ids.Add(gcLog.ID);
                                        }
                                    }
                                }

                                logCount += resp.Logs.Count();
                                if (Core.Settings.Default.LiveAPIUpdateGeocacheLogsMax > 0)
                                {
                                    int left = Core.Settings.Default.LiveAPIUpdateGeocacheLogsMax - logCount;
                                    if (left < maxPerPage)
                                    {
                                        maxPerPage = left;
                                    }
                                }
                                if (maxPerPage > 0)
                                {
                                    Thread.Sleep(Core.Settings.Default.LiveAPIDelayGetGeocacheLogsByCacheCode);
                                    resp = client.Client.GetGeocacheLogsByCacheCode(client.Token, gcList[0].Code, logCount, maxPerPage);
                                }
                                else
                                {
                                    done = true;
                                }
                            }
                            if (resp.Status.StatusCode != 0)
                            {
                                Core.ApplicationData.Instance.Logger.AddLog(new Import(), Core.Logger.Level.Error, resp.Status.StatusMessage);
                                break;
                            }
                            else
                            {
                                if (Core.Settings.Default.LiveAPIDeselectAfterUpdate)
                                {
                                    gcList[0].Selected = false;
                                }
                                if (Core.Settings.Default.LiveAPIUpdateGeocacheLogsMax == 0)
                                {
                                    List<Core.Data.Log> allLogs = Utils.DataAccess.GetLogs(db, gcList[0].Code);
                                    foreach (Core.Data.Log gim in allLogs)
                                    {
                                        if (!ids.Contains(gim.ID))
                                        {
                                            gim.DeleteRecord();
                                            db.LogCollection.Remove(gim);
                                        }
                                    }
                                }
                            }

                            index++;
                            if (!progress.Update("UpdatingGeocache", totalcount, index))
                            {
                                break;
                            }
                            gcList.RemoveAt(0);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(new Import(), e);
            }
        }


        public static async Task ImportGeocacheImagesAsync(Core.Storage.Database db, List<Core.Data.Geocache> gcList)
        {
            using (Utils.DataUpdater upd = new Utils.DataUpdater(db))
            {
                await Task.Run(() =>
                    {
                        ImportGeocacheImages(db, gcList);
                    });
            }
        }

        public static void ImportGeocacheImages(Core.Storage.Database db, List<Core.Data.Geocache> gcList)
        {
            try
            {
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock("UpdatingGeocaches", "UpdatingGeocache", gcList.Count, 0, true))
                {
                    int totalcount = gcList.Count;
                    using (LiveAPI.GeocachingLiveV6 client = new LiveAPI.GeocachingLiveV6())
                    {
                        int index = 0;
                        while (gcList.Count > 0)
                        {
                            if (index > 0)
                            {
                                Thread.Sleep(Core.Settings.Default.LiveAPIDelayGetImagesForGeocache);
                            }
                            var resp = client.Client.GetImagesForGeocache(client.Token, gcList[0].Code);
                            if (resp.Status.StatusCode == 0)
                            {
                                if (resp.Images != null)
                                {
                                    List<string> ids = new List<string>();
                                    foreach (var img in resp.Images)
                                    {
                                        if (img.Url.IndexOf("/cache/log/") < 0)
                                        {
                                            Core.Data.GeocacheImage gcImg = ImportGeocacheImage(db, img, gcList[0].Code);
                                            if (gcImg != null)
                                            {
                                                ids.Add(gcImg.ID);
                                            }
                                        }
                                    }
                                    List<Core.Data.GeocacheImage> allImages = Utils.DataAccess.GetGeocacheImages(db, gcList[0].Code);
                                    foreach (Core.Data.GeocacheImage gim in allImages)
                                    {
                                        if (!ids.Contains(gim.ID))
                                        {
                                            gim.DeleteRecord();
                                            db.GeocacheImageCollection.Remove(gim);
                                        }
                                    }
                                }

                                if (Core.Settings.Default.LiveAPIDeselectAfterUpdate)
                                {
                                    gcList[0].Selected = false;
                                }

                                index++;
                                if (!progress.Update("UpdatingGeocache", totalcount, index))
                                {
                                    break;
                                }
                                gcList.RemoveAt(0);
                            }
                            else
                            {
                                Core.ApplicationData.Instance.Logger.AddLog(new Import(),  Core.Logger.Level.Error, resp.Status.StatusMessage);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(new Import(), e);
            }
        }

        public static void ImportGeocaches(Core.Storage.Database db, LiveV6.SearchForGeocachesRequest req, int max)
        {
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock("ImportingGeocaches", "ImportingGeocaches", max, 0, true))
            {
                try
                {
                    using (GeocachingLiveV6 client = new GeocachingLiveV6())
                    {
                        req.AccessToken = client.Token;

                        var resp = client.Client.SearchForGeocaches(req);
                        if (resp.Status.StatusCode == 0 && resp.Geocaches != null)
                        {
                            ImportGeocaches(db, resp.Geocaches);

                            if (resp.Geocaches.Count() >= req.MaxPerPage && req.MaxPerPage < max)
                            {
                                if (progress.Update("ImportingGeocaches", max, resp.Geocaches.Count()))
                                {
                                    var mreq = new LiveV6.GetMoreGeocachesRequest();
                                    mreq.AccessToken = req.AccessToken;
                                    mreq.GeocacheLogCount = req.GeocacheLogCount;
                                    mreq.MaxPerPage = (int)Math.Min(req.MaxPerPage, max - resp.Geocaches.Count());
                                    mreq.StartIndex = resp.Geocaches.Count();
                                    mreq.TrackableLogCount = req.TrackableLogCount;
                                    mreq.IsLite = req.IsLite;
                                    mreq.GeocacheLogCount = req.GeocacheLogCount;

                                    while (resp.Status.StatusCode == 0 && resp.Geocaches != null && resp.Geocaches.Count() >= req.MaxPerPage)
                                    {
                                        resp = client.Client.GetMoreGeocaches(mreq);

                                        if (resp.Status.StatusCode == 0 && resp.Geocaches != null)
                                        {
                                            ImportGeocaches(db, resp.Geocaches);
                                            if (!progress.Update("ImportingGeocaches", max, mreq.StartIndex + resp.Geocaches.Count()))
                                            {
                                                break;
                                            }

                                            mreq.StartIndex += resp.Geocaches.Count();
                                            mreq.MaxPerPage = (int)Math.Min(req.MaxPerPage, max - mreq.StartIndex);
                                            if (mreq.StartIndex >= max - 1)
                                            {
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            Core.ApplicationData.Instance.Logger.AddLog(new Import(), Core.Logger.Level.Error, resp.Status.StatusMessage);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Core.ApplicationData.Instance.Logger.AddLog(new Import(), Core.Logger.Level.Error, resp.Status.StatusMessage);
                        }
                    }
                }
                catch (Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(new Import(), e);
                }
            }
        }


        public static void ImportGeocacheStatus(Core.Storage.Database db, List<string> gcCodes)
        {
            try
            {
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock("UpdatingGeocaches", "UpdatingGeocaches", gcCodes.Count, 0, true))
                {
                    int totalcount = gcCodes.Count;
                    using (var client = new GeocachingLiveV6())
                    {
                        int index = 0;
                        while (gcCodes.Count > 0)
                        {
                            var req = new LiveV6.GetGeocacheStatusRequest();
                            req.AccessToken = client.Token;
                            req.CacheCodes = (from a in gcCodes select a).Take(Core.Settings.Default.LiveAPIGetGeocacheStatusBatchSize).ToArray();
                            index += req.CacheCodes.Length;
                            gcCodes.RemoveRange(0, req.CacheCodes.Length);
                            var resp = client.Client.GetGeocacheStatus(req);
                            if (resp.Status.StatusCode == 0 && resp.GeocacheStatuses != null)
                            {
                                foreach (var gs in resp.GeocacheStatuses)
                                {
                                    Core.Data.Geocache gc = db.GeocacheCollection.GetGeocache(gs.CacheCode);
                                    if (gc != null)
                                    {
                                        gc.DataFromDate = DateTime.Now;
                                        gc.Archived = gs.Archived;
                                        gc.Available = gs.Available;
                                        if (!gc.Locked)
                                        {
                                            gc.Name = gs.CacheName;
                                        }
                                        gc.MemberOnly = gs.Premium;
                                        if (Core.Settings.Default.LiveAPIDeselectAfterUpdate)
                                        {
                                            gc.Selected = false;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Core.ApplicationData.Instance.Logger.AddLog(new Import(), new Exception(resp.Status.StatusMessage));
                                break;
                            }
                            if (!progress.Update("UpdatingGeocaches", totalcount, index))
                            {
                                break;
                            }
                            if (gcCodes.Count > 0)
                            {
                                System.Threading.Thread.Sleep(Core.Settings.Default.LiveAPIDelayGetGeocacheStatus);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(new Import(), e);
            }
        }


        public static void ImportGeocaches(Core.Storage.Database db, List<string> gcCodes)
        {
            try
            {
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock("ImportingGeocaches", "ImportingGeocaches", gcCodes.Count, 0, true))
                {
                    int totalcount = gcCodes.Count;
                    using (var client = new GeocachingLiveV6())
                    {
                        int index = 0;
                        while (gcCodes.Count > 0)
                        {
                            LiveV6.SearchForGeocachesRequest req = new LiveV6.SearchForGeocachesRequest();
                            req.IsLite = Core.Settings.Default.LiveAPIMemberTypeId == 1;
                            req.AccessToken = client.Token;
                            req.CacheCode = new LiveV6.CacheCodeFilter();
                            req.CacheCode.CacheCodes = (from a in gcCodes select a).Take(Core.Settings.Default.LiveAPIImportGeocachesBatchSize).ToArray();
                            req.MaxPerPage = Core.Settings.Default.LiveAPIImportGeocachesBatchSize;
                            req.GeocacheLogCount = 5;
                            index += req.CacheCode.CacheCodes.Length;
                            gcCodes.RemoveRange(0, req.CacheCode.CacheCodes.Length);
                            var resp = client.Client.SearchForGeocaches(req);
                            if (resp.Status.StatusCode == 0 && resp.Geocaches != null)
                            {
                                List<Core.Data.Geocache> upList = ImportGeocaches(db, resp.Geocaches);
                                if (Core.Settings.Default.LiveAPIDeselectAfterUpdate)
                                {
                                    foreach (var g in upList)
                                    {
                                        g.Selected = false;
                                    }
                                }
                            }
                            else
                            {
                                Core.ApplicationData.Instance.Logger.AddLog(new Import(), new Exception(resp.Status.StatusMessage));
                                break;
                            }
                            if (!progress.Update("ImportingGeocaches", totalcount, index))
                            {
                                break;
                            }
                            if (gcCodes.Count>0)
                            {
                                System.Threading.Thread.Sleep(Core.Settings.Default.LiveAPIDelaySearchForGeocaches);
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(new Import(), e);
            }
        }

        public static List<Core.Data.Geocache> ImportGeocaches(Core.Storage.Database db, LiveV6.Geocache[] gcList)
        {
            List<Core.Data.Geocache> result = new List<Core.Data.Geocache>();
            if (gcList != null)
            {
                foreach (var gc in gcList)
                {
                    Core.Data.Geocache g = ImportGeocache(db, gc);
                    if (g!=null)
                    {
                        result.Add(g);
                    }
                }
            }
            return result;
        }

        public static Core.Data.Geocache ImportGeocache(Core.Storage.Database db, LiveV6.Geocache gc)
        {
            Core.Data.Geocache result = null;
            if (gc != null)
            {
                result = db.GeocacheCollection.GetGeocache(gc.Code);

                Core.Data.IGeocacheData gcData;
                if (result == null)
                {
                    gcData = new Core.Data.GeocacheData();
                    gcData.Code = gc.Code;
                }
                else
                {
                    gcData = result;
                }

                gcData.Archived = gc.Archived ?? false;
                gcData.Available = gc.Available ?? true;

                if (result == null || !result.Locked)
                {
                    gcData.Container = Utils.DataAccess.GetGeocacheContainer((int)gc.ContainerType.ContainerTypeId);
                    if (gc.Attributes != null)
                    {
                        List<int> attr = new List<int>();
                        foreach (LiveV6.Attribute a in gc.Attributes)
                        {
                            attr.Add(a.IsOn ? a.AttributeTypeID : -1 * a.AttributeTypeID);
                        }
                        gcData.AttributeIds = attr;
                    }
                    if (gc.Latitude != null) gcData.Lat = (double)gc.Latitude;
                    if (gc.Longitude != null) gcData.Lon = (double)gc.Longitude;
                    if (gc.Country != null) gcData.Country = gc.Country;
                    gcData.DataFromDate = DateTime.Now;
                    gcData.Difficulty = gc.Difficulty;
                    gcData.Terrain = gc.Terrain;
                    gcData.Name = gc.Name;
                    if (gc.FavoritePoints != null)
                    {
                        gcData.Favorites = (int)gc.FavoritePoints;
                    }
                    gcData.GeocacheType = Utils.DataAccess.GetGeocacheType((int)gc.CacheType.GeocacheTypeId);
                    if (gc.LongDescription != null)
                    {
                        gcData.LongDescription = gc.LongDescription;
                        gcData.LongDescriptionInHtml = gc.LongDescriptionIsHtml;
                    }
                    if (gc.EncodedHints != null)
                    {
                        gcData.EncodedHints = gc.EncodedHints;
                    }
                    gcData.MemberOnly = gc.IsPremium ?? false;
                    gcData.Owner = gc.Owner.UserName;
                    if (gc.Owner.Id != null)
                    {
                        gcData.OwnerId = gc.Owner.Id.ToString();
                    }
                    gcData.PlacedBy = gc.PlacedBy;
                    gcData.PublishedTime = gc.UTCPlaceDate;
                    if (gcData.ShortDescription != null)
                    {
                        gcData.ShortDescription = gc.ShortDescription;
                        gcData.ShortDescriptionInHtml = gc.ShortDescriptionIsHtml;
                    }
                    if (gc.State == "None")
                    {
                        gcData.State = "";
                    }
                    else
                    {
                        gcData.State = gc.State;
                    }
                    gcData.PersonalNote = gc.GeocacheNote;
                    if (gc.HasbeenFoundbyUser != null)
                    {
                        gcData.Found = (bool)gc.HasbeenFoundbyUser;
                    }
                    gcData.Url = gc.Url;
                    gcData.PersonalNote = gc.GeocacheNote ?? "";

                    if (gcData is Core.Data.GeocacheData)
                    {
                        if (Utils.DataAccess.AddGeocache(db, gcData as Core.Data.GeocacheData))
                        {
                            result = db.GeocacheCollection.GetGeocache(gcData.Code);
                        }
                    }
                    if (result != null)
                    {
                        Utils.Calculus.SetDistanceAndAngleGeocacheFromLocation(result, Core.ApplicationData.Instance.CenterLocation);

                        ImportLogs(db, gc.GeocacheLogs);
                        ImportWaypoints(db, gc.AdditionalWaypoints);
                        ImportUserWaypoints(db, gc.UserWaypoints, gc.Code);
                        ImportGeocacheImages(db, gc.Images, gc.Code);
                    }
                }
            }
            return result;
        }

        public static List<Core.Data.Log> ImportLogs(Core.Storage.Database db, LiveV6.GeocacheLog[] lgs)
        {
            List<Core.Data.Log> result = new List<Core.Data.Log>();
            if (lgs != null)
            {
                foreach (var lg in lgs)
                {
                    Core.Data.Log g = ImportLog(db, lg);
                    if (g != null)
                    {
                        result.Add(g);
                    }
                }
            }
            return result;
        }

        public static Core.Data.Log ImportLog(Core.Storage.Database db, LiveV6.GeocacheLog lg)
        {
            Core.Data.Log result = null;
            if (lg != null)
            {
                result = db.LogCollection.GetLog(lg.Code);

                Core.Data.ILogData lgData;
                if (result == null)
                {
                    lgData = new Core.Data.LogData();
                    lgData.ID = lg.Code;
                }
                else
                {
                    lgData = result;
                }

                lgData.DataFromDate = DateTime.Now;
                lgData.Date = lg.VisitDate;
                lgData.Encoded = lg.LogIsEncoded;
                lgData.Finder = lg.Finder.UserName;
                lgData.FinderId = lg.Finder.Id.ToString();
                lgData.GeocacheCode = lg.CacheCode;
                lgData.LogType = Utils.DataAccess.GetLogType((int)lg.LogType.WptLogTypeId);
                lgData.Text = lg.LogText;

                if (lgData is Core.Data.LogData)
                {
                    if (Utils.DataAccess.AddLog(db, lgData as Core.Data.LogData))
                    {
                        result = db.LogCollection.GetLog(lgData.ID);

                        if (result!=null)
                        {
                            ImportLogImages(db, lg.Images, result.ID);
                        }
                    }
                }

            }
            return result;
        }

        public static List<Core.Data.LogImage> ImportLogImages(Core.Storage.Database db, LiveV6.ImageData[] imgs, string LogId)
        {
            List<Core.Data.LogImage> result = new List<Core.Data.LogImage>();
            if (imgs != null)
            {
                foreach (var lg in imgs)
                {
                    Core.Data.LogImage g = ImportLogImage(db, lg, LogId);
                    if (g != null)
                    {
                        result.Add(g);
                    }
                }
            }
            return result;
        }

        public static Core.Data.LogImage ImportLogImage(Core.Storage.Database db, LiveV6.ImageData img, string LogId)
        {
            Core.Data.LogImage result = null;
            if (img != null)
            {
                result = db.LogImageCollection.GetLogImage(img.Url);

                Core.Data.ILogImageData lgiData;
                if (result == null)
                {
                    lgiData = new Core.Data.LogImageData();
                    lgiData.ID = img.Url;
                    lgiData.LogId = LogId;
                    lgiData.Url = img.Url;
                }
                else
                {
                    lgiData = result;
                }

                lgiData.DataFromDate = DateTime.Now;
                lgiData.Name = img.Name;

                if (lgiData is Core.Data.LogImageData)
                {
                    if (Utils.DataAccess.AddLogImage(db, lgiData as Core.Data.LogImageData))
                    {
                        result = db.LogImageCollection.GetLogImage(img.Url);
                    }
                }
            }
            return result;
        }

        public static List<Core.Data.Waypoint> ImportWaypoints(Core.Storage.Database db, LiveV6.AdditionalWaypoint[] wps)
        {
            List<Core.Data.Waypoint> result = new List<Core.Data.Waypoint>();
            if (wps != null)
            {
                foreach (var lg in wps)
                {
                    Core.Data.Waypoint g = ImportWaypoint(db, lg);
                    if (g != null)
                    {
                        result.Add(g);
                    }
                }
            }
            return result;
        }

        public static Core.Data.Waypoint ImportWaypoint(Core.Storage.Database db, LiveV6.AdditionalWaypoint wp)
        {
            Core.Data.Waypoint result = null;
            if (wp != null)
            {
                result = db.WaypointCollection.GetWaypoint(wp.Code);

                Core.Data.IWaypointData wpd;
                if (result == null)
                {
                    wpd = new Core.Data.WaypointData();
                    wpd.Code = wp.Code;
                    wpd.ID = wp.Code;
                }
                else
                {
                    wpd = result;
                }

                wpd.DataFromDate = DateTime.Now;
                wpd.WPType = Utils.DataAccess.GetWaypointType(wp.WptTypeID);
                wpd.Comment = wp.Comment;
                wpd.Description = wp.Description;
                wpd.GeocacheCode = wp.GeocacheCode;
                wpd.Lat = wp.Latitude;
                wpd.Lon = wp.Longitude;
                wpd.Name = wp.Name;
                wpd.Time = wp.UTCEnteredDate;
                wpd.Url = wp.Url;
                wpd.UrlName = wp.UrlName;

                if (wpd is Core.Data.WaypointData)
                {
                    if (Utils.DataAccess.AddWaypoint(db, wpd as Core.Data.WaypointData))
                    {
                        result = db.WaypointCollection.GetWaypoint(wp.Code);
                    }
                }
            }
            return result;
        }

        public static List<Core.Data.UserWaypoint> ImportUserWaypoints(Core.Storage.Database db, LiveV6.UserWaypoint[] wps, string GeocacheCode)
        {
            List<Core.Data.UserWaypoint> result = new List<Core.Data.UserWaypoint>();
            List<Core.Data.UserWaypoint> curUwps = Utils.DataAccess.GetUserWaypointsFromGeocache(db, GeocacheCode);
            if (wps != null)
            {
                foreach (var lg in wps)
                {
                    Core.Data.UserWaypoint g = ImportUserWaypoint(db, lg);
                    if (g != null)
                    {
                        result.Add(g);
                        if (curUwps.Contains(g))
                        {
                            curUwps.Remove(g);
                        }
                    }
                }
            }
            foreach (var g in curUwps)
            {
                g.DeleteRecord();
                db.UserWaypointCollection.Remove(g);
            }
            return result;
        }

        public static Core.Data.UserWaypoint ImportUserWaypoint(Core.Storage.Database db, LiveV6.UserWaypoint wp)
        {
            Core.Data.UserWaypoint result = null;
            if (wp != null)
            {
                result = db.UserWaypointCollection.GetUserWaypoint(wp.ID.ToString());

                Core.Data.IUserWaypointData wpd;
                if (result == null)
                {
                    wpd = new Core.Data.UserWaypointData();
                    wpd.ID = wp.ID.ToString();
                }
                else
                {
                    wpd = result;
                }

                wpd.Description = wp.Description;
                wpd.GeocacheCode = wp.CacheCode;
                wpd.Lat = wp.Latitude;
                wpd.Lon = wp.Longitude;
                wpd.Date = wp.UTCDate.ToLocalTime();

                if (wpd is Core.Data.UserWaypointData)
                {
                    if (Utils.DataAccess.AddUserWaypoint(db, wpd as Core.Data.UserWaypointData))
                    {
                        result = db.UserWaypointCollection.GetUserWaypoint(wp.ID.ToString());

                        if (wp.IsCorrectedCoordinate)
                        {
                            Core.Data.Geocache thisGC = db.GeocacheCollection.GetGeocache(wp.CacheCode);
                            if (thisGC != null)
                            {
                                thisGC.CustomLat = wp.Latitude;
                                thisGC.CustomLon = wp.Longitude;
                            }
                        }

                    }
                }

            }
            return result;
        }


        public static List<Core.Data.GeocacheImage> ImportGeocacheImages(Core.Storage.Database db, LiveV6.ImageData[] wps, string GeocacheCode)
        {
            List<Core.Data.GeocacheImage> result = new List<Core.Data.GeocacheImage>();
            List<Core.Data.GeocacheImage> curImgs = Utils.DataAccess.GetGeocacheImages(db, GeocacheCode);
            if (wps != null)
            {
                foreach (var lg in wps)
                {
                    Core.Data.GeocacheImage g = ImportGeocacheImage(db, lg, GeocacheCode);
                    if (g != null)
                    {
                        result.Add(g);
                        if (curImgs.Contains(g))
                        {
                            curImgs.Remove(g);
                        }
                    }
                }
            }
            foreach(var g in curImgs)
            {
                g.DeleteRecord();
                db.GeocacheImageCollection.Remove(g);
            }
            return result;
        }


        public static Core.Data.GeocacheImage ImportGeocacheImage(Core.Storage.Database db, LiveV6.ImageData img, string GeocacheCode)
        {
            Core.Data.GeocacheImage result = null;
            if (img != null)
            {
                result = db.GeocacheImageCollection.GetGeocacheImage(img.ImageGuid.ToString());

                Core.Data.IGeocacheImageData wpd;
                if (result == null)
                {
                    wpd = new Core.Data.GeocacheImageData();
                    wpd.ID = img.ImageGuid.ToString();
                }
                else
                {
                    wpd = result;
                }

                wpd.DataFromDate = DateTime.Now;
                wpd.GeocacheCode = GeocacheCode;
                wpd.Name = img.Name;
                wpd.Url = img.Url;
                wpd.ThumbUrl = img.ThumbUrl;
                wpd.MobileUrl = img.MobileUrl;
                wpd.Description = img.Description;

                if (wpd is Core.Data.GeocacheImageData)
                {
                    if (Utils.DataAccess.AddGeocacheImage(db, wpd as Core.Data.GeocacheImageData))
                    {
                        result = db.GeocacheImageCollection.GetGeocacheImage(img.ImageGuid.ToString());
                    }
                }

            }
            return result;
        }

        public async Task ImportNotesAsync(Core.Storage.Database db, bool importMissing)
        {
            using (Utils.DataUpdater upd = new Utils.DataUpdater(db))
            {
                await Task.Run(() =>
                {
                    ImportNotes(db, importMissing);
                });
            }
        }
        public void ImportNotes(Core.Storage.Database db, bool importMissing)
        {
            List<LiveAPI.LiveV6.CacheNote> missingGeocaches = new List<LiveAPI.LiveV6.CacheNote>();
            bool error = false;
            try
            {
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock("ImportingGeocachingcomFieldNotes", "ImportingGeocachingcomFieldNotes", 1, 0))
                {
                    //clear all notes
                    foreach (var gc in db.GeocacheCollection)
                    {
                        gc.PersonalNote = "";
                    }

                    using (LiveAPI.GeocachingLiveV6 client = new LiveAPI.GeocachingLiveV6(false))
                    {
                        int maxPerRequest = 100;
                        int startIndex = 0;
                        var resp = client.Client.GetUsersCacheNotes(client.Token, startIndex, maxPerRequest);
                        while (resp.Status.StatusCode == 0)
                        {
                            foreach (var n in resp.CacheNotes)
                            {
                                var gc = db.GeocacheCollection.GetGeocache(n.CacheCode);
                                if (gc != null)
                                {
                                    string s = n.Note ?? "";
                                    s = s.Replace("\r", "");
                                    s = s.Replace("\n", "\r\n");
                                    gc.PersonalNote = s;
                                }
                                else
                                {
                                    missingGeocaches.Add(n);
                                }
                            }
                            if (resp.CacheNotes.Count() >= maxPerRequest)
                            {
                                startIndex += resp.CacheNotes.Count();
                                Thread.Sleep(2100);
                                resp = client.Client.GetUsersCacheNotes(client.Token, startIndex, maxPerRequest);
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (resp.Status.StatusCode != 0)
                        {
                            error = true;
                            if (!string.IsNullOrEmpty(resp.Status.StatusMessage))
                            {
                                Core.ApplicationData.Instance.Logger.AddLog(this, Core.Logger.Level.Error, resp.Status.StatusMessage);
                            }
                        }
                    }
                }
                if (!error && missingGeocaches.Count > 0)
                {
                    List<string> gcList = (from a in missingGeocaches select a.CacheCode).ToList();
                    if (importMissing)
                    {
                        LiveAPI.Import.ImportGeocaches(db, gcList);
                        foreach (var n in missingGeocaches)
                        {
                            var gc = db.GeocacheCollection.GetGeocache(n.CacheCode);
                            if (gc != null)
                            {
                                string s = n.Note ?? "";
                                s = s.Replace("\r", "");
                                s = s.Replace("\n", "\r\n");
                                gc.PersonalNote = s;
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
