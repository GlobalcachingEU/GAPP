using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Data.Common;
using System.Globalization;
using System.ComponentModel;
using System.Windows;
using System.Data;

namespace GAPPSF.Core
{
    public class SqliteSettingsStorage: ISettingsStorage
    {
        private  Utils.DBCon _dbcon = null;
        private Hashtable _availableKeys;
        private Hashtable _ignoredGeocacheCodes;
        private Hashtable _ignoredGeocacheNames;
        private Hashtable _ignoredGeocacheOwners;

        public SqliteSettingsStorage()
        {
            _availableKeys = new Hashtable();
            _ignoredGeocacheCodes = new Hashtable();
            _ignoredGeocacheNames = new Hashtable();
            _ignoredGeocacheOwners = new Hashtable();
            try
            {
                string sf = Properties.Settings.Default.SettingsFolder;
                if (string.IsNullOrEmpty(sf))
                {
                    sf = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GAPPSF");
                }
                if (!Directory.Exists(sf))
                {
                    Directory.CreateDirectory(sf);
                }
                Properties.Settings.Default.SettingsFolder = sf;
                Properties.Settings.Default.Save();

                sf = Path.Combine(sf, "settings.db3");
                //if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                {
                    _dbcon = new Utils.DBConComSqlite(sf);

                    if (!_dbcon.TableExists("settings"))
                    {
                        _dbcon.ExecuteNonQuery("create table 'settings' (item_name text, item_value text)");
                        _dbcon.ExecuteNonQuery("create index idx_key on settings (item_name)");
                    }
                    else
                    {
                        DbDataReader dr = _dbcon.ExecuteReader("select item_name, item_value from settings");
                        while (dr.Read())
                        {
                            _availableKeys[dr[0] as string] = dr[1] as string;
                        }
                    }
                    if (!_dbcon.TableExists("ignoregc"))
                    {
                        _dbcon.ExecuteNonQuery("create table 'ignoregc' (item_name text, item_value text)");
                    }
                    else
                    {
                        DbDataReader dr = _dbcon.ExecuteReader("select item_name, item_value from ignoregc");
                        while (dr.Read())
                        {
                            string item = dr[0] as string;
                            if (item == "code")
                            {
                                _ignoredGeocacheCodes[dr[1] as string] = true;
                            }
                            else if (item == "name")
                            {
                                _ignoredGeocacheNames[dr[1] as string] = true;
                            }
                            else if (item == "owner")
                            {
                                _ignoredGeocacheOwners[dr[1] as string] = true;
                            }
                        }
                    }
                    if (!_dbcon.TableExists("gccombm"))
                    {
                        _dbcon.ExecuteNonQuery("create table 'gccombm' (bm_id text, bm_name text, bmguid text)");
                        _dbcon.ExecuteNonQuery("create index idx_bmid on gccombm (bm_id)");
                    }
                    if (!_dbcon.TableExists("gccomgc"))
                    {
                        _dbcon.ExecuteNonQuery("create table 'gccomgc' (bm_id text, gccode text)");
                        _dbcon.ExecuteNonQuery("create index idx_bmgcid on gccomgc (bm_id)");
                    }
                    if (!_dbcon.TableExists("attachm"))
                    {
                        _dbcon.ExecuteNonQuery("create table 'attachm' (gccode text, filename text, comments text)");
                        _dbcon.ExecuteNonQuery("create index idx_att on attachm (gccode)");
                    }
                    if (!_dbcon.TableExists("formulasolv"))
                    {
                        _dbcon.ExecuteNonQuery("create table 'formulasolv' (gccode text, formula text)");
                        _dbcon.ExecuteNonQuery("create index idx_form on formulasolv (gccode)");
                    }
                    if (!_dbcon.TableExists("gcnotes"))
                    {
                        _dbcon.ExecuteNonQuery("create table 'gcnotes' (gccode text, notes text)");
                        _dbcon.ExecuteNonQuery("create index idx_note on gcnotes (gccode)");
                    }
                    if (!_dbcon.TableExists("gccollection"))
                    {
                        _dbcon.ExecuteNonQuery("create table 'gccollection' (col_id integer primary key autoincrement, name text)");
                        //_dbcon.ExecuteNonQuery("create index idx_col on gccollection (name)");
                    }
                    if (!_dbcon.TableExists("gcincol"))
                    {
                        _dbcon.ExecuteNonQuery("create table 'gcincol' (col_id integer, gccode text)");
                        _dbcon.ExecuteNonQuery("create index idx_gccol on gcincol (col_id)");
                    }
                    if (!_dbcon.TableExists("gcdist"))
                    {
                        _dbcon.ExecuteNonQuery("create table 'gcdist' (gccode text, dist float)");
                        _dbcon.ExecuteNonQuery("create index idx_dist on gcdist (gccode)");
                    }
                    if (!_dbcon.TableExists("gcvotes"))
                    {
                        _dbcon.ExecuteNonQuery("create table 'gcvotes' (gccode text, VoteMedian float, VoteAvg float, VoteCnt integer, VoteUser float)");
                        _dbcon.ExecuteNonQuery("create unique index idx_gcvotes on gcvotes (gccode)");
                    }

                    if (!_dbcon.TableExists("trkgroups"))
                    {
                        _dbcon.ExecuteNonQuery("create table 'trkgroups' (id integer, name text)");
                    }
                    if (!_dbcon.TableExists("trkimages"))
                    {
                        _dbcon.ExecuteNonQuery("create table 'trkimages' (url text, imagedata blob)");
                    }
                    if (!_dbcon.TableExists("trktrackables"))
                    {
                        _dbcon.ExecuteNonQuery("create table 'trktrackables' (groupid integer, AllowedToBeCollected integer, Archived integer, BugTypeID integer, Code text, CurrentGeocacheCode text, CurrentGoal text, DateCreated text, Description text, IconUrl text, Id integer, InCollection integer, Name text, TBTypeName text, Url text, WptTypeID integer, Owner text, HopCount integer, DiscoverCount integer, InCacheCount integer, DistanceKm real, Lat real, Lon real)");
                        _dbcon.ExecuteNonQuery("create index idx_trackablesgroup on trktrackables (groupid)");
                        _dbcon.ExecuteNonQuery("create index idx_trackablescode on trktrackables (code)");
                    }
                    if (!_dbcon.TableExists("trktravels"))
                    {
                        _dbcon.ExecuteNonQuery("create table 'trktravels' (pos integer, TrackableCode text, GeocacheCode text, lat real, lon real, DateLogged text)");
                        _dbcon.ExecuteNonQuery("create index idx_travels on trktravels (TrackableCode)");
                    }
                    if (!_dbcon.TableExists("trklogs"))
                    {
                        _dbcon.ExecuteNonQuery("create table 'trklogs' (TrackableCode text, ID integer, LogCode text, GeocacheCode text, IsArchived integer, LoggedBy text, LogGuid text, LogIsEncoded integer, LogText text, WptLogTypeId integer, Url text, UTCCreateDate text, VisitDate text)");
                        _dbcon.ExecuteNonQuery("create index idx_logstb on trklogs (TrackableCode)");
                        _dbcon.ExecuteNonQuery("create index idx_logsid on trklogs (ID)");
                    }


                    object o = _dbcon.ExecuteScalar("PRAGMA integrity_check");
                    if (o as string == "ok")
                    {
                        //what is expected
                    }
                    else
                    {
                        //oeps?
                        _dbcon.Dispose();
                        _dbcon = null;
                    }
                }
            }
            catch//(Exception e)
            {
                //Core.ApplicationData.Instance.Logger.AddLog(this, e);
                _dbcon = null;
            }
        }

