using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using Gavaghan.Geodesy;

namespace GlobalcachingApplication.Utils
{
    public class GPXProcessor
    {
        public class ResultData
        {
            public List<Framework.Data.Geocache> Geocaches = new List<Framework.Data.Geocache>();
            public List<Framework.Data.Waypoint> Waypoints = new List<Framework.Data.Waypoint>();
            public List<Framework.Data.Log> Logs = new List<Framework.Data.Log>();
            public List<Framework.Data.LogImage> LogImages = new List<Framework.Data.LogImage>();
        }

        private Framework.Interfaces.ICore _core = null;
        private DateTime _gpxDataTime = DateTime.Now;
        private Version _cachesGpxVersion = null;

        public GPXProcessor(Framework.Interfaces.ICore core)
        {
            _core = core;
        }

        public Version CachesGPXVersion
        {
            get { return _cachesGpxVersion; }
        }

        public ResultData ProcessOpencachingComGGZFile(string filename)
        {
            ResultData result = new ResultData();
            //zip or gpx?
            try
            {
                byte[] data = new byte[1024];
                using (var fs = System.IO.File.OpenRead(filename))
                using (ZipInputStream s = new ZipInputStream(fs))
                {
                    ZipEntry theEntry = s.GetNextEntry();
                    while (theEntry != null)
                    {
                        if (theEntry.IsFile && theEntry.Name.ToLower().EndsWith(".gpx"))
                        {
                            StringBuilder sb = new StringBuilder();
                            while (true)
                            {
                                int size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    if (sb.Length == 0 && data[0] == 0xEF && size > 2)
                                    {
                                        sb.Append(System.Text.ASCIIEncoding.UTF8.GetString(data, 3, size - 3));
                                    }
                                    else
                                    {
                                        sb.Append(System.Text.ASCIIEncoding.UTF8.GetString(data, 0, size));
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (theEntry.Name.ToLower().IndexOf("-wpts.") > 0)
                            {
                                ProcessGeocachingComGPXWaypoints(result, sb.ToString());
                            }
                            else
                            {
                                ProcessGeocachingComGPXCaches(result, sb.ToString());
                            }
                        }

                        try
                        {
                            theEntry = s.GetNextEntry();
                        }
                        catch
                        {
                            //opencaching.de zip files generates an exception
                            theEntry = null;
                        }
                    }
                }
            }
            catch
            {
            }
            return result;
        }

        public ResultData ProcessGeocachingComGPXFile(string filename)
        {
            ResultData result = new ResultData();
            //zip or gpx?
            try
            {
                if (filename.ToLower().EndsWith("-wpts.gpx"))
                {
                    ProcessGeocachingComGPXWaypoints(result, System.IO.File.ReadAllText(filename));
                }
                else if (filename.ToLower().EndsWith(".gpx"))
                {
                    ProcessGeocachingComGPXCaches(result, System.IO.File.ReadAllText(filename));
                }
                else
                {
                    //zip - containts waypoints and caches
                    using (var fs = System.IO.File.OpenRead(filename))
                    using (ZipInputStream s = new ZipInputStream(fs))
                    {
                        ZipEntry theEntry = s.GetNextEntry();
                        byte[] data = new byte[1024];
                        while (theEntry != null)
                        {
                            StringBuilder sb = new StringBuilder();
                            while (true)
                            {
                                int size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    if (sb.Length == 0 && data[0] == 0xEF && size > 2)
                                    {
                                        sb.Append(System.Text.ASCIIEncoding.UTF8.GetString(data, 3, size - 3));
                                    }
                                    else
                                    {
                                        sb.Append(System.Text.ASCIIEncoding.UTF8.GetString(data, 0, size));
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (theEntry.Name.ToLower().IndexOf("-wpts.") > 0)
                            {
                                ProcessGeocachingComGPXWaypoints(result, sb.ToString());
                            }
                            else
                            {
                                ProcessGeocachingComGPXCaches(result, sb.ToString());
                            }

                            try
                            {
                                theEntry = s.GetNextEntry();
                            }
                            catch
                            {
                                //opencaching.de zip files generates an exception
                                theEntry = null;
                            }
                        }
                    }
                }
            }
            catch
            {
                result = null;
            }
            return result;
        }

        public void ProcessGeocachingComGPXWaypoints(ResultData data, string gpxDoc)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(gpxDoc);
                XmlElement root = doc.DocumentElement;
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("x", root.NamespaceURI);

                _gpxDataTime = DateTime.Parse(root.SelectSingleNode("x:time", nsmgr).InnerText);

                XmlNodeList wps = root.SelectNodes("x:wpt", nsmgr);
                if (wps != null)
                {
                    foreach (XmlNode n in wps)
                    {
                        Framework.Data.Waypoint wp = new Framework.Data.Waypoint();

                        wp.Code = n.SelectSingleNode("x:name", nsmgr).InnerText;
                        wp.GeocacheCode = string.Concat("GC",wp.Code.Substring(2));
                        wp.DataFromDate = _gpxDataTime;
                        wp.Comment = n.SelectSingleNode("x:cmt", nsmgr).InnerText;
                        wp.Lat = Utils.Conversion.StringToDouble(n.Attributes["lat"].InnerText);
                        wp.Lon = Utils.Conversion.StringToDouble(n.Attributes["lon"].InnerText);
                        wp.Description = n.SelectSingleNode("x:desc", nsmgr).InnerText;
                        wp.ID = wp.Code;
                        wp.Name = wp.Code;
                        wp.Comment = n.SelectSingleNode("x:cmt", nsmgr).InnerText;
                        wp.Time = DateTime.Parse(n.SelectSingleNode("x:time", nsmgr).InnerText);
                        wp.Url = n.SelectSingleNode("x:url", nsmgr).InnerText;
                        wp.UrlName = n.SelectSingleNode("x:urlname", nsmgr).InnerText;
                        wp.WPType = DataAccess.GetWaypointType(_core.WaypointTypes, n.SelectSingleNode("x:sym", nsmgr).InnerText);

                        data.Waypoints.Add(wp);
                    }
                }

            }
            catch
            {
            }
        }

        public string SafeInnerText(XmlNode n, string defaultValue)
        {
            if (n != null)
            {
                return n.InnerText;
            }
            else
            {
                return defaultValue;
            }
        }
        public string SafeAttributeInnerText(XmlNode n, string attr, string defaultValue)
        {
            if (n != null)
            {
                XmlAttribute res = n.Attributes[attr];
                if (res != null)
                {
                    return res.InnerText;
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }

        public void ProcessGeocachingComGPXCaches(ResultData data, string gpxDoc)
        {
            try
            {
                string firstPart;
                if (gpxDoc.Length < 2000)
                {
                    firstPart = gpxDoc;
                }
                else
                {
                    firstPart = gpxDoc.Substring(0, 2000);
                }
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(gpxDoc);
                XmlElement root = doc.DocumentElement;
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);

                if (firstPart.IndexOf("http://www.topografix.com/GPX/1/1", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    nsmgr.AddNamespace("x", "http://www.topografix.com/GPX/1/1");
                }
                else
                {
                    nsmgr.AddNamespace("x", "http://www.topografix.com/GPX/1/0");
                }

                if (firstPart.IndexOf("http://www.groundspeak.com/cache/1/0/2", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    nsmgr.AddNamespace("y", "http://www.groundspeak.com/cache/1/0/2");
                    _cachesGpxVersion = new Version(1,0,2);
                }
                else if (firstPart.IndexOf("http://www.groundspeak.com/cache/1/0/1", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    if (firstPart.IndexOf("creator=\"Opencaching.de - http://www.opencaching.de\"") > 0)
                    {
                        nsmgr.AddNamespace("y", "http://www.groundspeak.com/cache/1/0");
                    }
                    else if (firstPart.IndexOf("http://www.opencaching.com/xmlschemas/opencaching/1/0") > 0)
                    {
                        nsmgr.AddNamespace("y", "http://www.groundspeak.com/cache/1/0");
                    }
                    else
                    {
                        nsmgr.AddNamespace("y", "http://www.groundspeak.com/cache/1/0/1");
                    }
                    _cachesGpxVersion = new Version(1, 0, 1);
                }
                else if (firstPart.IndexOf("http://www.groundspeak.com/cache/1/1", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    nsmgr.AddNamespace("y", "http://www.groundspeak.com/cache/1/1");
                    _cachesGpxVersion = new Version(1, 1, 0);
                }
                else
                {
                    nsmgr.AddNamespace("y", "http://www.groundspeak.com/cache/1/0");
                    _cachesGpxVersion = new Version(1, 0, 0);
                }

                _gpxDataTime = DateTime.Parse(root.SelectSingleNode("x:time", nsmgr).InnerText);

                XmlNodeList wps = root.SelectNodes("x:wpt", nsmgr);
                if (wps != null)
                {
                    Version V102 = new Version(1, 0, 2);
                    foreach (XmlNode wp in wps)
                    {
                        XmlNode n = wp.SelectSingleNode("y:cache", nsmgr);
                        if (n == null)
                        {
                            //assume Child waypoint
                            //check if it is a child waypoint by checking the name
                            Framework.Data.Geocache parentGeocache = null;
                            string wpname = wp.SelectSingleNode("x:name", nsmgr).InnerText.Substring(2);
                            parentGeocache = (from g in data.Geocaches where g.Code.Substring(2) == wpname select g).FirstOrDefault();
                            if (parentGeocache == null)
                            {
                                parentGeocache = (from Framework.Data.Geocache g in _core.Geocaches where g.Code.Substring(2) == wpname select g).FirstOrDefault();
                                if (parentGeocache == null)
                                {
                                    //parent is not available
                                    //now what?
                                    continue;
                                }
                            }

                            Framework.Data.Waypoint cwp = new Framework.Data.Waypoint();

                            cwp.Code = wp.SelectSingleNode("x:name", nsmgr).InnerText;
                            cwp.GeocacheCode = string.Concat(parentGeocache.Code.Substring(0,2), cwp.Code.Substring(2));
                            cwp.DataFromDate = _gpxDataTime;
                            cwp.Comment = wp.SelectSingleNode("x:cmt", nsmgr).InnerText;
                            if (SafeAttributeInnerText(wp, "lat", "").Length > 0)
                            {
                                cwp.Lat = Utils.Conversion.StringToDouble(wp.Attributes["lat"].InnerText);
                                cwp.Lon = Utils.Conversion.StringToDouble(wp.Attributes["lon"].InnerText);
                                if (Math.Abs((double)cwp.Lat) < 0.0001 && Math.Abs((double)cwp.Lon) < 0.0001)
                                {
                                    cwp.Lat = null;
                                    cwp.Lon = null;
                                }
                            }
                            else
                            {
                                cwp.Lat = null;
                                cwp.Lon = null;
                            }
                            cwp.Description = wp.SelectSingleNode("x:desc", nsmgr).InnerText;
                            cwp.ID = cwp.Code;
                            cwp.Name = cwp.Code;
                            cwp.Comment = wp.SelectSingleNode("x:cmt", nsmgr).InnerText;
                            cwp.Time = DateTime.Parse(wp.SelectSingleNode("x:time", nsmgr).InnerText);
                            cwp.Url = SafeInnerText(wp.SelectSingleNode("x:url", nsmgr),"");
                            cwp.UrlName = SafeInnerText(wp.SelectSingleNode("x:urlname", nsmgr),"");
                            cwp.WPType = DataAccess.GetWaypointType(_core.WaypointTypes, wp.SelectSingleNode("x:sym", nsmgr).InnerText);

                            data.Waypoints.Add(cwp);

                            continue;
                        }

                        Framework.Data.Geocache gc = new Framework.Data.Geocache();

                        gc.Lat = Utils.Conversion.StringToDouble(SafeAttributeInnerText(wp, "lat", "0.0"));
                        gc.Lon = Utils.Conversion.StringToDouble(SafeAttributeInnerText(wp, "lon", "0.0"));
                        gc.Code = wp.SelectSingleNode("x:name", nsmgr).InnerText;
                        n = wp.SelectSingleNode("y:cache", nsmgr);
                        gc.Title = n.SelectSingleNode("y:name", nsmgr).InnerText;
                        gc.DataFromDate = _gpxDataTime;
                        gc.PublishedTime = DateTime.Parse(wp.SelectSingleNode("x:time", nsmgr).InnerText);
                        gc.Url = SafeInnerText(wp.SelectSingleNode("x:url", nsmgr), "");
                        if (SafeInnerText(wp.SelectSingleNode("x:sym", nsmgr), "").EndsWith(" Found"))
                        {
                            gc.Found = true;
                        }
                        gc.Available = bool.Parse(n.Attributes["available"].InnerText);
                        gc.Archived = bool.Parse(SafeInnerText(n.Attributes["archived"], "False"));
                        gc.Country = SafeInnerText(n.SelectSingleNode("y:country", nsmgr), "");
                        gc.State = SafeInnerText(n.SelectSingleNode("y:state", nsmgr), "");
                        gc.ID = SafeInnerText(n.Attributes["id"], "");
                        gc.OwnerId = SafeAttributeInnerText(n.SelectSingleNode("y:owner", nsmgr), "id", "");
                        if (_cachesGpxVersion >= V102)
                        {
                            gc.GeocacheType = DataAccess.GetGeocacheType(_core.GeocacheTypes, int.Parse(SafeAttributeInnerText(n.SelectSingleNode("y:type", nsmgr), "id", "-1")));
                            gc.Container = DataAccess.GetGeocacheContainer(_core.GeocacheContainers, int.Parse(SafeAttributeInnerText(n.SelectSingleNode("y:container", nsmgr), "id", "-1")));
                            gc.Favorites = int.Parse(SafeInnerText(n.SelectSingleNode("y:favorite_points", nsmgr), "0"));
                            gc.MemberOnly = bool.Parse(SafeInnerText(n.Attributes["memberonly"], "False"));
                            gc.CustomCoords = bool.Parse(SafeInnerText(n.Attributes["customcoords"], "False"));
                            gc.PersonaleNote = SafeInnerText(n.Attributes["personal_note"], "");
                        }
                        else
                        {
                            string srchTxt = SafeInnerText(n.SelectSingleNode("y:type", nsmgr), "Unknown");
                            if (!srchTxt.StartsWith("Groundspeak"))
                            {
                                if (srchTxt.Contains("Trash"))
                                {
                                    srchTxt = "Trash";
                                }
                                else
                                {
                                    int pos = srchTxt.IndexOf(' ');
                                    if (pos > 0)
                                    {
                                        srchTxt = srchTxt.Substring(0, pos);
                                    }
                                }
                            }
                            gc.GeocacheType = DataAccess.GetGeocacheType(_core.GeocacheTypes, srchTxt);
                            gc.Container = DataAccess.GetGeocacheContainer(_core.GeocacheContainers, SafeInnerText(n.SelectSingleNode("y:container", nsmgr), "Unknown"));
                        }
                        gc.PlacedBy = SafeInnerText(n.SelectSingleNode("y:placed_by", nsmgr),"");
                        gc.Owner = SafeInnerText(n.SelectSingleNode("y:owner", nsmgr),"");
                        gc.Terrain = Conversion.StringToDouble(SafeInnerText(n.SelectSingleNode("y:terrain", nsmgr), "1"));
                        gc.Difficulty = Conversion.StringToDouble(SafeInnerText(n.SelectSingleNode("y:difficulty", nsmgr), "1"));
                        gc.ShortDescription = SafeInnerText(n.SelectSingleNode("y:short_description", nsmgr), "");
                        gc.ShortDescriptionInHtml = bool.Parse(SafeAttributeInnerText(n.SelectSingleNode("y:short_description", nsmgr), "html", "False"));
                        gc.LongDescription = SafeInnerText(n.SelectSingleNode("y:long_description", nsmgr), "");
                        gc.LongDescriptionInHtml = bool.Parse(SafeAttributeInnerText(n.SelectSingleNode("y:long_description", nsmgr), "html", "False"));
                        gc.EncodedHints = SafeInnerText(n.SelectSingleNode("y:encoded_hints", nsmgr), "");

                        XmlNode attrs = n.SelectSingleNode("y:attributes", nsmgr);
                        if (attrs != null && attrs.ChildNodes!=null)
                        {
                            foreach (XmlNode attr in attrs.ChildNodes)
                            {
                                int attrId = int.Parse(attr.Attributes["id"].InnerText);
                                int attrInc = int.Parse(SafeAttributeInnerText(attr, "inc", "1"));
                                if (attrInc == 1)
                                {
                                    gc.AttributeIds.Add(attrId);
                                }
                                else
                                {
                                    gc.AttributeIds.Add(-1*attrId);
                                }
                            }
                        }

                        Calculus.SetDistanceAndAngleGeocacheFromLocation(gc, _core.CenterLocation);

                        data.Geocaches.Add(gc);

                        //Logs
                        XmlNode ln = n.SelectSingleNode("y:logs", nsmgr);
                        if (ln != null)
                        {
                            XmlNodeList logs = ln.SelectNodes("y:log", nsmgr);
                            if (logs != null)
                            {
                                foreach (XmlNode l in logs)
                                {
                                    Framework.Data.Log lg = new Framework.Data.Log();

                                    lg.GeocacheCode = gc.Code;
                                    string lid = SafeAttributeInnerText(l, "id", "");

                                    if (lid.StartsWith("GL"))
                                    {
                                        lg.ID = lid;
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(lid) || lid.StartsWith("-"))
                                        {
                                            continue;
                                        }
                                        try
                                        {
                                            lg.ID = string.Concat("GL", Utils.Conversion.GetCacheCodeFromCacheID(int.Parse(lid)).Substring(2));
                                        }
                                        catch
                                        {
                                            continue;
                                        }
                                    }

                                    lg.DataFromDate = _gpxDataTime;
                                    lg.Date = DateTime.Parse(l.SelectSingleNode("y:date", nsmgr).InnerText);
                                    lg.Encoded = bool.Parse(l.SelectSingleNode("y:text", nsmgr).Attributes["encoded"].InnerText);
                                    lg.Text = l.SelectSingleNode("y:text", nsmgr).InnerText;
                                    lg.Finder = l.SelectSingleNode("y:finder", nsmgr).InnerText;
                                    if (l.SelectSingleNode("y:finder", nsmgr).Attributes["id"] != null)
                                    {
                                        lg.FinderId = l.SelectSingleNode("y:finder", nsmgr).Attributes["id"].InnerText;
                                    }
                                    else
                                    {
                                        //GCTour has no finder id
                                        lg.FinderId = "1";
                                    }
                                    if (_cachesGpxVersion >= V102)
                                    {
                                        lg.LogType = DataAccess.GetLogType(_core.LogTypes, int.Parse(l.SelectSingleNode("y:type", nsmgr).Attributes["id"].InnerText));
                                    }
                                    else
                                    {
                                        lg.LogType = DataAccess.GetLogType(_core.LogTypes, l.SelectSingleNode("y:type", nsmgr).InnerText);
                                    }

                                    data.Logs.Add(lg);

                                    //log images
                                    XmlNode lni = l.SelectSingleNode("y:images", nsmgr);
                                    if (lni != null)
                                    {
                                        XmlNodeList logis = lni.SelectNodes("y:image", nsmgr);
                                        if (logis != null)
                                        {
                                            foreach (XmlNode li in logis)
                                            {
                                                Framework.Data.LogImage lgi = new Framework.Data.LogImage();

                                                lgi.Url = li.SelectSingleNode("y:url", nsmgr).InnerText;
                                                lgi.ID = lgi.Url;
                                                lgi.LogID = lg.ID;
                                                lgi.Name = li.SelectSingleNode("y:name", nsmgr).InnerText;
                                                lgi.DataFromDate = _gpxDataTime;

                                                data.LogImages.Add(lgi);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }

        }
    }
}
