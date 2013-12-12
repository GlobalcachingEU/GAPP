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
        public const long DATABASE_CONTENT_OFFSET = 1024;
        public const long DATABASE_META_VERSION_POS = 0;
        public const long DATABASE_META_ACTIVEGEOCACHE_POS = 8;

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

        public long Version { get; private set; }


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

        private string _lastActiveGeocacheCode = "";
        public string LastActiveGeocacheCode
        {
            get { return _lastActiveGeocacheCode; }
            set
            {
                if (_lastActiveGeocacheCode!=value)
                {
                    _lastActiveGeocacheCode = value;
                    this.FileStream.Position = DATABASE_META_ACTIVEGEOCACHE_POS;
                    BinaryWriter.Write(_lastActiveGeocacheCode ?? "");
                }
            }
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
                this.FileStream = File.Open(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                BinaryReader = new System.IO.BinaryReader(this.FileStream);
                BinaryWriter = new System.IO.BinaryWriter(this.FileStream);
                bool cancelled = false;
                if (LoadDatabaseMetaData())
                {
                    if (!LoadIndexFile(ref cancelled))
                    {
                        if (!cancelled)
                        {
                            result = LoadDatabaseFile();
                        }
                    }
                    else
                    {
                        result = true;
                    }
                }
            }
            catch
            {

            }
            return result;
        }

        public void Flush()
        {
            if (FileStream != null)
            {
                FileStream.Flush();
            }
            if (_fileStreamIdx != null)
            {
                _fileStreamIdx.Flush();
            }
        }

        private bool LoadDatabaseMetaData()
        {
            bool result = true;
            if (this.FileStream.Length==0)
            {
                //create meta data
                this.FileStream.SetLength(DATABASE_CONTENT_OFFSET);
                this.FileStream.Position = DATABASE_META_VERSION_POS;
                this.Version = 1;
                BinaryWriter.Write(this.Version);
                _lastActiveGeocacheCode = "";
                this.FileStream.Position = DATABASE_META_ACTIVEGEOCACHE_POS;
                BinaryWriter.Write(_lastActiveGeocacheCode);
            }
            else if (this.FileStream.Length >= DATABASE_CONTENT_OFFSET)
            {
                //read meta data
                this.FileStream.Position = DATABASE_META_VERSION_POS;
                this.Version = BinaryReader.ReadInt64();
                this.FileStream.Position = DATABASE_META_ACTIVEGEOCACHE_POS;
                _lastActiveGeocacheCode = BinaryReader.ReadString();
            }
            else
            {
                result = false;
            }
            return result;
        }

        private bool LoadIndexFile(ref bool cancelled)
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
                    using (Utils.ProgressBlock prog = new ProgressBlock("LoadingDatabase", "Loading", 100, 0, true))
                    {
                        int maxChunks = 1000;
                        int chunkSize = 117;
                        byte[] buffer = new byte[maxChunks * chunkSize];
                        using (MemoryStream ms = new MemoryStream(buffer))
                        using (BinaryReader br = new BinaryReader(ms))
                        {
                            while (_fileStreamIdx.Position < max)
                            {
                                long startPos = _fileStreamIdx.Position;
                                int chunksRead = _fileStreamIdx.Read(buffer, 0, maxChunks * chunkSize) / chunkSize;
                                for (int i = 0; i < chunksRead; i++)
                                {
                                    RecordInfo ri = new RecordInfo();
                                    ri.Database = this;
                                    ri.OffsetIdx = startPos + i * chunkSize;
                                    ms.Position = i * chunkSize;
                                    ri.Offset = br.ReadInt64();
                                    ri.Length = br.ReadInt64();
                                    ri.FieldType = br.ReadByte();
                                    if (ri.FieldType != RECORD_EMPTY)
                                    {
                                        ri.ID = br.ReadString();
                                        ri.SubID = br.ReadString();
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
                                        if (!prog.Update("Loading", 100, (int)(100.0 * (double)_fileStreamIdx.Position / (double)max)))
                                        {
                                            cancelled = true;
                                            break;
                                        }
                                        nextUpdate = DateTime.Now.AddSeconds(1);
                                    }
                                }
                            }
                            if (!cancelled)
                            {
                                Utils.Calculus.SetDistanceAndAngleGeocacheFromLocation(GeocacheCollection, Core.ApplicationData.Instance.CenterLocation);
                                result = true;
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
            //index file not available
            //create one
            //this is an exception. (should be anyway)
            //first create it in a temporary file and copy to target if finished
            bool result = false;
            bool cancelled = false;
            try
            {
                string fn = string.Concat(FileName, ".gsx");
                if (File.Exists(fn))
                {
                    File.Delete(fn);
                }
                byte[] buffer = new byte[117];
                using (TemporaryFile tf = new TemporaryFile())
                using (FileStream fsIdx = File.Open(tf.Path, FileMode.OpenOrCreate, FileAccess.Write))
                using (MemoryStream ms = new MemoryStream(buffer))
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    long max = this.FileStream.Length;
                    DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                    using (Utils.ProgressBlock prog = new ProgressBlock("LoadingDatabase", "Loading", 100, 0, true))
                    {
                        this.FileStream.Position = DATABASE_CONTENT_OFFSET;
                        long eof = this.FileStream.Length;
                        while (this.FileStream.Position < eof)
                        {
                            RecordInfo ri = new RecordInfo();
                            ri.Database = this;
                            ri.Offset = this.FileStream.Position;
                            this.FileStream.Position = ri.Offset + RECORD_POS_LENGTH;
                            ri.Length = BinaryReader.ReadInt64();
                            this.FileStream.Position = ri.Offset + RECORD_POS_FIELDTYPE;
                            ri.FieldType = BinaryReader.ReadByte();
                            if (ri.FieldType == RECORD_EMPTY)
                            {
                                _emptyRecords.Add(ri);
                                _emptyRecordListSorted = false;
                            }
                            else
                            {
                                this.FileStream.Position = ri.Offset + RECORD_POS_ID;
                                ri.ID = BinaryReader.ReadString();
                                ri.SubID = BinaryReader.ReadString();
                                switch (ri.FieldType)
                                {
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
                            }
                            this.FileStream.Position = ri.Offset + ri.Length;

                            ri.OffsetIdx = fsIdx.Position;
                            ms.Position = 0;
                            bw.Write(ri.Offset);
                            bw.Write(ri.Length);
                            bw.Write(ri.FieldType);
                            if (ri.FieldType != RECORD_EMPTY)
                            {
                                bw.Write(ri.ID);
                                bw.Write(ri.SubID);
                            }
                            else
                            {
                                bw.Write("");
                                bw.Write("");
                            }
                            fsIdx.Write(buffer, 0, 117);

                            if (DateTime.Now >= nextUpdate)
                            {
                                if (!prog.Update("Loading", 100, (int)(100.0 * (double)this.FileStream.Position / (double)max)))
                                {
                                    cancelled = true;
                                    break;
                                }
                                nextUpdate = DateTime.Now.AddSeconds(1);
                            }
                        }
                        //if all OK and not canceled
                        fsIdx.Close();
                        if (!cancelled)
                        {
                            File.Copy(tf.Path, fn);
                            _fileStreamIdx = File.Open(fn, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                            Utils.Calculus.SetDistanceAndAngleGeocacheFromLocation(this.GeocacheCollection, ApplicationData.Instance.CenterLocation);
                            result = true;
                        }
                    }
                }
            }
            catch
            {
            }
            return result;
        }

        public RecordInfo RequestGeocacheRecord(string id, string subId, byte[] recordData, long minimumLength, long extraBuffer)
        {
            return RequestRecord(id, subId ?? "", RECORD_GEOCACHE, recordData, minimumLength, extraBuffer);
        }

        public RecordInfo RequestLogRecord(string id, string subId, byte[] recordData, long minimumLength, long extraBuffer)
        {
            return RequestRecord(id, subId ?? "", RECORD_LOG, recordData, minimumLength, extraBuffer);
        }

        public RecordInfo RequestWaypointRecord(string id, string subId, byte[] recordData, long minimumLength, long extraBuffer)
        {
            return RequestRecord(id, subId ?? "", RECORD_WAYPOINT, recordData, minimumLength, extraBuffer);
        }

        public RecordInfo RequestUserWaypointRecord(string id, string subId, byte[] recordData, long minimumLength, long extraBuffer)
        {
            return RequestRecord(id, subId ?? "", RECORD_USERWAYPOINT, recordData, minimumLength, extraBuffer);
        }
        public RecordInfo RequestLogImageRecord(string id, string subId, byte[] recordData, long minimumLength, long extraBuffer)
        {
            return RequestRecord(id, subId ?? "", RECORD_LOGIMAGE, recordData, minimumLength, extraBuffer);
        }
        public RecordInfo RequestGeocacheImageRecord(string id, string subId, byte[] recordData, long minimumLength, long extraBuffer)
        {
            return RequestRecord(id, subId??"", RECORD_GEOCACHEIMAGE, recordData, minimumLength, extraBuffer);
        }

        public void DeleteRecord(RecordInfo ri)
        {
            FileStream.Position = ri.Offset + RECORD_POS_FIELDTYPE;
            BinaryWriter.Write(RECORD_EMPTY);
            _emptyRecords.Add(ri);
            _emptyRecordListSorted = false;

            if (_fileStreamIdx!=null)
            {
                _fileStreamIdx.Position = ri.OffsetIdx + 16;
                _fileStreamIdx.WriteByte(RECORD_EMPTY);
            }
        }

        private RecordInfo RequestRecord(string id, string subId, byte recordType, byte[] recordData, long minimumLength, long extraBuffer)
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
                result.SubID = subId;
                result.FieldType = recordType;
                result.Database = this;
                FileStream.SetLength(result.Offset + result.Length);

                //add index
                if (_fileStreamIdx != null)
                {
                    _fileStreamIdx.Position = _fileStreamIdx.Length;
                    byte[] buffer = new byte[117];
                    using (MemoryStream ms = new MemoryStream(buffer))
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        result.OffsetIdx = _fileStreamIdx.Position;
                        ms.Position = 0;
                        bw.Write(result.Offset);
                        bw.Write(result.Length);
                        bw.Write(result.FieldType);
                        bw.Write(result.ID);
                        bw.Write(result.SubID);
                        _fileStreamIdx.Write(buffer, 0, 117);
                    }
                }
            }
            else
            {
                //re-use of an empty record
                result.ID = id;
                result.SubID = subId;
                _emptyRecords.Remove(result);
                result.FieldType = recordType;

                //change index
                _fileStreamIdx.Position = result.OffsetIdx;
                byte[] buffer = new byte[117];
                using (MemoryStream ms = new MemoryStream(buffer))
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    result.OffsetIdx = _fileStreamIdx.Position;
                    ms.Position = 0;
                    bw.Write(result.Offset);
                    bw.Write(result.Length);
                    bw.Write(result.FieldType);
                    bw.Write(result.ID);
                    bw.Write(result.SubID);
                    _fileStreamIdx.Write(buffer, 0, 117);
                }

            }
            //write record header data
            FileStream.Position = result.Offset + RECORD_POS_LENGTH;
            BinaryWriter.Write(result.Length);
            FileStream.Position = result.Offset + RECORD_POS_FIELDTYPE;
            FileStream.WriteByte(recordType);
            FileStream.Position = result.Offset + RECORD_POS_ID;
            BinaryWriter.Write(result.ID);
            BinaryWriter.Write(result.SubID);

            //write data
            FileStream.Position = result.Offset + 150;
            FileStream.Write(recordData, 150, (int)minimumLength - 150);

            //Flush();

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
