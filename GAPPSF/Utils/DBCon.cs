using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Utils
{
    public class DBCon : IDisposable
    {
        private DbConnection _dbcon = null;
        private DbCommand _cmd = null;
        private DbDataReader _rdr = null;
        private DbTransaction _tran = null;

        public DBCon()
        {
        }


        public DbConnection Connection
        {
            get
            {
                return _dbcon;
            }
            protected set
            {
                _dbcon = value;
            }
        }

        public virtual bool ColumnExists(string tableName, string columnName)
        {
            return false;
        }

        public virtual bool TableExists(string tableName)
        {
            return false;
        }

        public DbCommand Command
        {
            get
            {
                if (_cmd == null)
                {
                    _cmd = _dbcon.CreateCommand();
                }
                return _cmd;
            }
        }

        public DbDataReader ExecuteReader(string command)
        {
            if (_rdr != null && !_rdr.IsClosed)
            {
                _rdr.Close();
            }
            Command.CommandText = command;
            _rdr = _cmd.ExecuteReader();
            return _rdr;
        }

        public object ExecuteScalar(string command)
        {
            if (_rdr != null && !_rdr.IsClosed)
            {
                _rdr.Close();
            }
            Command.CommandText = command;
            return _cmd.ExecuteScalar();
        }

        public int ExecuteNonQuery(string command)
        {
            if (_rdr != null && !_rdr.IsClosed)
            {
                _rdr.Close();
            }
            Command.CommandText = command;
            return _cmd.ExecuteNonQuery();
        }

        public void ExecuteBatchNonQuery(string sql)
        {
            StringBuilder sqlBatch = new StringBuilder();
            sql += "\nGO";   // make sure last batch is executed.
            foreach (string line in sql.Split(new string[2] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.ToUpperInvariant().Trim() == "GO")
                {
                    string cmd = sqlBatch.ToString().Trim();
                    if (cmd.Length > 0)
                    {
                        ExecuteNonQuery(cmd);
                    }
                    sqlBatch.Length = 0;
                }
                else
                {
                    string s = line.Trim();
                    if (!s.StartsWith("--") && !s.StartsWith("/*") && !s.StartsWith("print "))
                    {
                        sqlBatch.AppendLine(line);
                    }
                }
            }
        }
        #region IDisposable Members

        public void Dispose()
        {
            if (_tran != null)
            {
                _tran.Dispose();
                _tran = null;
            }
            if (_rdr != null)
            {
                if (!_rdr.IsClosed)
                {
                    _rdr.Close();
                }
                _rdr.Dispose();
                _rdr = null;
            }
            if (_cmd != null)
            {
                _cmd.Dispose();
                _cmd = null;
            }
            if (_dbcon != null)
            {
                _dbcon.Close();
                _dbcon = null;
            }
        }

        #endregion

        public void BeginTran()
        {
            if (_tran != null)
            {
                _tran.Dispose();
                _tran = null;
            }
            _tran = _dbcon.BeginTransaction();
        }

        public void RollbackTran()
        {
            if (_tran != null)
            {
                _tran.Rollback();
                _tran.Dispose();
                _tran = null;
            }
        }

        public void CommitTran()
        {
            if (_tran != null)
            {
                _tran.Commit();
                _tran.Dispose();
                _tran = null;
            }
        }
    }
}
