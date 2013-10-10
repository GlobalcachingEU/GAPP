using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GlobalcachingApplication.Plugins.Maps.OSMBinMap
{
    public class RequestedTileInformation
    {
        public MapFile mapFile { get; set; }

        //static overall properties
        public long X { get; set; }
        public long Y { get; set; }
        public int Zoom { get; set; }
        public ZoomIntervalConfiguration ZoomIntervalConfig { get; set; }
        public bool UseSubTileBitmapMask { get; set; }
        public int SubTileBitmapMask { get; set; }
        public long FromBaseTileX { get; set; }
        public long FromBaseTileY { get; set; }
        public long ToBaseTileX { get; set; }
        public long ToBaseTileY { get; set; }
        public long FromBlockX { get; set; }
        public long FromBlockY { get; set; }
        public long ToBlockX { get; set; }
        public long ToBlockY { get; set; }

        //dynamic data per base tile
        public byte[] TileDataBuffer { get; set; }
        public MemoryStream TileDataBufferStream { get; set; }
        public double TileLatitudeDeg { get; set; }
        public double TileLongitudeDeg { get; set; }
        public int TileLatitude { get; set; }
        public int TileLongitude { get; set; }
        public int[,] ZoomTable { get; set; }
        public int ZoomTableRow { get; set; }
        public int PoisOnQueryZoomLevel { get; set; }
        public int WaysOnQueryZoomLevel { get; set; }
        public long FirstWayOffset { get; set; }

        public List<POI> POIs { get; set; }
        public List<Way> Ways { get; set; }
    }
}
