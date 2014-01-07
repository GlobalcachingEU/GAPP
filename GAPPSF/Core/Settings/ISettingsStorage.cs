using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public interface ISettingsStorage: IDisposable
    {
        void StoreSetting(string name, string value);
        Hashtable LoadSettings();

        //geocache ignore filter
        Hashtable LoadIgnoredGeocacheCodes();
        List<string> LoadIgnoredGeocacheNames();
        List<string> LoadIgnoredGeocacheOwners();
        void ClearGeocacheIgnoreFilters();
        void AddIgnoreGeocacheCode(string code);
        void AddIgnoreGeocacheName(string name);
        void AddIgnoreGeocacheOwner(string owner);
        void DeleteIgnoreGeocacheCode(string code);
        void DeleteIgnoreGeocacheName(string name);
        void DeleteIgnoreGeocacheOwner(string owner);

        //gccom bookmarks
        List<GCComBookmarks.Bookmark> LoadGCComBookmarks();
        void AddGCComBookmark(GCComBookmarks.Bookmark bm);
        void DeleteGCComBookmark(GCComBookmarks.Bookmark bm);
        List<string> LoadGCComBookmarkGeocaches(GCComBookmarks.Bookmark bm);
        void SaveGCComBookmarkGeocaches(GCComBookmarks.Bookmark bm, List<string> gcCodes);

        //attachements
        List<Attachement.Item> GetAttachements(string gcCode);
        void AddAttachement(Attachement.Item item);
        void DeleteAttachement(Attachement.Item item);

        //formula solver
        string GetFormula(string gcCode);
        void SetFormula(string gcCode, string formula);
    }
}
