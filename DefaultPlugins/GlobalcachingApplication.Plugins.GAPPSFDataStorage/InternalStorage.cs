using GlobalcachingApplication.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.GAPPSFDataStorage
{
    public class InternalStorage : Utils.BasePlugin.BaseInternalStorage
    {
        public const string STR_LOADING = "Loading...";
        public const string STR_LOADINGDATA = "Loading data...";
        public const string STR_SAVING = "Saving...";
        public const string STR_SAVINGDATA = "Saving data...";
        public const string STR_SAVINGGEOCACHES = "Saving geocaches...";
        public const string STR_SAVINGLOGS = "Saving logs...";
        public const string STR_SAVINGLOGIMAGES = "Saving log images...";
        public const string STR_SAVINGGEOCACHEIMAGES = "Saving geocache images...";
        public const string STR_SAVINGWAYPOINTS = "Saving waypoints...";
        public const string STR_BACKINGUPDATA = "Creating backup...";
        public const string STR_RESTORINGDATA = "Restoring backup...";

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

        private FileStream _fileStream = null;
        private List<RecordInfo> _emptyRecords = new List<RecordInfo>();
        private bool _emptyRecordsSorted = false;

        private Hashtable _geocachesInDB = new Hashtable();
        private Hashtable _logsInDB = new Hashtable();
        private Hashtable _logimgsInDB = new Hashtable();
        private Hashtable _wptsInDB = new Hashtable();
        private Hashtable _usrwptsInDB = new Hashtable();
        private Hashtable _geocacheimgsInDB = new Hashtable();

        private void closeCurrentFile()
        {
            if (_fileStream!=null)
            {
                _fileStream.Dispose();
                _fileStream = null;
            }
            _emptyRecords.Clear();
            _emptyRecordsSorted = false;
            _geocachesInDB.Clear();
            _logsInDB.Clear();
            _logimgsInDB.Clear();
            _wptsInDB.Clear();
            _usrwptsInDB.Clear();
            _geocacheimgsInDB.Clear();
        }

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LOADING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LOADINGDATA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SAVING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SAVINGDATA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SAVINGGEOCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SAVINGLOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SAVINGLOGIMAGES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SAVINGGEOCACHEIMAGES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SAVINGLOGIMAGES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SAVINGWAYPOINTS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_BACKINGUPDATA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_RESTORINGDATA));

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }
            if (string.IsNullOrEmpty(Properties.Settings.Default.ActiveDataFile))
            {
                Properties.Settings.Default.ActiveDataFile = System.IO.Path.Combine(core.PluginDataPath, "GAPPSFDataStorage.gpp");
                Properties.Settings.Default.Save();
            }

            SetDataSourceName(Properties.Settings.Default.ActiveDataFile);
            core.Logs.LoadFullData += new Framework.EventArguments.LoadFullLogEventHandler(Logs_LoadFullData);
            core.Geocaches.LoadFullData += new Framework.EventArguments.LoadFullGeocacheEventHandler(Geocaches_LoadFullData);

            return base.Initialize(core);
        }

        private void Geocaches_LoadFullData(object sender, Framework.EventArguments.LoadFullGeocacheEventArgs e)
        {
            if (_fileStream!=null)
            {
                RecordInfo ri = _geocachesInDB[e.RequestedForGeocache.Code] as RecordInfo;
                if (ri != null)
                {
                    BinaryReader br = new BinaryReader(_fileStream);
                    //using (BinaryReader br = new BinaryReader(_fileStream))
                    {
                        _fileStream.Position = ri.Offset + 3000;
                        e.LongDescriptionInHtml = br.ReadBoolean();
                        e.ShortDescriptionInHtml = br.ReadBoolean();
                        e.ShortDescription = br.ReadString();
                        e.LongDescription = br.ReadString();
                    }
                }
            }
        }

        private void Logs_LoadFullData(object sender, Framework.EventArguments.LoadFullLogEventArgs e)
        {
            if (_fileStream != null)
            {
                RecordInfo ri = _logsInDB[e.RequestForLog.ID] as RecordInfo;
                if (ri != null)
                {
                    BinaryReader br = new BinaryReader(_fileStream);
                    //using (BinaryReader br = new BinaryReader(_fileStream))
                    {
                        _fileStream.Position = ri.Offset + 170;
                        e.Encoded = br.ReadBoolean();
                        _fileStream.Position = ri.Offset + 320;
                        e.FinderId = br.ReadString();
                        _fileStream.Position = ri.Offset + 350;
                        e.TBCode = br.ReadString();
                        _fileStream.Position = ri.Offset + 380;
                        e.Text = br.ReadString();
                    }
                }
            }
        }

        public override void StartReleaseForCopy()
        {
            if (_fileStream!=null)
            {
                _fileStream.Dispose();
                _fileStream = null;
            }
        }

        public override void EndReleaseForCopy()
        {
            if (File.Exists(Properties.Settings.Default.ActiveDataFile))
            {
                _fileStream = File.Open(Properties.Settings.Default.ActiveDataFile, FileMode.Open, FileAccess.ReadWrite);
            }
        }

        public override Framework.Data.InternalStorageDestination ActiveStorageDestination
        {
            get
            {
                Framework.Data.InternalStorageDestination isd = null;
                if (!string.IsNullOrEmpty(Properties.Settings.Default.ActiveDataFile))
                {
                    isd = new Framework.Data.InternalStorageDestination();
                    isd.Name = Path.GetFileName(Properties.Settings.Default.ActiveDataFile);
                    isd.PluginType = this.GetType().ToString();
                    isd.StorageInfo = new string[] { Properties.Settings.Default.ActiveDataFile };
                }
                return isd;
            }
        }

        protected override bool PrepareSetStorageDestination(Framework.Data.InternalStorageDestination dst)
        {
            bool result = false;
            if (dst != null &&
                dst.PluginType == this.GetType().ToString() &&
                dst.StorageInfo != null &&
                dst.StorageInfo.Length > 0 &&
                File.Exists(dst.StorageInfo[0]))
            {
                Properties.Settings.Default.ActiveDataFile = dst.StorageInfo[0];
                Properties.Settings.Default.Save();
                SetDataSourceName(Properties.Settings.Default.ActiveDataFile);

                Core.Geocaches.Clear();
                Core.Logs.Clear();
                Core.Waypoints.Clear();
                Core.LogImages.Clear();
                Core.UserWaypoints.Clear();
                Core.GeocacheImages.Clear();

                result = true;
            }
            return result;
        }

        protected override bool SupportsLoadingInBackground
        {
            get
            {
                return false;
            }
        }

        public override string FriendlyName
        {
            get { return "Internal Storage"; }
        }

        public override bool PrepareSaveAs()
        {
            bool result = false;
            using (System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog())
            {
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(Properties.Settings.Default.ActiveDataFile);

                dlg.Filter = "*.gsf|*.gsf";
                dlg.FileName = "";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        if (System.IO.File.Exists(dlg.FileName))
                        {
                            System.IO.File.Delete(dlg.FileName);
                        }
                        Properties.Settings.Default.ActiveDataFile = dlg.FileName;
                        Properties.Settings.Default.Save();

                        foreach (Framework.Data.Geocache gc in Core.Geocaches)
                        {
                            gc.Saved = false;
                        }
                        foreach (Framework.Data.Log gc in Core.Logs)
                        {
                            gc.Saved = false;
                        }
                        foreach (Framework.Data.LogImage gc in Core.LogImages)
                        {
                            gc.Saved = false;
                        }
                        foreach (Framework.Data.GeocacheImage gc in Core.GeocacheImages)
                        {
                            gc.Saved = false;
                        }
                        foreach (Framework.Data.Waypoint gc in Core.Waypoints)
                        {
                            gc.Saved = false;
                        }
                        SetDataSourceName(Properties.Settings.Default.ActiveDataFile);
                        result = true;
                    }
                    catch
                    {

                    }
                }
            }
            return result;
        }

        public override bool SaveAs()
        {
            bool result = false;
            FileStream newFileStream = File.Open(Properties.Settings.Default.ActiveDataFile, FileMode.Create, FileAccess.ReadWrite);

            _emptyRecords.Clear();
            _emptyRecordsSorted = false;
            _geocachesInDB.Clear();
            _logsInDB.Clear();
            _logimgsInDB.Clear();
            _wptsInDB.Clear();
            _usrwptsInDB.Clear();
            _geocacheimgsInDB.Clear();

            result = Save(newFileStream, true);
            if (_fileStream != null)
            {
                _fileStream.Dispose();
                _fileStream = null;
            }
            _fileStream = newFileStream;
            return result;
        }

        public override bool PrepareNew()
        {
            bool result = false;
            using (System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog())
            {
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(Properties.Settings.Default.ActiveDataFile);

                dlg.Filter = "*.gsf|*.gsf";
                dlg.FileName = "";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        if (System.IO.File.Exists(dlg.FileName))
                        {
                            System.IO.File.Delete(dlg.FileName);
                        }
                        Properties.Settings.Default.ActiveDataFile = dlg.FileName;
                        Properties.Settings.Default.Save();

                        SetDataSourceName(Properties.Settings.Default.ActiveDataFile);
                        using (FrameworkDataUpdater upd = new FrameworkDataUpdater(Core))
                        {
                            Core.Geocaches.Clear();
                            Core.Logs.Clear();
                            Core.LogImages.Clear();
                            Core.Waypoints.Clear();
                            Core.UserWaypoints.Clear();
                            Core.GeocacheImages.Clear();
                        }

                        FileStream newFileStream = File.Open(Properties.Settings.Default.ActiveDataFile, FileMode.Create, FileAccess.ReadWrite);
                        closeCurrentFile();
                        _fileStream = newFileStream;

                        return true;
                    }
                    catch
                    {

                    }
                }
            }
            return result;
        }

        public override bool NewFile()
        {
            bool result = true;
            Save(_fileStream, true);
            return result;
        }
        public override bool PrepareOpen()
        {
            bool result = false;
            using (System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog())
            {
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(Properties.Settings.Default.ActiveDataFile);

                dlg.Filter = "*.gsf|*.gsf";
                dlg.FileName = "";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Properties.Settings.Default.ActiveDataFile = dlg.FileName;
                    Properties.Settings.Default.Save();
                    SetDataSourceName(Properties.Settings.Default.ActiveDataFile);

                    Core.Geocaches.Clear();
                    Core.Logs.Clear();
                    Core.Waypoints.Clear();
                    Core.LogImages.Clear();
                    Core.UserWaypoints.Clear();
                    Core.GeocacheImages.Clear();

                    result = true;
                }
            }
            return result;
        }

        public override bool Open(bool geocachesOnly)
        {
            bool result = false;
            closeCurrentFile();
            _fileStream = File.Open(Properties.Settings.Default.ActiveDataFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            result = Load(geocachesOnly);
            return result;
        }

        public override bool Load(bool geocachesOnly)
        {
            bool result = true;
            if (File.Exists(Properties.Settings.Default.ActiveDataFile))
            {
                closeCurrentFile();
                _fileStream = File.Open(Properties.Settings.Default.ActiveDataFile, FileMode.Open, FileAccess.ReadWrite);
                if (_fileStream.Length >= DATABASE_CONTENT_OFFSET)
                {
                    BinaryReader fbr = new BinaryReader(_fileStream);
                    byte[] buffer = new byte[10000000];
                    using (MemoryStream ms = new MemoryStream(buffer))
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        long max = _fileStream.Length;
                        DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                        using (Utils.ProgressBlock prog = new Utils.ProgressBlock(this, STR_LOADING, STR_LOADINGDATA, 100, 0))
                        {
                            _fileStream.Position = DATABASE_CONTENT_OFFSET;
                            long eof = _fileStream.Length;
                            while (_fileStream.Position < eof)
                            {
                                RecordInfo ri = new RecordInfo();
                                ri.Offset = _fileStream.Position;
                                _fileStream.Position = ri.Offset + RECORD_POS_LENGTH;
                                ri.Length = fbr.ReadInt64();
                                _fileStream.Position = ri.Offset + RECORD_POS_FIELDTYPE;
                                ri.FieldType = fbr.ReadByte();
                                if (ri.FieldType == RECORD_EMPTY)
                                {
                                    _emptyRecords.Add(ri);
                                    _emptyRecordsSorted = false;
                                }
                                else
                                {
                                    _fileStream.Position = ri.Offset + RECORD_POS_ID;
                                    ri.ID = fbr.ReadString();
                                    ri.SubID = fbr.ReadString();
                                    _fileStream.Position = ri.Offset + 150;
                                    _fileStream.Read(buffer, 150, (int)(ri.Length - 150));
                                    ms.Position = 150;
                                    switch (ri.FieldType)
                                    {
                                        case RECORD_GEOCACHE:
                                            readGeocacheData(ri, ms, br);
                                            break;
                                        case RECORD_LOG:
                                            readLogData(ri, ms, br);
                                            break;
                                        case RECORD_WAYPOINT:
                                            readWaypointData(ri, ms, br);
                                            break;
                                        case RECORD_USERWAYPOINT:
                                            readUserWaypointData(ri, ms, br);
                                            break;
                                        case RECORD_LOGIMAGE:
                                            readLogImageData(ri, ms, br);
                                            break;
                                        case RECORD_GEOCACHEIMAGE:
                                            readGeocacheImageData(ri, ms, br);
                                            break;
                                    }
                                }
                                _fileStream.Position = ri.Offset + ri.Length;

                                if (DateTime.Now >= nextUpdate)
                                {
                                    prog.UpdateProgress(STR_LOADING, STR_LOADINGDATA, 100, (int)(100.0 * (double)_fileStream.Position / (double)max));
                                    nextUpdate = DateTime.Now.AddSeconds(1);
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }


        private void readGeocacheImageData(RecordInfo ri, MemoryStream ms, BinaryReader br)
        {
            _geocacheimgsInDB.Add(ri.ID, ri);
            Framework.Data.GeocacheImage l = new Framework.Data.GeocacheImage();
            l.ID = ri.ID;

            l.DataFromDate = DateTimeFromLong(br.ReadInt64());
            l.GeocacheCode = br.ReadString();
            ms.Position = 220;
            l.Url = br.ReadString();
            ms.Position = 420;
            l.MobileUrl = br.ReadString();
            ms.Position = 520;
            l.ThumbUrl = br.ReadString();
            ms.Position = 620;
            l.Name = br.ReadString();
            ms.Position = 800;
            l.Description = br.ReadString();

            l.Saved = true;
            l.IsDataChanged = false;
            Core.GeocacheImages.Add(l);
        }


        private void readLogImageData(RecordInfo ri, MemoryStream ms, BinaryReader br)
        {
            _logimgsInDB.Add(ri.ID, ri);
            Framework.Data.LogImage l = new Framework.Data.LogImage();
            l.ID = ri.ID;

            l.DataFromDate = DateTimeFromLong(br.ReadInt64());
            ms.Position = 180;
            l.LogID = br.ReadString();
            ms.Position = 220;
            l.Url = br.ReadString();
            ms.Position = 420;
            l.Name = br.ReadString();

            l.Saved = true;
            l.IsDataChanged = false;
            Core.LogImages.Add(l);
        }


        private void readUserWaypointData(RecordInfo ri, MemoryStream ms, BinaryReader br)
        {
            _usrwptsInDB.Add(ri.ID, ri);
            Framework.Data.UserWaypoint wp = new Framework.Data.UserWaypoint();
            wp.ID = int.Parse(ri.ID);
            //wp.GeocacheCode = ri.SubID;

            wp.Date = DateTimeFromLong(br.ReadInt64());
            wp.Lat = br.ReadDouble();
            wp.Lon = br.ReadDouble();
            ms.Position = 200;
            wp.GeocacheCode = br.ReadString();
            ms.Position = 220;
            wp.Description = br.ReadString();

            wp.Saved = true;
            wp.IsDataChanged = false;
            Core.UserWaypoints.Add(wp);
        }

        private void readWaypointData(RecordInfo ri, MemoryStream ms, BinaryReader br)
        {
            _wptsInDB.Add(ri.ID, ri);
            Framework.Data.Waypoint wp = new Framework.Data.Waypoint();
            wp.Code = ri.ID;
            //wp.GeocacheCode = ri.SubID;

            wp.DataFromDate = DateTimeFromLong(br.ReadInt64());
            if (br.ReadBoolean())
            {
                wp.Lat = br.ReadDouble();
            }
            else
            {
                wp.Lat = null;
                br.ReadDouble();
            }
            if (br.ReadBoolean())
            {
                wp.Lon = br.ReadDouble();
            }
            else
            {
                wp.Lon = null;
                br.ReadDouble();
            }
            wp.Time = DateTimeFromLong(br.ReadInt64());
            wp.WPType = Utils.DataAccess.GetWaypointType(Core.WaypointTypes, br.ReadInt32());

            ms.Position = 200;
            wp.GeocacheCode = br.ReadString();
            ms.Position = 240;
            wp.Code = br.ReadString();
            ms.Position = 280;
            wp.Description = br.ReadString();
            ms.Position = 500;
            wp.Name = br.ReadString();
            ms.Position = 600;
            wp.Url = br.ReadString();
            ms.Position = 700;
            wp.UrlName = br.ReadString();
            ms.Position = 800;
            wp.Comment = br.ReadString();

            wp.Saved = true;
            wp.IsDataChanged = false;
            Core.Waypoints.Add(wp);
        }

        private void readLogData(RecordInfo ri, MemoryStream ms, BinaryReader br)
        {
            _logsInDB.Add(ri.ID, ri);
            Framework.Data.Log l = new Framework.Data.Log();
            l.ID = ri.ID;
            //l.GeocacheCode = ri.SubID;

            l.LogType = Utils.DataAccess.GetLogType(Core.LogTypes, br.ReadInt32());
            l.Date = DateTimeFromLong(br.ReadInt64());
            l.DataFromDate = DateTimeFromLong(br.ReadInt64());
            //l.Encoded = br.ReadBoolean();
            ms.Position = 180;
            l.GeocacheCode = br.ReadString();
            ms.Position = 220;
            l.Finder = br.ReadString();
            //ms.Position = 320;
            //l.FinderId = br.ReadString();
            //ms.Position = 350;
            //l.TBCode = br.ReadString();
            //ms.Position = 380;
            //l.Text = br.ReadString();

            l.Saved = true;
            l.IsDataChanged = false;
            Core.Logs.Add(l);
        }

        private void readGeocacheData(RecordInfo ri, MemoryStream ms, BinaryReader br)
        {
            _geocachesInDB.Add(ri.ID, ri);
            Framework.Data.Geocache gc = new Framework.Data.Geocache();
            gc.Code = ri.ID;

            //read data
            gc.Archived = br.ReadBoolean();
            gc.Available = br.ReadBoolean();
            gc.Container = Utils.DataAccess.GetGeocacheContainer(Core.GeocacheContainers, br.ReadInt32());
            gc.DataFromDate = DateTimeFromLong(br.ReadInt64());
            gc.PublishedTime = DateTimeFromLong(br.ReadInt64());
            gc.Difficulty = br.ReadDouble();
            gc.Terrain = br.ReadDouble();
            gc.Favorites = br.ReadInt32();
            gc.Flagged = br.ReadBoolean();
            gc.Found = br.ReadBoolean();
            gc.GeocacheType = Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, br.ReadInt32());
            gc.Lat = br.ReadDouble();
            gc.Lon = br.ReadDouble();
            gc.Locked = br.ReadBoolean();
            if (br.ReadBoolean())
            {
                gc.CustomLat = br.ReadDouble();
                gc.CustomLon = br.ReadDouble();
            }
            else
            {
                gc.CustomLat = null;
                gc.CustomLon = null;
                br.ReadDouble();
                br.ReadDouble();
            }
            gc.MemberOnly = br.ReadBoolean();
            List<int> attrList = new List<int>();
            int cnt = br.ReadByte();
            for (int i = 0; i < cnt; i++)
            {
                attrList.Add(br.ReadInt32());
            }
            gc.AttributeIds = attrList;

            ms.Position = 300;
            gc.City = br.ReadString();
            ms.Position = 400;
            gc.Country = br.ReadString();
            ms.Position = 500;
            gc.EncodedHints = br.ReadString();
            ms.Position = 1000;
            gc.Municipality = br.ReadString();
            ms.Position = 1100;
            gc.Name = br.ReadString();
            ms.Position = 1200; //spare now
            gc.Notes = br.ReadString();
            ms.Position = 2000;
            gc.Owner = br.ReadString();
            ms.Position = 2100;
            gc.OwnerId = br.ReadString();
            ms.Position = 2150;
            gc.PersonaleNote = br.ReadString();
            ms.Position = 2400;
            gc.PlacedBy = br.ReadString();
            ms.Position = 2500;
            gc.State = br.ReadString();
            ms.Position = 2600;
            gc.Url = br.ReadString();
            //ms.Position = 3000;
            //gc.LongDescriptionInHtml = br.ReadBoolean();
            //gc.ShortDescriptionInHtml = br.ReadBoolean();
            //gc.ShortDescription = br.ReadString();
            //gc.LongDescription = br.ReadString();

            Calculus.SetDistanceAndAngleGeocacheFromLocation(gc, Core.CenterLocation);
            gc.Saved = true;
            gc.IsDataChanged = false;
            Core.Geocaches.Add(gc);
        }


        public override bool Save()
        {
            bool result = true;
            result = Save(_fileStream, false);
            return result;
        }

        public bool Save(FileStream fileStream, bool forceFullData)
        {
            bool result = true;
            byte isFree = RECORD_EMPTY;
            //todo
            //note: delete index file or create one? for now just delete
            try
            {
                byte[] memBuffer = new byte[10 * 1024 * 1024];
                using (Utils.ProgressBlock fixpr = new Utils.ProgressBlock(this, STR_SAVING, STR_SAVINGDATA, 1, 0))
                using (MemoryStream ms = new MemoryStream(memBuffer))
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    string fn = string.Concat(fileStream.Name, ".gsx");
                    if (File.Exists(fn))
                    {
                        File.Delete(fn);
                    }

                    //**********************************************
                    //          GEOCACHES
                    //**********************************************

                    //delete geocaches that are not in the list anymore.
                    List<RecordInfo> deletedRecords = (from RecordInfo ri in _geocachesInDB.Values where Core.Geocaches.GetGeocache(ri.ID) == null select ri).ToList();
                    foreach (RecordInfo ri in deletedRecords)
                    {
                        //scratch file to mark it as free
                        fileStream.Position = ri.Offset + RECORD_POS_FIELDTYPE;
                        fileStream.WriteByte(isFree);

                        //mark current record as free
                        _emptyRecords.Add(ri);
                        _emptyRecordsSorted = false;
                        _geocachesInDB.Remove(ri.ID);
                    }

                    List<Framework.Data.Geocache> gclist = (from Framework.Data.Geocache wp in Core.Geocaches
                                                            where !wp.Saved
                                                            select wp).ToList();
                    if (gclist.Count > 0)
                    {
                        using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_SAVING, STR_SAVINGGEOCACHES, gclist.Count, 0))
                        {
                        }
                    }

                }
            }
            catch
            {

            }
            return result;
        }

        public static long DateTimeToLong(DateTime dt)
        {
            if (dt == DateTime.MinValue)
            {
                return 0;
            }
            else
            {
                try
                {
                    return dt.ToFileTime();
                }
                catch
                {
                    return 0;
                }
            }
        }

        public static DateTime DateTimeFromLong(long l)
        {
            if (l == 0)
            {
                return DateTime.MinValue;
            }
            else
            {
                try
                {
                    return DateTime.FromFileTime(l);
                }
                catch
                {
                    return DateTime.MinValue;
                }
            }
        }

    }
}
