using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace GlobalcachingApplication.Plugins.LogImagesViewer
{
    public class ImageDownloader: IDisposable
    {
        private WebClient _wc = null;
        private IImageDownloaderCallback _owner;

        public ImageDownloader(IImageDownloaderCallback owner)
        {
            _owner = owner;
            _wc = new WebClient();
            _wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(_wc_DownloadProgressChanged);
            _wc.DownloadDataCompleted += new DownloadDataCompletedEventHandler(_wc_DownloadDataCompleted);
        }

        void _wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                _owner.LoadCompleted(e.Result, e.UserState as string);
            }
        }

        void _wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _owner.LoadProgressChanged(e.ProgressPercentage);            
        }

        public void LoadAsync(string url)
        {
            Stop();
            _owner.LoadProgressChanged(0);
            _wc.DownloadDataAsync(new Uri(url), url);
        }

        public void Stop()
        {
            if (_wc.IsBusy)
            {
                _wc.CancelAsync();
                while (_wc.IsBusy)
                {
                    System.Windows.Forms.Application.DoEvents();
                }
            }
        }

        public void Dispose()
        {
            if (_wc != null)
            {
                _wc.Dispose();
                _wc = null;
            }
        }
    }
}
