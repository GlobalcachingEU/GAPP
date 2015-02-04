using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Data.Common;

namespace GlobalcachingApplication.Plugins.Bookmark
{
    public class Repository
    {
        private static Repository _uniqueInstance = null;
        private static object _lockObject = new object();

        private Hashtable _bookmarks = null;
        private Framework.Interfaces.ICore _core = null;

        public event EventHandler DataChanged;

        public class BookmarkPoco
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        public class BookmarkCodePoco
        {
            public int BookmarkID { get; set; }
            public string Code { get; set; }
        }

        private Repository()
        {
        }

        public static Repository Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            _uniqueInstance = new Repository();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        public void Initialize(Framework.Interfaces.ICore core)
        {
            _core = core;
            if (_bookmarks == null)
            {
                _bookmarks = new Hashtable();

                try
                {
                    lock (_core.SettingsProvider)
                    {
                        if (!_core.SettingsProvider.TableExists(_core.SettingsProvider.GetFullTableName("bookmark")))
                        {
                            _core.SettingsProvider.Database.Execute(string.Format("create table '{0}' (ID integer primary key autoincrement, Name text)", _core.SettingsProvider.GetFullTableName("bookmark")));
                        }
                        if (!_core.SettingsProvider.TableExists(_core.SettingsProvider.GetFullTableName("bmcodes")))
                        {
                            _core.SettingsProvider.Database.Execute(string.Format("create table '{0}' (BookmarkID integer, Code text)", _core.SettingsProvider.GetFullTableName("bmcodes")));
                        }

                        var bms = _core.SettingsProvider.Database.Fetch<BookmarkPoco>(string.Format("select * from {0}", _core.SettingsProvider.GetFullTableName("bookmark")));
                        foreach (var bm in bms)
                        {
                            BookmarkInfo bmi = new BookmarkInfo();
                            bmi.ID = bm.ID;
                            bmi.Name = bm.Name;
                            _bookmarks.Add(bmi.Name.ToLower(), bmi);
                        }

                        var bmcs = _core.SettingsProvider.Database.Fetch<BookmarkCodePoco>(string.Format("select * from {0}", _core.SettingsProvider.GetFullTableName("bmcodes")));
                        foreach (var bmc in bmcs)
                        {
                            BookmarkInfo bmi = (from BookmarkInfo b in _bookmarks.Values where b.ID == bmc.BookmarkID select b).FirstOrDefault();
                            if (bmi != null)
                            {
                                bmi.GeocacheCodes.Add(bmc.Code, true);
                            }
                        }

                    }
                }
                catch
                {
                }
            }
        }

        public void OnDataChanged()
        {
            if (DataChanged != null)
            {
                DataChanged(this, EventArgs.Empty);
            }
        }

        public List<BookmarkInfo> AvailableBookmarks
        {
            get
            {
                return (from BookmarkInfo bm in _bookmarks.Values select bm).ToList();
            }
        }

        public List<string> AvailableCollections(List<string> lst)
        {
            lst.AddRange((from string s in _bookmarks.Keys select s).ToList());
            return lst;
        }

        public BookmarkInfo AddCollection(string name)
        {
            BookmarkInfo bmi = _bookmarks[name.ToLower()] as BookmarkInfo;
            if (bmi == null)
            {
                int id;
                lock (_core.SettingsProvider)
                {
                    _core.SettingsProvider.Database.Execute(string.Format("insert into {1} (Name) values ('{0}')", name.Replace("'", "''"), _core.SettingsProvider.GetFullTableName("bookmark")));
                    id = _core.SettingsProvider.Database.ExecuteScalar<int>(string.Format("select ID from {1} where Name = '{0}'", name.Replace("'", "''"), _core.SettingsProvider.GetFullTableName("bookmark")));
                }
                bmi = new BookmarkInfo();
                bmi.ID = id;
                bmi.Name = name;
                _bookmarks.Add(name.ToLower(), bmi);
            }
            return bmi;
        }

        public void DeleteCollection(string name)
        {
            BookmarkInfo bmi = _bookmarks[name.ToLower()] as BookmarkInfo;
            if (bmi != null)
            {
                lock (_core.SettingsProvider)
                {
                    _core.SettingsProvider.Database.Execute(string.Format("delete from {1} where BookmarkID={0}", bmi.ID, _core.SettingsProvider.GetFullTableName("bmcodes")));
                    _core.SettingsProvider.Database.Execute(string.Format("delete from {1} where ID={0}", bmi.ID, _core.SettingsProvider.GetFullTableName("bookmark")));
                }

                _bookmarks.Remove(name);
            }
        }

        public void AddToCollection(string collectionName, string geocacheCode)
        {
            BookmarkInfo bmi = AddCollection(collectionName);
            if (bmi != null)
            {
                if (bmi.GeocacheCodes[geocacheCode.ToUpper()] == null)
                {
                    lock (_core.SettingsProvider)
                    {
                        _core.SettingsProvider.Database.Execute(string.Format("insert into {2} (BookmarkID, Code) values ({0}, '{1}')", bmi.ID, geocacheCode.ToUpper(), _core.SettingsProvider.GetFullTableName("bmcodes")));
                    }
                    bmi.GeocacheCodes.Add(geocacheCode.ToUpper(), true);
                }
            }
        }

        public void RemoveFromCollection(string collectionName, string geocacheCode)
        {
            BookmarkInfo bmi = _bookmarks[collectionName.ToLower()] as BookmarkInfo;
            if (bmi != null)
            {
                if (bmi.GeocacheCodes[geocacheCode.ToUpper()] != null)
                {
                    lock (_core.SettingsProvider)
                    {
                        _core.SettingsProvider.Database.Execute(string.Format("delete from {2} where BookmarkID={0} and Code='{1}'", bmi.ID, geocacheCode.ToUpper(), _core.SettingsProvider.GetFullTableName("bmcodes")));
                    }
                    bmi.GeocacheCodes.Remove(geocacheCode.ToUpper());
                }
            }
        }

        public bool InCollection(string collectionName, string geocacheCode)
        {
            bool result = false;
            BookmarkInfo bmi = _bookmarks[collectionName.ToLower()] as BookmarkInfo;
            if (bmi != null)
            {
                result = (bmi.GeocacheCodes[geocacheCode.ToUpper()] != null);
            }
            return result;
        }

    }
}
