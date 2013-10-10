using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Utils
{
    public class GeocacheIgnoreSupport
    {
        private static GeocacheIgnoreSupport _uniqueInstance = null;
        private static object _lockObject = new object();

        private List<Framework.Interfaces.IGeocacheIgnoreFilter> _gcIgnorePlugins = new List<Framework.Interfaces.IGeocacheIgnoreFilter>();
        private Framework.Interfaces.ICore _core = null;

        public int IgnoreCounter { get; set; }

        public static GeocacheIgnoreSupport Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            _uniqueInstance = new GeocacheIgnoreSupport();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        public virtual void UpdateIgnoreGeocachesSupportPlugins(Framework.Interfaces.ICore core)
        {
            _core = core;
            List<Framework.Interfaces.IPlugin> p = core.GetPlugin(Framework.PluginType.GeocacheIgnoreFilter);
            if (p != null)
            {
                foreach (Framework.Interfaces.IGeocacheIgnoreFilter mwp in p)
                {
                    if (!_gcIgnorePlugins.Contains(mwp))
                    {
                        _gcIgnorePlugins.Add(mwp);
                    }
                }
                foreach (Framework.Interfaces.IPlugin mwp in _gcIgnorePlugins)
                {
                    if (!p.Contains(mwp))
                    {
                        _gcIgnorePlugins.Remove(mwp as Framework.Interfaces.IGeocacheIgnoreFilter);
                    }
                }
            }
        }


        public bool IgnoreGeocache(string code)
        {
            bool result = false;
            foreach (Framework.Interfaces.IGeocacheIgnoreFilter p in _gcIgnorePlugins)
            {
                if (p.IgnoreGeocache(code))
                {
                    IgnoreCounter++;
                    result = true;
                    break;
                }
            }
            return result;
        }

        public bool IgnoreGeocache(Framework.Data.Geocache gc)
        {
            bool result = false;
            foreach (Framework.Interfaces.IGeocacheIgnoreFilter p in _gcIgnorePlugins)
            {
                if (p.IgnoreGeocache(gc))
                {
                    IgnoreCounter++;
                    result = true;
                    break;
                }
            }
            return result;
        }

        public List<string> FilterGeocaches(List<string> codes)
        {
            List<string> result = codes;
            if (codes != null)
            {
                int orgCount = codes.Count;
                foreach (Framework.Interfaces.IGeocacheIgnoreFilter p in _gcIgnorePlugins)
                {
                    result = p.FilterGeocaches(codes);
                }
                int newCount = codes.Count;
                if (orgCount != newCount)
                {
                    IgnoreCounter += (orgCount - newCount);
                }
            }
            return result;
        }

    }
}
