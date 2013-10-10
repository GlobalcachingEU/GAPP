using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalcachingApplication.Framework.Data;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions.CoordinateFunctions
{
    public class Waypoint: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            ArgumentChecker checker = new ArgumentChecker(ctx, this.GetType().Name);
            if (ctx.Core.ActiveGeocache != null)
            {
                string codeToSearch = (args.Length == 0)
                    ? ctx.Core.ActiveGeocache.Code
                    : args[0].ToString();

                if (codeToSearch.Length == 0)
                {
                    codeToSearch = ctx.Core.ActiveGeocache.Code;
                }

                // Geocache Code
                if (codeToSearch == ctx.Core.ActiveGeocache.Code)
                {
                    return GetPostion(ctx.Core.ActiveGeocache);
                }

                // Waypoints
                var wpt = Utils.DataAccess.GetWaypointsFromGeocache(ctx.Core.Waypoints, ctx.Core.ActiveGeocache.Code)
                    .FirstOrDefault(w => w.Code == args[0].ToString());
                if ((wpt != null) && (wpt.Lat != null) && (wpt.Lon != null))
                {
                    return GetPosition(wpt);
                }

                // UserWaypoints
            }
            return "";
        }

        private object GetPosition(Framework.Data.Waypoint wpt)
        {
            double lat, lon;
            if (wpt.Lat != null)
            {
                lat = (double)wpt.Lat;
            }
            else
            {
                return "";
            }
            if (wpt.Lon != null)
            {
                lon = (double)wpt.Lon;
            }
            else
            {
                return "";
            }
            return Utils.Conversion.GetCoordinatesPresentation(lat, lon);
        }

        private string GetPostion(Geocache geocache)
        {
            double lat, lon;
            if (geocache.CustomLat != null)
            {
                lat = (double)geocache.CustomLat;
            }
            else if (geocache.Lat != null)
            {
                lat = (double)geocache.Lat;
            }
            else
            {
                return "";
            }

            if (geocache.CustomLon != null)
            {
                lon = (double)geocache.CustomLon;
            }
            else if (geocache.Lon != null)
            {
                lon = (double)geocache.Lon;
            }
            else
            {
                return "";
            }
            return Utils.Conversion.GetCoordinatesPresentation(lat, lon);
        }


    }
}
