using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.IO;
using System.Reflection;

namespace GlobalcachingApplication.Plugins.Geometry
{
    public class AreasPlugin: Utils.BasePlugin.Plugin, Framework.Interfaces.IGeometry
    {
        private string _databaseFilename;
        private List<Framework.Data.AreaInfo> _cachedAreaInfo = null;
        private int _maxPolyId = 0;
        private int _maxAreaInfoId = 5600;

        public override string FriendlyName
        {
            get
            {
                return "Area definitions";
            }
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Geometry;
            }
        }

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            bool result = false;
            if (base.Initialize(core))
            {
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_ADDCREATE));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_AREA));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_DELETE));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_LEVEL));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_PARENT));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_RESTOREDEFAULT));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_SET));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_IMPORTINGFILE));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_IMPORTINGPOLY));

                if (Properties.Settings.Default.UpgradeNeeded)
                {
                    Properties.Settings.Default.Upgrade();
                    Properties.Settings.Default.UpgradeNeeded = false;
                    Properties.Settings.Default.Save();
                }

                _databaseFilename = System.IO.Path.Combine(core.PluginDataPath, "geometry.db3" );
                try
                {
                    string fld = System.IO.Path.GetDirectoryName(_databaseFilename);
                    if (!System.IO.Directory.Exists(fld))
                    {
                        System.IO.Directory.CreateDirectory(fld);
                    }

                    if (Properties.Settings.Default.FirstUse && !File.Exists(_databaseFilename))
                    {
                        using (var strm = Assembly.GetExecutingAssembly().GetManifestResourceStream("GlobalcachingApplication.Plugins.Geometry.geometry.db3"))
                        {
                            byte[] data = new byte[strm.Length];
                            strm.Read(data, 0, data.Length);
                            File.WriteAllBytes(_databaseFilename, data);
                        }
                    }
                    Properties.Settings.Default.FirstUse = false;
                    Properties.Settings.Default.Save();

                    result = true;
                }
                catch
                {
                }
            }
            return result;
        }

        internal void RestoreDefaultDatabase()
        {
            try
            {
                _cachedAreaInfo = null;
                using (var strm = Assembly.GetExecutingAssembly().GetManifestResourceStream("GlobalcachingApplication.Plugins.Geometry.geometry.db3"))
                {
                    byte[] data = new byte[strm.Length];
                    strm.Read(data, 0, data.Length);
                    File.WriteAllBytes(_databaseFilename, data);
                }
            }
            catch
            {
            }
        }

        public List<Framework.Data.AreaInfo> GetAreasOfLocation(Framework.Data.Location loc)
        {
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            PreLoadAreaInfo();
            List<Framework.Data.AreaInfo> affectedAreas = (from ai in _cachedAreaInfo
                                                           where ai.MinLat <= loc.Lat && ai.MaxLat >= loc.Lat && ai.MinLon <= loc.Lon && ai.MaxLon >= loc.Lon
                                                           select ai).ToList();
            foreach (Framework.Data.AreaInfo ai in affectedAreas)
            {
                GetPolygonOfArea(ai);
                if (ai.Polygons != null)
                {
                    List<Framework.Data.Polygon> pgs = (from pg in ai.Polygons
                                                        where pg.MinLat <= loc.Lat && pg.MaxLat >= loc.Lat && pg.MinLon <= loc.Lon && pg.MaxLon >= loc.Lon
                                                        select pg).ToList();
                    foreach (Framework.Data.Polygon pg in pgs)
                    {
                        if (Utils.Calculus.PointInPolygon(pg, loc))
                        {
                            result.Add(ai);
                            break;
                        }
                    }
                }
            }
            return result;
        }

        public List<Framework.Data.AreaInfo> GetAreasOfLocation(Framework.Data.Location loc, List<Framework.Data.AreaInfo> inAreas)
        {
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            PreLoadAreaInfo();
            List<Framework.Data.AreaInfo> affectedAreas = (from ai in _cachedAreaInfo join b in inAreas on ai equals b
                                                           where ai.MinLat <= loc.Lat && ai.MaxLat >= loc.Lat && ai.MinLon <= loc.Lon && ai.MaxLon >= loc.Lon
                                                           select ai).ToList();
            foreach (Framework.Data.AreaInfo ai in affectedAreas)
            {
                GetPolygonOfArea(ai);
                if (ai.Polygons != null)
                {
                    List<Framework.Data.Polygon> pgs = (from pg in ai.Polygons
                                                        where pg.MinLat <= loc.Lat && pg.MaxLat >= loc.Lat && pg.MinLon <= loc.Lon && pg.MaxLon >= loc.Lon
                                                        select pg).ToList();
                    foreach (Framework.Data.Polygon pg in pgs)
                    {
                        if (Utils.Calculus.PointInPolygon(pg, loc))
                        {
                            result.Add(ai);
                            break;
                        }
                    }
                }
            }
            return result;
        }

        public List<Framework.Data.AreaInfo> GetEnvelopAreasOfLocation(Framework.Data.Location loc)
        {
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            PreLoadAreaInfo();
            List<Framework.Data.AreaInfo> affectedAreas = (from ai in _cachedAreaInfo
                                                           where ai.MinLat <= loc.Lat && ai.MaxLat >= loc.Lat && ai.MinLon <= loc.Lon && ai.MaxLon >= loc.Lon
                                                           select ai).ToList();
            foreach (Framework.Data.AreaInfo ai in affectedAreas)
            {
                GetPolygonOfArea(ai);
                if (ai.Polygons != null)
                {
                    if ((from pg in ai.Polygons
                         where pg.MinLat <= loc.Lat && pg.MaxLat >= loc.Lat && pg.MinLon <= loc.Lon && pg.MaxLon >= loc.Lon
                         select pg).Count() > 0)
                    {
                        result.Add(ai);
                    }
                }
            }
            return result;
        }

        public List<Framework.Data.AreaInfo> GetEnvelopAreasOfLocation(Framework.Data.Location loc, List<Framework.Data.AreaInfo> inAreas)
        {
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            PreLoadAreaInfo();
            List<Framework.Data.AreaInfo> affectedAreas = (from ai in _cachedAreaInfo
                                                           join b in inAreas on ai equals b
                                                           where ai.MinLat <= loc.Lat && ai.MaxLat >= loc.Lat && ai.MinLon <= loc.Lon && ai.MaxLon >= loc.Lon
                                                           select ai).ToList();
            foreach (Framework.Data.AreaInfo ai in affectedAreas)
            {
                GetPolygonOfArea(ai);
                if (ai.Polygons != null)
                {
                    if ((from pg in ai.Polygons
                         where pg.MinLat <= loc.Lat && pg.MaxLat >= loc.Lat && pg.MinLon <= loc.Lon && pg.MaxLon >= loc.Lon
                         select pg).Count() > 0)
                    {
                        result.Add(ai);
                    }
                }
            }
            return result;
        }


        public List<Framework.Data.AreaInfo> GetAreasByName(string name)
        {
            PreLoadAreaInfo();
            return (from ai in _cachedAreaInfo where ai.Name.ToLower() == name.ToLower() select ai).ToList();
        }

        public List<Framework.Data.AreaInfo> GetAreasByLevel(Framework.Data.AreaType level)
        {
            PreLoadAreaInfo();
            return (from ai in _cachedAreaInfo where ai.Level == level select ai).ToList();
        }

        public List<Framework.Data.AreaInfo> GetAreasByName(string name, Framework.Data.AreaType level)
        {
            PreLoadAreaInfo();
            return (from ai in _cachedAreaInfo where ai.Name.ToLower() == name.ToLower() && ai.Level==level select ai).ToList();
        }

        public List<Framework.Data.AreaInfo> GetAreasByID(object id)
        {
            PreLoadAreaInfo();
            return (from ai in _cachedAreaInfo where ai.ID == id select ai).ToList();
        }

        public void GetPolygonOfArea(Framework.Data.AreaInfo area)
        {
            PreLoadAreaInfo();
            if (area.Polygons == null)
            {
                Framework.Data.AreaInfo ainf = (from ai in _cachedAreaInfo where ai.ID == area.ID select ai).FirstOrDefault();
                if (ainf!=null)
                {
                    try
                    {
                        area.Polygons = new List<Framework.Data.Polygon>();
                        using (Utils.DBCon dbcon = new Utils.DBConComSqlite(_databaseFilename))
                        {
                            int actPolyId = -1;
                            Framework.Data.Polygon polg = null;
                            DbDataReader dr = dbcon.ExecuteReader(string.Format("select * from poly where areainfoid={0} order by id, position", area.ID));
                            while (dr.Read())
                            {
                                int polyId = (int)dr["id"];
                                if (actPolyId != polyId)
                                {
                                    polg = new Framework.Data.Polygon();
                                    area.Polygons.Add(polg);
                                    actPolyId = polyId;
                                }
                                polg.AddLocation(new Framework.Data.Location((double)dr["lat"], (double)dr["lon"]));
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        public List<Framework.Data.AreaInfo> GetAreasByParentID(object parentid)
        {
            PreLoadAreaInfo();
            return (from ai in _cachedAreaInfo where ai.ParentID==parentid select ai).ToList();
        }

        private void PreLoadAreaInfo()
        {
            if (_cachedAreaInfo == null)
            {
                _cachedAreaInfo = new List<Framework.Data.AreaInfo>();

                try
                {
                    using (Utils.DBCon dbcon = new Utils.DBConComSqlite(_databaseFilename))
                    {
                        if (InitDatabase(dbcon))
                        {
                            DbDataReader dr = dbcon.ExecuteReader("select * from areainfo");
                            while (dr.Read())
                            {
                                Framework.Data.AreaInfo ai = new Framework.Data.AreaInfo();
                                ai.ID = (int)dr["id"];
                                ai.ParentID = dr["parentid"] == DBNull.Value ? null : dr["parentid"];
                                ai.Level = (Framework.Data.AreaType)(short)(int)dr["level"];
                                ai.Name = (string)dr["name"];
                                ai.MinLat = (double)dr["minlat"];
                                ai.MinLon = (double)dr["minlon"];
                                ai.MaxLat = (double)dr["maxlat"];
                                ai.MaxLon = (double)dr["maxlon"];
                                _cachedAreaInfo.Add(ai);
                            }
                        }

                        object o = dbcon.ExecuteScalar("select Max(id) from areainfo");
                        if (o != null)
                        {
                            _maxAreaInfoId = (int)(long)o;
                        }
                        o = dbcon.ExecuteScalar("select Max(id) from poly");
                        if (o != null)
                        {
                            _maxPolyId = (int)(long)o;
                        }
                    }
                }
                catch
                {
                    //corrupt file
                    _cachedAreaInfo.Clear();
                }
            }
        }

        public bool InitDatabase(Utils.DBCon dbcon)
        {
            /*
             * database:
             * areainfo -> id, parentid, level, name, minlat, minlon, maxlat, maxlon
             * poly -> id, areainfoid, position, lat, lon
             */
            bool result = false;
            try
            {
                object o = dbcon.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='areainfo'");
                if (o == null || o.GetType() == typeof(DBNull))
                {
                    dbcon.ExecuteNonQuery("create table 'areainfo' (id integer, parentid integer, level integer, name text, minlat real, minlon real, maxlat real, maxlon real)");
                }

                o = dbcon.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='poly'");
                if (o == null || o.GetType() == typeof(DBNull))
                {
                    dbcon.ExecuteNonQuery("create table 'poly' (id integer, areainfoid integer, position integer, lat real, lon real)");
                    dbcon.ExecuteNonQuery("create index idx_poly on poly (areainfoid)");
                }

                result = true;
            }
            catch
            {
            }
            return result;
        }

        public override List<System.Windows.Forms.UserControl> CreateConfigurationPanels()
        {
            List<System.Windows.Forms.UserControl> pnls = base.CreateConfigurationPanels();
            if (pnls == null) pnls = new List<System.Windows.Forms.UserControl>();
            pnls.Add(new SettingsPanel(this));
            return pnls;
        }

        internal bool setParent(Framework.Data.AreaInfo ai, Framework.Data.AreaInfo parent)
        {
            bool result = true;
            try
            {
                using (Utils.DBCon dbcon = new Utils.DBConComSqlite(_databaseFilename))
                {
                    dbcon.ExecuteNonQuery(string.Format("update areainfo set parentid= where id={0}", ai.ID, parent == null ? "NULL" : parent.ID));
                    ai.ParentID = parent == null ? null : parent.ID;
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }

        internal bool deleteArea(Framework.Data.AreaInfo ai)
        {
            bool result = true;
            try
            {
                using (Utils.DBCon dbcon = new Utils.DBConComSqlite(_databaseFilename))
                {
                    dbcon.ExecuteNonQuery(string.Format("delete from poly where areainfoid={0}", ai.ID));
                    dbcon.ExecuteNonQuery(string.Format("delete from areainfo where id={0}", ai.ID));
                    _cachedAreaInfo.Remove(ai);
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }

        internal bool deleteArea(Framework.Data.AreaType level, string areaName)
        {
            PreLoadAreaInfo();
            bool result = true;
            try
            {
                List<Framework.Data.AreaInfo> ail = GetAreasByName(areaName, level);
                if (ail.Count > 0)
                {
                    foreach (Framework.Data.AreaInfo ai in ail)
                    {
                        using (Utils.DBCon dbcon = new Utils.DBConComSqlite(_databaseFilename))
                        {
                            dbcon.ExecuteNonQuery(string.Format("delete from poly where areainfoid={0}", ai.ID));
                            dbcon.ExecuteNonQuery(string.Format("delete from areainfo where id={0}", ai.ID));
                            _cachedAreaInfo.Remove(ai);
                        }
                    }
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }

        internal bool processGpxFile(Framework.Data.AreaType level, string f, Framework.Data.AreaInfo parent)
        {
            PreLoadAreaInfo();
            bool result = false;
            try
            {
            }
            catch 
            { 
            }
            return result;
        }

        internal bool processTextFile(Framework.Data.AreaType level, string f, Framework.Data.AreaInfo parent)
        {
            PreLoadAreaInfo();
            bool result = false;
            try
            {
                //filename = AreaName
                string areaName = System.IO.Path.GetFileNameWithoutExtension(f);
                char[] num = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                int pos = 0;
                while (pos < areaName.Length && !num.Contains(areaName[pos])) pos++;
                areaName = areaName.Substring(0, pos);
                string[] lines = System.IO.File.ReadAllLines(f);
                pos = 0;
                if (lines[0].StartsWith("# GsakName="))
                {
                    areaName = lines[0].Substring(lines[0].IndexOf('=')+1).Trim();
                    pos++;
                }
                string slat = "";
                string slon = "";
                double lat;
                double lon;
                bool newPoly;
                Framework.Data.AreaInfo ai;
                List<Framework.Data.AreaInfo> ailist = GetAreasByName(areaName, level);
                bool firstPoint = true;
                if (ailist.Count > 0)
                {
                    ai = ailist[0];
                    firstPoint = false;
                    newPoly = false;
                }
                else
                {
                    newPoly = true;
                    ai = new Framework.Data.AreaInfo();
                    ai.ID = _maxAreaInfoId + 1;
                    _maxAreaInfoId++;
                    ai.Name = areaName;
                    ai.Level = level;
                    ai.Polygons = new List<Framework.Data.Polygon>();
                }
                Framework.Data.Polygon poly = new Framework.Data.Polygon();
                _maxPolyId++;
                using (Utils.DBCon dbcon = new Utils.DBConComSqlite(_databaseFilename))
                {
                    dbcon.BeginTran();
                    while (pos < lines.Length)
                    {
                        string s = lines[pos].Trim();
                        if (!s.StartsWith("#"))
                        {
                            string[] parts = s.Split(new char[] { ' ', ',', '\t'},  StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length == 2)
                            {
                                lat = Utils.Conversion.StringToDouble(parts[0]);
                                lon = Utils.Conversion.StringToDouble(parts[1]);
                                if (firstPoint)
                                {
                                    ai.MinLat = lat;
                                    ai.MaxLat = lat;
                                    ai.MinLon = lon;
                                    ai.MaxLon = lon;
                                    firstPoint = false;
                                }
                                else
                                {
                                    if (lat < ai.MinLat) ai.MinLat = lat;
                                    if (lat > ai.MaxLat) ai.MaxLat = lat;
                                    if (lon < ai.MinLon) ai.MinLon = lon;
                                    if (lon > ai.MaxLon) ai.MaxLon = lon;
                                }
                                poly.AddLocation(new Framework.Data.Location(lat, lon));
                                dbcon.ExecuteNonQuery(string.Format("insert into poly (id, areainfoid, position, lat, lon) values ({0}, {1}, {2}, {3}, {4})", _maxPolyId, ai.ID, poly.Count, lat.ToString().Replace(',', '.'), lon.ToString().Replace(',', '.')));
                                if (poly.Count == 1)
                                {
                                    slat = parts[0];
                                    slon = parts[1];
                                    ai.Polygons.Add(poly);
                                }
                                else if (slat == parts[0] && slon == parts[1])
                                {
                                    poly = new Framework.Data.Polygon();
                                    _maxPolyId++;
                                }
                            }
                        }
                        pos++;
                    }
                    if (newPoly)
                    {
                        _cachedAreaInfo.Add(ai);
                        dbcon.ExecuteNonQuery(string.Format("insert into areainfo (id, parentid, level, name, minlat, minlon, maxlat, maxlon) values ({0}, {1}, {2}, '{3}', {4}, {5}, {6}, {7})", ai.ID, parent == null ? "NULL" : parent.ID, (short)ai.Level, ai.Name.Replace("'", "''"), ai.MinLat.ToString().Replace(',', '.'), ai.MinLon.ToString().Replace(',', '.'), ai.MaxLat.ToString().Replace(',', '.'), ai.MaxLon.ToString().Replace(',', '.')));
                    }
                    else
                    {
                        dbcon.ExecuteNonQuery(string.Format("update areainfo set parentid={0}, minlat={1}, minlon={2}, maxlat={3}, maxlon={4} where id={5}", parent == null ? "NULL" : parent.ID, ai.MinLat.ToString().Replace(',', '.'), ai.MinLon.ToString().Replace(',', '.'), ai.MaxLat.ToString().Replace(',', '.'), ai.MaxLon.ToString().Replace(',', '.'), ai.ID));
                    }
                    dbcon.CommitTran();
                    result = true;
                }
            }
            catch
            {
            }
            return result;
        }

    }
}
