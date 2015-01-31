using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using GlobalcachingApplication.Utils;
using System.Collections;

namespace GlobalcachingApplication.Plugins.GAPPDataStorage
{
    public partial class InternalStorage
    {
        private string _lastInsertFromFolder = null;
        private string _selectedInsertFromFilename = null;

        protected override bool SupportsInsertFromDatabase
        {
            get
            {
                return true;
            }
        }

        public override bool PrepareInsertFromDatabase()
        {
            bool result = false;
            using (System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog())
            {
                if (string.Compare(PluginSettings.Instance.ActiveDataFile, dlg.FileName, true) != 0)
                {
                    if (string.IsNullOrEmpty(_lastInsertFromFolder))
                    {
                        _lastInsertFromFolder = System.IO.Path.GetDirectoryName(PluginSettings.Instance.ActiveDataFile);
                    }
                    dlg.InitialDirectory = _lastInsertFromFolder;
                    dlg.Filter = "*.gpp|*.gpp";
                    dlg.FileName = "";
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        if (string.Compare(PluginSettings.Instance.ActiveDataFile, dlg.FileName, true) != 0)
                        {
                            _selectedInsertFromFilename = dlg.FileName;
                            result = true;
                        }
                    }
                }
            }
            return result;
        }

        public override bool InsertFromDatabaseOnlyNew()
        {
            return insertFromDatabase(false);
        }

        public override bool InsertFromDatabaseOverwrite()
        {
            return insertFromDatabase(true);
        }

