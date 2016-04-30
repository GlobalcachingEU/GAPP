using Community.CsharpSqlite.SQLiteClient;
using GAPPSF.Core;
using GAPPSF.Core.Data;
using GAPPSF.Core.Storage;
using GAPPSF.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.GSAK
{
    public class Importer
    {
        async public static Task<bool> PerformAction(Database database, string filename)
        {
            bool result = false;
            using (DataUpdater upd = new DataUpdater(database))
            {
                await Task.Run(() => { result = Import(database, filename); });
            }
            return result;
        }

        private static bool Import(Database database, string filename)
        {
            try
            {
                using (Utils.ProgressBlock allprog = new ProgressBlock("ImportGSAKDatabase", "Importing", 6, 0))
                {
                    System.Collections.Hashtable logTypes = new System.Collections.Hashtable();
                    String connect = String.Format("data source=file:{0}", filename);
                    using (SqliteConnection dbcon = new SqliteConnection(connect))
                    {

                        //System.Diagnostics.Debugger.Break();
                        logTypes.Add("Found it", 2);
                        logTypes.Add("Didn't find it", 3);
                        logTypes.Add("Write note", 4);
                        logTypes.Add("Archive", 5);
                        logTypes.Add("Needs Archived", 7);
                        logTypes.Add("Will Attend", 9);
                        logTypes.Add("Attended", 10);
                        logTypes.Add("Webcam Photo Taken", 11);
                        logTypes.Add("Unarchive", 12);
                        logTypes.Add("Temporarily Disable Listing", 22);
                        logTypes.Add("Temporarily Disable", 22);
                        logTypes.Add("Enable Listing", 23);
                        logTypes.Add("Enable", 23);
                        logTypes.Add("Publish Listing", 24);
                        logTypes.Add("Publish", 24);
                        logTypes.Add("Retract Listing", 25);
                        logTypes.Add("Retract", 25);
                        logTypes.Add("Needs Maintenance", 45);
                        logTypes.Add("Owner Maintenance", 46);
                        logTypes.Add("Update Coordinates", 47);
                        logTypes.Add("Post Reviewer Note", 68);
                        logTypes.Add("Announcement", 74);

                        dbcon.Open();

                        SqliteCommand lookup = new SqliteCommand("select aId, aInc from attributes where aCode = @Code", dbcon);
                        lookup.CommandType = CommandType.Text;
                        DbParameter par = lookup.CreateParameter();
                        par.Direction = ParameterDirection.Input;
                        par.ParameterName = "@Code";
                        lookup.Parameters.Add(par);
                        lookup.Prepare();

                        SqliteCommand import = new SqliteCommand("select count(1) from caches", dbcon);
                        import.CommandType = CommandType.Text;

                        int index = 0;
                        int gcCount = (int)(long)import.ExecuteScalar();
                        if (gcCount > 0)
                        {
                            DateTime progShow = DateTime.Now.AddSeconds(1);
                            using (Utils.ProgressBlock prog = new ProgressBlock("ImportingGeocaches", 1, 0))
                            {
                                bool isPremiumAvailable = false;
                                bool isFavPointAvailable = false;
                                bool isGCNoteAvailable = false;

                                try
                                {
                                    import.CommandText = "select IsPremium from Caches limit 1";
                                    using (SqliteDataReader checkdr = import.ExecuteReader())
                                    {
                                        isPremiumAvailable = true;
                                    }
                                }
                                catch
                                {
                                }

                                try
                                {
                                    import.CommandText = "select FavPoints from Caches limit 1";
                                    using (SqliteDataReader checkdr = import.ExecuteReader())
                                    {
                                        isFavPointAvailable = true;
                                    }
                                }
                                catch
                                {
                                }

                                try
                                {
                                    import.CommandText = "select gcnote from Caches limit 1";
                                    using (SqliteDataReader checkdr = import.ExecuteReader())
                                    {
                                        isGCNoteAvailable = true;
                                    }
                                }
                                catch
                                {
                                }

                                import.CommandText = "select caches.Code, Name, LastGPXDate, PlacedDate, Latitude, Longitude, Status, " +
                                    "Archived, Country, State, County, CacheType, PlacedBy, OwnerName, OwnerId, Container, Terrain, Difficulty, ShortHTM" +
                                    ", LongHTM, " +
                                    string.Format("{0}", isPremiumAvailable ? "IsPremium, " : "") +
                                    " HasCorrected, LatOriginal, LonOriginal, UserFlag, Found, " +
                                    string.Format("{0}", isFavPointAvailable ? "FavPoints, " : "") +
                                    " ShortDescription, LongDescription, Hints, Url, UserNote" +
                                    string.Format("{0}", isGCNoteAvailable ? ", gcnote" : "") +
                                    " from caches" +
                                    " inner join cachememo on cachememo.code = caches.code";

                                SqliteDataReader dr = import.ExecuteReader();

                                while (dr.Read())
                                {
                                    GeocacheData gc = new GeocacheData();
                                    int cacheType;
                                    try
                                    {
                                        cacheType = getCacheType(((String)dr["CacheType"])[0]);
                                    }
                                    catch (ArgumentOutOfRangeException)
                                    {
                                        continue;
                                    }
                                    int container = getContainer(((String)dr["Container"])[0]);

                                    gc.Code = (string)dr["code"];
                                    gc.Name = (string)dr["name"];
                                    if (string.IsNullOrEmpty((string)dr["LastGPXDate"]))
                                    {
                                        gc.DataFromDate = DateTime.Now.Date;
                                    }
                                    else
                                    {
                                        gc.DataFromDate = DateTime.Parse((string)dr["LastGPXDate"]);
                                    }
                                    gc.Available = ((String)dr["Status"]).Equals("A");
                                    gc.Archived = (int)dr["archived"] != 0;
                                    gc.Country = (string)dr["country"];
                                    gc.State = (string)dr["state"];
                                    gc.Municipality = (string)dr["county"];

                                    gc.GeocacheType = Utils.DataAccess.GetGeocacheType(cacheType);
                                    gc.PlacedBy = (string)dr["placedby"];
                                    gc.Owner = (string)dr["OwnerName"];
                                    gc.OwnerId = dr["ownerid"].GetType() == typeof(DBNull) ? "" : dr["ownerid"].ToString();
                                    gc.Container = Utils.DataAccess.GetGeocacheContainer(container);
                                    gc.Terrain = (double)dr["terrain"];
                                    gc.Difficulty = (double)dr["difficulty"];
                                    gc.ShortDescription = (string)dr["ShortDescription"];
                                    gc.ShortDescriptionInHtml = (int)dr["ShortHTM"] != 0;
                                    gc.LongDescription = (string)dr["LongDescription"];
                                    gc.LongDescriptionInHtml = (int)dr["LongHTM"] != 0;
                                    gc.EncodedHints = (string)dr["Hints"];
                                    gc.Url = (string)dr["url"];
                                    if (isPremiumAvailable)
                                    {
                                        gc.MemberOnly = (int)dr["IsPremium"] != 0;
                                    }
                                    else
                                    {
                                        gc.MemberOnly = false;
                                    }
                                    bool customCoords = (int)dr["HasCorrected"] != 0;
                                    if (customCoords)
                                    {
                                        gc.CustomLat = Utils.Conversion.StringToDouble(dr["Latitude"] as String);
                                        gc.CustomLon = Utils.Conversion.StringToDouble(dr["Longitude"] as String);
                                        gc.Lat = Utils.Conversion.StringToDouble(dr["LatOriginal"] as string);
                                        gc.Lon = Utils.Conversion.StringToDouble(dr["LonOriginal"] as string);
                                    }
                                    else
                                    {
                                        gc.Lat = Utils.Conversion.StringToDouble(dr["Latitude"] as string);
                                        gc.Lon = Utils.Conversion.StringToDouble(dr["Longitude"] as string);
                                    }

                                    par.Value = gc.Code;
                                    DbDataReader attrs = lookup.ExecuteReader();
                                    List<int> attrList = new List<int>();
                                    while (attrs.Read())
                                    {
                                        int attr = (int)(int)attrs["aId"];
                                        if (attrs["aInc"].ToString() == "0")
                                        {
                                            attr *= -1;
                                        }
                                        attrList.Add(attr);
                                    }
                                    attrs.Close();
                                    gc.AttributeIds = attrList;

                                    gc.Notes = (string)dr["UserNote"];
                                    gc.PublishedTime = DateTime.Parse((string)dr["PlacedDate"]);
                                    if (isGCNoteAvailable)
                                    {
                                        gc.PersonalNote = (string)dr["gcnote"];
                                    }
                                    else
                                    {
                                        gc.PersonalNote = "";
                                    }
                                    gc.Flagged = (int)dr["UserFlag"] != 0;
                                    gc.Found = (int)dr["Found"] != 0;

                                    if (isFavPointAvailable)
                                    {
                                        gc.Favorites = (int)(int)dr["FavPoints"];
                                    }
                                    else
                                    {
                                        gc.Favorites = 0;
                                    }

                                    DataAccess.AddGeocache(database, gc);

                                    index++;
                                    if (DateTime.Now >= progShow)
                                    {
                                        prog.Update("ImportingGeocaches", gcCount, index);
                                        progShow = DateTime.Now.AddSeconds(1);
                                    }
                                }
                                dr.Close();

                            }
                        }
                        allprog.Update("Importing", 5, 1);

                        import.CommandText = "select count(1) from logs";
                        int logCount = (int)(long)import.ExecuteScalar();
                        if (logCount > 0)
                        {
                            DateTime progShow = DateTime.Now.AddSeconds(1);
                            using (Utils.ProgressBlock progress = new Utils.ProgressBlock("ImportingLogs", logCount, 0))
                            {
                                index = 0;
                                import.CommandText = "select l.lLogId, l.lParent, lDate, lTime, lBy, lownerid, lEncoded, lType, lText " +
                                    " from logs l" +
                                    " inner join logmemo m on m.lLogId = l.lLogId and m.lParent = l.lParent";
                                DbDataReader dr = import.ExecuteReader();
                                while (dr.Read())
                                {
                                    Core.Data.LogData lg = new Core.Data.LogData();

                                    String type = (String)dr["lType"];
                                    int logType = (int)logTypes[type];

                                    //id text, gccode text, tbcode text, date text, finder text, finderid text, logtext text, encoded integer, datafromdate text, logtype integer
                                    lg.ID = dr["lLogiD"].ToString();
                                    lg.GeocacheCode = (string)dr["lParent"];
                                    lg.TBCode = "";
                                    lg.Date = (DateTime)dr["lDate"];
                                    lg.Finder = (string)dr["lBy"];
                                    lg.FinderId = dr["lownerid"].ToString();
                                    lg.Text = (string)dr["lText"];
                                    lg.Encoded = (long)dr["lEncoded"] != 0;
                                    lg.DataFromDate = DateTime.Now;
                                    lg.LogType = Utils.DataAccess.GetLogType(logType);

                                    DataAccess.AddLog(database, lg);

                                    index++;
                                    if (DateTime.Now >= progShow)
                                    {
                                        progress.Update("ImportingLogs", logCount, index);
                                        progShow = DateTime.Now.AddSeconds(1);
                                    }
                                }
                                dr.Close();
                            }
                        }
                        allprog.Update("Importing", 5, 2);

                        import.CommandText = "select count(1) from logimages";
                        int logimgCount = 0;
                        try
                        {
                            logimgCount = (int)(long)import.ExecuteScalar();
                        }
                        catch
                        {
                            //table does not exists
                        }
                        if (logimgCount > 0)
                        {
                            DateTime progShow = DateTime.Now.AddSeconds(1);
                            using (Utils.ProgressBlock progress = new Utils.ProgressBlock("ImportingLogImages", logimgCount, 0))
                            {
                                index = 0;
                                import.CommandText = "select iCode, iLogId, iImage, iName from logimages";
                                DbDataReader dr = import.ExecuteReader();
                                while (dr.Read())
                                {
                                    Core.Data.LogImageData lg = new Core.Data.LogImageData();

                                    lg.ID = (string)dr["iCode"];
                                    lg.LogId = dr["iLogID"].ToString();
                                    lg.Url = (string)dr["iImage"];
                                    lg.Name = (string)dr["iName"];

                                    DataAccess.AddLogImage(database, lg);

                                    index++;
                                    if (DateTime.Now >= progShow)
                                    {
                                        progress.Update("ImportingLogImages", logimgCount, index);
                                        progShow = DateTime.Now.AddSeconds(1);
                                    }
                                }
                                dr.Close();
                            }
                        }
                        allprog.Update("Importing", 5, 3);

                        //id text, code text, geocachecode text, name text, datafromdate text, comment text, description text, url text, urlname text, wptype integer, lat real, lon real, time text
                        import.CommandText = "select count(1) from waypoints";

                        int wptCount = (int)(long)import.ExecuteScalar();
                        if (wptCount > 0)
                        {
                            DateTime progShow = DateTime.Now.AddSeconds(1);
                            using (Utils.ProgressBlock progress = new Utils.ProgressBlock("ImportingWaypoints", wptCount, 0))
                            {
                                index = 0;
                                import.CommandText = "select w.cParent, w.cCode, cName, cDate, cType, cLat, cLon," +
                                    " cComment, cUrl" +
                                    " from waypoints w" +
                                    " inner join wayMemo m on w.cParent = m.cParent and w.cCode=m.cCode";
                                DbDataReader dr = import.ExecuteReader();
                                while (dr.Read())
                                {
                                    Core.Data.WaypointData wp = new Core.Data.WaypointData();

                                    int wpType = getWPType(((string)dr["cType"])[0]);

                                    wp.ID = (string)dr["cCode"];
                                    wp.Code = (string)dr["cCode"];
                                    wp.Url = (string)dr["cUrl"];
                                    //wp.UrlName = (string)dr["urlname"];
                                    wp.Name = (string)dr["cName"];
                                    wp.DataFromDate = (DateTime)dr["cDate"];
                                    wp.Comment = (string)dr["cComment"];
                                    wp.GeocacheCode = (string)dr["cParent"];
                                    //wp.Description = (string)dr["description"];
                                    wp.WPType = Utils.DataAccess.GetWaypointType(wpType);
                                    double lat = Utils.Conversion.StringToDouble(dr["clat"] as string);
                                    double lon = Utils.Conversion.StringToDouble(dr["clon"] as string);
                                    if (Math.Abs(lat) < 0.00001)
                                    {
                                        wp.Lat = null;
                                    }
                                    else
                                    {
                                        wp.Lat = lat;
                                    }
                                    if (Math.Abs(lon) < 0.00001)
                                    {
                                        wp.Lon = null;
                                    }
                                    else
                                    {
                                        wp.Lon = lon;
                                    }
                                    wp.Time = (DateTime)dr["cDate"];

                                    wp.Description = wp.WPType.Name;
                                    wp.UrlName = wp.WPType.Name;

                                    DataAccess.AddWaypoint(database, wp);

                                    index++;
                                    if (DateTime.Now >= progShow)
                                    {
                                        progress.Update("ImportingWaypoints", wptCount, index);
                                        progShow = DateTime.Now.AddSeconds(1);
                                    }
                                }
                                dr.Close();
                            }
                        }
                        allprog.Update("Importing", 5, 4);

                        try
                        {
                            //import corrected if table exists
                            import.CommandText = "select kCode, kAfterLat, kAfterLon from Corrected";
                            DbDataReader dr = import.ExecuteReader();
                            while (dr.Read())
                            {
                                string gcCode = dr["kCode"] as string ?? "";
                                Core.Data.Geocache gc = database.GeocacheCollection.GetGeocache(gcCode);
                                if (gc != null)
                                {
                                    object oLat = dr["kAfterLat"];
                                    object oLon = dr["kAfterLon"];
                                    if (oLat != null && oLat.GetType() != typeof(DBNull) &&
                                        oLon != null && oLon.GetType() != typeof(DBNull))
                                    {
                                        string sLat = oLat as string;
                                        string sLon = oLon as string;
                                        if (sLat.Length > 0 && sLon.Length > 0)
                                        {
                                            gc.CustomLat = Utils.Conversion.StringToDouble(sLat);
                                            gc.CustomLon = Utils.Conversion.StringToDouble(sLon);
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                        allprog.Update("Importing", 5, 5);
                    }
                }
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(new Importer(), e);
            }
            return true;
        }

        private static int getWPType(char type)
        {
            switch (type)
            {
                case 'P': return 217;
                case 'F': return 220;
                case 'Q': return 218;
                case 'R': return 452;
                case 'S': return 219;
                case 'T': return 221;
                default: return 219;
            }
        }

        private static int getContainer(char container)
        {
            switch (container)
            {
                case 'N': return 1;
                case 'M': return 2;
                case 'R': return 3;
                case 'L': return 4;
                case 'V': return 5;
                case 'O': return 6;
                case 'S': return 8;
                default: return 6;
            }
        }

        private static int getCacheType(char type)
        {
            switch (type)
            {
                case 'T': return 2;
                case 'M': return 3;
                case 'V': return 4;
                case 'B': return 5;
                case 'E': return 6;
                case 'U': return 8;
                case 'A': return 9;
                case 'W': return 11;
                case 'L': return 12;
                case 'C': return 13;
                case 'R': return 137;
                case 'Z': return 453;
                case 'X': return 1304;
                case 'I': return 1858;
                case 'F': return 3653;
                case 'H': return 3773;
                case 'D': return 3774;
                default: throw new ArgumentOutOfRangeException();
            }
        }

    }
}
