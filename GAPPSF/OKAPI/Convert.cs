using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GAPPSF.Utils;

namespace GAPPSF.OKAPI
{
    public class Convert
    {
        public static Core.Data.Geocache Geocache(Core.Storage.Database db, OKAPIService.Geocache gc)
        {
            Core.Data.Geocache result = null;
            if (gc != null)
            {
                result = db.GeocacheCollection.GetGeocache(gc.code);

                Core.Data.IGeocacheData gcData;
                if (result == null)
                {
                    gcData = new Core.Data.GeocacheData();
                    gcData.Code = gc.code;
                }
                else
                {
                    gcData = result;
                }

                gcData.Archived = gc.status == "Archived";
                gcData.Available = gc.status == "Available";

                if (result == null || !result.Locked)
                {
                    gcData.DataFromDate = DateTime.Now;

                    List<int> attris = new List<int>();
                    foreach (string at in gc.attr_acodes)
                    {
                        int code = OKAPIService.MapAttributeACodeToAttributeID(at);
                        if (code > 0)
                        {
                            attris.Add(code);
                        }
                    }
                    gcData.AttributeIds = attris;

                    // 'none', 'nano', 'micro', 'small', 'regular', 'large', 'xlarge', 'other'
                    // we chose to map the containers
                    if (gc.size2 == "none")
                    {
                        result.Container = Utils.DataAccess.GetGeocacheContainer(5);
                    }
                    else if (gc.size2 == "micro" || gc.size2 == "nano")
                    {
                        result.Container = Utils.DataAccess.GetGeocacheContainer(2);
                    }
                    else if (gc.size2 == "small")
                    {
                        result.Container = Utils.DataAccess.GetGeocacheContainer(8);
                    }
                    else if (gc.size2 == "regular")
                    {
                        result.Container = Utils.DataAccess.GetGeocacheContainer(3);
                    }
                    else if (gc.size2 == "large" || gc.size2 == "xlarge")
                    {
                        result.Container = Utils.DataAccess.GetGeocacheContainer(4);
                    }
                    else
                    {
                        result.Container = Utils.DataAccess.GetGeocacheContainer(6);
                    }
                    gcData.Country = gc.country ?? "";
                    gcData.Difficulty = gc.difficulty;
                    gcData.EncodedHints = gc.hint2;
                    gcData.Found = gc.is_found;
                    if (gc.type.ToLower().Contains("traditional"))
                    {
                        gcData.GeocacheType = Utils.DataAccess.GetGeocacheType(96001);
                    }
                    else if (gc.type.ToLower().Contains("multi"))
                    {
                        gcData.GeocacheType = Utils.DataAccess.GetGeocacheType(96002);
                    }
                    else if (gc.type.ToLower().Contains("quiz") || gc.type.ToLower().Contains("puzzle"))
                    {
                        gcData.GeocacheType = Utils.DataAccess.GetGeocacheType(96008);
                    }
                    else if (gc.type.ToLower().Contains("virtual"))
                    {
                        gcData.GeocacheType = Utils.DataAccess.GetGeocacheType(96003);
                    }
                    else if (gc.type.ToLower().Contains("event"))
                    {
                        gcData.GeocacheType = Utils.DataAccess.GetGeocacheType(96004);
                    }
                    else if (gc.type.ToLower().Contains("webcam"))
                    {
                        gcData.GeocacheType = Utils.DataAccess.GetGeocacheType(96006);
                    }
                    else if (gc.type.ToLower().Contains("location") || gc.type.Contains("moving"))
                    {
                        gcData.GeocacheType = Utils.DataAccess.GetGeocacheType(96007);
                    }
                    else
                    {
                        gcData.GeocacheType = Utils.DataAccess.GetGeocacheType(96005);
                    }
                    gcData.Lat = Utils.Conversion.StringToDouble(gc.location.Substring(0, gc.location.IndexOf('|')));
                    gcData.Lon = Utils.Conversion.StringToDouble(gc.location.Substring(gc.location.IndexOf('|') + 1));
                    gcData.LongDescription = gc.description;
                    gcData.LongDescriptionInHtml = true;
                    gcData.Name = gc.name;
                    gcData.Owner = gc.owner.username;
                    gcData.OwnerId = gc.owner.uuid;
                    gcData.PlacedBy = gc.owner.username;
                    gcData.PublishedTime = DateTime.Parse(gc.date_hidden);
                    gcData.ShortDescription = "";
                    gcData.ShortDescriptionInHtml = true;
                    gcData.State = gc.state ?? "";
                    gcData.Terrain = gc.terrain;
                    gcData.Url = gc.url ?? "";

                    if (gcData is Core.Data.GeocacheData)
                    {
                        if (Utils.DataAccess.AddGeocache(db, gcData as Core.Data.GeocacheData))
                        {
                            result = db.GeocacheCollection.GetGeocache(gcData.Code);
                        }
                        else
                        {
                            result = null;
                        }
                    }
                    if (result != null)
                    {
                        Calculus.SetDistanceAndAngleGeocacheFromLocation(result, Core.ApplicationData.Instance.CenterLocation);
                    }
                }
            }
            return result;
        }


