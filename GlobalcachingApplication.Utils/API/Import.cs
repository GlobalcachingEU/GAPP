using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Utils.API
{
    public class Import
    {
        public static List<Framework.Data.Geocache> AddGeocaches(Framework.Interfaces.ICore core, LiveV6.Geocache[] gcList)
        {
            List<Framework.Data.Geocache> result = new List<Framework.Data.Geocache>();
            if (gcList != null)
            {
                foreach (var gcApi in gcList)
                {
                    Framework.Data.Geocache activeGC = Utils.API.Convert.Geocache(core, gcApi);
                    if (AddGeocache(core, activeGC))
                    {
                        if (gcApi.GeocacheLogs != null)
                        {
                            foreach (var l in gcApi.GeocacheLogs)
                            {
                                Framework.Data.Log lg = Convert.Log(core, l);
                                AddLog(core, lg);
                                if (l.Images != null)
                                {
                                    foreach (var li in l.Images)
                                    {
                                        AddLogImage(core, Convert.LogImage(core, li, lg.ID));
                                    }
                                }
                            }
                        }
                        if (gcApi.AdditionalWaypoints != null)
                        {
                            foreach (var wp in gcApi.AdditionalWaypoints)
                            {
                                AddWaypoint(core, Convert.Waypoint(core, wp));
                            }
                        }
                        List<int> ids = new List<int>();
                        if (gcApi.UserWaypoints != null)
                        {
                            foreach (var wp in gcApi.UserWaypoints)
                            {
                                AddUserWaypoint(core, Convert.UserWaypoint(core, wp));
                                ids.Add((int)wp.ID);
                                //copy user waypoint to corrected as assumed it is
                                if (wp.IsCorrectedCoordinate)
                                {
                                    Framework.Data.Geocache thisGC = Utils.DataAccess.GetGeocache(core.Geocaches, activeGC.Code);
                                    if (thisGC != null)
                                    {
                                        thisGC.CustomLat = wp.Latitude;
                                        thisGC.CustomLon = wp.Longitude;
                                    }
                                }
                            }
                        }
                        DataAccess.CheckUserWaypointsForGeocache(core.UserWaypoints, gcApi.Code, ids);

                        if (gcApi.Images != null)
                        {
                            foreach (var imgd in gcApi.Images)
                            {
                                AddGeocacheImage(core, Convert.GeocacheImage(core, imgd, activeGC.Code));                                
                            }
                        }
                    }
                }
            }
            return result;
        }

        public static bool AddGeocache(Framework.Interfaces.ICore core, Framework.Data.Geocache gc)
        {
            bool result = false;

            Framework.Data.Geocache oldgc = DataAccess.GetGeocache(core.Geocaches, gc.Code);
            if (oldgc == null)
            {
                if (!Utils.GeocacheIgnoreSupport.Instance.IgnoreGeocache(gc))
                {
                    core.Geocaches.Add(gc);
                    result = true;
                }
            }
            else
            {
                result = true;
                if (gc.DataFromDate >= oldgc.DataFromDate)
                {
                    DataAccess.UpdateGeocacheData(oldgc, gc, null);
                }
            }

            return result;
        }

        public static bool AddWaypoint(Framework.Interfaces.ICore core, Framework.Data.Waypoint wp)
        {
            bool result = false;

            Framework.Data.Waypoint oldwp = DataAccess.GetWaypoint(core.Waypoints, wp.Code);
            if (oldwp == null)
            {
                core.Waypoints.Add(wp);
                result = true;
            }
            else
            {
                if (wp.DataFromDate >= oldwp.DataFromDate)
                {
                    DataAccess.UpdateWaypointData(oldwp, wp);
                }
            }
            return result;
        }

        public static bool AddUserWaypoint(Framework.Interfaces.ICore core, Framework.Data.UserWaypoint wp)
        {
            bool result = false;

            Framework.Data.UserWaypoint oldwp = DataAccess.GetUserWaypoint(core.UserWaypoints, wp.ID);
            if (oldwp == null)
            {
                core.UserWaypoints.Add(wp);
                result = true;
            }
            else
            {
                DataAccess.UpdateUserWaypointData(oldwp, wp);
            }
            return result;
        }

        public static bool AddLog(Framework.Interfaces.ICore core, Framework.Data.Log l)
        {
            bool result = false;

            Framework.Data.Log oldwp = DataAccess.GetLog(core.Logs, l.ID);
            if (oldwp == null)
            {
                core.Logs.Add(l);
                result = true;
            }
            else
            {
                if (l.DataFromDate >= oldwp.DataFromDate)
                {
                    DataAccess.UpdateLogData(oldwp, l);
                }
            }
            if (l.LogType.AsFound && core.GeocachingAccountNames.GetAccountName(l.GeocacheCode).ToLower() == l.Finder.ToLower())
            {
                //found
                Framework.Data.Geocache gc = DataAccess.GetGeocache(core.Geocaches, l.GeocacheCode);
                if (gc != null)
                {
                    gc.Found = true;
                }
            }

            return result;
        }

        public static bool AddLogImage(Framework.Interfaces.ICore core, Framework.Data.LogImage l)
        {
            bool result = false;

            Framework.Data.LogImage oldwp = DataAccess.GetLogImage(core.LogImages, l.ID);
            if (oldwp == null)
            {
                core.LogImages.Add(l);
                result = true;
            }
            else
            {
                if (l.DataFromDate >= oldwp.DataFromDate)
                {
                    DataAccess.UpdateLogImageData(oldwp, l);
                }
            }

            return result;
        }

        public static bool AddGeocacheImage(Framework.Interfaces.ICore core, Framework.Data.GeocacheImage l)
        {
            bool result = false;

            Framework.Data.GeocacheImage oldwp = DataAccess.GetGeocacheImage(core.GeocacheImages, l.ID);
            if (oldwp == null)
            {
                core.GeocacheImages.Add(l);
                result = true;
            }
            else
            {
                if (l.DataFromDate >= oldwp.DataFromDate)
                {
                    DataAccess.UpdateGeocacheImageData(oldwp, l);
                }
            }

            return result;
        }

    }
}
