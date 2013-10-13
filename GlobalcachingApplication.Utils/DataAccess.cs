using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Utils
{
    public class DataAccess
    {
        public static Framework.Data.Geocache GetGeocache(Framework.Data.GeocacheCollection cacheCollection, string CacheCode)
        {
            return cacheCollection.GetGeocache(CacheCode);
        }

        public static List<Framework.Data.Geocache> GetSelectedGeocaches(Framework.Data.GeocacheCollection cacheCollection)
        {
            List<Framework.Data.Geocache> result = null;

            result = (from Framework.Data.Geocache wp in cacheCollection
                      where wp.Selected
                      select wp).ToList();

            return result;
        }

        public static bool IsGeocacheFound(Framework.Interfaces.ICore core, Framework.Data.Geocache gc, string byUser)
        {
            bool result = false;
            if (core.GeocachingAccountNames.GetAccountName(gc.Code).ToLower() == byUser.ToLower())
            {
                result = gc.Found;
            }
            else
            {
                if (!result)
                {
                    string tl = byUser.ToLower();
                    result = (from Framework.Data.Log l in core.Logs where l.GeocacheCode == gc.Code && l.Finder.ToLower() == tl && l.LogType.AsFound select l).FirstOrDefault() != null;
                }
            }
            return result;
        }

        public static List<Framework.Data.Geocache> GetFoundGeocaches(Framework.Data.GeocacheCollection cacheCollection, Framework.Data.LogCollection logCollection, Framework.Data.GeocachingComAccountInfo accountInfo)
        {
            List<Framework.Data.Geocache> result = null;

            if (accountInfo.AccountName.Length == 0)
            {
                result = new List<Framework.Data.Geocache>();
            }
            else
            {
                string tl = accountInfo.AccountName.ToLower();
                result = (from Framework.Data.Geocache wp in cacheCollection
                          join Framework.Data.Log l in logCollection on wp.Code equals l.GeocacheCode
                          where l.Finder.ToLower() == tl && l.LogType.AsFound
                          select wp).Distinct().ToList();
                List<Framework.Data.Geocache> result2 = (from Framework.Data.Geocache wp in cacheCollection where wp.Found && !result.Contains(wp) select wp).ToList();
                result.AddRange(result2);
            }
            return result;
        }

        public static List<Framework.Data.Geocache> GetFoundGeocaches(Framework.Data.GeocacheCollection cacheCollection, Framework.Data.LogCollection logCollection, string byUser)
        {
            List<Framework.Data.Geocache> result = null;

            if (byUser.Length == 0)
            {
                result = new List<Framework.Data.Geocache>();
            }
            else
            {
                string tl = byUser.ToLower();
                result = (from Framework.Data.Geocache wp in cacheCollection
                          join Framework.Data.Log l in logCollection on wp.Code equals l.GeocacheCode
                          where l.Finder.ToLower() == tl && l.LogType.AsFound
                          select wp).Distinct().ToList();
            }
            return result;
        }

        public static Framework.Data.UserWaypoint GetUserWaypoint(Framework.Data.UserWaypointCollection wpCollection, int wpId)
        {
            return wpCollection.getWaypoint(wpId);
        }

        public static Framework.Data.Waypoint GetWaypoint(Framework.Data.WaypointCollection wpCollection, string wpCode)
        {
            return wpCollection.getWaypoint(wpCode);
        }

        public static List<Framework.Data.Waypoint> GetWaypointsFromGeocache(Framework.Data.WaypointCollection wpCollection, string geocacheCode)
        {
            //grouping
            return wpCollection.GetWaypoints(geocacheCode);

            /*
            List<Framework.Data.Waypoint> result = null;

            result = (from Framework.Data.Waypoint wp in wpCollection
                      where wp.GeocacheCode == geocacheCode
                      select wp).ToList();

            return result;
             * */
        }

        public static List<Framework.Data.UserWaypoint> GetUserWaypointsFromGeocache(Framework.Data.UserWaypointCollection wpCollection, string geocacheCode)
        {
            List<Framework.Data.UserWaypoint> result = null;

            result = (from Framework.Data.UserWaypoint wp in wpCollection
                      where wp.GeocacheCode == geocacheCode
                      select wp).OrderBy(x => x.ID).ToList();

            return result;
        }

        public static void CheckUserWaypointsForGeocache(Framework.Data.UserWaypointCollection wpCollection, string geocacheCode, List<int> validUserWaypointIds)
        {
            List<Framework.Data.UserWaypoint> curList = (from Framework.Data.UserWaypoint w in wpCollection where w.GeocacheCode == geocacheCode select w).ToList();
            foreach (var wp in curList)
            {
                if (!validUserWaypointIds.Contains(wp.ID))
                {
                    wpCollection.Remove(wp);
                }
            }
        }

        public static Framework.Data.GeocacheImage GetGeocacheImage(Framework.Data.GeocacheImageCollection lCollection, string lId)
        {
            return lCollection.GetGeocacheImage(lId);
        }

        public static List<Framework.Data.GeocacheImage> GetGeocacheImages(Framework.Data.GeocacheImageCollection lCollection, string GeocacheCode)
        {
            //grouping
            return lCollection.GetGeocacheImages(GeocacheCode);
        }

        public static Framework.Data.Log GetLog(Framework.Data.LogCollection lCollection, string lId)
        {
            return lCollection.GetLog(lId);
        }

        public static List<Framework.Data.Log> GetLogs(Framework.Data.LogCollection lCollection, string GeocacheCode)
        {
            //grouping
            return lCollection.GetLogs(GeocacheCode);

            /*
            List<Framework.Data.Log> result = null;

            result = (from Framework.Data.Log l in lCollection
                      where l.GeocacheCode == GeocacheCode
                      orderby l.Date descending
                      select l).ToList();

            return result;
             * */
        }

        public static Framework.Data.LogImage GetLogImage(Framework.Data.LogImageCollection lCollection, string lId)
        {
            return lCollection.GetLogImage(lId);
        }

        public static List<Framework.Data.LogImage> GetLogImages(Framework.Data.LogImageCollection lCollection, string logId)
        {
            //grouping
            return lCollection.GetLogImages(logId);

            /*
            List<Framework.Data.LogImage> result = null;

            result = (from Framework.Data.LogImage l in lCollection
                      where l.LogID == logId
                      select l).ToList();

            return result;
             * */
        }

        public static List<Framework.Data.LogImage> GetLogImages(Framework.Data.LogImageCollection lCollection, Framework.Data.LogCollection logCollection, string GeocacheCode)
        {
            List<Framework.Data.LogImage> result = new List<Framework.Data.LogImage>();
            List<Framework.Data.Log> logs = GetLogs(logCollection, GeocacheCode);
            foreach (Framework.Data.Log l in logs)
            {
                result.AddRange(lCollection.GetLogImages(l.ID));
            }
            return result;
        }

        public static Framework.Data.GeocacheType GetGeocacheType(Framework.Data.GeocacheTypeCollection gtCollection, int typeId)
        {
            Framework.Data.GeocacheType result = null;

            result = (from gt in gtCollection
                      where gt.ID == typeId
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = (from gt in gtCollection
                          where gt.ID == 0
                          select gt).FirstOrDefault();
            }
            return result;
        }

        public static Framework.Data.GeocacheType GetGeocacheType(Framework.Data.GeocacheTypeCollection gtCollection, string keyWord)
        {
            return GetGeocacheType(gtCollection, keyWord, 0);
        }
        public static Framework.Data.GeocacheType GetGeocacheType(Framework.Data.GeocacheTypeCollection gtCollection, string keyWord, int minID)
        {
            Framework.Data.GeocacheType result = null;

            keyWord = keyWord.ToLower();
            result = (from gt in gtCollection
                      where gt.ID>= minID && gt.Name.ToLower().Contains(keyWord)
                      orderby gt.ID
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = GetGeocacheType(gtCollection, 01);
            }
            return result;
        }

        public static Framework.Data.WaypointType GetWaypointType(Framework.Data.WaypointTypeCollection wptCollection, int typeId)
        {
            Framework.Data.WaypointType result = null;

            result = (from gt in wptCollection
                      where gt.ID == typeId
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = (from gt in wptCollection
                          where gt.ID == 0
                          select gt).FirstOrDefault();
            }
            return result;
        }

        public static Framework.Data.WaypointType GetWaypointType(Framework.Data.WaypointTypeCollection wptCollection, string keyWord)
        {
            Framework.Data.WaypointType result = null;

            keyWord = keyWord.ToLower();
            result = (from gt in wptCollection
                      where gt.Name.ToLower().Contains(keyWord)
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = GetWaypointType(wptCollection, 0);
            }
            return result;
        }

        public static Framework.Data.LogType GetLogType(Framework.Data.LogTypeCollection ltCollection, int typeId)
        {
            Framework.Data.LogType result = null;

            result = (from gt in ltCollection
                      where gt.ID == typeId
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = (from gt in ltCollection
                          where gt.ID == 0
                          select gt).FirstOrDefault();
            }
            return result;
        }

        public static Framework.Data.LogType GetLogType(Framework.Data.LogTypeCollection ltCollection, string keyWord)
        {
            Framework.Data.LogType result = null;

            keyWord = keyWord.ToLower();
            result = (from gt in ltCollection
                      where gt.Name.ToLower().Contains(keyWord)
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = GetLogType(ltCollection, 0);
            }
            return result;
        }

        public static Framework.Data.GeocacheAttribute GetGeocacheAttribute(Framework.Data.GeocacheAttributeCollection ltCollection, int typeId)
        {
            Framework.Data.GeocacheAttribute result = null;

            result = (from gt in ltCollection
                      where gt.ID == (int)Math.Abs(typeId)
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = (from gt in ltCollection
                          where gt.ID == 0
                          select gt).FirstOrDefault();
            }
            return result;
        }


        public static Framework.Data.GeocacheContainer GetGeocacheContainer(Framework.Data.GeocacheContainerCollection gccCollection, int containerId)
        {
            Framework.Data.GeocacheContainer result = null;

            result = (from gt in gccCollection
                      where gt.ID == containerId
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = (from gt in gccCollection
                          where gt.ID == 0
                          select gt).FirstOrDefault();
            }
            return result;
        }

        public static Framework.Data.GeocacheContainer GetGeocacheContainer(Framework.Data.GeocacheContainerCollection gccCollection, string container)
        {
            Framework.Data.GeocacheContainer result = null;

            container = container.ToLower();
            result = (from gt in gccCollection
                      where gt.Name.ToLower() == container
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = GetGeocacheContainer(gccCollection, 0);
            }
            return result;
        }

        public static bool UpdateGeocacheData(Framework.Data.Geocache gc, Framework.Data.Geocache newData, Version gpxDataVersion)
        {
            bool result = false;
            if (gc.Code == newData.Code)
            {
                gc.UpdateFrom(newData, gpxDataVersion);
                result = true;
            }
            return result;
        }

        public static bool UpdateWaypointData(Framework.Data.Waypoint wp, Framework.Data.Waypoint newData)
        {
            bool result = false;
            if (wp.Code == newData.Code)
            {
                wp.UpdateFrom(newData);
                result = true;
            }
            return result;
        }

        public static bool UpdateUserWaypointData(Framework.Data.UserWaypoint wp, Framework.Data.UserWaypoint newData)
        {
            bool result = false;
            if (wp.ID == newData.ID)
            {
                wp.UpdateFrom(newData);
                result = true;
            }
            return result;
        }

        public static bool UpdateLogData(Framework.Data.Log l, Framework.Data.Log newData)
        {
            bool result = false;
            if (l.ID == newData.ID)
            {
                l.UpdateFrom(newData);
                result = true;
            }
            return result;
        }

        public static bool UpdateLogImageData(Framework.Data.LogImage l, Framework.Data.LogImage newData)
        {
            bool result = false;
            if (l.ID == newData.ID)
            {
                l.UpdateFrom(newData);
                result = true;
            }
            return result;
        }

        public static bool UpdateGeocacheImageData(Framework.Data.GeocacheImage l, Framework.Data.GeocacheImage newData)
        {
            bool result = false;
            if (l.ID == newData.ID)
            {
                l.UpdateFrom(newData);
                result = true;
            }
            return result;
        }

        public static void DeleteGeocache(Framework.Interfaces.ICore core, Framework.Data.Geocache gc)
        {
            using (FrameworkDataUpdater upd = new FrameworkDataUpdater(core))
            {
                List<Framework.Data.Waypoint> wps = GetWaypointsFromGeocache(core.Waypoints, gc.Code);
                foreach (Framework.Data.Waypoint wp in wps)
                {
                    core.Waypoints.Remove(wp);
                }
                List<Framework.Data.GeocacheImage> imgs = GetGeocacheImages(core.GeocacheImages, gc.Code);
                foreach (Framework.Data.GeocacheImage img in imgs)
                {
                    core.GeocacheImages.Remove(img);
                }
                List<Framework.Data.Log> logs = GetLogs(core.Logs, gc.Code);
                foreach (Framework.Data.Log lg in logs)
                {
                    DeleteLog(core, lg);
                }
                core.Geocaches.Remove(gc);
            }
        }

        public static void DeleteLog(Framework.Interfaces.ICore core, Framework.Data.Log log)
        {
            core.Logs.BeginUpdate();
            core.LogImages.BeginUpdate();
            List<Framework.Data.LogImage> limgs = GetLogImages(core.LogImages, log.ID);
            foreach (Framework.Data.LogImage li in limgs)
            {
                core.LogImages.Remove(li);
            }
            core.Logs.Remove(log);
            core.LogImages.EndUpdate();
            core.Logs.EndUpdate();
        }

    }
}
