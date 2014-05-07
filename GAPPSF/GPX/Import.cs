using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GAPPSF.GPX
{
    public class Import
    {
        private DateTime _gpxDataTime = DateTime.Now;
        private Version _cachesGpxVersion = null;

        public async Task ImportFileAsync(string fileName)
        {
            using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
            {
                await Task.Run(new Action(() => ImportFile(fileName)));
            }
        }

        public async Task ImportFilesAsync(string[] fileNames)
        {
            using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
            {
                await Task.Run(new Action(() => ImportFiles(fileNames)));
            }
        }

        public void ImportFile(string fileName)
        {
            ImportFile(fileName, null);
        }
        public void ImportFile(string fileName, bool? isZip)
        {
            try
            {
                if (isZip==true || fileName.ToLower().EndsWith(".zip"))
                {
                    byte[] data = new byte[1024];
                    using (var fs = System.IO.File.OpenRead(fileName))
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

                                if (theEntry.Name.ToLower().EndsWith(".gpx"))
                                {
                                    ImportGPX(sb.ToString());
                                }
                            }

                            try
                            {
                                theEntry = s.GetNextEntry();
                            }
                            catch//(Exception e)
                            {
                                //Core.ApplicationData.Instance.Logger.AddLog(this, e);
                                //opencaching.de zip files generates an exception
                                theEntry = null;
                            }
                        }
                    }
                }
                else if (fileName.ToLower().EndsWith("gpx"))
                {
                    ImportGPX(File.ReadAllText(fileName));
                }
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
        }

        public void ImportFiles(string[] fileNames)
        {
            foreach(string f in fileNames)
            {
                ImportFile(f);
            }
        }

        public void ImportGPX(string gpxDoc)
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
                    _cachesGpxVersion = new Version(1, 0, 2);
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
                            //assume Child waypoint and yeah, well....multiple sources like GC or OB should be handled
                            //TODO: check source and don't assume GC
                            //check if it is a child waypoint by checking the name
                            string fullwpname = wp.SelectSingleNode("x:name", nsmgr).InnerText;
                            string wpname = fullwpname.Substring(2);

                            Core.Data.Geocache parentGeocache = null;
                            string parentCode;
                            parentGeocache = (from g in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where g.Code.Substring(2) == wpname select g).FirstOrDefault();
                            if (parentGeocache == null)
                            {
                                //parent is not available
                                //now what?
                                //just add it
                                //continue;
                                parentCode = string.Concat("GC", wpname);
                            }
                            else
                            {
                                parentCode = parentGeocache.Code;
                            }

                            Core.Data.IWaypointData cwp = null;
                            cwp = (from a in Utils.DataAccess.GetWaypointsFromGeocache(Core.ApplicationData.Instance.ActiveDatabase, parentCode) where a.Code == fullwpname select a).FirstOrDefault();
                            if (cwp == null)
                            {
                                cwp = new Core.Data.WaypointData();
                            }
                            else
                            {
                                if (cwp.DataFromDate >= _gpxDataTime)
                                {
                                    continue;
                                }
                            }

                            cwp.Code = wp.SelectSingleNode("x:name", nsmgr).InnerText;
                            cwp.GeocacheCode = string.Concat(parentCode.Substring(0, 2), cwp.Code.Substring(2));
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
                            cwp.Url = SafeInnerText(wp.SelectSingleNode("x:url", nsmgr), "");
                            cwp.UrlName = SafeInnerText(wp.SelectSingleNode("x:urlname", nsmgr), "");
                            cwp.WPType = Utils.DataAccess.GetWaypointType(wp.SelectSingleNode("x:sym", nsmgr).InnerText);

                            if (cwp is Core.Data.WaypointData)
                            {
                                Utils.DataAccess.AddWaypoint(Core.ApplicationData.Instance.ActiveDatabase, cwp as Core.Data.WaypointData);
                            }
                            continue;
                        }

                        string code = wp.SelectSingleNode("x:name", nsmgr).InnerText;

                        Core.Data.IGeocacheData gc = null;
                        gc = (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Code == code select a).FirstOrDefault();
                        if (gc == null)
                        {
                            gc = new Core.Data.GeocacheData();
                        }
                        else
                        {
                            if (gc.DataFromDate >= _gpxDataTime)
                            {
                                continue;
                            }
                        }

                        gc.Available = bool.Parse(n.Attributes["available"].InnerText);
                        gc.Archived = bool.Parse(SafeInnerText(n.Attributes["archived"], "False"));

                        if (!(gc is Core.Data.Geocache) || !(gc as Core.Data.Geocache).Locked)
                        {
                            gc.Lat = Utils.Conversion.StringToDouble(SafeAttributeInnerText(wp, "lat", "0.0"));
                            gc.Lon = Utils.Conversion.StringToDouble(SafeAttributeInnerText(wp, "lon", "0.0"));
                            gc.Code = code;
                            n = wp.SelectSingleNode("y:cache", nsmgr);
                            gc.DataFromDate = _gpxDataTime;
                            gc.Name = n.SelectSingleNode("y:name", nsmgr).InnerText;
                            gc.PublishedTime = DateTime.Parse(wp.SelectSingleNode("x:time", nsmgr).InnerText);
                            gc.Url = SafeInnerText(wp.SelectSingleNode("x:url", nsmgr), "");
                            if (SafeInnerText(wp.SelectSingleNode("x:sym", nsmgr), "").EndsWith(" Found"))
                            {
                                gc.Found = true;
                            }
                            gc.Country = SafeInnerText(n.SelectSingleNode("y:country", nsmgr), "");
                            gc.State = SafeInnerText(n.SelectSingleNode("y:state", nsmgr), "");
                            gc.OwnerId = SafeAttributeInnerText(n.SelectSingleNode("y:owner", nsmgr), "id", "");
                            if (_cachesGpxVersion >= V102)
                            {
                                gc.GeocacheType = Utils.DataAccess.GetGeocacheType(int.Parse(SafeAttributeInnerText(n.SelectSingleNode("y:type", nsmgr), "id", "-1")));
                                gc.Container = Utils.DataAccess.GetGeocacheContainer(int.Parse(SafeAttributeInnerText(n.SelectSingleNode("y:container", nsmgr), "id", "-1")));
                                gc.Favorites = int.Parse(SafeInnerText(n.SelectSingleNode("y:favorite_points", nsmgr), "0"));
                                gc.MemberOnly = bool.Parse(SafeInnerText(n.Attributes["memberonly"], "False"));
                                //gc.CustomCoords = bool.Parse(SafeInnerText(n.Attributes["customcoords"], "False"));
                                gc.PersonalNote = SafeInnerText(n.Attributes["personal_note"], "");
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
                                gc.GeocacheType = Utils.DataAccess.GetGeocacheType(srchTxt);
                                gc.Container = Utils.DataAccess.GetGeocacheContainer(SafeInnerText(n.SelectSingleNode("y:container", nsmgr), "Unknown"));
                            }
                            gc.PlacedBy = SafeInnerText(n.SelectSingleNode("y:placed_by", nsmgr), "");
                            gc.Owner = SafeInnerText(n.SelectSingleNode("y:owner", nsmgr), "");
                            gc.Terrain = Utils.Conversion.StringToDouble(SafeInnerText(n.SelectSingleNode("y:terrain", nsmgr), "1"));
                            gc.Difficulty = Utils.Conversion.StringToDouble(SafeInnerText(n.SelectSingleNode("y:difficulty", nsmgr), "1"));
                            gc.ShortDescription = SafeInnerText(n.SelectSingleNode("y:short_description", nsmgr), "");
                            gc.ShortDescriptionInHtml = bool.Parse(SafeAttributeInnerText(n.SelectSingleNode("y:short_description", nsmgr), "html", "False"));
                            gc.LongDescription = SafeInnerText(n.SelectSingleNode("y:long_description", nsmgr), "");
                            gc.LongDescriptionInHtml = bool.Parse(SafeAttributeInnerText(n.SelectSingleNode("y:long_description", nsmgr), "html", "False"));
                            gc.EncodedHints = SafeInnerText(n.SelectSingleNode("y:encoded_hints", nsmgr), "");

                            gc.AttributeIds = new List<int>();
                            XmlNode attrs = n.SelectSingleNode("y:attributes", nsmgr);
                            if (attrs != null && attrs.ChildNodes != null)
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
                                        gc.AttributeIds.Add(-1 * attrId);
                                    }
                                }
                            }

                            bool gcAdded = true;
                            if (gc is Core.Data.GeocacheData)
                            {
                                gcAdded = Utils.DataAccess.AddGeocache(Core.ApplicationData.Instance.ActiveDatabase, gc as Core.Data.GeocacheData);
                            }
                            if (gcAdded)
                            {
                                gc = (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Code == code select a).FirstOrDefault();
                                Utils.Calculus.SetDistanceAndAngleGeocacheFromLocation(gc as Core.Data.Geocache, Core.ApplicationData.Instance.CenterLocation);


                                //Logs
                                XmlNode ln = n.SelectSingleNode("y:logs", nsmgr);
                                if (ln != null)
                                {
                                    XmlNodeList logs = ln.SelectNodes("y:log", nsmgr);
                                    if (logs != null)
                                    {
                                        foreach (XmlNode l in logs)
                                        {

                                            string lid = SafeAttributeInnerText(l, "id", "");

                                            if (lid.StartsWith("GL"))
                                            {
                                                //lg.ID = lid;
                                            }
                                            else
                                            {
                                                if (string.IsNullOrEmpty(lid) || lid.StartsWith("-"))
                                                {
                                                    continue;
                                                }
                                                try
                                                {
                                                    lid = string.Concat("GL", Utils.Conversion.GetCacheCodeFromCacheID(int.Parse(lid)).Substring(2));
                                                }
                                                catch
                                                {
                                                    continue;
                                                }
                                            }

                                            Core.Data.ILogData lg = null;
                                            lg = (from a in Utils.DataAccess.GetLogs(Core.ApplicationData.Instance.ActiveDatabase, gc.Code) where a.ID == lid select a).FirstOrDefault();
                                            if (lg == null)
                                            {
                                                lg = new Core.Data.LogData();
                                            }

                                            lg.ID = lid;
                                            lg.GeocacheCode = gc.Code;
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
                                                lg.LogType = Utils.DataAccess.GetLogType(int.Parse(l.SelectSingleNode("y:type", nsmgr).Attributes["id"].InnerText));
                                            }
                                            else
                                            {
                                                lg.LogType = Utils.DataAccess.GetLogType(l.SelectSingleNode("y:type", nsmgr).InnerText);
                                            }

                                            if (lg is Core.Data.LogData)
                                            {
                                                Utils.DataAccess.AddLog(Core.ApplicationData.Instance.ActiveDatabase, lg as Core.Data.LogData);
                                            }

                                            //log images
                                            XmlNode lni = l.SelectSingleNode("y:images", nsmgr);
                                            if (lni != null)
                                            {
                                                XmlNodeList logis = lni.SelectNodes("y:image", nsmgr);
                                                if (logis != null)
                                                {
                                                    foreach (XmlNode li in logis)
                                                    {
                                                        string url = li.SelectSingleNode("y:url", nsmgr).InnerText;

                                                        Core.Data.ILogImageData lgi = null;
                                                        lgi = (from a in Utils.DataAccess.GetLogImages(Core.ApplicationData.Instance.ActiveDatabase, lg.ID) where a.ID == url select a).FirstOrDefault();
                                                        if (lgi == null)
                                                        {
                                                            lgi = new Core.Data.LogImageData();
                                                        }
                                                        lgi.Url = url;
                                                        lgi.ID = lgi.Url;
                                                        lgi.LogId = lg.ID;
                                                        lgi.Name = li.SelectSingleNode("y:name", nsmgr).InnerText;
                                                        lgi.DataFromDate = _gpxDataTime;

                                                        if (lgi is Core.Data.LogImageData)
                                                        {
                                                            Utils.DataAccess.AddLogImage(Core.ApplicationData.Instance.ActiveDatabase, lgi as Core.Data.LogImageData);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
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

    }
}
