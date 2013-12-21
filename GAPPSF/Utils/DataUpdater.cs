using GAPPSF.Core;
using GAPPSF.Core.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Utils
{
    public class DataUpdater: IDisposable
    {
        private List<Database> _dbList = null;
        private bool _readOnly = false;

        public DataUpdater(Database db)
            : this(db, false)
        {
        }
        public DataUpdater(Database db, bool readOnly)
        {
            _readOnly = readOnly;
            _dbList = new List<Database>();
            if (db != null)
            {
                _dbList.Add(db);
            }
            else
            {
                foreach (Database d in ApplicationData.Instance.Databases)
                {
                    _dbList.Add(d);
                }
            }
            foreach (Database d in _dbList)
            {
                blockUpdates(d);
            }
        }

        private void blockUpdates(Database db)
        {
            db.GeocacheCollection.BeginUpdate();
            db.LogCollection.BeginUpdate();
            db.WaypointCollection.BeginUpdate();
            db.UserWaypointCollection.BeginUpdate();
            db.LogImageCollection.BeginUpdate();
            db.GeocacheImageCollection.BeginUpdate();
        }

        private void unblockUpdates(Database db)
        {
            db.GeocacheImageCollection.EndUpdate();
            db.LogImageCollection.EndUpdate();
            db.UserWaypointCollection.EndUpdate();
            db.WaypointCollection.EndUpdate();
            db.LogCollection.EndUpdate();
            db.GeocacheCollection.EndUpdate();

            if (!_readOnly)
            {
                db.Flush();
            }
        }

        public void Dispose()
        {
            if (_dbList != null)
           {
               foreach (Database d in _dbList)
               {
                   unblockUpdates(d);
               }
           }
        }
    }
}
