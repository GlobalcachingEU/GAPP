using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.LogImagesViewer
{
    public interface IImageDownloaderCallback
    {
        void LoadProgressChanged(int progress);
        void LoadCompleted(byte[] data, string url);
    }
}
