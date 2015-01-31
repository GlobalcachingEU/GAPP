using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalcachingApplication.Utils;
using System.IO;
using System.Xml;
using System.Collections;

namespace GlobalcachingApplication.Plugins.GAPPDataStorage
{
    public partial class InternalStorage
    {
        private Hashtable _uniqueCheckList;

        protected override bool SupportsRepairActiveDatabase
        {
            get
            {
                return true;
            }
        }

        public override bool PrepareRepair()
        {
            bool result = false;
            if (_fileCollection != null)
            {
                _fileCollection.Dispose();
                _fileCollection = null;

                using (FrameworkDataUpdater upd = new FrameworkDataUpdater(Core))
                {
                    Core.Geocaches.Clear();
                    Core.Logs.Clear();
                    Core.LogImages.Clear();
                    Core.Waypoints.Clear();
                    Core.UserWaypoints.Clear();
                }

                result = true;
            }
            return result;
        }

        public override bool Repair(bool geocachesOnly)
        {
            bool result = false;

            //repair
            _fileCollection = new FileCollection(PluginSettings.Instance.ActiveDataFile);
            if (File.Exists(_fileCollection.DatabaseInfoFilename))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(_fileCollection.DatabaseInfoFilename);
                XmlElement root = doc.DocumentElement;
                int storageVersion = int.Parse(root.SelectSingleNode("StorageVersion").InnerText);

                if (storageVersion == 1)
                {
                    _uniqueCheckList = new Hashtable();
                    result = RepairV1();
                }
            }

            //load geocaches
            if (result)
            {
                result = Open(geocachesOnly);
            }
            return result;
        }

        public bool checkUniqueID(RecordInfo ri, FileStream fs)
        {
            bool result = true;
            if (!ri.FreeSlot)
            {
                try
                {
                    _uniqueCheckList.Add(ri.ID, ri.ID);
                }
                catch
                {
                    //patch free
                    fs.Position = ri.Offset + sizeof(long);
                    fs.WriteByte(0);
                    fs.Position = ri.Offset + ri.Length;
                    result = false;
                }
            }
            return result;
        }

