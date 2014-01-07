using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Data.Common;

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
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
                _dbcon = null;
            }
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
                DbDataReader dr = _dbcon.ExecuteReader(string.Format("select gccode from gccomgc where bm_id='{0}'", bm.ID));
                while (dr.Read())
                {
                    result.Add(dr[0] as string);
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


    }
}
