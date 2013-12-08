using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Storage
{
    public class DatabaseCollection: List<Database>, INotifyCollectionChanged, IDisposable
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private int _currentBufferLevel;

        public DatabaseCollection()
        {
            _currentBufferLevel = Core.Settings.Default.DataBufferLevel;
            Core.Settings.Default.PropertyChanged += Default_PropertyChanged;
        }

        void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DataBufferLevel")
            {
                foreach(Database db in this)
                {
                    foreach (var gc in db.GeocacheCollection)
                    {
                        gc.BufferLevelChanged(_currentBufferLevel);
                    }
                    foreach (var lg in db.LogCollection)
                    {
                        lg.BufferLevelChanged(_currentBufferLevel);
                    }
                }
                _currentBufferLevel = Core.Settings.Default.DataBufferLevel;
                GC.Collect();
            }
        }

        public void Dispose()
        {
            if (_currentBufferLevel >= 0)
            {
                GAPPSF.Core.Settings.Default.PropertyChanged -= Default_PropertyChanged;
                _currentBufferLevel = -1;
            }
        }

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
