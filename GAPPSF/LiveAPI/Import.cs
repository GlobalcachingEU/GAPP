using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.LiveAPI
{
    public class Import
    {
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

    }
}
