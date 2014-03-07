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

        //integrity of settings
        bool IsStorageOK { get; }
        bool CreateBackup();
        List<string> AvailableBackups { get; }
        bool RemoveBackup(string id);
        bool PrepareRestoreBackup(string id);

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

        //geocache notes
        string GetGeocacheNotes(string gcCode);
        void SetGeocacheNotes(string gcCode, string notes);

        //Geocache Collections
        List<string> AvailableCollections();
        List<string> GetGeocachesInCollection(string collectionName);
        void AddCollection(string name);
        void DeleteCollection(string name);
        void AddToCollection(string collectionName, string geocacheCode);
        void AddToCollection(int collectionID, string geocacheCode);
        void RemoveFromCollection(string collectionName, string geocacheCode);
        void RemoveFromCollection(int collectionID, string geocacheCode);
        bool InCollection(string collectionName, string geocacheCode);
        bool InCollection(int collectionID, string geocacheCode);
        int GetCollectionID(string collectionName);

        //geocache distance
        double? GetGeocacheDistance(string gcCode);
        void SetGeocacheDistance(string gcCode, double? dist);

        //GCVotes
        double? GetGCVoteMedian(string gcCode);
        double? GetGCVoteAverage(string gcCode);
        int? GetGCVoteCount(string gcCode);
        double? GetGCVoteUser(string gcCode);
        void SetGCVote(string gcCode, double median, double average, int cnt, double? user);
        void ClearGCVotes();

        //trackable groups
        List<UIControls.Trackables.TrackableGroup> GetTrackableGroups();
        void AddTrackableGroup(UIControls.Trackables.TrackableGroup grp);
        void DeleteTrackableGroup(UIControls.Trackables.TrackableGroup grp);
        List<UIControls.Trackables.TrackableItem> GetTrackables(UIControls.Trackables.TrackableGroup grp);
        void AddUpdateTrackable(UIControls.Trackables.TrackableGroup grp, UIControls.Trackables.TrackableItem trackable);
        void DeleteTrackable(UIControls.Trackables.TrackableGroup grp, UIControls.Trackables.TrackableItem trackable);
        List<UIControls.Trackables.TravelItem> GetTrackableTravels(UIControls.Trackables.TrackableItem trackable);
        void UpdateTrackableTravels(UIControls.Trackables.TrackableItem trackable, List<UIControls.Trackables.TravelItem> travels);
        List<UIControls.Trackables.LogItem> GetTrackableLogs(UIControls.Trackables.TrackableItem trackable);
        void UpdateTrackableLogs(UIControls.Trackables.TrackableItem trackable, List<UIControls.Trackables.LogItem> logs);
        byte[] GetTrackableIconData(string iconUrl);
    }
}
