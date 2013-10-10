using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Data
{
    public class LogImage: DataObject, IComparable
    {
        private string _id;
        private DateTime _dataFromDate = DateTime.MinValue;
        private string _logId;
        private string _url;
        private string _name;

        private bool _dataChanged = false;
        public event EventArguments.LogImageEventHandler DataChanged;
        private bool _updating = false;
        private bool _saved = false;

        public LogImage()
        {
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public void UpdateFrom(LogImage l)
        {
            BeginUpdate();
            this.ID = l.ID;
            this.DataFromDate = l.DataFromDate;
            this.LogID = l.LogID;
            this.Url = l.Url;
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

        public string LogID
        {
            get { return _logId; }
            set
            {
                if (_logId != value)
                {
                    _logId = value;
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
                DataChanged(sender, new EventArguments.LogImageEventArgs(this));
            }
        }


        public int CompareTo(object obj)
        {
            return string.Compare(this.ID, ((LogImage)obj).ID);
        }
    }
}
