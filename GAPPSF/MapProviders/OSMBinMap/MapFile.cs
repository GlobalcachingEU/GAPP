using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;

namespace GAPPSF.MapProviders.OSMBinMap
{
    public class MapFile: IDisposable
    {
        private string _filename;
        private FileHeader _fileHeader = null;
        private FileStream _fileStream = null;
        private SubFile[] _subFiles = null;
        private MapFilesHandler _mapFileHandler = null;

        //helpers
        private byte[] buffer = null;

        public MapFile(MapFilesHandler mapFileHandler, string filename)
        {
            _mapFileHandler = mapFileHandler;
            _filename = filename;
            try
            {
                FileStream fs = File.OpenRead(_filename);
                _fileStream = fs;
                buffer = new byte[30];
            }
            catch
            {
            }
        }

        public FileStream StreamReader
        {
            get { return _fileStream; }
        }

        public FileHeader Header
        {
            get { return _fileHeader; }
        }

        public MapFilesHandler MapFileHandler
        {
            get { return _mapFileHandler; }
        }

        public bool ReadHeader()
        {
            FileHeader fh = new FileHeader(this);
            if (fh.Read())
            {
                _fileHeader = fh;
                _subFiles = new SubFile[fh.ZoomIntervalCount];
                for (int i = 0; i < fh.ZoomIntervalCount; i++)
                {
                    _subFiles[i] = new SubFile(this, fh.ZoomIntervalConfigurations[i]);
                    _subFiles[i].ReadHeader();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public RequestedTileInformation GetTileInformation(long x, long y, int zoom)
        {
            RequestedTileInformation result = null;
            //select the correct subfile
            SubFile sf = (from s in _subFiles where zoom >= s.ZoomIntervalConfig.MinZoomLevel && zoom <= s.ZoomIntervalConfig.MaxZoomLevel select s).FirstOrDefault();
            if (sf != null)
            {
                result = sf.GetTileInformation(x, y, zoom);
            }
            return result;
        }

        public static byte[] ReverseBytes(byte[] inArray, int startIndex, int length)
        {
            byte temp;
            int ctr = startIndex;
            int highCtr = startIndex + length - 1;

            for (int i = 0; i < length / 2; i++)
            {
                temp = inArray[ctr];
                inArray[ctr] = inArray[highCtr];
                inArray[highCtr] = temp;
                highCtr--;
                ctr++;
            }
            return inArray;
        }

        public void ReadBuffer(byte[] buffer, long startAt, int length)
        {
            lock (this)
            {
                _fileStream.Position = startAt;
                _fileStream.Read(buffer, 0, length);
            }
        }

        public string ReadVariableString()
        {
            int length = (byte)_fileStream.ReadByte();
            byte[] bytes = new byte[length];
            _fileStream.Read(bytes, 0, length);
            return System.Text.ASCIIEncoding.ASCII.GetString(bytes.ToArray());
        }

        public string ReadFixedString(int length)
        {
            if (buffer.Length < length) buffer = new byte[length];
            _fileStream.Read(buffer, 0, length);
            return System.Text.ASCIIEncoding.ASCII.GetString(buffer,0,length);
        }

        public Int32 ReadInt32()
        {
            _fileStream.Read(buffer, 0, 4);
            return BitConverter.ToInt32(ReverseBytes(buffer, 0, 4), 0);
        }
        public Int64 ReadInt64()
        {
            _fileStream.Read(buffer, 0, 8);
            return BitConverter.ToInt64(ReverseBytes(buffer, 0, 8), 0);
        }
        public Int16 ReadInt16()
        {
            _fileStream.Read(buffer, 0, 2);
            return BitConverter.ToInt16(ReverseBytes(buffer, 0, 2), 0);
        }

        public static UInt32 ReadVBEUint(MemoryStream ms)
        {
            int variableByteDecode = 0;
            byte variableByteShift = 0;

            // check if the continuation bit is set
            byte b = (byte)ms.ReadByte();
            while ((b & 0x80) != 0 && variableByteShift<32)
            {
                variableByteDecode |= (b & 0x7f) << variableByteShift;
                variableByteShift += 7;
                b = (byte)ms.ReadByte();
            }

            // read the seven data bits from the last byte
            return (UInt32)(variableByteDecode | ((int)b << variableByteShift));
        }
        
        public static int ReadVBESint(MemoryStream ms)
        {
            int variableByteDecode = 0;
            byte variableByteShift = 0;

            // check if the continuation bit is set
            byte b = (byte)ms.ReadByte();
            while ((b & 0x80) != 0 && variableByteShift < 32)
            {
                variableByteDecode |= (int)(b & 0x7f) << variableByteShift;
                variableByteShift += 7;
                b = (byte)ms.ReadByte();
            }

            // read the six data bits from the last byte
            if ((b & 0x40) != 0)
            {
                // negative
                variableByteDecode = -(variableByteDecode | ((int)(b & 0x3f) << variableByteShift));
                return variableByteDecode;
            }
            // positive
            return variableByteDecode | ((b & 0x3f) << variableByteShift);
        }

        public static string ReadVariableString(MemoryStream ms)
        {
            int length = (byte)ms.ReadByte();
            byte[] bytes = new byte[length];
            ms.Read(bytes, 0, length);
            return System.Text.ASCIIEncoding.ASCII.GetString(bytes.ToArray());
        }

        public static string ReadUTF8EncodedString(MemoryStream ms)
        {
            return ReadUTF8EncodedString((int)ReadVBEUint(ms), ms);
        }

        public static string ReadUTF8EncodedString(int stringLength, MemoryStream ms)
        {
            if (stringLength > 0 && ms.Position + stringLength <= ms.Length)
            {
                byte[] bytes = new byte[stringLength];
                ms.Read(bytes, 0, stringLength);
                return System.Text.UTF8Encoding.UTF8.GetString(bytes);
            }
            return null;
        }

        public static UInt16 ReadUInt16(MemoryStream ms)
        {
            byte[] buffer = new byte[2];
            ms.Read(buffer, 0, 2);
            return BitConverter.ToUInt16(ReverseBytes(buffer, 0, 2), 0);
        }

        /// <summary>Returns the tile index along the X axis for the specified longitude.</summary>
        /// <param name="longitude">The longitude coordinate.</param>
        /// <param name="zoom">The zoom level of the desired tile index.</param>
        /// <returns>The tile index along the X axis.</returns>
        /// <remarks>The longitude is not checked to be valid and, therefore, the output may not be a valid index.</remarks>
        public static double GetTileX(double longitude, int zoom)
        {
            // n = 2 ^ zoom
            // xtile = ((lon_deg + 180) / 360) * n
            return ((longitude + 180.0) / 360.0) * Math.Pow(2, zoom);
        }

        /// <summary>Returns the tile index along the Y axis for the specified latitude.</summary>
        /// <param name="latitude">The latitude coordinate.</param>
        /// <param name="zoom">The zoom level of the desired tile index.</param>
        /// <returns>The tile index along the Y axis.</returns>
        /// <remarks>The latitude is not checked to be valid and, therefore, the output may not be a valid index.</remarks>
        public static double GetTileY(double latitude, int zoom)
        {
            // n = 2 ^ zoom
            // ytile = (1 - (log(tan(lat_rad) + sec(lat_rad)) / π)) / 2 * n
            double radians = latitude * Math.PI / 180.0;
            double log = Math.Log(Math.Tan(radians) + (1.0 / Math.Cos(radians)));
            return (1.0 - (log / Math.PI)) * Math.Pow(2, zoom - 1);
        }

        /// <summary>Returns the latitude for the specified tile number.</summary>
        /// <param name="tileY">The tile number along the Y axis.</param>
        /// <param name="zoom">The zoom level of the tile index.</param>
        /// <returns>A decimal degree for the latitude, limited to aproximately +- 85.0511 degrees.</returns>
        public static double GetLatitude(double tileY, int zoom)
        {
            // n = 2 ^ zoom
            // lat_rad = arctan(sinh(π * (1 - 2 * ytile / n)))
            // lat_deg = lat_rad * 180.0 / π
            double tile = Clip(1 - ((2 * tileY) / Math.Pow(2, zoom)), -1, 1); // Limit value we pass to sinh
            return Math.Atan(Math.Sinh(Math.PI * tile)) * 180.0 / Math.PI;
        }

        /// <summary>Returns the longitude for the specified tile number.</summary>
        /// <param name="tileX">The tile number along the X axis.</param>
        /// <param name="zoom">The zoom level of the tile index.</param>
        /// <returns>A decimal degree for the longitude, limited to +- 180 degrees.</returns>
        public static double GetLongitude(double tileX, int zoom)
        {
            // n = 2 ^ zoom
            // lon_deg = xtile / n * 360.0 - 180.0
            double degrees = tileX / Math.Pow(2, zoom) * 360.0;
            return Clip(degrees, 0, 360) - 180.0; // Make sure we limit its range
        }

        protected static double Clip(double value, double minimum, double maximum)
        {
            if (value < minimum)
            {
                return minimum;
            }
            if (value > maximum)
            {
                return maximum;
            }
            return value;
        }

        public void Dispose()
        {
            if (_fileStream != null)
            {
                _fileStream.Dispose();
                _fileStream = null;
            }
        }
    }
}
