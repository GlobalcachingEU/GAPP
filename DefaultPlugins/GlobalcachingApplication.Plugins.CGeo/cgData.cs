using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;

namespace GlobalcachingApplication.Plugins.CGeo
{
    public class cgData
    {
        private Utils.DBConComSqlite _db = null;

        private static int dbVersion = 67; 

        private static string dbName = "data";
        private static string dbTableCaches = "cg_caches";
        private static string dbTableLists = "cg_lists";
        private static string dbTableAttributes = "cg_attributes";
        private static string dbTableWaypoints = "cg_waypoints";
        private static string dbTableSpoilers = "cg_spoilers";
        private static string dbTableLogs = "cg_logs";
        private static string dbTableLogCount = "cg_logCount";
        private static string dbTableLogImages = "cg_logImages";
        private static string dbTableLogsOffline = "cg_logs_offline";
        private static string dbTableTrackables = "cg_trackables";
        private static string dbTableSearchDestionationHistory = "cg_search_destination_history";
        private static string dbCreateCaches = ""
                + "create table " + dbTableCaches + " ("
                + "_id integer primary key autoincrement, "
                + "updated long not null, "
                + "detailed integer not null default 0, "
                + "detailedupdate long, "
                + "visiteddate long, "
                + "geocode text unique not null, "
                + "reason integer not null default 0, " // cached, favorite...
                + "cacheid text, "
                + "guid text, "
                + "type text, "
                + "name text, "
                + "owner text, "
                + "owner_real text, "
                + "hidden long, "
                + "hint text, "
                + "size text, "
                + "difficulty float, "
                + "terrain float, "
                + "latlon text, "
                + "location text, "
                + "direction double, "
                + "distance double, "
                + "latitude double, "
                + "longitude double, "
                + "reliable_latlon integer, "
                + "elevation double, "
                + "personal_note text, "
                + "shortdesc text, "
                + "description text, "
                + "favourite_cnt integer, "
                + "rating float, "
                + "votes integer, "
                + "myvote float, "
                + "disabled integer not null default 0, "
                + "archived integer not null default 0, "
                + "members integer not null default 0, "
                + "found integer not null default 0, "
                + "favourite integer not null default 0, "
                + "inventorycoins integer default 0, "
                + "inventorytags integer default 0, "
                + "inventoryunknown integer default 0, "
                + "onWatchlist integer default 0, "
                + "coordsChanged integer default 0, "
                + "finalDefined integer default 0"
                + "); ";

        private static string dbCreateLists = ""
                + "create table " + dbTableLists + " ("
                + "_id integer primary key autoincrement, "
                + "title text not null, "
                + "updated long not null, "
                + "latitude double, "
                + "longitude double "
                + "); ";
        private static string dbCreateAttributes = ""
                + "create table " + dbTableAttributes + " ("
                + "_id integer primary key autoincrement, "
                + "geocode text not null, "
                + "updated long not null, " // date of save
                + "attribute text "
                + "); ";

        private static string dbCreateWaypoints = ""
                + "create table " + dbTableWaypoints + " ("
                + "_id integer primary key autoincrement, "
                + "geocode text not null, "
                + "updated long not null, " // date of save
                + "type text not null default 'waypoint', "
                + "prefix text, "
                + "lookup text, "
                + "name text, "
                + "latlon text, "
                + "latitude double, "
                + "longitude double, "
                + "note text, "
                + "own integer default 0, "
                + "visited integer default 0"
                + "); ";
        private static string dbCreateSpoilers = ""
                + "create table " + dbTableSpoilers + " ("
                + "_id integer primary key autoincrement, "
                + "geocode text not null, "
                + "updated long not null, " // date of save
                + "url text, "
                + "title text, "
                + "description text "
                + "); ";
        private static string dbCreateLogs = ""
                + "create table " + dbTableLogs + " ("
                + "_id integer primary key autoincrement, "
                + "geocode text not null, "
                + "updated long not null, " // date of save
                + "type integer not null default 4, "
                + "author text, "
                + "log text, "
                + "date long, "
                + "found integer not null default 0, "
                + "friend integer "
                + "); ";

