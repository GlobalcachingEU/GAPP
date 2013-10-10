using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalcachingApplication.Utils;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;

namespace GlobalcachingApplication.Plugins.InternalStorage
{
    public class InternalStorage: Utils.BasePlugin.BaseInternalStorage
    {
        public const string STR_LOADING = "Loading...";
        public const string STR_LOADINGDATA = "Loading data...";
        public const string STR_LOADINGGEOCACHES = "Loading geocaches...";
        public const string STR_LOADINGLOGS = "Loading logs...";
        public const string STR_LOADINGLOGIMAGES = "Loading log images...";
        public const string STR_LOADINGWAYPOINTS = "Loading waypoints...";
        public const string STR_SAVING = "Saving...";
        public const string STR_SAVINGDATA = "Saving data...";
        public const string STR_SAVINGGEOCACHES = "Saving geocaches...";
        public const string STR_SAVINGLOGS = "Saving logs...";
        public const string STR_SAVINGLOGIMAGES = "Saving log images...";
        public const string STR_SAVINGWAYPOINTS = "Saving waypoints...";

        private SQLiteConnection _dbcon = null;

        //caching
        Hashtable _geocachesInDB = new Hashtable();
        Hashtable _logsInDB = new Hashtable();
        Hashtable _logimgsInDB = new Hashtable();
        Hashtable _wptsInDB = new Hashtable();
        Hashtable _usrwptsInDB = new Hashtable();

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LOADING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LOADINGDATA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LOADINGGEOCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LOADINGLOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LOADINGLOGIMAGES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_LOADINGWAYPOINTS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SAVING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SAVINGDATA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SAVINGGEOCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SAVINGLOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SAVINGLOGIMAGES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SAVINGLOGIMAGES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SAVINGWAYPOINTS));

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }
            if (string.IsNullOrEmpty(Properties.Settings.Default.ActiveDataFile))
            {
                Properties.Settings.Default.ActiveDataFile = System.IO.Path.Combine(core.PluginDataPath, "InternalStorage.db3");
                Properties.Settings.Default.Save();
            }
            try
            {
                string fld = System.IO.Path.GetDirectoryName(Properties.Settings.Default.ActiveDataFile);
                if (!System.IO.Directory.Exists(fld))
                {
                    System.IO.Directory.CreateDirectory(fld);
                }
                _dbcon = new SQLiteConnection(string.Format("Data Source={0}", Properties.Settings.Default.ActiveDataFile));
                InitDatabase(_dbcon);
            }
            catch
            {
                _dbcon = null;
            }

            SetDataSourceName(Properties.Settings.Default.ActiveDataFile);
            core.Logs.LoadFullData += new Framework.EventArguments.LogEventHandler(Logs_LoadFullData);
            core.Geocaches.LoadFullData += new Framework.EventArguments.GeocacheEventHandler(Geocaches_LoadFullData);

            bool result = base.Initialize(core);
            return result;
        }

        public override Framework.Data.InternalStorageDestination ActiveStorageDestination
        {
            get
            {
                Framework.Data.InternalStorageDestination isd = null;
                if (_dbcon != null)
                {
                    isd = new Framework.Data.InternalStorageDestination();
                    isd.Name = System.IO.Path.GetFileName(Properties.Settings.Default.ActiveDataFile);
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
                System.IO.File.Exists(dst.StorageInfo[0]))
            {
                Properties.Settings.Default.ActiveDataFile = dst.StorageInfo[0];
                Properties.Settings.Default.Save();
                SetDataSourceName(Properties.Settings.Default.ActiveDataFile);

                Core.Geocaches.Clear();
                Core.Logs.Clear();
                Core.Waypoints.Clear();
                Core.LogImages.Clear();
                Core.UserWaypoints.Clear();

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

        void Geocaches_LoadFullData(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (_dbcon != null)
            {
                try
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(string.Format("select shortdescr, shortdescrhtml, longdescr, longdescrhtml from geocache where Code='{0}'", e.Geocache.Code.Replace("'", "''")), _dbcon))
                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            e.Geocache.ShortDescription = (string)dr["shortdescr"];
                            e.Geocache.ShortDescriptionInHtml = (long)dr["shortdescrhtml"] != 0;
                            e.Geocache.LongDescription = (string)dr["longdescr"];
                            e.Geocache.LongDescriptionInHtml = (long)dr["longdescrhtml"] != 0;
                        }
                    }
                }
                catch
                {
                }
            }
        }

        void Logs_LoadFullData(object sender, Framework.EventArguments.LogEventArgs e)
        {
            if (_dbcon != null)
            {
                try
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(string.Format("select tbcode, finderid, logtext, encoded from log where ID='{0}'", e.Log.ID.Replace("'", "''")), _dbcon))
                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                    if (dr.Read())
                    {
                        e.Log.TBCode = dr["tbcode"] as string;
                        e.Log.FinderId = dr["finderid"] as string;
                        e.Log.Text = dr["logtext"] as string;
                        e.Log.Encoded = (long)dr["encoded"] == 0 ? false : true;
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
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(Properties.Settings.Default.ActiveDataFile);

                dlg.Filter = "*.db3|*.db3";
                dlg.FileName = "";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        if (System.IO.File.Exists(dlg.FileName))
                        {
                            System.IO.File.Delete(dlg.FileName);
                        }
                        using (SQLiteConnection dbcon = new SQLiteConnection(string.Format("Data Source={0}", dlg.FileName)))
                        {
                            if (InitDatabase(dbcon))
                            {

                                Properties.Settings.Default.ActiveDataFile = dlg.FileName;
                                Properties.Settings.Default.Save();
                                SetDataSourceName(Properties.Settings.Default.ActiveDataFile);

                                ClearCache();
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
                                foreach (Framework.Data.Waypoint gc in Core.Waypoints)
                                {
                                    gc.Saved = false;
                                }
                                result = true;
                            }
                        }
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
            SQLiteConnection dbcon = new SQLiteConnection(string.Format("Data Source={0}", Properties.Settings.Default.ActiveDataFile));
            if (InitDatabase(dbcon))
            {
                result = Save(dbcon, true);
                if (_dbcon != null)
                {
                    _dbcon.Dispose();
                    _dbcon = null;
                }
                _dbcon = dbcon;
            }
            return result;
        }

        public override bool PrepareNew()
        {
            bool result = false;
            using (System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog())
            {
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(Properties.Settings.Default.ActiveDataFile);

                dlg.Filter = "*.db3|*.db3";
                dlg.FileName = "";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        if (System.IO.File.Exists(dlg.FileName))
                        {
                            System.IO.File.Delete(dlg.FileName);
                        }
                        SQLiteConnection dbcon = new SQLiteConnection(string.Format("Data Source={0}", dlg.FileName));
                        if (InitDatabase(dbcon))
                        {

                            Properties.Settings.Default.ActiveDataFile = dlg.FileName;
                            Properties.Settings.Default.Save();
                            SetDataSourceName(Properties.Settings.Default.ActiveDataFile);

                            ClearCache();
                            using (FrameworkDataUpdater upd = new FrameworkDataUpdater(Core))
                            {
                                Core.Geocaches.Clear();
                                Core.Logs.Clear();
                                Core.LogImages.Clear();
                                Core.Waypoints.Clear();
                                Core.UserWaypoints.Clear();
                            }
                            if (_dbcon != null)
                            {
                                _dbcon.Dispose();
                                _dbcon = null;
                            }
                            _dbcon = dbcon;

                            result = true;
                        }
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
            return result;
        }


        public override bool PrepareOpen()
        {
            bool result = false;
            using (System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog())
            {
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(Properties.Settings.Default.ActiveDataFile);

                dlg.Filter = "*.db3|*.db3";
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
                    ClearCache();

                    result = true;
                }
            }
            return result;
        }
        public override bool Open(bool geocachesOnly)
        {
            bool result = false;
            if (_dbcon != null)
            {
                _dbcon.Dispose();
                _dbcon = null;
            }
            _dbcon = new SQLiteConnection(string.Format("Data Source={0}", Properties.Settings.Default.ActiveDataFile));
            if (InitDatabase(_dbcon))
            {
                result = Load(geocachesOnly);
            }
            else
            {
                _dbcon = null;
            }
            return result;
        }


        private void ClearCache()
        {
            _geocachesInDB.Clear();
            _logimgsInDB.Clear();
            _logsInDB.Clear();
            _wptsInDB.Clear();
            _usrwptsInDB.Clear();
        }

        public override bool Load(bool geocachesOnly)
        {
            bool result = true;
            if (_dbcon != null)
            {
                List<string> activeAttr = new List<string>();
                using (SQLiteCommand cmd = new SQLiteCommand("select field_name from geocache_cfields", _dbcon))
                using (SQLiteDataReader dr = cmd.ExecuteReader())
                while (dr.Read())
                {
                    string s = string.Format("{0}", dr["field_name"]);
                    activeAttr.Add(s);
                    Core.Geocaches.AddCustomAttribute(s);
                }

                int gcCount = 0;
                int logCount = 0;
                int logimgCount = 0;
                int wptCount = 0;
                using (SQLiteCommand cmd = new SQLiteCommand("select geocache, log, waypoint, logimage from counter", _dbcon))
                using (SQLiteDataReader dr = cmd.ExecuteReader())
                if (dr.Read())
                {
                    gcCount = (int)(long)dr["geocache"];
                    logCount = (int)(long)dr["log"];
                    logimgCount = (int)(long)dr["waypoint"];
                    wptCount = (int)(long)dr["logimage"];
                }

                using (Utils.ProgressBlock fixscr = new ProgressBlock(this, STR_LOADING, STR_LOADINGDATA, 1, 0))
                {
                    using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_LOADING, STR_LOADINGGEOCACHES, gcCount, 0))
                    {
                        int index = 0;
                        int procStep = 0;
                        using (SQLiteCommand cmd = new SQLiteCommand("select id, code, name, datafromdate, lat, lon, disttocent, angletocent, available, archived, country, state, cachetype, placedby, owner, ownerid, container, terrain, difficulty, encodedhints, url, memberonly, customcoords, attrids, favorites, selected, municipality, city, customlat, customlon, notes, publiceddate, personalnote, flagged, found, locked from geocache", _dbcon))
                        using (SQLiteDataReader dr = cmd.ExecuteReader())
                            while (dr.Read())
                            {
                                Framework.Data.Geocache gc = new Framework.Data.Geocache();

                                gc.ID = (string)dr["id"];
                                gc.Code = (string)dr["code"];
                                if (string.IsNullOrEmpty(gc.ID) && !string.IsNullOrEmpty(gc.Code) && gc.Code.ToUpper().StartsWith("GC"))
                                {
                                    gc.ID = Utils.Conversion.GetCacheIDFromCacheCode(gc.Code).ToString();
                                }
                                gc.Name = (string)dr["name"];
                                gc.DataFromDate = DateTime.Parse((string)dr["datafromdate"]).ToLocalTime();
                                gc.Lat = (double)dr["lat"];
                                gc.Lon = (double)dr["lon"];
                                //gc.DistanceToCenter = (int)(long)dr["disttocent"];
                                //gc.AngleToCenter = (int)(long)dr["angletocent"];
                                gc.Available = (int)(long)dr["available"] != 0;
                                gc.Archived = (int)(long)dr["archived"] != 0;
                                gc.Country = (string)dr["country"];
                                gc.State = (string)dr["state"];
                                gc.GeocacheType = Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, (int)(long)dr["cachetype"]);
                                gc.PlacedBy = (string)dr["placedby"];
                                gc.Owner = (string)dr["owner"];
                                gc.OwnerId = (string)dr["ownerid"];
                                gc.Container = Utils.DataAccess.GetGeocacheContainer(Core.GeocacheContainers, (int)(long)dr["container"]);
                                gc.Terrain = (double)dr["terrain"];
                                gc.Difficulty = (double)dr["difficulty"];
                                gc.EncodedHints = (string)dr["encodedhints"];
                                gc.Url = (string)dr["url"];
                                gc.MemberOnly = (int)(long)dr["memberonly"] != 0;
                                gc.CustomCoords = (int)(long)dr["customcoords"] != 0;
                                string s = (string)dr["attrids"];
                                string[] parts = s.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                List<int> attrList = new List<int>();
                                if (parts != null)
                                {
                                    foreach (string si in parts)
                                    {
                                        attrList.Add(int.Parse(si));
                                    }
                                }
                                gc.AttributeIds = attrList;

                                gc.Municipality = (string)dr["municipality"];
                                gc.City = (string)dr["city"];
                                object o = dr["customlat"];
                                if (o == null || o.GetType() == typeof(DBNull))
                                {
                                    gc.CustomLat = null;
                                }
                                else
                                {
                                    gc.CustomLat = (double?)o;
                                }
                                o = dr["customlon"];
                                if (o == null || o.GetType() == typeof(DBNull))
                                {
                                    gc.CustomLon = null;
                                }
                                else
                                {
                                    gc.CustomLon = (double?)o;
                                }
                                gc.Notes = (string)dr["notes"];
                                gc.PublishedTime = DateTime.Parse((string)dr["publiceddate"]).ToLocalTime();
                                gc.PersonaleNote = (string)dr["personalnote"];
                                gc.Flagged = (int)(long)dr["flagged"] != 0;
                                gc.Found = (int)(long)dr["found"] != 0;
                                gc.Locked = (int)(long)dr["locked"] != 0;

                                gc.Favorites = (int)(long)dr["favorites"];
                                gc.Selected = (int)(long)dr["selected"] != 0;

                                foreach (string attrField in activeAttr)
                                {
                                    object objValue = null;
                                    try
                                    {
                                        objValue = dr[string.Format("_{0}", attrField)];
                                    }
                                    catch
                                    {
                                    }
                                    gc.SetCustomAttribute(attrField, objValue);
                                }

                                Calculus.SetDistanceAndAngleGeocacheFromLocation(gc, Core.CenterLocation);

                                gc.Saved = true;
                                gc.IsDataChanged = false;

                                _geocachesInDB[gc.Code] = gc.Code;
                                Core.Geocaches.Add(gc);

                                index++;
                                procStep++;
                                if (procStep >= 1000)
                                {
                                    progress.UpdateProgress(STR_LOADING, STR_LOADINGGEOCACHES, gcCount, index);
                                    procStep = 0;
                                }
                            }
                    }

                    if (!geocachesOnly)
                    {

                        using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_LOADING, STR_LOADINGLOGS, logCount, 0))
                        {
                            int index = 0;
                            int procStep = 0;
                            using (SQLiteCommand cmd = new SQLiteCommand("select ID, gccode, Finder, DataFromDate, LogType, Date from log", _dbcon))
                            using (SQLiteDataReader dr = cmd.ExecuteReader())
                                while (dr.Read())
                                {
                                    Framework.Data.Log lg = new Framework.Data.Log();

                                    //id text, gccode text, tbcode text, date text, finder text, finderid text, logtext text, encoded integer, datafromdate text, logtype integer
                                    lg.ID = (string)dr["id"];
                                    lg.GeocacheCode = (string)dr["gccode"];
                                    lg.Date = DateTime.Parse((string)dr["date"]).ToLocalTime();
                                    lg.Finder = (string)dr["finder"];
                                    lg.DataFromDate = DateTime.Parse((string)dr["datafromdate"]).ToLocalTime();
                                    lg.LogType = Utils.DataAccess.GetLogType(Core.LogTypes, (int)(long)dr["logtype"]);

                                    lg.Saved = true;
                                    lg.IsDataChanged = false;

                                    _logsInDB[lg.ID] = lg.ID;
                                    Core.Logs.Add(lg);

                                    index++;
                                    procStep++;
                                    if (procStep >= 20000)
                                    {
                                        progress.UpdateProgress(STR_LOADING, STR_LOADINGLOGS, logCount, index);
                                        procStep = 0;
                                    }
                                }
                        }


                        //(id text, logid text, url text, name text, datafromdate text)
                        using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_LOADING, STR_LOADINGLOGIMAGES, logimgCount, 0))
                        {
                            int index = 0;
                            int procStep = 0;
                            using (SQLiteCommand cmd = new SQLiteCommand("select * from logimage", _dbcon))
                            using (SQLiteDataReader dr = cmd.ExecuteReader())
                                while (dr.Read())
                                {
                                    Framework.Data.LogImage lg = new Framework.Data.LogImage();

                                    lg.ID = (string)dr["id"];
                                    lg.LogID = (string)dr["logid"];
                                    lg.Url = (string)dr["url"];
                                    lg.Name = (string)dr["name"];
                                    lg.DataFromDate = DateTime.Parse((string)dr["datafromdate"]).ToLocalTime();

                                    lg.Saved = true;
                                    lg.IsDataChanged = false;

                                    _logimgsInDB[lg.ID] = lg.ID;
                                    Core.LogImages.Add(lg);

                                    index++;
                                    procStep++;
                                    if (procStep >= 2000)
                                    {
                                        progress.UpdateProgress("Loading...", "Loading log images...", logimgCount, index);
                                        procStep = 0;
                                    }
                                }
                        }


                        //id text, code text, geocachecode text, name text, datafromdate text, comment text, description text, url text, urlname text, wptype integer, lat real, lon real, time text
                        using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_LOADING, STR_LOADINGWAYPOINTS, wptCount, 0))
                        {
                            int index = 0;
                            int procStep = 0;
                            using (SQLiteCommand cmd = new SQLiteCommand("select * from waypoint", _dbcon))
                            using (SQLiteDataReader dr = cmd.ExecuteReader())
                                while (dr.Read())
                                {
                                    Framework.Data.Waypoint wp = new Framework.Data.Waypoint();

                                    wp.ID = (string)dr["id"];
                                    wp.Code = (string)dr["code"];
                                    wp.Url = (string)dr["url"];
                                    wp.UrlName = (string)dr["urlname"];
                                    wp.Name = (string)dr["name"];
                                    wp.DataFromDate = DateTime.Parse((string)dr["datafromdate"]).ToLocalTime();
                                    wp.Comment = (string)dr["comment"];
                                    wp.GeocacheCode = (string)dr["geocachecode"];
                                    wp.Description = (string)dr["description"];
                                    wp.WPType = Utils.DataAccess.GetWaypointType(Core.WaypointTypes, (int)(long)dr["wptype"]);
                                    object o = dr["lat"];
                                    if (o == null || o.GetType() == typeof(DBNull))
                                    {
                                        wp.Lat = null;
                                    }
                                    else
                                    {
                                        wp.Lat = (double?)o;
                                    }
                                    o = dr["lon"];
                                    if (o == null || o.GetType() == typeof(DBNull))
                                    {
                                        wp.Lon = null;
                                    }
                                    else
                                    {
                                        wp.Lon = (double?)o;
                                    }
                                    wp.Time = DateTime.Parse((string)dr["time"]).ToLocalTime();

                                    wp.Saved = true;
                                    wp.IsDataChanged = false;

                                    _wptsInDB[wp.Code] = wp.Code;
                                    Core.Waypoints.Add(wp);

                                    index++;
                                    procStep++;
                                    if (procStep >= 20000)
                                    {
                                        progress.UpdateProgress(STR_LOADING, STR_LOADINGWAYPOINTS, wptCount, index);
                                        procStep = 0;
                                    }
                                }

                            using (SQLiteCommand cmd = new SQLiteCommand("select * from userwaypoint", _dbcon))
                            using (SQLiteDataReader dr = cmd.ExecuteReader())
                                while (dr.Read())
                                {
                                    Framework.Data.UserWaypoint wp = new Framework.Data.UserWaypoint();

                                    wp.ID = (int)(long)dr["id"];
                                    wp.GeocacheCode = (string)dr["geocachecode"];
                                    wp.Description = (string)dr["description"];
                                    wp.Lat = (double)dr["lat"];
                                    wp.Lon = (double)dr["lon"];
                                    wp.Date = DateTime.Parse((string)dr["time"]).ToLocalTime();

                                    wp.Saved = true;
                                    wp.IsDataChanged = false;

                                    _usrwptsInDB[wp.ID] = wp.ID;
                                    Core.UserWaypoints.Add(wp);
                                }
                        }
                    }
                }
            }

            return result;
        }


        public override bool LoadLogs(List<Framework.Data.Log> logs)
        {
            bool result = false;
            SQLiteConnection dbcon = new SQLiteConnection(string.Format("Data Source={0}", Properties.Settings.Default.ActiveDataFile));
            if (dbcon != null && InitDatabase(dbcon))
            {
                int logCount = 0;
                using (SQLiteCommand cmd = new SQLiteCommand("select log from counter", dbcon))
                using (SQLiteDataReader dr = cmd.ExecuteReader())
                    if (dr.Read())
                    {
                        logCount = (int)(long)dr["log"];
                    }
                UpdateLoadingInBackgroundProgress(STR_LOADING_LOGS_BG, logCount, 0);

                int index = 0;
                int procStep = 0;
                using (SQLiteCommand cmd = new SQLiteCommand("select ID, gccode, Finder, DataFromDate, LogType, Date from log", dbcon))
                {
                    cmd.CommandTimeout = 0;
                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                        while (dr.Read())
                        {
                            Framework.Data.Log lg = new Framework.Data.Log();

                            lg.ID = (string)dr["id"];
                            lg.GeocacheCode = (string)dr["gccode"];
                            lg.Date = DateTime.Parse((string)dr["date"]).ToLocalTime();
                            lg.Finder = (string)dr["finder"];
                            lg.DataFromDate = DateTime.Parse((string)dr["datafromdate"]).ToLocalTime();
                            lg.LogType = Utils.DataAccess.GetLogType(Core.LogTypes, (int)(long)dr["logtype"]);

                            lg.Saved = true;
                            lg.IsDataChanged = false;

                            _logsInDB[lg.ID] = lg.ID;
                            logs.Add(lg);

                            index++;
                            procStep++;
                            if (procStep >= 20000)
                            {
                                UpdateLoadingInBackgroundProgress(STR_LOADING_LOGS_BG, logCount, index);
                                procStep = 0;
                            }
                        }
                }
                dbcon.Dispose();
                dbcon = null;
            }
            return result;
        }
        public override bool LoadWaypoints(List<Framework.Data.Waypoint> wps, List<Framework.Data.UserWaypoint> usrwps)
        {
            bool result = false;
            SQLiteConnection dbcon = new SQLiteConnection(string.Format("Data Source={0}", Properties.Settings.Default.ActiveDataFile));
            if (dbcon != null && InitDatabase(dbcon))
            {
                int wptCount = 0;
                using (SQLiteCommand cmd = new SQLiteCommand("select waypoint from counter", dbcon))
                using (SQLiteDataReader dr = cmd.ExecuteReader())
                    if (dr.Read())
                    {
                        wptCount = (int)(long)dr["waypoint"];
                    }

                int index = 0;
                int procStep = 0;
                using (SQLiteCommand cmd = new SQLiteCommand("select * from waypoint", dbcon))
                {
                    cmd.CommandTimeout = 0;
                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                        while (dr.Read())
                        {
                            Framework.Data.Waypoint wp = new Framework.Data.Waypoint();

                            wp.ID = (string)dr["id"];
                            wp.Code = (string)dr["code"];
                            wp.Url = (string)dr["url"];
                            wp.UrlName = (string)dr["urlname"];
                            wp.Name = (string)dr["name"];
                            wp.DataFromDate = DateTime.Parse((string)dr["datafromdate"]).ToLocalTime();
                            wp.Comment = (string)dr["comment"];
                            wp.GeocacheCode = (string)dr["geocachecode"];
                            wp.Description = (string)dr["description"];
                            wp.WPType = Utils.DataAccess.GetWaypointType(Core.WaypointTypes, (int)(long)dr["wptype"]);
                            object o = dr["lat"];
                            if (o == null || o.GetType() == typeof(DBNull))
                            {
                                wp.Lat = null;
                            }
                            else
                            {
                                wp.Lat = (double?)o;
                            }
                            o = dr["lon"];
                            if (o == null || o.GetType() == typeof(DBNull))
                            {
                                wp.Lon = null;
                            }
                            else
                            {
                                wp.Lon = (double?)o;
                            }
                            wp.Time = DateTime.Parse((string)dr["time"]).ToLocalTime();

                            wp.Saved = true;
                            wp.IsDataChanged = false;

                            _wptsInDB[wp.Code] = wp.Code;
                            wps.Add(wp);

                            index++;
                            procStep++;
                            if (procStep >= 20000)
                            {
                                UpdateLoadingInBackgroundProgress(STR_LOADING_WAYPOINTS_BG, wptCount, index);
                                procStep = 0;
                            }
                        }
                }
                using (SQLiteCommand cmd = new SQLiteCommand("select * from userwaypoint", dbcon))
                {
                    cmd.CommandTimeout = 0;
                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                        while (dr.Read())
                        {
                            Framework.Data.UserWaypoint wp = new Framework.Data.UserWaypoint();

                            wp.ID = (int)(long)dr["id"];
                            wp.GeocacheCode = (string)dr["geocachecode"];
                            wp.Description = (string)dr["description"];
                            wp.Lat = (double)dr["lat"];
                            wp.Lon = (double)dr["lon"];
                            wp.Date = DateTime.Parse((string)dr["time"]).ToLocalTime();

                            wp.Saved = true;
                            wp.IsDataChanged = false;

                            _usrwptsInDB[wp.ID] = wp.ID;
                            usrwps.Add(wp);
                        }
                }
                dbcon.Dispose();
                dbcon = null;
            }
            return result;
        }
        public override bool LoadLogImages(List<Framework.Data.LogImage> logimgs)
        {
            bool result = false;
            SQLiteConnection dbcon = new SQLiteConnection(string.Format("Data Source={0}", Properties.Settings.Default.ActiveDataFile));
            if (dbcon != null && InitDatabase(dbcon))
            {
                int logimgCount = 0;
                using (SQLiteCommand cmd = new SQLiteCommand("select logimage from counter", dbcon))
                using (SQLiteDataReader dr = cmd.ExecuteReader())
                    if (dr.Read())
                    {
                        logimgCount = (int)(long)dr["logimage"];
                    }

                int index = 0;
                int procStep = 0;
                using (SQLiteCommand cmd = new SQLiteCommand("select * from logimage", dbcon))
                using (SQLiteDataReader dr = cmd.ExecuteReader())
                    while (dr.Read())
                    {
                        Framework.Data.LogImage lg = new Framework.Data.LogImage();

                        lg.ID = (string)dr["id"];
                        lg.LogID = (string)dr["logid"];
                        lg.Url = (string)dr["url"];
                        lg.Name = (string)dr["name"];
                        lg.DataFromDate = DateTime.Parse((string)dr["datafromdate"]).ToLocalTime();

                        lg.Saved = true;
                        lg.IsDataChanged = false;

                        _logimgsInDB[lg.ID] = lg.ID;
                        logimgs.Add(lg);

                        index++;
                        procStep++;
                        if (procStep >= 2000)
                        {
                            UpdateLoadingInBackgroundProgress(STR_LOADING_LOGIMAGES_BG, logimgCount, index);
                            procStep = 0;
                        }
                    }

                dbcon.Dispose();
                dbcon = null;
            }
            return result;
        }


        public bool ColumnExists(SQLiteConnection dbcon, string tableName, string columnName)
        {
            bool result = false;
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(string.Format("pragma table_info({0})", tableName), dbcon))
                using (SQLiteDataReader dr = cmd.ExecuteReader())
                while (dr.Read())
                {
                    if (dr[1] is string)
                    {
                        if ((string)dr[1] == columnName)
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            catch
            {
            }
            return result;
        }

        public override bool Save()
        {
            return Save(_dbcon, false);
        }
        public bool Save(SQLiteConnection dbcon, bool forceFullData)
        {
            bool result = true;
            using (Utils.ProgressBlock fixpr = new ProgressBlock(this, STR_SAVING, STR_SAVINGDATA, 1, 0))
            {
                if (dbcon != null)
                {
                    string[] custAttr = Core.Geocaches.CustomAttributes;
                    List<string> activeAttr = new List<string>();
                    using (SQLiteCommand cmd = new SQLiteCommand("select field_name from geocache_cfields", dbcon))
                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                    while (dr.Read())
                    {
                        activeAttr.Add(string.Format("{0}", dr["field_name"]));
                    }

                    foreach (string s in activeAttr)
                    {
                        if (!custAttr.Contains(s) && ColumnExists(dbcon, "geocache", string.Format("_{0}", s)))
                        {
                            //drop column not supported!
                        }
                    }
                    //geocache_cfields
                    using (SQLiteCommand cmd = new SQLiteCommand("delete from geocache_cfields", dbcon))
                        cmd.ExecuteNonQuery();
                    foreach (string s in custAttr)
                    {
                        if (!activeAttr.Contains(s))
                        {
                            using (SQLiteCommand cmd = new SQLiteCommand(string.Format("insert into geocache_cfields (field_name) values ('{0}')", s), dbcon))
                                cmd.ExecuteNonQuery();
                        }
                        if (!ColumnExists(dbcon, "geocache", string.Format("_{0}", s)))
                        {
                            using (SQLiteCommand cmd = new SQLiteCommand(string.Format("alter table geocache add _{0} text)", s), dbcon))
                                cmd.ExecuteNonQuery();
                        }
                    }

                    //delete geoacches that are not in the list anymore.
                    string[] c = (from string a in _geocachesInDB.Keys select a).ToArray();
                    using (SQLiteCommand cmd = new SQLiteCommand(dbcon))
                        for (int i = 0; i < c.Length; i++)
                        {
                            if (Utils.DataAccess.GetGeocache(Core.Geocaches, c[i]) == null)
                            {
                                cmd.CommandText = string.Format("delete from geocache where code='{0}'", c[i]);
                                cmd.ExecuteNonQuery();
                                _geocachesInDB.Remove(c[i]);
                            }
                        }

                    //reset selection
                    using (SQLiteCommand cmd = new SQLiteCommand("update geocache set selected=0", dbcon))
                        cmd.ExecuteNonQuery();

                    //now get all the selected and data changed geocaches
                    List<Framework.Data.Geocache> gclist = (from Framework.Data.Geocache wp in Core.Geocaches
                                                            where wp.Selected || !wp.Saved
                                                            select wp).ToList();
                    if (gclist.Count > 0)
                    {
                        using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_SAVING, STR_SAVINGGEOCACHES, gclist.Count, 0))
                        {
                            string updateSqlFull = "update geocache set id=@id, name=@name, datafromdate=@datafromdate, lat=@lat, lon=@lon, disttocent=@disttocent, angletocent=@angletocent, available=@available, archived=@archived, country=@country, state=@state, cachetype=@cachetype, placedby=@placedby, owner=@owner, ownerid=@ownerid, container=@container, terrain=@terrain, difficulty=@difficulty, shortdescr=@shortdescr, shortdescrhtml=@shortdescrhtml, longdescr=@longdescr, longdescrhtml=@longdescrhtml, encodedhints=@encodedhints, url=@url, memberonly=@memberonly, customcoords=@customcoords, attrids=@attrids, favorites=@favorites, selected=@selected, municipality=@municipality, city=@city, customlat=@customlat, customlon=@customlon, notes=@notes, publiceddate=@publiceddate, personalnote=@personalnote, flagged=@flagged, found=@found, locked=@locked where code=@code";
                            string insertSqlFull = "insert into geocache (id, code, name, datafromdate, lat, lon, disttocent, angletocent, available, archived, country, state, cachetype, placedby, owner, ownerid, container, terrain, difficulty, shortdescr, shortdescrhtml, longdescr, longdescrhtml, encodedhints, url, memberonly, customcoords, attrids, favorites, selected, municipality, city, customlat, customlon, notes, publiceddate, personalnote, flagged, found, locked) values (@id, @code, @name, @datafromdate, @lat, @lon, @disttocent, @angletocent, @available, @archived, @country, @state, @cachetype, @placedby, @owner, @ownerid, @container, @terrain, @difficulty, @shortdescr, @shortdescrhtml, @longdescr, @longdescrhtml, @encodedhints, @url, @memberonly, @customcoords, @attrids, @favorites, @selected, @municipality, @city, @customlat, @customlon, @notes, @publiceddate, @personalnote, @flagged, @found, @locked)";

                            string updateSqlShort = "update geocache set id=@id, name=@name, datafromdate=@datafromdate, lat=@lat, lon=@lon, disttocent=@disttocent, angletocent=@angletocent, available=@available, archived=@archived, country=@country, state=@state, cachetype=@cachetype, placedby=@placedby, owner=@owner, ownerid=@ownerid, container=@container, terrain=@terrain, difficulty=@difficulty, encodedhints=@encodedhints, url=@url, memberonly=@memberonly, customcoords=@customcoords, attrids=@attrids, favorites=@favorites, selected=@selected, municipality=@municipality, city=@city, customlat=@customlat, customlon=@customlon, notes=@notes, publiceddate=@publiceddate, personalnote=@personalnote, flagged=@flagged, found=@found, locked=@locked where code=@code";
                            string insertSqlShort = "insert into geocache (id, code, name, datafromdate, lat, lon, disttocent, angletocent, available, archived, country, state, cachetype, placedby, owner, ownerid, container, terrain, difficulty, encodedhints, url, memberonly, customcoords, attrids, favorites, selected, municipality, city, customlat, customlon, notes, publiceddate, personalnote, flagged, found, locked) values (@id, @code, @name, @datafromdate, @lat, @lon, @disttocent, @angletocent, @available, @archived, @country, @state, @cachetype, @placedby, @owner, @ownerid, @container, @terrain, @difficulty, @encodedhints, @url, @memberonly, @customcoords, @attrids, @favorites, @selected, @municipality, @city, @customlat, @customlon, @notes, @publiceddate, @personalnote, @flagged, @found, @locked)";

                            using (SQLiteCommand cmd = new SQLiteCommand(dbcon))
                            {
                                cmd.CommandType = CommandType.Text;
                                DbParameter par = cmd.CreateParameter();
                                par.ParameterName = "@id";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@code";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@name";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@datafromdate";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@lat";
                                par.DbType = DbType.Double;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@lon";
                                par.DbType = DbType.Double;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@disttocent";
                                par.DbType = DbType.Int64;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@angletocent";
                                par.DbType = DbType.Int32;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@available";
                                par.DbType = DbType.Int32;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@archived";
                                par.DbType = DbType.Int32;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@country";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@state";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@cachetype";
                                par.DbType = DbType.Int32;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@placedby";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@owner";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@ownerid";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@container";
                                par.DbType = DbType.Int32;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@terrain";
                                par.DbType = DbType.Double;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@difficulty";
                                par.DbType = DbType.Double;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@shortdescr";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@shortdescrhtml";
                                par.DbType = DbType.Int32;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@longdescr";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@longdescrhtml";
                                par.DbType = DbType.Int32;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@encodedhints";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@url";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@memberonly";
                                par.DbType = DbType.Int32;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@customcoords";
                                par.DbType = DbType.Int32;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@attrids";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@favorites";
                                par.DbType = DbType.Int32;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@selected";
                                par.DbType = DbType.Int32;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@municipality";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@city";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@customlat";
                                par.DbType = DbType.Double;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@customlon";
                                par.DbType = DbType.Double;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@notes";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@publiceddate";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@personalnote";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@flagged";
                                par.DbType = DbType.Int32;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@found";
                                par.DbType = DbType.Int32;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@locked";
                                par.DbType = DbType.Int32;
                                cmd.Parameters.Add(par);

                                foreach (string s in custAttr)
                                {
                                    par = cmd.CreateParameter();
                                    par.ParameterName = string.Format("@_{0}", s);
                                    par.DbType = DbType.String;
                                    cmd.Parameters.Add(par);
                                }

                                cmd.Prepare();

                                StringBuilder sb = new StringBuilder();

                                using (SQLiteTransaction trans = dbcon.BeginTransaction())
                                {
                                    int index = 0;
                                    int procStep = 0;
                                    foreach (Framework.Data.Geocache gc in gclist)
                                    {
                                        index++;
                                        procStep++;
                                        if (!gc.Saved)
                                        {
                                            cmd.Parameters["@id"].Value = gc.ID ?? "";
                                            cmd.Parameters["@code"].Value = gc.Code;
                                            cmd.Parameters["@name"].Value = gc.Name ?? "";
                                            cmd.Parameters["@datafromdate"].Value = gc.DataFromDate.ToUniversalTime().ToString("u");
                                            cmd.Parameters["@lat"].Value = gc.Lat;
                                            cmd.Parameters["@lon"].Value = gc.Lon;
                                            cmd.Parameters["@disttocent"].Value = gc.DistanceToCenter;
                                            cmd.Parameters["@angletocent"].Value = gc.AngleToCenter;
                                            cmd.Parameters["@available"].Value = gc.Available ? 1 : 0;
                                            cmd.Parameters["@archived"].Value = gc.Archived ? 1 : 0;
                                            cmd.Parameters["@country"].Value = gc.Country ?? "";
                                            cmd.Parameters["@state"].Value = gc.State ?? "";
                                            cmd.Parameters["@cachetype"].Value = gc.GeocacheType == null ? -1 : gc.GeocacheType.ID;
                                            cmd.Parameters["@placedby"].Value = gc.PlacedBy ?? "";
                                            cmd.Parameters["@owner"].Value = gc.Owner ?? "";
                                            cmd.Parameters["@ownerid"].Value = gc.OwnerId ?? "";
                                            cmd.Parameters["@container"].Value = gc.Container == null ? -1 : gc.Container.ID;
                                            cmd.Parameters["@terrain"].Value = gc.Terrain;
                                            cmd.Parameters["@difficulty"].Value = gc.Difficulty;
                                            if (forceFullData || gc.FullDataLoaded)
                                            {
                                                cmd.Parameters["@shortdescr"].Value = gc.ShortDescription ?? "";
                                                cmd.Parameters["@shortdescrhtml"].Value = gc.ShortDescriptionInHtml ? 1 : 0;
                                                cmd.Parameters["@longdescr"].Value = gc.LongDescription ?? "";
                                                cmd.Parameters["@longdescrhtml"].Value = gc.LongDescriptionInHtml ? 1 : 0;
                                            }
                                            cmd.Parameters["@encodedhints"].Value = gc.EncodedHints ?? "";
                                            cmd.Parameters["@url"].Value = gc.Url ?? "";
                                            cmd.Parameters["@memberonly"].Value = gc.MemberOnly ? 1 : 0;
                                            cmd.Parameters["@customcoords"].Value = gc.CustomCoords ? 1 : 0;
                                            sb.Length = 0;
                                            foreach (int attrId in gc.AttributeIds)
                                            {
                                                sb.AppendFormat("|{0}|", attrId);
                                            }
                                            cmd.Parameters["@attrids"].Value = sb.ToString();
                                            cmd.Parameters["@favorites"].Value = gc.Favorites;
                                            cmd.Parameters["@selected"].Value = gc.Selected ? 1 : 0;
                                            cmd.Parameters["@municipality"].Value = gc.Municipality ?? "";
                                            cmd.Parameters["@city"].Value = gc.City ?? "";
                                            if (gc.CustomLat == null)
                                            {
                                                cmd.Parameters["@customlat"].Value = DBNull.Value;
                                            }
                                            else
                                            {
                                                cmd.Parameters["@customlat"].Value = gc.CustomLat;
                                            }
                                            if (gc.CustomLon == null)
                                            {
                                                cmd.Parameters["@customlon"].Value = DBNull.Value;
                                            }
                                            else
                                            {
                                                cmd.Parameters["@customlon"].Value = gc.CustomLon;
                                            }
                                            cmd.Parameters["@notes"].Value = gc.Notes ?? "";
                                            cmd.Parameters["@publiceddate"].Value = gc.PublishedTime.ToUniversalTime().ToString("u");
                                            cmd.Parameters["@personalnote"].Value = gc.PersonaleNote ?? "";
                                            cmd.Parameters["@flagged"].Value = gc.Flagged ? 1 : 0;
                                            cmd.Parameters["@found"].Value = gc.Found ? 1 : 0;
                                            cmd.Parameters["@locked"].Value = gc.Locked ? 1 : 0;

                                            foreach (string s in custAttr)
                                            {
                                                object o = gc.GetCustomAttribute(s);
                                                if (o == null)
                                                {
                                                    cmd.Parameters[string.Format("@_{0}", s)].Value = DBNull.Value;
                                                }
                                                else
                                                {
                                                    cmd.Parameters[string.Format("@_{0}", s)].Value = o.ToString();
                                                }
                                            }

                                            bool indb = _geocachesInDB[gc.Code] != null;
                                            if (forceFullData || gc.FullDataLoaded)
                                            {
                                                cmd.CommandText = updateSqlFull;
                                                if (!indb || cmd.ExecuteNonQuery() == 0)
                                                {
                                                    cmd.CommandText = insertSqlFull;
                                                    cmd.ExecuteNonQuery();
                                                    if (!indb)
                                                    {
                                                        _geocachesInDB[gc.Code] = gc.Code;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                cmd.CommandText = updateSqlShort;
                                                if (!indb || cmd.ExecuteNonQuery() == 0)
                                                {
                                                    cmd.CommandText = insertSqlShort;
                                                    cmd.ExecuteNonQuery();
                                                    if (!indb)
                                                    {
                                                        _geocachesInDB[gc.Code] = gc.Code;
                                                    }
                                                }
                                            }
                                            gc.Saved = true;
                                        }
                                        else if (gc.Selected)
                                        {
                                            cmd.CommandText = string.Format("update geocache set selected=1 where code='{0}'", gc.Code);
                                            cmd.ExecuteNonQuery();
                                        }
                                        if (procStep >= 200)
                                        {
                                            progress.UpdateProgress(STR_SAVING, STR_SAVINGGEOCACHES, gclist.Count, index);
                                            procStep = 0;
                                        }
                                    }
                                    trans.Commit();
                                }
                            }
                        }
                    }

                    //delete logs that are not in the list anymore.
                    c = (from string a in _logsInDB.Keys select a).ToArray();
                    using (SQLiteCommand cmd = new SQLiteCommand(dbcon))
                        for (int i = 0; i < c.Length; i++)
                        {
                            if (Utils.DataAccess.GetLog(Core.Logs, c[i]) == null)
                            {
                                cmd.CommandText = string.Format("delete from log where id='{0}'", c[i]);
                                cmd.ExecuteNonQuery();
                                _logsInDB.Remove(c[i]);
                            }
                        }

                    //now get all the selected and data changed geocaches
                    List<Framework.Data.Log> lglist = (from Framework.Data.Log wp in Core.Logs
                                                       where !wp.Saved
                                                       select wp).ToList();
                    if (lglist.Count > 0)
                    {
                        using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_SAVING, STR_SAVINGLOGS, lglist.Count, 0))
                        {
                            // tbcode, finderid, logtext, encoded
                            string updateSqlFull = "update log set gccode=@gccode, tbcode=@tbcode, date=@date, finder=@finder, finderid=@finderid, logtext=@logtext, encoded=@encoded, datafromdate=@datafromdate, logtype=@logtype where id=@id";
                            string insertSqlFull = "insert into log (id, gccode, tbcode, date, finder, finderid, logtext, encoded, datafromdate, logtype) values (@id, @gccode, @tbcode, @date, @finder, @finderid, @logtext, @encoded, @datafromdate, @logtype)";

                            string updateSqlShort = "update log set gccode=@gccode, date=@date, finder=@finder, datafromdate=@datafromdate, logtype=@logtype where id=@id";
                            string insertSqlShort = "insert into log (id, gccode, date, finder, datafromdate, logtype) values (@id, @gccode, @date, @finder, @datafromdate, @logtype)";

                            using (SQLiteCommand cmd = new SQLiteCommand(dbcon))
                            {
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.Clear();
                                DbParameter par = cmd.CreateParameter();
                                par.ParameterName = "@id";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@gccode";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@tbcode";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@date";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@finder";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@finderid";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@logtext";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@encoded";
                                par.DbType = DbType.Int32;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@datafromdate";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@logtype";
                                par.DbType = DbType.Int32;
                                cmd.Parameters.Add(par);

                                cmd.Prepare();

                                using (SQLiteTransaction trans = dbcon.BeginTransaction())
                                {
                                    int index = 0;
                                    int procStep = 0;
                                    foreach (Framework.Data.Log lg in lglist)
                                    {
                                        index++;
                                        procStep++;

                                        cmd.Parameters["@id"].Value = lg.ID;
                                        cmd.Parameters["@gccode"].Value = lg.GeocacheCode;
                                        cmd.Parameters["@date"].Value = lg.Date.ToUniversalTime().ToString("u");
                                        cmd.Parameters["@finder"].Value = lg.Finder ?? "";
                                        cmd.Parameters["@datafromdate"].Value = lg.DataFromDate.ToUniversalTime().ToString("u");
                                        cmd.Parameters["@logtype"].Value = lg.LogType == null ? -1 : lg.LogType.ID;
                                        if (forceFullData || lg.FullDataLoaded)
                                        {
                                            cmd.Parameters["@tbcode"].Value = lg.TBCode ?? "";
                                            cmd.Parameters["@finderid"].Value = lg.FinderId ?? "";
                                            cmd.Parameters["@logtext"].Value = lg.Text ?? "";
                                            cmd.Parameters["@encoded"].Value = lg.Encoded ? 1 : 0;
                                        }

                                        bool indb = _logsInDB[lg.ID] != null;
                                        if (forceFullData || lg.FullDataLoaded)
                                        {
                                            cmd.CommandText = updateSqlFull;
                                            if (!indb || cmd.ExecuteNonQuery() == 0)
                                            {
                                                cmd.CommandText = insertSqlFull;
                                                cmd.ExecuteNonQuery();
                                                if (!indb)
                                                {
                                                    _logsInDB[lg.ID] = lg.ID;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            cmd.CommandText = updateSqlShort;
                                            if (!indb || cmd.ExecuteNonQuery() == 0)
                                            {
                                                cmd.CommandText = insertSqlShort;
                                                cmd.ExecuteNonQuery();
                                                if (!indb)
                                                {
                                                    _logsInDB[lg.ID] = lg.ID;
                                                }
                                            }
                                        }
                                        lg.Saved = true;

                                        if (procStep >= 2000)
                                        {
                                            progress.UpdateProgress(STR_SAVING, STR_SAVINGLOGS, lglist.Count, index);
                                            procStep = 0;
                                        }
                                    }

                                    trans.Commit();

                                }
                            }
                        }
                    }



                    //delete log images that are not in the list anymore.
                    c = (from string a in _logimgsInDB.Keys select a).ToArray();
                    using (SQLiteCommand cmd = new SQLiteCommand(dbcon))
                        for (int i = 0; i < c.Length; i++)
                        {
                            if (Utils.DataAccess.GetLogImage(Core.LogImages, c[i]) == null)
                            {
                                cmd.CommandText = string.Format("delete from logimage where id='{0}'", c[i]);
                                cmd.ExecuteNonQuery();
                                _logimgsInDB.Remove(c[i]);
                            }
                        }

                    //now get all the selected and data changed geocaches
                    List<Framework.Data.LogImage> imglist = (from Framework.Data.LogImage wp in Core.LogImages
                                                             where !wp.Saved
                                                             select wp).ToList();
                    if (imglist.Count > 0)
                    {
                        using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_SAVING, STR_SAVINGLOGIMAGES, imglist.Count, 0))
                        {
                            string updateSql = "update logimage set logid=@logid, url=@url, name=@name, datafromdate=@datafromdate where id=@id";
                            string insertSql = "insert into logimage (id, logid, url, name, datafromdate) values (@id, @logid, @url, @name, @datafromdate)";

                            using (SQLiteCommand cmd = new SQLiteCommand(dbcon))
                            {

                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.Clear();
                                DbParameter par = cmd.CreateParameter();
                                par.ParameterName = "@id";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@logid";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@url";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@name";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@datafromdate";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                cmd.Prepare();

                                using (SQLiteTransaction trans = dbcon.BeginTransaction())
                                {
                                    int index = 0;
                                    int procStep = 0;
                                    foreach (Framework.Data.LogImage lg in imglist)
                                    {
                                        index++;
                                        procStep++;

                                        cmd.Parameters["@id"].Value = lg.ID;
                                        cmd.Parameters["@logid"].Value = lg.LogID ?? "";
                                        cmd.Parameters["@url"].Value = lg.Url ?? "";
                                        cmd.Parameters["@name"].Value = lg.Name ?? "";
                                        cmd.Parameters["@datafromdate"].Value = lg.DataFromDate.ToUniversalTime().ToString("u");

                                        bool indb = _logimgsInDB[lg.ID] != null;
                                        cmd.CommandText = updateSql;
                                        if (!indb || cmd.ExecuteNonQuery() == 0)
                                        {
                                            cmd.CommandText = insertSql;
                                            cmd.ExecuteNonQuery();
                                            if (!indb)
                                            {
                                                _logimgsInDB[lg.ID] = lg.ID;
                                            }
                                        }
                                        lg.Saved = true;

                                        if (procStep >= 500)
                                        {
                                            progress.UpdateProgress(STR_SAVING, STR_SAVINGLOGIMAGES, imglist.Count, index);
                                            procStep = 0;
                                        }
                                    }
                                    trans.Commit();
                                }
                            }
                        }
                    }


                    c = (from string a in _wptsInDB.Keys select a).ToArray();
                    using (SQLiteCommand cmd = new SQLiteCommand(dbcon))
                    for (int i = 0; i < c.Length; i++)
                    {
                        if (Utils.DataAccess.GetWaypoint(Core.Waypoints, c[i]) == null)
                        {
                            cmd.CommandText = string.Format("delete from waypoint where code='{0}'", c[i]);
                            cmd.ExecuteNonQuery();
                            _wptsInDB.Remove(c[i]);
                        }
                    }

                    //now get all the selected and data changed geocaches
                    List<Framework.Data.Waypoint> wplist = (from Framework.Data.Waypoint wp in Core.Waypoints
                                                            where !wp.Saved
                                                            select wp).ToList();
                    if (wplist.Count > 0)
                    {
                        using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_SAVING, STR_SAVINGWAYPOINTS, wplist.Count, 0))
                        {
                            string updateSql = "update waypoint set id=@id, geocachecode=@geocachecode, name=@name, datafromdate=@datafromdate, comment=@comment, description=@description, url=@url, urlname=@urlname, wptype=@wptype, lat=@lat, lon=@lon, time=@time where code=@code";
                            string insertSql = "insert into waypoint (id, code, geocachecode, name, datafromdate, comment, description, url, urlname, wptype, lat, lon, time) values (@id, @code, @geocachecode, @name, @datafromdate, @comment, @description, @url, @urlname, @wptype, @lat, @lon, @time)";

                            using (SQLiteCommand cmd = new SQLiteCommand(dbcon))
                            {

                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.Clear();
                                DbParameter par = cmd.CreateParameter();
                                par.ParameterName = "@id";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@code";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@url";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@name";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@datafromdate";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@geocachecode";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@comment";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@description";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@urlname";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@wptype";
                                par.DbType = DbType.Int32;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@lat";
                                par.DbType = DbType.Double;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@lon";
                                par.DbType = DbType.Double;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@time";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                cmd.Prepare();

                                using (SQLiteTransaction trans = dbcon.BeginTransaction())
                                {
                                    int index = 0;
                                    int procStep = 0;
                                    foreach (Framework.Data.Waypoint wp in wplist)
                                    {
                                        index++;
                                        procStep++;

                                        cmd.Parameters["@id"].Value = wp.ID;
                                        cmd.Parameters["@code"].Value = wp.Code ?? "";
                                        cmd.Parameters["@url"].Value = wp.Url ?? "";
                                        cmd.Parameters["@urlname"].Value = wp.UrlName ?? "";
                                        cmd.Parameters["@name"].Value = wp.Name ?? "";
                                        cmd.Parameters["@comment"].Value = wp.Comment ?? "";
                                        cmd.Parameters["@geocachecode"].Value = wp.GeocacheCode ?? "";
                                        cmd.Parameters["@description"].Value = wp.Description ?? "";
                                        cmd.Parameters["@datafromdate"].Value = wp.DataFromDate.ToUniversalTime().ToString("u");
                                        cmd.Parameters["@time"].Value = wp.Time.ToUniversalTime().ToString("u");
                                        cmd.Parameters["@wptype"].Value = wp.WPType.ID;
                                        if (wp.Lat == null)
                                        {
                                            cmd.Parameters["@lat"].Value = DBNull.Value;
                                        }
                                        else
                                        {
                                            cmd.Parameters["@lat"].Value = (double)wp.Lat;
                                        }
                                        if (wp.Lon == null)
                                        {
                                            cmd.Parameters["@lon"].Value = DBNull.Value;
                                        }
                                        else
                                        {
                                            cmd.Parameters["@lon"].Value = wp.Lon;
                                        }

                                        bool indb = _wptsInDB[wp.Code] != null;
                                        cmd.CommandText = updateSql;
                                        if (!indb || cmd.ExecuteNonQuery() == 0)
                                        {
                                            cmd.CommandText = insertSql;
                                            cmd.ExecuteNonQuery();
                                            if (!indb)
                                            {
                                                _wptsInDB[wp.Code] = wp.Code;
                                            }
                                        }
                                        wp.Saved = true;

                                        if (procStep >= 500)
                                        {
                                            progress.UpdateProgress(STR_SAVING, STR_SAVINGWAYPOINTS, wplist.Count, index);
                                            procStep = 0;
                                        }
                                    }
                                    trans.Commit();
                                }
                            }
                        }
                    }

                    int[] ci = (from int a in _usrwptsInDB.Keys select a).ToArray();
                    using (SQLiteCommand cmd = new SQLiteCommand(dbcon))
                        for (int i = 0; i < ci.Length; i++)
                        {
                            if (Utils.DataAccess.GetUserWaypoint(Core.UserWaypoints, ci[i]) == null)
                            {
                                cmd.CommandText = string.Format("delete from userwaypoint where id={0}", ci[i]);
                                cmd.ExecuteNonQuery();
                                _usrwptsInDB.Remove(ci[i]);
                            }
                        }

                    //now get all the selected and data changed geocaches
                    List<Framework.Data.UserWaypoint> usrwplist = (from Framework.Data.UserWaypoint wp in Core.UserWaypoints
                                                            where !wp.Saved
                                                            select wp).ToList();
                    if (usrwplist.Count > 0)
                    {
                            string updateSql = "update userwaypoint set geocachecode=@geocachecode, description=@description, lat=@lat, lon=@lon, time=@time where id=@id";
                            string insertSql = "insert into userwaypoint (id, geocachecode, description, lat, lon, time) values (@id, @geocachecode, @description, @lat, @lon, @time)";

                            using (SQLiteCommand cmd = new SQLiteCommand(dbcon))
                            {

                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.Clear();
                                DbParameter par = cmd.CreateParameter();
                                par.ParameterName = "@id";
                                par.DbType = DbType.Int32;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@geocachecode";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@description";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@lat";
                                par.DbType = DbType.Double;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@lon";
                                par.DbType = DbType.Double;
                                cmd.Parameters.Add(par);
                                par = cmd.CreateParameter();
                                par.ParameterName = "@time";
                                par.DbType = DbType.String;
                                cmd.Parameters.Add(par);
                                cmd.Prepare();

                                using (SQLiteTransaction trans = dbcon.BeginTransaction())
                                {
                                    foreach (Framework.Data.UserWaypoint wp in usrwplist)
                                    {
                                        cmd.Parameters["@id"].Value = wp.ID;
                                        cmd.Parameters["@geocachecode"].Value = wp.GeocacheCode ?? "";
                                        cmd.Parameters["@description"].Value = wp.Description ?? "";
                                        cmd.Parameters["@time"].Value = wp.Date.ToUniversalTime().ToString("u");
                                        cmd.Parameters["@lat"].Value = (double)wp.Lat;
                                        cmd.Parameters["@lon"].Value = wp.Lon;

                                        bool indb = _usrwptsInDB[wp.ID] != null;
                                        cmd.CommandText = updateSql;
                                        if (!indb || cmd.ExecuteNonQuery() == 0)
                                        {
                                            cmd.CommandText = insertSql;
                                            cmd.ExecuteNonQuery();
                                            if (!indb)
                                            {
                                                _usrwptsInDB[wp.ID] = wp.ID;
                                            }
                                        }
                                        wp.Saved = true;
                                    }
                                    trans.Commit();
                                }
                            }
                    }

                    using (SQLiteCommand cmd = new SQLiteCommand(string.Format("update counter set geocache={0}, log={1}, waypoint={2}, logimage={3}", _geocachesInDB.Count, _logsInDB.Count, _wptsInDB.Count, _logimgsInDB.Count), dbcon))
                        cmd.ExecuteNonQuery();
                }
            }
            return result;
        }

        private bool InitDatabase(SQLiteConnection dbcon)
        {
            bool result = false;
            try
            {
                if (dbcon.State == ConnectionState.Closed)
                {
                    dbcon.Open();
                }
                using (SQLiteCommand cmd = new SQLiteCommand(dbcon))
                {
                    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='project_info'";
                    object o = cmd.ExecuteScalar();
                    if (o == null || o.GetType() == typeof(DBNull))
                    {
                        cmd.CommandText = "create table 'project_info' (item_name text, item_value text)";
                        cmd.ExecuteNonQuery();
                    }

                    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='geocache_cfields'";
                    o = cmd.ExecuteScalar();
                    if (o == null || o.GetType() == typeof(DBNull))
                    {
                        cmd.CommandText = "create table 'geocache_cfields' (field_name text)";
                        cmd.ExecuteNonQuery();
                    }

                    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='geocache'";
                    o = cmd.ExecuteScalar();
                    if (o == null || o.GetType() == typeof(DBNull))
                    {
                        cmd.CommandText = "create table 'geocache' (id text, code text, name text, datafromdate text, lat real, lon real, disttocent integer, angletocent integer, available integer, archived integer, country text, state text, cachetype integer, placedby text, owner text, ownerid text, container integer, terrain real, difficulty real, shortdescr text, shortdescrhtml integer, longdescr text, longdescrhtml integer, encodedhints text, url text, memberonly integer, customcoords integer, attrids text, favorites integer, selected integer, municipality text, city text, customlat real, customlon real, notes text, publiceddate text, personalnote text, flagged integer, found integer, locked integer)";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "create unique index idx_geocache on geocache (code)";
                        cmd.ExecuteNonQuery();
                    }
                    else if (!ColumnExists(dbcon, "geocache", "locked"))
                    {
                        cmd.CommandText = "alter table geocache add locked int";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "update geocache set locked=0";
                        cmd.ExecuteNonQuery();
                    }

                    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='log'";
                    o = cmd.ExecuteScalar();
                    if (o == null || o.GetType() == typeof(DBNull))
                    {
                        cmd.CommandText = "create table 'log' (id text, gccode text, tbcode text, date text, finder text, finderid text, logtext text, encoded integer, datafromdate text, logtype integer)";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "create unique index idx_log on log (id)";
                        cmd.ExecuteNonQuery();
                    }

                    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='logimage'";
                    o = cmd.ExecuteScalar();
                    if (o == null || o.GetType() == typeof(DBNull))
                    {
                        cmd.CommandText = "create table 'logimage' (id text, logid text, url text, name text, datafromdate text)";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "create unique index idx_logimage on logimage (id)";
                        cmd.ExecuteNonQuery();
                    }

                    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='waypoint'";
                    o = cmd.ExecuteScalar();
                    if (o == null || o.GetType() == typeof(DBNull))
                    {
                        cmd.CommandText = "create table 'waypoint' (id text, code text, geocachecode text, name text, datafromdate text, comment text, description text, url text, urlname text, wptype integer, lat real, lon real, time text)";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "create unique index idx_waypoint on waypoint (code)";
                        cmd.ExecuteNonQuery();
                    }

                    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='userwaypoint'";
                    o = cmd.ExecuteScalar();
                    if (o == null || o.GetType() == typeof(DBNull))
                    {
                        cmd.CommandText = "create table 'userwaypoint' (id integer, geocachecode text, description text, lat real, lon real, time text)";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "create unique index idx_userwaypoint on userwaypoint (id)";
                        cmd.ExecuteNonQuery();
                    }

                    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='counter'";
                    o = cmd.ExecuteScalar();
                    if (o == null || o.GetType() == typeof(DBNull))
                    {
                        cmd.CommandText = "create table 'counter' (geocache integer, log integer, waypoint integer, logimage integer)";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "insert into counter (geocache, log, waypoint, logimage) values (0, 0, 0, 0)";
                        cmd.ExecuteNonQuery();
                    }
                }
                result = true;
            }
            catch
            {
            }
            return result;
        }
    }
}
