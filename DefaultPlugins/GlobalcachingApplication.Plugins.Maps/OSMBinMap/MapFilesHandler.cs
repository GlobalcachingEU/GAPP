using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;

namespace GlobalcachingApplication.Plugins.Maps.OSMBinMap
{
    public class MapFilesHandler: IDisposable
    {
        private string _mapFilesFolder;
        private List<MapFile> _mapFiles = new List<MapFile>();
        private MapControl.MapControlFactory _mapControlFactory;

        public MapFilesHandler(MapControl.MapControlFactory mapControlFactory)
        {
            _mapControlFactory = mapControlFactory;
            reload();
        }

        public MapControl.MapControlFactory MapControlFactory
        {
            get { return _mapControlFactory; }
        }

        public void reload()
        {
            clearMapFiles();
            _mapFilesFolder = Properties.Settings.Default.OSMOfflineMapFolder;

            try
            {
                string[] fl = Directory.GetFiles(_mapFilesFolder);
                foreach (string fn in fl)
                {
                    if (!Properties.Settings.Default.DisabledMaps.Contains(System.IO.Path.GetFileName(fn)))
                    {
                        MapFile mf = new MapFile(this, fn);
                        if (mf.ReadHeader())
                        {
                            _mapFiles.Add(mf);
                        }
                        else
                        {
                            mf.Dispose();
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public BitmapImage GetTileBitmap(Uri uri)
        {
            BitmapImage result = null;
#if DEBUG2
            
            lock (this)
            {
#endif
                try
                {
                    string[] parts = uri.AbsoluteUri.Split(new char[] { '/', '.' }, StringSplitOptions.RemoveEmptyEntries);
                    int zoom = int.Parse(parts[4]);
                    long x = long.Parse(parts[5]);
                    long y = long.Parse(parts[6]);
                    double lat = MapFile.GetLatitude(y, zoom);
                    double lon = MapFile.GetLongitude(x, zoom);
                    var mfs = from m in _mapFiles where lat >= m.Header.MinLat && lat < m.Header.MaxLat &&  lon >= m.Header.MinLon && lon < m.Header.MaxLon select m;
                    List<RequestedTileInformation> tis = new List<RequestedTileInformation>();
                    foreach (MapFile mf in mfs)
                    {
                        RequestedTileInformation ti = mf.GetTileInformation(x, y, zoom);
                        if (ti != null)
                        {
                            tis.Add(ti);
                        }
                    }
                    if (tis.Count > 0)
                    {
                        result = TileRenderer.GetTileBitmap(tis);
                    }
                }
                catch
                {
                }
#if DEBUG2
            }
#endif
                return result;
        }

        private void clearMapFiles()
        {
            foreach (var m in _mapFiles)
            {
                m.Dispose();
            }
            _mapFiles.Clear();
        }

        public void Dispose()
        {
            clearMapFiles();
        }
    }
}
