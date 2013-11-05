using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Utils
{
    public class GeometrySupport
    {
        private static GeometrySupport _uniqueInstance = null;
        private static object _lockObject = new object();
        private List<Framework.Interfaces.IGeometry> _geometryPlugins = new List<Framework.Interfaces.IGeometry>();
        private Framework.Interfaces.ICore _core = null;

        public static GeometrySupport Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            _uniqueInstance = new GeometrySupport();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        public virtual void UpdateGeometrySupportPlugins(Framework.Interfaces.ICore core)
        {
            _core = core;
            List<Framework.Interfaces.IPlugin> p = core.GetPlugin(Framework.PluginType.Geometry);
            if (p != null)
            {
                foreach (Framework.Interfaces.IGeometry mwp in p)
                {
                    if (!_geometryPlugins.Contains(mwp))
                    {
                        _geometryPlugins.Add(mwp);
                    }
                }
                foreach (Framework.Interfaces.IPlugin mwp in _geometryPlugins)
                {
                    if (!p.Contains(mwp))
                    {
                        _geometryPlugins.Remove(mwp as Framework.Interfaces.IGeometry);
                    }
                }
            }
        }

        public List<Framework.Data.AreaInfo> GetAreasOfLocation(Framework.Data.Location loc)
        {
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            foreach (Framework.Interfaces.IGeometry g in _geometryPlugins)
            {
                result.AddRange(g.GetAreasOfLocation(loc));
            }
            return result;
        }

        public List<Framework.Data.AreaInfo> GetAreasOfLocation(Framework.Data.Location loc, List<Framework.Data.AreaInfo> inAreas)
        {
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            foreach (Framework.Interfaces.IGeometry g in _geometryPlugins)
            {
                result.AddRange(g.GetAreasOfLocation(loc, inAreas));
            }
            return result;
        }

        public List<Framework.Data.AreaInfo> GetEnvelopAreasOfLocation(Framework.Data.Location loc)
        {
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            foreach (Framework.Interfaces.IGeometry g in _geometryPlugins)
            {
                result.AddRange(g.GetEnvelopAreasOfLocation(loc));
            }
            return result;
        }

        public List<Framework.Data.AreaInfo> GetEnvelopAreasOfLocation(Framework.Data.Location loc, List<Framework.Data.AreaInfo> inAreas)
        {
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            foreach (Framework.Interfaces.IGeometry g in _geometryPlugins)
            {
                result.AddRange(g.GetEnvelopAreasOfLocation(loc, inAreas));
            }
            return result;
        }


        public List<Framework.Data.AreaInfo> GetAreasByName(string name)
        {
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            foreach (Framework.Interfaces.IGeometry g in _geometryPlugins)
            {
                result.AddRange(g.GetAreasByName(name));
            }
            return result;
        }

        public List<Framework.Data.AreaInfo> GetAreasByName(string name, Framework.Data.AreaType level)
        {
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            foreach (Framework.Interfaces.IGeometry g in _geometryPlugins)
            {
                result.AddRange(g.GetAreasByName(name, level));
            }
            return result;
        }

        public List<Framework.Data.AreaInfo> GetAreasByID(object id)
        {
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            foreach (Framework.Interfaces.IGeometry g in _geometryPlugins)
            {
                result.AddRange(g.GetAreasByID(id));
            }
            return result;
        }

        public void GetPolygonOfArea(Framework.Data.AreaInfo area)
        {
            foreach (Framework.Interfaces.IGeometry g in _geometryPlugins)
            {
                g.GetPolygonOfArea(area);
                if (area.Polygons != null)
                {
                    break;
                }
            }
        }

        public List<Framework.Data.AreaInfo> GetAreasByLevel(Framework.Data.AreaType level)
        {
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            foreach (Framework.Interfaces.IGeometry g in _geometryPlugins)
            {
                result.AddRange(g.GetAreasByLevel(level));
            }
            return result;
        }

        public List<Framework.Data.AreaInfo> GetAreasByParentID(object parentid)
        {
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            foreach (Framework.Interfaces.IGeometry g in _geometryPlugins)
            {
                result.AddRange(g.GetAreasByParentID(parentid));
            }
            return result;
        }

    }
}
