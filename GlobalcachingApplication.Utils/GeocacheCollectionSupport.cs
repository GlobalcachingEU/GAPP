using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Utils
{
    public class GeocacheCollectionSupport
    {
        private static GeocacheCollectionSupport _uniqueInstance = null;
        private static object _lockObject = new object();

        private List<Framework.Interfaces.IPluginGeocacheCollection> _gcCollectionPlugins = new List<Framework.Interfaces.IPluginGeocacheCollection>();
        private Framework.Interfaces.ICore _core = null;

        public static GeocacheCollectionSupport Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            _uniqueInstance = new GeocacheCollectionSupport();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        public virtual void UpdateIgnoreGeocachesCollectionPlugins(Framework.Interfaces.ICore core)
        {
            _core = core;
            List<Framework.Interfaces.IPlugin> p = core.GetPlugin(Framework.PluginType.GeocacheCollection);
            if (p != null)
            {
                foreach (Framework.Interfaces.IPluginGeocacheCollection mwp in p)
                {
                    if (!_gcCollectionPlugins.Contains(mwp))
                    {
                        _gcCollectionPlugins.Add(mwp);
                    }
                }
                foreach (Framework.Interfaces.IPlugin mwp in _gcCollectionPlugins)
                {
                    if (!p.Contains(mwp))
                    {
                        _gcCollectionPlugins.Remove(mwp as Framework.Interfaces.IPluginGeocacheCollection);
                    }
                }
            }
        }

        public List<string> AvailableCollections()
        {
            List<string> result = new List<string>();
            foreach (Framework.Interfaces.IPluginGeocacheCollection p in _gcCollectionPlugins)
            {
                p.AvailableCollections(result);
            }
            return result;
        }

        public void AddCollection(string name)
        {
            foreach (Framework.Interfaces.IPluginGeocacheCollection p in _gcCollectionPlugins)
            {
                p.AddCollection(name);
            }
        }

        public void DeleteCollection(string name)
        {
            foreach (Framework.Interfaces.IPluginGeocacheCollection p in _gcCollectionPlugins)
            {
                p.DeleteCollection(name);
            }
        }

        public void AddToCollection(string collectionName, string geocacheCode)
        {
            foreach (Framework.Interfaces.IPluginGeocacheCollection p in _gcCollectionPlugins)
            {
                p.AddToCollection(collectionName, geocacheCode);
            }
        }

        public void RemoveFromCollection(string collectionName, string geocacheCode)
        {
            foreach (Framework.Interfaces.IPluginGeocacheCollection p in _gcCollectionPlugins)
            {
                p.RemoveFromCollection(collectionName, geocacheCode);
            }
        }

        public bool InCollection(string collectionName, string geocacheCode)
        {
            bool result = false;
            foreach (Framework.Interfaces.IPluginGeocacheCollection p in _gcCollectionPlugins)
            {
                if (p.InCollection(collectionName, geocacheCode))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
    }
}
