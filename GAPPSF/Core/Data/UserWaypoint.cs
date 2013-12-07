using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public class UserWaypoint : DataObject, IUserWaypointData, INotifyPropertyChanged, IComparable
    {
        private static byte[] _buffer = new byte[10000];

        //already stored
        public UserWaypoint(Storage.RecordInfo recordInfo)
            : base(recordInfo)
        {
            _id = recordInfo.ID;
            _geocacheCode = recordInfo.SubID;
        }

        //new record to be stored
        public UserWaypoint(Storage.Database db, IUserWaypointData data)
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
                bw.Write(Utils.Conversion.DateTimeToLong(data.Date)); //150
                bw.Write(data.Lat); //158
                bw.Write(data.Lon); //166
                ms.Position = 200;
                bw.Write(data.GeocacheCode); 
                ms.Position = 220;
                bw.Write(data.Description);

                RecordInfo = db.RequestUserWaypointRecord(data.ID, _geocacheCode, _buffer, ms.Position, 50);
            }
            db.UserWaypointCollection.Add(this);
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




        //byffered READONLY
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

        public string Description
        {
            get
            {
                return readString(220);
            }
            set
            {
                string s = Description;
                SetProperty(220, ref s, value);
            }
        }

        public double Lat
        {
            get
            {
                return readDouble(158);
            }
            set
            {
                double _d = Lat;
                SetProperty(158, ref _d, value);
            }
        }

        public double Lon
        {
            get
            {
                return readDouble(166);
            }
            set
            {
                double _d = Lon;
                SetProperty(166, ref _d, value);
            }
        }

        public DateTime Date
        {
            get
            {
                return DateTime.FromFileTime(readLong(150));
            }
            set
            {
                DateTime _dt = Date;
                SetProperty(150, ref _dt, value);
            }
        }
    }
}
