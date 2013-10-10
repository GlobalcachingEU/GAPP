using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Data
{
    public class Waypoint : DataObject, IComparable
    {
        private string _id;
        private string _name;
        private string _comment;
        private string _description;
        private string _url;
        private string _urlName;
        private WaypointType _wpType;
        private string _code;
        private string _geocacheCode;
        private DateTime _dataFromDate = DateTime.MinValue;
        private double? _lat = null;
        private double? _lon = null;
        private DateTime _time = DateTime.MinValue;

        private bool _dataChanged = false;
        public event EventArguments.WaypointEventHandler DataChanged;
        private bool _updating = false;
        private bool _saved = false;

        public Waypoint()
        {
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public void UpdateFrom(Waypoint wp)
        {
            BeginUpdate();
            this.GeocacheCode = wp.GeocacheCode;
            this.Code = wp.Code;
            this.ID = wp.ID;
            this.DataFromDate = wp.DataFromDate;
            this.Name = wp.Name;
            this.Comment = wp.Comment;
            this.Description = wp.Description;
            this.Url = wp.Url;
            this.UrlName = wp.UrlName;
            this.WPType = wp.WPType;
            this.Lat = wp.Lat;
            this.Lon = wp.Lon;
            this.Time = wp.Time;

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

        public string UrlName
        {
            get { return _urlName; }
            set
            {
                if (_urlName != value)
                {
                    _urlName = value;
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

        public string Comment
        {
            get { return _comment; }
            set
            {
                if (_comment != value)
                {
                    _comment = value;
                    OnDataChanged(this);
                }
            }
        }

        public double? Lat
        {
            get { return _lat; }
            set
            {
                if (_lat != value)
                {
                    _lat = value;
                    OnDataChanged(this);
                }
            }
        }

        public double? Lon
        {
            get { return _lon; }
            set
            {
                if (_lon != value)
                {
                    _lon = value;
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

        public DateTime Time
        {
            get { return _time; }
            set
            {
                if (_time != value)
                {
                    _time = value;
                    OnDataChanged(this);
                }
            }
        }

        public WaypointType WPType 
        {
            get { return _wpType; }
            set
            {
                if (_wpType != value)
                {
                    _wpType = value;
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

        public string Code
        {
            get { return _code; }
            set
            {
                if (_code != value)
                {
                    _code = value;
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
                DataChanged(sender, new EventArguments.WaypointEventArgs(this));
            }
        }


        public int CompareTo(object obj)
        {
            return string.Compare(this.Code, ((Waypoint)obj).Code);
        }
    }
}
