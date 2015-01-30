using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Web;
using System.Collections;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.CGeo
{
    public class Export : Utils.BasePlugin.BaseExportFilter
    {
        public const string ACTION_EXPORT_ALL = "Export to c:geo|All";
        public const string ACTION_EXPORT_SELECTED = "Export to c:geo|Selected";
        public const string ACTION_EXPORT_ACTIVE = "Export to c:geo|Active";

        public const string STR_NOGEOCACHESELECTED = "No geocache selected for export";
        public const string STR_ERROR = "Error";
        public const string STR_EXPORTINGCB = "Exporting to c:geo...";
        public const string STR_CREATINGFILE = "Creating file...";

        private string _folder = null;
        private List<Framework.Data.Geocache> _gcList = null;
        private Utils.DBConComSqlite _dbcon = null;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_EXPORT_ALL);
            AddAction(ACTION_EXPORT_SELECTED);
            AddAction(ACTION_EXPORT_ACTIVE);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_EXPORTINGCB));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CREATINGFILE));

            return await base.InitializeAsync(core);
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

        private long GetcgeoTime(DateTime dt)
        {

            long result;
            result = (long)(dt - (new DateTime(1970, 1, 1))).TotalMilliseconds;
            return result;
        }

        protected override void ExportMethod()
        {
            int max = _gcList.Count;
            int index = 0;

            Hashtable htAttributes = new Hashtable();
            htAttributes[1] = "dogs";
            htAttributes[2] = "fee";
            htAttributes[3] = "rappelling";
            htAttributes[4] = "boat";
            htAttributes[5] = "scuba";
            htAttributes[6] = "kids";
            htAttributes[7] = "onehour";
            htAttributes[8] = "scenic";
            htAttributes[9] = "hiking";
            htAttributes[10] = "climbing";
            htAttributes[11] = "wading";
            htAttributes[12] = "swimming";
            htAttributes[13] = "available";
            htAttributes[14] = "night";
            htAttributes[15] = "winter";
            htAttributes[16] = "cactus";
            htAttributes[17] = "poisonoak";
            htAttributes[18] = "dangerousanimals";
            htAttributes[19] = "ticks";
            htAttributes[20] = "mine";
            htAttributes[21] = "cliff";
            htAttributes[22] = "hunting";
            htAttributes[23] = "danger";
            htAttributes[24] = "wheelchair";
            htAttributes[25] = "parking";
            htAttributes[26] = "public";
            htAttributes[27] = "water";
            htAttributes[28] = "restrooms";
            htAttributes[29] = "phone";
            htAttributes[30] = "picnic";
            htAttributes[31] = "camping";
            htAttributes[32] = "bicycles";
            htAttributes[33] = "motorcycles";
            htAttributes[34] = "quads";
            htAttributes[35] = "jeeps";
            htAttributes[36] = "snowmobiles";
            htAttributes[37] = "horses";
            htAttributes[38] = "campfires";
            htAttributes[39] = "thorn";
            htAttributes[40] = "stealth";
            htAttributes[41] = "stroller";
            htAttributes[42] = "firstaid";
            htAttributes[43] = "cow";
            htAttributes[44] = "flashlight";
            htAttributes[45] = "landf";
            htAttributes[46] = "rv";
            htAttributes[47] = "field_puzzle";
            htAttributes[48] = "UV";
            htAttributes[49] = "snowshoes";
            htAttributes[50] = "skiis";
            htAttributes[51] = "s-tool";
            htAttributes[52] = "nightcache";
            htAttributes[53] = "parkngrab";
            htAttributes[54] = "AbandonedBuilding";
            htAttributes[55] = "hike_short";
            htAttributes[56] = "hike_med";
            htAttributes[57] = "hike_long";
            htAttributes[58] = "fuel";
            htAttributes[59] = "food";
            htAttributes[60] = "wirelessbeacon";
            htAttributes[61] = "partnership";
            htAttributes[62] = "seasonal";
            htAttributes[63] = "touristOK";
            htAttributes[64] = "treeclimbing";
            htAttributes[65] = "frontyard";
            htAttributes[66] = "teamwork";
            htAttributes[67] = "geotour";


            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_EXPORTINGCB, STR_CREATINGFILE, max, 0, true))
            {
                string cbFile = System.IO.Path.Combine(_folder, "cgeo.sqlite");
                string cbDataFile = System.IO.Path.Combine(_folder, "data");

                if (System.IO.File.Exists(cbFile))
                {
                    System.IO.File.Delete(cbFile);
                }
                if (System.IO.File.Exists(cbDataFile))
                {
                    System.IO.File.Delete(cbDataFile);
                }
                CreateDatabase(cbFile);

                if (_dbcon != null)
                {
                    _dbcon.ExecuteNonQuery("CREATE TABLE android_metadata (locale TEXT)");
                    _dbcon.ExecuteNonQuery("INSERT INTO android_metadata VALUES('en_US')");
                    //_dbcon.ExecuteNonQuery("CREATE TABLE sqlite_sequence (name, seq)");

                    DbCommand cmd = _dbcon.Command;

                    int detailed = 1;
                    int reason = 1;
                    int reliable_latlon = 1;

                    DateTime dt = DateTime.Now.AddSeconds(2);
                    foreach (Framework.Data.Geocache gc in _gcList)
                    {
                        //----------------------------
                        // CACHE
                        //----------------------------
                        cmd.Parameters.Clear();
                        cmd.CommandText = "insert into cg_caches (updated, detailed, detailedupdate, visiteddate, geocode, reason, cacheid, guid, type, name, owner, owner_real, hidden, hint, size, difficulty, terrain, latlon, location, latitude, longitude, reliable_latlon, personal_note, shortdesc, description, favourite_cnt, disabled, archived, members, found, coordsChanged, finalDefined) values (@updated, 1, @detailedupdate, @visiteddate, @geocode, @reason, @cacheid, @guid, @type, @name, @owner, @owner_real, @hidden, @hint, @size, @difficulty, @terrain, @latlon, @location, @latitude, @longitude, @reliable_latlon, @personal_note, @shortdesc, @description, @favourite_cnt, @disabled, @archived, @members, @found, @coordsChanged, @finalDefined)";
                        DbParameter par;
                        par = cmd.CreateParameter();
                        par.ParameterName = "@updated";
                        par.DbType = DbType.Int64;
                        par.Value = GetcgeoTime(DateTime.Now);
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@detailed";
                        par.DbType = DbType.Int32;
                        par.Value = detailed;
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@detailedupdate";
                        par.DbType = DbType.UInt64;
                        par.Value = GetcgeoTime(DateTime.Now);
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@visiteddate";
                        par.DbType = DbType.UInt64;
                        if (gc.FoundDate == null)
                        {
                            par.Value = 0;
                        }
                        else
                        {
                            par.Value = GetcgeoTime((DateTime)gc.FoundDate);
                        }
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@geocode";
                        par.DbType = DbType.String;
                        par.Value = gc.Code;
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@reason";
                        par.DbType = DbType.Int32;
                        par.Value = reason;
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@cacheid";
                        par.DbType = DbType.String;
                        if (gc.ID.StartsWith("GC"))
                        {
                            par.Value = Utils.Conversion.GetCacheIDFromCacheCode(gc.Code).ToString();
                        }
                        else
                        {
                            par.Value = gc.ID;
                        }
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@guid";
                        par.DbType = DbType.String;
                        par.Value = DBNull.Value;
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@type";
                        par.DbType = DbType.String;
                        par.Value = gc.GeocacheType.GPXTag.Split(new char[] { ' ', '-' })[0].ToLower();
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@name";
                        par.DbType = DbType.String;
                        par.Value = gc.Name??"";
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@owner";
                        par.DbType = DbType.String;
                        par.Value = gc.PlacedBy ?? "";
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@owner_real";
                        par.DbType = DbType.String;
                        par.Value = gc.Owner ?? "";
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@hidden";
                        par.DbType = DbType.UInt64;
                        par.Value = GetcgeoTime(gc.PublishedTime);
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@hint";
                        par.DbType = DbType.String;
                        par.Value = gc.EncodedHints ?? "";
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@size";
                        par.DbType = DbType.String;
                        par.Value = gc.Container.Name.ToLower();
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@difficulty";
                        par.DbType = DbType.Single;
                        par.Value = (float)gc.Difficulty;
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@terrain";
                        par.DbType = DbType.Single;
                        par.Value = (float)gc.Terrain;
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@latlon";
                        par.DbType = DbType.String;
                        par.Value = DBNull.Value;
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@location";
                        par.DbType = DbType.String;
                        if (string.IsNullOrEmpty(gc.State))
                        {
                            par.Value = gc.Country ?? "";
                        }
                        else
                        {
                            par.Value = string.Format("{0}, {1}", gc.State, gc.Country ?? "");
                        }
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@latitude";
                        par.DbType = DbType.Double;
                        par.Value = gc.ContainsCustomLatLon ? gc.CustomLat : gc.Lat;
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@longitude";
                        par.DbType = DbType.Double;
                        par.Value = gc.ContainsCustomLatLon ? gc.CustomLon : gc.Lon;
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@reliable_latlon";
                        par.DbType = DbType.Int32;
                        par.Value = reliable_latlon;
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@personal_note";
                        par.DbType = DbType.String;
                        par.Value = gc.PersonaleNote??"";
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@shortdesc";
                        par.DbType = DbType.String;
                        if (gc.ShortDescriptionInHtml)
                        {
                            par.Value = gc.ShortDescription ?? "";
                        }
                        else
                        {
                            par.Value = HttpUtility.HtmlEncode(gc.ShortDescription ?? "");
                        }
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@description";
                        par.DbType = DbType.String;
                        if (gc.LongDescriptionInHtml)
                        {
                            par.Value = gc.LongDescription ?? "";
                        }
                        else
                        {
                            par.Value = HttpUtility.HtmlEncode(gc.LongDescription ?? "");
                        }
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@favourite_cnt";
                        par.DbType = DbType.Int32;
                        par.Value = gc.Favorites;
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@disabled";
                        par.DbType = DbType.Int32;
                        par.Value = gc.Available ? 0 : 1;
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@archived";
                        par.DbType = DbType.Int32;
                        par.Value = gc.Archived ? 1 : 0;
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@members";
                        par.DbType = DbType.Int32;
                        par.Value = gc.MemberOnly ? 1 : 0;
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@found";
                        par.DbType = DbType.Int32;
                        par.Value = gc.Found ? 1 : 0;
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@coordsChanged";
                        par.DbType = DbType.Int32;
                        par.Value = gc.CustomCoords ? 1 : 0;
                        cmd.Parameters.Add(par);
                        par = cmd.CreateParameter();
                        par.ParameterName = "@finalDefined";
                        par.DbType = DbType.Int32;
                        par.Value = 0;
                        cmd.Parameters.Add(par);

                        int res = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();

                        //----------------------------
                        // ATTRIBUTES
                        //----------------------------
                        List<int> attrs = gc.AttributeIds;
                        if (attrs != null && attrs.Count > 0)
                        {
                            cmd.CommandText = "insert into cg_attributes (geocode, updated, attribute) values (@geocode, @updated, @attribute)";
                            par = cmd.CreateParameter();
                            par.ParameterName = "@geocode";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@updated";
                            par.DbType = DbType.Int64;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@attribute";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            foreach (int att in attrs)
                            {
                                string atname = htAttributes[Math.Abs(att)] as string;
                                if (!string.IsNullOrEmpty(atname))
                                {
                                    atname = atname.ToLower();
                                    cmd.Parameters["@geocode"].Value = gc.Code;
                                    cmd.Parameters["@updated"].Value = GetcgeoTime(DateTime.Now);
                                    cmd.Parameters["@attribute"].Value = att > 0 ? string.Format("{0}_yes", atname) : string.Format("{0}_no", atname);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            cmd.Parameters.Clear();
                        }

                        //----------------------------
                        // WAYPOINTS
                        //----------------------------
                        List<Framework.Data.Waypoint> wpts = Utils.DataAccess.GetWaypointsFromGeocache(Core.Waypoints, gc.Code);
                        if (wpts != null && wpts.Count > 0)
                        {
                            cmd.CommandText = "insert into cg_waypoints (geocode, updated, type, prefix, lookup, name, latlon, latitude, longitude, note) values (@geocode, @updated, @type, @prefix, @lookup, @name, @latlon, @latitude, @longitude, @note)";
                            par = cmd.CreateParameter();
                            par.ParameterName = "@geocode";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@updated";
                            par.DbType = DbType.Int64;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@type";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@prefix";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@lookup";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@name";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@latlon";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@latitude";
                            par.DbType = DbType.Double;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@longitude";
                            par.DbType = DbType.Double;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@note";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            foreach (Framework.Data.Waypoint wp in wpts)
                            {
                                if (wp.Lat != null && wp.Lon != null)
                                {
                                    cmd.Parameters["@geocode"].Value = gc.Code;
                                    cmd.Parameters["@updated"].Value = GetcgeoTime(DateTime.Now);
                                    switch (wp.WPType.ID)
                                    {
                                        case 217:
                                            cmd.Parameters["@type"].Value = "pkg";
                                            break;
                                        case 220:
                                            cmd.Parameters["@type"].Value = "flag";
                                            break;
                                        case 218:
                                            cmd.Parameters["@type"].Value = "puzzle";
                                            break;
                                        case 452:
                                            cmd.Parameters["@type"].Value = "waypoint";
                                            break;
                                        case 219:
                                            cmd.Parameters["@type"].Value = "stage";
                                            break;
                                        case 221:
                                            cmd.Parameters["@type"].Value = "trailhead";
                                            break;
                                        default:
                                            cmd.Parameters["@type"].Value = "waypoint";
                                            break;
                                    }
                                    cmd.Parameters["@prefix"].Value = wp.Code.Substring(0,2);
                                    cmd.Parameters["@lookup"].Value = "---";
                                    cmd.Parameters["@name"].Value = wp.Description ?? "";
                                    cmd.Parameters["@latlon"].Value = Utils.Conversion.GetCoordinatesPresentation((double)wp.Lat,(double)wp.Lon);
                                    cmd.Parameters["@latitude"].Value = (double)wp.Lat;
                                    cmd.Parameters["@longitude"].Value = (double)wp.Lon;
                                    cmd.Parameters["@note"].Value = wp.Comment??"";

                                    cmd.ExecuteNonQuery();
                                }
                            }
                            cmd.Parameters.Clear();
                        }
                        
                        //----------------------------
                        // LOGS
                        //----------------------------
                        List<Framework.Data.Log> lgs = Utils.DataAccess.GetLogs(Core.Logs, gc.Code);
                        if (lgs != null && lgs.Count > 0)
                        {
                            cmd.CommandText = "insert into cg_logs (geocode, updated, type, author, log, date, friend) values (@geocode, @updated, @type, @author, @log, @date, 0)";
                            par = cmd.CreateParameter();
                            par.ParameterName = "@geocode";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@updated";
                            par.DbType = DbType.Int64;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@type";
                            par.DbType = DbType.Int32;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@author";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@log";
                            par.DbType = DbType.String;
                            cmd.Parameters.Add(par);
                            par = cmd.CreateParameter();
                            par.ParameterName = "@date";
                            par.DbType = DbType.Int64;
                            cmd.Parameters.Add(par);

                            foreach (Framework.Data.Log lg in lgs)
                            {
                                cmd.Parameters["@geocode"].Value = gc.Code;
                                cmd.Parameters["@updated"].Value = GetcgeoTime(DateTime.Now);
                                cmd.Parameters["@type"].Value = lg.LogType.ID;
                                cmd.Parameters["@author"].Value = lg.Finder??"";
                                cmd.Parameters["@log"].Value = HttpUtility.HtmlEncode(lg.Text ?? "").Replace("\r", "").Replace("\n", "<br />");
                                cmd.Parameters["@date"].Value = GetcgeoTime(lg.Date);

                                cmd.ExecuteNonQuery();
                            }
                            cmd.Parameters.Clear();
                        }

                        index++;
                        if (DateTime.Now > dt)
                        {
                            if (!progress.UpdateProgress(STR_EXPORTINGCB, STR_CREATINGFILE, max, index))
                            {
                                break;
                            }
                            dt = DateTime.Now.AddSeconds(2);
                        }
                    }
                    //_dbcon.ExecuteNonQuery(string.Format("insert into sqlite_sequence (name, seq) values ('cg_caches', {0})", index));
                    _dbcon.Dispose();
                    _dbcon = null;

                    //not working. you have to go through recover database on c:geo
                    //System.IO.File.Copy(cbFile, cbDataFile);
                }
            }
        }

        private void CreateDatabase(string filename)
        {
            _dbcon = new Utils.DBConComSqlite(filename);
            cgData cgd = new cgData(_dbcon);
            cgd.Create();
        }
    }
}
