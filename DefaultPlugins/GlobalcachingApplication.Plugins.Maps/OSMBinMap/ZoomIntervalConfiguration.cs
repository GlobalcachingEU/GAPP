using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.Maps.OSMBinMap
{
    public class ZoomIntervalConfiguration
    {
        public int BaseZoomLevel { get; private set; }
        public int MinZoomLevel { get; private set; }
        public int MaxZoomLevel { get; private set; }
        public long AbsStartPositionSubFile { get; private set; }
        public long SubFileSize { get; private set; }

        public ZoomIntervalConfiguration(int bzl, int minzl, int maxzl, long absStartPos, long filesize)
        {
            BaseZoomLevel = bzl;
            MinZoomLevel = minzl;
            MaxZoomLevel = maxzl;
            AbsStartPositionSubFile = absStartPos;
            SubFileSize = filesize;
        }
    }
}
