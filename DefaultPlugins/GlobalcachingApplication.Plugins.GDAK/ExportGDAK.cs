using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using Community.CsharpSqlite.SQLiteClient;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.GDAK
{
    public class ExportGDAK : Utils.BasePlugin.BaseExportFilter
    {
        public const string STR_NOGEOCACHESELECTED = "No geocache selected for export";
        public const string STR_ERROR = "Error";
        public const string STR_EXPORTINGGPX = "Exporting GDAK...";
        public const string STR_CREATINGFILE = "Creating file...";
        public const string STR_SAVING = "Saving...";
        public const string STR_SAVINGDATA = "Saving data...";
        public const string STR_SAVINGGEOCACHES = "Saving geocaches...";

        public const string ACTION_EXPORT_ALL = "Export GDAK|All";
        public const string ACTION_EXPORT_SELECTED = "Export GDAK|Selected";

        private string _filename = null;
        private List<Framework.Data.Geocache> _gcList = null;

        public override string FriendlyName
        {
            get { return "Export GDAK"; }
        }

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            var p = new PluginSettings(core);

            AddAction(ACTION_EXPORT_ALL);
            AddAction(ACTION_EXPORT_SELECTED);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_EXPORTINGGPX));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CREATINGFILE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SAVING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SAVINGDATA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SAVINGGEOCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_MAXLOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_GRABBEDIMG));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_MAXFILESINFOLDER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_MAXFILESINFOLDERNULL));


            return await base.InitializeAsync(core);
        }

        public async override Task<bool> ActionAsync(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_EXPORT_ALL || action == ACTION_EXPORT_SELECTED)
                {
                    if (action == ACTION_EXPORT_ALL)
                    {
                        _gcList = (from Framework.Data.Geocache a in Core.Geocaches select a).ToList();
                    }
                    else if (action == ACTION_EXPORT_SELECTED)
                    {
                        _gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                    }
                    if (_gcList == null || _gcList.Count == 0)
                    {
                        System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                    else
                    {
                        using (System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog())
                        {
                            dlg.FileName = "sqlite.db3";
                            dlg.Filter = "sqlite.db3|sqlite.db3";
                            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                _filename = dlg.FileName;
                                await PerformExport();
                            }
                        }
                    }
                }
            }
            return result;
        }


        private string[] SQLCREATEDBTABLES = new String[]{
		"CREATE TABLE Attributes (aCode text, aId integer, aInc integer);",
		"CREATE TABLE CacheImages" +
		"  (iCode text default '' not null, iName iCode text default '' not null, iDescription iCode text default '' not null," +
		"  iGuid text default '' not null,iImage text default '' not null, iDownloaded integer default 0 not null);",
		"CREATE TABLE CacheMemo (Code text default \"\" not null,LongDescription text default \"\" not null, ShortDescription text default \"\" not null," +
		"Url text default \"\" not null,Hints text default \"\" not null,UserNote text default \"\" not null,TravelBugs text default \"\" not null);",
		"CREATE TABLE Caches" +
		"  (Code text default '' not null, Name text default '' not null, PlacedBy text default '' not null, Archived integer default 0 not null," +
		"   CacheId text default '1' not null, CacheType text default 'O' not null, Changed text default current_date not null," +
		"   Container text default 'Unknown' not null, Country text default '' not null," +
		"   Difficulty real default  1.0 not null, DNF integer default  0 not null, DNFDate text default '' not null, Found integer default  0 not null," +
		"   HasCorrected integer default  0 not null," +
		"   HasTravelBug integer default  0 not null, HasUserNote integer default 0 not null,  " +
		"   LastUserDate text default current_date  not null, Latitude text default '0.0' not null," +
		"   LongHtm integer default 0 not null, Longitude text default '0.0' not null, " +
		"   OwnerName text default '' not null," +
		"   PlacedDate text default current_date not null, ShortHtm integer default 0 not null,  " +
		"   State text default '' not null,  " +
		"   Terrain real default 1.0 not null, UserData text default '' not null, User2 text default '' not null, User3 text default '' not null, " +
		"   User4 text default '' not null, UserFlag integer default 0 not null, UserNoteDate text default  '' not null, " +
		"   IsOwner integer default 0 not null, LatOriginal text default '0.0' not null, LonOriginal text default '0.0' not null," +
		"   Created text default current_date not null, Status text default 'A' not null, " +
		"   GcNote text default '' not null,IsPremium Integer default 0 not null,Guid text default '' not null,FavPoints Integer default 0 not null, IsLite Integer default 1 not null);",
		"CREATE TABLE LogImages" +
		"  (iCode text default '' not null, iLogid Integer default 0, iName text default '' not null, iDescription iCode text default '' not null," +
		"  iGuid text default '' not null,iImage text default '' not null);",
		"CREATE TABLE LogMemo (lParent ,lLogId integer, lText default \"\");",
		"CREATE TABLE Logs (lParent not null,lLogId integer not null, lType, lBy, lDate date, lLat, lLon, lEncoded boolean, lownerid integer, lHasHtml boolean, lIsowner boolean, lTime text, lUploaded integer default 1 not null);",
		"CREATE TABLE WayMemo (cParent,cCode,cComment default \"\", cUrl default \"\");",
		"CREATE TABLE Waypoints (cParent not null,cCode not null, cPrefix, cName, cType, cLat, cLon, cByuser boolean, cDate date, cFlag boolean, sB1 boolean);",
		"Create TABLE OtherApp (name, value);"
	};
        private string[] SQLCREATEDBINDEXES = new string[]{
		"CREATE INDEX CacheImagesI1 on CacheImages (iCode);",
		"CREATE UNIQUE INDEX CachesCode on Caches (code);",
		"CREATE INDEX LogImagesI1 on LogImages (iCode,iLogId);",
		"CREATE UNIQUE INDEX MemoCode on  cachememo (code);",
		"CREATE UNIQUE INDEX acode on attributes (aCode,aId);",
		"CREATE UNIQUE INDEX logParent on  logmemo (lParent,lLogId);",
		"CREATE UNIQUE INDEX logkey on logs (lparent,llogid);",
		"CREATE UNIQUE INDEX wayParent on  waymemo (cParent,cCode);",
		"CREATE UNIQUE INDEX waykey on waypoints (cparent,ccode);"
	};
        private string[] SQLCREATEDBTRIGGERS = new string[]{		
		"CREATE TRIGGER Delete_Caches Delete ON Caches" +
		" BEGIN" +
		" delete from logs where lparent = old.code;" +
		" delete from waypoints where cparent = old.code;" +
		" delete from CacheMemo where code = old.code;" +
		" delete from Attributes where acode = old.code;" +
		" delete from CacheImages where iCode = old.code;" +
		" END;",
		"CREATE TRIGGER Delete_Logs Delete ON Logs" +
		" BEGIN" +
		" delete from logmemo where lparent = old.lparent and llogid = old.llogid ;" +
		" delete from LogImages where iCode = old.lparent and ilogid = old.llogid ;" +
		" END;",
		"CREATE TRIGGER Delete_Waypoints Delete ON Waypoints" +
		" BEGIN" +
		" delete from waymemo where cparent = old.cparent and ccode = old.ccode;" +
		" END;"
	};

        protected override void ExportMethod()
        {
            using (Utils.ProgressBlock fixscr = new Utils.ProgressBlock(this, STR_EXPORTINGGPX, STR_CREATINGFILE, 1, 0))
            {
                System.Collections.Hashtable logTypes = new System.Collections.Hashtable();
                logTypes.Add(2,"Found it");
                logTypes.Add(3,"Didn't find it");
                logTypes.Add(4,"Write note");
                logTypes.Add(5,"Archive");
                logTypes.Add(7,"Needs Archived");
                logTypes.Add(9,"Will Attend");
                logTypes.Add(10,"Attended");
                logTypes.Add(11,"Webcam Photo Taken");
                logTypes.Add(12,"Unarchive");
                logTypes.Add(22,"Temporarily Disable Listing");
                logTypes.Add(23,"Enable Listing");
                logTypes.Add(24,"Publish Listing");
                logTypes.Add(25,"Retract Listing");
                logTypes.Add(45,"Needs Maintenance");
                logTypes.Add(46,"Owner Maintenance");
                logTypes.Add(47,"Update Coordinates");
                logTypes.Add(68,"Post Reviewer Note");
                logTypes.Add(74, "Announcement");

                if (System.IO.File.Exists(_filename))
                {
                    System.IO.File.Delete(_filename);
                }
                SqliteConnection dbconFiles = null;
                string basePath = null;
                int imgFolderIndex = 0;
                int imgInFolderCount = 0;
                if (PluginSettings.Instance.ExportGrabbedImages)
                {
                    basePath = System.IO.Path.GetDirectoryName(_filename);
                    basePath = System.IO.Path.Combine(basePath, "GrabbedImages");
                    if (!System.IO.Directory.Exists(basePath))
                    {
                        System.IO.Directory.CreateDirectory(basePath);
                    }
                    if (PluginSettings.Instance.MaxFilesInFolder > 0)
                    {
                        string imgSubFolder = System.IO.Path.Combine(basePath, string.Format("batch{0}", imgFolderIndex));
                        while (System.IO.Directory.Exists(imgSubFolder))
                        {
                            imgFolderIndex++;
                            imgSubFolder = System.IO.Path.Combine(basePath, string.Format("batch{0}", imgFolderIndex));
                        }
                    }
                    dbconFiles = new SqliteConnection(string.Format("data source=file:{0}", System.IO.Path.Combine(basePath,"files.db3")));
                    dbconFiles.Open();
                    using (SqliteCommand cmd = new SqliteCommand("", dbconFiles))
                    {
                        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='fdone'";
                        object o = cmd.ExecuteScalar();
                        if (o == null || o.GetType() == typeof(DBNull))
                        {
                            cmd.CommandText = "CREATE TABLE fdone (dlink text)";
                            cmd.ExecuteNonQuery();
                            cmd.CommandText = "CREATE INDEX ifdone on fdone (dlink)";
                            cmd.ExecuteNonQuery();
                        }

                        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='files'";
                        o = cmd.ExecuteScalar();
                        if (o == null || o.GetType() == typeof(DBNull))
                        {
                            cmd.CommandText = "CREATE TABLE files (Link text collate nocase, Fname text collate nocase, Found integer)";
                            cmd.ExecuteNonQuery();
                            cmd.CommandText = "CREATE INDEX ilink on files (Link)";
                            cmd.ExecuteNonQuery();
                            cmd.CommandText = "CREATE INDEX ifname on files (Fname)";
                            cmd.ExecuteNonQuery();
                        }

                        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='purge'";
                        o = cmd.ExecuteScalar();
                        if (o == null || o.GetType() == typeof(DBNull))
                        {
                            cmd.CommandText = "CREATE TABLE purge (pfile text)";
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                using (SqliteConnection dbcon = new SqliteConnection(string.Format("data source=file:{0}", _filename)))
                {
                    dbcon.Open();
                    using (SqliteCommand cmd = new SqliteCommand("",dbcon))
                    {
                        foreach (string s in SQLCREATEDBTABLES)
                        {
                            cmd.CommandText = s;
                            cmd.ExecuteNonQuery();
                        }
                        foreach (string s in SQLCREATEDBINDEXES)
                        {
                            cmd.CommandText = s;
                            cmd.ExecuteNonQuery();
                        }
                        foreach (string s in SQLCREATEDBTRIGGERS)
                        {
                            cmd.CommandText = s;
                            cmd.ExecuteNonQuery();
                        }
                        cmd.CommandText = "PRAGMA user_version = 5003";
                        cmd.ExecuteNonQuery();
                    }

                    DbParameter par;
                    using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_SAVING, STR_SAVINGGEOCACHES, _gcList.Count, 0))
                    {
                        using (SqliteCommand cmd = new SqliteCommand("", dbcon))
                        using (SqliteCommand cmd2 = new SqliteCommand("", dbcon))
                        using (SqliteCommand cmd3 = new SqliteCommand("", dbcon))
                        using (SqliteCommand cmd4 = new SqliteCommand("", dbcon))
                        using (SqliteCommand cmd5 = new SqliteCommand("", dbcon))
                        using (SqliteCommand cmd6 = new SqliteCommand("", dbcon))
                        using (SqliteCommand cmd7 = new SqliteCommand("", dbcon))
                        {
                            cmd.CommandText = "insert into Caches (Code, Name, PlacedBy, Archived, CacheId, CacheType, Container, Country, Difficulty, Found, HasCorrected, HasUserNote, Latitude, LongHtm, Longitude, OwnerName, PlacedDate, ShortHtm, State, Terrain, UserFlag, IsOwner, LatOriginal, LonOriginal, Status, GcNote, IsPremium, FavPoints, IsLite) values (@Code, @Name, @PlacedBy, @Archived, @CacheId, @CacheType, @Container, @Country, @Difficulty, @Found, @HasCorrected, @HasUserNote, @Latitude, @LongHtm, @Longitude, @OwnerName, @PlacedDate, @ShortHtm, @State, @Terrain, @UserFlag, @IsOwner, @LatOriginal, @LonOriginal, @Status, @GcNote, @IsPremium, @FavPoints, @IsLite)";
                            cmd2.CommandText = "insert into CacheMemo (Code, LongDescription, ShortDescription, Url, Hints, UserNote) values (@Code, @LongDescription, @ShortDescription, @Url, @Hints, @UserNote)";
                            cmd3.CommandText = "insert into Attributes (aCode, aId, aInc) values (@aCode, @aId, @aInc)";
                            cmd4.CommandText = "insert into LogMemo (lParent, lLogId, lText) values (@lParent, @lLogId, @lText)";
                            cmd5.CommandText = "insert into Logs (lParent, lLogId, lType, lBy, lDate, lLat, lLon, lEncoded, lownerid, lHasHtml, lIsowner, lTime) values (@lParent, @lLogId, @lType, @lBy, @lDate, @lLat, @lLon, @lEncoded, @lownerid, @lHasHtml, @lIsowner, @lTime)";
                            cmd6.CommandText = "insert into WayMemo (cParent, cCode, cComment, cUrl) values (@cParent, @cCode, @cComment, @cUrl)";
                            cmd7.CommandText = "insert into Waypoints (cParent, cCode, cPrefix, cName, cType, cLat, cLon, cByuser, cDate, cFlag, sB1) values (@cParent, @cCode, @cPrefix, @cName, @cType, @cLat, @cLon, @cByuser, @cDate, @cFlag, @sB1)";

                            par = cmd7.CreateParameter();
                            par.ParameterName = "@cParent";
                            par.DbType = DbType.String;
                            cmd7.Parameters.Add(par);
                            par = cmd7.CreateParameter();
                            par.ParameterName = "@cCode";
                            par.DbType = DbType.String;
                            cmd7.Parameters.Add(par);
                            par = cmd7.CreateParameter();
                            par.ParameterName = "@cPrefix";
                            par.DbType = DbType.String;
                            cmd7.Parameters.Add(par);
                            par = cmd7.CreateParameter();
                            par.ParameterName = "@cName";
                            par.DbType = DbType.String;
                            cmd7.Parameters.Add(par);
                            par = cmd7.CreateParameter();
                            par.ParameterName = "@cType";
                            par.DbType = DbType.String;
                            cmd7.Parameters.Add(par);
                            par = cmd7.CreateParameter();
                            par.ParameterName = "@cLat";
                            par.DbType = DbType.String;
                            cmd7.Parameters.Add(par);
                            par = cmd7.CreateParameter();
                            par.ParameterName = "@cLon";
                            par.DbType = DbType.String;
                            cmd7.Parameters.Add(par);
                            par = cmd7.CreateParameter();
                            par.ParameterName = "@cByuser";
                            par.DbType = DbType.Boolean;
                            cmd7.Parameters.Add(par);
                            par = cmd7.CreateParameter();
                            par.ParameterName = "@cDate";
                            par.DbType = DbType.String;
                            cmd7.Parameters.Add(par);
                            par = cmd7.CreateParameter();
                            par.ParameterName = "@cFlag";
                            par.DbType = DbType.Boolean;
                            cmd7.Parameters.Add(par);
                            par = cmd7.CreateParameter();
                            par.ParameterName = "@sB1";
                            par.DbType = DbType.Boolean;
                            cmd7.Parameters.Add(par);

                            par = cmd6.CreateParameter();
                            par.ParameterName = "@cParent";
                            par.DbType = DbType.String;
                            cmd6.Parameters.Add(par);
                            par = cmd6.CreateParameter();
                            par.ParameterName = "@cCode";
                            par.DbType = DbType.String;
                            cmd6.Parameters.Add(par);
                            par = cmd6.CreateParameter();
                            par.ParameterName = "@cComment";
                            par.DbType = DbType.String;
                            cmd6.Parameters.Add(par);
                            par = cmd6.CreateParameter();
                            par.ParameterName = "@cUrl";
                            par.DbType = DbType.String;
                            cmd6.Parameters.Add(par);

                            par = cmd5.CreateParameter();
                            par.ParameterName = "@lParent";
                            par.DbType = DbType.String;
                            cmd5.Parameters.Add(par);
                            par = cmd5.CreateParameter();
                            par.ParameterName = "@lLogId";
                            par.DbType = DbType.Int32;
                            cmd5.Parameters.Add(par);
                            par = cmd5.CreateParameter();
                            par.ParameterName = "@lType";
                            par.DbType = DbType.String;
                            cmd5.Parameters.Add(par);
                            par = cmd5.CreateParameter();
                            par.ParameterName = "@lBy";
                            par.DbType = DbType.String;
                            cmd5.Parameters.Add(par);
                            par = cmd5.CreateParameter();
                            par.ParameterName = "@lDate";
                            par.DbType = DbType.String;
                            cmd5.Parameters.Add(par);
                            par = cmd5.CreateParameter();
                            par.ParameterName = "@lLat";
                            par.DbType = DbType.String;
                            cmd5.Parameters.Add(par);
                            par = cmd5.CreateParameter();
                            par.ParameterName = "@lLon";
                            par.DbType = DbType.String;
                            cmd5.Parameters.Add(par);
                            par = cmd5.CreateParameter();
                            par.ParameterName = "@lEncoded";
                            par.DbType = DbType.Boolean;
                            cmd5.Parameters.Add(par);
                            par = cmd5.CreateParameter();
                            par.ParameterName = "@lownerid";
                            par.DbType = DbType.Int32;
                            cmd5.Parameters.Add(par);
                            par = cmd5.CreateParameter();
                            par.ParameterName = "@lHasHtml";
                            par.DbType = DbType.Boolean;
                            cmd5.Parameters.Add(par);
                            par = cmd5.CreateParameter();
                            par.ParameterName = "@lIsowner";
                            par.DbType = DbType.Boolean;
                            cmd5.Parameters.Add(par);
                            par = cmd5.CreateParameter();
                            par.ParameterName = "@lTime";
                            par.DbType = DbType.String;
                            cmd5.Parameters.Add(par);

                            par = cmd4.CreateParameter();
                            par.ParameterName = "@lParent";
                            par.DbType = DbType.String;
                            cmd4.Parameters.Add(par);
                            par = cmd4.CreateParameter();
                            par.ParameterName = "@lLogId";
                            par.DbType = DbType.Int32;
                            cmd4.Parameters.Add(par);
                            par = cmd4.CreateParameter();
                            par.ParameterName = "@lText";
                            par.DbType = DbType.String;
                            cmd4.Parameters.Add(par);

                            par = cmd3.CreateParameter();
                            par.ParameterName = "@aCode";
                            par.DbType = DbType.String;
                            cmd3.Parameters.Add(par);
                            par = cmd3.CreateParameter();
                            par.ParameterName = "@aId";
                            par.DbType = DbType.Int32;
                            cmd3.Parameters.Add(par);
                            par = cmd3.CreateParameter();
                            par.ParameterName = "@aInc";
                            par.DbType = DbType.Int32;
                            cmd3.Parameters.Add(par);

                            par = cmd2.CreateParameter();
                            par.ParameterName = "@Code";
                            par.DbType = DbType.String;
                            cmd2.Parameters.Add(par);
                            par = cmd2.CreateParameter();
                            par.ParameterName = "@LongDescription";
                            par.DbType = DbType.String;
                            cmd2.Parameters.Add(par);
                            par = cmd2.CreateParameter();
                            par.ParameterName = "@ShortDescription";
                            par.DbType = DbType.String;
                            cmd2.Parameters.Add(par);
                            par = cmd2.CreateParameter();
                            par.ParameterName = "@Url";
                            par.DbType = DbType.String;
                            cmd2.Parameters.Add(par);
                            par = cmd2.CreateParameter();
                            par.ParameterName = "@Hints";
                            par.DbType = DbType.String;
                            cmd2.Parameters.Add(par);
                            par = cmd2.CreateParameter();
                            par.ParameterName = "@UserNote";
                            par.DbType = DbType.String;
                            cmd2.Parameters.Add(par);

                            par = cmd.CreateParameter();
                            par.ParameterName = "@Code";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@Name";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@PlacedBy";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@Archived";
                            par.DbType = DbType.Int32;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@CacheId";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@CacheType";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@Container";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@Country";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@Difficulty";
                            par.DbType = DbType.Double;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@Found";
                            par.DbType = DbType.Int32;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@HasCorrected";
                            par.DbType = DbType.Int32;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@HasUserNote";
                            par.DbType = DbType.Int32;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@Latitude";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@LongHtm";
                            par.DbType = DbType.Int32;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@Longitude";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@OwnerName";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@PlacedDate";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@ShortHtm";
                            par.DbType = DbType.Int32;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@State";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@Terrain";
                            par.DbType = DbType.Double;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@UserFlag";
                            par.DbType = DbType.Int32;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@IsOwner";
                            par.DbType = DbType.Int32;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@LatOriginal";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@LonOriginal";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@Status";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@GcNote";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@IsPremium";
                            par.DbType = DbType.Int32;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@FavPoints";
                            par.DbType = DbType.Int32;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@IsLite";
                            par.DbType = DbType.Int32;
                            cmd.Parameters.Add(par);

                            cmd.Prepare();
                            cmd2.Prepare();
                            cmd3.Prepare();
                            cmd4.Prepare();
                            cmd5.Prepare();
                            cmd6.Prepare();
                            cmd7.Prepare();

                            using (DbTransaction trans = dbcon.BeginTransaction())
                            {
                                int index = 0;
                                int procStep = 0;
                                foreach (Framework.Data.Geocache gc in _gcList)
                                {
                                    cmd2.Parameters["@Code"].Value = gc.Code;
                                    cmd2.Parameters["@LongDescription"].Value = gc.LongDescription ?? "";
                                    cmd2.Parameters["@ShortDescription"].Value = gc.ShortDescription ?? "";
                                    cmd2.Parameters["@Url"].Value = gc.Url ?? "";
                                    cmd2.Parameters["@Hints"].Value = gc.EncodedHints ?? "";
                                    cmd2.Parameters["@UserNote"].Value = string.IsNullOrEmpty(gc.Notes) ? "" : System.Web.HttpUtility.HtmlDecode(gc.Notes);

                                    cmd.Parameters["@Code"].Value = gc.Code;
                                    cmd.Parameters["@Name"].Value = gc.Name??"";
                                    cmd.Parameters["@PlacedBy"].Value = gc.PlacedBy ?? "";
                                    cmd.Parameters["@Archived"].Value = gc.Archived ? 1:0;
                                    cmd.Parameters["@CacheId"].Value = gc.ID ?? "1";
                                    cmd.Parameters["@CacheType"].Value = getCacheType(gc.GeocacheType);
                                    cmd.Parameters["@Container"].Value = getContainer(gc.Container);
                                    cmd.Parameters["@Country"].Value = gc.Country ?? "";
                                    cmd.Parameters["@Difficulty"].Value = gc.Difficulty;
                                    cmd.Parameters["@Found"].Value = gc.Found ? 1 : 0;
                                    cmd.Parameters["@HasCorrected"].Value = (gc.CustomCoords || gc.ContainsCustomLatLon) ? 1 : 0;
                                    cmd.Parameters["@HasUserNote"].Value = gc.ContainsNote ? 1 : 0;
                                    cmd.Parameters["@LatOriginal"].Value = gc.Lat.ToString().Replace(',', '.');
                                    cmd.Parameters["@LonOriginal"].Value = gc.Lon.ToString().Replace(',', '.');
                                    if (gc.ContainsCustomLatLon)
                                    {
                                        cmd.Parameters["@Latitude"].Value = gc.CustomLat.ToString().Replace(',', '.');
                                        cmd.Parameters["@Longitude"].Value = gc.CustomLon.ToString().Replace(',', '.');
                                    }
                                    else
                                    {
                                        cmd.Parameters["@Latitude"].Value = gc.Lat.ToString().Replace(',', '.');
                                        cmd.Parameters["@Longitude"].Value = gc.Lon.ToString().Replace(',', '.');
                                    }
                                    cmd.Parameters["@LongHtm"].Value = gc.LongDescriptionInHtml ? 1 : 0;
                                    cmd.Parameters["@OwnerName"].Value = gc.Owner ?? "";
                                    cmd.Parameters["@PlacedDate"].Value = gc.PublishedTime.ToString("yyyy-MM-dd");
                                    cmd.Parameters["@ShortHtm"].Value = gc.ShortDescriptionInHtml ? 1 : 0;
                                    cmd.Parameters["@State"].Value = gc.State ?? "";
                                    cmd.Parameters["@Terrain"].Value = gc.Terrain;
                                    cmd.Parameters["@UserFlag"].Value = gc.Flagged ? 1 : 0;
                                    cmd.Parameters["@IsOwner"].Value = gc.IsOwn ? 1 : 0;
                                    cmd.Parameters["@Status"].Value = gc.Available ? "A" : gc.Archived ? "X" : "T";
                                    cmd.Parameters["@GcNote"].Value = gc.PersonaleNote ?? "";
                                    cmd.Parameters["@IsPremium"].Value = gc.MemberOnly ? 1 : 0;
                                    cmd.Parameters["@FavPoints"].Value = gc.Favorites;
                                    cmd.Parameters["@IsLite"].Value = 0;

                                    cmd.ExecuteNonQuery();
                                    cmd2.ExecuteNonQuery();

                                    List<int> attr = gc.AttributeIds;
                                    foreach (int att in attr)
                                    {
                                        cmd3.Parameters["@aCode"].Value = gc.Code;
                                        cmd3.Parameters["@aId"].Value = Math.Abs(att);
                                        cmd3.Parameters["@aInc"].Value = att < 0 ? 0 : 1;

                                        cmd3.ExecuteNonQuery();
                                    }

                                    List<Framework.Data.Log> logs = Utils.DataAccess.GetLogs(Core.Logs, gc.Code).Take(PluginSettings.Instance.MaxLogs).ToList();
                                    foreach (Framework.Data.Log l in logs)
                                    {
                                        try
                                        {
                                            int logid =  0;
                                            if (!int.TryParse(l.ID, out logid))
                                            {
                                                logid = Utils.Conversion.GetCacheIDFromCacheCode(l.ID);
                                            }
                                            cmd4.Parameters["@lLogId"].Value = logid; 
                                            cmd4.Parameters["@lText"].Value = l.Text ?? "";
                                            cmd4.Parameters["@lParent"].Value = gc.Code;
                                            cmd4.ExecuteNonQuery();

                                            cmd5.Parameters["@lLogId"].Value = logid;
                                            cmd5.Parameters["@lParent"].Value = gc.Code;
                                            object o = logTypes[l.LogType.ID];
                                            if (o == null)
                                            {
                                                cmd5.Parameters["@lType"].Value = 4;
                                            }
                                            else
                                            {
                                                cmd5.Parameters["@lType"].Value = (string)o;
                                            }
                                            cmd5.Parameters["@lBy"].Value = l.Finder??"";
                                            cmd5.Parameters["@lDate"].Value = l.Date.ToString("yyyy-MM-dd HH:mm:ss");
                                            cmd5.Parameters["@lLat"].Value = DBNull.Value;
                                            cmd5.Parameters["@lLon"].Value = DBNull.Value;
                                            cmd5.Parameters["@lEncoded"].Value =l.Encoded;
                                            try
                                            {
                                                cmd5.Parameters["@lownerid"].Value = int.Parse(l.FinderId);
                                            }
                                            catch
                                            {
                                            }
                                            cmd5.Parameters["@lHasHtml"].Value = false;
                                            cmd5.Parameters["@lIsowner"].Value = (l.Finder==Core.GeocachingAccountNames.GetAccountName(gc.Code));
                                            cmd5.Parameters["@lTime"].Value = "";

                                            cmd5.ExecuteNonQuery();

                                        }
                                        catch
                                        {
                                        }
                                    }

                                    List<Framework.Data.Waypoint> wps = Utils.DataAccess.GetWaypointsFromGeocache(Core.Waypoints, gc.Code);
                                    foreach (Framework.Data.Waypoint w in wps)
                                    {
                                        try
                                        {
                                            cmd6.Parameters["@cParent"].Value = gc.Code;
                                            cmd6.Parameters["@cCode"].Value = w.Code;
                                            cmd6.Parameters["@cComment"].Value = w.Comment;
                                            cmd6.Parameters["@cUrl"].Value = w.Url;

                                            cmd7.Parameters["@cParent"].Value = gc.Code;
                                            cmd7.Parameters["@cCode"].Value = w.Code;
                                            cmd7.Parameters["@cPrefix"].Value = w.Code.Substring(0,2);
                                            cmd7.Parameters["@cName"].Value = w.Name??"";
                                            cmd7.Parameters["@cType"].Value = getWPType(w.WPType);
                                            cmd7.Parameters["@cLat"].Value = w.Lat == null ? "0.0" : w.Lat.ToString().Replace(',', '.');
                                            cmd7.Parameters["@cLon"].Value = w.Lon == null ? "0.0" : w.Lon.ToString().Replace(',', '.');
                                            cmd7.Parameters["@cByuser"].Value = false;
                                            cmd7.Parameters["@cDate"].Value = w.Time.ToString("yyyy-MM-dd");
                                            cmd7.Parameters["@cFlag"].Value = false;
                                            cmd7.Parameters["@sB1"].Value = false;

                                            cmd7.ExecuteNonQuery();
                                            cmd6.ExecuteNonQuery();
                                        }
                                        catch
                                        {
                                        }
                                    }

                                    if (dbconFiles != null && (gc.LongDescriptionInHtml || gc.ShortDescriptionInHtml))
                                    {
                                        try
                                        {
                                            List<string> linksInDescr = Utils.ImageSupport.GetImageUrlsFromGeocache(gc);
                                            foreach (string link in linksInDescr)
                                            {
                                                string p = Utils.ImageSupport.Instance.GetImagePath(link);
                                                if (!string.IsNullOrEmpty(p) && IsLocalFile(p))
                                                {
                                                    using (SqliteCommand filescmd = new SqliteCommand("", dbconFiles))
                                                    {
                                                        filescmd.CommandText = string.Format("SELECT Fname FROM files WHERE Link='{0}'", link.Replace("'", "''"));
                                                        object o = filescmd.ExecuteScalar();
                                                        if (o == null || o.GetType() == typeof(DBNull))
                                                        {
                                                            filescmd.CommandText = string.Format("insert into files (Link, Fname, Found) values ('{0}', '{1}', 1)", link.Replace("'", "''"), System.IO.Path.GetFileName(p).Replace("'", "''"));
                                                            filescmd.ExecuteNonQuery();
                                                        }
                                                    }
                                                    if (PluginSettings.Instance.MaxFilesInFolder > 0)
                                                    {
                                                        imgInFolderCount++;
                                                        if (imgInFolderCount > PluginSettings.Instance.MaxFilesInFolder)
                                                        {
                                                            imgFolderIndex++;
                                                            imgInFolderCount = 1;
                                                        }
                                                        string imgSubFolder = System.IO.Path.Combine(basePath, string.Format("batch{0}", imgFolderIndex));
                                                        if (imgInFolderCount == 1)
                                                        {
                                                            if (!System.IO.Directory.Exists(imgSubFolder))
                                                            {
                                                                System.IO.Directory.CreateDirectory(imgSubFolder);
                                                            }
                                                        }
                                                        string dst = System.IO.Path.Combine(imgSubFolder, System.IO.Path.GetFileName(p));
                                                        if (!System.IO.File.Exists(dst))
                                                        {
                                                            System.IO.File.Copy(p, dst, true);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        string dst = System.IO.Path.Combine(basePath, System.IO.Path.GetFileName(p));
                                                        if (!System.IO.File.Exists(dst))
                                                        {
                                                            System.IO.File.Copy(p, dst, true);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        catch
                                        {
                                        }

                                    }

                                    index++;
                                    procStep++;
                                    if (procStep >= 200)
                                    {
                                        progress.UpdateProgress(STR_SAVING, STR_SAVINGGEOCACHES, _gcList.Count, index);
                                        procStep = 0;
                                    }
                                }
                                trans.Commit();
                            }
                        }
                    }
                }
                if (dbconFiles != null)
                {
                    dbconFiles.Dispose();
                    dbconFiles = null;
                }
            }
        }

        private bool IsLocalFile(string p)
        {
            bool result;
            try
            {
                result = new Uri(p).IsFile;
                if (result)
                {
                    result = !p.StartsWith("/") && !p.StartsWith("\\")&&  System.IO.File.Exists(p);
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }

        private string getWPType(Framework.Data.WaypointType type)
        {
            switch (type.ID)
            {
                case 217: return "Parking Area";
                case 220: return "Final location";
                case 218: return "Question to Answer";
                case 452: return "Reference Point";
                case 219: return "Stages of a Multicache";
                case 221: return "Trailhead";
                default: return "Stages of a Multicache";
            }
        }

        private string getContainer(Framework.Data.GeocacheContainer container)
        {
            switch (container.ID)
            {
                case 1: return "Not chosen";
                case 2: return "Micro";
                case 3: return "Regular";
                case 4: return "Large";
                case 5: return "Virtual";
                case 6: return "Other";
                case 8: return "Small";
                case 9: return "A";
                default: return "Other";
            }
        }

        private string getCacheType(Framework.Data.GeocacheType type)
        {
            switch (type.ID)
            {
                case 2: return "T";
                case 3: return "M";
                case 4: return "V";
                case 5: return "B";
                case 6: return "E";
                case 8: return "U";
                case 9: return "A";
                case 11: return "W";
                case 12: return "L";
                case 13: return "C";
                case 137: return "R";
                case 453: return "Z";
                case 1304: return "X";
                case 1858: return "I";
                case 3653: return "F";
                case 3773: return "H";
                case 3774: return "D";
                default: return "O";
            }
        }

        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            foreach (System.Windows.Forms.UserControl uc in configPanels)
            {
                if (uc is SettingsPanel)
                {
                    PluginSettings.Instance.MaxLogs = (int)(uc as SettingsPanel).numericUpDown1.Value;
                    PluginSettings.Instance.ExportGrabbedImages = (uc as SettingsPanel).checkBox1.Checked;
                    PluginSettings.Instance.MaxFilesInFolder = (int)(uc as SettingsPanel).numericUpDown2.Value;
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

    }
}
