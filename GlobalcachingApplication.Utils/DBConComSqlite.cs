using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using Community.CsharpSqlite.SQLiteClient;

namespace GlobalcachingApplication.Utils
{
    public class DBConComSqlite: DBCon
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

    }
}
