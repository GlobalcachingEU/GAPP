using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;

namespace GAPPSF.MapProviders.OSMBinMap
{
    public class MapFilesHandler: IDisposable
    {
        private List<MapFile> _mapFiles = new List<MapFile>();
        private GAPPSF.MapProviders.MapControlFactory _mapControlFactory;

        public MapFilesHandler(GAPPSF.MapProviders.MapControlFactory mapControlFactory)
        {
            _mapControlFactory = mapControlFactory;

            //Core.Settings.Default.MapsOSMBinOfflineMapFiles = @"c:\Users\peters-r1\Downloads\maps\netherlands.map";

            if (!string.IsNullOrEmpty(Core.Settings.Default.MapsOSMBinOfflineMapFiles))
            {
                string[] fl = Core.Settings.Default.MapsOSMBinOfflineMapFiles.Split(new char[] { '\r','\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach(string s in fl)
                {
                    _mapControlFactory.OSMBinFiles.Add(s);
                }
            }
            reload();

            _mapControlFactory.OSMBinFiles.CollectionChanged += OSMBinFiles_CollectionChanged;
        }

        void OSMBinFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in _mapControlFactory.OSMBinFiles)
            {
                sb.AppendLine(s);
            }
            Core.Settings.Default.MapsOSMBinOfflineMapFiles = sb.ToString();
            reload();
        }

        public GAPPSF.MapProviders.MapControlFactory MapControlFactory
        {
            get { return _mapControlFactory; }
        }

        public void reload()
        {
            clearMapFiles();

            if (!string.IsNullOrEmpty(Core.Settings.Default.MapsOSMBinOfflineMapFiles))
            {
                try
                {
                    foreach (string fn in _mapControlFactory.OSMBinFiles)
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
                catch(Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
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
            _mapControlFactory.OSMBinFiles.CollectionChanged -= OSMBinFiles_CollectionChanged;
            clearMapFiles();
        }
    }
}
