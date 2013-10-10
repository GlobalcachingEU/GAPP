using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Web;

namespace GlobalcachingApplication.Plugins.GCEU
{
    public class PuzzelRecovery : Utils.BasePlugin.Plugin
    {
        public const string ACTION_PERFORM = "Globalcaching.eu - recover puzzles";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            //AddAction(ACTION_PERFORM);

            return base.Initialize(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Action;
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_PERFORM)
                {
                    using (DBConSqlServer dbcon = new DBConSqlServer())
                    {
                        List<string> allCodes = new List<string>();

                        /*
                        SqlDataReader dr = dbcon.ExecuteReader("select Waypoint from GlobalCache");
                        while (dr.Read())
                        {
                            allCodes.Add(dr.GetString(0));
                        }
                        dr = dbcon.ExecuteReader("select Waypoint from Caches where Substring(Waypoint,1,1)='N'");
                        while (dr.Read())
                        {
                            allCodes.Remove(dr.GetString(0));
                        }

                        foreach (string wp in allCodes)
                        {
                            if ((int)dbcon.ExecuteScalar(string.Format("select count(1) from GlobalLog where Waypoint='{0}'", wp)) > 0)
                            {
                                //ok, was published once (regardless of deletion)
                                string minLogDate = (string)dbcon.ExecuteScalar(string.Format("select Min(Logdate) from GlobalLog where Waypoint='{0}'", wp));
                                //Logtype, UserID
                                //check archived, just of any (found=1)
                                bool archived = (int)dbcon.ExecuteScalar(string.Format("select count(1) from GlobalLog where Waypoint='{0}' and Logtype=7 and Deleted=0", wp)) > 0;

                                string owner = (string)dbcon.ExecuteScalar(string.Format("select yaf_User.Name from Yaf_User inner join GlobalCache on GlobalCache.UserID=yaf_User.UserID where Waypoint='{0}'", wp));
                                string ownerId = dbcon.ExecuteScalar(string.Format("select UserID from GlobalCache where Waypoint='{0}'", wp)).ToString();

                                dr = dbcon.ExecuteReader(string.Format("select * from GlobalCache where Waypoint='{0}'", wp));
                                if (dr.Read())
                                {
                                    //all fields should be filled
                                    string Title = getDBString(dr, "Title");
                                    string OwnerAlias = getDBString(dr, "OwnerAlias");
                                    string CacheType = getDBString(dr, "CacheType");
                                    string ContainerSize = getDBString(dr, "ContainerSize");
                                    double lat = 1.0;
                                    double lon = 1.0;
                                    string Country = getDBString(dr, "Country");
                                    string Difficulty = getDBString(dr, "Difficulty");
                                    string Terrain = getDBString(dr, "Terrain");
                                    bool DescriptionInHTML = (bool)dr["DescriptionInHTML"];
                                    bool Adoption = (bool)dr["Adoption"];
                                    string Description = getDBString(dr, "Description");
                                    string Hint = getDBString(dr, "Hint");
                                    string Attributes = getDBString(dr, "Attributes");
                                    string area = "";
                                    double Distance = -1;
                                    object o = dr["Distance"];
                                    if (o.GetType() != typeof(System.DBNull))
                                    {
                                        Distance = (double)dr["Distance"];
                                    }

                                    string url = string.Format("http://www.globalcaching.eu/Puzzles/Puzzle.aspx?wp={0}", wp);

                                    if (Title.Length == 0 ||
                                        CacheType.Length == 0 ||
                                        ContainerSize.Length == 0 ||
                                        lat < 0 ||
                                        lon < 0 ||
                                        Country.Length == 0 ||
                                        Difficulty.Length == 0 ||
                                        Terrain.Length == 0 ||
                                        Description.Length == 0 ||
                                        Country != "Netherlands" ||
                                        Distance < 0)
                                    {
                                        //oeps
                                    }
                                    else
                                    {
                                        StringBuilder sb = new StringBuilder();
                                        sb.AppendFormat("<time>{0}T00:00:00</time>", dr["CreationDate"]); sb.AppendLine();
                                        sb.AppendFormat("<name>{0}</name>", wp); sb.AppendLine();
                                        sb.AppendFormat("<desc>{0} by {1}, {2} ({3}/{4})</desc>", HttpUtility.HtmlEncode(Title), HttpUtility.HtmlEncode(owner), CacheType, Difficulty, Terrain); sb.AppendLine();
                                        sb.AppendFormat("<url>{0}</url>", url); sb.AppendLine();
                                        sb.AppendFormat("<urlname>{0}</urlname>", HttpUtility.HtmlEncode(Title)); sb.AppendLine();
                                        sb.AppendLine("<sym>Geocache</sym>");
                                        sb.AppendFormat("<type>Geocache|{0}</type>", CacheType); sb.AppendLine();
                                        sb.AppendFormat("<groundspeak:cache id=\"{0}\" available=\"True\" archived=\"False\" xmlns:groundspeak=\"http://www.groundspeak.com/cache/1/0\">", dr["ID"]); sb.AppendLine();
                                        sb.AppendFormat("<groundspeak:name>{0}</groundspeak:name>", HttpUtility.HtmlEncode(Title)); sb.AppendLine();
                                        sb.AppendFormat("<groundspeak:placed_by>{0}</groundspeak:placed_by>", HttpUtility.HtmlEncode(owner)); sb.AppendLine();
                                        sb.AppendFormat("<groundspeak:owner id=\"{0}\">{1}</groundspeak:owner>", ownerId, HttpUtility.HtmlEncode(owner)); sb.AppendLine();
                                        sb.AppendFormat("<groundspeak:type>{0}</groundspeak:type>", CacheType); sb.AppendLine();
                                        sb.AppendFormat("<groundspeak:container>{0}</groundspeak:container>", ContainerSize); sb.AppendLine();
                                        sb.AppendFormat("<groundspeak:difficulty>{0}</groundspeak:difficulty>", Difficulty); sb.AppendLine();
                                        sb.AppendFormat("<groundspeak:terrain>{0}</groundspeak:terrain>", Terrain); sb.AppendLine();
                                        sb.AppendFormat("<groundspeak:country>{0}</groundspeak:country>", HttpUtility.HtmlEncode(Country)); sb.AppendLine();
                                        sb.AppendLine("<groundspeak:state>");
                                        sb.AppendLine("</groundspeak:state>");
                                        string tmp;
                                        if (DescriptionInHTML) tmp = "True"; else tmp = "False";
                                        sb.AppendFormat("<groundspeak:short_description html=\"{0}\">", tmp); sb.AppendLine();
                                        sb.AppendLine("</groundspeak:short_description>");
                                        sb.AppendFormat("<groundspeak:long_description html=\"{0}\">{1}", tmp, HttpUtility.HtmlEncode(Description)); sb.AppendLine();
                                        sb.AppendLine("</groundspeak:long_description>");
                                        sb.AppendFormat("<groundspeak:encoded_hints>{0}", HttpUtility.HtmlEncode(Hint)); sb.AppendLine();
                                        sb.AppendLine("</groundspeak:encoded_hints>");
                                        sb.AppendLine("<groundspeak:logs>");
                                        sb.AppendLine("</groundspeak:logs>");
                                        sb.AppendLine("<groundspeak:travelbugs />");
                                        sb.AppendLine("</groundspeak:cache>");

                                        string sadopt = "0";
                                        if (Adoption) sadopt = "1";
                                        
                                        dbcon.ExecuteNonQuery(string.Format("INSERT INTO Caches (Waypoint, Linktowp, Archived, CacheType, Lat, Lon, ContainerSize, Difficulty, Terrain, Beschrijving, HiddenDate, Area, Owner, Description, gpx, Country, Validated, Available, UserID, Afstand, Adoption, Attributes) VALUES ('{0}', '{1}', {20}, '{2}', {3}, {4}, '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', 1, {15}, {16}, {17}, {18}, '{19}')", wp, url, CacheType, lat.ToString().Replace(',', '.'), lon.ToString().Replace(',', '.'), ContainerSize, Difficulty, Terrain, Utils.Conversion.StripHtmlTags(Description).Replace("'", "''"), minLogDate, area, owner.Replace('\'', ' '), Title.Replace("'", "''"), sb.ToString().Replace("'", "''"), Country.Replace("'", "''"), archived ? "0" : "1", ownerId, Distance.ToString("0.0").Replace(',', '.').Replace(".0", ""), sadopt, Attributes, archived ? "1" : "0"));
                                        if (OwnerAlias.Length == 0)
                                        {
                                            dbcon.ExecuteNonQuery(string.Format("update Caches set OwnerAlias = NULL where Waypoint='{0}'", wp));
                                        }
                                        else
                                        {
                                            dbcon.ExecuteNonQuery(string.Format("update Caches set OwnerAlias = '{1}' where Waypoint='{0}'", wp, OwnerAlias.Replace("'", "")));
                                        }
                                        
                                    }
                                }

                            }
                        }
                        */


                        //FTF
                        SqlDataReader dr = dbcon.ExecuteReader("select Waypoint from Caches where Substring(Waypoint,1,1)='N' and (FTF is NULL or Waypoint='NC0001')");
                        while (dr.Read())
                        {
                            allCodes.Add(dr.GetString(0));
                        }
                        foreach (string wp in allCodes)
                        {
                            dr = dbcon.ExecuteReader(string.Format("select top 3 UserID from GlobalLog where Waypoint = '{0}' and Logtype=1 and Deleted=0 order by EntryDateTime asc", wp));
                            if (dr.Read())
                            {
                                string ftf = dr[0].ToString();
                                string stf = "NULL";
                                string ttf = "NULL";
                                if (dr.Read())
                                {
                                    stf = dr[0].ToString();
                                    if (dr.Read())
                                    {
                                        ttf = dr[0].ToString();
                                    }
                                }
                                dbcon.ExecuteNonQuery(string.Format("update Caches set FTF = {1}, STF = {2}, TTF = {3} where Waypoint='{0}'", wp, ftf, stf, ttf));
                            }
                        }
                    }
                }
            }
            return result;
        }

        private string getDBString(SqlDataReader dr, string item)
        {
            object o = dr[item];
            if (o.GetType() == typeof(System.DBNull))
            {
                return "";
            }
            return dr[item].ToString();
        }

    }
}
