using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Utils.API
{
    public class Convert
    {
        public static Framework.Data.Geocache Geocache(Framework.Interfaces.ICore core, LiveV6.Geocache gc)
        {
            Framework.Data.Geocache result = null;
            if (gc != null)
            {
                Framework.Data.Geocache tmp = DataAccess.GetGeocache(core.Geocaches, gc.Code);
                result = new Framework.Data.Geocache();
                if (tmp != null)
                {
                    result.UpdateFrom(tmp, null);
                }
                result.Code = gc.Code;
                result.ID = gc.ID.ToString();
                result.Archived = gc.Archived ?? false;
                result.Available = gc.Available ?? true;
                result.Container = DataAccess.GetGeocacheContainer(core.GeocacheContainers, (int)gc.ContainerType.ContainerTypeId);
                if (gc.Attributes!=null)
                {
                    List<int> attr = new List<int>();
                    foreach (LiveV6.Attribute a in gc.Attributes)
                    {
                        attr.Add(a.IsOn ? a.AttributeTypeID : -1 * a.AttributeTypeID);
                    }
                    result.AttributeIds = attr;
                }
                if (gc.Latitude != null) result.Lat = (double)gc.Latitude;
                if (gc.Longitude != null) result.Lon = (double)gc.Longitude;
                if (gc.Country != null) result.Country = gc.Country;
                result.DataFromDate = DateTime.Now;
                result.Difficulty = gc.Difficulty;
                result.Terrain = gc.Terrain;
                result.Title = gc.Name;
                if (gc.FavoritePoints != null)
                {
                    result.Favorites = (int)gc.FavoritePoints;
                }
                result.GeocacheType = DataAccess.GetGeocacheType(core.GeocacheTypes, (int)gc.CacheType.GeocacheTypeId);
                if (gc.LongDescription != null)
                {
                    result.LongDescription = gc.LongDescription;
                    result.LongDescriptionInHtml = gc.LongDescriptionIsHtml;
                }
                if (gc.EncodedHints != null)
                {
                    result.EncodedHints = gc.EncodedHints;
                }
                result.MemberOnly = gc.IsPremium ?? false;
                result.Owner = gc.Owner.UserName;
                if (gc.Owner.Id != null)
                {
                    result.OwnerId = gc.Owner.Id.ToString();
                }
                result.PlacedBy = gc.PlacedBy;
                result.PublishedTime = gc.UTCPlaceDate;
                if (result.ShortDescription != null)
                {
                    result.ShortDescription = gc.ShortDescription;
                    result.ShortDescriptionInHtml = gc.ShortDescriptionIsHtml;
                }
                if (gc.State == "None")
                {
                    result.State = "";
                }
                else
                {
                    result.State = gc.State;
                }
                result.PersonaleNote = gc.GeocacheNote;
                if (gc.HasbeenFoundbyUser!=null)
                {
                    result.Found = (bool)gc.HasbeenFoundbyUser;
                }
                result.Url = gc.Url;
                result.PersonaleNote = gc.GeocacheNote ?? "";

                Calculus.SetDistanceAndAngleGeocacheFromLocation(result, core.CenterLocation);
            }
            return result;
        }

        public static Framework.Data.Log Log(Framework.Interfaces.ICore core, LiveV6.GeocacheLog lg)
        {
            Framework.Data.Log result = null;
            if (lg != null)
            {
                Framework.Data.Log tmp = DataAccess.GetLog(core.Logs, lg.Code);
                result = new Framework.Data.Log();
                if (tmp != null)
                {
                    result.UpdateFrom(tmp);
                }
                result.DataFromDate = DateTime.Now;
                result.Date = lg.VisitDate;
                result.Encoded = lg.LogIsEncoded;
                result.Finder = lg.Finder.UserName;
                result.FinderId = lg.Finder.Id.ToString();
                result.GeocacheCode = lg.CacheCode;
                result.ID = lg.Code;
                result.LogType = DataAccess.GetLogType(core.LogTypes, (int)lg.LogType.WptLogTypeId);
                result.Text = lg.LogText;
            }
            return result;
        }

        public static Framework.Data.LogImage LogImage(Framework.Interfaces.ICore core, LiveV6.ImageData img, string LogId)
        {
            Framework.Data.LogImage result = null;
            if (img != null)
            {
                Framework.Data.LogImage tmp = DataAccess.GetLogImage(core.LogImages, img.Url);
                result = new Framework.Data.LogImage();
                if (tmp != null)
                {
                    result.UpdateFrom(tmp);
                }
                result.ID = img.Url;
                result.DataFromDate = DateTime.Now;
                result.LogID = LogId;
                result.Name = img.Name;
                result.Url = img.Url;
            }
            return result;
        }
        public static Framework.Data.Waypoint Waypoint(Framework.Interfaces.ICore core, LiveV6.AdditionalWaypoint wp)
        {
            Framework.Data.Waypoint result = null;
            if (wp != null)
            {
                Framework.Data.Waypoint tmp = DataAccess.GetWaypoint(core.Waypoints, wp.Code);
                result = new Framework.Data.Waypoint();
                if (tmp != null)
                {
                    result.UpdateFrom(tmp);
                }
                result.Code = wp.Code;
                result.DataFromDate = DateTime.Now;
                result.WPType = DataAccess.GetWaypointType(core.WaypointTypes, wp.WptTypeID);
                result.Comment = wp.Comment;
                result.Description = wp.Description;
                result.GeocacheCode = wp.GeocacheCode;
                result.ID = wp.Code;
                result.Lat = wp.Latitude;
                result.Lon = wp.Longitude;
                result.Name = wp.Name;
                result.Time = wp.UTCEnteredDate;
                result.Url = wp.Url;
                result.UrlName = wp.UrlName;
            }
            return result;
        }

        public static Framework.Data.UserWaypoint UserWaypoint(Framework.Interfaces.ICore core, LiveV6.UserWaypoint wp)
        {
            Framework.Data.UserWaypoint result = null;
            if (wp != null)
            {
                Framework.Data.UserWaypoint tmp = DataAccess.GetUserWaypoint(core.UserWaypoints, (int)wp.ID);
                result = new Framework.Data.UserWaypoint();
                if (tmp != null)
                {
                    result.UpdateFrom(tmp);
                }
                result.Description = wp.Description;
                result.GeocacheCode = wp.CacheCode;
                result.ID = (int)wp.ID;
                result.Lat = wp.Latitude;
                result.Lon = wp.Longitude;
                result.Date = wp.UTCDate.ToLocalTime();
            }
            return result;
        }

        public static Framework.Data.GeocacheImage GeocacheImage(Framework.Interfaces.ICore core, LiveV6.ImageData img, string GeocacheCode)
        {
            Framework.Data.GeocacheImage result = null;
            if (img != null)
            {
                Framework.Data.GeocacheImage tmp = DataAccess.GetGeocacheImage(core.GeocacheImages, img.ImageGuid.ToString());
                result = new Framework.Data.GeocacheImage();
                if (tmp != null)
                {
                    result.UpdateFrom(tmp);
                }
                result.ID = img.ImageGuid.ToString();
                result.DataFromDate = DateTime.Now;
                result.GeocacheCode = GeocacheCode;
                result.Name = img.Name;
                result.Url = img.Url;
                result.ThumbUrl = img.ThumbUrl;
                result.MobileUrl = img.MobileUrl;
                result.Description = img.Description;
            }
            return result;
        }

    }
}
