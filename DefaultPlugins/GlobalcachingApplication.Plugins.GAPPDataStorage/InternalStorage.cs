using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Xml;
using GlobalcachingApplication.Utils;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.GAPPDataStorage
{
    public partial class InternalStorage : Utils.BasePlugin.BaseInternalStorage
    {
        public const string STR_LOADING = "Loading...";
        public const string STR_LOADINGDATA = "Loading data...";
        public const string STR_LOADINGGEOCACHES = "Loading geocaches...";
        public const string STR_LOADINGLOGS = "Loading logs...";
        public const string STR_LOADINGLOGIMAGES = "Loading log images...";
        public const string STR_LOADINGGEOCACHEIMAGES = "Loading geocache images...";
        public const string STR_LOADINGWAYPOINTS = "Loading waypoints...";
        public const string STR_SAVING = "Saving...";
        public const string STR_SAVINGDATA = "Saving data...";
        public const string STR_SAVINGGEOCACHES = "Saving geocaches...";
        public const string STR_SAVINGLOGS = "Saving logs...";
        public const string STR_SAVINGLOGIMAGES = "Saving log images...";
        public const string STR_SAVINGGEOCACHEIMAGES = "Saving geocache images...";
        public const string STR_SAVINGWAYPOINTS = "Saving waypoints...";
        public const string STR_BACKINGUPDATA = "Creating backup...";
        public const string STR_RESTORINGDATA = "Restoring backup...";

        public const string EXT_GEOCACHES = ".cch";
        public const string EXT_LOGS = ".lgs";
        public const string EXT_WAYPPOINTS = ".wpt";
        public const string EXT_USERWAYPOINTS = ".uwp";
        public const string EXT_LOGIMAGES = ".lmg";
        public const string EXT_GEOCACHEIMAGES = ".gmg";
        public const string EXT_DATABASEINFO = ".gpp";

        public class FileCollection : IDisposable
        {
            public string BaseFilename { get; private set; }
            public FileStream _fsGeocaches { get; private set; }
            public FileStream _fsLogs { get; private set; }
            public string DatabaseInfoFilename { get { return getFilename(BaseFilename, EXT_DATABASEINFO); } }
            public string WaypointsFilename { get { return getFilename(BaseFilename, EXT_WAYPPOINTS); } }
            public string UserWaypointsFilename { get { return getFilename(BaseFilename, EXT_USERWAYPOINTS); } }
            public string LogImagesFilename { get { return getFilename(BaseFilename, EXT_LOGIMAGES); } }
            public string GeocacheImagesFilename { get { return getFilename(BaseFilename, EXT_GEOCACHEIMAGES); } }

            public Hashtable _geocachesInDB { get; private set; }
            public Hashtable _logsInDB { get; private set; }
            public Hashtable _logimgsInDB { get; private set; }
            public Hashtable _wptsInDB { get; private set; }
            public Hashtable _usrwptsInDB { get; private set; }
            public Hashtable _geocacheimgsInDB { get; private set; }

            public FileCollection(string baseFilename)
            {
                BaseFilename = baseFilename;
                _fsGeocaches = File.Open(getFilename(baseFilename, EXT_GEOCACHES), FileMode.OpenOrCreate, FileAccess.ReadWrite);
                _fsLogs = File.Open(getFilename(baseFilename, EXT_LOGS), FileMode.OpenOrCreate, FileAccess.ReadWrite);

                _geocachesInDB = new Hashtable();
                _logsInDB = new Hashtable();
                _logimgsInDB = new Hashtable();
                _wptsInDB = new Hashtable();
                _usrwptsInDB = new Hashtable();
                _geocacheimgsInDB = new Hashtable();
            }

            public static string getFilename(string targetFile, string extension)
            {
                return Path.Combine(Path.GetDirectoryName(targetFile), string.Format("{0}{1}", Path.GetFileNameWithoutExtension(targetFile), extension));
            }

            public void StartReleaseForCopy()
            {
                _fsGeocaches.Close();
                _fsLogs.Close();
                _fsGeocaches.Dispose();
                _fsLogs.Dispose();
                _fsGeocaches = null;
                _fsLogs = null;
            }

            public void EndReleaseForCopy()
            {
                _fsGeocaches = File.Open(getFilename(BaseFilename, EXT_GEOCACHES), FileMode.OpenOrCreate, FileAccess.ReadWrite);
                _fsLogs = File.Open(getFilename(BaseFilename, EXT_LOGS), FileMode.OpenOrCreate, FileAccess.ReadWrite);
            }


            public void Dispose()
            {
                if (_fsGeocaches != null)
                {
                    _fsGeocaches.Dispose();
                    _fsGeocaches = null;
                }
                if (_fsLogs != null)
                {
                    _fsLogs.Dispose();
                    _fsLogs = null;
                }
            }
        }
        private FileCollection _fileCollection = null;

        public class RecordInfo
        {
            public string ID { get; set; }
            public bool FreeSlot { get; set; }
            public long Offset { get; set; }
            public long Length { get; set; }
        }

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            var p = new PluginSettings(core);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LOADING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LOADINGDATA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LOADINGGEOCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LOADINGLOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LOADINGLOGIMAGES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LOADINGGEOCACHEIMAGES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LOADINGWAYPOINTS));
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

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_BACKUPFOLDER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_MAXCOUNT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_MAXDAYS));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(RestoreForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(RestoreForm.STR_BACKUPFOLDER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(RestoreForm.STR_BACKUPS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(RestoreForm.STR_DATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(RestoreForm.STR_FILE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(RestoreForm.STR_OK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(RestoreForm.STR_PATH));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(RestoreForm.STR_RESTOREFOLDER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(RestoreForm.STR_WARNING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(RestoreForm.STR_OVERWRITE));

            if (string.IsNullOrEmpty(PluginSettings.Instance.ActiveDataFile))
            {
                PluginSettings.Instance.ActiveDataFile = System.IO.Path.Combine(core.PluginDataPath, "GAPPDataStorage.gpp" );
            }

            try
            {
                _fileCollection = new FileCollection(PluginSettings.Instance.ActiveDataFile);
            }
            catch
            {
            }

            SetDataSourceName(PluginSettings.Instance.ActiveDataFile);
            core.Logs.LoadFullData += new Framework.EventArguments.LoadFullLogEventHandler(Logs_LoadFullData);
            core.Geocaches.LoadFullData += new Framework.EventArguments.LoadFullGeocacheEventHandler(Geocaches_LoadFullData);

            return await base.InitializeAsync(core);
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

        public override void StartReleaseForCopy()
        {
            if (_fileCollection != null)
            {
                if (CheckBackgroundTaskNotRunning())
                {
                    _fileCollection.StartReleaseForCopy();
                }
            }
        }

        public override void EndReleaseForCopy()
        {
            if (_fileCollection != null)
            {
                if (CheckBackgroundTaskNotRunning())
                {
                    _fileCollection.EndReleaseForCopy();
                }
            }
        }

        public override Framework.Data.InternalStorageDestination ActiveStorageDestination 
        { 
            get 
            {
                Framework.Data.InternalStorageDestination isd = null;
                if (_fileCollection != null)
                {
                    isd = new Framework.Data.InternalStorageDestination();
                    isd.Name = Path.GetFileName(_fileCollection.BaseFilename);
                    isd.PluginType = this.GetType().ToString();
                    isd.StorageInfo = new string[] { _fileCollection.BaseFilename };
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
                PluginSettings.Instance.ActiveDataFile = dst.StorageInfo[0];
                SetDataSourceName(PluginSettings.Instance.ActiveDataFile);

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
                return true;
            }
        }

        void Geocaches_LoadFullData(object sender, Framework.EventArguments.LoadFullGeocacheEventArgs e)
        {
            if (_fileCollection!=null)
            {
                try
                {
                    string id = string.Concat("F_",e.RequestedForGeocache.Code);
                    RecordInfo ri = _fileCollection._geocachesInDB[id] as RecordInfo;
                    if (ri != null)
                    {
                        byte[] memblock = new byte[ri.Length];
                        _fileCollection._fsGeocaches.Position = ri.Offset;
                        _fileCollection._fsGeocaches.Read(memblock, 0, memblock.Length);
                        using (MemoryStream ms = new MemoryStream(memblock))
                        using (BinaryReader br = new BinaryReader(ms))
                        {
                            ms.Position = sizeof(long)+1;
                            br.ReadString(); //id
                            e.ShortDescription = br.ReadString();
                            e.ShortDescriptionInHtml = br.ReadBoolean();
                            e.LongDescription = br.ReadString();
                            e.LongDescriptionInHtml = br.ReadBoolean();
                        }
                    }
                }
                catch
                {
                }
            }
        }

        void Logs_LoadFullData(object sender, Framework.EventArguments.LoadFullLogEventArgs e)
        {
            if (_fileCollection != null)
            {
                try
                {
                    string id = string.Concat("F_", e.RequestForLog.ID);
                    RecordInfo ri = _fileCollection._logsInDB[id] as RecordInfo;
                    if (ri != null)
                    {
                        byte[] memblock = new byte[ri.Length];
                        _fileCollection._fsLogs.Position = ri.Offset;
                        _fileCollection._fsLogs.Read(memblock, 0, memblock.Length);
                        using (MemoryStream ms = new MemoryStream(memblock))
                        using (BinaryReader br = new BinaryReader(ms))
                        {
                            ms.Position = sizeof(long) + 1;
                            br.ReadString(); //id
                            e.TBCode = br.ReadString();
                            e.FinderId = br.ReadString();
                            e.Text = br.ReadString();
                            e.Encoded = br.ReadBoolean();
                        }
                    }
                }
                catch
                {
                }
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
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(PluginSettings.Instance.ActiveDataFile);

                dlg.Filter = "*.gpp|*.gpp";
                dlg.FileName = "";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        if (System.IO.File.Exists(dlg.FileName))
                        {
                            System.IO.File.Delete(dlg.FileName);
                        }
                        string fn = FileCollection.getFilename(dlg.FileName, EXT_GEOCACHES);
                        if (System.IO.File.Exists(fn))
                        {
                            System.IO.File.Delete(fn);
                        }
                        fn = FileCollection.getFilename(dlg.FileName, EXT_LOGIMAGES);
                        if (System.IO.File.Exists(fn))
                        {
                            System.IO.File.Delete(fn);
                        }
                        fn = FileCollection.getFilename(dlg.FileName, EXT_GEOCACHEIMAGES);
                        if (System.IO.File.Exists(fn))
                        {
                            System.IO.File.Delete(fn);
                        }
                        fn = FileCollection.getFilename(dlg.FileName, EXT_LOGS);
                        if (System.IO.File.Exists(fn))
                        {
                            System.IO.File.Delete(fn);
                        }
                        fn = FileCollection.getFilename(dlg.FileName, EXT_USERWAYPOINTS);
                        if (System.IO.File.Exists(fn))
                        {
                            System.IO.File.Delete(fn);
                        }
                        fn = FileCollection.getFilename(dlg.FileName, EXT_WAYPPOINTS);
                        if (System.IO.File.Exists(fn))
                        {
                            System.IO.File.Delete(fn);
                        }

                        PluginSettings.Instance.ActiveDataFile = dlg.FileName;

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
                        SetDataSourceName(PluginSettings.Instance.ActiveDataFile);
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
            FileCollection newFileCollection = new FileCollection(PluginSettings.Instance.ActiveDataFile);
            result = Save(newFileCollection, true);
            if (_fileCollection != null)
            {
                _fileCollection.Dispose();
                _fileCollection = null;
            }
            _fileCollection = newFileCollection;
            return result;
        }
        public override bool PrepareNew()
        {
            bool result = false;
            using (System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog())
            {
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(PluginSettings.Instance.ActiveDataFile);

                dlg.Filter = "*.gpp|*.gpp";
                dlg.FileName = "";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        if (System.IO.File.Exists(dlg.FileName))
                        {
                            System.IO.File.Delete(dlg.FileName);
                        }
                        string fn = FileCollection.getFilename(dlg.FileName, EXT_GEOCACHES);
                        if (System.IO.File.Exists(fn))
                        {
                            System.IO.File.Delete(fn);
                        }
                        fn = FileCollection.getFilename(dlg.FileName, EXT_LOGIMAGES);
                        if (System.IO.File.Exists(fn))
                        {
                            System.IO.File.Delete(fn);
                        }
                        fn = FileCollection.getFilename(dlg.FileName, EXT_GEOCACHEIMAGES);
                        if (System.IO.File.Exists(fn))
                        {
                            System.IO.File.Delete(fn);
                        }
                        fn = FileCollection.getFilename(dlg.FileName, EXT_LOGS);
                        if (System.IO.File.Exists(fn))
                        {
                            System.IO.File.Delete(fn);
                        }
                        fn = FileCollection.getFilename(dlg.FileName, EXT_USERWAYPOINTS);
                        if (System.IO.File.Exists(fn))
                        {
                            System.IO.File.Delete(fn);
                        }
                        fn = FileCollection.getFilename(dlg.FileName, EXT_WAYPPOINTS);
                        if (System.IO.File.Exists(fn))
                        {
                            System.IO.File.Delete(fn);
                        }
                        PluginSettings.Instance.ActiveDataFile = dlg.FileName;

                        SetDataSourceName(PluginSettings.Instance.ActiveDataFile);
                        using (FrameworkDataUpdater upd = new FrameworkDataUpdater(Core))
                        {
                            Core.Geocaches.Clear();
                            Core.Logs.Clear();
                            Core.LogImages.Clear();
                            Core.Waypoints.Clear();
                            Core.UserWaypoints.Clear();
                            Core.GeocacheImages.Clear();
                        }

                        FileCollection newFileCollection = new FileCollection(PluginSettings.Instance.ActiveDataFile);
                        if (_fileCollection != null)
                        {
                            _fileCollection.Dispose();
                            _fileCollection = null;
                        }
                        _fileCollection = newFileCollection;

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
            Save(_fileCollection, true);
            return result;
        }
        public override bool PrepareOpen()
        {
            bool result = false;
            using (System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog())
            {
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(PluginSettings.Instance.ActiveDataFile);

                dlg.Filter = "*.gpp|*.gpp";
                dlg.FileName = "";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    PluginSettings.Instance.ActiveDataFile = dlg.FileName;
                    SetDataSourceName(PluginSettings.Instance.ActiveDataFile);

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
            if (_fileCollection != null)
            {
                _fileCollection.Dispose();
                _fileCollection = null;
            }
            _fileCollection = new FileCollection(PluginSettings.Instance.ActiveDataFile);
            result = Load(geocachesOnly);
            return result;
        }

        public override bool Load(bool geocachesOnly)
        {
            bool result = true;

            if (File.Exists(_fileCollection.DatabaseInfoFilename))
            {
                int lsize = sizeof(long);
                byte[] memBuffer = new byte[10 * 1024 * 1024];
                using (MemoryStream ms = new MemoryStream(memBuffer))
                using (BinaryReader br = new BinaryReader(ms))
                {

                    int gcCount = 0;
                    int logCount = 0;
                    int logimgCount = 0;
                    int geocacheimgCount = 0;
                    int wptCount = 0;
                    int usrwptCount = 0;

                    XmlDocument doc = new XmlDocument();
                    doc.Load(_fileCollection.DatabaseInfoFilename);
                    XmlElement root = doc.DocumentElement;
                    gcCount = int.Parse(root.SelectSingleNode("GeocacheCount").InnerText);
                    logCount = int.Parse(root.SelectSingleNode("LogCount").InnerText);
                    logimgCount = int.Parse(root.SelectSingleNode("LogImagesCount").InnerText);
                    wptCount = int.Parse(root.SelectSingleNode("WaypointCount").InnerText);
                    usrwptCount = int.Parse(root.SelectSingleNode("UserWaypointCount").InnerText);
                    if (root.SelectSingleNode("GeocacheImagesCount") != null)
                    {
                        geocacheimgCount = int.Parse(root.SelectSingleNode("GeocacheImagesCount").InnerText);
                    }

                    DateTime nextUpdateTime = DateTime.MinValue;
                    using (Utils.ProgressBlock fixscr = new Utils.ProgressBlock(this, STR_LOADING, STR_LOADINGDATA, 1, 0))
                    {
                        using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_LOADING, STR_LOADINGGEOCACHES, gcCount, 0))
                        {
                            int index = 0;
                            //int procStep = 0;
                            List<Framework.Data.Geocache> gcList = new List<Framework.Data.Geocache>();

                            FileStream fs = _fileCollection._fsGeocaches;
                            fs.Position = 0;
                            long eof = fs.Length;
                            while (fs.Position < eof)
                            {
                                RecordInfo ri = new RecordInfo();
                                ri.Offset = fs.Position;
                                fs.Read(memBuffer, 0, lsize + 1);
                                ms.Position = 0;
                                ri.Length = br.ReadInt64();
                                if (memBuffer[lsize] == 0)
                                {
                                    //free
                                    ri.FreeSlot = true;
                                    ri.ID = string.Concat("_", ri.Offset.ToString());
                                    fs.Position = ri.Offset + ri.Length;
                                }
                                else if (memBuffer[lsize] == 2)
                                {
                                    //lazy loading
                                    ri.FreeSlot = false;
                                    int readCount = Math.Min(32, (int)(ri.Length - lsize - 1));
                                    fs.Read(memBuffer, 0, readCount);
                                    ms.Position = 0;
                                    ri.ID = br.ReadString();
                                    fs.Position = ri.Offset + ri.Length;
                                }
                                else
                                {
                                    //read
                                    ri.FreeSlot = false;
                                    Framework.Data.Geocache gc = new Framework.Data.Geocache();

                                    fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                    ms.Position = 0;
                                    gc.Code = br.ReadString();
                                    ri.ID = gc.Code;

                                    gc.Archived = br.ReadBoolean();
                                    gc.AttributeIds = ReadIntegerArray(br);
                                    gc.Available = br.ReadBoolean();
                                    gc.City = br.ReadString();
                                    gc.Container = Utils.DataAccess.GetGeocacheContainer(Core.GeocacheContainers, br.ReadInt32());
                                    gc.CustomCoords = br.ReadBoolean();
                                    gc.Country = br.ReadString();
                                    if (br.ReadBoolean())
                                    {
                                        gc.CustomLat = br.ReadDouble();
                                        gc.CustomLon = br.ReadDouble();
                                    }
                                    gc.Difficulty = br.ReadDouble();
                                    gc.EncodedHints = br.ReadString();
                                    gc.Favorites = br.ReadInt32();
                                    gc.Flagged = br.ReadBoolean();
                                    gc.Found = br.ReadBoolean();
                                    gc.GeocacheType = Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, br.ReadInt32());
                                    gc.ID = br.ReadString();
                                    gc.Lat = br.ReadDouble();
                                    gc.Lon = br.ReadDouble();
                                    gc.MemberOnly = br.ReadBoolean();
                                    gc.Municipality = br.ReadString();
                                    gc.Name = br.ReadString();
                                    gc.Notes = br.ReadString();
                                    gc.Owner = br.ReadString();
                                    gc.OwnerId = br.ReadString();
                                    gc.PersonaleNote = br.ReadString();
                                    gc.PlacedBy = br.ReadString();
                                    gc.PublishedTime = DateTime.Parse(br.ReadString());
                                    gc.State = br.ReadString();
                                    gc.Terrain = br.ReadDouble();
                                    gc.Title = br.ReadString();
                                    gc.Url = br.ReadString();
                                    gc.DataFromDate = DateTime.Parse(br.ReadString());
                                    gc.Locked = br.ReadBoolean();

                                    Calculus.SetDistanceAndAngleGeocacheFromLocation(gc, Core.CenterLocation);
                                    gc.Saved = true;
                                    gc.IsDataChanged = false;
                                    //gcList.Add(gc);
                                    Core.Geocaches.Add(gc);

                                    index++;
                                    //procStep++;
                                    //if (procStep >= 1000)
                                    if (DateTime.Now>=nextUpdateTime)
                                    {
                                        progress.UpdateProgress(STR_LOADING, STR_LOADINGGEOCACHES, gcCount, index);
                                        nextUpdateTime = DateTime.Now.AddSeconds(1);
                                        //procStep = 0;
                                    }
                                }
                                _fileCollection._geocachesInDB.Add(ri.ID, ri);
                            }
                            //Core.Geocaches.Add(gcList);
                        }

                        if (!geocachesOnly)
                        {
                            using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_LOADING, STR_LOADINGLOGS, logCount, 0))
                            {
                                int index = 0;
                                nextUpdateTime = DateTime.MinValue;

                                List<Framework.Data.Log> lgList = new List<Framework.Data.Log>();

                                FileStream fs = _fileCollection._fsLogs;
                                fs.Position = 0;
                                long eof = fs.Length;
                                while (fs.Position < eof)
                                {
                                    RecordInfo ri = new RecordInfo();
                                    ri.Offset = fs.Position;
                                    fs.Read(memBuffer, 0, lsize + 1);
                                    ms.Position = 0;
                                    ri.Length = br.ReadInt64();
                                    if (memBuffer[lsize] == 0)
                                    {
                                        //free
                                        ri.FreeSlot = true;
                                        ri.ID = string.Concat("_", ri.Offset.ToString());
                                        fs.Position = ri.Offset + ri.Length;
                                    }
                                    else if (memBuffer[lsize] == 2)
                                    {
                                        //lazy loading
                                        ri.FreeSlot = false;
                                        int readCount = Math.Min(32, (int)(ri.Length - lsize - 1));
                                        fs.Read(memBuffer, 0, readCount);
                                        ms.Position = 0;
                                        ri.ID = br.ReadString();
                                        fs.Position = ri.Offset + ri.Length;
                                    }
                                    else
                                    {
                                        //read
                                        ri.FreeSlot = false;
                                        Framework.Data.Log log = new Framework.Data.Log();

                                        fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                        ms.Position = 0;
                                        log.ID = br.ReadString();
                                        ri.ID = log.ID;

                                        log.DataFromDate = DateTime.Parse(br.ReadString());
                                        log.Date = DateTime.Parse(br.ReadString());
                                        log.Finder = br.ReadString();
                                        log.GeocacheCode = br.ReadString();
                                        log.ID = br.ReadString();
                                        log.LogType = Utils.DataAccess.GetLogType(Core.LogTypes, br.ReadInt32());

                                        log.Saved = true;
                                        log.IsDataChanged = false;
                                        lgList.Add(log);

                                        index++;
                                        //procStep++;
                                        //if (procStep >= 1000)
                                        if (DateTime.Now >= nextUpdateTime)
                                        {
                                            progress.UpdateProgress(STR_LOADING, STR_LOADINGLOGS, logCount, index);
                                            nextUpdateTime = DateTime.Now.AddSeconds(1);
                                            //procStep = 0;
                                        }
                                    }
                                    _fileCollection._logsInDB.Add(ri.ID, ri);
                                }
                                Core.Logs.Add(lgList);
                            }


                            using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_LOADING, STR_LOADINGWAYPOINTS, wptCount, 0))
                            {
                                int index = 0;
                                int procStep = 0;

                                List<Framework.Data.Waypoint> wptList = new List<Framework.Data.Waypoint>();

                                using (FileStream fs = File.Open(_fileCollection.WaypointsFilename, FileMode.OpenOrCreate, FileAccess.Read))
                                {
                                    fs.Position = 0;
                                    long eof = fs.Length;
                                    while (fs.Position < eof)
                                    {
                                        RecordInfo ri = new RecordInfo();
                                        ri.Offset = fs.Position;
                                        fs.Read(memBuffer, 0, lsize + 1);
                                        ms.Position = 0;
                                        ri.Length = br.ReadInt64();
                                        if (memBuffer[lsize] == 0)
                                        {
                                            //free
                                            ri.FreeSlot = true;
                                            ri.ID = string.Concat("_", ri.Offset.ToString());
                                            fs.Position = ri.Offset + ri.Length;
                                        }
                                        else
                                        {
                                            //read
                                            ri.FreeSlot = false;
                                            Framework.Data.Waypoint wp = new Framework.Data.Waypoint();

                                            fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                            ms.Position = 0;
                                            wp.Code = br.ReadString();
                                            ri.ID = wp.Code;

                                            wp.Comment = br.ReadString();
                                            wp.DataFromDate = DateTime.Parse(br.ReadString());
                                            wp.Description = br.ReadString();
                                            wp.GeocacheCode = br.ReadString();
                                            wp.ID = br.ReadString();
                                            if (br.ReadBoolean())
                                            {
                                                wp.Lat = br.ReadDouble();
                                                wp.Lon = br.ReadDouble();
                                            }
                                            wp.Name = br.ReadString();
                                            wp.Time = DateTime.Parse(br.ReadString());
                                            wp.Url = br.ReadString();
                                            wp.UrlName = br.ReadString();
                                            wp.WPType = Utils.DataAccess.GetWaypointType(Core.WaypointTypes, br.ReadInt32());

                                            wp.Saved = true;
                                            wp.IsDataChanged = false;
                                            wptList.Add(wp);

                                            index++;
                                            procStep++;
                                            if (procStep >= 1000)
                                            {
                                                progress.UpdateProgress(STR_LOADING, STR_LOADINGWAYPOINTS, wptCount, index);
                                                procStep = 0;
                                            }
                                        }
                                        _fileCollection._wptsInDB.Add(ri.ID, ri);
                                    }
                                }
                                Core.Waypoints.Add(wptList);
                            }

                            using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_LOADING, STR_LOADINGLOGIMAGES, logimgCount, 0))
                            {
                                int index = 0;
                                int procStep = 0;

                                List<Framework.Data.LogImage> lgiList = new List<Framework.Data.LogImage>();

                                using (FileStream fs = File.Open(_fileCollection.LogImagesFilename, FileMode.OpenOrCreate, FileAccess.Read))
                                {
                                    fs.Position = 0;
                                    long eof = fs.Length;
                                    while (fs.Position < eof)
                                    {
                                        RecordInfo ri = new RecordInfo();
                                        ri.Offset = fs.Position;
                                        fs.Read(memBuffer, 0, lsize + 1);
                                        ms.Position = 0;
                                        ri.Length = br.ReadInt64();
                                        if (memBuffer[lsize] == 0)
                                        {
                                            //free
                                            ri.FreeSlot = true;
                                            ri.ID = string.Concat("_", ri.Offset.ToString());
                                            fs.Position = ri.Offset + ri.Length;
                                        }
                                        else
                                        {
                                            //read
                                            ri.FreeSlot = false;
                                            Framework.Data.LogImage li = new Framework.Data.LogImage();

                                            fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                            ms.Position = 0;
                                            li.ID = br.ReadString();
                                            ri.ID = li.ID;

                                            li.DataFromDate = DateTime.Parse(br.ReadString());
                                            li.LogID = br.ReadString();
                                            li.Name = br.ReadString();
                                            li.Url = br.ReadString();

                                            li.Saved = true;
                                            li.IsDataChanged = false;
                                            lgiList.Add(li);

                                            index++;
                                            procStep++;
                                            if (procStep >= 1000)
                                            {
                                                progress.UpdateProgress(STR_LOADING, STR_LOADINGLOGIMAGES, logimgCount, index);
                                                procStep = 0;
                                            }
                                        }
                                        _fileCollection._logimgsInDB.Add(ri.ID, ri);
                                    }
                                }
                                Core.LogImages.Add(lgiList);
                            }

                            using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_LOADING, STR_LOADINGGEOCACHEIMAGES, geocacheimgCount, 0))
                            {
                                int index = 0;
                                int procStep = 0;

                                List<Framework.Data.GeocacheImage> lgiList = new List<Framework.Data.GeocacheImage>();

                                using (FileStream fs = File.Open(_fileCollection.GeocacheImagesFilename, FileMode.OpenOrCreate, FileAccess.Read))
                                {
                                    fs.Position = 0;
                                    long eof = fs.Length;
                                    while (fs.Position < eof)
                                    {
                                        RecordInfo ri = new RecordInfo();
                                        ri.Offset = fs.Position;
                                        fs.Read(memBuffer, 0, lsize + 1);
                                        ms.Position = 0;
                                        ri.Length = br.ReadInt64();
                                        if (memBuffer[lsize] == 0)
                                        {
                                            //free
                                            ri.FreeSlot = true;
                                            ri.ID = string.Concat("_", ri.Offset.ToString());
                                            fs.Position = ri.Offset + ri.Length;
                                        }
                                        else
                                        {
                                            //read
                                            ri.FreeSlot = false;
                                            Framework.Data.GeocacheImage li = new Framework.Data.GeocacheImage();

                                            fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                            ms.Position = 0;
                                            li.ID = br.ReadString();
                                            ri.ID = li.ID;

                                            li.DataFromDate = DateTime.Parse(br.ReadString());
                                            li.GeocacheCode = br.ReadString();
                                            li.Description = br.ReadString();
                                            li.Name = br.ReadString();
                                            li.Url = br.ReadString();
                                            li.MobileUrl = br.ReadString();
                                            li.ThumbUrl = br.ReadString();

                                            li.Saved = true;
                                            li.IsDataChanged = false;
                                            lgiList.Add(li);

                                            index++;
                                            procStep++;
                                            if (procStep >= 1000)
                                            {
                                                progress.UpdateProgress(STR_LOADING, STR_LOADINGGEOCACHEIMAGES, geocacheimgCount, index);
                                                procStep = 0;
                                            }
                                        }
                                        _fileCollection._geocacheimgsInDB.Add(ri.ID, ri);
                                    }
                                }
                                Core.GeocacheImages.Add(lgiList);
                            }

                            //using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_LOADING, STR_LOADINGLOGIMAGES, logimgCount, 0))
                            {
                                int index = 0;
                                int procStep = 0;

                                List<Framework.Data.UserWaypoint> uwplList = new List<Framework.Data.UserWaypoint>();

                                using (FileStream fs = File.Open(_fileCollection.UserWaypointsFilename, FileMode.OpenOrCreate, FileAccess.Read))
                                {
                                    fs.Position = 0;
                                    long eof = fs.Length;
                                    while (fs.Position < eof)
                                    {
                                        RecordInfo ri = new RecordInfo();
                                        ri.Offset = fs.Position;
                                        fs.Read(memBuffer, 0, lsize + 1);
                                        ms.Position = 0;
                                        ri.Length = br.ReadInt64();
                                        if (memBuffer[lsize] == 0)
                                        {
                                            //free
                                            ri.FreeSlot = true;
                                            ri.ID = string.Concat("_", ri.Offset.ToString());
                                            fs.Position = ri.Offset + ri.Length;
                                        }
                                        else
                                        {
                                            //read
                                            ri.FreeSlot = false;
                                            Framework.Data.UserWaypoint wp = new Framework.Data.UserWaypoint();

                                            fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                            ms.Position = 0;
                                            ri.ID = br.ReadString();
                                            wp.ID = int.Parse(ri.ID);

                                            wp.Description = br.ReadString();
                                            wp.GeocacheCode = br.ReadString();
                                            wp.Lat = br.ReadDouble();
                                            wp.Lon = br.ReadDouble();
                                            wp.Date = DateTime.Parse(br.ReadString());

                                            wp.Saved = true;
                                            wp.IsDataChanged = false;
                                            uwplList.Add(wp);

                                            index++;
                                            procStep++;
                                            if (procStep >= 1000)
                                            {
                                                //progress.UpdateProgress(STR_LOADING, STR_LOADINGLOGIMAGES, logimgCount, index);
                                                procStep = 0;
                                            }
                                        }
                                        _fileCollection._usrwptsInDB.Add(ri.ID, ri);
                                    }
                                }
                                Core.UserWaypoints.Add(uwplList);
                            }

                        }
                    }
                }
            }

            return result;
        }

        public override bool LoadLogs(List<Framework.Data.Log> logs)
        {
            bool result = true;
            int index = 0;

            if (File.Exists(_fileCollection.DatabaseInfoFilename))
            {

                //int procStep = 0;
                DateTime nextUpdateTime = DateTime.MinValue;

                int lsize = sizeof(long);
                byte[] memBuffer = new byte[50 * 1024];
                using (MemoryStream ms = new MemoryStream(memBuffer))
                using (BinaryReader br = new BinaryReader(ms))
                {

                    int logCount = 0;

                    XmlDocument doc = new XmlDocument();
                    doc.Load(_fileCollection.DatabaseInfoFilename);
                    XmlElement root = doc.DocumentElement;
                    logCount = int.Parse(root.SelectSingleNode("LogCount").InnerText);

                    UpdateLoadingInBackgroundProgress(STR_LOADING_LOGS_BG, logCount, 0);

                    FileStream fs = _fileCollection._fsLogs;
                    fs.Position = 0;
                    long eof = fs.Length;
                    while (fs.Position < eof)
                    {
                        RecordInfo ri = new RecordInfo();
                        ri.Offset = fs.Position;
                        fs.Read(memBuffer, 0, lsize + 1);
                        ms.Position = 0;
                        ri.Length = br.ReadInt64();
                        if (memBuffer[lsize] == 0)
                        {
                            //free
                            ri.FreeSlot = true;
                            ri.ID = string.Concat("_", ri.Offset.ToString());
                            fs.Position = ri.Offset + ri.Length;
                        }
                        else if (memBuffer[lsize] == 2)
                        {
                            //lazy loading
                            ri.FreeSlot = false;
                            int readCount = Math.Min(42, (int)(ri.Length - lsize - 1));
                            fs.Read(memBuffer, 0, readCount);
                            ms.Position = 0;
                            ri.ID = br.ReadString();
                            fs.Position = ri.Offset + ri.Length;
                        }
                        else
                        {
                            //read
                            ri.FreeSlot = false;
                            Framework.Data.Log log = new Framework.Data.Log();

                            fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                            ms.Position = 0;
                            log.ID = br.ReadString();
                            ri.ID = log.ID;

                            log.DataFromDate = DateTime.Parse(br.ReadString());
                            log.Date = DateTime.Parse(br.ReadString());
                            log.Finder = br.ReadString();
                            log.GeocacheCode = br.ReadString();
                            log.ID = br.ReadString();
                            log.LogType = Utils.DataAccess.GetLogType(Core.LogTypes, br.ReadInt32());

                            log.Saved = true;
                            log.IsDataChanged = false;
                            logs.Add(log);

                            index++;
                            //procStep++;
                            //if (procStep >= 20000)
                            if (DateTime.Now >= nextUpdateTime)
                            {
                                UpdateLoadingInBackgroundProgress(STR_LOADING_LOGS_BG, logCount, index);
                                nextUpdateTime = DateTime.Now.AddSeconds(1);
                                //procStep = 0;
                            }
                        }
                        _fileCollection._logsInDB.Add(ri.ID, ri);
                    }
                }
            }
            return result;
        }
        public override bool LoadWaypoints(List<Framework.Data.Waypoint> wps, List<Framework.Data.UserWaypoint> usrwps)
        {
            bool result = true;
            int index = 0;
            int procStep = 0;

            if (File.Exists(_fileCollection.DatabaseInfoFilename))
            {

                int lsize = sizeof(long);
                byte[] memBuffer = new byte[5 * 1024];
                using (MemoryStream ms = new MemoryStream(memBuffer))
                using (BinaryReader br = new BinaryReader(ms))
                {

                    int wptCount = 0;

                    XmlDocument doc = new XmlDocument();
                    doc.Load(_fileCollection.DatabaseInfoFilename);
                    XmlElement root = doc.DocumentElement;
                    wptCount = int.Parse(root.SelectSingleNode("WaypointCount").InnerText);

                    using (FileStream fs = File.Open(_fileCollection.WaypointsFilename, FileMode.OpenOrCreate, FileAccess.Read))
                    {
                        fs.Position = 0;
                        long eof = fs.Length;
                        while (fs.Position < eof)
                        {
                            RecordInfo ri = new RecordInfo();
                            ri.Offset = fs.Position;
                            fs.Read(memBuffer, 0, lsize + 1);
                            ms.Position = 0;
                            ri.Length = br.ReadInt64();
                            if (memBuffer[lsize] == 0)
                            {
                                //free
                                ri.FreeSlot = true;
                                ri.ID = string.Concat("_", ri.Offset.ToString());
                                fs.Position = ri.Offset + ri.Length;
                            }
                            else
                            {
                                //read
                                ri.FreeSlot = false;
                                Framework.Data.Waypoint wp = new Framework.Data.Waypoint();

                                fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                ms.Position = 0;
                                wp.Code = br.ReadString();
                                ri.ID = wp.Code;

                                wp.Comment = br.ReadString();
                                wp.DataFromDate = DateTime.Parse(br.ReadString());
                                wp.Description = br.ReadString();
                                wp.GeocacheCode = br.ReadString();
                                wp.ID = br.ReadString();
                                if (br.ReadBoolean())
                                {
                                    wp.Lat = br.ReadDouble();
                                    wp.Lon = br.ReadDouble();
                                }
                                wp.Name = br.ReadString();
                                wp.Time = DateTime.Parse(br.ReadString());
                                wp.Url = br.ReadString();
                                wp.UrlName = br.ReadString();
                                wp.WPType = Utils.DataAccess.GetWaypointType(Core.WaypointTypes, br.ReadInt32());

                                wp.Saved = true;
                                wp.IsDataChanged = false;
                                wps.Add(wp);

                                index++;
                                procStep++;
                                if (procStep >= 20000)
                                {
                                    UpdateLoadingInBackgroundProgress(STR_LOADING_WAYPOINTS_BG, wptCount, index);
                                    procStep = 0;
                                }
                            }
                            _fileCollection._wptsInDB.Add(ri.ID, ri);
                        }
                    }

                    using (FileStream fs = File.Open(_fileCollection.UserWaypointsFilename, FileMode.OpenOrCreate, FileAccess.Read))
                    {
                        fs.Position = 0;
                        long eof = fs.Length;
                        while (fs.Position < eof)
                        {
                            RecordInfo ri = new RecordInfo();
                            ri.Offset = fs.Position;
                            fs.Read(memBuffer, 0, lsize + 1);
                            ms.Position = 0;
                            ri.Length = br.ReadInt64();
                            if (memBuffer[lsize] == 0)
                            {
                                //free
                                ri.FreeSlot = true;
                                ri.ID = string.Concat("_", ri.Offset.ToString());
                                fs.Position = ri.Offset + ri.Length;
                            }
                            else
                            {
                                //read
                                ri.FreeSlot = false;
                                Framework.Data.UserWaypoint wp = new Framework.Data.UserWaypoint();

                                fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                ms.Position = 0;
                                ri.ID = br.ReadString();
                                wp.ID = int.Parse(ri.ID);

                                wp.Description = br.ReadString();
                                wp.GeocacheCode = br.ReadString();
                                wp.Lat = br.ReadDouble();
                                wp.Lon = br.ReadDouble();
                                wp.Date = DateTime.Parse(br.ReadString());

                                wp.Saved = true;
                                wp.IsDataChanged = false;
                                usrwps.Add(wp);
                            }
                            _fileCollection._usrwptsInDB.Add(ri.ID, ri);
                        }
                    }

                }
            }

            return result;
        }
        public override bool LoadLogImages(List<Framework.Data.LogImage> logimgs)
        {
            bool result = true;
            int index = 0;
            int procStep = 0;

            if (File.Exists(_fileCollection.DatabaseInfoFilename))
            {

                int lsize = sizeof(long);
                byte[] memBuffer = new byte[5 * 1024];
                using (MemoryStream ms = new MemoryStream(memBuffer))
                using (BinaryReader br = new BinaryReader(ms))
                {

                    int logimgCount = 0;

                    XmlDocument doc = new XmlDocument();
                    doc.Load(_fileCollection.DatabaseInfoFilename);
                    XmlElement root = doc.DocumentElement;
                    logimgCount = int.Parse(root.SelectSingleNode("LogImagesCount").InnerText);

                    using (FileStream fs = File.Open(_fileCollection.LogImagesFilename, FileMode.OpenOrCreate, FileAccess.Read))
                    {
                        fs.Position = 0;
                        long eof = fs.Length;
                        while (fs.Position < eof)
                        {
                            RecordInfo ri = new RecordInfo();
                            ri.Offset = fs.Position;
                            fs.Read(memBuffer, 0, lsize + 1);
                            ms.Position = 0;
                            ri.Length = br.ReadInt64();
                            if (memBuffer[lsize] == 0)
                            {
                                //free
                                ri.FreeSlot = true;
                                ri.ID = string.Concat("_", ri.Offset.ToString());
                                fs.Position = ri.Offset + ri.Length;
                            }
                            else
                            {
                                //read
                                ri.FreeSlot = false;
                                Framework.Data.LogImage li = new Framework.Data.LogImage();

                                fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                ms.Position = 0;
                                li.ID = br.ReadString();
                                ri.ID = li.ID;

                                li.DataFromDate = DateTime.Parse(br.ReadString());
                                li.LogID = br.ReadString();
                                li.Name = br.ReadString();
                                li.Url = br.ReadString();

                                li.Saved = true;
                                li.IsDataChanged = false;
                                logimgs.Add(li);

                                index++;
                                procStep++;
                                if (procStep >= 2000)
                                {
                                    UpdateLoadingInBackgroundProgress(STR_LOADING_LOGIMAGES_BG, logimgCount, index);
                                    procStep = 0;
                                }
                            }
                            _fileCollection._logimgsInDB.Add(ri.ID, ri);
                        }
                    }
                }
            }
            return result;
        }
        public override bool LoadGeocacheImages(List<Framework.Data.GeocacheImage> geocacheimgs)
        {
            bool result = true;
            int index = 0;
            int procStep = 0;

            if (File.Exists(_fileCollection.DatabaseInfoFilename))
            {

                int lsize = sizeof(long);
                byte[] memBuffer = new byte[5 * 1024];
                using (MemoryStream ms = new MemoryStream(memBuffer))
                using (BinaryReader br = new BinaryReader(ms))
                {

                    int geocacheimgCount = 0;

                    XmlDocument doc = new XmlDocument();
                    doc.Load(_fileCollection.DatabaseInfoFilename);
                    XmlElement root = doc.DocumentElement;
                    if (root.SelectSingleNode("GeocacheImagesCount") != null)
                    {
                        geocacheimgCount = int.Parse(root.SelectSingleNode("GeocacheImagesCount").InnerText);
                    }

                    using (FileStream fs = File.Open(_fileCollection.GeocacheImagesFilename, FileMode.OpenOrCreate, FileAccess.Read))
                    {
                        fs.Position = 0;
                        long eof = fs.Length;
                        while (fs.Position < eof)
                        {
                            RecordInfo ri = new RecordInfo();
                            ri.Offset = fs.Position;
                            fs.Read(memBuffer, 0, lsize + 1);
                            ms.Position = 0;
                            ri.Length = br.ReadInt64();
                            if (memBuffer[lsize] == 0)
                            {
                                //free
                                ri.FreeSlot = true;
                                ri.ID = string.Concat("_", ri.Offset.ToString());
                                fs.Position = ri.Offset + ri.Length;
                            }
                            else
                            {
                                //read
                                ri.FreeSlot = false;
                                Framework.Data.GeocacheImage li = new Framework.Data.GeocacheImage();

                                fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                ms.Position = 0;
                                li.ID = br.ReadString();
                                ri.ID = li.ID;

                                li.DataFromDate = DateTime.Parse(br.ReadString());
                                li.GeocacheCode = br.ReadString();
                                li.Description = br.ReadString();
                                li.Name = br.ReadString();
                                li.Url = br.ReadString();
                                li.MobileUrl = br.ReadString();
                                li.ThumbUrl = br.ReadString();

                                li.Saved = true;
                                li.IsDataChanged = false;
                                geocacheimgs.Add(li);

                                index++;
                                procStep++;
                                if (procStep >= 2000)
                                {
                                    UpdateLoadingInBackgroundProgress(STR_LOADING_GEOCACHEIMAGES_BG, geocacheimgCount, index);
                                    procStep = 0;
                                }
                            }
                            _fileCollection._geocacheimgsInDB.Add(ri.ID, ri);
                        }
                    }
                }
            }
            return result;
        }




        public override bool Save()
        {
            return Save(_fileCollection, false);
        }
        public bool Save(FileCollection fc, bool forceFullData)
        {
            bool result = true;
            using (Utils.ProgressBlock fixpr = new Utils.ProgressBlock(this, STR_SAVING, STR_SAVINGDATA, 1, 0))
            {
                byte[] memBuffer = new byte[10 * 1024 * 1024];
                byte isFree = 0;
                byte notFree = 1;
                byte notFreeF = 2;
                using (MemoryStream ms = new MemoryStream(memBuffer))
                using (BinaryWriter bw = new BinaryWriter(ms))
                {

                    //**********************************************
                    //fc.DatabaseInfoFilename
                    //**********************************************
                    XmlDocument doc = new XmlDocument();
                    XmlElement root = doc.CreateElement("info");
                    doc.AppendChild(root);

                    XmlElement el = doc.CreateElement("IsLittleEndian");
                    XmlText txt = doc.CreateTextNode(BitConverter.IsLittleEndian.ToString());
                    el.AppendChild(txt);
                    root.AppendChild(el);

                    el = doc.CreateElement("GAPPVersion");
                    txt = doc.CreateTextNode(Core.Version.ToString());
                    el.AppendChild(txt);
                    root.AppendChild(el);

                    el = doc.CreateElement("StorageVersion");
                    txt = doc.CreateTextNode("1");
                    el.AppendChild(txt);
                    root.AppendChild(el);

                    el = doc.CreateElement("GeocacheCount");
                    txt = doc.CreateTextNode(Core.Geocaches.Count.ToString());
                    el.AppendChild(txt);
                    root.AppendChild(el);

                    el = doc.CreateElement("LogCount");
                    txt = doc.CreateTextNode(Core.Logs.Count.ToString());
                    el.AppendChild(txt);
                    root.AppendChild(el);

                    el = doc.CreateElement("LogImagesCount");
                    txt = doc.CreateTextNode(Core.LogImages.Count.ToString());
                    el.AppendChild(txt);
                    root.AppendChild(el);

                    el = doc.CreateElement("WaypointCount");
                    txt = doc.CreateTextNode(Core.Waypoints.Count.ToString());
                    el.AppendChild(txt);
                    root.AppendChild(el);

                    el = doc.CreateElement("UserWaypointCount");
                    txt = doc.CreateTextNode(Core.UserWaypoints.Count.ToString());
                    el.AppendChild(txt);
                    root.AppendChild(el);

                    el = doc.CreateElement("GeocacheImagesCount");
                    txt = doc.CreateTextNode(Core.GeocacheImages.Count.ToString());
                    el.AppendChild(txt);
                    root.AppendChild(el);

                    doc.Save(fc.DatabaseInfoFilename);
                    //**********************************************
                    //**********************************************

                    //**********************************************
                    //          GEOCACHES
                    //**********************************************

                    //delete geocaches that are not in the list anymore.
                    List<RecordInfo> deletedRecords = (from RecordInfo ri in fc._geocachesInDB.Values where !ri.FreeSlot && ri.ID[0] != 'F' && Core.Geocaches.GetGeocache(ri.ID) == null select ri).ToList();
                    List<RecordInfo> deletedFRecords = new List<RecordInfo>();
                    foreach (RecordInfo ri in deletedRecords)
                    {
                        string id = ri.ID;

                        //mark current record as free (change id)
                        fc._geocachesInDB.Remove(ri.ID);
                        ri.ID = string.Concat("_", ri.ID);
                        ri.FreeSlot = true;
                        fc._geocachesInDB.Add(ri.ID, ri);

                        //scratch file to mark it as free
                        fc._fsGeocaches.Position = ri.Offset + sizeof(long);
                        fc._fsGeocaches.WriteByte(isFree);

                        //get the F_ record too
                        RecordInfo fri = fc._geocachesInDB[string.Concat("F_", id)] as RecordInfo;
                        if (fri != null && !fri.FreeSlot)
                        {
                            //mark current record as free (change id)
                            fc._geocachesInDB.Remove(fri.ID);
                            fri.ID = string.Concat("_", fri.ID);
                            fri.FreeSlot = true;
                            fc._geocachesInDB.Add(fri.ID, fri);

                            //scratch file to mark it as free
                            fc._fsGeocaches.Position = fri.Offset + sizeof(long);
                            fc._fsGeocaches.WriteByte(isFree);

                            deletedFRecords.Add(fri);
                        }
                    }
                    deletedRecords.AddRange(deletedFRecords);

                    //now get all the selected and data changed geocaches
                    List<Framework.Data.Geocache> gclist = (from Framework.Data.Geocache wp in Core.Geocaches
                                                            where !wp.Saved
                                                            select wp).ToList();
                    if (gclist.Count > 0)
                    {
                        using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_SAVING, STR_SAVINGGEOCACHES, gclist.Count, 0))
                        {
                            //fix block > ID = GC12345
                            //fulldata > ID = F_GC12345

                            long recordLength = 0;
                            byte[] extraBuffer = new byte[200];
                            List<RecordInfo> freeRecords = (from RecordInfo ri in fc._geocachesInDB.Values where ri.FreeSlot select ri).OrderByDescending(x=>x.Length).ToList();

                            int index = 0;
                            int procStep = 0;
                            foreach (Framework.Data.Geocache gc in gclist)
                            {
                                //write to block
                                ms.Position = 0;

                                //block header
                                bw.Write(recordLength); //overwrite afterwards
                                bw.Write(notFree);
                                bw.Write(gc.Code);

                                bw.Write(gc.Archived);
                                WriteIntegerArray(bw, gc.AttributeIds);
                                bw.Write(gc.Available);
                                bw.Write(gc.City ?? "");
                                bw.Write(gc.Container.ID);
                                bw.Write(gc.CustomCoords);
                                bw.Write(gc.Country ?? "");
                                bw.Write(gc.ContainsCustomLatLon);
                                if (gc.ContainsCustomLatLon)
                                {
                                    bw.Write((double)gc.CustomLat);
                                    bw.Write((double)gc.CustomLon);
                                }
                                bw.Write(gc.Difficulty);
                                bw.Write(gc.EncodedHints ?? "");
                                bw.Write(gc.Favorites);
                                bw.Write(gc.Flagged);
                                bw.Write(gc.Found);
                                bw.Write(gc.GeocacheType.ID);
                                bw.Write(gc.ID ?? "");
                                bw.Write(gc.Lat);
                                bw.Write(gc.Lon);
                                bw.Write(gc.MemberOnly);
                                bw.Write(gc.Municipality ?? "");
                                bw.Write(gc.Name ?? "");
                                bw.Write(gc.Notes ?? "");
                                bw.Write(gc.Owner ?? "");
                                bw.Write(gc.OwnerId ?? "");
                                bw.Write(gc.PersonaleNote ?? "");
                                bw.Write(gc.PlacedBy ?? "");
                                bw.Write(((DateTime)gc.PublishedTime).ToString("s"));
                                bw.Write(gc.State ?? "");
                                bw.Write(gc.Terrain);
                                bw.Write(gc.Title ?? "");
                                bw.Write(gc.Url ?? "");
                                bw.Write(gc.DataFromDate.ToString("s"));
                                bw.Write(gc.Locked);

                                writeRecord(fc._geocachesInDB, gc.Code, ms, bw, fc._fsGeocaches, memBuffer, extraBuffer, freeRecords);

                                //other record
                                if (forceFullData || gc.FullDataLoaded)
                                {
                                    string id = string.Concat("F_", gc.Code);
                                    //write to block
                                    ms.Position = 0;

                                    //block header
                                    bw.Write(recordLength); //overwrite afterwards
                                    bw.Write(notFreeF);
                                    bw.Write(id);

                                    bw.Write(gc.ShortDescription ?? "");
                                    bw.Write(gc.ShortDescriptionInHtml);
                                    bw.Write(gc.LongDescription ?? "");
                                    bw.Write(gc.LongDescriptionInHtml);

                                    writeRecord(fc._geocachesInDB, id, ms, bw, fc._fsGeocaches, memBuffer, extraBuffer, freeRecords);
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
                        fc._fsGeocaches.Flush();
                    }

                    //**********************************************
                    //          LOGS
                    //**********************************************

                    //delete geocaches that are not in the list anymore.
                    deletedRecords = (from RecordInfo ri in fc._logsInDB.Values where !ri.FreeSlot && ri.ID[0] != 'F' && Core.Logs.GetLog(ri.ID) == null select ri).ToList();
                    deletedFRecords.Clear();
                    foreach (RecordInfo ri in deletedRecords)
                    {
                        string id = ri.ID;

                        //mark current record as free (change id)
                        fc._logsInDB.Remove(ri.ID);
                        ri.ID = string.Concat("_", ri.ID);
                        ri.FreeSlot = true;
                        fc._logsInDB.Add(ri.ID, ri);

                        //scratch file to mark it as free
                        fc._fsLogs.Position = ri.Offset + sizeof(long);
                        fc._fsLogs.WriteByte(isFree);

                        //get the F_ record too
                        RecordInfo fri = fc._logsInDB[string.Concat("F_", id)] as RecordInfo;
                        if (fri != null && !fri.FreeSlot)
                        {
                            //mark current record as free (change id)
                            fc._logsInDB.Remove(fri.ID);
                            fri.ID = string.Concat("_", fri.ID);
                            fri.FreeSlot = true;
                            fc._logsInDB.Add(fri.ID, fri);

                            //scratch file to mark it as free
                            fc._fsLogs.Position = fri.Offset + sizeof(long);
                            fc._fsLogs.WriteByte(isFree);

                            deletedFRecords.Add(fri);
                        }
                    }
                    deletedRecords.AddRange(deletedFRecords);

                    List<Framework.Data.Log> lglist = (from Framework.Data.Log wp in Core.Logs
                                                       where !wp.Saved
                                                       select wp).ToList();
                    if (lglist.Count > 0)
                    {
                        List<RecordInfo> freeRecords = (from RecordInfo ri in fc._logsInDB.Values where ri.FreeSlot select ri).OrderByDescending(x => x.Length).ToList();

                        int index = 0;
                        int procStep = 0;
                        using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_SAVING, STR_SAVINGLOGS, lglist.Count, 0))
                        {
                            long recordLength = 0;
                            byte[] extraBuffer = new byte[50];
                            foreach (Framework.Data.Log l in lglist)
                            {
                                //write to block
                                ms.Position = 0;

                                //block header
                                bw.Write(recordLength); //overwrite afterwards
                                bw.Write(notFree);
                                bw.Write(l.ID);

                                bw.Write(l.DataFromDate.ToString("s"));
                                bw.Write(l.Date.ToString("s"));
                                bw.Write(l.Finder ?? "");
                                bw.Write(l.GeocacheCode ?? "");
                                bw.Write(l.ID);
                                bw.Write(l.LogType.ID);

                                writeRecord(fc._logsInDB, l.ID, ms, bw, fc._fsLogs, memBuffer, extraBuffer, freeRecords);

                                if (forceFullData || l.FullDataLoaded)
                                {
                                    string id = string.Concat("F_", l.ID);
                                    //write to block
                                    ms.Position = 0;

                                    //block header
                                    bw.Write(recordLength); //overwrite afterwards
                                    bw.Write(notFreeF);
                                    bw.Write(id);

                                    bw.Write(l.TBCode ?? "");
                                    bw.Write(l.FinderId ?? "");
                                    bw.Write(l.Text ?? "");
                                    bw.Write(l.Encoded);

                                    writeRecord(fc._logsInDB, id, ms, bw, fc._fsLogs, memBuffer, extraBuffer, freeRecords);
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
                        fc._fsLogs.Flush();
                    }

                    //**********************************************
                    //          WAYPOINTS
                    //**********************************************

                    using (FileStream fs = File.Open(fc.WaypointsFilename, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        //delete geocaches that are not in the list anymore.
                        deletedRecords = (from RecordInfo ri in fc._wptsInDB.Values where !ri.FreeSlot && Core.Waypoints.getWaypoint(ri.ID) == null select ri).ToList();
                        foreach (RecordInfo ri in deletedRecords)
                        {
                            //mark current record as free (change id)
                            fc._wptsInDB.Remove(ri.ID);
                            ri.ID = string.Concat("_", ri.ID);
                            ri.FreeSlot = true;
                            fc._wptsInDB.Add(ri.ID, ri);

                            //scratch file to mark it as free
                            fs.Position = ri.Offset + sizeof(long);
                            fs.WriteByte(isFree);
                        }

                        List<Framework.Data.Waypoint> wptlist = (from Framework.Data.Waypoint wp in Core.Waypoints
                                                                 where !wp.Saved
                                                                 select wp).ToList();
                        if (wptlist.Count > 0)
                        {
                            List<RecordInfo> freeRecords = (from RecordInfo ri in fc._wptsInDB.Values where ri.FreeSlot select ri).OrderByDescending(x => x.Length).ToList();

                            int index = 0;
                            int procStep = 0;
                            using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_SAVING, STR_SAVINGWAYPOINTS, wptlist.Count, 0))
                            {
                                long recordLength = 0;
                                byte[] extraBuffer = new byte[10];
                                foreach (Framework.Data.Waypoint wp in wptlist)
                                {
                                    //write to block
                                    ms.Position = 0;

                                    //block header
                                    bw.Write(recordLength); //overwrite afterwards
                                    bw.Write(notFree);
                                    bw.Write(wp.Code);

                                    bw.Write(wp.Comment ?? "");
                                    bw.Write(wp.DataFromDate.ToString("s"));
                                    bw.Write(wp.Description ?? "");
                                    bw.Write(wp.GeocacheCode ?? "");
                                    bw.Write(wp.ID ?? "");
                                    if (wp.Lat == null || wp.Lon == null)
                                    {
                                        bw.Write(false);
                                    }
                                    else
                                    {
                                        bw.Write(true);
                                        bw.Write((double)wp.Lat);
                                        bw.Write((double)wp.Lon);
                                    }
                                    bw.Write(wp.Name ?? "");
                                    bw.Write(wp.Time.ToString("s"));
                                    bw.Write(wp.Url ?? "");
                                    bw.Write(wp.UrlName ?? "");
                                    bw.Write(wp.WPType.ID);

                                    writeRecord(fc._wptsInDB, wp.Code, ms, bw, fs, memBuffer, extraBuffer, freeRecords);

                                    wp.Saved = true;

                                    index++;
                                    procStep++;
                                    if (procStep >= 1000)
                                    {
                                        progress.UpdateProgress(STR_SAVING, STR_SAVINGWAYPOINTS, lglist.Count, index);
                                        procStep = 0;
                                    }
                                }
                            }
                        }
                        fs.Flush();
                    }

                    //**********************************************
                    //          LOGIMAGES
                    //**********************************************

                    using (FileStream fs = File.Open(fc.LogImagesFilename, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        //delete geocaches that are not in the list anymore.
                        deletedRecords = (from RecordInfo ri in fc._logimgsInDB.Values where !ri.FreeSlot && Core.LogImages.GetLogImage(ri.ID) == null select ri).ToList();
                        foreach (RecordInfo ri in deletedRecords)
                        {
                            //mark current record as free (change id)
                            fc._logimgsInDB.Remove(ri.ID);
                            ri.ID = string.Concat("_", ri.ID);
                            ri.FreeSlot = true;
                            fc._logimgsInDB.Add(ri.ID, ri);

                            //scratch file to mark it as free
                            fs.Position = ri.Offset + sizeof(long);
                            fs.WriteByte(isFree);
                        }

                        List<Framework.Data.LogImage> lgimglist = (from Framework.Data.LogImage wp in Core.LogImages
                                                                   where !wp.Saved
                                                                   select wp).ToList();
                        if (lgimglist.Count > 0)
                        {
                            List<RecordInfo> freeRecords = (from RecordInfo ri in fc._logimgsInDB.Values where ri.FreeSlot select ri).OrderByDescending(x => x.Length).ToList();

                            int index = 0;
                            int procStep = 0;
                            using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_SAVING, STR_SAVINGLOGIMAGES, lgimglist.Count, 0))
                            {
                                long recordLength = 0;
                                byte[] extraBuffer = new byte[10];
                                foreach (Framework.Data.LogImage li in lgimglist)
                                {
                                    //write to block
                                    ms.Position = 0;

                                    //block header
                                    bw.Write(recordLength); //overwrite afterwards
                                    bw.Write(notFree);
                                    bw.Write(li.ID);
                                    bw.Write(li.DataFromDate.ToString("s"));
                                    bw.Write(li.LogID ?? "");
                                    bw.Write(li.Name ?? "");
                                    bw.Write(li.Url ?? "");

                                    writeRecord(fc._logimgsInDB, li.ID, ms, bw, fs, memBuffer, extraBuffer, freeRecords);

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
                        fs.Flush();
                    }

                    //**********************************************
                    //          GEOCACHEIMAGES
                    //**********************************************

                    using (FileStream fs = File.Open(fc.GeocacheImagesFilename, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        //delete geocaches that are not in the list anymore.
                        deletedRecords = (from RecordInfo ri in fc._geocacheimgsInDB.Values where !ri.FreeSlot && Core.GeocacheImages.GetGeocacheImage(ri.ID) == null select ri).ToList();
                        foreach (RecordInfo ri in deletedRecords)
                        {
                            //mark current record as free (change id)
                            fc._geocacheimgsInDB.Remove(ri.ID);
                            ri.ID = string.Concat("_", ri.ID);
                            ri.FreeSlot = true;
                            fc._geocacheimgsInDB.Add(ri.ID, ri);

                            //scratch file to mark it as free
                            fs.Position = ri.Offset + sizeof(long);
                            fs.WriteByte(isFree);
                        }

                        List<Framework.Data.GeocacheImage> lgimglist = (from Framework.Data.GeocacheImage wp in Core.GeocacheImages
                                                                   where !wp.Saved
                                                                   select wp).ToList();
                        if (lgimglist.Count > 0)
                        {
                            List<RecordInfo> freeRecords = (from RecordInfo ri in fc._geocacheimgsInDB.Values where ri.FreeSlot select ri).OrderByDescending(x => x.Length).ToList();

                            int index = 0;
                            int procStep = 0;
                            using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_SAVING, STR_SAVINGGEOCACHEIMAGES, lgimglist.Count, 0))
                            {
                                long recordLength = 0;
                                byte[] extraBuffer = new byte[100];
                                foreach (Framework.Data.GeocacheImage li in lgimglist)
                                {
                                    //write to block
                                    ms.Position = 0;

                                    //block header
                                    bw.Write(recordLength); //overwrite afterwards
                                    bw.Write(notFree);
                                    bw.Write(li.ID);
                                    bw.Write(li.DataFromDate.ToString("s"));
                                    bw.Write(li.GeocacheCode ?? "");
                                    bw.Write(li.Description ?? "");
                                    bw.Write(li.Name ?? "");
                                    bw.Write(li.Url ?? "");
                                    bw.Write(li.MobileUrl ?? "");
                                    bw.Write(li.ThumbUrl ?? "");

                                    writeRecord(fc._geocacheimgsInDB, li.ID, ms, bw, fs, memBuffer, extraBuffer, freeRecords);

                                    li.Saved = true;

                                    index++;
                                    procStep++;
                                    if (procStep >= 1000)
                                    {
                                        progress.UpdateProgress(STR_SAVING, STR_SAVINGGEOCACHEIMAGES, lgimglist.Count, index);
                                        procStep = 0;
                                    }
                                }
                            }
                        }
                        fs.Flush();
                    }

                    //**********************************************
                    //          USER WAYPOINTS
                    //**********************************************

                    using (FileStream fs = File.Open(fc.UserWaypointsFilename, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        //delete geocaches that are not in the list anymore.
                        deletedRecords = (from RecordInfo ri in fc._usrwptsInDB.Values where !ri.FreeSlot && Core.UserWaypoints.getWaypoint(int.Parse(ri.ID)) == null select ri).ToList();
                        foreach (RecordInfo ri in deletedRecords)
                        {
                            //mark current record as free (change id)
                            fc._usrwptsInDB.Remove(ri.ID);
                            ri.ID = string.Concat("_", ri.ID);
                            ri.FreeSlot = true;
                            fc._usrwptsInDB.Add(ri.ID, ri);

                            //scratch file to mark it as free
                            fs.Position = ri.Offset + sizeof(long);
                            fs.WriteByte(isFree);
                        }

                        List<Framework.Data.UserWaypoint> usrwptlist = (from Framework.Data.UserWaypoint wp in Core.UserWaypoints
                                                                        where !wp.Saved
                                                                        select wp).ToList();
                        if (usrwptlist.Count > 0)
                        {
                            List<RecordInfo> freeRecords = (from RecordInfo ri in fc._usrwptsInDB.Values where ri.FreeSlot select ri).OrderByDescending(x => x.Length).ToList();

                            long recordLength = 0;
                            byte[] extraBuffer = new byte[10];
                            foreach (Framework.Data.UserWaypoint wp in usrwptlist)
                            {
                                //write to block
                                ms.Position = 0;

                                //block header
                                bw.Write(recordLength); //overwrite afterwards
                                bw.Write(notFree);
                                bw.Write(wp.ID.ToString());
                                bw.Write(wp.Description ?? "");
                                bw.Write(wp.GeocacheCode ?? "");
                                bw.Write(wp.Lat);
                                bw.Write(wp.Lon);
                                bw.Write(wp.Date.ToString("s"));

                                writeRecord(fc._usrwptsInDB, wp.ID.ToString(), ms, bw, fs, memBuffer, extraBuffer, freeRecords);

                                wp.Saved = true;

                            }

                        }
                        fs.Flush();
                    }


                }
            }
            return result;
        }

        private void writeRecord(Hashtable ht, string id, MemoryStream ms, BinaryWriter bw, FileStream fs, byte[] memBuffer, byte[] extraBuffer, List<RecordInfo> freeRecords)
        {
            //look for record
            long recordLength = ms.Position;
            int lsize = sizeof(long);
            byte isFree = 0;
            bool findFreeSlot = false;
            RecordInfo ri = ht[id] as RecordInfo;
            if (ri == null)
            {
                //find free slot
                findFreeSlot = true;
            }
            else if (ri.Length >= recordLength)
            {
                //length stays the same
                fs.Position = ri.Offset + lsize;
                fs.Write(memBuffer, lsize, (int)recordLength - lsize);
            }
            else
            {
                //current is too small
                //mark current record als free (change id)
                ht.Remove(ri.ID);
                ri.ID = string.Concat("_", ri.ID);
                ri.FreeSlot = true;
                ht.Add(ri.ID, ri);

                //scratch file to mark it as free
                fs.Position = ri.Offset + lsize;
                fs.WriteByte(isFree);

                //find free slot
                findFreeSlot = true;
            }
            if (findFreeSlot)
            {
                //get list of free slots before saving (saves time, otherwise going through the list too often
                //ri = (from r in freeRecords where r.Length >= recordLength select r).FirstOrDefault();
                //assume sorted from large to small
                if (freeRecords.Count > 0 && freeRecords[0].Length >= recordLength)
                {
                    ri = freeRecords[0];
                }
                else
                {
                    ri = null;
                }
                if (ri != null)
                {
                    freeRecords.Remove(ri);

                    ht.Remove(ri.ID);
                    ri.ID = id;
                    ri.FreeSlot = false;
                    ht.Add(ri.ID, ri);

                    //insert new record
                    fs.Position = ri.Offset + lsize;
                    fs.Write(memBuffer, lsize, (int)recordLength - lsize);
                }
                else
                {
                    //add
                    bw.Write(extraBuffer);
                    recordLength = ms.Position;
                    ms.Seek(0, SeekOrigin.Begin);
                    bw.Write(recordLength); //overwrite afterwards

                    ri = new RecordInfo();
                    ri.FreeSlot = false;
                    ri.ID = id;
                    ri.Length = recordLength;
                    ri.Offset = fs.Length;
                    ht[id] = ri;

                    fs.Position = fs.Length;
                    fs.Write(memBuffer, 0, (int)recordLength);
                }
            }
        }

        public void WriteIntegerArray(BinaryWriter fs, List<int> values)
        {
            fs.Write(values.Count);
            foreach (int i in values)
            {
                fs.Write(i);
            }
        }

        public List<int> ReadIntegerArray(BinaryReader fs)
        {
            List<int> result = new List<int>();
            int count = fs.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                result.Add(fs.ReadInt32());
            }
            return result;
        }

    }
}
