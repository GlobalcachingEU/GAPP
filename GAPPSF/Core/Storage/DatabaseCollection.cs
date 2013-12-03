using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Storage
{
    public class DatabaseCollection: List<Database>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void listUpdated()
        {
            StringBuilder sb = new StringBuilder();
            foreach(Database db in this)
            {
                sb.AppendLine(db.FileName);
            }
            Core.Settings.Default.OpenedDatabases = sb.ToString();
        }

        public new void Add(Database db)
        {
            base.Add(db);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public new void Remove(Database db)
        {
            db.Dispose();
            base.Remove(db);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public new void RemoveAt(int index)
        {
            base.Remove(this[index]);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public new void Clear()
        {
            foreach(Database db in this)
            {
                db.Dispose();
            }
            base.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            listUpdated();
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }

    }
}
