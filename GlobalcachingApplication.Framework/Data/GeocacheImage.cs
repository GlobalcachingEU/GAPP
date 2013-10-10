using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Data
{
    public class GeocacheImage: DataObject, IComparable
    {
        private string _id; //guid
        private DateTime _dataFromDate = DateTime.MinValue;
        private string _geocacheCode;
        private string _url;
        private string _name;
        private string _mobileUrl;
        private string _thumbUrl;
        private string _description;

        private bool _dataChanged = false;
        public event EventArguments.GeocacheImageEventHandler DataChanged;
        private bool _updating = false;
        private bool _saved = false;

        public GeocacheImage()
        {
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public void UpdateFrom(GeocacheImage l)
        {
            BeginUpdate();
            this.ID = l.ID;
            this.DataFromDate = l.DataFromDate;
            this.GeocacheCode = l.GeocacheCode;
            this.Url = l.Url;
            this.MobileUrl = l.MobileUrl;
            this.ThumbUrl = l.ThumbUrl;
            this.Description = l.Description;
            this.Name = l.Name;
            EndUpdate();
        }

        public string ID
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnDataChanged(this);
                }
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnDataChanged(this);
                }
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnDataChanged(this);
                }
            }
        }

        public string Url
        {
            get { return _url; }
            set
            {
                if (_url != value)
                {
                    _url = value;
                    OnDataChanged(this);
                }
            }
        }

        public string MobileUrl
        {
            get { return _mobileUrl; }
            set
            {
                if (_mobileUrl != value)
                {
                    _mobileUrl = value;
                    OnDataChanged(this);
                }
            }
        }

        public string ThumbUrl
        {
            get { return _thumbUrl; }
            set
            {
                if (_thumbUrl != value)
                {
                    _thumbUrl = value;
                    OnDataChanged(this);
                }
            }
        }

        public string GeocacheCode
        {
            get { return _geocacheCode; }
            set
            {
                if (_geocacheCode != value)
                {
                    _geocacheCode = value;
                    OnDataChanged(this);
                }
            }
        }

        public DateTime DataFromDate
        {
            get { return _dataFromDate; }
            set
            {
                if (_dataFromDate != value)
                {
                    _dataFromDate = value;
                    OnDataChanged(this);
                }
            }
        }


        public bool Saved
        {
            get { return _saved; }
            set { _saved = value; }
        }
        public bool IsDataChanged
        {
            get { return _dataChanged; }
            set { _dataChanged = value; }
        }
        public void BeginUpdate()
        {
            _updating = true;
        }
        public void EndUpdate()
        {
            _updating = false;
            if (_dataChanged)
            {
                OnDataChanged(this);
            }
        }

        public void OnDataChanged(object sender)
        {
            _saved = false;
            _dataChanged = true;
            if (!_updating && DataChanged != null)
            {
                DataChanged(sender, new EventArguments.GeocacheImageEventArgs(this));
            }
        }


        public int CompareTo(object obj)
        {
            return string.Compare(this.ID, ((GeocacheImage)obj).ID);
        }
    }
}