        private bool insertFromDatabase(bool overwrite)
        {
            bool result = false;

            try
            {
                int lsize = sizeof(long);
                byte[] memBuffer = new byte[10 * 1024 * 1024];
                using (MemoryStream ms = new MemoryStream(memBuffer))
                using (BinaryReader br = new BinaryReader(ms))
                using (FileCollection fc = new FileCollection(_selectedInsertFromFilename))
                {

                    int gcCount = 0;
                    int logCount = 0;
                    int logimgCount = 0;
                    int geocacheimgCount = 0;
                    int wptCount = 0;
                    int usrwptCount = 0;

                    Hashtable htInsertedGeocaches = new Hashtable();
                    Hashtable htInsertedLogs = new Hashtable();

                    XmlDocument doc = new XmlDocument();
                    doc.Load(fc.DatabaseInfoFilename);
                    XmlElement root = doc.DocumentElement;
                    gcCount = int.Parse(root.SelectSingleNode("GeocacheCount").InnerText);
                    logCount = int.Parse(root.SelectSingleNode("LogCount").InnerText);
                    logimgCount = int.Parse(root.SelectSingleNode("LogImagesCount").InnerText);
                    wptCount = int.Parse(root.SelectSingleNode("WaypointCount").InnerText);
                    usrwptCount = int.Parse(root.SelectSingleNode("UserWaypointCount").InnerText);
                    if (root.SelectSingleNode("GeocacheImagesCount") != null)
                    {
                        geocacheimgCount = int.Parse(root.SelectSingleNode("GeocacheImagesCount").InnerText);
                    }

                    DateTime nextUpdateTime = DateTime.MinValue;
                    RecordInfo ri = new RecordInfo();
                    using (Utils.ProgressBlock fixscr = new Utils.ProgressBlock(this, STR_LOADING, STR_LOADINGDATA, 1, 0))
                    {
                        using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_LOADING, STR_LOADINGGEOCACHES, gcCount, 0))
                        {
                            int index = 0;

                            FileStream fs = fc._fsGeocaches;
                            fs.Position = 0;
                            long eof = fs.Length;
                            while (fs.Position < eof)
                            {
                                ri.Offset = fs.Position;
                                fs.Read(memBuffer, 0, lsize + 1);
                                ms.Position = 0;
                                ri.Length = br.ReadInt64();
                                if (memBuffer[lsize] == 0)
                                {
                                    //free
                                    //ignore
                                    fs.Position = ri.Offset + ri.Length;
                                }
                                else if (memBuffer[lsize] == 2)
                                {
                                    //read
                                    fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                    ms.Position = 0;
                                    ri.ID = br.ReadString().Substring(2);
                                    bool newGeocache;
                                    Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(Core.Geocaches, ri.ID);
                                    if (gc == null)
                                    {
                                        gc = new Framework.Data.Geocache();
                                        htInsertedGeocaches.Add(ri.ID, gc);
                                        newGeocache = true;
                                    }
                                    else
                                    {
                                        if (overwrite || htInsertedGeocaches[ri.ID] != null)
                                        {
                                            newGeocache = false;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    gc.BeginUpdate();
                                    try
                                    {
                                        gc.Code = ri.ID;
                                        gc.ShortDescription = br.ReadString();
                                        gc.ShortDescriptionInHtml = br.ReadBoolean();
                                        gc.LongDescription = br.ReadString();
                                        gc.LongDescriptionInHtml = br.ReadBoolean();
                                    }
                                    catch
                                    {
                                    }
                                    gc.EndUpdate();
                                    if (newGeocache)
                                    {
                                        Core.Geocaches.Add(gc);
                                    }
                                }
                                else
                                {
                                    //read

                                    fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                    ms.Position = 0;
                                    ri.ID = br.ReadString();

                                    bool newGeocache;
                                    Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(Core.Geocaches, ri.ID);
                                    if (gc == null)
                                    {
                                        gc = new Framework.Data.Geocache();
                                        htInsertedGeocaches.Add(ri.ID, gc);
                                        newGeocache = true;
                                    }
                                    else
                                    {
                                        if (overwrite || htInsertedGeocaches[ri.ID] != null)
                                        {
                                            newGeocache = false;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    gc.BeginUpdate();
                                    try
                                    {
                                        gc.Code = ri.ID;
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

                                        Calculus.SetDistanceAndAngleGeocacheFromLocation(gc, Core.CenterLocation);
                                    }
                                    catch
                                    {
                                    }
                                    gc.EndUpdate();

                                    if (newGeocache)
                                    {
                                        Core.Geocaches.Add(gc);
                                    }

                                    index++;
                                    if (DateTime.Now >= nextUpdateTime)
                                    {
                                        progress.UpdateProgress(STR_LOADING, STR_LOADINGGEOCACHES, gcCount, index);
                                        nextUpdateTime = DateTime.Now.AddSeconds(1);
                                    }
                                }
                            }
                        }

                        using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_LOADING, STR_LOADINGLOGS, logCount, 0))
                        {
                            int index = 0;
                            nextUpdateTime = DateTime.MinValue;

                            FileStream fs = fc._fsLogs;
                            fs.Position = 0;
                            long eof = fs.Length;
                            while (fs.Position < eof)
                            {
                                ri.Offset = fs.Position;
                                fs.Read(memBuffer, 0, lsize + 1);
                                ms.Position = 0;
                                ri.Length = br.ReadInt64();
                                if (memBuffer[lsize] == 0)
                                {
                                    //free
                                    fs.Position = ri.Offset + ri.Length;
                                }
                                else if (memBuffer[lsize] == 2)
                                {
                                    //read
                                    fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                    ms.Position = 0;
                                    ri.ID = br.ReadString().Substring(2);
                                    fs.Position = ri.Offset + ri.Length;

                                    bool newLog;
                                    bool logComplete = true;
                                    Framework.Data.Log log = Utils.DataAccess.GetLog(Core.Logs, ri.ID);
                                    if (log == null)
                                    {
                                        newLog = true;
                                        log = htInsertedLogs[ri.ID] as Framework.Data.Log;
                                        if (log == null)
                                        {
                                            log = new Framework.Data.Log();
                                            htInsertedLogs.Add(ri.ID, log);
                                            logComplete = false;
                                        }
                                        else
                                        {
                                            logComplete = true;
                                        }
                                    }
                                    else
                                    {
                                        if (overwrite)
                                        {
                                            newLog = false;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    log.BeginUpdate();
                                    try
                                    {
                                        log.ID = ri.ID;
                                        log.TBCode = br.ReadString();
                                        log.FinderId = br.ReadString();
                                        log.Text = br.ReadString();
                                        log.Encoded = br.ReadBoolean();
                                    }
                                    catch
                                    {
                                    }
                                    log.EndUpdate();
                                    if (newLog && logComplete && htInsertedLogs[ri.ID] != null)
                                    {
                                        if (htInsertedGeocaches[log.GeocacheCode ?? ""] != null || Utils.DataAccess.GetGeocache(Core.Geocaches, log.GeocacheCode ?? "") != null)
                                        {
                                            Core.Logs.Add(log);
                                        }
                                        htInsertedLogs.Remove(ri.ID);
                                    }
                                }
                                else
                                {
                                    //read
                                    fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                    ms.Position = 0;
                                    ri.ID = br.ReadString();

                                    bool newLog;
                                    bool logComplete = true;
                                    Framework.Data.Log log = Utils.DataAccess.GetLog(Core.Logs, ri.ID);
                                    if (log == null)
                                    {
                                        newLog = true;
                                        log = htInsertedLogs[ri.ID] as Framework.Data.Log;
                                        if (log == null)
                                        {
                                            log = new Framework.Data.Log();
                                            htInsertedLogs.Add(ri.ID, log);
                                            logComplete = false;
                                        }
                                        else
                                        {
                                            logComplete = true;
                                        }
                                    }
                                    else
                                    {
                                        if (overwrite)
                                        {
                                            newLog = false;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    log.BeginUpdate();
                                    try
                                    {

                                        log.ID = ri.ID;
                                        log.DataFromDate = DateTime.Parse(br.ReadString());
                                        log.Date = DateTime.Parse(br.ReadString());
                                        log.Finder = br.ReadString();
                                        log.GeocacheCode = br.ReadString();
                                        log.ID = br.ReadString();
                                        log.LogType = Utils.DataAccess.GetLogType(Core.LogTypes, br.ReadInt32());
                                    }
                                    catch
                                    {
                                    }
                                    log.EndUpdate();

                                    if (newLog && logComplete && htInsertedLogs[ri.ID] != null)
                                    {
                                        //check if geocache is present
                                        if (htInsertedGeocaches[log.GeocacheCode ?? ""] != null || Utils.DataAccess.GetGeocache(Core.Geocaches, log.GeocacheCode ?? "") != null)
                                        {
                                            Core.Logs.Add(log);
                                        }
                                        htInsertedLogs.Remove(ri.ID);
                                    }

                                    index++;
                                    if (DateTime.Now >= nextUpdateTime)
                                    {
                                        progress.UpdateProgress(STR_LOADING, STR_LOADINGLOGS, logCount, index);
                                        nextUpdateTime = DateTime.Now.AddSeconds(1);
                                    }
                                }
                            }
                        }


                        using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_LOADING, STR_LOADINGWAYPOINTS, wptCount, 0))
                        {
                            int index = 0;
                            int procStep = 0;

                            using (FileStream fs = File.Open(fc.WaypointsFilename, FileMode.OpenOrCreate, FileAccess.Read))
                            {
                                fs.Position = 0;
                                long eof = fs.Length;
                                while (fs.Position < eof)
                                {
                                    ri.Offset = fs.Position;
                                    fs.Read(memBuffer, 0, lsize + 1);
                                    ms.Position = 0;
                                    ri.Length = br.ReadInt64();
                                    if (memBuffer[lsize] == 0)
                                    {
                                        //free
                                        fs.Position = ri.Offset + ri.Length;
                                    }
                                    else
                                    {
                                        //read
                                        fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                        ms.Position = 0;
                                        ri.ID = br.ReadString();

                                        bool newWp;
                                        Framework.Data.Waypoint wp = Utils.DataAccess.GetWaypoint(Core.Waypoints, ri.ID);
                                        if (wp == null)
                                        {
                                            newWp = true;
                                            wp = new Framework.Data.Waypoint();
                                        }
                                        else
                                        {
                                            if (overwrite)
                                            {
                                                newWp = false;
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }

                                        wp.BeginUpdate();
                                        try
                                        {
                                            wp.Code = ri.ID;
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
                                        }
                                        catch
                                        {
                                        }
                                        wp.EndUpdate();
                                        if (newWp)
                                        {
                                            if (Utils.DataAccess.GetGeocache(Core.Geocaches, wp.GeocacheCode ?? "") != null)
                                            {
                                                Core.Waypoints.Add(wp);
                                            }
                                        }

                                        index++;
                                        procStep++;
                                        if (procStep >= 1000)
                                        {
                                            progress.UpdateProgress(STR_LOADING, STR_LOADINGWAYPOINTS, wptCount, index);
                                            procStep = 0;
                                        }
                                    }
                                }
                            }
                        }

                        using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_LOADING, STR_LOADINGLOGIMAGES, logimgCount, 0))
                        {
                            int index = 0;
                            int procStep = 0;

                            using (FileStream fs = File.Open(fc.LogImagesFilename, FileMode.OpenOrCreate, FileAccess.Read))
                            {
                                fs.Position = 0;
                                long eof = fs.Length;
                                while (fs.Position < eof)
                                {
                                    ri.Offset = fs.Position;
                                    fs.Read(memBuffer, 0, lsize + 1);
                                    ms.Position = 0;
                                    ri.Length = br.ReadInt64();
                                    if (memBuffer[lsize] == 0)
                                    {
                                        //free
                                        fs.Position = ri.Offset + ri.Length;
                                    }
                                    else
                                    {
                                        //read
                                        fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                        ms.Position = 0;
                                        ri.ID = br.ReadString();

                                        bool newWp;
                                        Framework.Data.LogImage li = Utils.DataAccess.GetLogImage(Core.LogImages, ri.ID);
                                        if (li == null)
                                        {
                                            newWp = true;
                                            li = new Framework.Data.LogImage();
                                        }
                                        else
                                        {
                                            if (overwrite)
                                            {
                                                newWp = false;
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }

                                        li.BeginUpdate();
                                        try
                                        {
                                            li.ID = ri.ID;
                                            li.DataFromDate = DateTime.Parse(br.ReadString());
                                            li.LogID = br.ReadString();
                                            li.Name = br.ReadString();
                                            li.Url = br.ReadString();
                                        }
                                        catch
                                        {
                                        }
                                        li.EndUpdate();

                                        if (newWp)
                                        {
                                            if (Utils.DataAccess.GetLog(Core.Logs, li.LogID ?? "") != null)
                                            {
                                                Core.LogImages.Add(li);
                                            }
                                        }

                                        index++;
                                        procStep++;
                                        if (procStep >= 1000)
                                        {
                                            progress.UpdateProgress(STR_LOADING, STR_LOADINGLOGIMAGES, logimgCount, index);
                                            procStep = 0;
                                        }
                                    }
                                }
                            }
                        }

                        using (Utils.ProgressBlock progress = new ProgressBlock(this, STR_LOADING, STR_LOADINGGEOCACHEIMAGES, geocacheimgCount, 0))
                        {
                            int index = 0;
                            int procStep = 0;

                            using (FileStream fs = File.Open(fc.GeocacheImagesFilename, FileMode.OpenOrCreate, FileAccess.Read))
                            {
                                fs.Position = 0;
                                long eof = fs.Length;
                                while (fs.Position < eof)
                                {
                                    ri.Offset = fs.Position;
                                    fs.Read(memBuffer, 0, lsize + 1);
                                    ms.Position = 0;
                                    ri.Length = br.ReadInt64();
                                    if (memBuffer[lsize] == 0)
                                    {
                                        //free
                                        fs.Position = ri.Offset + ri.Length;
                                    }
                                    else
                                    {
                                        //read
                                        fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                        ms.Position = 0;
                                        ri.ID = br.ReadString();

                                        bool newWp;
                                        Framework.Data.GeocacheImage wp = Utils.DataAccess.GetGeocacheImage(Core.GeocacheImages, ri.ID);
                                        if (wp == null)
                                        {
                                            newWp = true;
                                            wp = new Framework.Data.GeocacheImage();
                                        }
                                        else
                                        {
                                            if (overwrite)
                                            {
                                                newWp = false;
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }

                                        wp.BeginUpdate();
                                        try
                                        {
                                            wp.ID = ri.ID;
                                            wp.DataFromDate = DateTime.Parse(br.ReadString());
                                            wp.GeocacheCode = br.ReadString();
                                            wp.Description = br.ReadString();
                                            wp.Name = br.ReadString();
                                            wp.Url = br.ReadString();
                                            wp.MobileUrl = br.ReadString();
                                            wp.ThumbUrl = br.ReadString();
                                        }
                                        catch
                                        {
                                        }
                                        wp.EndUpdate();
                                        if (newWp)
                                        {
                                            if (Utils.DataAccess.GetGeocache(Core.Geocaches, wp.GeocacheCode ?? "") != null)
                                            {
                                                Core.GeocacheImages.Add(wp);
                                            }
                                        }

                                        index++;
                                        procStep++;
                                        if (procStep >= 1000)
                                        {
                                            progress.UpdateProgress(STR_LOADING, STR_LOADINGGEOCACHEIMAGES, geocacheimgCount, index);
                                            procStep = 0;
                                        }
                                    }
                                }
                            }
                        }

                        {
                            using (FileStream fs = File.Open(fc.UserWaypointsFilename, FileMode.OpenOrCreate, FileAccess.Read))
                            {
                                fs.Position = 0;
                                long eof = fs.Length;
                                while (fs.Position < eof)
                                {
                                    ri.Offset = fs.Position;
                                    fs.Read(memBuffer, 0, lsize + 1);
                                    ms.Position = 0;
                                    ri.Length = br.ReadInt64();
                                    if (memBuffer[lsize] == 0)
                                    {
                                        //free
                                        fs.Position = ri.Offset + ri.Length;
                                    }
                                    else
                                    {
                                        //read
                                        fs.Read(memBuffer, 0, (int)(ri.Length - lsize - 1));
                                        ms.Position = 0;
                                        ri.ID = br.ReadString();

                                        bool newWp;
                                        Framework.Data.UserWaypoint wp = Utils.DataAccess.GetUserWaypoint(Core.UserWaypoints, int.Parse(ri.ID));
                                        if (wp == null)
                                        {
                                            newWp = true;
                                            wp = new Framework.Data.UserWaypoint();
                                        }
                                        else
                                        {
                                            if (overwrite)
                                            {
                                                newWp = false;
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }

                                        wp.BeginUpdate();
                                        try
                                        {
                                            wp.ID = int.Parse(ri.ID);
                                            wp.Description = br.ReadString();
                                            wp.GeocacheCode = br.ReadString();
                                            wp.Lat = br.ReadDouble();
                                            wp.Lon = br.ReadDouble();
                                            wp.Date = DateTime.Parse(br.ReadString());
                                        }
                                        catch
                                        {
                                        }
                                        if (newWp)
                                        {
                                            if (Utils.DataAccess.GetGeocache(Core.Geocaches, wp.GeocacheCode ?? "") != null)
                                            {
                                                Core.UserWaypoints.Add(wp);
                                            }
                                        }

                                        wp.EndUpdate();
                                    }
                                }
                            }
                        }
                    }
                }
                result = true;
            }
            catch
            {
            }
            return result;
        }
    }
}
