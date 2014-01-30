using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public List<string> AvailableCollections()
        {
            return _settingsStorage.AvailableCollections();
        }
        public List<string> GetGeocachesInCollection(string collectionName)
        {
            return _settingsStorage.GetGeocachesInCollection(collectionName);
        }
        public void AddCollection(string name)
        {
            _settingsStorage.AddCollection(name);
        }
        public void DeleteCollection(string name)
        {
            _settingsStorage.DeleteCollection(name);

        }
        public void AddToCollection(string collectionName, string geocacheCode)
        {
            _settingsStorage.AddToCollection(collectionName, geocacheCode);
        }
        public void AddToCollection(int collectionID, string geocacheCode)
        {
            _settingsStorage.AddToCollection(collectionID, geocacheCode);
        }
        public void RemoveFromCollection(string collectionName, string geocacheCode)
        {
            _settingsStorage.RemoveFromCollection(collectionName, geocacheCode);
        }
        public void RemoveFromCollection(int collectionID, string geocacheCode)
        {
            _settingsStorage.RemoveFromCollection(collectionID, geocacheCode);
        }
        public bool InCollection(string collectionName, string geocacheCode)
        {
            return _settingsStorage.InCollection(collectionName, geocacheCode);
        }
        public bool InCollection(int collectionID, string geocacheCode)
        {
            return _settingsStorage.InCollection(collectionID, geocacheCode);
        }
        public int GetCollectionID(string collectionName)
        {
            return _settingsStorage.GetCollectionID(collectionName);
        }


        public int GCCollectionWindowWidth
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int GCCollectionWindowHeight
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int GCCollectionWindowTop
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }
        public int GCCollectionWindowLeft
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }
    }
}
