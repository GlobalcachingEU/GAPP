using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GAPPSF.Core.Data
{
    [XmlRoot("AccountInfoCollection")]
    public class AccountInfoCollection : List<AccountInfo>, INotifyCollectionChanged
    {
        private Hashtable _gcPrefix;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public AccountInfoCollection()
        {
            _gcPrefix = new Hashtable();
        }

        public AccountInfo GetAccountInfo(string gcPrefix)
        {
            return _gcPrefix[gcPrefix] as AccountInfo;
        }

        private void rebuildHashtable()
        {
            _gcPrefix.Clear();
            foreach(AccountInfo ai in this)
            {
                if (_gcPrefix[ai.GeocacheCodePrefix] == null)
                {
                    _gcPrefix.Add(ai.GeocacheCodePrefix, ai);
                }
            }
        }

        public new void Add(AccountInfo ai)
        {
            if (GetAccountInfo(ai.GeocacheCodePrefix) == null)
            {
                base.Add(ai);
                ai.PropertyChanged += ai_PropertyChanged;
                rebuildHashtable();
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            else
            {
                GetAccountInfo(ai.GeocacheCodePrefix).AccountName = ai.AccountName;
            }
        }

        void ai_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            rebuildHashtable();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));            
        }
        public new void Remove(AccountInfo ai)
        {
            base.RemoveAt(IndexOf(ai));
        }
        public new void RemoveAt(int index)
        {
            (this[index] as AccountInfo).PropertyChanged -= ai_PropertyChanged;
            base.RemoveAt(index);
            rebuildHashtable();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public new void Clear()
        {
            while (this.Count > 0)
            {
                RemoveAt(0);
            }
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
