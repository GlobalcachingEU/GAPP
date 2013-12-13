using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace GAPPSF.Utils
{
    public class Geocoder
    {
        public static string GetCityName(Core.Data.Location ll)
        {
            return GetCityName(ll.Lat, ll.Lon);
        }
        public static string GetCityName(double lat, double lon)
        {
            string result = "";
            try
            {
                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(string.Format("http://maps.googleapis.com/maps/api/geocode/xml?latlng={0},{1}&sensor=false", lat.ToString().Replace(',', '.'), lon.ToString().Replace(',', '.')));
                wr.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
                wr.Method = WebRequestMethods.Http.Get;
                HttpWebResponse webResponse = (HttpWebResponse)wr.GetResponse();
                StreamReader reader = new StreamReader(webResponse.GetResponseStream());
                string doc = reader.ReadToEnd();
                webResponse.Close();
                if (doc != null && doc.Length > 0)
                {
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(doc);
                    XmlNodeList nl = xdoc.SelectNodes("GeocodeResponse/result/address_component");
                    foreach (XmlNode n in nl)
                    {
                        XmlNode nt = n.SelectSingleNode("type");
                        if (nt != null && nt.InnerText == "locality")
                        {
                            result = n.SelectSingleNode("long_name").InnerText;
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

        public static Core.Data.Location GetLocationOfAddress(string address)
        {
            Core.Data.Location result = null;
            try
            {
                string s = null;
                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(string.Format("http://maps.googleapis.com/maps/api/geocode/xml?address={0}&sensor=false", HttpUtility.UrlEncode(address)));
                wr.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
                wr.Method = WebRequestMethods.Http.Get;
                using (HttpWebResponse webResponse = (HttpWebResponse)wr.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        s = reader.ReadToEnd();
                        webResponse.Close();
                    }
                }
                if (s != null && s.Length > 0)
                {
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(s);
                    XmlNode n = xdoc.SelectSingleNode("GeocodeResponse/result/geometry/location");
                    result = new Core.Data.Location(Conversion.StringToDouble(n.SelectSingleNode("lat").InnerText), Conversion.StringToDouble(n.SelectSingleNode("lng").InnerText));
                }
            }
            catch
            {
                result = null;
            }
            return result;
        }
    }
}
