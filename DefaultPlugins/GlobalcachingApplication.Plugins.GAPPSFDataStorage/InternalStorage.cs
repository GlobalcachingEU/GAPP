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

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_MAXCOUNT));

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }
            if (string.IsNullOrEmpty(Properties.Settings.Default.ActiveDataFile))
            {
                Properties.Settings.Default.ActiveDataFile = System.IO.Path.Combine(core.PluginDataPath, "GAPPSFDataStorage.gsf");
                Properties.Settings.Default.Save();
            }

            SetDataSourceName(Properties.Settings.Default.ActiveDataFile);
            core.Logs.LoadFullData += new Framework.EventArguments.LoadFullLogEventHandler(Logs_LoadFullData);
            core.Geocaches.LoadFullData += new Framework.EventArguments.LoadFullGeocacheEventHandler(Geocaches_LoadFullData);

            return base.Initialize(core);
        }

        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            foreach (System.Windows.Forms.UserControl uc in configPanels)
            {
                if (uc is SettingsPanel)
                {
                    (uc as SettingsPanel).Apply();
                    break;
                }
            }
            return true;
        }

        public override List<System.Windows.Forms.UserControl> CreateConfigurationPanels()
        {
            List<System.Windows.Forms.UserControl> pnls = base.CreateConfigurationPanels();
            if (pnls == null) pnls = new List<System.Windows.Forms.UserControl>();
            pnls.Add(new SettingsPanel());
            return pnls;
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
                if (_fileStream.Length==0)
                {
                    BinaryWriter fbw = new BinaryWriter(_fileStream);
                    //create meta data
                    _fileStream.SetLength(DATABASE_CONTENT_OFFSET);
                    _fileStream.Position = DATABASE_META_VERSION_POS;
                    long v = 1;
                    fbw.Write(v);
                    _fileStream.Position = DATABASE_META_ACTIVEGEOCACHE_POS;
                    fbw.Write("");
                }
                else
                {
                    //read meta data
                    BinaryReader fbw = new BinaryReader(_fileStream);
                    _fileStream.Position = DATABASE_META_VERSION_POS;
                    long v;
                    v= fbw.ReadInt64();
                    if (v!=1)
                    {
                        _fileStream.Dispose();
                        _fileStream = null;
                        throw new Exception("Not supported file version");
                    }
                }
                if (_fileStream.Length > DATABASE_CONTENT_OFFSET)
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

        private void writeGeocacheData(Framework.Data.Geocache gc, MemoryStream ms, BinaryWriter bw)
        {
            //ms.Position = 0;

            ms.Position = 150;
            bw.Write(gc.Archived); //150
            bw.Write(gc.Available); //151
            bw.Write(gc.Container == null ? 0 : gc.Container.ID); //152
            bw.Write(DateTimeToLong(gc.DataFromDate)); //156
            bw.Write(DateTimeToLong(gc.PublishedTime)); //164
            bw.Write(gc.Difficulty); //172
            bw.Write(gc.Terrain); //180
            bw.Write(gc.Favorites); //188
            bw.Write(gc.Flagged); //192
            bw.Write(gc.Found); //193
            bw.Write(gc.GeocacheType == null ? 0 : gc.GeocacheType.ID); //194
            bw.Write(gc.Lat); //198
            bw.Write(gc.Lon); //206
            bw.Write(gc.Locked); //214
            bw.Write(gc.CustomLat != null); //215
            if (gc.CustomLat != null)
            {
                bw.Write((double)gc.CustomLat); //216
            }
            else
            {
                bw.Write((double)0.0);
            }
            bw.Write(gc.CustomLon != null); //224
            if (gc.CustomLon != null)
            {
                bw.Write((double)gc.CustomLon); //225
            }
            else
            {
                bw.Write((double)0.0);
            }
            bw.Write(gc.MemberOnly);

            byte attrCount = (byte)(gc.AttributeIds == null ? 0 : gc.AttributeIds.Count);
            bw.Write(attrCount); //234
            if (attrCount > 0)
            {
                foreach (int i in gc.AttributeIds)
                {
                    bw.Write(i);
                }
            }
            ms.Position = 300;
            bw.Write(GetSafeString(300, 400, gc.City) ?? "");
            ms.Position = 400;
            bw.Write(GetSafeString(400, 500, gc.Country) ?? "");
            ms.Position = 500;
            bw.Write(GetSafeString(500, 1000, gc.EncodedHints) ?? "");
            ms.Position = 1000;
            bw.Write(GetSafeString(1000, 1100, gc.Municipality) ?? "");
            ms.Position = 1100;
            bw.Write(GetSafeString(1100, 1200, gc.Name) ?? "");
            ms.Position = 1200; //spare now
            string sOrg = gc.Notes ?? "";
            string s = GetSafeString(1200, 2000, gc.Notes) ?? "";
            if (s!=sOrg)
            {
                s = Utils.Conversion.StripHtmlTags(s);
                s = GetSafeString(1200, 1990, s) ?? "";
                s = string.Concat("<P>", s, "</P>");
            }
            bw.Write(s);
            ms.Position = 2000;
            bw.Write(GetSafeString(2000, 2100, gc.Owner) ?? "");
            ms.Position = 2100;
            bw.Write(GetSafeString(2100, 2150, gc.OwnerId) ?? "");
            ms.Position = 2150;
            bw.Write(GetSafeString(2150, 2400, gc.PersonaleNote) ?? "");
            ms.Position = 2400;
            bw.Write(GetSafeString(2400, 2500, gc.PlacedBy) ?? "");
            ms.Position = 2500;
            bw.Write(GetSafeString(2500, 2600, gc.State) ?? "");
            ms.Position = 2600;
            bw.Write(GetSafeString(2600, 2800, gc.Url) ?? "");
        }

        private void writeLogData(Framework.Data.Log data, MemoryStream ms, BinaryWriter bw)
        {
            //ms.Position = 0;

            ms.Position = 150;
            bw.Write(data.LogType.ID); //150
            bw.Write(DateTimeToLong(data.Date)); //154
            bw.Write(DateTimeToLong(data.DataFromDate)); //162
            bw.Write(data.Encoded); //170
            ms.Position = 180;
            bw.Write(data.GeocacheCode ?? "");
            ms.Position = 220;
            bw.Write(data.Finder ?? "");
            //ms.Position = 320;
            //bw.Write(data.FinderId ?? "");
            //ms.Position = 350;
            //bw.Write(data.TBCode ?? "");
            //ms.Position = 380;
            //bw.Write(data.Text ?? "");
        }


        private void writeWaypointData(Framework.Data.Waypoint data, MemoryStream ms, BinaryWriter bw)
        {
            ms.Position = 150;
            bw.Write(DateTimeToLong(data.DataFromDate)); //150
            bw.Write((bool)(data.Lat != null)); //158
            bw.Write(data.Lat == null ? (double)0.0 : (double)data.Lat); //159
            bw.Write((bool)(data.Lon != null)); //167
            bw.Write(data.Lon == null ? (double)0.0 : (double)data.Lon); //168
            bw.Write(DateTimeToLong(data.Time)); //176
            bw.Write(data.WPType.ID); //184
            //spare
            ms.Position = 200;
            bw.Write(data.GeocacheCode);
            ms.Position = 240;
            bw.Write(data.Code ?? "");
            ms.Position = 280;
            bw.Write(GetSafeString(280, 500, data.Description) ?? "");
            ms.Position = 500;
            bw.Write(GetSafeString(500, 600, data.Name) ?? "");
            ms.Position = 600;
            bw.Write(data.Url ?? "");
            ms.Position = 700;
            bw.Write(data.UrlName ?? "");
            ms.Position = 800;
            bw.Write(data.Comment ?? "");
        }


        private void writeLogImageData(Framework.Data.LogImage data, MemoryStream ms, BinaryWriter bw)
        {
            ms.Position = 150;
            bw.Write(DateTimeToLong(data.DataFromDate)); //150
            ms.Position = 180;
            bw.Write(data.LogID ?? "");
            ms.Position = 220;
            bw.Write(data.Url ?? "");
            ms.Position = 420;
            bw.Write(data.Name ?? "");
        }


        private void writeGeocacheImageData(Framework.Data.GeocacheImage data, MemoryStream ms, BinaryWriter bw)
        {
            ms.Position = 150;
            bw.Write(DateTimeToLong(data.DataFromDate)); //150
            ms.Position = 180;
            bw.Write(data.GeocacheCode ?? "");
            ms.Position = 220;
            bw.Write(data.Url);
            ms.Position = 420;
            bw.Write(data.MobileUrl ?? "");
            ms.Position = 520;
            bw.Write(data.ThumbUrl ?? "");
            ms.Position = 620;
            bw.Write(GetSafeString(620, 800, data.Name) ?? "");
            ms.Position = 800;
            bw.Write(data.Description ?? "");
        }

        private void writeUserWaypointData(Framework.Data.UserWaypoint data, MemoryStream ms, BinaryWriter bw)
        {
            ms.Position = 150;
            bw.Write(DateTimeToLong(data.Date)); //150
            bw.Write(data.Lat); //158
            bw.Write(data.Lon); //166
            ms.Position = 200;
            bw.Write(data.GeocacheCode);
            ms.Position = 220;
            bw.Write(data.Description);
        }

        public override bool Save()
        {
            bool result = true;
            result = Save(_fileStream, false);
            return result;
        }

        public bool Save(FileStream fileStream, bool forceFullData)
        {
            if (fileStream == null) return false;

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

                    //write header
                    if (fileStream.Length == 0)
                    {
                        BinaryWriter fbw = new BinaryWriter(fileStream);
                        //create meta data
                        fileStream.SetLength(DATABASE_CONTENT_OFFSET);
                        fileStream.Position = DATABASE_META_VERSION_POS;
                        long v = 1;
                        fbw.Write(v);
                        fileStream.Position = DATABASE_META_ACTIVEGEOCACHE_POS;
                        fbw.Write("");
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
                            int index = 0;
                            int procStep = 0;
                            foreach (Framework.Data.Geocache gc in gclist)
                            {
                                writeGeocacheData(gc, ms, bw);

                                RecordInfo ri = _geocachesInDB[gc.Code] as RecordInfo;
                                if (forceFullData || gc.FullDataLoaded || ri == null)
                                {
                                    ms.Position = 3000;
                                    bw.Write(gc.LongDescriptionInHtml); //3000
                                    bw.Write(gc.ShortDescriptionInHtml); //3001
                                    bw.Write(gc.ShortDescription ?? ""); //3002
                                    bw.Write(gc.LongDescription ?? "");

                                    //check length
                                    if (ri == null || ri.Length < ms.Position)
                                    {
                                        if (ri != null)
                                        {
                                            //scratch file to mark it as free
                                            fileStream.Position = ri.Offset + RECORD_POS_FIELDTYPE;
                                            fileStream.WriteByte(isFree);

                                            //mark current record as free
                                            _emptyRecords.Add(ri);
                                            _emptyRecordsSorted = false;
                                            _geocachesInDB.Remove(ri.ID);
                                        }
                                        ri = RequestGeocacheRecord(fileStream, gc.Code, "", memBuffer, ms.Position, 500);
                                        _geocachesInDB.Add(gc.Code, ri);
                                    }
                                    else
                                    {
                                        //still fits
                                        fileStream.Position = ri.Offset + 150;
                                        fileStream.Write(memBuffer, 150, (int)ms.Position - 150);
                                    }
                                }
                                else
                                {
                                    //always fits
                                    //skip first 150
                                    fileStream.Position = ri.Offset + 150;
                                    fileStream.Write(memBuffer, 150, (int)ms.Position - 150);
                                }

                                gc.Saved = true;

                                index++;
                                procStep++;
                                if (procStep >= 1000)
                                {
                                    progress.UpdateProgress(STR_SAVING, STR_SAVINGGEOCACHES, gclist.Count, index);
                                    procStep = 0;
                                }
                            }
                        }
                    }

                    //**********************************************
                    //          LOGS
                    //**********************************************

                    //delete items that are not in the list anymore.
                    deletedRecords = (from RecordInfo ri in _logsInDB.Values where Core.Logs.GetLog(ri.ID) == null select ri).ToList();
                    foreach (RecordInfo ri in deletedRecords)
                    {
                        //scratch file to mark it as free
                        fileStream.Position = ri.Offset + RECORD_POS_FIELDTYPE;
                        fileStream.WriteByte(isFree);

                        //mark current record as free
                        _emptyRecords.Add(ri);
                        _emptyRecordsSorted = false;
                        _logsInDB.Remove(ri.ID);
                    }

                    List<Framework.Data.Log> lglist = (from Framework.Data.Log wp in Core.Logs
                                                       where !wp.Saved
                                                       select wp).ToList();
                    if (lglist.Count > 0)
                    {
                        using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_SAVING, STR_SAVINGLOGS, lglist.Count, 0))
                        {
                            int index = 0;
                            int procStep = 0;
                            foreach (Framework.Data.Log l in lglist)
                            {
                                writeLogData(l, ms, bw);

                                RecordInfo ri = _logsInDB[l.ID] as RecordInfo;
                                if (forceFullData || l.FullDataLoaded || ri == null)
                                {
                                    ms.Position = 320;
                                    bw.Write(l.FinderId ?? "");
                                    ms.Position = 350;
                                    bw.Write(l.TBCode ?? "");
                                    ms.Position = 380;
                                    bw.Write(l.Text ?? "");

                                    //check length
                                    if (ri == null || ri.Length < ms.Position)
                                    {
                                        if (ri != null)
                                        {
                                            //scratch file to mark it as free
                                            fileStream.Position = ri.Offset + RECORD_POS_FIELDTYPE;
                                            fileStream.WriteByte(isFree);

                                            //mark current record as free
                                            _emptyRecords.Add(ri);
                                            _emptyRecordsSorted = false;
                                            _logsInDB.Remove(ri.ID);
                                        }
                                        ri = RequestLogRecord(fileStream, l.ID, l.GeocacheCode, memBuffer, ms.Position, 100);
                                        _logsInDB.Add(l.ID, ri);
                                    }
                                    else
                                    {
                                        //still fits
                                        fileStream.Position = ri.Offset + 150;
                                        fileStream.Write(memBuffer, 150, (int)ms.Position - 150);
                                    }
                                }
                                else
                                {
                                    //always fits
                                    //skip first 150
                                    fileStream.Position = ri.Offset + 150;
                                    fileStream.Write(memBuffer, 150, (int)ms.Position - 150);
                                }

                                l.Saved = true;

                                index++;
                                procStep++;
                                if (procStep >= 1000)
                                {
                                    progress.UpdateProgress(STR_SAVING, STR_SAVINGLOGS, lglist.Count, index);
                                    procStep = 0;
                                }
                            }
                        }
                    }

                    //**********************************************
                    //          WAYPOINTS
                    //**********************************************

                    //delete items that are not in the list anymore.
                    deletedRecords = (from RecordInfo ri in _wptsInDB.Values where Core.Waypoints.getWaypoint(ri.ID) == null select ri).ToList();
                    foreach (RecordInfo ri in deletedRecords)
                    {
                        //scratch file to mark it as free
                        fileStream.Position = ri.Offset + RECORD_POS_FIELDTYPE;
                        fileStream.WriteByte(isFree);

                        //mark current record as free
                        _emptyRecords.Add(ri);
                        _emptyRecordsSorted = false;
                        _wptsInDB.Remove(ri.ID);
                    }
                    List<Framework.Data.Waypoint> wptlist = (from Framework.Data.Waypoint wp in Core.Waypoints
                                                             where !wp.Saved
                                                             select wp).ToList();
                    if (wptlist.Count > 0)
                    {
                        int index = 0;
                        int procStep = 0;
                        using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_SAVING, STR_SAVINGWAYPOINTS, wptlist.Count, 0))
                        {
                            foreach (Framework.Data.Waypoint wp in wptlist)
                            {
                                writeWaypointData(wp, ms, bw);

                                RecordInfo ri = _wptsInDB[wp.Code] as RecordInfo;
                                //check length
                                if (ri == null || ri.Length < ms.Position)
                                {
                                    if (ri != null)
                                    {
                                        //scratch file to mark it as free
                                        fileStream.Position = ri.Offset + RECORD_POS_FIELDTYPE;
                                        fileStream.WriteByte(isFree);

                                        //mark current record as free
                                        _emptyRecords.Add(ri);
                                        _emptyRecordsSorted = false;
                                        _wptsInDB.Remove(ri.ID);
                                    }
                                    ri = RequestWaypointRecord(fileStream, wp.Code, wp.GeocacheCode, memBuffer, ms.Position, 10);
                                    _wptsInDB.Add(wp.Code, ri);
                                }
                                else
                                {
                                    //still fits
                                    fileStream.Position = ri.Offset + 150;
                                    fileStream.Write(memBuffer, 150, (int)ms.Position - 150);
                                }

                                wp.Saved = true;

                                index++;
                                procStep++;
                                if (procStep >= 1000)
                                {
                                    progress.UpdateProgress(STR_SAVING, STR_SAVINGLOGS, wptlist.Count, index);
                                    procStep = 0;
                                }
                            }
                        }
                    }

                    //**********************************************
                    //          LOGIMAGES
                    //**********************************************
                    //delete items that are not in the list anymore.
                    deletedRecords = (from RecordInfo ri in _logimgsInDB.Values where Core.LogImages.GetLogImage(ri.ID) == null select ri).ToList();
                    foreach (RecordInfo ri in deletedRecords)
                    {
                        //scratch file to mark it as free
                        fileStream.Position = ri.Offset + RECORD_POS_FIELDTYPE;
                        fileStream.WriteByte(isFree);

                        //mark current record as free
                        _emptyRecords.Add(ri);
                        _emptyRecordsSorted = false;
                        _logimgsInDB.Add(ri.ID, ri);
                    }

                    List<Framework.Data.LogImage> lgimglist = (from Framework.Data.LogImage wp in Core.LogImages
                                                               where !wp.Saved
                                                               select wp).ToList();
                    if (lgimglist.Count > 0)
                    {
                        int index = 0;
                        int procStep = 0;
                        using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_SAVING, STR_SAVINGLOGIMAGES, lgimglist.Count, 0))
                        {
                            foreach (Framework.Data.LogImage li in lgimglist)
                            {
                                writeLogImageData(li, ms, bw);

                                RecordInfo ri = _logimgsInDB[li.ID] as RecordInfo;
                                //check length
                                if (ri == null || ri.Length < ms.Position)
                                {
                                    if (ri != null)
                                    {
                                        //scratch file to mark it as free
                                        fileStream.Position = ri.Offset + RECORD_POS_FIELDTYPE;
                                        fileStream.WriteByte(isFree);

                                        //mark current record as free
                                        _emptyRecords.Add(ri);
                                        _emptyRecordsSorted = false;
                                        _logimgsInDB.Remove(ri.ID);
                                    }
                                    ri = RequestLogImageRecord(fileStream, li.ID, li.LogID, memBuffer, ms.Position, 10);
                                    _logimgsInDB.Add(li.ID, ri);
                                }
                                else
                                {
                                    //still fits
                                    fileStream.Position = ri.Offset + 150;
                                    fileStream.Write(memBuffer, 150, (int)ms.Position - 150);
                                }

                                li.Saved = true;

                                index++;
                                procStep++;
                                if (procStep >= 1000)
                                {
                                    progress.UpdateProgress(STR_SAVING, STR_SAVINGLOGIMAGES, lgimglist.Count, index);
                                    procStep = 0;
                                }
                            }
                        }
                    }

                    //**********************************************
                    //          GEOCACHEIMAGES
                    //**********************************************
                    //delete items that are not in the list anymore.
                    deletedRecords = (from RecordInfo ri in _geocacheimgsInDB.Values where Core.GeocacheImages.GetGeocacheImage(ri.ID) == null select ri).ToList();
                    foreach (RecordInfo ri in deletedRecords)
                    {
                        //scratch file to mark it as free
                        fileStream.Position = ri.Offset + RECORD_POS_FIELDTYPE;
                        fileStream.WriteByte(isFree);

                        //mark current record as free
                        _emptyRecords.Add(ri);
                        _emptyRecordsSorted = false;
                        _geocacheimgsInDB.Add(ri.ID, ri);
                    }

                    List<Framework.Data.GeocacheImage> gcimglist = (from Framework.Data.GeocacheImage wp in Core.GeocacheImages
                                                                    where !wp.Saved
                                                                    select wp).ToList();
                    if (gcimglist.Count > 0)
                    {
                        int index = 0;
                        int procStep = 0;
                        using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_SAVING, STR_SAVINGGEOCACHEIMAGES, gcimglist.Count, 0))
                        {
                            foreach (Framework.Data.GeocacheImage li in gcimglist)
                            {
                                writeGeocacheImageData(li, ms, bw);

                                RecordInfo ri = _geocacheimgsInDB[li.ID] as RecordInfo;
                                //check length
                                if (ri == null || ri.Length < ms.Position)
                                {
                                    if (ri != null)
                                    {
                                        //scratch file to mark it as free
                                        fileStream.Position = ri.Offset + RECORD_POS_FIELDTYPE;
                                        fileStream.WriteByte(isFree);

                                        //mark current record as free
                                        _emptyRecords.Add(ri);
                                        _emptyRecordsSorted = false;
                                        _geocacheimgsInDB.Remove(ri.ID);
                                    }
                                    ri = RequestGeocacheImageRecord(fileStream, li.ID, li.GeocacheCode, memBuffer, ms.Position, 10);
                                    _geocacheimgsInDB.Add(li.ID, ri);
                                }
                                else
                                {
                                    //still fits
                                    fileStream.Position = ri.Offset + 150;
                                    fileStream.Write(memBuffer, 150, (int)ms.Position - 150);
                                }

                                li.Saved = true;

                                index++;
                                procStep++;
                                if (procStep >= 1000)
                                {
                                    progress.UpdateProgress(STR_SAVING, STR_SAVINGGEOCACHEIMAGES, gcimglist.Count, index);
                                    procStep = 0;
                                }
                            }
                        }
                    }

                    //**********************************************
                    //          USER WAYPOINTS
                    //**********************************************
                    //delete items that are not in the list anymore.
                    deletedRecords = (from RecordInfo ri in _usrwptsInDB.Values where Core.UserWaypoints.getWaypoint(int.Parse(ri.ID)) == null select ri).ToList();
                    foreach (RecordInfo ri in deletedRecords)
                    {
                        //scratch file to mark it as free
                        fileStream.Position = ri.Offset + RECORD_POS_FIELDTYPE;
                        fileStream.WriteByte(isFree);

                        //mark current record as free
                        _emptyRecords.Add(ri);
                        _emptyRecordsSorted = false;
                        _usrwptsInDB.Add(ri.ID, ri);
                    }

                    List<Framework.Data.UserWaypoint> usrwptlist = (from Framework.Data.UserWaypoint wp in Core.UserWaypoints
                                                                    where !wp.Saved
                                                                    select wp).ToList();
                    if (usrwptlist.Count > 0)
                    {
                        foreach (Framework.Data.UserWaypoint wp in usrwptlist)
                        {
                            writeUserWaypointData(wp, ms, bw);

                            RecordInfo ri = _usrwptsInDB[wp.ID] as RecordInfo;
                            //check length
                            if (ri == null || ri.Length < ms.Position)
                            {
                                if (ri != null)
                                {
                                    //scratch file to mark it as free
                                    fileStream.Position = ri.Offset + RECORD_POS_FIELDTYPE;
                                    fileStream.WriteByte(isFree);

                                    //mark current record as free
                                    _emptyRecords.Add(ri);
                                    _emptyRecordsSorted = false;
                                    _usrwptsInDB.Remove(ri.ID);
                                }
                                ri = RequestUserWaypointRecord(fileStream, wp.ID.ToString(), wp.GeocacheCode, memBuffer, ms.Position, 10);
                                _usrwptsInDB.Add(wp.ID, ri);
                            }
                            else
                            {
                                //still fits
                                fileStream.Position = ri.Offset + 150;
                                fileStream.Write(memBuffer, 150, (int)ms.Position - 150);
                            }

                            wp.Saved = true;
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


        public RecordInfo RequestGeocacheRecord(FileStream fileStream, string id, string subId, byte[] recordData, long minimumLength, long extraBuffer)
        {
            return RequestRecord(fileStream, id, subId ?? "", RECORD_GEOCACHE, recordData, minimumLength, extraBuffer);
        }

        public RecordInfo RequestLogRecord(FileStream fileStream, string id, string subId, byte[] recordData, long minimumLength, long extraBuffer)
        {
            return RequestRecord(fileStream, id, subId ?? "", RECORD_LOG, recordData, minimumLength, extraBuffer);
        }

        public RecordInfo RequestWaypointRecord(FileStream fileStream, string id, string subId, byte[] recordData, long minimumLength, long extraBuffer)
        {
            return RequestRecord(fileStream, id, subId ?? "", RECORD_WAYPOINT, recordData, minimumLength, extraBuffer);
        }

        public RecordInfo RequestUserWaypointRecord(FileStream fileStream, string id, string subId, byte[] recordData, long minimumLength, long extraBuffer)
        {
            return RequestRecord(fileStream, id, subId ?? "", RECORD_USERWAYPOINT, recordData, minimumLength, extraBuffer);
        }
        public RecordInfo RequestLogImageRecord(FileStream fileStream, string id, string subId, byte[] recordData, long minimumLength, long extraBuffer)
        {
            return RequestRecord(fileStream, id, subId ?? "", RECORD_LOGIMAGE, recordData, minimumLength, extraBuffer);
        }
        public RecordInfo RequestGeocacheImageRecord(FileStream fileStream, string id, string subId, byte[] recordData, long minimumLength, long extraBuffer)
        {
            return RequestRecord(fileStream, id, subId ?? "", RECORD_GEOCACHEIMAGE, recordData, minimumLength, extraBuffer);
        }

        private RecordInfo RequestRecord(FileStream fileStream, string id, string subId, byte recordType, byte[] recordData, long minimumLength, long extraBuffer)
        {
            RecordInfo result = null;
            if (!_emptyRecordsSorted)
            {
                _emptyRecords.Sort(delegate(RecordInfo x, RecordInfo y)
                {
                    return x.Length.CompareTo(y.Length);
                });
                _emptyRecordsSorted = true;
            }
            result = (from a in _emptyRecords where a.Length >= minimumLength select a).FirstOrDefault();

            if (result == null)
            {
                result = new RecordInfo();
                result.Length = minimumLength + extraBuffer;
                result.Offset = _fileStream.Length;
                result.ID = id;
                result.SubID = subId;
                result.FieldType = recordType;
                fileStream.SetLength(result.Offset + result.Length);
            }
            else
            {
                //re-use of an empty record
                result.ID = id;
                result.SubID = subId;
                _emptyRecords.Remove(result);
                result.FieldType = recordType;
            }
            //write record header data
            BinaryWriter bw = new BinaryWriter(_fileStream);
            fileStream.Position = result.Offset + RECORD_POS_LENGTH;
            bw.Write(result.Length);
            fileStream.Position = result.Offset + RECORD_POS_FIELDTYPE;
            fileStream.WriteByte(recordType);
            fileStream.Position = result.Offset + RECORD_POS_ID;
            bw.Write(result.ID);
            bw.Write(result.SubID);

            //write data
            fileStream.Position = result.Offset + 150;
            fileStream.Write(recordData, 150, (int)minimumLength - 150);

            //Flush();

            return result;
        }

        protected string GetSafeString(long pos, long nextpos, string value)
        {
            string result = value;
            if (!string.IsNullOrEmpty(value))
            {
                while (!checkStringFits(result, pos, nextpos))
                {
                    result = result.Substring(0, result.Length - 1);
                }
            }
            return result;
        }

        private static byte[] _buffer = new byte[10000000];
        private static byte[] _checkbuffer = new byte[5000000];
        private static MemoryStream _ms = new MemoryStream(_checkbuffer);
        private static BinaryWriter _bw = new BinaryWriter(_ms);
        protected bool checkStringFits(string s, long startPos, long nextPos)
        {
            bool result;
            _ms.Position = 0;
            _bw.Write(s);
            result = (_ms.Position <= (nextPos - startPos));
            return result;
        }


        protected override bool SupportsBackupRestoreDatabase
        {
            get
            {
                return true;
            }
        }

        public override bool PrepareBackup()
        {
            return (!string.IsNullOrEmpty(Properties.Settings.Default.ActiveDataFile) && _fileStream!=null);
        }

        public override bool Backup()
        {
            bool result = false;
            try
            {
                //file.bak01, file.bak02... file.bakNN where NN is the latest
                string fn = string.Format("{0}.bak{1}", Properties.Settings.Default.ActiveDataFile, Properties.Settings.Default.BackupKeepMaxCount.ToString("00"));
                if (File.Exists(fn))
                {
                    //ok, maximum reached
                    //delete the oldest and rename the others
                    fn = string.Format("{0}.bak{1}", Properties.Settings.Default.ActiveDataFile, 1.ToString("00"));
                    if (File.Exists(fn))
                    {
                        File.Delete(fn);
                    }
                    for (int i = 1; i < Properties.Settings.Default.BackupKeepMaxCount; i++)
                    {
                        string fns = string.Format("{0}.bak{1}", Properties.Settings.Default.ActiveDataFile, (i + 1).ToString("00"));
                        string fnd = string.Format("{0}.bak{1}", Properties.Settings.Default.ActiveDataFile, i.ToString("00"));
                        if (File.Exists(fns))
                        {
                            File.Move(fns, fnd);
                        }
                    }
                    fn = string.Format("{0}.bak{1}", Properties.Settings.Default.ActiveDataFile, Properties.Settings.Default.BackupKeepMaxCount.ToString("00"));
                }
                else
                {
                    //look for latest
                    int i = 1;
                    fn = string.Format("{0}.bak{1}", Properties.Settings.Default.ActiveDataFile, i.ToString("00"));
                    while (File.Exists(fn))
                    {
                        i++;
                        fn = string.Format("{0}.bak{1}", Properties.Settings.Default.ActiveDataFile, i.ToString("00"));
                    }
                }
                DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                using (Utils.ProgressBlock prog = new ProgressBlock(this, STR_BACKINGUPDATA, STR_BACKINGUPDATA, 100, 0))
                {
                    using (System.IO.FileStream fs = File.OpenWrite(fn))
                    {
                        int read;
                        byte[] buffer = new byte[10 * 1024 * 1024];
                        fs.SetLength(_fileStream.Length);
                        _fileStream.Position = 0;
                        while (_fileStream.Position < _fileStream.Length)
                        {
                            read = _fileStream.Read(buffer, 0, buffer.Length);
                            fs.Write(buffer, 0, read);
                            if (DateTime.Now >= nextUpdate)
                            {
                                prog.UpdateProgress(STR_BACKINGUPDATA, STR_BACKINGUPDATA, 100, (int)(100.0 * (double)_fileStream.Position / (double)_fileStream.Length));
                                nextUpdate = DateTime.Now.AddSeconds(1);
                            }
                        }
                    }
                    _fileStream.Position = 0;
                    result = true;
                }
            }
            catch
            {
            }
            return result;
        }


        private string _fileToRestore = null;
        public override bool PrepareRestore()
        {
            bool result = false;
            //select backup to restore
            using (System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog())
            {
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(Properties.Settings.Default.ActiveDataFile);

                try
                {
                    dlg.Filter = "*.gsf.bak*|*.gsf.bak*";
                    dlg.InitialDirectory = Path.GetDirectoryName(Properties.Settings.Default.ActiveDataFile);
                    dlg.FileName = "";
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        _fileToRestore = dlg.FileName;
                        closeCurrentFile();

                        Properties.Settings.Default.ActiveDataFile = _fileToRestore.Substring(0, _fileToRestore.LastIndexOf(".bak", StringComparison.InvariantCultureIgnoreCase));
                        Properties.Settings.Default.Save();
                        SetDataSourceName(Properties.Settings.Default.ActiveDataFile);

                        Core.Geocaches.Clear();
                        Core.Logs.Clear();
                        Core.Waypoints.Clear();
                        Core.LogImages.Clear();
                        Core.UserWaypoints.Clear();

                        result = true;
                    }
                }
                catch
                {

                }
            }
            return result;
        }

        public override bool Restore(bool geocachesOnly)
        {
            bool result = false;
            File.Copy(_fileToRestore, Properties.Settings.Default.ActiveDataFile, true);
            result = Open(geocachesOnly);
            return result;
        }
    }
}