        private static string dbCreateLogCount = ""
                + "create table " + dbTableLogCount + " ("
                + "_id integer primary key autoincrement, "
                + "geocode text not null, "
                + "updated long not null, " // date of save
                + "type integer not null default 4, "
                + "count integer not null default 0 "
                + "); ";
        private static string dbCreateLogImages = ""
                + "create table " + dbTableLogImages + " ("
                + "_id integer primary key autoincrement, "
                + "log_id integer not null, "
                + "title text not null, "
                + "url text not null"
                + "); ";
        private static string dbCreateLogsOffline = ""
                + "create table " + dbTableLogsOffline + " ("
                + "_id integer primary key autoincrement, "
                + "geocode text not null, "
                + "updated long not null, " // date of save
                + "type integer not null default 4, "
                + "log text, "
                + "date long "
                + "); ";
        private static string dbCreateTrackables = ""
                + "create table " + dbTableTrackables + " ("
                + "_id integer primary key autoincrement, "
                + "updated long not null, " // date of save
                + "tbcode text not null, "
                + "guid text, "
                + "title text, "
                + "owner text, "
                + "released long, "
                + "goal text, "
                + "description text, "
                + "geocode text "
                + "); ";

        private static string dbCreateSearchDestinationHistory = ""
                + "create table " + dbTableSearchDestionationHistory + " ("
                + "_id integer primary key autoincrement, "
                + "date long not null, "
                + "latitude double, "
                + "longitude double "
                + "); ";

        public cgData(Utils.DBConComSqlite db)
        {
            _db = db;
        }

        public void Create()
        {
            execSQL(string.Format("PRAGMA user_version = {0}", dbVersion));

            execSQL(dbCreateCaches);
            execSQL(dbCreateLists);
            execSQL(dbCreateAttributes);
            execSQL(dbCreateWaypoints);
            execSQL(dbCreateSpoilers);
            execSQL(dbCreateLogs);
            execSQL(dbCreateLogCount);
            execSQL(dbCreateLogImages);
            execSQL(dbCreateLogsOffline);
            execSQL(dbCreateTrackables);
            execSQL(dbCreateSearchDestinationHistory);

            createIndices();
        }

        private void createIndices()
        {
            execSQL("create index if not exists in_caches_geo on " + dbTableCaches + " (geocode)");
            execSQL("create index if not exists in_caches_guid on " + dbTableCaches + " (guid)");
            execSQL("create index if not exists in_caches_lat on " + dbTableCaches + " (latitude)");
            execSQL("create index if not exists in_caches_lon on " + dbTableCaches + " (longitude)");
            execSQL("create index if not exists in_caches_reason on " + dbTableCaches + " (reason)");
            execSQL("create index if not exists in_caches_detailed on " + dbTableCaches + " (detailed)");
            execSQL("create index if not exists in_caches_type on " + dbTableCaches + " (type)");
            execSQL("create index if not exists in_caches_visit_detail on " + dbTableCaches + " (visiteddate, detailedupdate)");
            execSQL("create index if not exists in_attr_geo on " + dbTableAttributes + " (geocode)");
            execSQL("create index if not exists in_wpts_geo on " + dbTableWaypoints + " (geocode)");
            execSQL("create index if not exists in_wpts_geo_type on " + dbTableWaypoints + " (geocode, type)");
            execSQL("create index if not exists in_spoil_geo on " + dbTableSpoilers + " (geocode)");
            execSQL("create index if not exists in_logs_geo on " + dbTableLogs + " (geocode)");
            execSQL("create index if not exists in_logcount_geo on " + dbTableLogCount + " (geocode)");
            execSQL("create index if not exists in_logsoff_geo on " + dbTableLogsOffline + " (geocode)");
            execSQL("create index if not exists in_trck_geo on " + dbTableTrackables + " (geocode)");
        }

        private void execSQL(string q)
        {
            _db.ExecuteNonQuery(q);
        }
    }
}
