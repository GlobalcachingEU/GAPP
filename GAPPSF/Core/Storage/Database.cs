using GAPPSF.Core.Data;
using GAPPSF.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Storage
{
    public class Database: IDisposable
    {
        public const byte RECORD_EMPTY = 0;
        public const byte RECORD_GEOCACHE = 1;
        public const byte RECORD_LOG = 2;
        public const long RECORD_POS_LENGTH = 0;
        public const long RECORD_POS_FIELDTYPE = 8;
        public const long RECORD_POS_ID = 50;

        public string FileName { get; private set; }
        public FileStream FileStream { get; private set; }
        public BinaryReader BinaryReader { get; private set; }
        public BinaryWriter BinaryWriter { get; private set; }

        public GeocacheCollection GeocacheCollection { get; private set; }
        public LogCollection LogCollection { get; private set; }

        private Hashtable _records = new Hashtable();
        private List<RecordInfo> _emptyRecords = new List<RecordInfo>();
        private bool _emptyRecordListSorted = false;

        public Database(string fn)
        {
            FileName = fn;
            this.GeocacheCollection = new GeocacheCollection(this);
            this.LogCollection = new LogCollection(this);
        }

        public override string ToString()
        {
            return Path.GetFileNameWithoutExtension(FileName);
        }

        public string Text
        {
            get { return ToString(); }
        }

        async public Task<bool> InitializeAsync()
        {
            bool result = false;
            using (DataUpdater upd = new DataUpdater(this))
            {
                await Task.Run(() => { result = Initialize(); });
            }
            return result;
        }

        public bool Initialize()
        {
            bool result = false;
            try
            {
                FileStream = File.Open(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                BinaryReader = new System.IO.BinaryReader(FileStream);
                BinaryWriter = new System.IO.BinaryWriter(FileStream);
                result = LoadDatabaseFile();
            }
            catch
            {

            }
            return result;
        }

        private bool LoadDatabaseFile()
        {
            bool result = false;
            try
            {
                long max = FileStream.Length;
                DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                using (Utils.ProgressBlock prog = new ProgressBlock("Loading database","Loading...",100,0))
                {
                    byte[] memBuffer = new byte[200];
                    using (MemoryStream ms = new MemoryStream(memBuffer))
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        FileStream fs = FileStream;
                        fs.Position = 0;
                        long eof = fs.Length;
                        while (fs.Position < eof)
                        {
                            RecordInfo ri = new RecordInfo();
                            ri.Database = this;
                            ri.Offset = fs.Position;
                            fs.Read(memBuffer, 0, 150);
                            ms.Position = RECORD_POS_LENGTH;
                            ri.Length = br.ReadInt64();
                            ms.Position = RECORD_POS_FIELDTYPE;
                            byte ft = br.ReadByte();
                            switch (ft)
                            {
                                case RECORD_EMPTY:
                                    //empty
                                    _emptyRecords.Add(ri);
                                    _emptyRecordListSorted = false;
                                    break;
                                case RECORD_GEOCACHE:
                                    ms.Position = RECORD_POS_ID;
                                    ri.ID = br.ReadString();
                                    _records.Add(ri.ID, ri);
                                    this.GeocacheCollection.Add(new Data.Geocache(ri));
                                    break;
                                case RECORD_LOG:
                                    ms.Position = RECORD_POS_ID;
                                    ri.ID = br.ReadString();
                                    _records.Add(ri.ID, ri);
                                    this.LogCollection.Add(new Data.Log(ri));
                                    break;
                            }
                            fs.Position = ri.Offset + ri.Length;

                            if (DateTime.Now>=nextUpdate)
                            {
                                prog.Update("Loading...", 100, (int)(100.0 * (double)fs.Position / (double)max));
                                nextUpdate = DateTime.Now.AddSeconds(1);
                            }
                        }
                    }
                }
                Utils.Calculus.SetDistanceAndAngleGeocacheFromLocation(this.GeocacheCollection, ApplicationData.Instance.CenterLocation);
                result = true;
            }
            catch
            {
            }
            return result;
        }

        public RecordInfo RequestGeocacheRecord(string id, byte[] recordData, long minimumLength, long extraBuffer)
        {
            return RequestRecord(id, RECORD_GEOCACHE, recordData, minimumLength, extraBuffer);
        }

        public RecordInfo RequestLogRecord(string id, byte[] recordData, long minimumLength, long extraBuffer)
        {
            return RequestRecord(id, RECORD_LOG, recordData, minimumLength, extraBuffer);
        }

        private RecordInfo RequestRecord(string id, byte recordType, byte[] recordData, long minimumLength, long extraBuffer)
        {
            RecordInfo result = _records[id] as RecordInfo;
            if (result != null)
            {
                if (result.Length < minimumLength)
                {
                    //too small now, mark as empty
                    FileStream.Position = result.Offset + RECORD_POS_FIELDTYPE;
                    FileStream.WriteByte(RECORD_EMPTY);
                    _records.Remove(id);
                    _emptyRecords.Add(result);
                    _emptyRecordListSorted = false;
                    result = null;
                }
            }
            if (result==null)
            {
                if (!_emptyRecordListSorted)
                {
                    _emptyRecords.Sort(delegate(RecordInfo x, RecordInfo y)
                    {
                        return x.Length.CompareTo(y.Length);
                    });
                    _emptyRecordListSorted = true;
                }
                result = (from a in _emptyRecords where a.Length >= minimumLength select a).FirstOrDefault();

                if (result == null)
                {
                    result = new RecordInfo();
                    result.Length = minimumLength + extraBuffer;
                    result.Offset = FileStream.Length;
                    result.ID = id;
                    result.Database = this;
                    FileStream.SetLength(result.Offset + result.Length);

                    _records.Add(id, result);
                }
                else
                {
                    //re-use of an empty record
                    result.ID = id;
                    _emptyRecords.Remove(result);
                    _records.Add(result.ID, result);
                }
            }
            if (result!=null)
            {
                //write record header data
                FileStream.Position = result.Offset + RECORD_POS_LENGTH;
                BinaryWriter.Write(result.Length);
                FileStream.Position = result.Offset + RECORD_POS_FIELDTYPE;
                FileStream.WriteByte(recordType);
                FileStream.Position = result.Offset + RECORD_POS_ID;
                BinaryWriter.Write(result.ID);

                //write data
                FileStream.Position = result.Offset + 150;
                FileStream.Write(recordData, 150, (int)minimumLength - 150);
            }
            return result;
        }

        public void Dispose()
        {
            if (this.BinaryWriter != null)
            {
                this.BinaryWriter.Dispose();
                this.BinaryWriter = null;
            }
            if (this.BinaryReader != null)
            {
                this.BinaryReader.Dispose();
                this.BinaryReader = null;
            }
            if (this.FileStream != null)
            {
                this.FileStream.Close();
                this.FileStream.Dispose();
                this.FileStream = null;
            }
        }
    }
}
