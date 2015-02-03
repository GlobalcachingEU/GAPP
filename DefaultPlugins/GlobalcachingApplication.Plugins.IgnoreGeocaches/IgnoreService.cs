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

        private Hashtable _filterFields = null;
        private Framework.Interfaces.ICore _core = null;

        public class FilterFieldPoco
        {
            public string field { get; set; }
            public string filter { get; set; }
        }

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
                InitDatabase(core);
                lock (core.SettingsProvider)
                {
                    List<FilterFieldPoco> pocos = core.SettingsProvider.Database.Fetch<FilterFieldPoco>(string.Format("select * from {0}", core.SettingsProvider.GetFullTableName("filterfields")));
                    foreach (var poco in pocos)
                    {
                        Hashtable ht = _filterFields[poco.field] as Hashtable;
                        if (ht != null)
                        {
                            ht.Add(poco.filter, true);
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
                lock (_core.SettingsProvider)
                {
                    _core.SettingsProvider.Database.Execute(string.Format("delete from {0}", _core.SettingsProvider.GetFullTableName("filterfields")));
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
                        lock (_core.SettingsProvider)
                        {
                            _core.SettingsProvider.Database.Execute(string.Format("delete from {2} where field = '{0}' and filter = '{1}'", field.ToString(), filter.Replace("'", "''"), _core.SettingsProvider.GetFullTableName("filterfields")));
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
                        lock (_core.SettingsProvider)
                        {
                            _core.SettingsProvider.Database.Execute(string.Format("insert into {2} (field, filter) values ('{0}', '{1}')", field.ToString(), filter.Replace("'", "''"), _core.SettingsProvider.GetFullTableName("filterfields")));
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
                lock (_core.SettingsProvider)
                {
                    Hashtable ht = _filterFields[FilterField.GeocacheCode.ToString()] as Hashtable;
                    if (ht != null)
                    {
                        foreach (string filter in codes)
                        {
                            if (ht[filter.ToUpper()]==null)
                            {
                                ht.Add(filter.ToUpper(), true);
                                _core.SettingsProvider.Database.Execute(string.Format("insert into {2} (field, filter) values ('{0}', '{1}')", FilterField.GeocacheCode.ToString(), filter.ToUpper().Replace("'", "''"), _core.SettingsProvider.GetFullTableName("filterfields")));
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void InitDatabase(Framework.Interfaces.ICore core)
        {
            try
            {
                lock (core.SettingsProvider)
                {
                    if (!core.SettingsProvider.TableExists(core.SettingsProvider.GetFullTableName("filterfields")))
                    {
                        core.SettingsProvider.Database.Execute(string.Format("create table '{0}' (field text, filter text)", core.SettingsProvider.GetFullTableName("filterfields")));
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
