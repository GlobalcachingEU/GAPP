using Community.CsharpSqlite.SQLiteClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Utils
{
    public class DBConComSqlite : DBCon
    {
        public DBConComSqlite(string filename)
        {
            Connection = new SqliteConnection(string.Format("data source=file:{0}", filename));
            Connection.Open();
        }

        public override bool ColumnExists(string tableName, string columnName)
        {
            bool result = false;
            try
            {
                DbDataReader dr = ExecuteReader(string.Format("pragma table_info({0})", tableName));
                while (dr.Read())
                {
                    if (dr[1] is string)
                    {
                        if ((string)dr[1] == columnName)
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            catch
            {
            }
            return result;
        }

        public override bool TableExists(string tableName)
        {
            bool result = false;
            try
            {
                object o = ExecuteScalar(string.Format("SELECT name FROM sqlite_master WHERE type='table' AND name='{0}'", tableName));
                if (o == null || o.GetType() == typeof(DBNull))
                {
                    result = false;
                }
                else
                {
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
