using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.Gallery
{
    public class ImageInfo
    {
        public string ImageGuid { get; set; }
        public string ThumbUrl { get; set; }
        public string Description { get; set; }
        public string MobileUrl { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public string LogCacheCode { get; set; }
        public string LogCode { get; set; }
        public string LogUrl { get; set; }
        public DateTime LogVisitDate { get; set; }

        public string ThumbFile { get; set; }
        public string ImgFile { get; set; }

    }
}