        public bool IsStorageOK 
        {
            get
            {
                bool result;
                lock (this)
                {
                    result = _dbcon != null;
                }
                return result;
            }
        }

        public bool CreateBackup()
        {
            bool result = false;
            try
            {
                File.Copy(Path.Combine(Properties.Settings.Default.SettingsFolder, "settings.db3"), Path.Combine(Properties.Settings.Default.SettingsFolder, string.Format("settings.{0}.db3", DateTime.Now.ToString("s").Replace(" ", "-").Replace(":", "-"))), true);
                result = true;
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
            return result;
        }

        public List<string> AvailableBackups 
        {
            get
            {
                List<string> result;
                try
                {
                    string[] fls = Directory.GetFiles(Properties.Settings.Default.SettingsFolder, "settings.*.db3");
                    result = (from s in fls select Path.GetFileName(s)).OrderBy(x => x).ToList();
                }
                catch (Exception e)
                {
                    result = new List<string>();
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
                return result;
            }
        }

        public bool RemoveBackup(string id)
        {
            bool result = false;
            try
            {
                File.Delete(Path.Combine(Properties.Settings.Default.SettingsFolder, id));
                result = true;
            }
            catch (Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
            return result;
        }

        public bool PrepareRestoreBackup(string id)
        {
            bool result = false;
            lock (this)
            {
                bool restoreConnection = _dbcon != null;
                try
                {
                    if (restoreConnection)
                    {
                        //copy settings.db3 to settings.db3.bak
                        if (_dbcon != null)
                        {
                            _dbcon.Dispose();
                            _dbcon = null;
                        }
                        File.Copy(Path.Combine(Properties.Settings.Default.SettingsFolder, "settings.db3"), Path.Combine(Properties.Settings.Default.SettingsFolder, "settings.db3.bak"), true);
                    }
                    File.Copy(Path.Combine(Properties.Settings.Default.SettingsFolder, id), Path.Combine(Properties.Settings.Default.SettingsFolder, "settings.db3"), true);

                    if (restoreConnection)
                    {
                        //connect to previous settings file, so the backup is not overwritten.
                        //application needs to restart
                        //after restart the backup is used
                        string sf = Path.Combine(Properties.Settings.Default.SettingsFolder, "settings.db3.bak");
                        _dbcon = new Utils.DBConComSqlite(sf);
                    }

                    result = true;
                }
                catch (Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);

                    if (restoreConnection)
                    {
                        string sf = Path.Combine(Properties.Settings.Default.SettingsFolder, "settings.db3");
                        if (_dbcon != null)
                        {
                            _dbcon.Dispose();
                            _dbcon = null;
                        }
                        _dbcon = new Utils.DBConComSqlite(sf);
                    }
                }
            }
            return result;
        }

        public void StoreSetting(string name, string value)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (_availableKeys.ContainsKey(name))
                    {
                        _dbcon.ExecuteNonQuery(string.Format("update settings set item_value={1} where item_name='{0}'", name, value == null ? "NULL" : string.Format("'{0}'", value.Replace("'", "''"))));
                    }
                    else
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into settings (item_name, item_value) values ('{0}', {1})", name, value == null ? "NULL" : string.Format("'{0}'", value.Replace("'", "''"))));
                    }
                    _availableKeys[name] = value;
                }
            }
        }

        public Hashtable LoadSettings()
        {
            Hashtable result = new Hashtable();
            foreach (DictionaryEntry kp in _availableKeys)
            {
                result.Add(kp.Key as string, kp.Value as string);
            }
            return result;
        }

        public void Dispose()
        {
            if (_dbcon!=null)
            {
                _dbcon.Dispose();
                _dbcon = null;
            }
        }


        public Hashtable LoadIgnoredGeocacheCodes()
        {
            Hashtable result = new Hashtable();
            foreach (DictionaryEntry kp in _ignoredGeocacheCodes)
            {
                result.Add(kp.Key as string, kp.Value as string);
            }
            return result;
        }

        public List<string> LoadIgnoredGeocacheNames()
        {
            return (from string a in _ignoredGeocacheNames.Keys select a).ToList();
        }

        public List<string> LoadIgnoredGeocacheOwners()
        {
            return (from string a in _ignoredGeocacheOwners.Keys select a).ToList();
        }

        public void ClearGeocacheIgnoreFilters()
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    _dbcon.ExecuteNonQuery("delete from ignoregc");
                    _ignoredGeocacheCodes.Clear();
                    _ignoredGeocacheNames.Clear();
                    _ignoredGeocacheOwners.Clear();
                }
            }
        }

        public void AddIgnoreGeocacheCode(string code)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (_ignoredGeocacheCodes[code] == null)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into ignoregc (item_name, item_value) values ('code', '{0}')", code));
                        _ignoredGeocacheCodes[code] = true;
                    }
                }
            }
        }

        public void AddIgnoreGeocacheName(string name)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (_ignoredGeocacheNames[name] == null)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into ignoregc (item_name, item_value) values ('name', '{0}')", name.Replace("'", "''")));
                        _ignoredGeocacheNames[name] = true;
                    }
                }
            }
        }

        public void AddIgnoreGeocacheOwner(string owner)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (_ignoredGeocacheOwners[owner] == null)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into ignoregc (item_name, item_value) values ('owner', '{0}')", owner.Replace("'", "''")));
                        _ignoredGeocacheOwners[owner] = true;
                    }
                }
            }
        }

        public void DeleteIgnoreGeocacheCode(string code)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (_ignoredGeocacheCodes[code] != null)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("delete from ignoregc where item_name='code' and item_value='{0}'", code));
                        _ignoredGeocacheCodes.Remove(code);
                    }
                }
            }
        }

        public void DeleteIgnoreGeocacheName(string name)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (_ignoredGeocacheNames[name] != null)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("delete from ignoregc where item_name='name' and item_value='{0}'", name.Replace("'", "''")));
                        _ignoredGeocacheNames.Remove(name);
                    }
                }
            }
        }

        public void DeleteIgnoreGeocacheOwner(string owner)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (_ignoredGeocacheOwners[owner] != null)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("delete from ignoregc where item_name='owner' and item_value='{0}'", owner.Replace("'", "''")));
                        _ignoredGeocacheOwners.Remove(owner);
                    }
                }
            }
        }

        public List<GCComBookmarks.Bookmark> LoadGCComBookmarks()
        {
            List<GCComBookmarks.Bookmark> result = new List<GCComBookmarks.Bookmark>();
            lock (this)
            {
                if (_dbcon != null)
                {
                    DbDataReader dr = _dbcon.ExecuteReader("select bm_id, bm_name, bmguid from gccombm");
                    while (dr.Read())
                    {
                        GCComBookmarks.Bookmark bm = new GCComBookmarks.Bookmark();
                        bm.Guid = dr["bmguid"] as string;
                        bm.ID = dr["bm_id"] as string;
                        bm.Name = dr["bm_name"] as string;
                        result.Add(bm);
                    }
                }
            }
            return result;
        }

        public void AddGCComBookmark(GCComBookmarks.Bookmark bm)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    _dbcon.ExecuteNonQuery(string.Format("insert into gccombm (bm_id, bm_name, bmguid) values ('{0}','{1}','{2}')", bm.ID, bm.Name.Replace("'", "''"), bm.Guid));
                }
            }
        }

        public void DeleteGCComBookmark(GCComBookmarks.Bookmark bm)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    _dbcon.ExecuteNonQuery(string.Format("delete from gccomgc where bm_id='{0}'", bm.ID));
                    _dbcon.ExecuteNonQuery(string.Format("delete from gccombm where bm_id='{0}'", bm.ID));
                }
            }
        }

        public List<string> LoadGCComBookmarkGeocaches(GCComBookmarks.Bookmark bm)
        {
            List<string> result = new List<string>();
            lock (this)
            {
                if (_dbcon != null)
                {
                    DbDataReader dr = _dbcon.ExecuteReader(string.Format("select gccode from gccomgc where bm_id='{0}'", bm.ID));
                    while (dr.Read())
                    {
                        result.Add(dr[0] as string);
                    }
                }
            }
            return result;
        }

        public void SaveGCComBookmarkGeocaches(GCComBookmarks.Bookmark bm, List<string> gcCodes)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    List<string> gcav = LoadGCComBookmarkGeocaches(bm);
                    foreach (string gc in gcav)
                    {
                        if (!gcCodes.Contains(gc))
                        {
                            _dbcon.ExecuteNonQuery(string.Format("delete from gccomgc where bm_id='{0}' and gccode='{1}'", bm.ID, gc));
                        }
                    }
                    foreach (string gc in gcCodes)
                    {
                        if (!gcav.Contains(gc))
                        {
                            _dbcon.ExecuteNonQuery(string.Format("insert into gccomgc (bm_id, gccode) values ('{0}', '{1}')", bm.ID, gc));
                        }
                    }
                }
            }
        }


        public List<Attachement.Item> GetAttachements(string gcCode)
        {
            List<Attachement.Item> result = new List<Attachement.Item>();
            lock (this)
            {
                if (_dbcon != null)
                {
                    DbDataReader dr;
                    if (string.IsNullOrEmpty(gcCode))
                    {
                        dr = _dbcon.ExecuteReader("select gccode, filename, comments from attachm");
                    }
                    else
                    {
                        dr = _dbcon.ExecuteReader(string.Format("select gccode, filename, comments from attachm where gccode='{0}'", gcCode));
                    }
                    while (dr.Read())
                    {
                        Attachement.Item it = new Attachement.Item();
                        it.GeocacheCode = dr["gccode"] as string;
                        it.FileName = dr["filename"] as string;
                        it.Comment = dr["comments"] as string;
                        result.Add(it);
                    }
                }
            }
            return result;
        }
        public void AddAttachement(Attachement.Item item)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    _dbcon.ExecuteNonQuery(string.Format("insert into attachm (gccode, filename, comments) values ('{0}','{1}','{2}')", item.GeocacheCode, item.FileName, item.Comment.Replace("'", "''")));
                }
            }
        }
        public void DeleteAttachement(Attachement.Item item)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    _dbcon.ExecuteNonQuery(string.Format("delete from attachm where gccode='{0}' and filename='{1}' and comments='{2}'", item.GeocacheCode, item.FileName, item.Comment.Replace("'", "''")));
                }
            }
        }


        public string GetFormula(string gcCode)
        {
            string result = null;
            lock (this)
            {
                if (_dbcon != null)
                {
                    result = _dbcon.ExecuteScalar(string.Format("select formula from formulasolv where gccode='{0}'", gcCode)) as string;
                }
            }
            return result;
        }
        public void SetFormula(string gcCode, string formula)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (string.IsNullOrEmpty(formula))
                    {
                        _dbcon.ExecuteNonQuery(string.Format("delete from formulasolv where gccode='{0}'", gcCode));
                    }
                    else if (_dbcon.ExecuteNonQuery(string.Format("update formulasolv set formula='{1}' where gccode='{0}'", gcCode, formula.Replace("'","''"))) == 0)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into formulasolv (gccode, formula) values ('{0}', '{1}')", gcCode, formula.Replace("'", "''")));
                    }
                }
            }
        }

        public string GetGeocacheNotes(string gcCode)
        {
            string result = null;
            lock (this)
            {
                if (_dbcon != null)
                {
                    result = _dbcon.ExecuteScalar(string.Format("select notes from gcnotes where gccode='{0}'", gcCode)) as string;
                }
            }
            return result;
        }

        public void SetGeocacheNotes(string gcCode, string notes)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (string.IsNullOrEmpty(notes))
                    {
                        _dbcon.ExecuteNonQuery(string.Format("delete from gcnotes where gccode='{0}'", gcCode));
                    }
                    else if (_dbcon.ExecuteNonQuery(string.Format("update gcnotes set notes='{1}' where gccode='{0}'", gcCode, notes.Replace("'", "''"))) == 0)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into gcnotes (gccode, notes) values ('{0}', '{1}')", gcCode, notes.Replace("'", "''")));
                    }
                }
            }
        }

        private int getGCCollectionID(string name)
        {
            int result = -1;
            if (_dbcon != null)
            {
                DbDataReader dr = _dbcon.ExecuteReader(string.Format("select col_id from gccollection where name='{0}'", name.Replace("'", "''")));
                if (dr.Read())
                {
                    result = dr.GetInt32(0);
                }
            }
            return result;
        }
        public int GetCollectionID(string collectionName)
        {
            int result = -1;
            lock (this)
            {
                if (_dbcon != null)
                {
                    result = getGCCollectionID(collectionName);
                }
            }
            return result;
        }
        public List<string> AvailableCollections()
        {
            List<string> result = new List<string>();
            lock (this)
            {
                if (_dbcon != null)
                {
                    DbDataReader dr = _dbcon.ExecuteReader("select name from gccollection");
                    while (dr.Read())
                    {
                        result.Add(dr[0] as string);
                    }
                }
            }
            return result;
        }
        public List<string> GetGeocachesInCollection(string collectionName)
        {
            List<string> result = new List<string>();
            lock (this)
            {
                if (_dbcon != null)
                {
                    int id = getGCCollectionID(collectionName);
                    if (id >= 0)
                    {
                        DbDataReader dr = _dbcon.ExecuteReader(string.Format("select gccode from gcincol where col_id={0}", id));
                        while (dr.Read())
                        {
                            result.Add(dr[0] as string);
                        }
                    }
                }
            }
            return result;
        }
        public void AddCollection(string name)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    int id = getGCCollectionID(name);
                    if (id < 0)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into gccollection (name) values ('{0}')", name.Replace("'", "''")));
                    }
                }
            }
        }
        public void DeleteCollection(string name)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    int id = getGCCollectionID(name);
                    if (id >= 0)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("delete from gcincol where col_id={0}", id));
                        _dbcon.ExecuteNonQuery(string.Format("delete from gccollection where col_id={0}", id));
                    }
                }
            }
        }
        public void AddToCollection(string collectionName, string geocacheCode)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    int id = getGCCollectionID(collectionName);
                    if (id >= 0)
                    {
                        if (!InCollection(id, geocacheCode))
                        {
                            _dbcon.ExecuteNonQuery(string.Format("insert into gcincol (col_id, gccode) values ({0}, '{1}')", id, geocacheCode.Replace("'", "''")));
                        }
                    }
                }
            }
        }
        public void AddToCollection(int collectionID, string geocacheCode)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (!InCollection(collectionID, geocacheCode))
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into gcincol (col_id, gccode) values ({0}, '{1}')", collectionID, geocacheCode.Replace("'", "''")));
                    }
                }
            }
        }
        public void RemoveFromCollection(string collectionName, string geocacheCode)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    int id = getGCCollectionID(collectionName);
                    if (id >= 0)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("delete from gcincol where col_id={0} and gccode='{1}'", id, geocacheCode.Replace("'", "''")));
                    }
                }
            }
        }
        public void RemoveFromCollection(int collectionID, string geocacheCode)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    _dbcon.ExecuteNonQuery(string.Format("delete from gcincol where col_id={0} and gccode='{1}'", collectionID, geocacheCode.Replace("'", "''")));
                }
            }
        }
        public bool InCollection(string collectionName, string geocacheCode)
        {
            bool result = false;
            lock (this)
            {
                if (_dbcon != null)
                {
                    int id = getGCCollectionID(collectionName);
                    if (id >= 0)
                    {
                        result = (long)_dbcon.ExecuteScalar(string.Format("select count(1) from gcincol where col_id={0} and gccode='{1}'", id, geocacheCode.Replace("'", "''"))) > 0;
                    }
                }
            }
            return result;
        }
        public bool InCollection(int collectionID, string geocacheCode)
        {
            bool result = false;
            lock (this)
            {
                if (_dbcon != null)
                {
                    result = (long)_dbcon.ExecuteScalar(string.Format("select count(1) from gcincol where col_id={0} and gccode='{1}'", collectionID, geocacheCode.Replace("'", "''"))) > 0;
                }
            }
            return result;
        }

        public double? GetGeocacheDistance(string gcCode)
        {
            double? result = null;
            lock (this)
            {
                if (_dbcon != null)
                {
                    result = (double?)_dbcon.ExecuteScalar(string.Format("select dist from gcdist where gccode='{0}'", gcCode));
                }
            }
            return result;
        }
        public void SetGeocacheDistance(string gcCode, double? dist)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (dist==null)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("delete from gcdist where gccode='{0}'", gcCode));
                    }
                    else if (_dbcon.ExecuteNonQuery(string.Format("update gcdist set dist={1} where gccode='{0}'", gcCode, dist.ToString().Replace(',','.'))) == 0)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into gcdist (gccode, dist) values ('{0}', {1})", gcCode, dist.ToString().Replace(',', '.')));
                    }
                }
            }
        }

        public double? GetGCVoteMedian(string gcCode)
        {
            double? result = null;
            lock (this)
            {
                if (_dbcon != null)
                {
                    result = (double?)_dbcon.ExecuteScalar(string.Format("select VoteMedian from gcvotes where gccode='{0}'", gcCode));
                }
            }
            return result;
        }
        public double? GetGCVoteAverage(string gcCode)
        {
            double? result = null;
            lock (this)
            {
                if (_dbcon != null)
                {
                    result = (double?)_dbcon.ExecuteScalar(string.Format("select VoteAvg from gcvotes where gccode='{0}'", gcCode));
                }
            }
            return result;
        }
        public int? GetGCVoteCount(string gcCode)
        {
            int? result = null;
            lock (this)
            {
                if (_dbcon != null)
                {
                    result = (int?)_dbcon.ExecuteScalar(string.Format("select VoteCnt from gcvotes where gccode='{0}'", gcCode));
                }
            }
            return result;
        }
        public double? GetGCVoteUser(string gcCode)
        {
            double? result = null;
            lock (this)
            {
                if (_dbcon != null)
                {
                    result = (double?)_dbcon.ExecuteScalar(string.Format("select VoteUser from gcvotes where gccode='{0}'", gcCode));
                }
            }
            return result;
        }

        public void SetGCVote(string gcCode, double median, double average, int cnt, double? user)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (_dbcon.ExecuteNonQuery(string.Format("update gcvotes set VoteMedian={0}, VoteAvg={1}, VoteCnt={2}, VoteUser={3} where gccode='{4}'", median.ToString(CultureInfo.InvariantCulture), average.ToString(CultureInfo.InvariantCulture), cnt, user == null ? "null" : ((double)user).ToString(CultureInfo.InvariantCulture), gcCode)) == 0)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into gcvotes (VoteMedian, VoteAvg, VoteCnt, VoteUser, gccode) values ({0}, {1}, {2}, {3}, '{4}')", median.ToString(CultureInfo.InvariantCulture), average.ToString(CultureInfo.InvariantCulture), cnt, user == null ? "null" : ((double)user).ToString(CultureInfo.InvariantCulture), gcCode));
                    }
                }
            }
        }

        public void ClearGCVotes()
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    _dbcon.ExecuteNonQuery("delete from gcvotes");
                }
            }
        }


        public List<UIControls.Trackables.TrackableGroup> GetTrackableGroups()
        {
            List<UIControls.Trackables.TrackableGroup> result = new List<UIControls.Trackables.TrackableGroup>();
            lock (this)
            {
                if (_dbcon != null)
                {
                    DbDataReader dr = _dbcon.ExecuteReader("select id, name from trkgroups");
                    while (dr.Read())
                    {
                        UIControls.Trackables.TrackableGroup tg = new UIControls.Trackables.TrackableGroup();
                        tg.ID = (int)dr["id"];
                        tg.Name = (string)dr["name"];
                        result.Add(tg);
                    }
                }
            }
            return result;
        }
        public void AddTrackableGroup(UIControls.Trackables.TrackableGroup grp)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if ((long)_dbcon.ExecuteScalar(string.Format("select count(1) from trkgroups where id={0}", grp.ID)) == 0)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into trkgroups (id, name) values ({0}, '{1}')", grp.ID, grp.Name.Replace("'", "''")));
                    }
                }
            }
        }
        public void DeleteTrackableGroup(UIControls.Trackables.TrackableGroup grp)
        {
            lock(this)
            {
                if (_dbcon != null)
                {
                    List<UIControls.Trackables.TrackableItem> trbls = GetTrackables(grp);
                    foreach(var t in trbls)
                    {
                        DeleteTrackable(grp, t);
                    }
                    _dbcon.ExecuteNonQuery(string.Format("delete from trkgroups where id={0}", grp.ID));
                }
            }
        }
        public List<UIControls.Trackables.TrackableItem> GetTrackables(UIControls.Trackables.TrackableGroup grp)
        {
            List<UIControls.Trackables.TrackableItem> result = new List<UIControls.Trackables.TrackableItem>();
            lock (this)
            {
                if (_dbcon != null)
                {
                    DbDataReader dr = _dbcon.ExecuteReader(string.Format("select * from trktrackables where groupid={0}", grp.ID));
                    while (dr.Read())
                    {
                        UIControls.Trackables.TrackableItem trk = new UIControls.Trackables.TrackableItem();
                        trk.Code = (string)dr["Code"];
                        if (dr["AllowedToBeCollected"] != null && dr["AllowedToBeCollected"].GetType() != typeof(DBNull))
                        {
                            trk.AllowedToBeCollected = (int)dr["AllowedToBeCollected"] != 0;
                        }
                        else
                        {
                            trk.AllowedToBeCollected = null;
                        }
                        trk.Archived = (int)dr["Archived"] != 0;
                        trk.BugTypeID = (int)dr["BugTypeID"];
                        trk.CurrentGeocacheCode = (string)dr["CurrentGeocacheCode"];
                        trk.CurrentGoal = (string)dr["CurrentGoal"];
                        trk.DateCreated = DateTime.Parse((string)dr["DateCreated"]);
                        trk.Description = (string)dr["Description"];
                        trk.IconUrl = (string)dr["IconUrl"];
                        trk.Id = (int)dr["Id"];
                        trk.InCollection = (int)dr["InCollection"] != 0;
                        trk.Name = (string)dr["Name"];
                        trk.TBTypeName = (string)dr["TBTypeName"];
                        trk.Url = (string)dr["Url"];
                        trk.WptTypeID = (int)dr["WptTypeID"];
                        trk.Owner = (string)dr["Owner"];

                        if (dr["HopCount"] != null && dr["HopCount"].GetType() != typeof(DBNull))
                        {
                            trk.HopCount = (int)dr["HopCount"];
                        }
                        else
                        {
                            trk.HopCount = 0;
                        }
                        if (dr["DiscoverCount"] != null && dr["DiscoverCount"].GetType() != typeof(DBNull))
                        {
                            trk.DiscoverCount = (int)dr["DiscoverCount"];
                        }
                        else
                        {
                            trk.DiscoverCount = 0;
                        }
                        if (dr["InCacheCount"] != null && dr["InCacheCount"].GetType() != typeof(DBNull))
                        {
                            trk.InCacheCount = (int)dr["InCacheCount"];
                        }
                        else
                        {
                            trk.InCacheCount = 0;
                        }
                        if (dr["DistanceKm"] != null && dr["DistanceKm"].GetType() != typeof(DBNull))
                        {
                            trk.DistanceKm = (double)dr["DistanceKm"];
                        }
                        else
                        {
                            trk.DistanceKm = 0.0;
                        }
                        if (dr["Lat"] != null && dr["Lat"].GetType() != typeof(DBNull))
                        {
                            trk.Lat = (double)dr["Lat"];
                        }
                        else
                        {
                            trk.Lat = null;
                        }
                        if (dr["Lon"] != null && dr["Lon"].GetType() != typeof(DBNull))
                        {
                            trk.Lon = (double)dr["Lon"];
                        }
                        else
                        {
                            trk.Lon = null;
                        }

                        result.Add(trk);
                    }

                    foreach(var t in result)
                    {
                        t.IconData = _dbcon.ExecuteScalar(string.Format("select imagedata from trkimages where url='{0}'", t.IconUrl ?? "")) as byte[];
                    }
                }
            }
            return result;
        }
        public byte[] GetTrackableIconData(string iconUrl)
        {
            byte[] result = null;
            lock (this)
            {
                if (_dbcon != null)
                {
                    result = _dbcon.ExecuteScalar(string.Format("select imagedata from trkimages where url='{0}'", iconUrl ?? "")) as byte[];
                }
            }
            return result;
        }

        public void AddUpdateTrackable(UIControls.Trackables.TrackableGroup grp, UIControls.Trackables.TrackableItem trackable)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    try
                    {
                        UIControls.Trackables.TrackableItem t = trackable;
                        DbParameter par;

                        if (!string.IsNullOrEmpty(trackable.IconUrl) && trackable.IconData != null)
                        {
                            long cnt = (long)_dbcon.ExecuteScalar(string.Format("select count(1) from trkimages where url='{0}'", t.IconUrl));
                            if (cnt == 0)
                            {
                                _dbcon.Command.Parameters.Clear();
                                par = _dbcon.Command.CreateParameter();
                                par.ParameterName = "@data";
                                par.DbType = DbType.Binary;
                                par.Value = trackable.IconData;
                                _dbcon.Command.Parameters.Add(par);
                                _dbcon.ExecuteNonQuery(string.Format("insert into trkimages (url, imagedata) values ('{0}', @data)", t.IconUrl));
                                _dbcon.Command.Parameters.Clear();
                            }
                        }

                        _dbcon.Command.Parameters.Clear();
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@groupid";
                        par.DbType = DbType.Int32;
                        par.Value = grp.ID;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@AllowedToBeCollected";
                        par.DbType = DbType.Int32;
                        if (t.AllowedToBeCollected == null)
                        {
                            par.Value = DBNull.Value;
                        }
                        else
                        {
                            par.Value = t.AllowedToBeCollected == true ? 1 : 0;
                        }
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@Archived";
                        par.DbType = DbType.Int32;
                        par.Value = t.Archived ? 1 : 0;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@BugTypeID";
                        par.DbType = DbType.Int32;
                        par.Value = t.BugTypeID;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@Code";
                        par.DbType = DbType.String;
                        par.Value = t.Code;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@CurrentGeocacheCode";
                        par.DbType = DbType.String;
                        par.Value = t.CurrentGeocacheCode ?? "";
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@CurrentGoal";
                        par.DbType = DbType.String;
                        par.Value = t.CurrentGoal ?? "";
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@DateCreated";
                        par.DbType = DbType.String;
                        par.Value = t.DateCreated.ToString("u");
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@Description";
                        par.DbType = DbType.String;
                        par.Value = t.Description ?? "";
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@IconUrl";
                        par.DbType = DbType.String;
                        par.Value = t.IconUrl ?? "";
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@Id";
                        par.DbType = DbType.Int32;
                        par.Value = t.Id;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@InCollection";
                        par.DbType = DbType.Int32;
                        par.Value = t.InCollection ? 1 : 0;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@Name";
                        par.DbType = DbType.String;
                        par.Value = t.Name ?? "";
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@TBTypeName";
                        par.DbType = DbType.String;
                        par.Value = t.TBTypeName ?? "";
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@Url";
                        par.DbType = DbType.String;
                        par.Value = t.Url ?? "";
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@WptTypeID";
                        par.DbType = DbType.Int32;
                        par.Value = t.WptTypeID;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@Owner";
                        par.DbType = DbType.String;
                        par.Value = t.Owner ?? "";
                        _dbcon.Command.Parameters.Add(par);

                        if (_dbcon.ExecuteNonQuery("update trktrackables set AllowedToBeCollected=@AllowedToBeCollected, Archived=@Archived, BugTypeID=@BugTypeID, CurrentGeocacheCode=@CurrentGeocacheCode, CurrentGoal=@CurrentGoal, DateCreated=@DateCreated, Description=@Description, IconUrl=@IconUrl, Id=@Id, InCollection=@InCollection, Name=@Name, TBTypeName=@TBTypeName, Url=@Url, WptTypeID=@WptTypeID, Owner=@Owner where groupid=@groupid and Code=@Code") == 0)
                        {
                            _dbcon.ExecuteNonQuery("insert into trktrackables (groupid, AllowedToBeCollected, Archived, BugTypeID, Code, CurrentGeocacheCode, CurrentGoal, DateCreated, Description, IconUrl, Id, InCollection, Name, TBTypeName, Url, WptTypeID, Owner) values (@groupid, @AllowedToBeCollected, @Archived, @BugTypeID, @Code, @CurrentGeocacheCode, @CurrentGoal, @DateCreated, @Description, @IconUrl, @Id, @InCollection, @Name, @TBTypeName, @Url, @WptTypeID, @Owner)");
                        }
                        _dbcon.Command.Parameters.Clear();
                    }
                    finally
                    {
                        _dbcon.Command.Parameters.Clear();
                    }
                }
            }
        }
        public void DeleteTrackable(UIControls.Trackables.TrackableGroup grp, UIControls.Trackables.TrackableItem trackable)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    _dbcon.ExecuteNonQuery(string.Format("delete from trktrackables where groupid={0} and Code='{1}'", grp.ID, trackable.Code));
                    if ((long)_dbcon.ExecuteScalar(string.Format("select count(1) from trktrackables", trackable.Code)) == 0)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("delete from trktravels where TrackableCode='{0}'", trackable.Code));
                        _dbcon.ExecuteNonQuery(string.Format("delete from trklogs where TrackableCode='{0}'", trackable.Code));
                    }
                }
            }
        }
        public List<UIControls.Trackables.TravelItem> GetTrackableTravels(UIControls.Trackables.TrackableItem trackable)
        {
            List<UIControls.Trackables.TravelItem> result = new List<UIControls.Trackables.TravelItem>();
            lock (this)
            {
                if (_dbcon != null)
                {
                    DbDataReader dr = _dbcon.ExecuteReader(string.Format("select GeocacheCode, lat, lon, DateLogged from trktravels where TrackableCode='{0}' order by pos", trackable.Code));
                    while (dr.Read())
                    {
                        UIControls.Trackables.TravelItem ti = new UIControls.Trackables.TravelItem();
                        ti.DateLogged = DateTime.Parse((string)dr["DateLogged"]);
                        ti.GeocacheCode = (string)dr["GeocacheCode"];
                        ti.Lat = (double)dr["lat"];
                        ti.Lon = (double)dr["lon"];
                        result.Add(ti);
                    }
                }
            }
            return result;
        }
        public void UpdateTrackableTravels(UIControls.Trackables.TrackableItem trackable, List<UIControls.Trackables.TravelItem> travels)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    try
                    {
                        _dbcon.ExecuteNonQuery(string.Format("delete from trktravels where TrackableCode='{0}'", trackable.Code));

                        DbParameter par;
                        _dbcon.Command.Parameters.Clear();
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@pos";
                        par.DbType = DbType.Int32;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@TrackableCode";
                        par.DbType = DbType.String;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@GeocacheCode";
                        par.DbType = DbType.String;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@lat";
                        par.DbType = DbType.Double;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@lon";
                        par.DbType = DbType.Double;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@DateLogged";
                        par.DbType = DbType.String;
                        _dbcon.Command.Parameters.Add(par);
                        double DistanceKm = 0.0;
                        double? LastLat = null;
                        double? LastLon = null;
                        for (int i = 0; i < travels.Count; i++)
                        {
                            _dbcon.Command.Parameters["@pos"].Value = i;
                            _dbcon.Command.Parameters["@TrackableCode"].Value = travels[i].TrackableCode;
                            _dbcon.Command.Parameters["@GeocacheCode"].Value = travels[i].GeocacheCode ?? "";
                            _dbcon.Command.Parameters["@lat"].Value = travels[i].Lat;
                            _dbcon.Command.Parameters["@lon"].Value = travels[i].Lon;
                            _dbcon.Command.Parameters["@DateLogged"].Value = travels[i].DateLogged.ToString("u");
                            _dbcon.ExecuteNonQuery("insert into trktravels (pos, TrackableCode, GeocacheCode, lat, lon, DateLogged) values (@pos, @TrackableCode, @GeocacheCode, @lat, @lon, @DateLogged)");

                            LastLat = travels[i].Lat;
                            LastLon = travels[i].Lon;
                            if (i > 0)
                            {
                                DistanceKm += (double)Utils.Calculus.CalculateDistance(travels[i - 1].Lat, travels[i - 1].Lon, travels[i].Lat, travels[i].Lon).EllipsoidalDistance / 1000.0;
                            }
                        }
                        _dbcon.Command.Parameters.Clear();

                        if (LastLat != null && LastLon != null)
                        {
                            _dbcon.ExecuteNonQuery(string.Format("update trktrackables set DistanceKm={0}, Lat={2}, Lon={3} where Code='{1}'", DistanceKm.ToString().Replace(',', '.'), trackable.Code, LastLat.ToString().Replace(',', '.'), LastLon.ToString().Replace(',', '.')));
                        }
                    }
                    finally
                    {
                        _dbcon.Command.Parameters.Clear();
                    }
                }
            }
        }
        public List<UIControls.Trackables.LogItem> GetTrackableLogs(UIControls.Trackables.TrackableItem trackable)
        {
            List<UIControls.Trackables.LogItem> result = new List<UIControls.Trackables.LogItem>();
            lock (this)
            {
                if (_dbcon != null)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, Logger.Level.Error, "GetTrackableLogs not implemented!");
                }
            }
            return result;
        }
        public void UpdateTrackableLogs(UIControls.Trackables.TrackableItem trackable, List<UIControls.Trackables.LogItem> logs)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    try
                    {
                        List<int> logsIndb = new List<int>();
                        DbDataReader dr = _dbcon.ExecuteReader(string.Format("select ID from trklogs where TrackableCode='{0}'", trackable.Code));
                        while (dr.Read())
                        {
                            logsIndb.Add((int)dr["ID"]);
                        }

                        DbParameter par;
                        _dbcon.Command.Parameters.Clear();
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@TrackableCode";
                        par.DbType = DbType.String;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@ID";
                        par.DbType = DbType.Int32;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@LogCode";
                        par.DbType = DbType.String;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@GeocacheCode";
                        par.DbType = DbType.String;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@IsArchived";
                        par.DbType = DbType.Int32;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@LoggedBy";
                        par.DbType = DbType.String;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@LogGuid";
                        par.DbType = DbType.String;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@LogText";
                        par.DbType = DbType.String;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@LogIsEncoded";
                        par.DbType = DbType.Int32;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@WptLogTypeId";
                        par.DbType = DbType.Int32;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@Url";
                        par.DbType = DbType.String;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@UTCCreateDate";
                        par.DbType = DbType.String;
                        _dbcon.Command.Parameters.Add(par);
                        par = _dbcon.Command.CreateParameter();
                        par.ParameterName = "@VisitDate";
                        par.DbType = DbType.String;
                        _dbcon.Command.Parameters.Add(par);

                        int HopCount = 0;
                        int DiscoverCount = 0;
                        int InCacheCount = 0;
                        for (int i = 0; i < logs.Count; i++)
                        {
                            _dbcon.Command.Parameters["@TrackableCode"].Value = logs[i].TrackableCode;
                            _dbcon.Command.Parameters["@ID"].Value = logs[i].ID;
                            _dbcon.Command.Parameters["@LogCode"].Value = logs[i].LogCode;
                            _dbcon.Command.Parameters["@GeocacheCode"].Value = logs[i].GeocacheCode ?? "";
                            _dbcon.Command.Parameters["@LogIsEncoded"].Value = logs[i].LogIsEncoded ? 1 : 0;
                            _dbcon.Command.Parameters["@IsArchived"].Value = logs[i].IsArchived ? 1 : 0;
                            _dbcon.Command.Parameters["@LoggedBy"].Value = logs[i].LoggedBy ?? "";
                            _dbcon.Command.Parameters["@LogGuid"].Value = logs[i].LogGuid ?? "";
                            _dbcon.Command.Parameters["@LogText"].Value = logs[i].LogText ?? "";
                            _dbcon.Command.Parameters["@WptLogTypeId"].Value = logs[i].WptLogTypeId;
                            _dbcon.Command.Parameters["@Url"].Value = logs[i].Url ?? "";
                            _dbcon.Command.Parameters["@UTCCreateDate"].Value = logs[i].UTCCreateDate.ToString("u");
                            _dbcon.Command.Parameters["@VisitDate"].Value = logs[i].VisitDate.ToString("u");

                            if (logsIndb.Contains(logs[i].ID))
                            {
                                //for performance reasons, do not update. Assume nothing changed
                                //_dbcon.ExecuteNonQuery("update logs....");
                                logsIndb.Remove(logs[i].ID);
                            }
                            else
                            {
                                _dbcon.ExecuteNonQuery("insert into trklogs (TrackableCode, ID, LogCode, GeocacheCode, IsArchived, LoggedBy, LogGuid, LogIsEncoded, LogText, WptLogTypeId, Url, UTCCreateDate, VisitDate) values (@TrackableCode, @ID, @LogCode, @GeocacheCode, @IsArchived, @LoggedBy, @LogGuid, @LogIsEncoded, @LogText, @WptLogTypeId, @Url, @UTCCreateDate, @VisitDate)");
                            }

                            switch (logs[i].WptLogTypeId)
                            {
                                case 75: //visit
                                    HopCount++;
                                    break;
                                case 14: //dropped
                                    HopCount++;
                                    InCacheCount++;
                                    break;
                                case 48: //disc
                                    DiscoverCount++;
                                    break;
                            }
                        }
                        _dbcon.ExecuteNonQuery(string.Format("update trktrackables set HopCount={0}, InCacheCount={1}, DiscoverCount={2} where Code='{3}'", HopCount, InCacheCount, DiscoverCount, trackable.Code));
                        foreach (int id in logsIndb)
                        {
                            _dbcon.ExecuteNonQuery(string.Format("delete from trklogs where ID={0}", id));
                        }
                        _dbcon.Command.Parameters.Clear();
                    }
                    finally
                    {
                        _dbcon.Command.Parameters.Clear();
                    }
                }
            }
        }

    }
}
