using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public class AccountInfoCollection : List<AccountInfo>, INotifyCollectionChanged
    {
        private Hashtable _gcPrefix;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public AccountInfoCollection()
        {
            _gcPrefix = new Hashtable();
        }

        private void rebuildHashtable()
        {
            _gcPrefix.Clear();
            foreach(AccountInfo ai in this)
            {
                _gcPrefix.Add(ai.GeocacheCodePrefix, ai);
            }
        }

        public new void Add(AccountInfo ai)
        {
            base.Add(ai);
            rebuildHashtable();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public new void Remove(AccountInfo ai)
        {
            base.Remove(ai);
            rebuildHashtable();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
            rebuildHashtable();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public new void Clear()
        {
            base.Clear();
            rebuildHashtable();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }

    }
}
