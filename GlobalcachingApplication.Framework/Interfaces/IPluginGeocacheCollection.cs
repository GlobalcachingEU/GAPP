using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Interfaces
{
    public interface IPluginGeocacheCollection
    {
        List<string> AvailableCollections(List<string> lst);
        void AddCollection(string name);
        void DeleteCollection(string name);
        void AddToCollection(string collectionName, string geocacheCode);
        void RemoveFromCollection(string collectionName, string geocacheCode);
        bool InCollection(string collectionName, string geocacheCode);
    }
}
