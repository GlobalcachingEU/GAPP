using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public class Log : DataObject, ILogData, INotifyPropertyChanged, IComparable
    {
        private static byte[] _buffer = new byte[50000];

        //already stored
        public Log(Storage.RecordInfo recordInfo)
            : base(recordInfo)
        {
            _id = recordInfo.ID;
            _geocacheCode = recordInfo.SubID;
        }

        //new record to be stored
        public Log(Storage.Database db, ILogData data)
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
                bw.Write(data.LogType.ID); //150
                bw.Write(Utils.Conversion.DateTimeToLong(data.Date)); //154
                bw.Write(Utils.Conversion.DateTimeToLong(data.DataFromDate)); //162
                bw.Write(data.Encoded); //170
                ms.Position = 180;
                bw.Write(data.GeocacheCode??"");
                ms.Position = 220;
                bw.Write(data.Finder??"");
                ms.Position = 320;
                bw.Write(data.FinderId??"");
                ms.Position = 350;
                bw.Write(data.TBCode??"");
                ms.Position = 380;
                bw.Write(data.Text??"");

                RecordInfo = db.RequestLogRecord(data.ID, data.GeocacheCode ?? "", _buffer, ms.Position, 100);
            }
            db.LogCollection.Add(this);
        }

        public int CompareTo(object obj)
        {
            return string.Compare(this.ID, ((Log)obj).ID);
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



        public LogType LogType
        {
            get
            {
                return Utils.DataAccess.GetLogType(readInt(150));
            }
            set
            {
                int i = readInt(150);
                SetProperty(150, ref i, value.ID);
            }
        }

        //buffered, readonly
        private string _geocacheCode = "";
        public string GeocacheCode
        {
            get
            {
                return _geocacheCode;
                //return readString(180);
            }
            set
            {
                //string s = GeocacheCode;
                //SetProperty(180, ref s, value);
            }
        }

        public string TBCode
        {
            get
            {
                return readString(350);
            }
            set
            {
                string s = TBCode;
                SetProperty(350, ref s, value);
            }
        }

        public DateTime Date
        {
            get
            {
                return DateTime.FromFileTime(readLong(154));
            }
            set
            {
                DateTime _dt = Date;
                SetProperty(154, ref _dt, value);
            }
        }

        public DateTime DataFromDate
        {
            get
            {
                return DateTime.FromFileTime(readLong(162));
            }
            set
            {
                DateTime _dt = DataFromDate;
                SetProperty(162, ref _dt, value);
            }
        }

        public string FinderId
        {
            get
            {
                return readString(320);
            }
            set
            {
                string s = FinderId;
                SetProperty(320, ref s, value);
            }
        }

        public string Finder
        {
            get
            {
                return readString(220);
            }
            set
            {
                string s = Finder;
                SetProperty(220, ref s, value);
            }
        }

        public string Text
        {
            get
            {
                return readString(380);
            }
            set
            {
                string s = Text;
                SetProperty(380, ref s, value);
            }
        }

        public bool Encoded
        {
            get
            {
                return readBool(170);
            }
            set
            {
                bool b = Encoded;
                SetProperty(170, ref b, value);
            }
        }
    }
}
