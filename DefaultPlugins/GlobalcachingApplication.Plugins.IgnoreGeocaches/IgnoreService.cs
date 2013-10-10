using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data.Common;

namespace GlobalcachingApplication.Plugins.IgnoreGeocaches
{
    public class IgnoreService
    {
        private static IgnoreService _uniqueInstance = null;
        private static object _lockObject = new object();

        public enum FilterField
        {
            GeocacheCode,
            GeocacheName,
            GeocacheOwner,
        }

        private string _databaseFile = null;
        private Hashtable _filterFields = null;
        private Framework.Interfaces.ICore _core = null;

        private IgnoreService(Framework.Interfaces.ICore core)
        {
            _filterFields = new Hashtable();
            _core = core;

            foreach (string s in Enum.GetNames(typeof(FilterField)))
            {
                _filterFields.Add(s, new Hashtable());
            }

            try
            {
                _databaseFile = System.IO.Path.Combine(core.PluginDataPath, "ignoregc.db3");
                InitDatabase();
                using (Utils.DBCon dbcon = new Utils.DBConComSqlite(_databaseFile))
                {
                    DbDataReader dr = dbcon.ExecuteReader("select field, filter from filterfields");
                    while (dr.Read())
                    {
                        Hashtable ht = _filterFields[dr["field"]] as Hashtable;
                        if (ht != null)
                        {
                            ht.Add(dr["filter"], true);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public string[] GetFilters(FilterField field)
        {
            Hashtable ht = _filterFields[field.ToString()] as Hashtable;
            if (ht != null)
            {
                ArrayList al = new ArrayList(ht.Keys);
                return ((string[])al.ToArray(typeof(string)));
            }
            else
            {
                return new string[0];
            }
        }

        public void Clear()
        {
            try
            {
                foreach (Hashtable ht in _filterFields.Values)
                {
                    ht.Clear();
                }
                using (Utils.DBCon dbcon = new Utils.DBConComSqlite(_databaseFile))
                {
                    dbcon.ExecuteNonQuery("delete from filterfields");
                }
            }
            catch
            {
            }
        }

        public void RemoveFilter(FilterField field, string filter)
        {
            try
            {
                Hashtable ht = _filterFields[field.ToString()] as Hashtable;
                if (ht != null)
                {
                    if (ht[filter]!=null)
                    {
                        ht.Remove(filter);
                        using (Utils.DBCon dbcon = new Utils.DBConComSqlite(_databaseFile))
                        {
                            dbcon.ExecuteNonQuery(string.Format("delete from filterfields where field = '{0}' and filter = '{1}'", field.ToString(), filter.Replace("'", "''")));
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public void AddFilter(FilterField field, string filter)
        {
            try
            {
                Hashtable ht = _filterFields[field.ToString()] as Hashtable;
                if (ht != null)
                {
                    if (ht[filter]==null)
                    {
                        ht.Add(filter, true);
                        using (Utils.DBCon dbcon = new Utils.DBConComSqlite(_databaseFile))
                        {
                            dbcon.ExecuteNonQuery(string.Format("insert into filterfields (field, filter) values ('{0}', '{1}')", field.ToString(), filter.Replace("'", "''")));
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public void AddCode(string code)
        {
            AddFilter(FilterField.GeocacheCode, code.ToUpper());
        }

        public void AddCodes(List<string> codes)
        {
            try
            {
                using (Utils.DBCon dbcon = new Utils.DBConComSqlite(_databaseFile))
                {
                    Hashtable ht = _filterFields[FilterField.GeocacheCode.ToString()] as Hashtable;
                    if (ht != null)
                    {
                        foreach (string filter in codes)
                        {
                            if (ht[filter.ToUpper()]==null)
                            {
                                ht.Add(filter.ToUpper(), true);
                                dbcon.ExecuteNonQuery(string.Format("insert into filterfields (field, filter) values ('{0}', '{1}')", FilterField.GeocacheCode.ToString(), filter.ToUpper().Replace("'", "''")));
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void InitDatabase()
        {
            try
            {
                if (!string.IsNullOrEmpty(_databaseFile))
                {
                    Utils.DBCon dbcon = new Utils.DBConComSqlite(_databaseFile);
                    object o = dbcon.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='filterfields'");
                    if (o == null || o.GetType() == typeof(DBNull))
                    {
                        dbcon.ExecuteNonQuery("create table 'filterfields' (field text, filter text)");
                    }
                }
            }
            catch
            {
            }
        }

        public static IgnoreService Instance(Framework.Interfaces.ICore core)
        {
            if (_uniqueInstance == null)
            {
                lock (_lockObject)
                {
                    if (_uniqueInstance == null)
                    {
                        _uniqueInstance = new IgnoreService(core);
                    }
                }
            }
            return _uniqueInstance;
        }

        public bool IgnoreGeocache(string code)
        {
            return (_filterFields[FilterField.GeocacheCode.ToString()] as Hashtable)[code] != null;
        }

        public bool IgnoreGeocache(Framework.Data.Geocache gc)
        {
            bool result = false;
            if ((_filterFields[FilterField.GeocacheCode.ToString()] as Hashtable)[gc.Code]!=null)
            {
                result = true;
            }
            else
            {
                if (!string.IsNullOrEmpty(gc.Name))
                {
                    foreach (string s in (_filterFields[FilterField.GeocacheName.ToString()] as Hashtable).Keys)
                    {
                        if (gc.Name.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0)
                        {
                            result = true;
                            break;
                        }
                    }
                }
                if (!result && !string.IsNullOrEmpty(gc.Owner))
                {
                    foreach (string s in (_filterFields[FilterField.GeocacheOwner.ToString()] as Hashtable).Keys)
                    {
                        if (string.Compare(gc.Owner, s, true) == 0)
                        {
                            result = true;
                            break;
                        }
                    }
                }
                
            }
            return result;
        }

        public List<string> FilterGeocaches(List<string> codes)
        {
            if (codes != null)
            {
                int index = 0;
                Hashtable ht = _filterFields[FilterField.GeocacheCode.ToString()] as Hashtable;
                while (index < codes.Count)
                {
                    if (ht[codes[index]]!=null)
                    {
                        codes.RemoveAt(index);
                    }
                    else
                    {
                        index++;
                    }
                }
            }
            return codes;
        }
    }
}
