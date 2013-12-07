using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public class Waypoint : DataObject, IWaypointData, INotifyPropertyChanged, IComparable
    {
        private static byte[] _buffer = new byte[10000];

        //already stored
        public Waypoint(Storage.RecordInfo recordInfo)
            : base(recordInfo)
        {
            _id = recordInfo.ID;
            _geocacheCode = recordInfo.SubID;
        }

        //new record to be stored
        public Waypoint(Storage.Database db, IWaypointData data)
            : base(null)
        {
            _id = data.ID;
            _geocacheCode = data.GeocacheCode;
            using (MemoryStream ms = new MemoryStream(_buffer))
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                ms.Position = 0;
                //todo: add string length checks!!!

                ms.Position = 150;
                bw.Write(Utils.Conversion.DateTimeToLong(data.DataFromDate)); //150
                bw.Write((bool)(data.Lat!=null)); //158
                bw.Write(data.Lat==null ? (double)0.0: (double)data.Lat); //159
                bw.Write((bool)(data.Lon != null)); //167
                bw.Write(data.Lon == null ? (double)0.0 : (double)data.Lon); //168
                bw.Write(Utils.Conversion.DateTimeToLong(data.Time)); //176
                bw.Write(data.WPType.ID); //184
                //spare
                ms.Position = 200;
                bw.Write(data.GeocacheCode);
                ms.Position = 240;
                bw.Write(data.Code);
                ms.Position = 280;
                bw.Write(data.Description);
                ms.Position = 500;
                bw.Write(data.Name);
                ms.Position = 600;
                bw.Write(data.Url);
                ms.Position = 700;
                bw.Write(data.UrlName);
                ms.Position = 800;
                bw.Write(data.Comment);

                RecordInfo = db.RequestWaypointRecord(data.ID, _geocacheCode, _buffer, ms.Position, 100);
            }
            db.WaypointCollection.Add(this);
        }

        public int CompareTo(object obj)
        {
            return string.Compare(this.ID, ((Waypoint)obj).ID);
        }

        //buffered READONLY
        private string _id = "";
        public string ID
        {
            get
            {
                return _id;
            }
            set
            {
                //SetProperty(-1, ref _id, value);
            }
        }



        public string Name
        {
            get
            {
                return readString(500);
            }
            set
            {
                string s = Name;
                SetProperty(500, ref s, value);
            }
        }

        public string Comment
        {
            get
            {
                return readString(800);
            }
            set
            {
                string s = Comment;
                SetProperty(800, ref s, value);
            }
        }

        public string Description
        {
            get
            {
                return readString(280);
            }
            set
            {
                string s = Description;
                SetProperty(280, ref s, value);
            }
        }

        public string Url
        {
            get
            {
                return readString(600);
            }
            set
            {
                string s = Url;
                SetProperty(600, ref s, value);
            }
        }

        public string UrlName
        {
            get
            {
                return readString(700);
            }
            set
            {
                string s = UrlName;
                SetProperty(700, ref s, value);
            }
        }

        public WaypointType WPType
        {
            get
            {
                return Utils.DataAccess.GetWaypointType(readInt(184));
            }
            set
            {
                int i = readInt(184);
                SetProperty(184, ref i, value.ID);
            }
        }

        public string Code
        {
            get
            {
                return readString(240);
            }
            set
            {
                string s = Code;
                SetProperty(240, ref s, value);
            }
        }

        //buffered READONLY
        private string _geocacheCode = "";
        public string GeocacheCode
        {
            get
            {
                return _geocacheCode;
                //return readString(200);
            }
            set
            {
                //string s = GeocacheCode;
                //SetProperty(200, ref s, value);
            }
        }

        public DateTime DataFromDate
        {
            get
            {
                return DateTime.FromFileTime(readLong(150));
            }
            set
            {
                DateTime _dt = DataFromDate;
                SetProperty(150, ref _dt, value);
            }
        }

        public double? Lat
        {
            get
            {
                if (readBool(158))
                {
                    return readDouble(159);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                double? cl = Lat;
                if (cl != null && value != null)
                {
                    double _d = (double)cl;
                    SetProperty(159, ref cl, value);
                }
                else
                {
                    StoreProperty(158, "LatAvail", value != null);
                    SetProperty(159, ref cl, value);
                }
            }
        }

        public double? Lon
        {
            get
            {
                if (readBool(167))
                {
                    return readDouble(168);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                double? cl = Lon;
                if (cl != null && value != null)
                {
                    double _d = (double)cl;
                    SetProperty(168, ref cl, value);
                }
                else
                {
                    StoreProperty(167, "LonAvail", value != null);
                    SetProperty(168, ref cl, value);
                }
            }
        }

        public DateTime Time
        {
            get
            {
                return DateTime.FromFileTime(readLong(176));
            }
            set
            {
                DateTime _dt = Time;
                SetProperty(176, ref _dt, value);
            }
        }
    }
}
