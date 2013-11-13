using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Utils
{
    public class Conversion
    {
        public static double StringToDouble(string s)
        {
            double d = 1.5;
            if (d.ToString().IndexOf('.') > 0)
            {
                return double.Parse(s.Replace(',', '.'));
            }
            else
            {
                return double.Parse(s.Replace('.', ','));
            }
        }

        public static string GetCoordinatesPresentation(GAPPSF.Core.Data.Location loc)
        {
            return GetCoordinatesPresentation(loc.Lat, loc.Lon);
        }
        public static string GetCoordinatesPresentation(double lat, double lon)
        {
            string NS = "N ";
            string EW = "E ";
            if (lat < 0)
            {
                lat = Math.Abs(lat);
                NS = "S ";
            }
            if (lon < 0)
            {
                lon = Math.Abs(lon);
                EW = "W ";
            }

            double minutes1 = 60.0 * (lat - (int)lat);
            double minutes2 = 60.0 * (lon - (int)lon);
            string d1 = (1000.0 * (minutes1 - (int)minutes1)).ToString("000");
            if (d1.Length > 3)
            {
                minutes1 += 1;
                d1 = "000";
            }
            string d2 = (1000.0 * (minutes2 - (int)minutes2)).ToString("000");
            if (d2.Length > 3)
            {
                minutes2 += 1;
                d2 = "000";
            }
            return string.Concat(
                NS, ((int)(lat)).ToString(), "° ", ((int)minutes1).ToString("00"), ".", d1, " ",
                EW, ((int)(lon)).ToString(), "° ", ((int)minutes2).ToString("00"), ".", d2
                );
        }

        public static GAPPSF.Core.Data.Location StringToLocation(string s)
        {
            GAPPSF.Core.Data.Location result = null;
            try
            {
                double Lat;
                double Lon;
                string[] parts = s.Split(new char[] { ' ', 'N', 'E', 'S', 'W', '.', '°', ',', '\'', '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 6 || parts.Length == 4)
                {
                    if (parts.Length == 6)
                    {
                        Lat = StringToDouble(parts[0]) + ((StringToDouble(parts[1]) + (StringToDouble(parts[2]) / 1000.0)) / 60.0);
                        Lon = StringToDouble(parts[3]) + ((StringToDouble(parts[4]) + (StringToDouble(parts[5]) / 1000.0)) / 60.0);

                        if (s.IndexOf("S", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Lat = -1.0 * Lat;
                        }
                        if (s.IndexOf("W", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Lon = -1.0 * Lon;
                        }
                    }
                    else
                    {
                        Lat = StringToDouble(string.Format("{0},{1}", parts[0], parts[1]));
                        Lon = StringToDouble(string.Format("{0},{1}", parts[2], parts[3]));
                    }
                    result = new GAPPSF.Core.Data.Location(Lat, Lon);

                }
            }
            catch
            {
            }
            return result;
        }

        public static int GetCacheIDFromCacheCode(string cacheCode)
        {
            const string v = "0123456789ABCDEFGHJKMNPQRTVWXYZ";

            int result = 0;
            try
            {
                string s = cacheCode.Substring(2).ToUpper();
                int baseValue = 31;
                if (s.Length < 4 || (s.Length == 4 && s[0] <= 'F'))
                {
                    baseValue = 16;
                }
                int mult = 1;
                while (s.Length > 0)
                {
                    char c = s[s.Length - 1];
                    result += mult * v.IndexOf(c);
                    mult *= baseValue;
                    s = s.Substring(0, s.Length - 1);
                }
                if (baseValue > 16)
                {
                    result -= 411120;
                }
            }
            catch
            {
                result = -1;
            }
            return result;
        }

        public static string GetCacheCodeFromCacheID(int cacheId)
        {
            const string v = "0123456789ABCDEFGHJKMNPQRTVWXYZ";

            string result = "";
            try
            {
                int i = cacheId;
                int baseValue = 31;
                if (i <= 65535)
                {
                    baseValue = 16;
                }
                else
                {
                    i += 411120;
                }
                while (i > 0)
                {
                    result = string.Concat(v[i % baseValue], result);
                    i /= baseValue;
                }
                result = string.Concat("GC", result);
            }
            catch
            {
                result = "";
            }
            return result;
        }


        public static string StripHtmlTags(string HTML)
        {
            // Removes tags from passed HTML           
            System.Text.RegularExpressions.Regex objRegEx = new System.Text.RegularExpressions.Regex("<[^>]*>");

            return objRegEx.Replace(HTML, "");
        }
    }
}
