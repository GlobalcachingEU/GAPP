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
        private static long[][] _bufferPropertyLevels = new long[][]
        {
            //4 levels
            new long[] {},
            new long[] {},
            new long[] {150, 154, 162, 170, 180,220,320,350},
            new long[] {150, 154, 162, 170, 180,220,320,350, 380}
        };

        //already stored
        public Log(Storage.RecordInfo recordInfo)
            : base(recordInfo)
        {
            CachePropertyPositions = _bufferPropertyLevels[Core.Settings.Default.DataBufferLevel];
        }

        //new record to be stored
        public Log(Storage.Database db, ILogData data)
            : this(null)
        {
            using (MemoryStream ms = new MemoryStream(DataBuffer))
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

                RecordInfo = db.RequestLogRecord(data.ID, data.GeocacheCode ?? "", DataBuffer, ms.Position, 100);
            }
            db.LogCollection.Add(this);
        }

        public int CompareTo(object obj)
        {
            return string.Compare(this.ID, ((Log)obj).ID);
        }

        public void BufferLevelChanged(int oldLevel)
        {
            CachePropertyPositions = _bufferPropertyLevels[Core.Settings.Default.DataBufferLevel];
            if (oldLevel > Core.Settings.Default.DataBufferLevel)
            {
                if (CachedPropertyValues != null)
                {
                    CachedPropertyValues.Clear();
                }
            }
        }

        public string ID
        {
            get
            {
                return RecordInfo.ID;
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

        public string GeocacheCode
        {
            get
            {
                return RecordInfo.SubID;
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
