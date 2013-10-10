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

        private Utils.DBCon _dbcon = null;
        private Hashtable _bookmarks = null;

        public event EventHandler DataChanged;

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
            if (_bookmarks == null)
            {
                _bookmarks = new Hashtable();

                try
                {
                    _dbcon = new Utils.DBConComSqlite(Path.Combine(core.PluginDataPath, "gccollections.db3"));

                    object o = _dbcon.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='bookmark'");
                    if (o == null || o.GetType() == typeof(DBNull))
                    {
                        _dbcon.ExecuteNonQuery("create table 'bookmark' (ID integer primary key autoincrement, Name text)");

                        _dbcon.ExecuteNonQuery("create table 'codes' (BookmarkID integer, Code text)");
                    }

                    DbDataReader dr = _dbcon.ExecuteReader("select ID, Name from bookmark");
                    while (dr.Read())
                    {
                        BookmarkInfo bmi = new BookmarkInfo();
                        bmi.ID = (int)dr["ID"];
                        bmi.Name = (string)dr["Name"];
                        _bookmarks.Add(bmi.Name.ToLower(), bmi);
                    }

                    dr = _dbcon.ExecuteReader("select BookmarkID, Code from codes");
                    while (dr.Read())
                    {
                        int id = (int)dr["BookmarkID"];
                        BookmarkInfo bmi = (from BookmarkInfo b in _bookmarks.Values where b.ID==id select b).FirstOrDefault();
                        if (bmi != null)
                        {
                            bmi.GeocacheCodes.Add(dr["Code"], true);
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
                _dbcon.ExecuteNonQuery(string.Format("insert into bookmark (Name) values ('{0}')", name.Replace("'", "''")));
                int id = (int)_dbcon.ExecuteScalar(string.Format("select ID from bookmark where Name = '{0}'", name.Replace("'", "''")));

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
                _dbcon.ExecuteNonQuery(string.Format("delete from codes where BookmarkID={0}", bmi.ID));
                _dbcon.ExecuteNonQuery(string.Format("delete from bookmark where ID={0}", bmi.ID));

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
                    _dbcon.ExecuteNonQuery(string.Format("insert into codes (BookmarkID, Code) values ({0}, '{1}')", bmi.ID, geocacheCode.ToUpper()));

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
                    _dbcon.ExecuteNonQuery(string.Format("delete from codes where BookmarkID={0} and Code='{1}'", bmi.ID, geocacheCode.ToUpper()));

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
