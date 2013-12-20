using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public class Logger
    {
        public enum Level : short
        {
            None = 0,
            Error = 1,
            Warning = 2,
            Info = 3,
        }

        public class LogEventArgs : EventArgs
        {
            public string ObjType { get; private set; }
            public string Message { get; private set; }
            public Level Level { get; private set; }
            public Exception Exception { get; private set; }

            public LogEventArgs(string objType, Level level, Exception e, string msg)
            {
                ObjType = objType;
                Message = msg;
                Level = level;
                Exception = e;
            }
        }

        public delegate void LogEventHandler(object sender, LogEventArgs e);
        public event LogEventHandler LogAdded;

        public void AddLog(object source, Exception e)
        {
            if (LogAdded!=null)
            {
                LogAdded(this, new LogEventArgs(source == null ? "" : source.GetType().ToString(), Level.Error, e, e.Message));
            }
        }
        public void AddLog(object source, Level level, string msg)
        {
            if (LogAdded != null)
            {
                LogAdded(this, new LogEventArgs(source == null ? "" : source.GetType().ToString(), level, null, msg));
            }
        }
    }
}
