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
            throw new NotImplementedException();
        }

        private void Logs_LoadFullData(object sender, Framework.EventArguments.LoadFullLogEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override void StartReleaseForCopy()
        {
            throw new NotImplementedException();
        }

        public override void EndReleaseForCopy()
        {
            throw new NotImplementedException();
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
            //todo
            return result;
        }

        public override bool SaveAs()
        {
            bool result = false;
            //todo
            return result;
        }

        public override bool PrepareNew()
        {
            bool result = false;
            //todo
            return result;
        }

        public override bool NewFile()
        {
            bool result = true;
            //todo
            return result;
        }
        public override bool PrepareOpen()
        {
            bool result = false;
            //todo
            return result;
        }

        public override bool Open(bool geocachesOnly)
        {
            bool result = false;
            Load(geocachesOnly);
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
                                ri.Length = br.ReadInt64();
                                _fileStream.Position = ri.Offset + RECORD_POS_FIELDTYPE;
                                ri.FieldType = br.ReadByte();
                                if (ri.FieldType == RECORD_EMPTY)
                                {
                                    _emptyRecords.Add(ri);
                                    _emptyRecordsSorted = false;
                                }
                                else
                                {
                                    _fileStream.Position = ri.Offset + RECORD_POS_ID;
                                    ri.ID = br.ReadString();
                                    ri.SubID = br.ReadString();
                                    _fileStream.Position = ri.Offset + 150;
                                    _fileStream.Read(buffer, 150, (int)(ri.Length - 150));
                                    //todo
                                    switch (ri.FieldType)
                                    {
                                        case RECORD_GEOCACHE:
                                            break;
                                        case RECORD_LOG:
                                            break;
                                        case RECORD_WAYPOINT:
                                            break;
                                        case RECORD_USERWAYPOINT:
                                            break;
                                        case RECORD_LOGIMAGE:
                                            break;
                                        case RECORD_GEOCACHEIMAGE:
                                            break;
                                    }
                                }

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


        public override bool Save()
        {
            bool result = true;
            //todo
            //note: delete index file or create one? for now just delete
            try
            {

            }
            catch
            {

            }
            return result;
        }


    }
}
