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
                result.Code = gc.code;
                result.ID = gc.code;
                result.Archived = gc.status == "Archived";
                result.Available = gc.status == "Available";

                //todo: result.AttributeIds

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
                //for now, we just map the geocache types
                //in the future we might want seperate icons
                if (gc.type == "Traditional")
                {
                    result.GeocacheType = Utils.DataAccess.GetGeocacheType(core.GeocacheTypes, 2);
                }
                else if (gc.type == "Multi")
                {
                    result.GeocacheType = Utils.DataAccess.GetGeocacheType(core.GeocacheTypes, 3);
                }
                else if (gc.type == "Quiz")
                {
                    result.GeocacheType = Utils.DataAccess.GetGeocacheType(core.GeocacheTypes, 8);
                }
                else if (gc.type == "Virtual")
                {
                    result.GeocacheType = Utils.DataAccess.GetGeocacheType(core.GeocacheTypes, 4);
                }
                else if (gc.type == "Event")
                {
                    result.GeocacheType = Utils.DataAccess.GetGeocacheType(core.GeocacheTypes, 6);
                }
                else
                {
                    result.GeocacheType = Utils.DataAccess.GetGeocacheType(core.GeocacheTypes, 8);
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
    }
}
