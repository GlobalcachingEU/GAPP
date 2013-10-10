using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Interfaces
{
    public interface IImageResource
    {
        string GetImagePath(Data.ImageSize imageSize, Data.GeocacheType geocacheType);
        string GetImagePath(Data.ImageSize imageSize, Data.GeocacheType geocacheType, bool corrected);
        string GetImagePath(Data.ImageSize imageSize, Data.GeocacheAttribute attr, Data.GeocacheAttribute.State state);
        string GetImagePath(Data.ImageSize imageSize, Data.LogType logType);
        string GetImagePath(Data.ImageSize imageSize, Data.WaypointType waypointType);
        string GetImagePath(Data.ImageSize imageSize, Data.GeocacheContainer container);
        string GetImagePath(Data.ImageSize imageSize, Data.CompassDirection compassDir);

        bool GrabImages(Data.Geocache gc);
        string GetImagePath(string orgUrl);
        string GetImagePath(Data.Geocache gc, string orgUrl);
    }
}
