using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.SHP
{
    public class ShapeFilesManager
    {
        private List<ShapeFile> _shapeFiles = new List<ShapeFile>();

        public ShapeFilesManager()
        {
        }

        public bool Initialize()
        {
            Clear();

            foreach (string shpFile in Properties.Settings.Default.ShapeFiles)
            {
                //enabled
                //file name
                //table name for name of area
                //Coord type
                //Area type
                //name prefix
                string[] parts = shpFile.Split(new char[] { '|' }, 6);
                if (parts.Length == 6)
                {
                    try
                    {
                        if (bool.Parse(parts[0]))
                        {
                            ShapeFile sf = new ShapeFile(parts[1]);
                            if (sf.Initialize(parts[2], (ShapeFile.CoordType)Enum.Parse(typeof(ShapeFile.CoordType), parts[3]), (Framework.Data.AreaType)Enum.Parse(typeof(Framework.Data.AreaType), parts[4]), parts[5]))
                            {
                                _shapeFiles.Add(sf);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
            return true;
        }

        private void Clear()
        {
            foreach (var sf in _shapeFiles)
            {
                sf.Dispose();
            }
            _shapeFiles.Clear();
        }

        public List<Framework.Data.AreaInfo> GetAreasOfLocation(Framework.Data.Location loc)
        {
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            foreach (var sf in _shapeFiles)
            {
                List<Framework.Data.AreaInfo> areas = sf.GetAreasOfLocation(loc);
                if (areas != null)
                {
                    result.AddRange(areas);
                }
            }
            return result;
        }

        public List<Framework.Data.AreaInfo> GetAreasOfLocation(Framework.Data.Location loc, List<Framework.Data.AreaInfo> inAreas)
        {
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            foreach (var sf in _shapeFiles)
            {
                List<Framework.Data.AreaInfo> areas = sf.GetAreasOfLocation(loc, inAreas);
                if (areas != null)
                {
                    result.AddRange(areas);
                }
            }
            return result;
        }

        public List<Framework.Data.AreaInfo> GetEnvelopAreasOfLocation(Framework.Data.Location loc)
        {
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            foreach (var sf in _shapeFiles)
            {
                List<Framework.Data.AreaInfo> areas = sf.GetEnvelopAreasOfLocation(loc);
                if (areas != null)
                {
                    result.AddRange(areas);
                }
            }
            return result;
        }

        public List<Framework.Data.AreaInfo> GetEnvelopAreasOfLocation(Framework.Data.Location loc, List<Framework.Data.AreaInfo> inAreas)
        {
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            foreach (var sf in _shapeFiles)
            {
                List<Framework.Data.AreaInfo> areas = sf.GetEnvelopAreasOfLocation(loc, inAreas);
                if (areas != null)
                {
                    result.AddRange(areas);
                }
            }
            return result;
        }

        public List<Framework.Data.AreaInfo> GetAreasByName(string name)
        {
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            foreach (var sf in _shapeFiles)
            {
                result.AddRange((from a in sf.AreaInfos where a.Name == name select a).ToList());
            }
            return result;
        }

        public List<Framework.Data.AreaInfo> GetAreasByName(string name, Framework.Data.AreaType level)
        {
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            foreach (var sf in _shapeFiles)
            {
                result.AddRange((from a in sf.AreaInfos where a.Name == name && a.Level==level select a).ToList());
            }
            return result;
        }

        public List<Framework.Data.AreaInfo> GetAreasByID(object id)
        {
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            foreach (var sf in _shapeFiles)
            {
                result.AddRange((from a in sf.AreaInfos where a.ID == id select a).ToList());
            }
            return result;
        }

        public List<Framework.Data.AreaInfo> GetAreasByLevel(Framework.Data.AreaType level)
        {
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            foreach (var sf in _shapeFiles)
            {
                result.AddRange((from a in sf.AreaInfos where a.Level == level select a).ToList());
            }
            return result;
        }

        public void GetPolygonOfArea(Framework.Data.AreaInfo area)
        {
            foreach (var sf in _shapeFiles)
            {
                sf.GetPolygonOfArea(area);
                if (area.Polygons != null)
                {
                    break;
                }
            }
        }

    }
}
