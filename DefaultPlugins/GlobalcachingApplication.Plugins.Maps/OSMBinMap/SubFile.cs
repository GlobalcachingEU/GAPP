using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace GlobalcachingApplication.Plugins.Maps.OSMBinMap
{
    public class SubFile
    {
        private MapFile _mapFile;
        private long[] _tileAbsOffsets = null;

        public ZoomIntervalConfiguration ZoomIntervalConfig { get; private set; }
        public string TileIndexSignature { get; private set; }
        public long MinX { get; private set; }
        public long MinY { get; private set; }
        public long MaxX { get; private set; }
        public long MaxY { get; private set; }

        public SubFile(MapFile mapfile, ZoomIntervalConfiguration zoonIntervalConfig)
        {
            _mapFile = mapfile;
            ZoomIntervalConfig = zoonIntervalConfig;
        }

        public void ReadHeader()
        {
            List<long> tileOffsets = new List<long>();

            _mapFile.StreamReader.Position = ZoomIntervalConfig.AbsStartPositionSubFile;
            MinX = (int)MapFile.GetTileX((double)_mapFile.Header.MinLon, ZoomIntervalConfig.BaseZoomLevel);
            MaxX = (int)MapFile.GetTileX((double)_mapFile.Header.MaxLon, ZoomIntervalConfig.BaseZoomLevel);
            MinY = (int)MapFile.GetTileY((double)_mapFile.Header.MaxLat, ZoomIntervalConfig.BaseZoomLevel);
            MaxY = (int)MapFile.GetTileY((double)_mapFile.Header.MinLat, ZoomIntervalConfig.BaseZoomLevel);
            if (_mapFile.Header.DebugInformationPresent)
            {
                TileIndexSignature = _mapFile.ReadFixedString(16);
            }
            else
            {
                TileIndexSignature = null;
            }
            long hTiles = MaxX - MinX + 1;
            long vTiles = MaxY - MinY + 1;
            long blockCount = hTiles * vTiles;
            long endOfIndex = ZoomIntervalConfig.AbsStartPositionSubFile + (blockCount * 5);
            //tile entries
            byte[] buffer = new byte[8];
            while (_mapFile.StreamReader.Position < endOfIndex)
            {
                Array.Clear(buffer,0, 8);
                _mapFile.StreamReader.Read(buffer, 3, 5);
                long offset;
                if ((buffer[3] & 0x80) == 0x80)
                {
                    //in water
                    offset = 0;
                }
                else
                {
                    offset = BitConverter.ToInt64(MapFile.ReverseBytes(buffer, 0, 8), 0);
                }
                tileOffsets.Add(offset);
            }
            _tileAbsOffsets = tileOffsets.ToArray();
        }

        public RequestedTileInformation GetTileInformation(long x, long y, int zoom)
        {
            RequestedTileInformation ti = new RequestedTileInformation();
            try
            {
                ti.mapFile = _mapFile;
                ti.X = x;
                ti.Y = y;
                ti.Zoom = zoom;
                ti.ZoomIntervalConfig = this.ZoomIntervalConfig;

                if (zoom < ZoomIntervalConfig.BaseZoomLevel)
                {
                    // calculate the XY numbers of the upper left and lower right sub-tiles
                    int zoomLevelDifference = ZoomIntervalConfig.BaseZoomLevel - zoom;
                    ti.FromBaseTileX = x << zoomLevelDifference;
                    ti.FromBaseTileY = y << zoomLevelDifference;
                    ti.ToBaseTileX = ti.FromBaseTileX + (1 << zoomLevelDifference) - 1;
                    ti.ToBaseTileY = ti.FromBaseTileY + (1 << zoomLevelDifference) - 1;
                    ti.UseSubTileBitmapMask = false;
                }
                else if (zoom > ZoomIntervalConfig.BaseZoomLevel)
                {
                    // calculate the XY numbers of the parent base tile
                    int zoomLevelDifference = zoom - ZoomIntervalConfig.BaseZoomLevel;
                    ti.FromBaseTileX = x >> zoomLevelDifference;
                    ti.FromBaseTileY = y >> zoomLevelDifference;
                    ti.ToBaseTileX = ti.FromBaseTileX;
                    ti.ToBaseTileY = ti.FromBaseTileY;
                    ti.UseSubTileBitmapMask = true;

                    //calculate ti.SubTileBitmapMask
                    if (zoomLevelDifference == 1)
                    {
                        if (x % 2 == 0 && y % 2 == 0)
                        {
                            // upper left quadrant
                            ti.SubTileBitmapMask = 0xcc00;
                        }
                        else if (x % 2 == 1 && y % 2 == 0)
                        {
                            // upper right quadrant
                            ti.SubTileBitmapMask = 0x3300;
                        }
                        else if (x % 2 == 0 && y % 2 == 1)
                        {
                            // lower left quadrant
                            ti.SubTileBitmapMask = 0xcc;
                        }
                        else
                        {
                            // lower right quadrant
                            ti.SubTileBitmapMask = 0x33;
                        }
                    }
                    else
                    {
                        // calculate the XY numbers of the second level sub-tile
                        long subtileX = x >> (zoomLevelDifference - 2);
                        long subtileY = y >> (zoomLevelDifference - 2);

                        // calculate the XY numbers of the parent tile
                        long parentTileX = subtileX >> 1;
                        long parentTileY = subtileY >> 1;

                        // determine the correct bitmask for all 16 sub-tiles
                        if (parentTileX % 2 == 0 && parentTileY % 2 == 0)
                        {
                            if (subtileX % 2 == 0 && subtileY % 2 == 0)
                            {
                                // upper left sub-tile
                                ti.SubTileBitmapMask = 0x8000;
                            }
                            else if (subtileX % 2 == 1 && subtileY % 2 == 0)
                            {
                                // upper right sub-tile
                                ti.SubTileBitmapMask = 0x4000;
                            }
                            else if (subtileX % 2 == 0 && subtileY % 2 == 1)
                            {
                                // lower left sub-tile
                                ti.SubTileBitmapMask = 0x800;
                            }
                            else
                            {
                                // lower right sub-tile
                                ti.SubTileBitmapMask = 0x400;
                            }
                        }
                        else if (parentTileX % 2 == 1 && parentTileY % 2 == 0)
                        {
                            if (subtileX % 2 == 0 && subtileY % 2 == 0)
                            {
                                // upper left sub-tile
                                ti.SubTileBitmapMask = 0x2000;
                            }
                            else if (subtileX % 2 == 1 && subtileY % 2 == 0)
                            {
                                // upper right sub-tile
                                ti.SubTileBitmapMask = 0x1000;
                            }
                            else if (subtileX % 2 == 0 && subtileY % 2 == 1)
                            {
                                // lower left sub-tile
                                ti.SubTileBitmapMask = 0x200;
                            }
                            else
                            {
                                // lower right sub-tile
                                ti.SubTileBitmapMask = 0x100;
                            }
                        }
                        else if (parentTileX % 2 == 0 && parentTileY % 2 == 1)
                        {
                            if (subtileX % 2 == 0 && subtileY % 2 == 0)
                            {
                                // upper left sub-tile
                                ti.SubTileBitmapMask = 0x80;
                            }
                            else if (subtileX % 2 == 1 && subtileY % 2 == 0)
                            {
                                // upper right sub-tile
                                ti.SubTileBitmapMask = 0x40;
                            }
                            else if (subtileX % 2 == 0 && subtileY % 2 == 1)
                            {
                                // lower left sub-tile
                                ti.SubTileBitmapMask = 0x8;
                            }
                            else
                            {
                                // lower right sub-tile
                                ti.SubTileBitmapMask = 0x4;
                            }
                        }
                        else
                        {
                            if (subtileX % 2 == 0 && subtileY % 2 == 0)
                            {
                                // upper left sub-tile
                                ti.SubTileBitmapMask = 0x20;
                            }
                            else if (subtileX % 2 == 1 && subtileY % 2 == 0)
                            {
                                // upper right sub-tile
                                ti.SubTileBitmapMask = 0x10;
                            }
                            else if (subtileX % 2 == 0 && subtileY % 2 == 1)
                            {
                                // lower left sub-tile
                                ti.SubTileBitmapMask = 0x2;
                            }
                            else
                            {
                                // lower right sub-tile
                                ti.SubTileBitmapMask = 0x1;
                            }
                        }
                    }

                }
                else
                {
                    // use the tile XY numbers of the requested tile
                    ti.FromBaseTileX = x;
                    ti.FromBaseTileY = y;
                    ti.ToBaseTileX = ti.FromBaseTileX;
                    ti.ToBaseTileY = ti.FromBaseTileY;
                    ti.UseSubTileBitmapMask = false;
                }

                ti.FromBlockX = Math.Max(ti.FromBaseTileX - MinX, 0);
                ti.FromBlockY = Math.Max(ti.FromBaseTileY - MinY, 0);
                ti.ToBlockX = Math.Min(ti.ToBaseTileX - MinX, MaxX);
                ti.ToBlockY = Math.Min(ti.ToBaseTileY - MinY, MaxY);

                ti.POIs = new List<POI>();
                ti.Ways = new List<Way>();
                for (long row = ti.FromBlockY; row <= ti.ToBlockY; row++)
                {
                    for (long column = ti.FromBlockX; column <= ti.ToBlockX; column++)
                    {
                        // calculate the actual block number of the needed block in the file
                        long blockNumber = (row * (MaxX - MinX + 1)) + column;
                        if (blockNumber >= _tileAbsOffsets.Length || _tileAbsOffsets[blockNumber] == 0 || _tileAbsOffsets[blockNumber] >= ZoomIntervalConfig.SubFileSize)
                        {
                            continue;
                        }

                        long blockSize;
                        if (blockNumber < _tileAbsOffsets.Length - 1)
                        {
                            blockSize = _tileAbsOffsets[blockNumber + 1] - _tileAbsOffsets[blockNumber];
                        }
                        else
                        {
                            blockSize = ZoomIntervalConfig.SubFileSize - _tileAbsOffsets[blockNumber] + 1;
                        }
                        if (blockSize < 1)
                        {
                            continue;
                        }

                        ti.TileDataBuffer = new byte[blockSize];
                        _mapFile.ReadBuffer(ti.TileDataBuffer, _tileAbsOffsets[blockNumber] + ZoomIntervalConfig.AbsStartPositionSubFile, (int)blockSize);
                        using (ti.TileDataBufferStream = new System.IO.MemoryStream(ti.TileDataBuffer))
                        {
                            // calculate the top-left coordinates of the underlying tile
                            ti.TileLatitudeDeg = MapFile.GetLatitude(MinY + row, ZoomIntervalConfig.BaseZoomLevel);
                            ti.TileLongitudeDeg = MapFile.GetLongitude(MinX + column, ZoomIntervalConfig.BaseZoomLevel);
                            ti.TileLatitude = (int)(ti.TileLatitudeDeg * 1000000);
                            ti.TileLongitude = (int)(ti.TileLongitudeDeg * 1000000);

                            if (_mapFile.Header.DebugInformationPresent)
                            {
                                ti.TileDataBufferStream.Position = 32;
                            }

                            //get the zoom table
                            int rows = ZoomIntervalConfig.MaxZoomLevel - ZoomIntervalConfig.MinZoomLevel + 1;
                            ti.ZoomTable = new int[rows, 2];

                            int cumulatedNumberOfPois = 0;
                            int cumulatedNumberOfWays = 0;

                            for (int r = 0; r < rows; r++)
                            {
                                cumulatedNumberOfPois += (int)MapFile.ReadVBEUint(ti.TileDataBufferStream);
                                cumulatedNumberOfWays += (int)MapFile.ReadVBEUint(ti.TileDataBufferStream);

                                ti.ZoomTable[r, 0] = cumulatedNumberOfPois;
                                ti.ZoomTable[r, 1] = cumulatedNumberOfWays;
                            }
                            ti.ZoomTableRow = zoom - ZoomIntervalConfig.MinZoomLevel;
                            ti.PoisOnQueryZoomLevel = ti.ZoomTable[ti.ZoomTableRow, 0];
                            ti.WaysOnQueryZoomLevel = ti.ZoomTable[ti.ZoomTableRow, 1];

                            ti.FirstWayOffset = MapFile.ReadVBEUint(ti.TileDataBufferStream);
                            ti.FirstWayOffset += ti.TileDataBufferStream.Position;

                            //get POIs
                            for (int p = 0; p < ti.PoisOnQueryZoomLevel; p++)
                            {
                                POI poi = new POI();

                                if (_mapFile.Header.DebugInformationPresent)
                                {
                                    ti.TileDataBufferStream.Position += 32;
                                }

                                //lat/lon
                                poi.Latitude = ti.TileLatitude + MapFile.ReadVBESint(ti.TileDataBufferStream);
                                poi.Longitude = ti.TileLongitude + MapFile.ReadVBESint(ti.TileDataBufferStream);

                                //layer and tag ids
                                poi.TagIDs = new List<int>();
                                byte b = (byte)ti.TileDataBufferStream.ReadByte();
                                poi.Layer = b >> 4;
                                int tagCount = (b & 0x0F);
                                for (int t = 0; t < tagCount; t++)
                                {
                                    poi.TagIDs.Add((int)MapFile.ReadVBEUint(ti.TileDataBufferStream));
                                }

                                //flags
                                b = (byte)ti.TileDataBufferStream.ReadByte();
                                if ((b & 0x80) == 0x80)
                                {
                                    poi.Name = MapFile.ReadUTF8EncodedString(ti.TileDataBufferStream);
                                }
                                else
                                {
                                    poi.Name = null;
                                }
                                if ((b & 0x40) == 0x40)
                                {
                                    poi.HouseNumber = MapFile.ReadUTF8EncodedString(ti.TileDataBufferStream);
                                }
                                else
                                {
                                    poi.HouseNumber = null;
                                }
                                if ((b & 0x20) == 0x20)
                                {
                                    poi.Elevation = MapFile.ReadVBESint(ti.TileDataBufferStream);
                                }
                                else
                                {
                                    poi.Elevation = null;
                                }

                                ti.POIs.Add(poi);
                            }

                            ti.TileDataBufferStream.Position = ti.FirstWayOffset;

                            //get ways
                            for (int w = 0; w < ti.WaysOnQueryZoomLevel; w++)
                            {
                                Way way = new Way();

                                if (_mapFile.Header.DebugInformationPresent)
                                {
                                    ti.TileDataBufferStream.Position += 32;
                                }

                                int wayDataSize = (int)MapFile.ReadVBEUint(ti.TileDataBufferStream);
                                int subTileBitmap = MapFile.ReadUInt16(ti.TileDataBufferStream);

                                if (ti.UseSubTileBitmapMask)
                                {
                                    // check if the way is inside the requested tile
                                    if ((ti.SubTileBitmapMask & subTileBitmap) == 0)
                                    {
                                        // skip the rest of the way and continue with the next way
                                        ti.TileDataBufferStream.Position += (wayDataSize - 2);
                                        continue;
                                    }
                                }

                                //layer and tags
                                way.TagIDs = new List<int>();
                                byte b = (byte)ti.TileDataBufferStream.ReadByte();
                                way.Layer = b >> 4;
                                int tagCount = (b & 0x0F);
                                for (int t = 0; t < tagCount; t++)
                                {
                                    way.TagIDs.Add((int)MapFile.ReadVBEUint(ti.TileDataBufferStream));
                                }

                                //feature bytes
                                b = (byte)ti.TileDataBufferStream.ReadByte();
                                if ((b & 0x80) == 0x80)
                                {
                                    way.Name = MapFile.ReadUTF8EncodedString(ti.TileDataBufferStream);
                                }
                                else
                                {
                                    way.Name = null;
                                }
                                if ((b & 0x40) == 0x40)
                                {
                                    way.HouseNumber = MapFile.ReadUTF8EncodedString(ti.TileDataBufferStream);
                                }
                                else
                                {
                                    way.HouseNumber = null;
                                }
                                if ((b & 0x20) == 0x20)
                                {
                                    way.Reference = MapFile.ReadUTF8EncodedString(ti.TileDataBufferStream);
                                }
                                else
                                {
                                    way.Reference = null;
                                }
                                if ((b & 0x10) == 0x10)
                                {
                                    way.LabelLatitude = ti.TileLatitude + MapFile.ReadVBESint(ti.TileDataBufferStream);
                                    way.LabelLongitude = ti.TileLongitude + MapFile.ReadVBESint(ti.TileDataBufferStream);
                                }
                                else
                                {
                                    way.LabelLatitude = null;
                                    way.LabelLongitude = null;
                                }

                                int numberOfBlocks;
                                if ((b & 0x08) == 0x08)
                                {
                                    numberOfBlocks = (int)MapFile.ReadVBEUint(ti.TileDataBufferStream);
                                }
                                else
                                {
                                    numberOfBlocks = 1;
                                }

                                bool doubleEncoding = ((b & 0x04) == 0x04);
                                way.WayDataBlocks = new List<Way.WayData>();
                                for (int wb = 0; wb < numberOfBlocks; wb++)
                                {
                                    //read: way data
                                    Way.WayData wd = new Way.WayData();

                                    int numberOfWayCoordBlocks = (int)MapFile.ReadVBEUint(ti.TileDataBufferStream);
                                    wd.DataBlock = new List<Way.WayCoordinateBlock>();
                                    for (int wcd = 0; wcd < numberOfWayCoordBlocks; wcd++)
                                    {
                                        //read way coordinate block
                                        Way.WayCoordinateBlock coordBlock = new Way.WayCoordinateBlock();
                                        coordBlock.CoordBlock = new List<Way.WayCoordinate>();

                                        int nodesCount = (int)MapFile.ReadVBEUint(ti.TileDataBufferStream);
                                        //Way.WayCoordinate diffwc = wc;
                                        if (doubleEncoding)
                                        {
                                            // get the first way node latitude single-delta offset (VBE-S)
                                            int wayNodeLatitude = ti.TileLatitude + MapFile.ReadVBESint(ti.TileDataBufferStream);
                                            // get the first way node longitude single-delta offset (VBE-S)
                                            int wayNodeLongitude = ti.TileLongitude + MapFile.ReadVBESint(ti.TileDataBufferStream);

                                            // store the first way node
                                            Way.WayCoordinate wc = new Way.WayCoordinate();
                                            wc.Latitude = wayNodeLatitude;
                                            wc.Longitude = wayNodeLongitude;
                                            coordBlock.CoordBlock.Add(wc);

                                            int previousSingleDeltaLatitude = 0;
                                            int previousSingleDeltaLongitude = 0;

                                            for (int n = 1; n < nodesCount; n++)
                                            {
                                                // get the way node latitude double-delta offset (VBE-S)
                                                int doubleDeltaLatitude = MapFile.ReadVBESint(ti.TileDataBufferStream);

                                                // get the way node longitude double-delta offset (VBE-S)
                                                int doubleDeltaLongitude = MapFile.ReadVBESint(ti.TileDataBufferStream);

                                                int singleDeltaLatitude = doubleDeltaLatitude + previousSingleDeltaLatitude;
                                                int singleDeltaLongitude = doubleDeltaLongitude + previousSingleDeltaLongitude;

                                                wayNodeLatitude = wayNodeLatitude + singleDeltaLatitude;
                                                wayNodeLongitude = wayNodeLongitude + singleDeltaLongitude;

                                                Way.WayCoordinate wcNext = new Way.WayCoordinate();
                                                wcNext.Latitude = wayNodeLatitude;
                                                wcNext.Longitude = wayNodeLongitude;
                                                coordBlock.CoordBlock.Add(wcNext);

                                                previousSingleDeltaLatitude = singleDeltaLatitude;
                                                previousSingleDeltaLongitude = singleDeltaLongitude;
                                            }
                                        }
                                        else
                                        {

                                            // get the first way node latitude single-delta offset (VBE-S)
                                            int wayNodeLatitude = ti.TileLatitude + MapFile.ReadVBESint(ti.TileDataBufferStream);
                                            // get the first way node longitude single-delta offset (VBE-S)
                                            int wayNodeLongitude = ti.TileLongitude + MapFile.ReadVBESint(ti.TileDataBufferStream);

                                            // store the first way node
                                            Way.WayCoordinate wc = new Way.WayCoordinate();
                                            wc.Latitude = wayNodeLatitude;
                                            wc.Longitude = wayNodeLongitude;
                                            coordBlock.CoordBlock.Add(wc);

                                            for (int n = 1; n < nodesCount; n++)
                                            {
                                                // get the way node latitude offset (VBE-S)
                                                wayNodeLatitude = wayNodeLatitude + MapFile.ReadVBESint(ti.TileDataBufferStream);

                                                // get the way node longitude offset (VBE-S)
                                                wayNodeLongitude = wayNodeLongitude + MapFile.ReadVBESint(ti.TileDataBufferStream);

                                                Way.WayCoordinate wcNext = new Way.WayCoordinate();
                                                wcNext.Latitude = wayNodeLatitude;
                                                wcNext.Longitude = wayNodeLongitude;
                                                coordBlock.CoordBlock.Add(wcNext);
                                            }
                                        }

                                        wd.DataBlock.Add(coordBlock);
                                    }

                                    way.WayDataBlocks.Add(wd);
                                }

                                ti.Ways.Add(way);
                            }
                        }
                    }
                }
                ti.TileDataBuffer = null; //memory can be released
            }
            catch
            {
                ti = null;
            }
            return ti;
        }
    }
}
