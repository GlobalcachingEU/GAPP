using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GlobalcachingApplication.Plugins.Maps.OSMBinMap
{
    public class FileHeader
    {
        private MapFile _mapFile;

        public string MagicByte { get; private set; }
        public int HeaderSize { get; private set; }
        public int FileVersion { get; private set; }
        public long FileSize { get; private set; }
        public DateTime CreationDate { get; private set; }
        public double MinLat { get; private set; }
        public double MinLon { get; private set; }
        public double MaxLat { get; private set; }
        public double MaxLon { get; private set; }
        public int TileSize { get; private set; }
        public string Projection { get; private set; }
        public bool DebugInformationPresent { get; private set; }
        public bool MapStartPositionPresent { get; private set; }
        public bool MapStartZoomLevelPresent { get; private set; }
        public bool LanguagePreferencePresent { get; private set; }
        public bool CommentPresent { get; private set; }
        public bool CreatedByPresent { get; private set; }
        public double? MapStartLat { get; private set; }
        public double? MapStartLon { get; private set; }
        public int? MapStartZoomLevel { get; private set; }
        public string LanguagePreference { get; private set; }
        public string Comment { get; private set; }
        public string CreatedBy { get; private set; }
        public Tag[] POITags { get; private set; }
        public Tag[] WayTags { get; private set; }
        public int ZoomIntervalCount { get; private set; }
        public ZoomIntervalConfiguration[] ZoomIntervalConfigurations { get; private set; }

        public FileHeader(MapFile mapFile)
        {
            _mapFile = mapFile;
        }

        public bool Read()
        {
            try
            {
                FileStream fs = _mapFile.StreamReader;
                fs.Position = 0;

                MagicByte = _mapFile.ReadFixedString(20);
                if (MagicByte != "mapsforge binary OSM") return false;
                HeaderSize = _mapFile.ReadInt32();
                FileVersion = _mapFile.ReadInt32();
                FileSize = _mapFile.ReadInt64();
                CreationDate = new DateTime(1970,1,1).AddMilliseconds(_mapFile.ReadInt64());
                //minLat, minLon, maxLat, maxLon
                MinLat = (double)_mapFile.ReadInt32() / 1000000.0;
                MinLon = (double)_mapFile.ReadInt32() / 1000000.0;
                MaxLat = (double)_mapFile.ReadInt32() / 1000000.0;
                MaxLon = (double)_mapFile.ReadInt32() / 1000000.0;
                TileSize = _mapFile.ReadInt16();
                Projection = _mapFile.ReadVariableString();
                byte b = (byte)fs.ReadByte();
                DebugInformationPresent = (b & 0x80) == 0x80;
                MapStartPositionPresent = (b & 0x40) == 0x40;
                MapStartZoomLevelPresent = (b & 0x20) == 0x20;
                LanguagePreferencePresent = (b & 0x10) == 0x10;
                CommentPresent = (b & 0x80) == 0x80;
                CreatedByPresent = (b & 0x40) == 0x40;
                //todo
                if (MapStartPositionPresent)
                {
                    MapStartLat = (double)_mapFile.ReadInt32() / 1000000.0;
                    MapStartLon = (double)_mapFile.ReadInt32() / 1000000.0;
                }
                else
                {
                    MapStartLat = null;
                    MapStartLon = null;
                }
                if (MapStartZoomLevelPresent)
                {
                    MapStartZoomLevel = fs.ReadByte();
                }
                else
                {
                    MapStartZoomLevel = null;
                }
                if (LanguagePreferencePresent)
                {
                    LanguagePreference = _mapFile.ReadVariableString();
                }
                else
                {
                    LanguagePreference = null;
                }
                if (CommentPresent)
                {
                    Comment = _mapFile.ReadVariableString();
                }
                else
                {
                    Comment = null;
                }
                if (CreatedByPresent)
                {
                    CreatedBy = _mapFile.ReadVariableString();
                }
                else
                {
                    CreatedBy = null;
                }
                int cnt = _mapFile.ReadInt16();
                POITags = new Tag[cnt];
                for (int i = 0; i < cnt; i++)
                {
                    string s = _mapFile.ReadVariableString();
                    string[] parts = s.Split(new char[] { '=' });
                    POITags[i] = new Tag(parts[0], parts[1]);
                }
                cnt = _mapFile.ReadInt16();
                WayTags = new Tag[cnt];
                for (int i = 0; i < cnt; i++)
                {
                    string s = _mapFile.ReadVariableString();
                    string[] parts = s.Split(new char[] { '=' });
                    WayTags[i] = new Tag(parts[0], parts[1]);
                }
                ZoomIntervalCount = fs.ReadByte();
                ZoomIntervalConfigurations = new ZoomIntervalConfiguration[ZoomIntervalCount];
                for (int i = 0; i < ZoomIntervalCount; i++)
                {
                    ZoomIntervalConfiguration zli = new ZoomIntervalConfiguration(
                        fs.ReadByte(),
                        fs.ReadByte(),
                        fs.ReadByte(),
                        _mapFile.ReadInt64(),
                        _mapFile.ReadInt64());
                    ZoomIntervalConfigurations[i] = zli;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
