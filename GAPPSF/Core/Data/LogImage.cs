using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public class LogImage : DataObject, ILogImageData, INotifyPropertyChanged, IComparable
    {
        //already stored
        public LogImage(Storage.RecordInfo recordInfo)
            : base(recordInfo)
        {
        }

        //new record to be stored
        public LogImage(Storage.Database db, ILogImageData data)
            : this(null)
        {
            using (MemoryStream ms = new MemoryStream(DataBuffer))
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                ms.Position = 0;
                //todo: add string length checks!!!

                ms.Position = 150;
                bw.Write(Utils.Conversion.DateTimeToLong(data.DataFromDate)); //150
                ms.Position = 180;
                bw.Write(data.LogId??"");
                ms.Position = 220;
                bw.Write(data.Url??"");
                ms.Position = 420;
                bw.Write(data.Name??"");

                RecordInfo = db.RequestLogRecord(data.ID, data.LogId ?? "", DataBuffer, ms.Position, 10);
            }
            db.LogImageCollection.Add(this);
        }

        public int CompareTo(object obj)
        {
            return string.Compare(this.ID, ((LogImage)obj).ID);
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

        public string LogId
        {
            get
            {
                return RecordInfo.SubID;
                //return readString(180);
            }
            set
            {
                //string s = LogId;
                //SetProperty(180, ref s, value);
            }
        }

        public string Url
        {
            get
            {
                return readString(220);
            }
            set
            {
                string s = Url;
                SetProperty(220, ref s, value);
            }
        }

        public string Name
        {
            get
            {
                return readString(420);
            }
            set
            {
                string s = Name;
                SetProperty(420, ref s, value);
            }
        }
    }
}