        public bool RepairV1()
        {
            bool result = true;

            if (File.Exists(_fileCollection.DatabaseInfoFilename))
            {
                int lsize = sizeof(long);
                byte[] memBuffer = new byte[10 * 1024 * 1024];
                using (MemoryStream ms = new MemoryStream(memBuffer))
                using (BinaryReader br = new BinaryReader(ms))
                {

                    int gcCount = 0;
                    int logCount = 0;
                    int logimgCount = 0;
                    int wptCount = 0;
                    int usrwptCount = 0;

                    XmlDocument doc = new XmlDocument();
                    doc.Load(_fileCollection.DatabaseInfoFilename);
                    XmlElement root = doc.DocumentElement;
                    gcCount = int.Parse(root.SelectSingleNode("GeocacheCount").InnerText);
                    logCount = int.Parse(root.SelectSingleNode("LogCount").InnerText);
                    logimgCount = int.Parse(root.SelectSingleNode("LogImagesCount").InnerText);
                    wptCount = int.Parse(root.SelectSingleNode("WaypointCount").InnerText);
                    usrwptCount = int.Parse(root.SelectSingleNode("UserWaypointCount").InnerText);

                    DateTime nextUpdateTime = DateTime.MinValue;
                    using (Utils.ProgressBlock fixscr = new Utils.ProgressBlock(this, STR_LOADING, STR_LOADINGDATA, 1, 0))
                    {
                        using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_LOADING, STR_LOADINGGEOCACHES, gcCount, 0))
                        {
                            int index = 0;

                            FileStream fs = _fileCollection._fsGeocaches;
                            fs.Position = 0;
                            long eof = fs.Length;
                            long lastOKPosition = 0;
                            RecordInfo ri = new RecordInfo();
                            try
                            {
                                while (fs.Position < eof)
                                {
                                    lastOKPosition = fs.Position;
                                    ri.Offset = fs.Position;
                                    fs.Read(memBuffer, 0, lsize + 1);
                                    ms.Position = 0;
                                    ri.Length = br.ReadInt64();
                                    if (memBuffer[lsize] == 0)
                                    {
                                        //free
                                        ri.FreeSlot = true;
                                        ri.ID = string.Concat("_", ri.Offset.ToString());
                                        fs.Position = ri.Offset + ri.Length;
                                    }
                                    else if (memBuffer[lsize] == 2)
                                    {
                                        ri.FreeSlot = false;
                                        fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                        ms.Position = 0;
                                        ri.ID = br.ReadString();
                                        br.ReadString();
                                        br.ReadBoolean();
                                        br.ReadString();
                                        br.ReadBoolean();
                                    }
                                    else
                                    {
                                        //read
                                        ri.FreeSlot = false;
                                        Framework.Data.Geocache gc = new Framework.Data.Geocache();

                                        fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                        ms.Position = 0;
                                        gc.Code = br.ReadString();
                                        ri.ID = gc.Code;

                                        gc.Archived = br.ReadBoolean();
                                        gc.AttributeIds = ReadIntegerArray(br);
                                        gc.Available = br.ReadBoolean();
                                        gc.City = br.ReadString();
                                        gc.Container = Utils.DataAccess.GetGeocacheContainer(Core.GeocacheContainers, br.ReadInt32());
                                        gc.CustomCoords = br.ReadBoolean();
                                        gc.Country = br.ReadString();
                                        if (br.ReadBoolean())
                                        {
                                            gc.CustomLat = br.ReadDouble();
                                            gc.CustomLon = br.ReadDouble();
                                        }
                                        gc.Difficulty = br.ReadDouble();
                                        gc.EncodedHints = br.ReadString();
                                        gc.Favorites = br.ReadInt32();
                                        gc.Flagged = br.ReadBoolean();
                                        gc.Found = br.ReadBoolean();
                                        gc.GeocacheType = Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, br.ReadInt32());
                                        gc.ID = br.ReadString();
                                        gc.Lat = br.ReadDouble();
                                        gc.Lon = br.ReadDouble();
                                        gc.MemberOnly = br.ReadBoolean();
                                        gc.Municipality = br.ReadString();
                                        gc.Name = br.ReadString();
                                        gc.Notes = br.ReadString();
                                        gc.Owner = br.ReadString();
                                        gc.OwnerId = br.ReadString();
                                        gc.PersonaleNote = br.ReadString();
                                        gc.PlacedBy = br.ReadString();
                                        gc.PublishedTime = DateTime.Parse(br.ReadString());
                                        gc.State = br.ReadString();
                                        gc.Terrain = br.ReadDouble();
                                        gc.Title = br.ReadString();
                                        gc.Url = br.ReadString();
                                        gc.DataFromDate = DateTime.Parse(br.ReadString());
                                        gc.Locked = br.ReadBoolean();

                                        gc.Saved = true;
                                        gc.IsDataChanged = false;

                                        index++;
                                        if (DateTime.Now >= nextUpdateTime)
                                        {
                                            progress.UpdateProgress(STR_LOADING, STR_LOADINGGEOCACHES, gcCount, index);
                                            nextUpdateTime = DateTime.Now.AddSeconds(1);
                                        }
                                    }

                                    checkUniqueID(ri, fs);
                                }
                            }
                            catch
                            {
                                //error in file after lastOKPosition
                                fs.SetLength(lastOKPosition);
                            }
                        }

                        _uniqueCheckList.Clear();
                        using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_LOADING, STR_LOADINGLOGS, logCount, 0))
                        {
                            int index = 0;
                            nextUpdateTime = DateTime.MinValue;

                            FileStream fs = _fileCollection._fsLogs;
                            fs.Position = 0;
                            long lastOKPosition = 0;
                            long eof = fs.Length;
                            RecordInfo ri = new RecordInfo();
                            try
                            {
                                while (fs.Position < eof)
                                {
                                    lastOKPosition = fs.Position;
                                    ri.Offset = fs.Position;
                                    fs.Read(memBuffer, 0, lsize + 1);
                                    ms.Position = 0;
                                    ri.Length = br.ReadInt64();
                                    if (memBuffer[lsize] == 0)
                                    {
                                        //free
                                        ri.FreeSlot = true;
                                        ri.ID = string.Concat("_", ri.Offset.ToString());
                                        fs.Position = ri.Offset + ri.Length;
                                    }
                                    else if (memBuffer[lsize] == 2)
                                    {
                                        ri.FreeSlot = false;
                                        fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                        ms.Position = 0;
                                        ri.ID = br.ReadString();
                                        br.ReadString();
                                        br.ReadString();
                                        br.ReadString();
                                        br.ReadBoolean();
                                    }
                                    else
                                    {
                                        //read
                                        ri.FreeSlot = false;
                                        Framework.Data.Log log = new Framework.Data.Log();

                                        fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                        ms.Position = 0;
                                        log.ID = br.ReadString();
                                        ri.ID = log.ID;

                                        log.DataFromDate = DateTime.Parse(br.ReadString());
                                        log.Date = DateTime.Parse(br.ReadString());
                                        log.Finder = br.ReadString();
                                        log.GeocacheCode = br.ReadString();
                                        log.ID = br.ReadString();
                                        log.LogType = Utils.DataAccess.GetLogType(Core.LogTypes, br.ReadInt32());

                                        log.Saved = true;
                                        log.IsDataChanged = false;

                                        index++;
                                        if (DateTime.Now >= nextUpdateTime)
                                        {
                                            progress.UpdateProgress(STR_LOADING, STR_LOADINGLOGS, logCount, index);
                                            nextUpdateTime = DateTime.Now.AddSeconds(1);
                                        }
                                    }
                                    checkUniqueID(ri, fs);
                                }
                            }
                            catch
                            {
                                //error in file after lastOKPosition
                                fs.SetLength(lastOKPosition);
                            }
                        }

                        _uniqueCheckList.Clear();
                        using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_LOADING, STR_LOADINGWAYPOINTS, wptCount, 0))
                        {
                            int index = 0;
                            int procStep = 0;

                            using (FileStream fs = File.Open(_fileCollection.WaypointsFilename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                            {
                                fs.Position = 0;
                                long eof = fs.Length;
                                long lastOKPosition = 0;
                                RecordInfo ri = new RecordInfo();
                                try
                                {
                                    while (fs.Position < eof)
                                    {
                                        lastOKPosition = fs.Position;
                                        ri.Offset = fs.Position;
                                        fs.Read(memBuffer, 0, lsize + 1);
                                        ms.Position = 0;
                                        ri.Length = br.ReadInt64();
                                        if (memBuffer[lsize] == 0)
                                        {
                                            //free
                                            ri.FreeSlot = true;
                                            ri.ID = string.Concat("_", ri.Offset.ToString());
                                            fs.Position = ri.Offset + ri.Length;
                                        }
                                        else
                                        {
                                            //read
                                            ri.FreeSlot = false;
                                            Framework.Data.Waypoint wp = new Framework.Data.Waypoint();

                                            fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                            ms.Position = 0;
                                            wp.Code = br.ReadString();
                                            ri.ID = wp.Code;

                                            wp.Comment = br.ReadString();
                                            wp.DataFromDate = DateTime.Parse(br.ReadString());
                                            wp.Description = br.ReadString();
                                            wp.GeocacheCode = br.ReadString();
                                            wp.ID = br.ReadString();
                                            if (br.ReadBoolean())
                                            {
                                                wp.Lat = br.ReadDouble();
                                                wp.Lon = br.ReadDouble();
                                            }
                                            wp.Name = br.ReadString();
                                            wp.Time = DateTime.Parse(br.ReadString());
                                            wp.Url = br.ReadString();
                                            wp.UrlName = br.ReadString();
                                            wp.WPType = Utils.DataAccess.GetWaypointType(Core.WaypointTypes, br.ReadInt32());

                                            wp.Saved = true;
                                            wp.IsDataChanged = false;

                                            index++;
                                            procStep++;
                                            if (procStep >= 1000)
                                            {
                                                progress.UpdateProgress(STR_LOADING, STR_LOADINGWAYPOINTS, wptCount, index);
                                                procStep = 0;
                                            }
                                        }
                                        checkUniqueID(ri, fs);
                                    }
                                }
                                catch
                                {
                                    //error in file after lastOKPosition
                                    fs.SetLength(lastOKPosition);
                                }
                            }
                        }

                        _uniqueCheckList.Clear();
                        using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_LOADING, STR_LOADINGLOGIMAGES, logimgCount, 0))
                        {
                            int index = 0;
                            int procStep = 0;

                            using (FileStream fs = File.Open(_fileCollection.LogImagesFilename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                            {
                                fs.Position = 0;
                                long eof = fs.Length;
                                long lastOKPosition = 0;
                                RecordInfo ri = new RecordInfo();
                                try
                                {
                                    while (fs.Position < eof)
                                    {
                                        lastOKPosition = fs.Position;
                                        ri.Offset = fs.Position;
                                        fs.Read(memBuffer, 0, lsize + 1);
                                        ms.Position = 0;
                                        ri.Length = br.ReadInt64();
                                        if (memBuffer[lsize] == 0)
                                        {
                                            //free
                                            ri.FreeSlot = true;
                                            ri.ID = string.Concat("_", ri.Offset.ToString());
                                            fs.Position = ri.Offset + ri.Length;
                                        }
                                        else
                                        {
                                            //read
                                            ri.FreeSlot = false;
                                            Framework.Data.LogImage li = new Framework.Data.LogImage();

                                            fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                            ms.Position = 0;
                                            li.ID = br.ReadString();
                                            ri.ID = li.ID;

                                            li.DataFromDate = DateTime.Parse(br.ReadString());
                                            li.LogID = br.ReadString();
                                            li.Name = br.ReadString();
                                            li.Url = br.ReadString();

                                            li.Saved = true;
                                            li.IsDataChanged = false;

                                            index++;
                                            procStep++;
                                            if (procStep >= 1000)
                                            {
                                                progress.UpdateProgress(STR_LOADING, STR_LOADINGLOGIMAGES, logimgCount, index);
                                                procStep = 0;
                                            }
                                        }
                                        checkUniqueID(ri, fs);
                                    }
                                }
                                catch
                                {
                                    //error in file after lastOKPosition
                                    fs.SetLength(lastOKPosition);
                                }
                            }
                        }

                        {
                            int index = 0;
                            _uniqueCheckList.Clear();
                            using (FileStream fs = File.Open(_fileCollection.UserWaypointsFilename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                            {
                                fs.Position = 0;
                                long eof = fs.Length;
                                long lastOKPosition = 0;
                                RecordInfo ri = new RecordInfo();
                                try
                                {
                                    while (fs.Position < eof)
                                    {
                                        lastOKPosition = fs.Position;
                                        ri.Offset = fs.Position;
                                        fs.Read(memBuffer, 0, lsize + 1);
                                        ms.Position = 0;
                                        ri.Length = br.ReadInt64();
                                        if (memBuffer[lsize] == 0)
                                        {
                                            //free
                                            ri.FreeSlot = true;
                                            ri.ID = string.Concat("_", ri.Offset.ToString());
                                            fs.Position = ri.Offset + ri.Length;
                                        }
                                        else
                                        {
                                            //read
                                            ri.FreeSlot = false;
                                            Framework.Data.UserWaypoint wp = new Framework.Data.UserWaypoint();

                                            fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                            ms.Position = 0;
                                            ri.ID = br.ReadString();
                                            wp.ID = int.Parse(ri.ID);

                                            wp.Description = br.ReadString();
                                            wp.GeocacheCode = br.ReadString();
                                            wp.Lat = br.ReadDouble();
                                            wp.Lon = br.ReadDouble();
                                            wp.Date = DateTime.Parse(br.ReadString());

                                            wp.Saved = true;
                                            wp.IsDataChanged = false;

                                            index++;
                                        }
                                        checkUniqueID(ri, fs);
                                    }
                                }
                                catch
                                {
                                    //error in file after lastOKPosition
                                    fs.SetLength(lastOKPosition);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
