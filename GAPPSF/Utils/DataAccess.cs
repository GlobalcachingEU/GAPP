using GAPPSF.Core;
using GAPPSF.Core.Data;
using GAPPSF.Core.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Utils
{
    class DataAccess
    {
        public static bool AddGeocache(Database db, GeocacheData gd)
        {
            bool result = true;
            Geocache gc = db.GeocacheCollection.GetGeocache(gd.Code);
            if (gc == null)
            {
                gc = new Geocache(db, gd);
            }
            else
            {
                if (gc.DataFromDate<gd.DataFromDate)
                {
                    gc.BeginUpdate();
                    GeocacheData.Copy(gd, gc);
                    gc.EndUpdate();
                }
            }
            Utils.Calculus.SetDistanceAndAngleGeocacheFromLocation(gc, ApplicationData.Instance.CenterLocation);
            return result;
        }

        public static bool AddLog(Database db, LogData gd)
        {
            bool result = true;
            Log gc = db.LogCollection.GetLog(gd.ID);
            if (gc == null)
            {
                gc = new Log(db, gd);
            }
            else
            {
                if (gc.DataFromDate < gd.DataFromDate)
                {
                    gc.BeginUpdate();
                    LogData.Copy(gd, gc);
                    gc.EndUpdate();
                }
            }
            return result;
        }

        public static bool AddWaypoint(Database db, WaypointData gd)
        {
            bool result = true;
            Waypoint gc = db.WaypointCollection.GetWaypoint(gd.ID);
            if (gc == null)
            {
                gc = new Waypoint(db, gd);
            }
            else
            {
                if (gc.DataFromDate < gd.DataFromDate)
                {
                    gc.BeginUpdate();
                    WaypointData.Copy(gd, gc);
                    gc.EndUpdate();
                }
            }
            return result;
        }

        public static bool AddLogImage(Database db,LogImageData gd)
        {
            bool result = true;
            LogImage gc = db.LogImageCollection.GetLogImage(gd.ID);
            if (gc == null)
            {
                gc = new LogImage(db, gd);
            }
            else
            {
                if (gc.DataFromDate < gd.DataFromDate)
                {
                    gc.BeginUpdate();
                    LogImageData.Copy(gd, gc);
                    gc.EndUpdate();
                }
            }
            return result;
        }

        public static bool AddGeocacheImage(Database db, GeocacheImageData gd)
        {
            bool result = true;
            GeocacheImage gc = db.GeocacheImageCollection.GetGeocacheImage(gd.ID);
            if (gc == null)
            {
                gc = new GeocacheImage(db, gd);
            }
            else
            {
                if (gc.DataFromDate < gd.DataFromDate)
                {
                    gc.BeginUpdate();
                    GeocacheImageData.Copy(gd, gc);
                    gc.EndUpdate();
                }
            }
            return result;
        }

        public static bool AddUserWaypoint(Database db, UserWaypointData gd)
        {
            bool result = true;
            UserWaypoint gc = db.UserWaypointCollection.GetUserWaypoint(gd.ID);
            if (gc == null)
            {
                gc = new UserWaypoint(db, gd);
            }
            else
            {
                gc.BeginUpdate();
                UserWaypointData.Copy(gd, gc);
                gc.EndUpdate();
            }
            return result;
        }

        public static GAPPSF.Core.Data.WaypointType GetWaypointType(int typeId)
        {
            GAPPSF.Core.Data.WaypointType result = null;

            result = (from gt in Core.ApplicationData.Instance.WaypointTypes
                      where gt.ID == typeId
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = (from gt in Core.ApplicationData.Instance.WaypointTypes
                          where gt.ID == 0
                          select gt).FirstOrDefault();
            }
            return result;
        }

        public static GAPPSF.Core.Data.WaypointType GetWaypointType(string keyWord)
        {
            GAPPSF.Core.Data.WaypointType result = null;

            keyWord = keyWord.ToLower();
            result = (from gt in Core.ApplicationData.Instance.WaypointTypes
                      where gt.Name.ToLower().Contains(keyWord)
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = GetWaypointType(0);
            }
            return result;
        }


        public static GAPPSF.Core.Data.LogType GetLogType(int typeId)
        {
            GAPPSF.Core.Data.LogType result = null;

            result = (from gt in ApplicationData.Instance.LogTypes
                      where gt.ID == typeId
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = (from gt in ApplicationData.Instance.LogTypes
                          where gt.ID == 0
                          select gt).FirstOrDefault();
            }
            return result;
        }


        public static Core.Data.LogType GetLogType(string keyWord)
        {
            Core.Data.LogType result = null;

            keyWord = keyWord.ToLower();
            result = (from gt in ApplicationData.Instance.LogTypes
                      where gt.Name.ToLower().Contains(keyWord)
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = GetLogType(0);
            }
            return result;
        }


        public static GAPPSF.Core.Data.GeocacheType GetGeocacheType(int typeId)
        {
            GAPPSF.Core.Data.GeocacheType result = null;

            result = (from gt in ApplicationData.Instance.GeocacheTypes
                      where gt.ID == typeId
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = (from gt in ApplicationData.Instance.GeocacheTypes
                          where gt.ID == 0
                          select gt).FirstOrDefault();
            }
            return result;
        }

        public static GAPPSF.Core.Data.GeocacheType GetGeocacheType(string keyWord)
        {
            return GetGeocacheType(keyWord, 0);
        }
        public static GAPPSF.Core.Data.GeocacheType GetGeocacheType(string keyWord, int minID)
        {
            GAPPSF.Core.Data.GeocacheType result = null;

            keyWord = keyWord.ToLower();
            result = (from gt in ApplicationData.Instance.GeocacheTypes
                      where gt.ID >= minID && gt.Name.ToLower().Contains(keyWord)
                      orderby gt.ID
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = GetGeocacheType(0);
            }
            return result;
        }


        public static GAPPSF.Core.Data.GeocacheContainer GetGeocacheContainer(int containerId)
        {
            GAPPSF.Core.Data.GeocacheContainer result = null;

            result = (from gt in ApplicationData.Instance.GeocacheContainers
                      where gt.ID == containerId
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = (from gt in ApplicationData.Instance.GeocacheContainers
                          where gt.ID == 0
                          select gt).FirstOrDefault();
            }
            return result;
        }

        public static GAPPSF.Core.Data.GeocacheContainer GetGeocacheContainer(string container)
        {
            GAPPSF.Core.Data.GeocacheContainer result = null;

            container = container.ToLower();
            result = (from gt in ApplicationData.Instance.GeocacheContainers
                      where gt.Name.ToLower() == container
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = GetGeocacheContainer(0);
            }
            return result;
        }

        public static List<GAPPSF.Core.Data.GeocacheImage> GetGeocacheImages(Database db, string GeocacheCode)
        {
            //grouping
            return db.GeocacheImageCollection.GetGeocacheImages(GeocacheCode);
        }


        public static List<GAPPSF.Core.Data.UserWaypoint> GetUserWaypointsFromGeocache(Database db, string geocacheCode)
        {
            return db.UserWaypointCollection.GetWaypoints(geocacheCode);
        }

        public static List<Core.Data.Waypoint> GetWaypointsFromGeocache(Database db, string geocacheCode)
        {
            return db.WaypointCollection.GetWaypoints(geocacheCode);
        }

        public static List<Core.Data.Log> GetLogs(Database db, string geocacheCode)
        {
            return db.LogCollection.GetLogs(geocacheCode);
        }

        public static List<Core.Data.LogImage> GetLogImages(Database db, string logId)
        {
            return db.LogImageCollection.GetLogImages(logId);
        }

    }
}
