using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Web;

namespace GlobalcachingApplication.Plugins.Cachebox
{
    public class Export : Utils.BasePlugin.BaseExportFilter
    {
        public const string ACTION_EXPORT_ALL = "Export to Cachebox|All";
        public const string ACTION_EXPORT_SELECTED = "Export to Cachebox|Selected";
        public const string ACTION_EXPORT_ACTIVE = "Export to Cachebox|Active";

        public const string STR_NOGEOCACHESELECTED = "No geocache selected for export";
        public const string STR_ERROR = "Error";
        public const string STR_EXPORTINGCB = "Exporting to Cachebox...";
        public const string STR_CREATINGFILE = "Creating file...";

        private string _folder = null;
        private List<Framework.Data.Geocache> _gcList = null;
        private Utils.DBConComSqlite _dbcon = null;

        public override string FriendlyName
        {
            get { return "Export to Cachebox"; }
        }

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            AddAction(ACTION_EXPORT_ALL);
            AddAction(ACTION_EXPORT_SELECTED);
            AddAction(ACTION_EXPORT_ACTIVE);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_EXPORTINGCB));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CREATINGFILE));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_GRABBEDIMG));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_MAXLOGS));

            return base.Initialize(core);
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_EXPORT_ALL || action == ACTION_EXPORT_SELECTED || action == ACTION_EXPORT_ACTIVE)
                {
                    if (action == ACTION_EXPORT_ALL)
                    {
                        _gcList = (from Framework.Data.Geocache a in Core.Geocaches select a).ToList();
                    }
                    else if (action == ACTION_EXPORT_SELECTED)
                    {
                        _gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                    }
                    else
                    {
                        if (Core.ActiveGeocache != null)
                        {
                            _gcList = new List<Framework.Data.Geocache>();
                            _gcList.Add(Core.ActiveGeocache);
                        }
                    }
                    if (_gcList == null || _gcList.Count == 0)
                    {
                        System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                    else
                    {
                        using (System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog())
                        {
                            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                _folder = dlg.SelectedPath;
                                PerformExport();
                            }
                        }
                    }
                }
            }
            return result;
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

        protected override void ExportMethod()
        {
            int max = _gcList.Count * 3; //caches, waypoints, logs
            int index = 0;
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_EXPORTINGCB, STR_CREATINGFILE, max, 0))
            {
                string cbFile = System.IO.Path.Combine(_folder, "cachebox.db3");

                if (System.IO.File.Exists(cbFile))
                {
                    System.IO.File.Delete(cbFile);
                }
                CreateDatabase(cbFile);

                if (_dbcon != null)
                {
                    int fixedCatId = 1;
                    string fixedGpxFilename = "12345678.gpx";

                    _dbcon.ExecuteNonQuery("PRAGMA user_version = 1022");

                    _dbcon.ExecuteNonQuery(string.Format("insert into Config (Key, Value) values ('{0}', '{1}')", "DatabaseSchemeVersion", "1022"));
                    _dbcon.ExecuteNonQuery(string.Format("insert into Config (Key, Value) values ('{0}', '{1}')", "DatabaseSchemeVersionWin", "1022"));
                    _dbcon.ExecuteNonQuery(string.Format("insert into Config (Key, Value) values ('{0}', '{1}')", "DatabaseId", DateTime.Now.ToFileTime()));
                    _dbcon.ExecuteNonQuery(string.Format("insert into Config (Key, Value) values ('{0}', '{1}')", "MasterDatabaseId", DateTime.Now.ToFileTime()+1));

                    _dbcon.ExecuteNonQuery(string.Format("insert into Category (Id, GpxFilename, Pinned) values ({0}, '{1}', {2})", fixedCatId, fixedGpxFilename, 0));
                    _dbcon.ExecuteNonQuery(string.Format("insert into GPXFilenames (Id, GpxFilename, Imported, CategoryId) values ({0}, '{1}', '{2}', {3})", 1, fixedGpxFilename, DateTime.Now.ToString("s"), fixedCatId));


                    //----------------------------
                    // CACHES
                    //----------------------------
                    DbCommand cmd = _dbcon.Command;
                    cmd.CommandText = "insert into Caches (Id, GcCode, GcId, Latitude, Longitude, Name, Size, Difficulty, Terrain, Archived, Available, Found, Type, PlacedBy, Owner, DateHidden, Hint, Description, Url, NumTravelbugs, Rating, Vote, VotePending, Notes, Solver, Favorit, AttributesPositive, AttributesNegative, TourName, GPXFilename_Id, HasUserData, ListingChanged, ImagesUpdated, DescriptionImagesUpdated, CorrectedCoordinates, AttributesPositiveHigh, AttributesNegativeHigh, State, Country) values (@Id, @GcCode, @GcId, @Latitude, @Longitude, @Name, @Size, @Difficulty, @Terrain, @Archived, @Available, @Found, @Type, @PlacedBy, @Owner, @DateHidden, @Hint, @Description, @Url, @NumTravelbugs, @Rating, @Vote, @VotePending, @Notes, @Solver, @Favorit, @AttributesPositive, @AttributesNegative, @TourName, @GPXFilename_Id, @HasUserData, @ListingChanged, @ImagesUpdated, @DescriptionImagesUpdated, @CorrectedCoordinates, @AttributesPositiveHigh, @AttributesNegativeHigh, @State, @Country)";
                    DbParameter par;
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Id";
                    par.DbType = DbType.Int64;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@GcCode";
                    par.DbType = DbType.String;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@GcId";
                    par.DbType = DbType.String;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Latitude";
                    par.DbType = DbType.Single;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Longitude";
                    par.DbType = DbType.Single;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Name";
                    par.DbType = DbType.String;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Size";
                    par.DbType = DbType.Int16;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Difficulty";
                    par.DbType = DbType.Int16;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Terrain";
                    par.DbType = DbType.Int16;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Archived";
                    par.DbType = DbType.Boolean;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Available";
                    par.DbType = DbType.Boolean;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Found";
                    par.DbType = DbType.Boolean;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Type";
                    par.DbType = DbType.Int16;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@PlacedBy";
                    par.DbType = DbType.String;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Owner";
                    par.DbType = DbType.String;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@DateHidden";
                    par.DbType = DbType.DateTime;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Hint";
                    par.DbType = DbType.String;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Description";
                    par.DbType = DbType.String;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Url";
                    par.DbType = DbType.String;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@NumTravelbugs";
                    par.DbType = DbType.Int16;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Rating";
                    par.DbType = DbType.Int16;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Vote";
                    par.DbType = DbType.Int16;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@VotePending";
                    par.DbType = DbType.Boolean;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Notes";
                    par.DbType = DbType.String;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Solver";
                    par.DbType = DbType.String;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Favorit";
                    par.DbType = DbType.Boolean;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@AttributesPositive";
                    par.DbType = DbType.Int64;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@AttributesNegative";
                    par.DbType = DbType.Int64;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@TourName";
                    par.DbType = DbType.String;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@GPXFilename_Id";
                    par.DbType = DbType.Int64;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@HasUserData";
                    par.DbType = DbType.Boolean;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@ListingChanged";
                    par.DbType = DbType.Boolean;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@ImagesUpdated";
                    par.DbType = DbType.Boolean;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@DescriptionImagesUpdated";
                    par.DbType = DbType.Boolean;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@CorrectedCoordinates";
                    par.DbType = DbType.Boolean;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@AttributesPositiveHigh";
                    par.DbType = DbType.Int64;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@AttributesNegativeHigh";
                    par.DbType = DbType.Int64;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@State";
                    par.DbType = DbType.String;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Country";
                    par.DbType = DbType.String;
                    cmd.Parameters.Add(par);

                    long startCacheId = DateTime.Now.ToFileTime() + 2;
                    long cacheId = startCacheId;
                    foreach (Framework.Data.Geocache gc in _gcList)
                    {
                        cmd.Parameters["@Id"].Value = cacheId;
                        cmd.Parameters["@GcCode"].Value = gc.Code;
                        cmd.Parameters["@GcId"].Value = gc.ID;
                        if (gc.ContainsCustomLatLon)
                        {
                            cmd.Parameters["@Latitude"].Value = (float)gc.CustomLat;
                            cmd.Parameters["@Longitude"].Value = (float)gc.CustomLon;
                        }
                        else
                        {
                            cmd.Parameters["@Latitude"].Value = (float)gc.Lat;
                            cmd.Parameters["@Longitude"].Value = (float)gc.Lon;
                        }
                        cmd.Parameters["@Name"].Value = gc.Name ?? "";
                        switch (gc.Container.ID)
                        {
                            case 2:
                                cmd.Parameters["@Size"].Value = 1;
                                break;
                            case 8:
                                cmd.Parameters["@Size"].Value = 2;
                                break;
                            case 4:
                                cmd.Parameters["@Size"].Value = 3;
                                break;
                            case 5:
                                cmd.Parameters["@Size"].Value = 4;
                                break;
                            default:
                                cmd.Parameters["@Size"].Value = 0;
                                break;
                        }
                        cmd.Parameters["@Difficulty"].Value = (Int16)(gc.Difficulty * 2.0);
                        cmd.Parameters["@Terrain"].Value = (Int16)(gc.Terrain * 2.0);
                        cmd.Parameters["@Archived"].Value = gc.Archived;
                        cmd.Parameters["@Available"].Value = gc.Available;
                        cmd.Parameters["@Found"].Value = gc.Found;
                        switch (gc.GeocacheType.ID)
                        {
                            case 2:
                                cmd.Parameters["@Type"].Value = 0;
                                break;
                            case 3:
                                cmd.Parameters["@Type"].Value = 1;
                                break;
                            case 4:
                                cmd.Parameters["@Type"].Value = 8;
                                break;
                            case 5:
                                cmd.Parameters["@Type"].Value = 9;
                                break;
                            case 6:
                                cmd.Parameters["@Type"].Value = 5;
                                break;
                            case 8:
                                cmd.Parameters["@Type"].Value = 2;
                                break;
                            case 11:
                                cmd.Parameters["@Type"].Value = 3;
                                break;
                            case 13:
                                cmd.Parameters["@Type"].Value = 7;
                                break;
                            case 137:
                                cmd.Parameters["@Type"].Value = 4;
                                break;
                            case 453:
                                cmd.Parameters["@Type"].Value = 6;
                                break;
                            case 1858:
                                cmd.Parameters["@Type"].Value = 10;
                                break;
                            default:
                                cmd.Parameters["@Type"].Value = 13;
                                break;
                        }
                        cmd.Parameters["@PlacedBy"].Value = gc.PlacedBy ?? "";
                        cmd.Parameters["@Owner"].Value = gc.Owner ?? "";
                        cmd.Parameters["@DateHidden"].Value = gc.PublishedTime;
                        cmd.Parameters["@Hint"].Value = gc.EncodedHints ?? "";
                        StringBuilder sb = new StringBuilder();
                        if (!string.IsNullOrEmpty(gc.ShortDescription))
                        {
                            if (gc.ShortDescriptionInHtml)
                            {
                                sb.Append(gc.ShortDescription);
                            }
                            else
                            {
                                sb.Append(HttpUtility.HtmlEncode(gc.ShortDescription).Replace("\r", "").Replace("\n", "<br />"));
                            }
                            sb.Append("<br />");
                        }
                        if (!string.IsNullOrEmpty(gc.LongDescription))
                        {
                            if (gc.LongDescriptionInHtml)
                            {
                                sb.Append(gc.LongDescription);
                            }
                            else
                            {
                                sb.Append(HttpUtility.HtmlEncode(gc.LongDescription).Replace("\r", "").Replace("\n", "<br />"));
                            }
                        }
                        cmd.Parameters["@Description"].Value = sb.ToString();
                        cmd.Parameters["@Url"].Value = gc.Url ?? "";
                        cmd.Parameters["@NumTravelbugs"].Value = 0;
                        cmd.Parameters["@Rating"].Value = 0;
                        cmd.Parameters["@Vote"].Value = 0;
                        cmd.Parameters["@VotePending"].Value = false;
                        sb.Length = 0;
                        if (gc.ContainsNote)
                        {
                            if (!string.IsNullOrEmpty(gc.PersonaleNote))
                            {
                                sb.Append(gc.PersonaleNote);
                                sb.Append(" - ");
                            }
                            if (!string.IsNullOrEmpty(gc.Notes))
                            {
                                sb.Append(Utils.Conversion.StripHtmlTags(gc.Notes));
                            }
                        }
                        cmd.Parameters["@Notes"].Value = sb.Length;
                        cmd.Parameters["@Solver"].Value = "";
                        cmd.Parameters["@Favorit"].Value = false;

                        DLong tmpAttributesNegative = new DLong(0, 0);
                        DLong tmpAttributesPositive = new DLong(0, 0);
                        List<int> attrs = gc.AttributeIds;
                        foreach (int ix in attrs)
                        {
                            if (ix > 0)
                            {
                                tmpAttributesPositive.BitOr(Attributes.GetAttributeDlong(Attributes.getAttributeEnumByGcComId(ix).Attribute));
                            }
                            else
                            {
                                tmpAttributesNegative.BitOr(Attributes.GetAttributeDlong(Attributes.getAttributeEnumByGcComId(-ix).Attribute));
                            }
                        }
                        long AttributePositiveLow = (long)tmpAttributesPositive.getLow();
                        long AttributePositiveHigh = (long)tmpAttributesPositive.getHigh();
                        long AttributesNegativeLow = (long)tmpAttributesNegative.getLow();
                        long AttributesNegativeHigh = (long)tmpAttributesNegative.getHigh();
                        cmd.Parameters["@AttributesPositive"].Value = AttributePositiveLow;
                        cmd.Parameters["@AttributesNegative"].Value = AttributesNegativeLow; 
                        cmd.Parameters["@AttributesPositiveHigh"].Value = AttributePositiveHigh;
                        cmd.Parameters["@AttributesNegativeHigh"].Value = AttributesNegativeHigh;
                        cmd.Parameters["@TourName"].Value = "";
                        cmd.Parameters["@AttributesNegative"].Value = 1;
                        cmd.Parameters["@HasUserData"].Value = false;
                        cmd.Parameters["@ListingChanged"].Value = false;
                        cmd.Parameters["@ImagesUpdated"].Value = false;
                        cmd.Parameters["@DescriptionImagesUpdated"].Value = false;
                        cmd.Parameters["@CorrectedCoordinates"].Value = false;
                        cmd.Parameters["@State"].Value = gc.State ?? "";
                        cmd.Parameters["@Country"].Value = gc.Country ?? "";

                        cmd.ExecuteNonQuery();
                        cacheId++;

                        index++;
                        if (index % 200 == 0)
                        {
                            progress.UpdateProgress(STR_EXPORTINGCB, STR_CREATINGFILE, max, index);
                        }
                    }

                    //----------------------------
                    // WAYPOINTS
                    //----------------------------
                    cmd.Parameters.Clear();
                    cmd.CommandText = "insert into Waypoint (GcCode, CacheId, Latitude, Longitude, Description, Clue, Type, SyncExclude, UserWaypoint, Title) values (@GcCode, @CacheId, @Latitude, @Longitude, @Description, @Clue, @Type, @SyncExclude, @UserWaypoint, @Title)";
                    par = cmd.CreateParameter();
                    par.ParameterName = "@GcCode";
                    par.DbType = DbType.String;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@CacheId";
                    par.DbType = DbType.Int64;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Latitude";
                    par.DbType = DbType.Single;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Longitude";
                    par.DbType = DbType.Single;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Description";
                    par.DbType = DbType.String;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Clue";
                    par.DbType = DbType.String;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Type";
                    par.DbType = DbType.Int16;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@SyncExclude";
                    par.DbType = DbType.Boolean;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@UserWaypoint";
                    par.DbType = DbType.Boolean;
                    cmd.Parameters.Add(par);
                    par = cmd.CreateParameter();
                    par.ParameterName = "@Title";
                    par.DbType = DbType.String;
                    cmd.Parameters.Add(par);

                    cacheId = startCacheId;
                    foreach (Framework.Data.Geocache gc in _gcList)
                    {
                        List<Framework.Data.Waypoint> wps = Utils.DataAccess.GetWaypointsFromGeocache(Core.Waypoints, gc.Code);
                        if (wps != null && wps.Count > 0)
                        {
                            foreach (Framework.Data.Waypoint wp in wps)
                            {
                                if (wp.Lat != null && wp.Lon != null)
                                {
                                    cmd.Parameters["@GcCode"].Value = wp.Code;
                                    cmd.Parameters["@CacheId"].Value = cacheId;
                                    cmd.Parameters["@Latitude"].Value = (float)(double)wp.Lat;
                                    cmd.Parameters["@Longitude"].Value = (float)(double)wp.Lon;
                                    cmd.Parameters["@Description"].Value = wp.Comment ?? "";
                                    cmd.Parameters["@Clue"].Value = "";
                                    switch (wp.WPType.ID)
                                    {
                                        case 217:
                                            cmd.Parameters["@Type"].Value = 17;
                                            break;
                                        case 220:
                                            cmd.Parameters["@Type"].Value = 18;
                                            break;
                                        case 218:
                                            cmd.Parameters["@Type"].Value = 15;
                                            break;
                                        case 452:
                                            cmd.Parameters["@Type"].Value = 11;
                                            break;
                                        case 219:
                                            cmd.Parameters["@Type"].Value = 14;
                                            break;
                                        case 221:
                                            cmd.Parameters["@Type"].Value = 16;
                                            break;
                                        default:
                                            cmd.Parameters["@Type"].Value = 13;
                                            break;
                                    }
                                    cmd.Parameters["@SyncExclude"].Value = false;
                                    cmd.Parameters["@UserWaypoint"].Value = string.IsNullOrEmpty(wp.Url) || !wp.Url.ToLower().StartsWith("http:");
                                    cmd.Parameters["@Title"].Value = wp.Description ?? "";

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        cacheId++;

                        index++;
                        if (index % 200 == 0)
                        {
                            progress.UpdateProgress(STR_EXPORTINGCB, STR_CREATINGFILE, max, index);
                        }
                    }
                    //----------------------------
                    // LOGS
                    //----------------------------
                    if (Properties.Settings.Default.MaxLogs > 0)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "insert into Logs (Id, CacheId, Timestamp, Finder, Type, Comment) values (@Id, @CacheId, @Timestamp, @Finder, @Type, @Comment)";
                        par = cmd.CreateParameter();
                        par.ParameterName = "@Id";
                        par.DbType = DbType.Int64;
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@CacheId";
                        par.DbType = DbType.Int64;
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@Timestamp";
                        par.DbType = DbType.DateTime;
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@Finder";
                        par.DbType = DbType.String;
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@Type";
                        par.DbType = DbType.Int16;
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@Comment";
                        par.DbType = DbType.String;
                        cmd.Parameters.Add(par);

                        long logId = 1;
                        cacheId = startCacheId;
                        foreach (Framework.Data.Geocache gc in _gcList)
                        {
                            List<Framework.Data.Log> logs = Utils.DataAccess.GetLogs(Core.Logs, gc.Code).Take(Properties.Settings.Default.MaxLogs).ToList();
                            if (logs != null && logs.Count > 0)
                            {
                                logs = logs.Take(Properties.Settings.Default.MaxLogs).ToList();
                                foreach (Framework.Data.Log lg in logs)
                                {
                                    long id = logId;
                                    if (lg.ID.StartsWith("GL"))
                                    {
                                        id = Utils.Conversion.GetCacheIDFromCacheCode(lg.ID);
                                    }
                                    else
                                    {
                                        long.TryParse(lg.ID, out id);
                                    }
                                    cmd.Parameters["@Id"].Value = id;
                                    cmd.Parameters["@CacheId"].Value = cacheId;
                                    cmd.Parameters["@Timestamp"].Value = lg.Date;
                                    cmd.Parameters["@Finder"].Value = lg.Finder ?? "";
                                    switch (lg.LogType.ID)
                                    {
                                        case 2:
                                            cmd.Parameters["@Type"].Value = 0;
                                            break;
                                        case 3:
                                            cmd.Parameters["@Type"].Value = 1;
                                            break;
                                        case 4:
                                            cmd.Parameters["@Type"].Value = 2;
                                            break;
                                        case 24:
                                            cmd.Parameters["@Type"].Value = 3;
                                            break;
                                        case 23:
                                            cmd.Parameters["@Type"].Value = 4;
                                            break;
                                        case 45:
                                            cmd.Parameters["@Type"].Value = 5;
                                            break;
                                        case 22:
                                            cmd.Parameters["@Type"].Value = 6;
                                            break;
                                        case 46:
                                        case 47:
                                            cmd.Parameters["@Type"].Value = 7;
                                            break;
                                        case 9:
                                            cmd.Parameters["@Type"].Value = 8;
                                            break;
                                        case 10:
                                            cmd.Parameters["@Type"].Value = 9;
                                            break;
                                        case 11:
                                            cmd.Parameters["@Type"].Value = 10;
                                            break;
                                        case 5:
                                        case 6:
                                        case 12:
                                            cmd.Parameters["@Type"].Value = 11;
                                            break;
                                        case 18:
                                            cmd.Parameters["@Type"].Value = 12;
                                            break;
                                        case 7:
                                            cmd.Parameters["@Type"].Value = 13;
                                            break;
                                        default:
                                            cmd.Parameters["@Type"].Value = 2;
                                            break;
                                    }
                                    cmd.Parameters["@Comment"].Value = lg.Text ?? "";

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            cacheId++;

                            index++;
                            if (index % 200 == 0)
                            {
                                progress.UpdateProgress(STR_EXPORTINGCB, STR_CREATINGFILE, max, index);
                            }
                        }
                    }


                    _dbcon.Dispose();
                }
            }
        }

        private void CreateDatabase(string filename)
        {
            _dbcon = new Utils.DBConComSqlite(filename);

            //most code copied from Cachebox (Android)
            int lastDatabaseSchemeVersion = -1;

            if (lastDatabaseSchemeVersion <= 0)
            {
                // First Initialization of the Database
                execSQL("CREATE TABLE [Caches] ([Id] bigint NOT NULL primary key,[GcCode] nvarchar(15) NOT NULL,[GcId] nvarchar (255) NULL,[Latitude] float NULL,[Longitude] float NULL,[Name] nchar (255) NULL,[Size] int NULL,[Difficulty] smallint NULL,[Terrain] smallint NULL,[Archived] bit NULL,[Available] bit NULL,[Found] bit NULL,[Type] smallint NULL,[PlacedBy] nvarchar (255) NULL,[Owner] nvarchar (255) NULL,[DateHidden] datetime NULL,[Hint] ntext NULL,[Description] ntext NULL,[Url] nchar (255) NULL,[NumTravelbugs] smallint NULL,[Rating] smallint NULL,[Vote] smallint NULL,[VotePending] bit NULL,[Notes] ntext NULL,[Solver] ntext NULL,[Favorit] bit NULL,[AttributesPositive] bigint NULL,[AttributesNegative] bigint NULL,[TourName] nchar (255) NULL,[GPXFilename_Id] bigint NULL,[HasUserData] bit NULL,[ListingCheckSum] int NULL DEFAULT 0,[ListingChanged] bit NULL,[ImagesUpdated] bit NULL,[DescriptionImagesUpdated] bit NULL,[CorrectedCoordinates] bit NULL);");
                execSQL("CREATE INDEX [archived_idx] ON [Caches] ([Archived] ASC);");
                execSQL("CREATE INDEX [AttributesNegative_idx] ON [Caches] ([AttributesNegative] ASC);");
                execSQL("CREATE INDEX [AttributesPositive_idx] ON [Caches] ([AttributesPositive] ASC);");
                execSQL("CREATE INDEX [available_idx] ON [Caches] ([Available] ASC);");
                execSQL("CREATE INDEX [Difficulty_idx] ON [Caches] ([Difficulty] ASC);");
                execSQL("CREATE INDEX [Favorit_idx] ON [Caches] ([Favorit] ASC);");
                execSQL("CREATE INDEX [found_idx] ON [Caches] ([Found] ASC);");
                execSQL("CREATE INDEX [GPXFilename_Id_idx] ON [Caches] ([GPXFilename_Id] ASC);");
                execSQL("CREATE INDEX [HasUserData_idx] ON [Caches] ([HasUserData] ASC);");
                execSQL("CREATE INDEX [ListingChanged_idx] ON [Caches] ([ListingChanged] ASC);");
                execSQL("CREATE INDEX [NumTravelbugs_idx] ON [Caches] ([NumTravelbugs] ASC);");
                execSQL("CREATE INDEX [placedby_idx] ON [Caches] ([PlacedBy] ASC);");
                execSQL("CREATE INDEX [Rating_idx] ON [Caches] ([Rating] ASC);");
                execSQL("CREATE INDEX [Size_idx] ON [Caches] ([Size] ASC);");
                execSQL("CREATE INDEX [Terrain_idx] ON [Caches] ([Terrain] ASC);");
                execSQL("CREATE INDEX [Type_idx] ON [Caches] ([Type] ASC);");

                execSQL("CREATE TABLE [CelltowerLocation] ([CellId] nvarchar (20) NOT NULL primary key,[Latitude] float NULL,[Longitude] float NULL);");

                execSQL("CREATE TABLE [GPXFilenames] ([Id] integer not null primary key autoincrement,[GPXFilename] nvarchar (255) NULL,[Imported] datetime NULL, [Name] nvarchar (255) NULL,[CacheCount] int NULL);");

                execSQL("CREATE TABLE [Logs] ([Id] bigint NOT NULL primary key, [CacheId] bigint NULL,[Timestamp] datetime NULL,[Finder] nvarchar (128) NULL,[Type] smallint NULL,[Comment] ntext NULL);");
                execSQL("CREATE INDEX [log_idx] ON [Logs] ([CacheId] ASC);");
                execSQL("CREATE INDEX [timestamp_idx] ON [Logs] ([Timestamp] ASC);");

                execSQL("CREATE TABLE [PocketQueries] ([Id] integer not null primary key autoincrement,[PQName] nvarchar (255) NULL,[CreationTimeOfPQ] datetime NULL);");

                execSQL("CREATE TABLE [Waypoint] ([GcCode] nvarchar(15) NOT NULL primary key,[CacheId] bigint NULL,[Latitude] float NULL,[Longitude] float NULL,[Description] ntext NULL,[Clue] ntext NULL,[Type] smallint NULL,[SyncExclude] bit NULL,[UserWaypoint] bit NULL,[Title] ntext NULL);");
                execSQL("CREATE INDEX [UserWaypoint_idx] ON [Waypoint] ([UserWaypoint] ASC);");

                execSQL("CREATE TABLE [Config] ([Key] nvarchar (30) NOT NULL, [Value] nvarchar (255) NULL);");
                execSQL("CREATE INDEX [Key_idx] ON [Config] ([Key] ASC);");

                execSQL("CREATE TABLE [Replication] ([Id] integer not null primary key autoincrement, [ChangeType] int NOT NULL, [CacheId] bigint NOT NULL, [WpGcCode] nvarchar(15) NOT NULL, [SolverCheckSum] int NULL, [NotesCheckSum] int NULL, [WpCoordCheckSum] int NULL);");
                execSQL("CREATE INDEX [Replication_idx] ON [Replication] ([Id] ASC);");
                execSQL("CREATE INDEX [ReplicationCache_idx] ON [Replication] ([CacheId] ASC);");
            }

				if (lastDatabaseSchemeVersion < 1003)
				{
					execSQL("CREATE TABLE [Locations] ([Id] integer not null primary key autoincrement, [Name] nvarchar (255) NULL, [Latitude] float NULL, [Longitude] float NULL);");
					execSQL("CREATE INDEX [Locatioins_idx] ON [Locations] ([Id] ASC);");

					execSQL("CREATE TABLE [SdfExport] ([Id]  integer not null primary key autoincrement, [Description] nvarchar(255) NULL, [ExportPath] nvarchar(255) NULL, [MaxDistance] float NULL, [LocationID] Bigint NULL, [Filter] ntext NULL, [Update] bit NULL, [ExportImages] bit NULL, [ExportSpoilers] bit NULL, [ExportMaps] bit NULL, [OwnRepository] bit NULL, [ExportMapPacks] bit NULL, [MaxLogs] int NULL);");
					execSQL("CREATE INDEX [SdfExport_idx] ON [SdfExport] ([Id] ASC);");

					execSQL("ALTER TABLE [CACHES] ADD [FirstImported] datetime NULL;");

					execSQL("CREATE TABLE [Category] ([Id]  integer not null primary key autoincrement, [GpxFilename] nvarchar(255) NULL, [Pinned] bit NULL default 0, [CacheCount] int NULL);");
					execSQL("CREATE INDEX [Category_idx] ON [Category] ([Id] ASC);");

					execSQL("ALTER TABLE [GpxFilenames] ADD [CategoryId] bigint NULL;");

					execSQL("ALTER TABLE [Caches] add [state] nvarchar(50) NULL;");
					execSQL("ALTER TABLE [Caches] add [country] nvarchar(50) NULL;");
				}
				if (lastDatabaseSchemeVersion < 1015)
				{
					// GpxFilenames mit Kategorien verknüpfen

					// alte Category Tabelle löschen
                    //altered, always is empty
				}
				if (lastDatabaseSchemeVersion < 1016)
				{
					execSQL("ALTER TABLE [CACHES] ADD [ApiStatus] smallint NULL default 0;");
				}
				if (lastDatabaseSchemeVersion < 1017)
				{
                    execSQL("CREATE TABLE [Trackable] ([Id] integer not null primary key autoincrement, [Archived] bit NULL, [GcCode] nvarchar(15) NOT NULL, [CacheId] bigint NULL, [CurrentGoal] ntext, [CurrentOwnerName] nvarchar (255) NULL, [DateCreated] datetime NULL, [Description] ntext, [IconUrl] nvarchar (255) NULL, [ImageUrl] nvarchar (255) NULL, [name] nvarchar (255) NULL, [OwnerName] nvarchar (255), [Url] nvarchar (255) NULL);");
					execSQL("CREATE INDEX [cacheid_idx] ON [Trackable] ([CacheId] ASC);");
					execSQL("CREATE TABLE [TbLogs] ([Id] integer not null primary key autoincrement, [TrackableId] integer not NULL, [CacheID] bigint NULL, [GcCode] nvarchar (15) NULL, [LogIsEncoded] bit NULL DEFAULT 0, [LogText] ntext, [LogTypeId] bigint NULL, [LoggedByName] nvarchar (255) NULL, [Visited] datetime NULL);");
					execSQL("CREATE INDEX [trackableid_idx] ON [TbLogs] ([TrackableId] ASC);");
					execSQL("CREATE INDEX [trackablecacheid_idx] ON [TBLOGS] ([CacheId] ASC);");
				}
				if (lastDatabaseSchemeVersion < 1018)
				{
					execSQL("ALTER TABLE [SdfExport] ADD [MapPacks] nvarchar(512) NULL;");

				}
				if (lastDatabaseSchemeVersion < 1019)
				{
					// neue Felder für die erweiterten Attribute einfügen
					execSQL("ALTER TABLE [CACHES] ADD [AttributesPositiveHigh] bigint NULL default 0");
					execSQL("ALTER TABLE [CACHES] ADD [AttributesNegativeHigh] bigint NULL default 0");

					// Die Nummerierung der Attribute stimmte nicht mit der von
					// Groundspeak überein. Bei 16 und 45 wurde jeweils eine
					// Nummber übersprungen
                    //altered, always empty
				}
				if (lastDatabaseSchemeVersion < 1020)
				{
					// for long Settings
					execSQL("ALTER TABLE [Config] ADD [LongString] ntext NULL;");

				}
				if (lastDatabaseSchemeVersion < 1021)
				{
					// Image Table
					execSQL("CREATE TABLE [Images] ([Id] integer not null primary key autoincrement, [CacheId] bigint NULL, [GcCode] nvarchar (15) NULL, [Description] ntext, [Name] nvarchar (255) NULL, [ImageUrl] nvarchar (255) NULL, [IsCacheImage] bit NULL);");
					execSQL("CREATE INDEX [images_cacheid_idx] ON [Images] ([CacheId] ASC);");
					execSQL("CREATE INDEX [images_gccode_idx] ON [Images] ([GcCode] ASC);");
					execSQL("CREATE INDEX [images_iscacheimage_idx] ON [Images] ([IsCacheImage] ASC);");
					execSQL("CREATE UNIQUE INDEX [images_imageurl_idx] ON [Images] ([ImageUrl] ASC);");
				}
				if (lastDatabaseSchemeVersion < 1022)
				{
					//execSQL("ALTER TABLE [Caches] ALTER COLUMN [GcCode] nvarchar(15) NOT NULL; ");

					//execSQL("ALTER TABLE [Waypoint] DROP CONSTRAINT Waypoint_PK ");
					//execSQL("ALTER TABLE [Waypoint] ALTER COLUMN [GcCode] nvarchar(15) NOT NULL; ");
					//execSQL("ALTER TABLE [Waypoint] ADD CONSTRAINT  [Waypoint_PK] PRIMARY KEY ([GcCode]); ");

					//execSQL("ALTER TABLE [Replication] ALTER COLUMN [WpGcCode] nvarchar(15) NOT NULL; ");
					//execSQL("ALTER TABLE [Trackable] ALTER COLUMN [GcCode] nvarchar(15) NOT NULL; ");
					//execSQL("ALTER TABLE [TbLogs] ALTER COLUMN [GcCode] nvarchar(15) NOT NULL; ");
					//execSQL("ALTER TABLE [Images] ALTER COLUMN [GcCode] nvarchar(15) NOT NULL; ");
				}

        }

        private void execSQL(string q)
        {
            _dbcon.ExecuteNonQuery(q);
        }
    }
}
