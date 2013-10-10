using System;
using System.IO;
using System.Net;
using System.Windows.Media.Imaging;
using System.Threading;

namespace GlobalcachingApplication.Plugins.Maps.MapControl
{
    public class BitmapStoreGoogle : BitmapStore
    {
        protected override string GetCacheFileName(Uri uri)
        {
            //http://mt1.google.com/vt/lyrs=m&x={1}&y={2}&z={0}
            //to: z/x/y.png
            string[] parts = uri.LocalPath.Substring(uri.LocalPath.IndexOf('&')).Split(new char[] { '=', '&', 'x', 'y', 'z' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Concat(Path.Combine(CacheFolder, parts[2], parts[0], parts[1]), ".png");
        }

        protected override BitmapImage DownloadBitmap(Uri uri)
        {
            BeginDownload();
            MemoryStream buffer = null;
            try
            {
                // First download the image to our memory.
                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.UserAgent = UserAgent;

                buffer = new MemoryStream();
                using (var response = request.GetResponse())
                {
                    var stream = response.GetResponseStream();
                    stream.CopyTo(buffer);
                    stream.Close();
                }

                // Then save a copy for future reference, making sure to rewind
                // the stream to the start.
                buffer.Position = 0;
                SaveCacheImage(buffer, uri);

                // Finally turn the memory into a beautiful picture.
                buffer.Position = 0;
                return GetImageFromStream(buffer);
            }
            catch (WebException)
            {
                RaiseDownloadError();
            }
            catch (NotSupportedException) // Problem creating the bitmap (messed up download?)
            {
                RaiseDownloadError();
            }
            finally
            {
                EndDownload();
                if (buffer != null)
                {
                    buffer.Dispose();
                }
            }
            return null;
        }

    }
}
