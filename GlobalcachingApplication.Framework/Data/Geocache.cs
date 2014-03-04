using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Data
{
    public class Geocache : DataObject, IComparable
    {
        public static Version V1 = new Version(1, 0, 0);
        public static Version V2 = new Version(1, 0, 1);
        public static Version V3 = new Version(1, 0, 2);
        private static Geocache _fullLoadGeocache = new Geocache();

        //data properties
        private string _id;
        private string _code;
        private string _title;
        private DateTime _dataFromDate = DateTime.MinValue;
        private DateTime _publishedTime = DateTime.MinValue;
        private double _lat;
        private double _lon;
        private long _distanceToCenter = 0;
        private int _angleToCenter = 0;
        private bool _available = true;
        private bool _archived = true;
        private string _country;
        private string _state;
        private string _municipality;
        private string _city;
        private GeocacheType _type;
        private string _placedBy;
        private string _owner;
        private string _ownerId;
        private GeocacheContainer _container;
        private double _terrain;
        private double _difficulty;
        private string _shortDescription = "";
        private bool _shortDescriptionInHtml = false;
        private string _longDescription = "";
        private bool _longDescriptionInHtml = false;
        private string _encodedHints;
        private string _url;
        private bool _memberOnly = false;
        private bool _customCoords = false; //means the lat and lon are custom, not original (by GS)
        private List<int> _attributeIds = new List<int>(); //negative value indicated NOT (like -1 means no dogs allowed)
        private int _favorites;
        private string _personalNote = ""; //from geocaching.com
        private bool _flagged = false;
        private bool _found = false; //found log was available or LiveAPI update says so
        private bool _locked = false;

        //additional props
        private double? _customLat = null;
        private double? _customLon = null;
        private string _notes = "";

        //operational properties
        private bool _selected = false;
        public event EventArguments.GeocacheEventHandler SelectedChanged;
        private bool _dataChanged = false;
        public event EventArguments.GeocacheEventHandler DataChanged;
        private bool _updating = false;
        private bool _saved = false;
        private bool _fullDataLoaded = true;
        private Hashtable _customAttributes = new Hashtable();
        public static List<string> CustomAttributesKeys = new List<string>();
        private Interfaces.ICore _core = null;

        //if _shortDescription is null, then cache is parially loaded
        public event EventArguments.LoadFullGeocacheEventHandler LoadFullData;
        private bool _loadingFullData = false;

        public Geocache()
        {
        }

        public override int GetHashCode()
        {
            return _code.GetHashCode();
        }

        public Interfaces.ICore Core
        {
            get { return _core; }
            set { _core = value; }
        }

        private void fullDataRequest(bool persist)
        {
            if (!FullDataLoaded)
            {
                if (!_loadingFullData && LoadFullData != null)
                {
                    _loadingFullData = true;
                    _fullLoadGeocache.Code = this.Code;
                    if (!persist)
                    {
                        _fullLoadGeocache._shortDescription = this._shortDescription;
                        _fullLoadGeocache._shortDescriptionInHtml = this._shortDescriptionInHtml;
                        _fullLoadGeocache._longDescription = this._longDescription;
                        _fullLoadGeocache._longDescriptionInHtml = this._longDescriptionInHtml;
                    }
                    var e = new EventArguments.LoadFullGeocacheEventArgs(_fullLoadGeocache);
                    LoadFullData(this, e);
                    _fullLoadGeocache._shortDescription = e.ShortDescription;
                    _fullLoadGeocache._shortDescriptionInHtml = e.ShortDescriptionInHtml;
                    _fullLoadGeocache._longDescription = e.LongDescription;
                    _fullLoadGeocache._longDescriptionInHtml = e.LongDescriptionInHtml;
                    _loadingFullData = false;
                    if (persist)
                    {
                        this._shortDescription = _fullLoadGeocache._shortDescription;
                        this._shortDescriptionInHtml = _fullLoadGeocache._shortDescriptionInHtml;
                        this._longDescription = _fullLoadGeocache._longDescription;
                        this._longDescriptionInHtml = _fullLoadGeocache._longDescriptionInHtml;
                        FullDataLoaded = true;
                    }
                }
            }
            else
            {
                _fullLoadGeocache._shortDescription = this._shortDescription;
                _fullLoadGeocache._shortDescriptionInHtml = this._shortDescriptionInHtml;
                _fullLoadGeocache._longDescription = this._longDescription;
                _fullLoadGeocache._longDescriptionInHtml = this._longDescriptionInHtml;
            }
        }

        public bool FullDataLoaded
        {
            get { return (_fullDataLoaded); }
            set { _fullDataLoaded = value; }
        }

        public void ClearFullData()
        {
            this._shortDescription = "";
            this._shortDescriptionInHtml = false;
            this._longDescription = "";
            this._longDescriptionInHtml = false;
            FullDataLoaded = false;
        }

        public void UpdateFrom(Geocache gc, Version gpxDataVersion)
        {
            BeginUpdate();
            if (this.Locked)
            {
                this.Available = gc.Available;
                this.Archived = gc.Archived;
            }
            else
            {
                this.Code = gc.Code;
                this.ID = gc.ID;
                this.DataFromDate = gc.DataFromDate;
                this.Lat = gc.Lat;
                this.Lon = gc.Lon;
                this.DistanceToCenter = gc.DistanceToCenter;
                this.AngleToCenter = gc.AngleToCenter;
                this.Available = gc.Available;
                this.Archived = gc.Archived;
                this.Country = gc.Country;
                this.State = gc.State;
                this.GeocacheType = gc.GeocacheType;
                this.Container = gc.Container;
                this.PublishedTime = gc.PublishedTime;
                this.Owner = gc.Owner;
                this.OwnerId = gc.OwnerId;
                this.Terrain = gc.Terrain;
                this.Difficulty = gc.Difficulty;
                this.ShortDescription = gc._shortDescription;
                this.ShortDescriptionInHtml = gc._shortDescriptionInHtml;
                this.LongDescription = gc._longDescription;
                this.LongDescriptionInHtml = gc._longDescriptionInHtml;
                this.EncodedHints = gc.EncodedHints;
                this.Title = gc.Title;
                this.Url = gc.Url;
                if (gpxDataVersion == null || gpxDataVersion >= V2)
                {
                    this.AttributeIds = gc.AttributeIds;
                    if (gpxDataVersion == null || gpxDataVersion >= V3)
                    {
                        this.MemberOnly = gc.MemberOnly;
                        this.CustomCoords = gc.CustomCoords;
                        this.Favorites = gc.Favorites;
                        this.PersonaleNote = gc.PersonaleNote;

                        if (gpxDataVersion == null)
                        {
                            this.Municipality = gc.Municipality;
                            this.City = gc.City;

                            this.CustomLat = gc.CustomLat;
                            this.CustomLon = gc.CustomLon;
                            this.Notes = gc.Notes;
                            this.Flagged = gc.Flagged;
                            this.Locked = gc.Locked;
                            this.Found = gc.Found;

                            this.Selected = gc.Selected;
                            this.Saved = gc.Saved;
                        }
                    }
                }
            }
            EndUpdate();
        }

        public void ResetWaypointsData()
        {
            _waypointsInfo = null;
        }

        private string _waypointsInfo = null;
        public string WaypointInfoString
        {
            get
            {
                if (_waypointsInfo == null)
                {
                    List<Framework.Data.Waypoint> wpl = _core.Waypoints.GetWaypoints(Code);
                    _waypointsInfo = string.Format("{0:00}/{1:00}", wpl.Count, (from w in wpl where w.Lat!=null && w.Lon!=null select w).Count());
                }
                return _waypointsInfo;
            }
        }

        public void ResetCachedUserWaypointsData()
        {
            _hasUserWaypoints = null;
        }

        private bool? _hasUserWaypoints = null;
        public bool HasUserWaypoints
        {
            get
            {
                if (_hasUserWaypoints == null)
                {
                    _hasUserWaypoints = (from Framework.Data.UserWaypoint w in _core.UserWaypoints where w.GeocacheCode == Code select w).FirstOrDefault() != null;
                }
                return (bool)_hasUserWaypoints;
            }
        }

        public void ResetCachedLogData()
        {
            _cachedFoundDateValid = false;
        }

        private bool _cachedFoundDateValid = false;
        private object _cachedFoundDate = null;
        public object FoundDate
        {
            get
            {
                if (!_cachedFoundDateValid)
                {
                    if (_core != null)
                    {
                        if (Found)
                        {
                            string usrName = _core.GeocachingAccountNames.GetAccountName(this.Code);
                            List<Framework.Data.Log> lgs = _core.Logs.GetLogs(this.Code);
                            if (lgs != null)
                            {
                                Framework.Data.Log l = (from Framework.Data.Log lg in lgs where lg.GeocacheCode == Code && lg.LogType.AsFound && lg.Finder == usrName select lg).FirstOrDefault();
                                if (l != null)
                                {
                                    //result = l.Date.ToString("yyyy-MM-dd");
                                    _cachedFoundDate = l.Date;
                                }
                            }
                        }
                    }
                    _cachedFoundDateValid = true;
                }
                return _cachedFoundDate;
            }
        }

        public void SetCustomAttribute(string name, object val)
        {
            if (_customAttributes.ContainsKey(name))
            {
                if (_customAttributes[name] != val)
                {
                    _customAttributes[name] = val;
                    OnDataChanged(this);
                }
            }
            else
            {
                _customAttributes.Add(name, val);
                OnDataChanged(this);
            }
        }

        public void DeleteCustomAttribute(string name)
        {
            if (_customAttributes.ContainsKey(name))
            {
                _customAttributes.Remove(name);
                OnDataChanged(this);
            }
        }

        public object GetCustomAttribute(string name)
        {
            object result = null;
            result = _customAttributes[name];
            return result;
        }

        private object GetCustomAttributeByIndex(int index)
        {
            if (CustomAttributesKeys.Count > index)
            {
                return GetCustomAttribute(CustomAttributesKeys[index]);
            }
            else
            {
                return null;
            }
        }

        public object CustomAttributeIndex0
        {
            get { return GetCustomAttributeByIndex(0); }
        }
        public object CustomAttributeIndex1
        {
            get { return GetCustomAttributeByIndex(1); }
        }
        public object CustomAttributeIndex2
        {
            get { return GetCustomAttributeByIndex(2); }
        }
        public object CustomAttributeIndex3
        {
            get { return GetCustomAttributeByIndex(3); }
        }
        public object CustomAttributeIndex4
        {
            get { return GetCustomAttributeByIndex(4); }
        }
        public object CustomAttributeIndex5
        {
            get { return GetCustomAttributeByIndex(5); }
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

        public string PersonaleNote
        {
            get { return _personalNote; }
            set
            {
                if (_personalNote != value)
                {
                    _personalNote = value;
                    OnDataChanged(this);
                }
            }
        }

        public bool Flagged
        {
            get { return _flagged; }
            set
            {
                if (_flagged != value)
                {
                    _flagged = value;
                    OnDataChanged(this);
                }
            }
        }

        public bool Locked
        {
            get { return _locked; }
            set
            {
                if (_locked != value)
                {
                    _locked = value;
                    OnDataChanged(this);
                }
            }
        }

        public bool Found
        {
            get { return _found; }
            set
            {
                if (_found != value)
                {
                    _found = value;
                    OnDataChanged(this);
                }
            }
        }

        public double? CustomLat
        {
            get { return _customLat; }
            set
            {
                if (_customLat != value)
                {
                    _customLat = value;
                    OnDataChanged(this);
                }
            }
        }
        public double? CustomLon
        {
            get { return _customLon; }
            set
            {
                if (_customLon != value)
                {
                    _customLon = value;
                    OnDataChanged(this);
                }
            }
        }

        public string Notes
        {
            get { return _notes; }
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    OnDataChanged(this);
                }
            }
        }

        public int Favorites
        {
            get { return _favorites; }
            set
            {
                if (_favorites != value)
                {
                    _favorites = value;
                    OnDataChanged(this);
                }
            }
        }

        public List<int> AttributeIds
        {
            get { return _attributeIds; }
            set
            {
                if (_attributeIds.Count == 0 && (value == null || value.Count == 0))
                {
                }
                else
                {
                    List<int> newAttr = value;
                    if (value == null)
                    {
                        newAttr = new List<int>();
                    }
                    if (_attributeIds.Count != newAttr.Count)
                    {
                        _attributeIds.Clear();
                        if (newAttr.Count > 0)
                        {
                            _attributeIds.AddRange(newAttr);
                        }
                        OnDataChanged(this);
                    }
                    else
                    {
                        int cnt =
                            (from c in _attributeIds
                            join p in newAttr on c equals p
                            select new { id = c }).Count();

                        if (_attributeIds.Count != cnt)
                        {
                            _attributeIds.Clear();
                            if (newAttr.Count > 0)
                            {
                                _attributeIds.AddRange(newAttr);
                            }
                            OnDataChanged(this);
                        }
                    }
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

        public string EncodedHints
        {
            get { return _encodedHints; }
            set
            {
                if (_encodedHints != value)
                {
                    _encodedHints = value;
                    OnDataChanged(this);
                }
            }
        }

        public string ShortDescription
        {
            get 
            {
                fullDataRequest(false);
                return _fullLoadGeocache._shortDescription; 
            }
            set
            {
                if (_fullLoadGeocache._shortDescription != value)
                {
                    fullDataRequest(true);
                    _shortDescription = value;
                    OnDataChanged(this);
                }
            }
        }
        public bool ShortDescriptionInHtml
        {
            get 
            {
                fullDataRequest(false);
                return _fullLoadGeocache._shortDescriptionInHtml; 
            }
            set
            {
                if (_fullLoadGeocache._shortDescriptionInHtml != value)
                {
                    fullDataRequest(true);
                    _shortDescriptionInHtml = value;
                    OnDataChanged(this);
                }
            }
        }
        public string LongDescription
        {
            get 
            {
                fullDataRequest(false);
                return _fullLoadGeocache._longDescription;
            }
            set
            {
                if (_fullLoadGeocache._longDescription != value)
                {
                    fullDataRequest(true);
                    _longDescription = value;
                    OnDataChanged(this);
                }
            }
        }
        public bool LongDescriptionInHtml
        {
            get 
            {
                fullDataRequest(false);
                return _fullLoadGeocache._longDescriptionInHtml; 
            }
            set
            {
                if (_fullLoadGeocache._longDescriptionInHtml != value)
                {
                    fullDataRequest(true);
                    _longDescriptionInHtml = value;
                    OnDataChanged(this);
                }
            }
        }

        public string PlacedBy
        {
            get { return _placedBy; }
            set
            {
                if (_placedBy != value)
                {
                    _placedBy = value;
                    OnDataChanged(this);
                }
            }
        }

        public bool IsOwn
        {
            get
            {
                bool result = false;
                if (_core != null)
                {
                    result = (_owner ?? "").Equals(_core.GeocachingAccountNames.GetAccountName(this.Code), StringComparison.CurrentCultureIgnoreCase);
                }
                return result;
            }
        }

        public string Owner
        {
            get { return _owner; }
            set
            {
                if (_owner != value)
                {
                    _owner = value;
                    OnDataChanged(this);
                }
            }
        }

        public string OwnerId
        {
            get { return _ownerId; }
            set
            {
                if (_ownerId != value)
                {
                    _ownerId = value;
                    OnDataChanged(this);
                }
            }
        }

        public GeocacheType GeocacheType
        {
            get { return _type; }
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnDataChanged(this);
                }
            }
        }

        public GeocacheContainer Container
        {
            get { return _container; }
            set
            {
                if (_container != value)
                {
                    _container = value;
                    OnDataChanged(this);
                }
            }
        }

        public string Municipality
        {
            get { return _municipality; }
            set
            {
                if (_municipality != value)
                {
                    _municipality = value;
                    OnDataChanged(this);
                }
            }
        }

        public string City
        {
            get { return _city; }
            set
            {
                if (_city != value)
                {
                    _city = value;
                    OnDataChanged(this);
                }
            }
        }

        public string Country
        {
            get { return _country; }
            set
            {
                if (_country != value)
                {
                    _country = value;
                    OnDataChanged(this);
                }
            }
        }

        public string State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnDataChanged(this);
                }
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnDataChanged(this);
                }
            }
        }

        public string Name
        {
            get { return Title; }
            set { Title = value; }
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

        public DateTime PublishedTime
        {
            get { return _publishedTime; }
            set
            {
                if (_publishedTime != value)
                {
                    _publishedTime = value;
                    OnDataChanged(this);
                }
            }
        }

        public double Terrain
        {
            get { return _terrain; }
            set
            {
                if (_terrain != value)
                {
                    _terrain = value;
                    OnDataChanged(this);
                }
            }
        }


        public double Difficulty
        {
            get { return _difficulty; }
            set
            {
                if (_difficulty != value)
                {
                    _difficulty = value;
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

        public bool Available
        {
            get { return _available; }
            set
            {
                if (_available != value)
                {
                    _available = value;
                    OnDataChanged(this);
                }
            }
        }

        public bool MemberOnly
        {
            get { return _memberOnly; }
            set
            {
                if (_memberOnly != value)
                {
                    _memberOnly = value;
                    OnDataChanged(this);
                }
            }
        }

        public bool CustomCoords
        {
            get { return _customCoords; }
            set
            {
                if (_customCoords != value)
                {
                    _customCoords = value;
                    OnDataChanged(this);
                }
            }
        }

        public bool Archived
        {
            get { return _archived; }
            set
            {
                if (_archived != value)
                {
                    _archived = value;
                    OnDataChanged(this);
                }
            }
        }

        public long DistanceToCenter
        {
            get { return _distanceToCenter; }
            set
            {
                if (_distanceToCenter != value)
                {
                    _distanceToCenter = value;
                    OnDataChanged(this, false);
                }
            }
        }

        public double DistanceToCenterMiles
        {
            get { return 0.0006214 * (double)_distanceToCenter; }
        }
        public double DistanceToCenterKilometers
        {
            get { return 0.001 * (double)_distanceToCenter; }
        }

        public bool ContainsNote
        {
            get { return (!string.IsNullOrEmpty(Notes) || !string.IsNullOrEmpty(PersonaleNote)); }
        }
        public bool ContainsCustomLatLon
        {
            get { return (_customLat!=null && _customLon!=null); }
        }

        public int AngleToCenter
        {
            get { return _angleToCenter; }
            set
            {
                if (_angleToCenter != value)
                {
                    _angleToCenter = value;
                    OnDataChanged(this, false);
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

        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (value != _selected)
                {
                    _selected = value;
                    OnSelectedChanged(this);
                }
            }
        }
        public void OnSelectedChanged(object sender)
        {
            if (SelectedChanged != null)
            {
                SelectedChanged(sender, new EventArguments.GeocacheEventArgs(this));
            }
        }
        public void OnDataChanged(object sender)
        {
            OnDataChanged(sender, true);
        }
        public void OnDataChanged(object sender, bool storageData)
        {
            if (!_loadingFullData)
            {
                if (storageData)
                {
                    _saved = false;
                }
                _dataChanged = true;
                if (!_updating && DataChanged != null)
                {
                    DataChanged(sender, new EventArguments.GeocacheEventArgs(this));
                }
            }
        }

        public int CompareTo(object obj)
        {
            return string.Compare(this.Code, ((Geocache)obj).Code);
        }
    }
}
