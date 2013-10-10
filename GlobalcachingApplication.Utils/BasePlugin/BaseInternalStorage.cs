using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace GlobalcachingApplication.Utils.BasePlugin
{
    public class BaseInternalStorage: Plugin, Framework.Interfaces.IPluginInternalStorage
    {
        protected const string ACTION_SAVE = "Save";
        protected const string ACTION_LOAD = "Load";
        protected const string ACTION_SAVEAS = "Save as";
        protected const string ACTION_OPEN = "Open";
        protected const string ACTION_NEW = "New";
        protected const string ACTION_BACKUP = "Backup";
        protected const string ACTION_RESTORE = "Restore";
        protected const string ACTION_REPAIR = "Repair active database";
        protected const string ACTION_RECENT = "Recent databases|tmp";
        protected const string ACTION_COPYTONEWSELECTED = "Copy to new database|Selected";
        protected const string ACTION_COPYTONEWACTIVE = "Copy to new database|Active";
        protected const string ACTION_COPYTOEXISTINGSELECTED = "Copy to existing database|Selected";
        protected const string ACTION_COPYTOEXISTINGACTIVE = "Copy to existing database|Active";
        protected const string ACTION_INSERTWITHOVERWRITE = "Insert from database|Overwrite existing";
        protected const string ACTION_INSERTONLYMISSING = "Insert from database|Do not overwrite existing";

        protected const string STR_LOADING_LOGS_BG = "Loading logs...";
        protected const string STR_LOADING_WAYPOINTS_BG = "Loading waypoints...";
        protected const string STR_LOADING_LOGIMAGES_BG = "Loading logimages...";
        protected const string STR_LOADING_GEOCACHEIMAGES_BG = "Loading geocache images...";
        protected const string STR_LOADING_DATAINBACKGROUND_PROCESS = "Loading data in background. Please wait till this is finished.";
        protected const string STR_ERROR = "Error";
        protected const string STR_ASKSAVEDATA = "The data has been changed. Do you want to save the data first?";
        protected const string STR_WARNING = "Warning";
        protected const string STR_DATABASEERROR = "Database error";
        protected const string STR_NOGEOCACHESELECTED = "No geocache selected for export";
        protected const string STR_BACKUPFAILED = "Backup failed";

        private ManualResetEvent _actionReady = new ManualResetEvent(false);
        private volatile bool _actionResult = false;
        private bool _dataLoaded = false;
        private bool _databaseError = false;

        private bool _cachesEmpty;
        private bool _logsEmpty;
        private bool _wpsEmpty;
        private bool _usrwpsEmpty;
        private bool _logImgsEmpty;
        private bool _geocacheImgsEmpty;

        //loading in background stuff
        private SynchronizationContext _context = null;
        private volatile bool _loadingInBackgroundActive = false;

        public event Framework.EventArguments.PluginEventHandler DataSourceNameChanged;
        private string _dataSourceName = "";
        private BaseInternalStorageBackgroundMessage _notificationMessage = null;
        private BaseInternalStorageRepairMessage _repairMessage = null;
        private List<Framework.Data.InternalStorageDestination> _recentDatabases = null;
        private string _mostRecentFile = null;

        protected List<Framework.Data.Geocache> CopyToList { get; private set; }

        public BaseInternalStorage()
        {
        }

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            _recentDatabases = new List<Framework.Data.InternalStorageDestination>();

            _context = SynchronizationContext.Current;
            if (_context == null)
            {
                _context = new SynchronizationContext();
            }

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LOADING_LOGS_BG));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LOADING_WAYPOINTS_BG));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LOADING_LOGIMAGES_BG));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LOADING_GEOCACHEIMAGES_BG));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LOADING_DATAINBACKGROUND_PROCESS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ASKSAVEDATA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_WARNING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ACTION_REPAIR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_DATABASEERROR));

            List<string> availableActions = new List<string>();

            availableActions.AddRange(new string[] { ACTION_NEW, ACTION_OPEN, ACTION_SAVE, ACTION_SAVEAS });
            if (SupportsRepairActiveDatabase)
            {
                //availableActions.AddRange(new string[] { ACTION_REPAIR });
            }
            if (SupportsBackupRestoreDatabase)
            {
                availableActions.AddRange(new string[] { ACTION_BACKUP, ACTION_RESTORE });
            }
            if (SupportsCopyToDatabase)
            {
                availableActions.AddRange( new string[] { ACTION_COPYTONEWSELECTED, ACTION_COPYTONEWACTIVE, ACTION_COPYTOEXISTINGSELECTED, ACTION_COPYTOEXISTINGACTIVE });
            }
            if (SupportsInsertFromDatabase)
            {
                availableActions.AddRange(new string[] { ACTION_INSERTWITHOVERWRITE, ACTION_INSERTONLYMISSING });
            }
            availableActions.Add(ACTION_RECENT);

            return base.Initialize(core, availableActions.ToArray());
        }

        public override void ApplicationInitialized()
        {
            base.ApplicationInitialized();

            Framework.Data.InternalStorageDestination curDatabase = ActiveStorageDestination;
            if (curDatabase != null)
            {
                _recentDatabases.Add(curDatabase);
            }
            loadMostRecentDatabases();
            SetMostRecentDatabasesMenu();
        }

        public virtual void StartReleaseForCopy()
        {
        }

        public virtual void EndReleaseForCopy()
        {
        }


        private void loadMostRecentDatabases()
        {
            try
            {
                _mostRecentFile = System.IO.Path.Combine(Core.PluginDataPath, string.Format("{0}.xml",this.GetType().ToString().Replace(".","")));

                if (System.IO.File.Exists(_mostRecentFile))
                {
                    Framework.Data.InternalStorageDestination curDatabase = null;
                    if (_recentDatabases.Count > 0)
                    {
                        curDatabase = _recentDatabases[0];
                    }

                    XmlDocument doc = new XmlDocument();
                    doc.Load(_mostRecentFile);
                    XmlElement root = doc.DocumentElement;

                    XmlNodeList bmNodes = root.SelectNodes("recent");
                    if (bmNodes != null)
                    {
                        foreach (XmlNode n in bmNodes)
                        {
                            Framework.Data.InternalStorageDestination bm = new Framework.Data.InternalStorageDestination();
                            bm.Name = n.SelectSingleNode("Name").InnerText;
                            bm.PluginType = n.SelectSingleNode("PluginType").InnerText;
                            XmlNodeList cNodes = n.SelectSingleNode("infos").SelectNodes("info");
                            if (cNodes != null)
                            {
                                bm.StorageInfo = new string[cNodes.Count];
                                for (int i = 0; i < cNodes.Count; i++)
                                {
                                    bm.StorageInfo[i] = cNodes[i].InnerText;
                                }
                            }
                            if (curDatabase == null || !curDatabase.SameDestination(bm))
                            {
                                _recentDatabases.Add(bm);
                            }
                        }
                    }
                }

            }
            catch
            {
            }
        }

        private void saveMostRecentDatabases()
        {
            //set current to number 1
            Framework.Data.InternalStorageDestination curDatabase = ActiveStorageDestination;
            if (curDatabase != null)
            {
                _recentDatabases.Insert(0, curDatabase);

                //filter out current in list after 1
                int i = 1;
                while (i < _recentDatabases.Count)
                {
                    if (curDatabase.SameDestination(_recentDatabases[i]))
                    {
                        _recentDatabases.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }

                //max 9
                while (_recentDatabases.Count > 9)
                {
                    _recentDatabases.RemoveAt(_recentDatabases.Count - 1);
                }
            }

            //save
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("mostrecent");
                doc.AppendChild(root);
                foreach (Framework.Data.InternalStorageDestination bmi in _recentDatabases)
                {
                    XmlElement bm = doc.CreateElement("recent");
                    root.AppendChild(bm);

                    XmlElement el = doc.CreateElement("Name");
                    XmlText txt = doc.CreateTextNode(bmi.Name);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("PluginType");
                    txt = doc.CreateTextNode(bmi.PluginType);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("infos");
                    bm.AppendChild(el);

                    if (bmi.StorageInfo != null)
                    {
                        foreach (string c in bmi.StorageInfo)
                        {
                            XmlElement cel = doc.CreateElement("info");
                            txt = doc.CreateTextNode(c);
                            cel.AppendChild(txt);
                            el.AppendChild(cel);
                        }
                    }
                }
                doc.Save(_mostRecentFile);
            }
            catch
            {
            }

            //rebuild menu according new order
            SetMostRecentDatabasesMenu();
        }

        public void SetMostRecentDatabasesMenu()
        {
            Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();

            //delete all current
            List<string> cur = GetActionSubactionList('|');
            foreach (string s in cur)
            {
                string[] parts = s.Split(new char[] { '|' });
                if (parts.Length == 2)
                {
                    if (parts[0] == "Recent databases")
                    {
                        RemoveAction(string.Format("{0}|{1}", parts[0], parts[1]));
                        main.RemoveAction(this, parts[0], parts[1]);
                    }
                }
            }

            //add current
            int index = 1;
            foreach (Framework.Data.InternalStorageDestination curDatabase in _recentDatabases)
            {
                AddAction(string.Format("Recent databases|{0} {1}", index, curDatabase.Name));
                main.AddAction(this, "Recent databases", string.Format("{0} {1}", index, curDatabase.Name));
                index++;
            }
        }

        public string DataSourceName 
        {
            get { return _dataSourceName; } 
        }

        public virtual Framework.Data.InternalStorageDestination ActiveStorageDestination { get { return null; } }

        public bool SetStorageDestination(Framework.Data.InternalStorageDestination dst)
        {
            bool result = false;
            if (CheckBackgroundTaskNotRunning())
            {
                if (dst != null)
                {
                    if (checkDataSaved())
                    {
                        if (PrepareSetStorageDestination(dst))
                        {
                            ExecuteInBackground(new ThreadStart(this.OpenMethod));
                            result = _actionResult;
                            saveMostRecentDatabases();
                        }
                    }
                }
            }
            return result;
        }

        protected virtual bool PrepareCopyToNew()
        {
            return false;
        }
        private void CopyToNew()
        {
            try
            {
                _actionReady.Reset();
                _actionResult = false;
                Thread thrd = new Thread(copyToNewThreadMethod);
                thrd.Start();
                while (!_actionReady.WaitOne(50))
                {
                    System.Windows.Forms.Application.DoEvents();
                }
                thrd.Join();
            }
            catch
            {
            }
        }
        private void copyToNewThreadMethod()
        {
            try
            {
                CopyToNewMethod();
            }
            catch
            {
            }
            _actionReady.Set();
        }
        protected virtual void CopyToNewMethod()
        {
        }

        protected virtual bool PrepareCopyToExisting()
        {
            return false;
        }
        private void CopyToExisting()
        {
            try
            {
                _actionReady.Reset();
                _actionResult = false;
                Thread thrd = new Thread(copyToExistingThreadMethod);
                thrd.Start();
                while (!_actionReady.WaitOne(50))
                {
                    System.Windows.Forms.Application.DoEvents();
                }
                thrd.Join();
            }
            catch
            {
            }
        }
        private void copyToExistingThreadMethod()
        {
            try
            {
                CopyToExistingMethod();
            }
            catch
            {
            }
            _actionReady.Set();
        }
        protected virtual void CopyToExistingMethod()
        {
        }

        protected virtual bool PrepareSetStorageDestination(Framework.Data.InternalStorageDestination dst)
        {
            return false;
        }

        protected bool CheckBackgroundTaskNotRunning()
        {
            if (_loadingInBackgroundActive)
            {
                System.Windows.Forms.MessageBox.Show(LanguageSupport.Instance.GetTranslation(STR_LOADING_DATAINBACKGROUND_PROCESS), LanguageSupport.Instance.GetTranslation(STR_ERROR));
                return false;
            }
            else
            {
                return true;
            }
        }

        protected void SetDataSourceName(string srcName)
        {
            if (_dataSourceName != srcName)
            {
                _dataSourceName = srcName;
                OnDataSourceNameChanged();
            }
        }
        public void OnDataSourceNameChanged()
        {
            if (DataSourceNameChanged != null)
            {
                DataSourceNameChanged(this, new Framework.EventArguments.PluginEventArgs(this));
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SAVE;
            }
        }

        public override void Close()
        {
            if (_actionReady != null)
            {
                _actionReady.Close();
                _actionReady = null;
            }
            base.Close();
        }

        protected virtual bool SupportsBackupRestoreDatabase
        {
            get { return false; }
        }

        protected virtual bool SupportsRepairActiveDatabase
        {
            get { return false; }
        }

        protected virtual bool SupportsInsertFromDatabase
        {
            get { return false; }
        }

        protected virtual bool SupportsCopyToDatabase
        {
            get { return false; }
        }

        protected virtual bool SupportsLoadingInBackground
        {
            get { return false; }
        }
        public bool LoadingInBackgroundActive
        {
            get { return _loadingInBackgroundActive; }
        }

        //On UI Context
        private void ExecuteInBackground(ThreadStart ts)
        {
            _cachesEmpty = Core.Geocaches.Count == 0;
            _logsEmpty = Core.Logs.Count == 0;
            _wpsEmpty = Core.Waypoints.Count == 0;
            _usrwpsEmpty = Core.UserWaypoints.Count == 0;
            _logImgsEmpty = Core.LogImages.Count == 0;
            _geocacheImgsEmpty = Core.GeocacheImages.Count == 0;
            using (Utils.FrameworkDataUpdater upd = new FrameworkDataUpdater(Core))
            {
                _actionReady.Reset();
                _actionResult = false;
                Thread thrd = new Thread(ts);
                thrd.IsBackground = true;
                thrd.Start();
                while (!_actionReady.WaitOne(50))
                {
                    System.Windows.Forms.Application.DoEvents();
                }
                thrd.Join();
            }
            _cachesEmpty = false;
            _logsEmpty = false;
            _wpsEmpty = false;
            _usrwpsEmpty = false;
            _logImgsEmpty = false;
            _geocacheImgsEmpty = false;
        }

        private bool checkDataSaved()
        {
            bool result = true;
            if (Core.AutoSaveOnClose)
            {
                result = SaveAllData();
            }
            else
            {
                int cnt = 0;
                cnt += (from Framework.Data.Geocache c in Core.Geocaches where !c.Saved select c).Count();
                cnt += (from Framework.Data.Log c in Core.Logs where !c.Saved select c).Count();
                cnt += (from Framework.Data.LogImage c in Core.LogImages where !c.Saved select c).Count();
                cnt += (from Framework.Data.Waypoint c in Core.Waypoints where !c.Saved select c).Count();
                if (cnt > 0)
                {
                    System.Windows.Forms.DialogResult res = MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_ASKSAVEDATA), Utils.LanguageSupport.Instance.GetTranslation(STR_WARNING), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button3);
                    if (res == System.Windows.Forms.DialogResult.Yes)
                    {

                        result = SaveAllData();
                    }
                    else if (res == System.Windows.Forms.DialogResult.No)
                    {
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            return result;
        }

        public bool SaveAllData()
        {
            if (CheckBackgroundTaskNotRunning())
            {
                ExecuteInBackground(new ThreadStart(this.SaveMethod));
                return _actionResult;
            }
            else
            {
                return false;
            }
        }

        public override bool ActionEnabled(string action, int selectCount, bool active)
        {
            bool result = base.ActionEnabled(action, selectCount, active);
            if (result)
            {
                result = !_loadingInBackgroundActive;
                if (result && action == ACTION_REPAIR)
                {
                    result = SupportsRepairActiveDatabase && _databaseError;
                }
            }
            return result;
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (CheckBackgroundTaskNotRunning())
                {
                    if (action == ACTION_NEW)
                    {
                        if (checkDataSaved())
                        {
                            if (PrepareNew())
                            {
                                DatabaseError = false;
                                ExecuteInBackground(new ThreadStart(this.NewMethod));
                                result = _actionResult;
                                saveMostRecentDatabases();
                            }
                        }
                    }
                    else if (action == ACTION_SAVEAS)
                    {
                        if (PrepareSaveAs())
                        {
                            DatabaseError = false;
                            ExecuteInBackground(new ThreadStart(this.SaveAsMethod));
                            result = _actionResult;
                            saveMostRecentDatabases();
                        }
                    }
                    else if (action == ACTION_OPEN)
                    {
                        if (checkDataSaved())
                        {
                            if (PrepareOpen())
                            {
                                DatabaseError = false;
                                ExecuteInBackground(new ThreadStart(this.OpenMethod));
                                result = _actionResult;
                                DatabaseError = !_actionResult;
                                saveMostRecentDatabases();
                            }
                        }
                    }
                    else if (action == ACTION_RESTORE)
                    {
                        if (checkDataSaved())
                        {
                            if (PrepareRestore())
                            {
                                DatabaseError = false;
                                ExecuteInBackground(new ThreadStart(this.RestoreMethod));
                                result = _actionResult;
                                DatabaseError = !_actionResult;
                                saveMostRecentDatabases();
                            }
                        }
                    }
                    else if (action == ACTION_BACKUP)
                    {
                        if (checkDataSaved())
                        {
                            if (PrepareBackup())
                            {
                                ExecuteInBackground(new ThreadStart(this.BackupMethod));
                            }
                            else
                            {
                                System.Windows.Forms.MessageBox.Show(LanguageSupport.Instance.GetTranslation(STR_BACKUPFAILED), LanguageSupport.Instance.GetTranslation(STR_ERROR));
                            }
                        }
                    }
                    else if (action == ACTION_REPAIR)
                    {
                        if (PrepareRepair())
                        {
                            DatabaseError = false;
                            ExecuteInBackground(new ThreadStart(this.RepairMethod));
                            result = _actionResult;
                            DatabaseError = !_actionResult;
                        }
                    }
                    else if (action == ACTION_INSERTONLYMISSING)
                    {
                        if (PrepareInsertFromDatabase())
                        {
                            ExecuteInBackground(new ThreadStart(this.InsertFromDatabaseOnlyNewMethod));
                            result = _actionResult;
                        }
                    }
                    else if (action == ACTION_INSERTWITHOVERWRITE)
                    {
                        if (PrepareInsertFromDatabase())
                        {
                            ExecuteInBackground(new ThreadStart(this.InsertFromDatabaseOverwriteMethod));
                            result = _actionResult;
                        }
                    }
                    else if (action == ACTION_COPYTOEXISTINGACTIVE)
                    {
                        if (Core.ActiveGeocache != null)
                        {
                            CopyToList = new List<Framework.Data.Geocache>();
                            CopyToList.Add(Core.ActiveGeocache);
                            if (PrepareCopyToExisting())
                            {
                                CopyToExisting();
                            }
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        }
                    }
                    else if (action == ACTION_COPYTOEXISTINGSELECTED)
                    {
                        CopyToList = DataAccess.GetSelectedGeocaches(Core.Geocaches);
                        if (CopyToList != null && CopyToList.Count > 0)
                        {
                            if (PrepareCopyToExisting())
                            {
                                CopyToExisting();
                            }
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        }
                    }
                    else if (action == ACTION_COPYTONEWACTIVE)
                    {
                        if (Core.ActiveGeocache != null)
                        {
                            CopyToList = new List<Framework.Data.Geocache>();
                            CopyToList.Add(Core.ActiveGeocache);
                            if (PrepareCopyToNew())
                            {
                                CopyToNew();
                            }
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        }
                    }
                    else if (action == ACTION_COPYTONEWSELECTED)
                    {
                        CopyToList = DataAccess.GetSelectedGeocaches(Core.Geocaches);
                        if (CopyToList != null && CopyToList.Count > 0)
                        {
                            if (PrepareCopyToNew())
                            {
                                CopyToNew();
                            }
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        }
                    }
                    else if (action == ACTION_SAVE)
                    {
                        ExecuteInBackground(new ThreadStart(this.SaveMethod));
                        result = _actionResult;
                        DatabaseError = !_actionResult;
                    }
                    else if (action == ACTION_LOAD)
                    {
                        DatabaseError = false;
                        ExecuteInBackground(new ThreadStart(this.LoadMethod));
                        result = _actionResult;
                        DatabaseError = !_actionResult;
                    }
                    else
                    {
                        string[] parts = action.Split(new char[] { '|' });
                        if (parts.Length == 2)
                        {
                            if (parts[0] == "Recent databases")
                            {
                                int index;
                                if (int.TryParse(parts[1].Substring(0, parts[1].IndexOf(' ')), out index))
                                {
                                    if (index > 0 && index <= _recentDatabases.Count)
                                    {
                                        DatabaseError = false;
                                        result = SetStorageDestination(_recentDatabases[index - 1]);
                                        DatabaseError = !_actionResult;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        private void SaveMethod()
        {
            try
            {
                _actionResult = Save();
            }
            catch
            {
            } 
            _actionReady.Set();
        }
        public virtual bool Save()
        {
            return true;
        }

        public virtual bool PrepareNew()
        {
            return true;
        }
        private void NewMethod()
        {
            try
            {
                _actionResult = NewFile();
            }
            catch
            {
            }
            _actionReady.Set();
        }
        public virtual bool NewFile()
        {
            return true;
        }

        public virtual bool PrepareSaveAs()
        {
            return true;
        }
        private void SaveAsMethod()
        {
            try
            {
                _actionResult = SaveAs();
            }
            catch
            {
            }
            _actionReady.Set();
        }
        public virtual bool SaveAs()
        {
            return true;
        }

        public virtual bool PrepareInsertFromDatabase()
        {
            return true;
        }
        private void InsertFromDatabaseOnlyNewMethod()
        {
            try
            {
                _actionResult = InsertFromDatabaseOnlyNew();
            }
            catch
            {
            }
            _actionReady.Set();
        }
        public virtual bool InsertFromDatabaseOnlyNew()
        {
            return true;
        }
        private void InsertFromDatabaseOverwriteMethod()
        {
            try
            {
                _actionResult = InsertFromDatabaseOverwrite();
            }
            catch
            {
            }
            _actionReady.Set();
        }
        public virtual bool InsertFromDatabaseOverwrite()
        {
            return true;
        }

        public virtual bool PrepareBackup()
        {
            return true;
        }
        private void BackupMethod()
        {
            try
            {
                _actionResult = Backup();
            }
            catch
            {
            }
            _actionReady.Set();
        }
        public virtual bool Backup()
        {
            return true;
        }

        private void LoadMethod()
        {
            try
            {
                _actionResult = Load(DoLoadInBackground);
                DatabaseError = !_actionResult;
                if (_actionResult && DoLoadInBackground)
                {
                    StartLoadingInBackground();
                }
            }
            catch
            {
                DatabaseError = true;
            }
            _actionReady.Set();
        }
        public virtual bool Load(bool geocachesOnly)
        {
            return true;
        }

        protected bool DatabaseError
        {
            get
            {
                return _databaseError;
            }
            set
            {
                _databaseError = value;
                if (_databaseError)
                {
                    if (_repairMessage == null)
                    {
                        _context.Send(new SendOrPostCallback(delegate(object state)
                        {
                            _repairMessage = new BaseInternalStorageRepairMessage();
                            OnNotification(_repairMessage);
                            _repairMessage.label1.Text = LanguageSupport.Instance.GetTranslation(STR_DATABASEERROR);
                            _repairMessage.button1.Text = LanguageSupport.Instance.GetTranslation(ACTION_REPAIR);
                            _repairMessage.button1.Enabled = SupportsRepairActiveDatabase;
                            _repairMessage.button1.Click += new EventHandler(button1_Click);
                        }), null);
                    }
                }
                else
                {
                    if (_repairMessage != null)
                    {
                        _context.Send(new SendOrPostCallback(delegate(object state)
                        {
                            _repairMessage.Visible = false;
                            _repairMessage = null;
                        }), null);
                    }
                }
            }
        }

        void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Action(ACTION_REPAIR);
            }
            catch
            {
            }
        }

        /*
         *  BEGIN
         *  LOADING IN BACKGROUND METHODS
         */
        private bool DoLoadInBackground
        {
            get { return Core.LoadLogsInBackground && SupportsLoadingInBackground; }
        }

        private void StartLoadingInBackground()
        {
            _context.Send(new SendOrPostCallback(delegate(object state)
            {
                _notificationMessage = new BaseInternalStorageBackgroundMessage();
                OnNotification(_notificationMessage);
                _notificationMessage.label1.Text = LanguageSupport.Instance.GetTranslation(STR_LOADING_LOGS_BG);
                Core.Logs.BeginUpdate();
                Core.UserWaypoints.BeginUpdate();
                Core.Waypoints.BeginUpdate();
                Core.LogImages.BeginUpdate();
                Core.GeocacheImages.BeginUpdate();
                _loadingInBackgroundActive = true;
            }), null);
            Thread thrd = new Thread(LoadInBackground);
            thrd.IsBackground = true;
            thrd.Start();
        }

        protected void UpdateLoadingInBackgroundProgress(string inf, int max, int pos)
        {
            _context.Post(new SendOrPostCallback(delegate(object state)
            {
                _notificationMessage.label1.Text = LanguageSupport.Instance.GetTranslation(inf);
                _notificationMessage.progressBar1.Value = Math.Min(Math.Min(pos, max), _notificationMessage.progressBar1.Maximum);
                _notificationMessage.progressBar1.Maximum = Math.Max(max, pos);
                _notificationMessage.label2.Text = max.ToString();
            }), null);
        }

        private void LoadInBackground()
        {
            List<Framework.Data.Log> logs = new List<Framework.Data.Log>();
            List<Framework.Data.Waypoint> wps = new List<Framework.Data.Waypoint>();
            List<Framework.Data.UserWaypoint> usrwps = new List<Framework.Data.UserWaypoint>();
            List<Framework.Data.LogImage> logimgs = new List<Framework.Data.LogImage>();
            List<Framework.Data.GeocacheImage> geocacheimgs = new List<Framework.Data.GeocacheImage>();
            try
            {
                LoadLogs(logs);
                Core.Logs.Add(logs);
                LoadWaypoints(wps, usrwps);
                Core.Waypoints.Add(wps);
                Core.UserWaypoints.Add(usrwps);
                LoadLogImages(logimgs);
                Core.LogImages.Add(logimgs);
                LoadGeocacheImages(geocacheimgs);
                Core.GeocacheImages.Add(geocacheimgs);
            }
            catch
            {
                DatabaseError = true;
            }
            _context.Post(new SendOrPostCallback(delegate(object state)
            {
                Core.GeocacheImages.EndUpdate();
                Core.LogImages.EndUpdate();
                Core.Waypoints.EndUpdate();
                Core.UserWaypoints.EndUpdate();
                Core.Logs.EndUpdate();
                _loadingInBackgroundActive = false;
                _notificationMessage.Visible = false;
                _notificationMessage = null;
            }), null);
        }

        public virtual bool LoadLogs(List<Framework.Data.Log> logs)
        {
            return true;
        }
        public virtual bool LoadWaypoints(List<Framework.Data.Waypoint> wps, List<Framework.Data.UserWaypoint> usrwps)
        {
            return true;
        }
        public virtual bool LoadLogImages(List<Framework.Data.LogImage> logimgs)
        {
            return true;
        }
        public virtual bool LoadGeocacheImages(List<Framework.Data.GeocacheImage> geocacheimgs)
        {
            return true;
        }

        /*
         *  END
         *  LOADING IN BACKGROUND METHODS
         */

        public virtual bool PrepareOpen()
        {
            return true;
        }
        private void OpenMethod()
        {
            try
            {
                _actionResult = Open(DoLoadInBackground);
                if (_actionResult && DoLoadInBackground)
                {
                    StartLoadingInBackground();
                }
            }
            catch
            {
            }
            _actionReady.Set();
        }
        public virtual bool Open(bool geocachesOnly)
        {
            return true;
        }

        public virtual bool PrepareRepair()
        {
            return true;
        }
        private void RepairMethod()
        {
            try
            {
                _actionResult = Repair(DoLoadInBackground);
                if (_actionResult && DoLoadInBackground)
                {
                    StartLoadingInBackground();
                }
            }
            catch
            {
            }
            _actionReady.Set();
        }
        public virtual bool Repair(bool geocachesOnly)
        {
            return true;
        }

        public virtual bool PrepareRestore()
        {
            return true;
        }
        private void RestoreMethod()
        {
            try
            {
                _actionResult = Restore(DoLoadInBackground);
                if (_actionResult && DoLoadInBackground)
                {
                    StartLoadingInBackground();
                }
            }
            catch
            {
            }
            _actionReady.Set();
        }
        public virtual bool Restore(bool geocachesOnly)
        {
            return true;
        }

        protected override void InitUIMainWindow(Framework.Interfaces.IPluginUIMainWindow mainWindowPlugin)
        {
            base.InitUIMainWindow(mainWindowPlugin);
            if (!_dataLoaded)
            {
                ExecuteInBackground(new ThreadStart(this.LoadMethod));
                _dataLoaded = true;
            }
        }

        public override Framework.PluginType PluginType
        {
            get { return Framework.PluginType.InternalStorage; }
        }

        protected virtual bool AddGeocache(Framework.Data.Geocache gc)
        {
            bool result = false;

            if (_cachesEmpty)
            {
                Core.Geocaches.Add(gc);
                result = true;
            }
            else
            {
                Framework.Data.Geocache oldgc = Utils.DataAccess.GetGeocache(Core.Geocaches, gc.Code);
                if (oldgc == null)
                {
                    Core.Geocaches.Add(gc);
                    result = true;
                }
                else
                {
                    if (gc.DataFromDate >= oldgc.DataFromDate)
                    {
                        Utils.DataAccess.UpdateGeocacheData(oldgc, gc, null);
                    }
                }
            }

            return result;
        }

        protected virtual bool AddWaypoint(Framework.Data.Waypoint wp)
        {
            bool result = false;

            if (_wpsEmpty)
            {
                Core.Waypoints.Add(wp);
                result = true;
            }
            else
            {
                Framework.Data.Waypoint oldwp = Utils.DataAccess.GetWaypoint(Core.Waypoints, wp.Code);
                if (oldwp == null)
                {
                    Core.Waypoints.Add(wp);
                    result = true;
                }
                else
                {
                    if (wp.DataFromDate >= oldwp.DataFromDate)
                    {
                        Utils.DataAccess.UpdateWaypointData(oldwp, wp);
                    }
                }
            }
            return result;
        }

        protected virtual bool AddUserWaypoint(Framework.Data.UserWaypoint wp)
        {
            bool result = false;

            if (_usrwpsEmpty)
            {
                Core.UserWaypoints.Add(wp);
                result = true;
            }
            else
            {
                Framework.Data.UserWaypoint oldwp = Utils.DataAccess.GetUserWaypoint(Core.UserWaypoints, wp.ID);
                if (oldwp == null)
                {
                    Core.UserWaypoints.Add(wp);
                    result = true;
                }
                else
                {
                    Utils.DataAccess.UpdateUserWaypointData(oldwp, wp);
                }
            }
            return result;
        }

        protected virtual bool AddLog(Framework.Data.Log l)
        {
            bool result = false;

            if (_logsEmpty)
            {
                Core.Logs.Add(l);
                result = true;
            }
            else
            {
                Framework.Data.Log oldwp = Utils.DataAccess.GetLog(Core.Logs, l.ID);
                if (oldwp == null)
                {
                    Core.Logs.Add(l);
                    result = true;
                }
                else
                {
                    if (l.DataFromDate >= l.DataFromDate)
                    {
                        Utils.DataAccess.UpdateLogData(oldwp, l);
                    }
                }
            }

            return result;
        }

        protected virtual bool AddLogImage(Framework.Data.LogImage l)
        {
            bool result = false;

            if (_logImgsEmpty)
            {
                Core.Logs.Add(l);
                result = true;
            }
            else
            {
                Framework.Data.LogImage oldwp = Utils.DataAccess.GetLogImage(Core.LogImages, l.ID);
                if (oldwp == null)
                {
                    Core.LogImages.Add(l);
                    result = true;
                }
                else
                {
                    if (l.DataFromDate >= l.DataFromDate)
                    {
                        Utils.DataAccess.UpdateLogImageData(oldwp, l);
                    }
                }
            }
            return result;
        }

        protected virtual bool AddGeocacheImage(Framework.Data.GeocacheImage l)
        {
            bool result = false;

            if (_geocacheImgsEmpty)
            {
                Core.GeocacheImages.Add(l);
                result = true;
            }
            else
            {
                Framework.Data.GeocacheImage oldwp = Utils.DataAccess.GetGeocacheImage(Core.GeocacheImages, l.ID);
                if (oldwp == null)
                {
                    Core.GeocacheImages.Add(l);
                    result = true;
                }
                else
                {
                    if (l.DataFromDate >= l.DataFromDate)
                    {
                        Utils.DataAccess.UpdateGeocacheImageData(oldwp, l);
                    }
                }
            }
            return result;
        }

    }
}
