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

        public SqliteSettingsStorage()
        {
            _availableKeys = new Hashtable();
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
    }
}
