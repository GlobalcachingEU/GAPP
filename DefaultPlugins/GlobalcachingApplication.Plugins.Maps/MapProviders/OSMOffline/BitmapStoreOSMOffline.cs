using System;
using System.IO;
using System.Net;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Reflection;

namespace GlobalcachingApplication.Plugins.Maps.MapControl
{
    public class BitmapStoreOSMOffline : BitmapStore
    {
        private OSMBinMap.MapFilesHandler _mapsHandler = null;

        public BitmapStoreOSMOffline(OSMBinMap.MapFilesHandler mapsHandler)
        {
            _mapsHandler = mapsHandler;
        }

        protected override BitmapImage DownloadBitmap(Uri uri)
        {
            BeginDownload();
            BitmapImage result = null;
            try
            {
                result = _mapsHandler.GetTileBitmap(uri);
                if (result != null)
                {
#if !DEBUG
                    SaveCacheImage(result, uri);
#endif
                }
                else
                {
                    lock (this)
                    {
                        result = new System.Windows.Media.Imaging.BitmapImage();
                        result.BeginInit();
                        result.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                        result.StreamSource = Assembly.GetExecutingAssembly().GetManifestResourceStream("GlobalcachingApplication.Plugins.Maps.OSMBinMap.MissingTile.png");
                        result.EndInit();
                        result.Freeze();
                    }
                }
            }
            catch
            {
                result = null;
                RaiseDownloadError();
            }
            finally
            {
                EndDownload();
            }
            return result;
        }

    }
}
