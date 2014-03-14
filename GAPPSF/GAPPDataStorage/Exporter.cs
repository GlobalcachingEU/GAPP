using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GAPPSF.GAPPDataStorage
{
    public class Exporter
    {
        public const string EXT_GEOCACHES = ".cch";
        public const string EXT_LOGS = ".lgs";
        public const string EXT_WAYPPOINTS = ".wpt";
        public const string EXT_USERWAYPOINTS = ".uwp";
        public const string EXT_LOGIMAGES = ".lmg";
        public const string EXT_GEOCACHEIMAGES = ".gmg";
        public const string EXT_DATABASEINFO = ".gpp";

        public async Task<bool> ExportAsync(string filename, Core.Storage.Database db, List<Core.Data.Geocache> gcList)
        {
            bool result = false;
            await Task.Run(() => { result = Export(filename, db, gcList); });
            return result;
        }

        private bool Export(string filename, Core.Storage.Database db, List<Core.Data.Geocache> gcList)
        {
            bool result = false;
            try
            {
                using (FileCollection fileCollection = new FileCollection(filename))
                {
                    result = Save(fileCollection, db, gcList);
                }
            }
            catch (Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
            return result;
        }

        public class FileCollection : IDisposable
        {
            public string BaseFilename { get; private set; }
            public FileStream _fsGeocaches { get; private set; }
            public FileStream _fsLogs { get; private set; }
            public string DatabaseInfoFilename { get { return getFilename(BaseFilename, EXT_DATABASEINFO); } }
            public string WaypointsFilename { get { return getFilename(BaseFilename, EXT_WAYPPOINTS); } }
            public string UserWaypointsFilename { get { return getFilename(BaseFilename, EXT_USERWAYPOINTS); } }
            public string LogImagesFilename { get { return getFilename(BaseFilename, EXT_LOGIMAGES); } }
            public string GeocacheImagesFilename { get { return getFilename(BaseFilename, EXT_GEOCACHEIMAGES); } }

            public FileCollection(string baseFilename)
            {
                BaseFilename = baseFilename;
                _fsGeocaches = File.Open(getFilename(baseFilename, EXT_GEOCACHES), FileMode.Create, FileAccess.Write);
                _fsLogs = File.Open(getFilename(baseFilename, EXT_LOGS), FileMode.Create, FileAccess.Write);
            }

            public static string getFilename(string targetFile, string extension)
            {
                return Path.Combine(Path.GetDirectoryName(targetFile), string.Format("{0}{1}", Path.GetFileNameWithoutExtension(targetFile), extension));
            }

            public void Dispose()
            {
                if (_fsGeocaches != null)
                {
                    _fsGeocaches.Dispose();
                    _fsGeocaches = null;
                }
                if (_fsLogs != null)
                {
                    _fsLogs.Dispose();
                    _fsLogs = null;
                }
            }
        }


        private bool Save(FileCollection fc, Core.Storage.Database db, List<Core.Data.Geocache> gclist)
        {
            bool result = true;
            using (Utils.ProgressBlock fixpr = new Utils.ProgressBlock("Saving", "SavingData", 1, 0))
            {
                byte[] memBuffer = new byte[10 * 1024 * 1024];
                byte isFree = 0;
                byte notFree = 1;
                byte notFreeF = 2;
                using (MemoryStream ms = new MemoryStream(memBuffer))
                using (BinaryWriter bw = new BinaryWriter(ms))
                {

                    //**********************************************
                    //fc.DatabaseInfoFilename
                    //**********************************************
                    XmlDocument doc = new XmlDocument();
                    XmlElement root = doc.CreateElement("info");
                    doc.AppendChild(root);

                    XmlElement el = doc.CreateElement("IsLittleEndian");
                    XmlText txt = doc.CreateTextNode(BitConverter.IsLittleEndian.ToString());
                    el.AppendChild(txt);
                    root.AppendChild(el);

                    el = doc.CreateElement("GAPPVersion");
                    txt = doc.CreateTextNode("1.9.19.0");
                    el.AppendChild(txt);
                    root.AppendChild(el);

                    el = doc.CreateElement("StorageVersion");
                    txt = doc.CreateTextNode("1");
                    el.AppendChild(txt);
                    root.AppendChild(el);

                    el = doc.CreateElement("GeocacheCount");
                    txt = doc.CreateTextNode(gclist.Count.ToString());
                    el.AppendChild(txt);
                    root.AppendChild(el);

                    List<Core.Data.Log> logs = new List<Core.Data.Log>();
                    foreach(var g in gclist)
                    {
                        logs.AddRange(Utils.DataAccess.GetLogs(db, g.Code));
                    }
                    el = doc.CreateElement("LogCount");
                    txt = doc.CreateTextNode(logs.Count.ToString());
                    el.AppendChild(txt);
                    root.AppendChild(el);

                    List<Core.Data.LogImage> logImages = new List<Core.Data.LogImage>();
                    foreach (var g in logs)
                    {
                        logImages.AddRange(Utils.DataAccess.GetLogImages(db, g.ID));
                    }
                    el = doc.CreateElement("LogImagesCount");
                    txt = doc.CreateTextNode(logImages.Count.ToString());
                    el.AppendChild(txt);
                    root.AppendChild(el);

                    List<Core.Data.Waypoint> waypoints = new List<Core.Data.Waypoint>();
                    foreach (var g in gclist)
                    {
                        waypoints.AddRange(Utils.DataAccess.GetWaypointsFromGeocache(db, g.Code));
                    }
                    el = doc.CreateElement("WaypointCount");
                    txt = doc.CreateTextNode(waypoints.Count.ToString());
                    el.AppendChild(txt);
                    root.AppendChild(el);

                    List<Core.Data.UserWaypoint> userWaypoints = new List<Core.Data.UserWaypoint>();
                    foreach (var g in gclist)
                    {
                        userWaypoints.AddRange(Utils.DataAccess.GetUserWaypointsFromGeocache(db, g.Code));
                    }
                    el = doc.CreateElement("UserWaypointCount");
                    txt = doc.CreateTextNode(userWaypoints.Count.ToString());
                    el.AppendChild(txt);
                    root.AppendChild(el);

                    List<Core.Data.GeocacheImage> geocacheImages = new List<Core.Data.GeocacheImage>();
                    foreach (var g in gclist)
                    {
                        geocacheImages.AddRange(Utils.DataAccess.GetGeocacheImages(db, g.Code));
                    }
                    el = doc.CreateElement("GeocacheImagesCount");
                    txt = doc.CreateTextNode(geocacheImages.Count.ToString());
                    el.AppendChild(txt);
                    root.AppendChild(el);

                    doc.Save(fc.DatabaseInfoFilename);
                    //**********************************************
                    //**********************************************

                    //**********************************************
                    //          GEOCACHES
                    //**********************************************

                    //now get all the selected and data changed geocaches
                    if (gclist.Count > 0)
                    {
                        using (Utils.ProgressBlock progress = new Utils.ProgressBlock("SavingGeocaches", gclist.Count, 0))
                        {
                            //fix block > ID = GC12345
                            //fulldata > ID = F_GC12345

                            long recordLength = 0;
                            byte[] extraBuffer = new byte[200];

                            int index = 0;
                            int procStep = 0;
                            foreach (var gc in gclist)
                            {
                                //write to block
                                ms.Position = 0;

                                //block header
                                bw.Write(recordLength); //overwrite afterwards
                                bw.Write(notFree);
                                bw.Write(gc.Code);

                                bw.Write(gc.Archived);
                                WriteIntegerArray(bw, gc.AttributeIds);
                                bw.Write(gc.Available);
                                bw.Write(gc.City ?? "");
                                bw.Write(gc.Container.ID);
                                bw.Write(false);
                                bw.Write(gc.Country ?? "");
                                bw.Write(gc.ContainsCustomLatLon);
                                if (gc.ContainsCustomLatLon)
                                {
                                    bw.Write((double)gc.CustomLat);
                                    bw.Write((double)gc.CustomLon);
                                }
                                bw.Write(gc.Difficulty);
                                bw.Write(gc.EncodedHints ?? "");
                                bw.Write(gc.Favorites);
                                bw.Write(gc.Flagged);
                                bw.Write(gc.Found);
                                bw.Write(gc.GeocacheType.ID);
                                bw.Write(Utils.Conversion.GetCacheIDFromCacheCode(gc.Code).ToString());
                                bw.Write(gc.Lat);
                                bw.Write(gc.Lon);
                                bw.Write(gc.MemberOnly);
                                bw.Write(gc.Municipality ?? "");
                                bw.Write(gc.Name ?? "");
                                bw.Write(gc.Notes ?? "");
                                bw.Write(gc.Owner ?? "");
                                bw.Write(gc.OwnerId ?? "");
                                bw.Write(gc.PersonalNote ?? "");
                                bw.Write(gc.PlacedBy ?? "");
                                bw.Write(((DateTime)gc.PublishedTime).ToString("s"));
                                bw.Write(gc.State ?? "");
                                bw.Write(gc.Terrain);
                                bw.Write(gc.Name ?? "");
                                bw.Write(gc.Url ?? "");
                                bw.Write(gc.DataFromDate.ToString("s"));
                                bw.Write(gc.Locked);

                                writeRecord(gc.Code, ms, bw, fc._fsGeocaches, memBuffer, extraBuffer);

                                //other record
                                string id = string.Concat("F_", gc.Code);
                                //write to block
                                ms.Position = 0;

                                //block header
                                bw.Write(recordLength); //overwrite afterwards
                                bw.Write(notFreeF);
                                bw.Write(id);

                                bw.Write(gc.ShortDescription ?? "");
                                bw.Write(gc.ShortDescriptionInHtml);
                                bw.Write(gc.LongDescription ?? "");
                                bw.Write(gc.LongDescriptionInHtml);

                                writeRecord(id, ms, bw, fc._fsGeocaches, memBuffer, extraBuffer);

                                index++;
                                procStep++;
                                if (procStep >= 1000)
                                {
                                    progress.Update("SavingGeocaches", gclist.Count, index);
                                    procStep = 0;
                                }
                            }
                        }
                        fc._fsGeocaches.Flush();
                    }

                    //**********************************************
                    //          LOGS
                    //**********************************************
                    if (logs.Count > 0)
                    {
                        int index = 0;
                        int procStep = 0;
                        using (Utils.ProgressBlock progress = new Utils.ProgressBlock("SavingLogs", logs.Count, 0))
                        {
                            long recordLength = 0;
                            byte[] extraBuffer = new byte[50];
                            foreach (var l in logs)
                            {
                                //write to block
                                ms.Position = 0;

                                //block header
                                bw.Write(recordLength); //overwrite afterwards
                                bw.Write(notFree);
                                bw.Write(l.ID);

                                bw.Write(l.DataFromDate.ToString("s"));
                                bw.Write(l.Date.ToString("s"));
                                bw.Write(l.Finder ?? "");
                                bw.Write(l.GeocacheCode ?? "");
                                bw.Write(l.ID);
                                bw.Write(l.LogType.ID);

                                writeRecord(l.ID, ms, bw, fc._fsLogs, memBuffer, extraBuffer);

                                string id = string.Concat("F_", l.ID);
                                //write to block
                                ms.Position = 0;

                                //block header
                                bw.Write(recordLength); //overwrite afterwards
                                bw.Write(notFreeF);
                                bw.Write(id);

                                bw.Write(l.TBCode ?? "");
                                bw.Write(l.FinderId ?? "");
                                bw.Write(l.Text ?? "");
                                bw.Write(l.Encoded);

                                writeRecord(id, ms, bw, fc._fsLogs, memBuffer, extraBuffer);

                                index++;
                                procStep++;
                                if (procStep >= 1000)
                                {
                                    progress.Update("SavingLogs", logs.Count, index);
                                    procStep = 0;
                                }
                            }
                        }
                        fc._fsLogs.Flush();
                    }

                    //**********************************************
                    //          WAYPOINTS
                    //**********************************************

                    using (FileStream fs = File.Open(fc.WaypointsFilename, FileMode.Create, FileAccess.Write))
                    {
                        if (waypoints.Count > 0)
                        {
                            int index = 0;
                            int procStep = 0;
                            using (Utils.ProgressBlock progress = new Utils.ProgressBlock("Saving", "SavingWaypoints", waypoints.Count, 0))
                            {
                                long recordLength = 0;
                                byte[] extraBuffer = new byte[10];
                                foreach (var wp in waypoints)
                                {
                                    //write to block
                                    ms.Position = 0;

                                    //block header
                                    bw.Write(recordLength); //overwrite afterwards
                                    bw.Write(notFree);
                                    bw.Write(wp.Code);

                                    bw.Write(wp.Comment ?? "");
                                    bw.Write(wp.DataFromDate.ToString("s"));
                                    bw.Write(wp.Description ?? "");
                                    bw.Write(wp.GeocacheCode ?? "");
                                    bw.Write(wp.ID ?? "");
                                    if (wp.Lat == null || wp.Lon == null)
                                    {
                                        bw.Write(false);
                                    }
                                    else
                                    {
                                        bw.Write(true);
                                        bw.Write((double)wp.Lat);
                                        bw.Write((double)wp.Lon);
                                    }
                                    bw.Write(wp.Name ?? "");
                                    bw.Write(wp.Time.ToString("s"));
                                    bw.Write(wp.Url ?? "");
                                    bw.Write(wp.UrlName ?? "");
                                    bw.Write(wp.WPType.ID);

                                    writeRecord(wp.Code, ms, bw, fs, memBuffer, extraBuffer);

                                    index++;
                                    procStep++;
                                    if (procStep >= 1000)
                                    {
                                        progress.Update("SavingWaypoints", waypoints.Count, index);
                                        procStep = 0;
                                    }
                                }
                            }
                        }
                        fs.Flush();
                    }

                    //**********************************************
                    //          LOGIMAGES
                    //**********************************************

                    using (FileStream fs = File.Open(fc.LogImagesFilename, FileMode.Create, FileAccess.Write))
                    {
                        if (logImages.Count > 0)
                        {
                            int index = 0;
                            int procStep = 0;
                            using (Utils.ProgressBlock progress = new Utils.ProgressBlock("Saving", "SavingLogImages", logImages.Count, 0))
                            {
                                long recordLength = 0;
                                byte[] extraBuffer = new byte[10];
                                foreach (var li in logImages)
                                {
                                    //write to block
                                    ms.Position = 0;

                                    //block header
                                    bw.Write(recordLength); //overwrite afterwards
                                    bw.Write(notFree);
                                    bw.Write(li.ID);
                                    bw.Write(li.DataFromDate.ToString("s"));
                                    bw.Write(li.LogId ?? "");
                                    bw.Write(li.Name ?? "");
                                    bw.Write(li.Url ?? "");

                                    writeRecord(li.ID, ms, bw, fs, memBuffer, extraBuffer);

                                    index++;
                                    procStep++;
                                    if (procStep >= 1000)
                                    {
                                        progress.Update("SavingLogImages", logImages.Count, index);
                                        procStep = 0;
                                    }
                                }
                            }
                        }
                        fs.Flush();
                    }

                    //**********************************************
                    //          GEOCACHEIMAGES
                    //**********************************************

                    using (FileStream fs = File.Open(fc.GeocacheImagesFilename, FileMode.Create, FileAccess.Write))
                    {
                        if (geocacheImages.Count > 0)
                        {
                            int index = 0;
                            int procStep = 0;
                            using (Utils.ProgressBlock progress = new Utils.ProgressBlock("Saving", "SavingGeocacheImages", geocacheImages.Count, 0))
                            {
                                long recordLength = 0;
                                byte[] extraBuffer = new byte[100];
                                foreach (var li in geocacheImages)
                                {
                                    //write to block
                                    ms.Position = 0;

                                    //block header
                                    bw.Write(recordLength); //overwrite afterwards
                                    bw.Write(notFree);
                                    bw.Write(li.ID);
                                    bw.Write(li.DataFromDate.ToString("s"));
                                    bw.Write(li.GeocacheCode ?? "");
                                    bw.Write(li.Description ?? "");
                                    bw.Write(li.Name ?? "");
                                    bw.Write(li.Url ?? "");
                                    bw.Write(li.MobileUrl ?? "");
                                    bw.Write(li.ThumbUrl ?? "");

                                    writeRecord(li.ID, ms, bw, fs, memBuffer, extraBuffer);

                                    index++;
                                    procStep++;
                                    if (procStep >= 1000)
                                    {
                                        progress.Update("SavingGeocacheImages", geocacheImages.Count, index);
                                        procStep = 0;
                                    }
                                }
                            }
                        }
                        fs.Flush();
                    }

                    //**********************************************
                    //          USER WAYPOINTS
                    //**********************************************

                    using (FileStream fs = File.Open(fc.UserWaypointsFilename, FileMode.Create, FileAccess.Write))
                    {
                        //delete geocaches that are not in the list anymore.
                        if (userWaypoints.Count > 0)
                        {
                            long recordLength = 0;
                            byte[] extraBuffer = new byte[10];
                            foreach (var wp in userWaypoints)
                            {
                                //write to block
                                ms.Position = 0;

                                //block header
                                bw.Write(recordLength); //overwrite afterwards
                                bw.Write(notFree);
                                bw.Write(wp.ID.ToString());
                                bw.Write(wp.Description ?? "");
                                bw.Write(wp.GeocacheCode ?? "");
                                bw.Write(wp.Lat);
                                bw.Write(wp.Lon);
                                bw.Write(wp.Date.ToString("s"));

                                writeRecord(wp.ID.ToString(), ms, bw, fs, memBuffer, extraBuffer);

                            }

                        }
                        fs.Flush();
                    }


                }
            }
            return result;
        }

        public void WriteIntegerArray(BinaryWriter fs, List<int> values)
        {
            fs.Write(values.Count);
            foreach (int i in values)
            {
                fs.Write(i);
            }
        }

        private void writeRecord(string id, MemoryStream ms, BinaryWriter bw, FileStream fs, byte[] memBuffer, byte[] extraBuffer)
        {
            long recordLength;
            //add
            bw.Write(extraBuffer);
            recordLength = ms.Position;
            ms.Seek(0, SeekOrigin.Begin);
            bw.Write(recordLength); //overwrite afterwards

            //fs.Position = fs.Length;
            fs.Write(memBuffer, 0, (int)recordLength);
        }

    }
}
