using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace GAPPSF.Shapefiles
{
    public class ShapeFilesManager: INotifyPropertyChanged
    {
        private static ShapeFilesManager _uniqueInstance = null;
        private static object _lockObject = new object();

        private List<ShapeFile> _shapeFiles = new List<ShapeFile>();

        public ShapeFilesManager()
        {
#if DEBUG
            if (_uniqueInstance != null)
            {
                //you used the wrong binding
                System.Diagnostics.Debugger.Break();
            }
#endif
            Initialize();
        }

        public static ShapeFilesManager Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            _uniqueInstance = new ShapeFilesManager();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        public bool Initialize()
        {
            Clear();

            if (!string.IsNullOrEmpty(Core.Settings.Default.ShapeFiles))
            {
                string[] lines = Core.Settings.Default.ShapeFiles.Split(new char[] {'\r','\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string shpFile in lines)
                {
                    //enabled
                    //file name
                    //table name for name of area
                    //Coord type
                    //Area type
                    //name prefix
                    string[] parts = shpFile.Split(new char[] { '|' }, 7);
                    if (parts.Length >= 6)
                    {
                        try
                        {
                            if (bool.Parse(parts[0]))
                            {
                                string encoding = "";
                                if (parts.Length >= 7)
                                {
                                    encoding = parts[6];
                                }
                                ShapeFile sf = new ShapeFile(parts[1]);
                                if (sf.Initialize(parts[2], (ShapeFile.CoordType)Enum.Parse(typeof(ShapeFile.CoordType), parts[3]), (Core.Data.AreaType)Enum.Parse(typeof(Core.Data.AreaType), parts[4]), parts[5], encoding))
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
            }

            if (PropertyChanged!=null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(""));
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

        public List<Core.Data.AreaInfo> GetAreasOfLocation(Core.Data.Location loc)
        {
            List<Core.Data.AreaInfo> result = new List<Core.Data.AreaInfo>();
            foreach (var sf in _shapeFiles)
            {
                List<Core.Data.AreaInfo> areas = sf.GetAreasOfLocation(loc);
                if (areas != null)
                {
                    result.AddRange(areas);
                }
            }
            return result;
        }

        public List<Core.Data.AreaInfo> GetAreasOfLocation(Core.Data.Location loc, List<Core.Data.AreaInfo> inAreas)
        {
            List<Core.Data.AreaInfo> result = new List<Core.Data.AreaInfo>();
            foreach (var sf in _shapeFiles)
            {
                List<Core.Data.AreaInfo> areas = sf.GetAreasOfLocation(loc, inAreas);
                if (areas != null)
                {
                    result.AddRange(areas);
                }
            }
            return result;
        }

        public List<Core.Data.AreaInfo> GetEnvelopAreasOfLocation(Core.Data.Location loc)
        {
            List<Core.Data.AreaInfo> result = new List<Core.Data.AreaInfo>();
            foreach (var sf in _shapeFiles)
            {
                List<Core.Data.AreaInfo> areas = sf.GetEnvelopAreasOfLocation(loc);
                if (areas != null)
                {
                    result.AddRange(areas);
                }
            }
            return result;
        }

        public List<Core.Data.AreaInfo> GetEnvelopAreasOfLocation(Core.Data.Location loc, List<Core.Data.AreaInfo> inAreas)
        {
            List<Core.Data.AreaInfo> result = new List<Core.Data.AreaInfo>();
            foreach (var sf in _shapeFiles)
            {
                List<Core.Data.AreaInfo> areas = sf.GetEnvelopAreasOfLocation(loc, inAreas);
                if (areas != null)
                {
                    result.AddRange(areas);
                }
            }
            return result;
        }

        public List<Core.Data.AreaInfo> GetAreasByName(string name)
        {
            List<Core.Data.AreaInfo> result = new List<Core.Data.AreaInfo>();
            foreach (var sf in _shapeFiles)
            {
                result.AddRange((from a in sf.AreaInfos where a.Name == name select a).ToList());
            }
            return result;
        }

        public List<Core.Data.AreaInfo> GetAreasByName(string name, Core.Data.AreaType level)
        {
            List<Core.Data.AreaInfo> result = new List<Core.Data.AreaInfo>();
            foreach (var sf in _shapeFiles)
            {
                result.AddRange((from a in sf.AreaInfos where a.Name == name && a.Level==level select a).ToList());
            }
            return result;
        }

        public List<Core.Data.AreaInfo> GetAreasByID(object id)
        {
            List<Core.Data.AreaInfo> result = new List<Core.Data.AreaInfo>();
            foreach (var sf in _shapeFiles)
            {
                result.AddRange((from a in sf.AreaInfos where a.ID == id select a).ToList());
            }
            return result;
        }

        public List<Core.Data.AreaInfo> GetAreasByLevel(Core.Data.AreaType level)
        {
            List<Core.Data.AreaInfo> result = new List<Core.Data.AreaInfo>();
            foreach (var sf in _shapeFiles)
            {
                result.AddRange((from a in sf.AreaInfos where a.Level == level select a).ToList());
            }
            return result;
        }

        public void GetPolygonOfArea(Core.Data.AreaInfo area)
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


        public event PropertyChangedEventHandler PropertyChanged;
    }
}
