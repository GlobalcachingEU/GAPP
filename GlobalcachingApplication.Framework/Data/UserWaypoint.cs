using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Data
{
    public class UserWaypoint
    {
        private int _id;
        private string _geocacheCode;
        private string _description;
        private double _lat;
        private double _lon;
        private DateTime _date;

        private bool _dataChanged = false;
        public event EventArguments.UserWaypointEventHandler DataChanged;
        private bool _updating = false;
        private bool _saved = false;

        public UserWaypoint()
        {
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public void UpdateFrom(UserWaypoint wp)
        {
            BeginUpdate();
            this.GeocacheCode = wp.GeocacheCode;
            this.ID = wp.ID;
            this.Description = wp.Description;
            this.Lat = wp.Lat;
            this.Lon = wp.Lon;
            this.Date = wp.Date;
            EndUpdate();
        }

        public int ID
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

        public double Lat
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

        public double Lon
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

        public DateTime Date
        {
            get { return _date; }
            set
            {
                if (_date != value)
                {
                    _date = value;
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
                DataChanged(sender, new EventArguments.UserWaypointEventArgs(this));
            }
        }

    }
}
