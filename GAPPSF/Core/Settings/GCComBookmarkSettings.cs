using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public string GCComBookmarksXml
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public long GCComBookmarksID
        {
            get { return long.Parse(GetProperty("0")); }
            set { SetProperty(value.ToString()); }
        }

        public List<GCComBookmarks.Bookmark> LoadGCComBookmarks()
        {
            return _settingsStorage.LoadGCComBookmarks();
        }
        public void AddGCComBookmark(GCComBookmarks.Bookmark bm)
        {
            _settingsStorage.AddGCComBookmark(bm);
        }
        public void DeleteGCComBookmark(GCComBookmarks.Bookmark bm)
        {
            _settingsStorage.DeleteGCComBookmark(bm);
        }
        public List<string> LoadGCComBookmarkGeocaches(GCComBookmarks.Bookmark bm)
        {
            return _settingsStorage.LoadGCComBookmarkGeocaches(bm);
        }
        public void SaveGCComBookmarkGeocaches(GCComBookmarks.Bookmark bm, List<string> gcCodes)
        {
            _settingsStorage.SaveGCComBookmarkGeocaches(bm, gcCodes);
        }

    }
}
