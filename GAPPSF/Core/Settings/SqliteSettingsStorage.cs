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
            }
            catch
            {
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

        public Hashtable LoadIgnoredGeocacheNames()
        {
            Hashtable result = new Hashtable();
            foreach (DictionaryEntry kp in _ignoredGeocacheNames)
            {
                result.Add(kp.Key as string, kp.Value as string);
            }
            return result;
        }

        public Hashtable LoadIgnoredGeocacheOwners()
        {
            Hashtable result = new Hashtable();
            foreach (DictionaryEntry kp in _ignoredGeocacheOwners)
            {
                result.Add(kp.Key as string, kp.Value as string);
            }
            return result;
        }

        public void ClearGeocacheIgnoreFilters()
        {
            if (_dbcon != null)
            {
                _dbcon.ExecuteNonQuery("delete from ignoregc");
                _ignoredGeocacheCodes.Clear();
                _ignoredGeocacheNames.Clear();
                _ignoredGeocacheOwners.Clear();
            }
        }

        public void AddIgnoreGeocacheCodes(List<string> codes)
        {
            if (_dbcon != null)
            {
                foreach(string s in codes)
                {
                    if (_ignoredGeocacheCodes[s]==null)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into ignoregc (item_name, item_value) values ('code', '{0}')", s));
                        _ignoredGeocacheCodes[s] = true;
                    }
                }
            }
        }

        public void AddIgnoreGeocacheNames(List<string> names)
        {
            if (_dbcon != null)
            {
                foreach (string s in names)
                {
                    if (_ignoredGeocacheNames[s] == null)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into ignoregc (item_name, item_value) values ('name', '{0}')", s.Replace("'","''")));
                        _ignoredGeocacheNames[s] = true;
                    }
                }
            }
        }

        public void AddIgnoreGeocacheOwners(List<string> owners)
        {
            if (_dbcon != null)
            {
                foreach (string s in owners)
                {
                    if (_ignoredGeocacheOwners[s] == null)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into ignoregc (item_name, item_value) values ('owner', '{0}')", s.Replace("'", "''")));
                        _ignoredGeocacheOwners[s] = true;
                    }
                }
            }
        }

        public void DeleteIgnoreGeocacheCodes(List<string> codes)
        {
            if (_dbcon != null)
            {
                foreach (string s in codes)
                {
                    if (_ignoredGeocacheCodes[s] != null)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("delete from ignoregc where item_name='code' and item_value='{0}'", s));
                        _ignoredGeocacheCodes.Remove(s);
                    }
                }
            }
        }

        public void DeleteIgnoreGeocacheNames(List<string> names)
        {
            if (_dbcon != null)
            {
                foreach (string s in names)
                {
                    if (_ignoredGeocacheNames[s] != null)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("delete from ignoregc where item_name='name' and item_value='{0}'", s.Replace("'", "''")));
                        _ignoredGeocacheNames.Remove(s);
                    }
                }
            }
        }

        public void DeleteIgnoreGeocacheOwners(List<string> owners)
        {
            if (_dbcon != null)
            {
                foreach (string s in owners)
                {
                    if (_ignoredGeocacheOwners[s] != null)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("delete from ignoregc where item_name='owner' and item_value='{0}'", s.Replace("'", "''")));
                        _ignoredGeocacheOwners.Remove(s);
                    }
                }
            }
        }
    }
}
