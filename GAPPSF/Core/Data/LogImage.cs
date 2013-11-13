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
        private static byte[] _buffer = new byte[2000];

        //already stored
        public LogImage(Storage.RecordInfo recordInfo)
            : base(recordInfo)
        {
            _id = recordInfo.ID;
        }

        //new record to be stored
        public LogImage(Storage.Database db, ILogImageData data)
            : base(null)
        {
            _id = data.ID;
            using (MemoryStream ms = new MemoryStream(_buffer))
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                ms.Position = 0;
                //todo: add string length checks!!!

                ms.Position = 150;
                bw.Write(data.DataFromDate.ToFileTime()); //150
                ms.Position = 180;
                bw.Write(data.LogId??"");
                ms.Position = 220;
                bw.Write(data.Url??"");
                ms.Position = 420;
                bw.Write(data.Name??"");

                RecordInfo = db.RequestLogRecord(data.ID, _buffer, ms.Position, 10);
            }
            db.LogImageCollection.Add(this);
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
                return readString(180);
            }
            set
            {
                string s = LogId;
                SetProperty(180, ref s, value);
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
