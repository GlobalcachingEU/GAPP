using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.IO;
using System.Xml;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public class OKAPIService
    {
        [Serializable]
        public class Log
        {
            public string uuid;
            public string date;
            public string cache_code;
            public string type;
            public string comment;
            [OptionalField]
            public User user;

            public Log() { }
            public Log(Dictionary<string, object> dictionary)
            {
                uuid = dictionary["uuid"] as string;
                date = dictionary["date"] as string;
                user = new User(dictionary["user"] as Dictionary<string, object>);
                type = dictionary["type"] as string;
                comment = dictionary["comment"] as string;
            }
        }

        [Serializable]
        public class Geocache
        {
            public string code;
            public string name;
            //public string names;
            public string location;
            public string type;
            public string status;
            public string url;
            public User owner;
            //public string gc_code;
            public bool is_found;
            public string size2;
            public double difficulty;
            public double terrain;
            public string description;
            public string hint2;
            public List<GeocacheImage> images;
            public List<string> attr_acodes;
            public List<Log> latest_logs;
            public string country;
            public string state;
            public string date_hidden;
            public List<Waypoint> alt_wpts;

            public Geocache() { }

            public Geocache(Dictionary<string, object> dictionary)
            {
                code = dictionary["code"] as string;
                name = dictionary["name"] as string;
                location = dictionary["location"] as string;
                type = dictionary["type"] as string;
                status = dictionary["status"] as string;
                url = dictionary["url"] as string;
                owner = new User(dictionary["owner"] as Dictionary<string, object>);
                is_found = (bool)dictionary["is_found"];
                size2 = dictionary["size2"] as string;
                if (dictionary["difficulty"].GetType() == typeof(int))
                {
                    difficulty = (int)dictionary["difficulty"];
                }
                else
                {
                    difficulty = (double)(decimal)dictionary["difficulty"];
                }
                if (dictionary["terrain"].GetType() == typeof(int))
                {
                    terrain = (int)dictionary["terrain"];
                }
                else
                {
                    terrain = (double)(decimal)dictionary["terrain"];
                }
                description = dictionary["description"] as string;
                hint2 = dictionary["hint2"] as string;
                owner = new User(dictionary["owner"] as Dictionary<string, object>);
                images = new List<GeocacheImage>();
                if (dictionary["images"] as object[] != null)
                {
                    foreach (Dictionary<string, object> dict in dictionary["images"] as object[])
                    {
                        GeocacheImage gc = new GeocacheImage(dict);
                        images.Add(gc);
                    }
                }
                attr_acodes = new List<string>();
                if (dictionary["attr_acodes"] as object[] != null)
                {
                    foreach (string s in dictionary["attr_acodes"] as object[])
                    {
                        attr_acodes.Add(s);
                    }
                }
                latest_logs = new List<Log>();
                if (dictionary["latest_logs"] as object[] != null)
                {
                    foreach (Dictionary<string, object> dict in dictionary["latest_logs"] as object[])
                    {
                        Log lg = new Log(dict);
                        lg.cache_code = code;
                        latest_logs.Add(lg);
                    }
                }
                country = dictionary["country"] as string;
                state = dictionary["state"] as string;
                date_hidden = dictionary["date_hidden"] as string;
                alt_wpts = new List<Waypoint>();
                if (dictionary["alt_wpts"] as object[] != null)
                {
                    foreach (Dictionary<string, object> dict in dictionary["alt_wpts"] as object[])
                    {
                        Waypoint lg = new Waypoint(dict);
                        lg.cache_code = code;
                        alt_wpts.Add(lg);
                    }
                }
            }
        }

        [Serializable]
        public class User
        {
            public string uuid;
            public string username;
            public string profile_url;

            public User() { }
            public User(Dictionary<string, object> dictionary)
            {
                uuid = dictionary["uuid"] as string;
                username = dictionary["username"] as string;
                profile_url = dictionary["profile_url"] as string;
            }
        }

        [Serializable]
        public class Waypoint
        {
            public string name;
            public string location;
            public string type;
            public string type_name;
            public string sym;
            public string description;
            [OptionalField]
            public string cache_code;

            public Waypoint() { }
            public Waypoint(Dictionary<string, object> dictionary)
            {
                name = dictionary["name"] as string;
                location = dictionary["location"] as string;
                type = dictionary["type"] as string;
                type_name = dictionary["type_name"] as string;
                sym = dictionary["sym"] as string;
                description = dictionary["description"] as string;
            }
        }

        [Serializable]
        public class GeocacheImage
        {
            public string uuid;
            public string url;
            public string thumb_url;
            public string caption;
            public string unique_caption;
            public bool is_spoiler;

            public GeocacheImage() { }
            public GeocacheImage(Dictionary<string, object> dictionary)
            {
                uuid = dictionary["uuid"] as string;
                url = dictionary["url"] as string;
                thumb_url = dictionary["thumb_url"] as string;
                caption = dictionary["caption"] as string;
                unique_caption = dictionary["unique_caption"] as string;
                is_spoiler = (bool)dictionary["is_spoiler"];
            }
        }


        public static void develop_CreateAttributesList(SiteInfo si)
        {
            StringBuilder sb = new StringBuilder();
            string url = string.Format("{0}services/attrs/attribute_index?fields=name|gc_equivs&consumer_key={1}", si.OKAPIBaseUrl, HttpUtility.UrlEncode(si.ConsumerKey));
            string doc = GetResultOfUrl(url);
            var json = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
            var dict = (IDictionary<string, object>)json.DeserializeObject(doc);
            foreach (KeyValuePair<string, object> kp in dict)
            {
                string name = kp.Key;
                Dictionary<string, object> attrs = kp.Value as Dictionary<string, object>;
                if (attrs != null)
                {
                    //string name = attrs["name"] as string;
                    if (attrs["gc_equivs"] as object[] != null)
                    {
                        foreach (Dictionary<string, object> attr in attrs["gc_equivs"] as object[])
                        {
                            int id = (int)attr["id"];
                            int inc = (int)attr["inc"];
                            sb.AppendLine(string.Format("else if (aCode==\"{1}\") result = {2}{0};", id, name, inc == 0 ? "" : "-"));
                        }
                    }
                }
            }
        }

        public static int MapAttributeACodeToAttributeID(string aCode)
        {
            //generated with develop_CreateAttributesList
            int result = 0;
            if (aCode == "A9") result = -60;
            else if (aCode == "A15") result = -47;
            else if (aCode == "A18") result = -24;
            else if (aCode == "A19") result = -53;
            else if (aCode == "A21") result = -9;
            else if (aCode == "A22") result = -11;
            else if (aCode == "A24") result = -10;
            else if (aCode == "A25") result = -12;
            else if (aCode == "A26") result = -2;
            else if (aCode == "A27") result = -32;
            else if (aCode == "A33") result = -25;
            else if (aCode == "A34") result = -26;
            else if (aCode == "A35") result = -27;
            else if (aCode == "A36") result = -28;
            else if (aCode == "A37") result = -29;
            else if (aCode == "A39") result = -13;
            else if (aCode == "A40") result = 13;
            else if (aCode == "A41") result = 14;
            else if (aCode == "A42") result = -14;
            else if (aCode == "A43") result = -52;
            else if (aCode == "A44") result = 62;
            else if (aCode == "A45") result = -62;
            else if (aCode == "A47") result = -15;
            else if (aCode == "A52") result = -44;
            else if (aCode == "A53") result = -3;
            else if (aCode == "A55") result = -5;
            else if (aCode == "A56") result = -51;
            else if (aCode == "A57") result = -4;
            else if (aCode == "A59") result = -23;
            else if (aCode == "A61") result = -21;
            else if (aCode == "A62") result = -22;
            else if (aCode == "A63") result = -39;
            else if (aCode == "A64") result = -19;
            else if (aCode == "A65") result = -20;
            else if (aCode == "A66") result = -17;
            else if (aCode == "A67") result = -18;
            else if (aCode == "A70") result = -6;
            else if (aCode == "A71") result = -6;
            else if (aCode == "A73") result = 13;
            return result;
        }

        public static List<Geocache> GetGeocaches(SiteInfo si, List<string> gcCodes)
        {
            List<Geocache> result = new List<Geocache>();
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}services/caches/geocaches?format=json&lpc=5&user_uuid={1}&fields=code|name|location|type|status|url|owner|is_found|size2|difficulty|terrain|description|hint2|images|attr_acodes|latest_logs|country|state|date_hidden|alt_wpts&cache_codes=", si.OKAPIBaseUrl, HttpUtility.UrlEncode(si.UserID));
            for (int i = 0; i < gcCodes.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append('|');
                }
                sb.Append(gcCodes[i]);
            }
            sb.AppendFormat("&consumer_key={0}", HttpUtility.UrlEncode(si.ConsumerKey));
            string doc = GetResultOfUrl(sb.ToString());
            var json = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
            var dict = (IDictionary<string, object>)json.DeserializeObject(doc);
            foreach (KeyValuePair<string, object> kp in dict)
            {
                if (kp.Value as Dictionary<string, object> != null)
                {
                    Geocache gc = new Geocache(kp.Value as Dictionary<string, object>);
                    result.Add(gc);
                }
            }
            return result;
        }

        public static List<string> GetGeocachesWithinRadius(SiteInfo si, Framework.Data.Location loc, double radiusKm, string filter)
        {
            List<string> result;
            if (!string.IsNullOrEmpty(filter))
            {
                filter = string.Concat("&", filter);
            }
            string url = string.Format("{0}services/caches/search/nearest?center={1}|{2}&radius={3}{4}&limit=500&consumer_key={5}", si.OKAPIBaseUrl, loc.Lat.ToString().Replace(',', '.'), loc.Lon.ToString().Replace(',', '.'), radiusKm.ToString().Replace(',', '.'), filter, HttpUtility.UrlEncode(si.ConsumerKey));
            result = GetGeocachesByUrl(url);
            return result;
        }

        public static List<string> GetGeocachesInBBox(SiteInfo si, double minLat, double minLon, double maxLat, double maxLon)
        {
            List<string> result;
            string url = string.Format("{0}services/caches/search/bbox?bbox={1}|{2}|{3}|{4}&limit=500&consumer_key={5}", si.OKAPIBaseUrl, minLat.ToString().Replace(',', '.'), minLon.ToString().Replace(',', '.'), maxLat.ToString().Replace(',', '.'), maxLon.ToString().Replace(',', '.'), HttpUtility.UrlEncode(si.ConsumerKey));
            result = GetGeocachesByUrl(url);
            return result;
        }


        public static List<string> GetGeocachesByUrl(string url)
        {
            List<string> result = new List<string>();
            string doc = GetResultOfUrl(url);
            var json = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
            var dict = (IDictionary<string, object>)json.DeserializeObject(doc);
            foreach (KeyValuePair<string, object> kp in dict)
            {
                object[] gcList = kp.Value as object[];
                if (gcList != null)
                {
                    foreach (string gc in gcList)
                    {
                        result.Add(gc);
                    }
                }
            }
            return result;
        }

        public static List<Log> GetLogsOfUserID(SiteInfo si, string userid, int max, int start)
        {
            List<Log> result;

            string url = string.Format("{0}services/logs/userlogs?format=json&user_uuid={1}&limit={2}&offset={3}&fields=uuid|username&consumer_key={4}", si.OKAPIBaseUrl, HttpUtility.UrlEncode(userid), max, start, HttpUtility.UrlEncode(si.ConsumerKey));
            result = GetResultOfUrl<List<Log>>(url);

            return result;
        }

        public static string GetUserID(SiteInfo si, ref string username)
        {
            string result = "";
            string url = string.Format("{0}services/users/by_username?format=xmlmap2&username={1}&fields=uuid|username&consumer_key={2}", si.OKAPIBaseUrl, HttpUtility.UrlEncode(username), HttpUtility.UrlEncode(si.ConsumerKey));
            string doc = GetResultOfUrl(url);
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(doc);
            XmlNodeList nl = xdoc.SelectNodes("/object/string");
            if (nl != null)
            {
                foreach (XmlNode n in nl)
                {
                    XmlAttribute attr = n.Attributes["key"];
                    if (attr != null && attr.Value == "uuid")
                    {
                        result = n.InnerText;
                    }
                    else if (attr != null && attr.Value == "username")
                    {
                        username = n.InnerText;
                    }
                }
            }
            return result;
        }

        public static string GetResultOfUrl(string url)
        {
            string result = "";
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
            wr.Method = WebRequestMethods.Http.Get;
            using (HttpWebResponse webResponse = (HttpWebResponse)wr.GetResponse())
            using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }

        public static T GetResultOfUrl<T>(string url)
        {
            var instance = typeof(T);
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
            wr.Method = WebRequestMethods.Http.Get;
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            using (System.IO.Stream stream = wr.GetResponse().GetResponseStream())
            {
                return (T)ser.ReadObject(stream);
            }
        }

    }
}
