using GAPPSF.Core.Data;
using GAPPSF.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GAPPSF.Core.Storage
{
    public class Database: IDisposable
    {
        public const byte RECORD_EMPTY = 0;
        public const byte RECORD_GEOCACHE = 1;
        public const byte RECORD_LOG = 2;
        public const byte RECORD_WAYPOINT = 3;
        public const byte RECORD_USERWAYPOINT = 4;
        public const byte RECORD_LOGIMAGE = 5;
        public const byte RECORD_GEOCACHEIMAGE = 6;
        public const long RECORD_POS_LENGTH = 0;
        public const long RECORD_POS_FIELDTYPE = 8;
        public const long RECORD_POS_ID = 50;

        public string FileName { get; private set; }
        public FileStream FileStream { get; private set; }
        public BinaryReader BinaryReader { get; private set; }
        public BinaryWriter BinaryWriter { get; private set; }
        private FileStream _fileStreamIdx = null;

        public LogCollection LogCollection { get; private set; }
        public LogImageCollection LogImageCollection { get; private set; }
        public WaypointCollection WaypointCollection { get; private set; }
        public UserWaypointCollection UserWaypointCollection { get; private set; }
        public GeocacheImageCollection GeocacheImageCollection { get; private set; }
        public GeocacheCollection GeocacheCollection { get; private set; }

        private List<RecordInfo> _emptyRecords = new List<RecordInfo>();
        private bool _emptyRecordListSorted = false;

        public Database(string fn)
        {
            FileName = fn;
            this.LogCollection = new LogCollection(this);
            this.LogImageCollection = new LogImageCollection(this);
            this.WaypointCollection = new WaypointCollection(this);
            this.UserWaypointCollection = new UserWaypointCollection(this);
            this.GeocacheImageCollection = new GeocacheImageCollection(this);
            this.GeocacheCollection = new GeocacheCollection(this);
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
                if (!LoadIndexFile())
                {
                    result = LoadDatabaseFile();
                }
            }
            catch
            {

            }
            return result;
        }

        private bool LoadIndexFile()
        {
            bool result = false;
            string fn = string.Concat(FileName, ".gsx");
            try
            {
                //check if file exists
                //if not, it will be created during loading of database file
                if (File.Exists(fn))
                {
                    _fileStreamIdx = File.Open(fn, FileMode.OpenOrCreate, FileAccess.ReadWrite);

                    long max = _fileStreamIdx.Length;
                    DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                    using (Utils.ProgressBlock prog = new ProgressBlock("Loading database", "Loading...", 100, 0))
                    {
                        byte[] buffer = new byte[125];
                        using (MemoryStream ms = new MemoryStream(buffer))
                        using (BinaryReader br = new BinaryReader(ms))
                        {
                            while (_fileStreamIdx.Position < max)
                            {
                                RecordInfo ri = new RecordInfo();
                                ri.OffsetIdx = _fileStreamIdx.Position;
                                _fileStreamIdx.Read(buffer, 0, 125);
                                ms.Position = 0;
                                ri.Offset = br.ReadInt64();
                                ri.Length = br.ReadInt64();
                                ri.FieldType = br.ReadByte();
                                if (ri.FieldType != RECORD_EMPTY)
                                {
                                    ri.ID = br.ReadString();
                                }
                                switch (ri.FieldType)
                                {
                                    case RECORD_EMPTY:
                                        //empty
                                        _emptyRecords.Add(ri);
                                        _emptyRecordListSorted = false;
                                        break;
                                    case RECORD_GEOCACHE:
                                        this.GeocacheCollection.Add(new Data.Geocache(ri));
                                        break;
                                    case RECORD_LOG:
                                        this.LogCollection.Add(new Data.Log(ri));
                                        break;
                                    case RECORD_WAYPOINT:
                                        this.WaypointCollection.Add(new Data.Waypoint(ri));
                                        break;
                                    case RECORD_USERWAYPOINT:
                                        this.UserWaypointCollection.Add(new Data.UserWaypoint(ri));
                                        break;
                                    case RECORD_LOGIMAGE:
                                        this.LogImageCollection.Add(new Data.LogImage(ri));
                                        break;
                                    case RECORD_GEOCACHEIMAGE:
                                        this.GeocacheImageCollection.Add(new Data.GeocacheImage(ri));
                                        break;
                                }

                                if (DateTime.Now >= nextUpdate)
                                {
                                    prog.Update("Loading...", 100, (int)(100.0 * (double)_fileStreamIdx.Position / (double)max));
                                    nextUpdate = DateTime.Now.AddSeconds(1);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                try
                {
                    if (_fileStreamIdx!=null)
                    {
                        _fileStreamIdx.Dispose();
                        _fileStreamIdx = null;
                    }
                    File.Delete(fn);
                }
                catch
                {
                }
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
                using (Utils.ProgressBlock prog = new ProgressBlock("Loading database", "Loading...", 100, 0))
                {
                    FileStream fs = FileStream;
                    fs.Position = 0;
                    long eof = fs.Length;
                    while (fs.Position < eof)
                    {
                        RecordInfo ri = new RecordInfo();
                        ri.Database = this;
                        ri.Offset = fs.Position;
                        fs.Position = ri.Offset+RECORD_POS_LENGTH;
                        ri.Length = BinaryReader.ReadInt64();
                        fs.Position = ri.Offset + RECORD_POS_FIELDTYPE;
                        ri.FieldType = BinaryReader.ReadByte();
                        switch (ri.FieldType)
                        {
                            case RECORD_EMPTY:
                                //empty
                                _emptyRecords.Add(ri);
                                _emptyRecordListSorted = false;
                                break;
                            case RECORD_GEOCACHE:
                                fs.Position = ri.Offset + RECORD_POS_ID;
                                ri.ID = BinaryReader.ReadString();
                                this.GeocacheCollection.Add(new Data.Geocache(ri));
                                break;
                            case RECORD_LOG:
                                fs.Position = ri.Offset + RECORD_POS_ID;
                                ri.ID = BinaryReader.ReadString();
                                this.LogCollection.Add(new Data.Log(ri));
                                break;
                            case RECORD_WAYPOINT:
                                fs.Position = ri.Offset + RECORD_POS_ID;
                                ri.ID = BinaryReader.ReadString();
                                this.WaypointCollection.Add(new Data.Waypoint(ri));
                                break;
                            case RECORD_USERWAYPOINT:
                                fs.Position = ri.Offset + RECORD_POS_ID;
                                ri.ID = BinaryReader.ReadString();
                                this.UserWaypointCollection.Add(new Data.UserWaypoint(ri));
                                break;
                            case RECORD_LOGIMAGE:
                                fs.Position = ri.Offset + RECORD_POS_ID;
                                ri.ID = BinaryReader.ReadString();
                                this.LogImageCollection.Add(new Data.LogImage(ri));
                                break;
                            case RECORD_GEOCACHEIMAGE:
                                fs.Position = ri.Offset + RECORD_POS_ID;
                                ri.ID = BinaryReader.ReadString();
                                this.GeocacheImageCollection.Add(new Data.GeocacheImage(ri));
                                break;
                        }
                        fs.Position = ri.Offset + ri.Length;
                        if (DateTime.Now >= nextUpdate)
                        {
                            prog.Update("Loading...", 100, (int)(100.0 * (double)fs.Position / (double)max));
                            nextUpdate = DateTime.Now.AddSeconds(1);
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

        public RecordInfo RequestWaypointRecord(string id, byte[] recordData, long minimumLength, long extraBuffer)
        {
            return RequestRecord(id, RECORD_WAYPOINT, recordData, minimumLength, extraBuffer);
        }

        public RecordInfo RequestUserWaypointRecord(string id, byte[] recordData, long minimumLength, long extraBuffer)
        {
            return RequestRecord(id, RECORD_USERWAYPOINT, recordData, minimumLength, extraBuffer);
        }
        public RecordInfo RequestLogImageRecord(string id, byte[] recordData, long minimumLength, long extraBuffer)
        {
            return RequestRecord(id, RECORD_LOGIMAGE, recordData, minimumLength, extraBuffer);
        }
        public RecordInfo RequestGeocacheImageRecord(string id, byte[] recordData, long minimumLength, long extraBuffer)
        {
            return RequestRecord(id, RECORD_GEOCACHEIMAGE, recordData, minimumLength, extraBuffer);
        }

        public void DeleteRecord(RecordInfo ri)
        {
            FileStream.Position = ri.Offset + RECORD_POS_FIELDTYPE;
            BinaryWriter.Write(RECORD_EMPTY);
            _emptyRecords.Add(ri);
            _emptyRecordListSorted = false;
        }

        private RecordInfo RequestRecord(string id, byte recordType, byte[] recordData, long minimumLength, long extraBuffer)
        {
            RecordInfo result = null;
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
            }
            else
            {
                //re-use of an empty record
                result.ID = id;
                _emptyRecords.Remove(result);
            }
            if (result != null)
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
            if (_fileStreamIdx!=null)
            {
                _fileStreamIdx.Close();
                _fileStreamIdx.Dispose();
                _fileStreamIdx = null;
            }
        }
    }
}