        public static Core.Data.Log Log(Core.Storage.Database db, OKAPIService.Log lg, string finder, string finderId)
        {
            Core.Data.Log result = null;
            if (lg != null)
            {
                result = db.LogCollection.GetLog(lg.uuid);

                Core.Data.ILogData lgData;
                if (result == null)
                {
                    lgData = new Core.Data.LogData();
                    lgData.ID = lg.uuid;
                }
                else
                {
                    lgData = result;
                }

                lgData.DataFromDate = DateTime.Now;
                lgData.Date = DateTime.Parse(lg.date);
                lgData.Encoded = false;
                if (lg.user == null)
                {
                    lgData.Finder = finder;
                    lgData.FinderId = finderId;
                }
                else
                {
                    lgData.Finder = lg.user.username;
                    lgData.FinderId = lg.user.uuid;
                }
                lgData.GeocacheCode = lg.cache_code;
                lgData.Text = lg.comment;
                lgData.LogType = Utils.DataAccess.GetLogType(lg.type);

                if (lgData is Core.Data.LogData)
                {
                    if (Utils.DataAccess.AddLog(db, lgData as Core.Data.LogData))
                    {
                        result = db.LogCollection.GetLog(lgData.ID);
                    }
                }

            }
            return result;
        }

        public static Core.Data.Waypoint Waypoint(Core.Storage.Database db, OKAPIService.Waypoint wp)
        {
            Core.Data.Waypoint result = null;
            if (wp != null)
            {
                result = db.WaypointCollection.GetWaypoint(wp.name);

                Core.Data.IWaypointData wpd;
                if (result == null)
                {
                    wpd = new Core.Data.WaypointData();
                    wpd.Code = wp.name;
                    wpd.ID = wp.name;
                }
                else
                {
                    wpd = result;
                }

                wpd.DataFromDate = DateTime.Now;
                //for now map: parking, path, stage, physical-stage, virtual-stage, final, poi, other
                if (wp.sym == "parking")
                {
                    wpd.WPType = DataAccess.GetWaypointType(217);
                }
                else if (wp.sym == "path")
                {
                    wpd.WPType = DataAccess.GetWaypointType(452);
                }
                else if (wp.sym == "stage" || wp.sym == "physical-stage" || wp.sym == "virtual-stage")
                {
                    result.WPType = DataAccess.GetWaypointType(219);
                }
                else if (wp.sym == "final")
                {
                    wpd.WPType = DataAccess.GetWaypointType(220);
                }
                else if (wp.sym == "poi")
                {
                    wpd.WPType = DataAccess.GetWaypointType(452);
                }
                else
                {
                    wpd.WPType = DataAccess.GetWaypointType(452);
                }
                wpd.Comment = wp.description;
                wpd.Description = wp.description;
                wpd.GeocacheCode = wp.cache_code;
                wpd.Lat = Utils.Conversion.StringToDouble(wp.location.Substring(0, wp.location.IndexOf('|')));
                wpd.Lon = Utils.Conversion.StringToDouble(wp.location.Substring(wp.location.IndexOf('|') + 1));
                wpd.Name = wp.type_name;
                wpd.Time = DateTime.Now;
                wpd.Url = "";
                wpd.UrlName = wp.type_name;

                if (wpd is Core.Data.WaypointData)
                {
                    if (Utils.DataAccess.AddWaypoint(db, wpd as Core.Data.WaypointData))
                    {
                        result = db.WaypointCollection.GetWaypoint(wpd.Code);
                    }
                }

            }
            return result;
        }

        public static Core.Data.GeocacheImage GeocacheImage(Core.Storage.Database db, OKAPIService.GeocacheImage img, string GeocacheCode)
        {
            Core.Data.GeocacheImage result = null;
            if (img != null)
            {
                result = db.GeocacheImageCollection.GetGeocacheImage(img.uuid);

                Core.Data.IGeocacheImageData wpd;
                if (result == null)
                {
                    wpd = new Core.Data.GeocacheImageData();
                    wpd.ID = img.uuid;
                }
                else
                {
                    wpd = result;
                }

                wpd.DataFromDate = DateTime.Now;
                wpd.GeocacheCode = GeocacheCode;
                wpd.Name = img.caption;
                wpd.Url = img.url;
                wpd.ThumbUrl = img.thumb_url;
                wpd.MobileUrl = img.thumb_url;
                wpd.Description = "";

                if (wpd is Core.Data.GeocacheImageData)
                {
                    if (Utils.DataAccess.AddGeocacheImage(db, wpd as Core.Data.GeocacheImageData))
                    {
                        result = db.GeocacheImageCollection.GetGeocacheImage(img.uuid);
                    }
                }

            }
            return result;
        }

    }
}
