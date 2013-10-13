using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalcachingApplication.Utils;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public class Convert
    {
        public static Framework.Data.Geocache Geocache(Framework.Interfaces.ICore core, OKAPIService.Geocache gc)
        {
            Framework.Data.Geocache result = null;
            if (gc != null)
            {
                Framework.Data.Geocache tmp = Utils.DataAccess.GetGeocache(core.Geocaches, gc.code);
                result = new Framework.Data.Geocache();
                if (tmp != null)
                {
                    result.UpdateFrom(tmp, null);
                }
                result.DataFromDate = DateTime.Now;
                result.Code = gc.code;
                result.ID = gc.code;
                result.Archived = gc.status == "Archived";
                result.Available = gc.status == "Available";

                List<int> attris = new List<int>();
                foreach (string at in gc.attr_acodes)
                {
                    int code = OKAPIService.MapAttributeACodeToAttributeID(at);
                    if (code > 0)
                    {
                        attris.Add(code);
                    }
                }
                result.AttributeIds = attris;

                // 'none', 'nano', 'micro', 'small', 'regular', 'large', 'xlarge', 'other'
                // we chose to map the containers
                if (gc.size2 == "none")
                {
                    result.Container = Utils.DataAccess.GetGeocacheContainer(core.GeocacheContainers, 5);
                }
                else if (gc.size2 == "micro" || gc.size2 == "nano")
                {
                    result.Container = Utils.DataAccess.GetGeocacheContainer(core.GeocacheContainers, 2);
                }
                else if (gc.size2 == "small")
                {
                    result.Container = Utils.DataAccess.GetGeocacheContainer(core.GeocacheContainers, 8);
                }
                else if (gc.size2 == "regular")
                {
                    result.Container = Utils.DataAccess.GetGeocacheContainer(core.GeocacheContainers, 3);
                }
                else if (gc.size2 == "large" || gc.size2 == "xlarge")
                {
                    result.Container = Utils.DataAccess.GetGeocacheContainer(core.GeocacheContainers, 4);
                }
                else
                {
                    result.Container = Utils.DataAccess.GetGeocacheContainer(core.GeocacheContainers, 6);
                }
                result.Country = gc.country ?? "";
                result.Difficulty = gc.difficulty;
                result.EncodedHints = gc.hint2;
                result.Found = gc.is_found;
                if (gc.type.ToLower().Contains("traditional"))
                {
                    result.GeocacheType = Utils.DataAccess.GetGeocacheType(core.GeocacheTypes, 96001);
                }
                else if (gc.type.ToLower().Contains("multi"))
                {
                    result.GeocacheType = Utils.DataAccess.GetGeocacheType(core.GeocacheTypes, 96002);
                }
                else if (gc.type.ToLower().Contains("quiz") || gc.type.ToLower().Contains("puzzle"))
                {
                    result.GeocacheType = Utils.DataAccess.GetGeocacheType(core.GeocacheTypes, 96008);
                }
                else if (gc.type.ToLower().Contains("virtual"))
                {
                    result.GeocacheType = Utils.DataAccess.GetGeocacheType(core.GeocacheTypes, 96003);
                }
                else if (gc.type.ToLower().Contains("event"))
                {
                    result.GeocacheType = Utils.DataAccess.GetGeocacheType(core.GeocacheTypes, 96004);
                }
                else if (gc.type.ToLower().Contains("webcam"))
                {
                    result.GeocacheType = Utils.DataAccess.GetGeocacheType(core.GeocacheTypes, 96006);
                }
                else if (gc.type.ToLower().Contains("location") || gc.type.Contains("moving"))
                {
                    result.GeocacheType = Utils.DataAccess.GetGeocacheType(core.GeocacheTypes, 96007);
                }
                else
                {
                    result.GeocacheType = Utils.DataAccess.GetGeocacheType(core.GeocacheTypes, 96005);
                }
                result.Lat = Utils.Conversion.StringToDouble(gc.location.Substring(0, gc.location.IndexOf('|')));
                result.Lon = Utils.Conversion.StringToDouble(gc.location.Substring(gc.location.IndexOf('|') + 1));
                result.LongDescription = gc.description;
                result.LongDescriptionInHtml = true;
                result.Name = gc.name;
                result.Owner = gc.owner.username;
                result.OwnerId = gc.owner.uuid;
                result.PlacedBy = gc.owner.username;
                result.PublishedTime = DateTime.Parse(gc.date_hidden);
                result.ShortDescription = "";
                result.ShortDescriptionInHtml = true;
                result.State = gc.state ?? "";
                result.Terrain = gc.terrain;
                result.Url = gc.url ?? "";

                Calculus.SetDistanceAndAngleGeocacheFromLocation(result, core.CenterLocation);
            }
            return result;
        }


        public static Framework.Data.Log Log(Framework.Interfaces.ICore core, OKAPIService.Log lg, string finder, string finderId)
        {
            Framework.Data.Log result = null;
            if (lg != null)
            {
                Framework.Data.Log tmp = Utils.DataAccess.GetLog(core.Logs, lg.cache_code);
                result = new Framework.Data.Log();
                if (tmp != null)
                {
                    result.UpdateFrom(tmp);
                }
                result.DataFromDate = DateTime.Now;
                result.Date = DateTime.Parse(lg.date);
                result.Encoded = false;
                if (lg.user == null)
                {
                    result.Finder = finder;
                    result.FinderId = finderId;
                }
                else
                {
                    result.Finder = lg.user.username;
                    result.FinderId = lg.user.uuid;
                }
                result.GeocacheCode = lg.cache_code;
                result.ID = lg.uuid;
                result.Text = lg.comment;
                result.LogType = Utils.DataAccess.GetLogType(core.LogTypes, lg.type);
            }
            return result;
        }

        public static Framework.Data.Waypoint Waypoint(Framework.Interfaces.ICore core, OKAPIService.Waypoint wp)
        {
            Framework.Data.Waypoint result = null;
            if (wp != null)
            {
                Framework.Data.Waypoint tmp = DataAccess.GetWaypoint(core.Waypoints, wp.name);
                result = new Framework.Data.Waypoint();
                if (tmp != null)
                {
                    result.UpdateFrom(tmp);
                }
                result.Code = wp.name;
                result.DataFromDate = DateTime.Now;
                //for now map: parking, path, stage, physical-stage, virtual-stage, final, poi, other
                if (wp.sym == "parking")
                {
                    result.WPType = DataAccess.GetWaypointType(core.WaypointTypes, 217);
                }
                else if (wp.sym == "path")
                {
                    result.WPType = DataAccess.GetWaypointType(core.WaypointTypes, 452);
                }
                else if (wp.sym == "stage" || wp.sym == "physical-stage" || wp.sym == "virtual-stage")
                {
                    result.WPType = DataAccess.GetWaypointType(core.WaypointTypes, 219);
                }
                else if (wp.sym == "final")
                {
                    result.WPType = DataAccess.GetWaypointType(core.WaypointTypes, 220);
                }
                else if (wp.sym == "poi")
                {
                    result.WPType = DataAccess.GetWaypointType(core.WaypointTypes, 452);
                }
                else
                {
                    result.WPType = DataAccess.GetWaypointType(core.WaypointTypes, 452);
                }
                result.Comment = wp.description;
                result.Description = wp.description;
                result.GeocacheCode = wp.cache_code;
                result.ID = wp.name;
                result.Lat = Utils.Conversion.StringToDouble(wp.location.Substring(0, wp.location.IndexOf('|')));
                result.Lon = Utils.Conversion.StringToDouble(wp.location.Substring(wp.location.IndexOf('|') + 1));
                result.Name = wp.type_name;
                result.Time = DateTime.Now;
                result.Url = "";
                result.UrlName = wp.type_name;
            }
            return result;
        }

        public static Framework.Data.GeocacheImage GeocacheImage(Framework.Interfaces.ICore core, OKAPIService.GeocacheImage img, string GeocacheCode)
        {
            Framework.Data.GeocacheImage result = null;
            if (img != null)
            {
                Framework.Data.GeocacheImage tmp = DataAccess.GetGeocacheImage(core.GeocacheImages, img.uuid);
                result = new Framework.Data.GeocacheImage();
                if (tmp != null)
                {
                    result.UpdateFrom(tmp);
                }
                result.ID = img.uuid;
                result.DataFromDate = DateTime.Now;
                result.GeocacheCode = GeocacheCode;
                result.Name = img.caption;
                result.Url = img.url;
                result.ThumbUrl = img.thumb_url;
                result.MobileUrl = img.thumb_url;
                result.Description = "";
            }
            return result;
        }

    }
}
