using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GAPPSF.Core.Data
{
    [XmlType("AccountInfoItem")] // define Type
    public class AccountInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public AccountInfo()
        {
            _geocacheCodePrefix = "";
            _accountName = "";
        }
        public AccountInfo(string geocacheCodePrefix)
        {
            _geocacheCodePrefix = geocacheCodePrefix;
            _accountName = "";
        }
        public AccountInfo(string geocacheCodePrefix, string accountName)
        {
            _geocacheCodePrefix = geocacheCodePrefix;
            _accountName = accountName;
        }

        private string _geocacheCodePrefix = null;
        [XmlElement("GeocacheCodePrefix")]
        public string GeocacheCodePrefix
        {
            get { return _geocacheCodePrefix; }
            set { SetProperty(ref _geocacheCodePrefix, value); }
        }

        private string _accountName = null;
        [XmlElement("AccountName")]
        public string AccountName
        {
            get { return _accountName; }
            set { SetProperty(ref _accountName, value); }
        }

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }

    }
}
