using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.LogImagesViewer
{
    public class ThumbInfo
    {
        public string ImageFileLocation { get; set; }
        public Framework.Data.LogImage LogImage { get; set; }
        public Framework.Data.Log Log { get; set; }
        public Framework.Data.Geocache Geocache { get; set; }
    }
}
