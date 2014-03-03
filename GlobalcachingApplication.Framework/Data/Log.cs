using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Data
{
    public class Log : DataObject, IComparable
    {
        private string _id;
        private LogType _logType;
        private string _geocacheCode; //either gc code or tb code, but can be both like dropping a trackable in a cache
        private string _tbCode;
        private DateTime _date = DateTime.MinValue;
        private DateTime _dataFromDate = DateTime.MinValue;
        private string _finderId = "";
        private string _finder = "";
        private string _text = "";
        private bool _encoded = false;

        private bool _dataChanged = false;
        public event EventArguments.LogEventHandler DataChanged;
        private bool _updating = false;
        private bool _saved = false;

        //if text is null, then log is parially loaded
        public event EventArguments.LoadFullLogEventHandler LoadFullData;
        private bool _loadingFullData = false;
        private bool _fullDataLoaded = true;
        private static Log _fullLoadLog = new Log();

        public Log()
        {
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        private void fullDataRequest(bool persist)
        {
            if (!FullDataLoaded)
            {
                if (!_loadingFullData && LoadFullData != null)
                {
                    _loadingFullData = true;
                    _fullLoadLog.ID = this.ID;
                    if (!persist)
                    {
                        _fullLoadLog._tbCode = this._tbCode;
                        _fullLoadLog._finderId = this._finderId;
                        _fullLoadLog._text = this._text;
                        _fullLoadLog._encoded = this._encoded;
                    }
                    var e = new EventArguments.LoadFullLogEventArgs(this);
                    LoadFullData(this, e);
                    _fullLoadLog._tbCode = e.TBCode;
                    _fullLoadLog._finderId = e.FinderId;
                    _fullLoadLog._text = e.Text;
                    _fullLoadLog._encoded = e.Encoded;
                    _loadingFullData = false;
                    if (persist)
                    {
                        this._tbCode = e.TBCode;
                        this._finderId = e.FinderId;
                        this._text = e.Text;
                        this._encoded = e.Encoded;
                        FullDataLoaded = true;
                    }
                }
            }
            else
            {
                _fullLoadLog._tbCode = this._tbCode;
                _fullLoadLog._finderId = this._finderId;
                _fullLoadLog._text = this._text;
                _fullLoadLog._encoded = this._encoded;
            }
        }

        public bool FullDataLoaded
        {
            get { return (_fullDataLoaded); }
            set { _fullDataLoaded = value; }
        }

        public void ClearFullData()
        {
            this._tbCode = "";
            this._finderId = "";
            this._text = "";
            this._encoded = false;
            FullDataLoaded = false;
        }

        public void UpdateFrom(Log l)
        {
            BeginUpdate();
            this.GeocacheCode = l.GeocacheCode;
            this.TBCode = l.TBCode;
            this.ID = l.ID;
            this.Date = l.Date;
            this.Finder = l.Finder;
            this.FinderId = l.FinderId;
            this.Text = l.Text;
            this.Encoded = l.Encoded;
            this.DataFromDate = l.DataFromDate;
            this.LogType = l.LogType;

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

        public string Text
        {
            get 
            {
                fullDataRequest(false);
                return _fullLoadLog._text; 
            }
            set
            {
                if (_fullLoadLog._text != value)
                {
                    fullDataRequest(true);
                    _text = value;
                    OnDataChanged(this);
                }
            }

        }

        public bool Encoded
        {
            get 
            {
                fullDataRequest(false);
                return _fullLoadLog._encoded; 
            }
            set
            {
                if (_fullLoadLog._encoded != value)
                {
                    fullDataRequest(true);
                    _encoded = value;
                    OnDataChanged(this);
                }
            }
        }

        public string Finder
        {
            get { return _finder; }
            set
            {
                if (_finder != value)
                {
                    _finder = value;
                    OnDataChanged(this);
                }
            }
        }

        public string FinderId
        {
            get 
            {
                fullDataRequest(false);
                return _fullLoadLog._finderId; 
            }
            set
            {
                if (_fullLoadLog._finderId != value)
                {
                    fullDataRequest(true);
                    _finderId = value;
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
        

        public LogType LogType 
        {
            get { return _logType; }
            set
            {
                if (_logType != value)
                {
                    _logType = value;
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

        public string TBCode
        {
            get 
            {
                fullDataRequest(false);
                return _fullLoadLog._tbCode; 
            }
            set
            {
                if (_fullLoadLog._tbCode != value)
                {
                    fullDataRequest(true);
                    _tbCode = value;
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
            if (!_loadingFullData)
            {
                _saved = false;
                _dataChanged = true;
                if (!_updating && DataChanged != null)
                {
                    DataChanged(sender, new EventArguments.LogEventArgs(this));
                }
            }
        }

        public int CompareTo(object obj)
        {
            return string.Compare(this.ID, ((Log)obj).ID);
        }
    }
}
