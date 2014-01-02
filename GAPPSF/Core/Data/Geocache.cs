using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public class Geocache : DataObject, IGeocacheData, INotifyPropertyChanged, IComparable
    {
        private static long[][] _bufferPropertyLevels = new long[][]
        {
            //4 levels
            new long[] {},
            new long[] {1100, 194, 198, 206},
            new long[] {1100, 194, 198, 206},
            new long[] {1100, 194, 198, 206}
        };

        //already stored
        public Geocache(Storage.RecordInfo recordInfo)
            : base(recordInfo)
        {
            CachePropertyPositions = _bufferPropertyLevels[Core.Settings.Default.DataBufferLevel];
        }

        //new record to be stored
        public Geocache(Storage.Database db, IGeocacheData data)
            : this(null)
        {
            createRecord(db, data);
            db.GeocacheCollection.Add(this);
        }

        private void createRecord(Storage.Database db, IGeocacheData data)
        {
            using (MemoryStream ms = new MemoryStream(DataBuffer))
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                ms.Position = 0;

                ms.Position = 150;
                bw.Write(data.Archived); //150
                bw.Write(data.Available); //151
                bw.Write(data.Container == null ? 0 : data.Container.ID); //152
                bw.Write(Utils.Conversion.DateTimeToLong(data.DataFromDate)); //156
                bw.Write(Utils.Conversion.DateTimeToLong(data.PublishedTime)); //164
                bw.Write(data.Difficulty); //172
                bw.Write(data.Terrain); //180
                bw.Write(data.Favorites); //188
                bw.Write(data.Flagged); //192
                bw.Write(data.Found); //193
                bw.Write(data.GeocacheType == null ? 0 : data.GeocacheType.ID); //194
                bw.Write(data.Lat); //198
                bw.Write(data.Lon); //206
                bw.Write(data.Locked); //214
                bw.Write(data.CustomLat != null); //215
                if (data.CustomLat != null)
                {
                    bw.Write((double)data.CustomLat); //216
                }
                else
                {
                    bw.Write((double)0.0);
                }
                bw.Write(data.CustomLon != null); //224
                if (data.CustomLon != null)
                {
                    bw.Write((double)data.CustomLon); //225
                }
                else
                {
                    bw.Write((double)0.0);
                }
                bw.Write(data.MemberOnly);

                byte attrCount = (byte)(data.AttributeIds == null ? 0 : data.AttributeIds.Count);
                bw.Write(attrCount); //234
                if (attrCount > 0)
                {
                    foreach (int i in data.AttributeIds)
                    {
                        bw.Write(i);
                    }
                }
                ms.Position = 300;
                bw.Write(data.City ?? "");
                ms.Position = 400;
                bw.Write(data.Country ?? "");
                ms.Position = 500;
                bw.Write(GetSafeString(500, 1000, data.EncodedHints) ?? "");
                ms.Position = 1000;
                bw.Write(data.Municipality ?? "");
                ms.Position = 1100;
                bw.Write(data.Name ?? "");
                ms.Position = 1200;
                bw.Write(GetSafeString(1200, 2000, data.Notes) ?? "");
                ms.Position = 2000;
                bw.Write(data.Owner ?? "");
                ms.Position = 2100;
                bw.Write(data.OwnerId ?? "");
                ms.Position = 2150;
                bw.Write(GetSafeString(2150, 2400, data.PersonalNote) ?? "");
                ms.Position = 2400;
                bw.Write(data.PlacedBy ?? "");
                ms.Position = 2500;
                bw.Write(data.State ?? "");
                ms.Position = 2600;
                bw.Write(data.Url ?? "");
                //spare
                ms.Position = 3000;
                bw.Write(data.LongDescriptionInHtml); //3000
                bw.Write(data.ShortDescriptionInHtml); //3001
                //read both if needs description!
                bw.Write(data.ShortDescription ?? ""); //3002
                bw.Write(data.LongDescription ?? "");

                RecordInfo = db.RequestGeocacheRecord(data.Code, "", DataBuffer, ms.Position, 500);
            }
        }

        public Storage.Database Database
        {
            get
            {
                return RecordInfo == null ? null : RecordInfo.Database;
            }
        }

        public int CompareTo(object obj)
        {
            return string.Compare(this.Code, ((Geocache)obj).Code);
        }

        public void BufferLevelChanged(int oldLevel)
        {
            CachePropertyPositions = _bufferPropertyLevels[Core.Settings.Default.DataBufferLevel];
            if (oldLevel>Core.Settings.Default.DataBufferLevel)
            {
                if (CachedPropertyValues != null)
                {
                    CachedPropertyValues.Clear();
                }
            }
        }


        private void checkDescriptionsFit(string sd, string ld)
        {
            string s = string.Concat(sd, ld);
            if (!checkStringFits(s, 3010)) //add bytes for extra string length of long description
            {
                //oeps: ran out of space
                //mark this record for deletion
                Storage.Database db = RecordInfo.Database;
                this.DeleteRecord();
                GeocacheData gd = new GeocacheData();
                GeocacheData.Copy(this, gd);
                gd.ShortDescription = sd ?? "";
                gd.LongDescription = ld ?? "";
                createRecord(db, gd);
            }
        }

        protected override void StoreProperty(long pos, string name, object value)
        {
            if (name == "ShortDescription")
            {
                this.RecordInfo.Database.FileStream.Position = this.RecordInfo.Offset + 3002;
                string sd = this.RecordInfo.Database.BinaryReader.ReadString();
                string ld = this.RecordInfo.Database.BinaryReader.ReadString();
                checkDescriptionsFit(value as string, ld);
                this.RecordInfo.Database.FileStream.Position = this.RecordInfo.Offset + 3002;
                this.RecordInfo.Database.BinaryWriter.Write(value as string ?? "");
                this.RecordInfo.Database.BinaryWriter.Write(ld);
            }
            else if (name == "LongDescription")
            {
                this.RecordInfo.Database.FileStream.Position = this.RecordInfo.Offset + 3002;
                string sd = this.RecordInfo.Database.BinaryReader.ReadString();
                checkDescriptionsFit(sd, value as string);
                this.RecordInfo.Database.FileStream.Position = this.RecordInfo.Offset + 3002;
                this.RecordInfo.Database.BinaryWriter.Write(sd);
                this.RecordInfo.Database.BinaryWriter.Write(value as string ?? "");
            }
            else
            {
                base.StoreProperty(pos, name, value);
            }
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
                    List<Data.Waypoint> wpl = RecordInfo.Database.WaypointCollection.GetWaypoints(Code);
                    _waypointsInfo = string.Format("{0:00}/{1:00}", wpl.Count, (from w in wpl where w.Lat != null && w.Lon != null select w).Count());
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
                    _hasUserWaypoints = (from Data.UserWaypoint w in RecordInfo.Database.UserWaypointCollection where w.GeocacheCode == Code select w).FirstOrDefault() != null;
                }
                return (bool)_hasUserWaypoints;
            }
        }

        public void ResetCachedLogData()
        {
            _cachedFoundDateValid = false;
        }

        public string AccountName
        {
            get
            {
                string result = "";
                if (this.Code.Length>1)
                {
                    AccountInfo ai = ApplicationData.Instance.AccountInfos.GetAccountInfo(this.Code.Substring(0, 2));
                    if (ai!=null)
                    {
                        result = ai.AccountName ?? "";
                    }
                }
                return result;
            }
        }

        public bool IsOwn
        {
            get
            {
                return (string.Compare(AccountName, this.Owner, true)==0);
            }
        }

        public bool ContainsNote
        {
            get
            {
                return (!string.IsNullOrEmpty(this.PersonalNote) && !string.IsNullOrEmpty(this.Notes));
            }
        }

        private bool _cachedFoundDateValid = false;
        private object _cachedFoundDate = null;
        public object FoundDate
        {
            get
            {
                if (!_cachedFoundDateValid)
                {
                    if (Found)
                    {
                        string usrName = AccountName;
                        List<Data.Log> lgs = RecordInfo.Database.LogCollection.GetLogs(this.Code);
                        if (lgs != null)
                        {
                            Data.Log l = (from Data.Log lg in lgs where lg.GeocacheCode == Code && lg.LogType.AsFound && lg.Finder == usrName select lg).FirstOrDefault();
                            if (l != null)
                            {
                                //result = l.Date.ToString("yyyy-MM-dd");
                                _cachedFoundDate = l.Date;
                            }
                        }
                    }
                    _cachedFoundDateValid = true;
                }
                return _cachedFoundDate;
            }
        }


        private bool _selected = false;
        public bool Selected
        {
            get { return _selected; }
            set
            {
                SetProperty(-1, ref _selected, value);
            }
        }

        public string Code
        {
            get
            {
                return RecordInfo.ID;
            }
            set
            {
                //SetProperty(-1, ref _code, value);
            }
        }

        private long _distanceToCenter = 0;
        public long DistanceToCenter
        {
            get { return _distanceToCenter; }
            set
            {
                SetProperty(-1, ref _distanceToCenter, value);
            }
        }

        private int _angleToCenter = 0;
        public int AngleToCenter
        {
            get { return _angleToCenter; }
            set
            {
                SetProperty(-1, ref _angleToCenter, value);
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

        public bool ContainsCustomLatLon
        {
            get { return (CustomLat != null && CustomLon != null); }
        }

        public string Name
        {
            get
            {
                return readString(1100);
            }
            set
            {
                string _name = Name;
                SetProperty(1100, ref _name, value);
            }
        }

        public DateTime DataFromDate
        {
            get
            {
                return Utils.Conversion.DateTimeFromLong(readLong(156));
            }
            set
            {
                DateTime _dt = DataFromDate;
                SetProperty(156, ref _dt, value);
            }
        }

        public DateTime PublishedTime
        {
            get
            {
                return Utils.Conversion.DateTimeFromLong(readLong(164));
            }
            set
            {
                DateTime _dt = PublishedTime;
                SetProperty(164, ref _dt, value);
            }
        }

        public double Lat
        {
            get
            {
                return readDouble(198);
            }
            set
            {
                double _d = Lat;
                SetProperty(198, ref _d, value);
            }
        }

        public double Lon
        {
            get
            {
                return readDouble(206);
            }
            set
            {
                double _d = Lon;
                SetProperty(206, ref _d, value);
            }
        }

        public double? CustomLat
        {
            get
            {
                if (readBool(215))
                {
                    return readDouble(216);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                double? cl = CustomLat;
                if (cl != null && value != null)
                {
                    double _d = (double)cl;
                    SetProperty(216, ref cl, value);
                }
                else
                {
                    StoreProperty(215, "CustomLat", value != null);
                    SetProperty(216, ref cl, value);                    
                }
            }
        }

        public double? CustomLon
        {
            get
            {
                if (readBool(224))
                {
                    return readDouble(225);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                double? cl = CustomLon;
                if (cl != null && value != null)
                {
                    double _d = (double)cl;
                    SetProperty(225, ref cl, value);
                }
                else
                {
                    StoreProperty(224, "CustomLon", value != null);
                    SetProperty(225, ref cl, value);
                }
            }
        }

        public bool Available
        {
            get
            {
                return readBool(151);
            }
            set
            {
                bool b = Available;
                SetProperty(151, ref b, value);
            }
        }

        public bool Archived
        {
            get
            {
                return readBool(150);
            }
            set
            {
                bool b = Archived;
                SetProperty(150, ref b, value);
            }
        }

        public string Country
        {
            get
            {
                return readString(400);
            }
            set
            {
                string s = Country;
                SetProperty(400, ref s, value);
            }
        }

        public string State
        {
            get
            {
                return readString(2500);
            }
            set
            {
                string s = State;
                SetProperty(2500, ref s, value);
            }
        }

        public string Municipality
        {
            get
            {
                return readString(1000);
            }
            set
            {
                string s = Municipality;
                SetProperty(1000, ref s, value);
            }
        }

        public string City
        {
            get
            {
                return readString(300);
            }
            set
            {
                string s = City;
                SetProperty(300, ref s, value);
            }
        }

        public GeocacheType GeocacheType
        {
            get
            {
                return Utils.DataAccess.GetGeocacheType(readInt(194));
            }
            set
            {
                int i = readInt(194);
                SetProperty(194, ref i, value.ID);
            }
        }

        public string PlacedBy
        {
            get
            {
                return readString(2400);
            }
            set
            {
                string s = PlacedBy;
                SetProperty(2400, ref s, value);
            }
        }

        public string Owner
        {
            get
            {
                return readString(2000);
            }
            set
            {
                string s = Owner;
                SetProperty(2000, ref s, value);
            }
        }

        public string OwnerId
        {
            get
            {
                return readString(2100);
            }
            set
            {
                string s = OwnerId;
                SetProperty(2100, ref s, value);
            }
        }

        public GeocacheContainer Container
        {
            get
            {
                return Utils.DataAccess.GetGeocacheContainer(readInt(152));
            }
            set
            {
                int i = readInt(152);
                SetProperty(152, ref i, value.ID);
            }
        }

        public double Terrain
        {
            get
            {
                return readDouble(180);
            }
            set
            {
                double d = Terrain;
                SetProperty(180, ref d, value);
            }
        }

        public double Difficulty
        {
            get
            {
                return readDouble(172);
            }
            set
            {
                double d = Difficulty;
                SetProperty(172, ref d, value);
            }
        }

        public string ShortDescription
        {
            get
            {
                return readString(3002);
            }
            set
            {
                string s = ShortDescription;
                SetProperty(-1, ref s, value);
            }
        }

        public bool ShortDescriptionInHtml
        {
            get
            {
                return readBool(3001);
            }
            set
            {
                bool b = ShortDescriptionInHtml;
                SetProperty(3001, ref b, value);
            }
        }

        public string LongDescription
        {
            get
            {
                readString(3002); //short description
                return this.RecordInfo.Database.BinaryReader.ReadString();
            }
            set
            {
                string s = LongDescription;
                SetProperty(-1, ref s, value);
            }
        }

        public bool LongDescriptionInHtml
        {
            get
            {
                return readBool(3000);
            }
            set
            {
                bool b = LongDescriptionInHtml;
                SetProperty(3000, ref b, value);
            }
        }

        public string EncodedHints
        {
            get
            {
                return readString(500);
            }
            set
            {
                string s = EncodedHints;
                SetStringProperty(500, 1000, ref s, value);
            }
        }

        public string Url
        {
            get
            {
                return readString(2600);
            }
            set
            {
                string s = EncodedHints;
                SetProperty(2600, ref s, value);
            }
        }

        public bool MemberOnly
        {
            get
            {
                return readBool(233);
            }
            set
            {
                bool b = MemberOnly;
                SetProperty(233, ref b, value);
            }
        }

        public List<int> AttributeIds
        {
            get
            {
                List<int> result = new List<int>();
                int cnt = readByte(234);
                int pos = 235;
                for (int i = 0; i < cnt; i++ )
                {
                    result.Add(readInt(pos));
                    pos += 4;
                }
                return result;
            }
            set
            {
                byte cnt = (byte)(value == null ? 0 : value.Count);
                StoreProperty(234, "AttributeIds", cnt);
                if (cnt > 0)
                {
                    int pos = 235;
                    foreach (int i in value)
                    {
                        StoreProperty(pos, "AttributeId", cnt);
                        pos += 4;
                    }
                }
            }
        }

        public int Favorites
        {
            get
            {
                return readInt(188);
            }
            set
            {
                int i = Favorites;
                SetProperty(188, ref i, value);
            }
        }

        public string PersonalNote
        {
            get
            {
                return readString(2150);
            }
            set
            {
                string s = PersonalNote;
                SetStringProperty(2150, 2400, ref s, value);
            }
        }

        public string Notes
        {
            get
            {
                return readString(1200);
            }
            set
            {
                string s = PersonalNote;
                SetStringProperty(1200, 2000, ref s, value);
            }
        }

        public bool Flagged
        {
            get
            {
                return readBool(192);
            }
            set
            {
                bool b = Flagged;
                SetProperty(192, ref b, value);
            }
        }

        public bool Found
        {
            get
            {
                return readBool(193);
            }
            set
            {
                bool b = Found;
                SetProperty(193, ref b, value);
            }
        }

        public bool Locked
        {
            get
            {
                return readBool(214);
            }
            set
            {
                bool b = Locked;
                SetProperty(214, ref b, value);
            }
        }
    }
}
