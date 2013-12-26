using System;
using System.Globalization;
using System.Windows.Media.Imaging;

namespace GAPPSF.MapProviders
{
    public class TileGeneratorOSM: TileGenerator
    {
        public TileGeneratorOSM(MapControlFactory mapControlFactory)
            : base(mapControlFactory)
        {
            _maxZoom = 18;
            _tileSize = 256;
        }

        private const string TileFormat = @"http://tile.openstreetmap.org/{0}/{1}/{2}.png";


        /// <summary>Returns the latitude for the specified tile number.</summary>
        /// <param name="tileY">The tile number along the Y axis.</param>
        /// <param name="zoom">The zoom level of the tile index.</param>
        /// <returns>A decimal degree for the latitude, limited to aproximately +- 85.0511 degrees.</returns>
        public override double GetLatitude(double tileY, int zoom)
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
        public override double GetLongitude(double tileX, int zoom)
        {
            // n = 2 ^ zoom
            // lon_deg = xtile / n * 360.0 - 180.0
            double degrees = tileX / Math.Pow(2, zoom) * 360.0;
            return Clip(degrees, 0, 360) - 180.0; // Make sure we limit its range
        }

        /// <summary>Returns the maximum size, in pixels, for the specifed zoom level.</summary>
        /// <param name="zoom">The zoom level to calculate the size for.</param>
        /// <returns>The size in pixels.</returns>
        public override double GetSize(int zoom)
        {
            return Math.Pow(2, zoom) * TileSize;
        }

        /// <summary>Returns the tile index along the X axis for the specified longitude.</summary>
        /// <param name="longitude">The longitude coordinate.</param>
        /// <param name="zoom">The zoom level of the desired tile index.</param>
        /// <returns>The tile index along the X axis.</returns>
        /// <remarks>The longitude is not checked to be valid and, therefore, the output may not be a valid index.</remarks>
        public override double GetTileX(double longitude, int zoom)
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
        public override double GetTileY(double latitude, int zoom)
        {
            // n = 2 ^ zoom
            // ytile = (1 - (log(tan(lat_rad) + sec(lat_rad)) / π)) / 2 * n
            double radians = latitude * Math.PI / 180.0;
            double log = Math.Log(Math.Tan(radians) + (1.0 / Math.Cos(radians)));
            return (1.0 - (log / Math.PI)) * Math.Pow(2, zoom - 1);
        }

        /// <summary>Returns a Tile for the specified area.</summary>
        /// <param name="zoom">The zoom level of the desired tile.</param>
        /// <param name="x">Tile index along the X axis.</param>
        /// <param name="y">Tile index along the Y axis.</param>
        /// <returns>
        /// If any of the indexes are outside the valid range of tile numbers for the specified zoom level,
        /// null will be returned.
        /// </returns>
        public override BitmapImage GetTileImage(int zoom, int x, int y)
        {
            if (string.IsNullOrEmpty(CacheFolder))
            {
                throw new InvalidOperationException("Must set the CacheFolder before calling GetTileImage.");
            }

            double tileCount = Math.Pow(2, zoom) - 1;
            if (x < 0 || y < 0 || x > tileCount || y > tileCount) // Bounds check
            {
                return null;
            }

            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, TileFormat, zoom, x, y));
            return _mapControlFactory.BitmapStore.GetImage(uri);
        }

        /// <summary>Returns the closest zoom level less than or equal to the specified map size.</summary>
        /// <param name="size">The size in pixels.</param>
        /// <returns>The closest zoom level for the specified size.</returns>
        public override int GetZoom(double size)
        {
            return (int)Math.Log(size, 2);
        }

    }
}
