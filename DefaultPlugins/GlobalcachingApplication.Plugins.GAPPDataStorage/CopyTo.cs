using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace GlobalcachingApplication.Plugins.GAPPDataStorage
{
    public partial class InternalStorage
    {
        private string _lastCopyToFolder = null;
        private string _selectedCopyToFilename = null;

        protected override bool SupportsCopyToDatabase
        {
            get
            {
                return true;
            }
        }

        protected override bool PrepareCopyToNew()
        {
            bool result = false;

            using (System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog())
            {
                if (string.Compare(Properties.Settings.Default.ActiveDataFile, dlg.FileName, true) != 0)
                {
                    if (string.IsNullOrEmpty(_lastCopyToFolder))
                    {
                        _lastCopyToFolder = System.IO.Path.GetDirectoryName(Properties.Settings.Default.ActiveDataFile);
                    }
                    dlg.InitialDirectory = _lastCopyToFolder;
                    dlg.Filter = "*.gpp|*.gpp";
                    dlg.FileName = "";
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        try
                        {
                            if (System.IO.File.Exists(dlg.FileName))
                            {
                                System.IO.File.Delete(dlg.FileName);
                            }
                            string fn = FileCollection.getFilename(dlg.FileName, EXT_GEOCACHES);
                            if (System.IO.File.Exists(fn))
                            {
                                System.IO.File.Delete(fn);
                            }
                            fn = FileCollection.getFilename(dlg.FileName, EXT_LOGIMAGES);
                            if (System.IO.File.Exists(fn))
                            {
                                System.IO.File.Delete(fn);
                            }
                            fn = FileCollection.getFilename(dlg.FileName, EXT_GEOCACHEIMAGES);
                            if (System.IO.File.Exists(fn))
                            {
                                System.IO.File.Delete(fn);
                            }
                            fn = FileCollection.getFilename(dlg.FileName, EXT_LOGS);
                            if (System.IO.File.Exists(fn))
                            {
                                System.IO.File.Delete(fn);
                            }
                            fn = FileCollection.getFilename(dlg.FileName, EXT_USERWAYPOINTS);
                            if (System.IO.File.Exists(fn))
                            {
                                System.IO.File.Delete(fn);
                            }
                            fn = FileCollection.getFilename(dlg.FileName, EXT_WAYPPOINTS);
                            if (System.IO.File.Exists(fn))
                            {
                                System.IO.File.Delete(fn);
                            }

                            _selectedCopyToFilename = dlg.FileName;
                            result = true;
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return result;
        }

        protected override bool PrepareCopyToExisting()
        {
            bool result = false;

            using (System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog())
            {
                if (string.Compare(Properties.Settings.Default.ActiveDataFile, dlg.FileName, true) != 0)
                {
                    if (string.IsNullOrEmpty(_lastCopyToFolder))
                    {
                        _lastCopyToFolder = System.IO.Path.GetDirectoryName(Properties.Settings.Default.ActiveDataFile);
                    }
                    dlg.InitialDirectory = _lastCopyToFolder;
                    dlg.Filter = "*.gpp|*.gpp";
                    dlg.FileName = "";
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        _selectedCopyToFilename = dlg.FileName;
                        result = true;
                    }
                }
            }
            return result;
        }


        protected override void CopyToNewMethod()
        {
            using (FileCollection fc = new FileCollection(_selectedCopyToFilename))
            {
                copyToSave(fc);
            }
        }

        protected override void CopyToExistingMethod()
        {
            using (FileCollection fc = new FileCollection(_selectedCopyToFilename))
            {
                if (readFiles(fc))
                {
                    copyToSave(fc);
                }
            }
        }

        private bool readFiles(FileCollection fc)
        {
            bool result = false;
            try
            {
                //todo: when version is not compatible anymore, do a check on storage version!!

                int lsize = sizeof(long);
                byte[] memBuffer = new byte[10 * 1024 * 1024];
                using (MemoryStream ms = new MemoryStream(memBuffer))
                using (BinaryReader br = new BinaryReader(ms))
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_LOADINGDATA, STR_LOADINGDATA, 1, 0))
                {
                    FileStream fs = fc._fsGeocaches;
                    fs.Position = 0;
                    long eof = fs.Length;
                    while (fs.Position < eof)
                    {
                        RecordInfo ri = new RecordInfo();
                        ri.Offset = fs.Position;
                        fs.Read(memBuffer, 0, lsize + 1);
                        ms.Position = 0;
                        ri.Length = br.ReadInt64();
                        if (memBuffer[lsize] == 0)
                        {
                            //free
                            ri.FreeSlot = true;
                            ri.ID = string.Concat("_", ri.Offset.ToString());
                        }
                        else
                        {
                            //lazy loading
                            ri.FreeSlot = false;
                            int readCount = Math.Min(42, (int)(ri.Length - lsize - 1));
                            fs.Read(memBuffer, 0, readCount);
                            ms.Position = 0;
                            ri.ID = br.ReadString();
                        }
                        fs.Position = ri.Offset + ri.Length;
                        fc._geocachesInDB.Add(ri.ID, ri);
                    }

                    fs = fc._fsLogs;
                    fs.Position = 0;
                    eof = fs.Length;
                    while (fs.Position < eof)
                    {
                        RecordInfo ri = new RecordInfo();
                        ri.Offset = fs.Position;
                        fs.Read(memBuffer, 0, lsize + 1);
                        ms.Position = 0;
                        ri.Length = br.ReadInt64();
                        if (memBuffer[lsize] == 0)
                        {
                            //free
                            ri.FreeSlot = true;
                            ri.ID = string.Concat("_", ri.Offset.ToString());
                        }
                        else
                        {
                            //lazy loading
                            ri.FreeSlot = false;
                            int readCount = Math.Min(32, (int)(ri.Length - lsize - 1));
                            fs.Read(memBuffer, 0, readCount);
                            ms.Position = 0;
                            ri.ID = br.ReadString();
                        }
                        fs.Position = ri.Offset + ri.Length;
                        fc._logsInDB.Add(ri.ID, ri);
                    }

                    using (fs = File.Open(fc.WaypointsFilename, FileMode.OpenOrCreate, FileAccess.Read))
                    {
                        fs.Position = 0;
                        eof = fs.Length;
                        while (fs.Position < eof)
                        {
                            RecordInfo ri = new RecordInfo();
                            ri.Offset = fs.Position;
                            fs.Read(memBuffer, 0, lsize + 1);
                            ms.Position = 0;
                            ri.Length = br.ReadInt64();
                            if (memBuffer[lsize] == 0)
                            {
                                //free
                                ri.FreeSlot = true;
                                ri.ID = string.Concat("_", ri.Offset.ToString());
                            }
                            else
                            {
                                //lazy loading
                                ri.FreeSlot = false;
                                int readCount = Math.Min(32, (int)(ri.Length - lsize - 1));
                                fs.Read(memBuffer, 0, readCount);
                                ms.Position = 0;
                                ri.ID = br.ReadString();
                            }
                            fs.Position = ri.Offset + ri.Length;
                            fc._wptsInDB.Add(ri.ID, ri);
                        }
                    }

                    using (fs = File.Open(fc.UserWaypointsFilename, FileMode.OpenOrCreate, FileAccess.Read))
                    {
                        fs.Position = 0;
                        eof = fs.Length;
                        while (fs.Position < eof)
                        {
                            RecordInfo ri = new RecordInfo();
                            ri.Offset = fs.Position;
                            fs.Read(memBuffer, 0, lsize + 1);
                            ms.Position = 0;
                            ri.Length = br.ReadInt64();
                            if (memBuffer[lsize] == 0)
                            {
                                //free
                                ri.FreeSlot = true;
                                ri.ID = string.Concat("_", ri.Offset.ToString());
                            }
                            else
                            {
                                //lazy loading
                                ri.FreeSlot = false;
                                int readCount = Math.Min(32, (int)(ri.Length - lsize - 1));
                                fs.Read(memBuffer, 0, readCount);
                                ms.Position = 0;
                                ri.ID = br.ReadString();
                            }
                            fs.Position = ri.Offset + ri.Length;
                            fc._usrwptsInDB.Add(ri.ID, ri);
                        }
                    }

                    using (fs = File.Open(fc.LogImagesFilename, FileMode.OpenOrCreate, FileAccess.Read))
                    {
                        fs.Position = 0;
                        eof = fs.Length;
                        while (fs.Position < eof)
                        {
                            RecordInfo ri = new RecordInfo();
                            ri.Offset = fs.Position;
                            fs.Read(memBuffer, 0, lsize + 1);
                            ms.Position = 0;
                            ri.Length = br.ReadInt64();
                            if (memBuffer[lsize] == 0)
                            {
                                //free
                                ri.FreeSlot = true;
                                ri.ID = string.Concat("_", ri.Offset.ToString());
                            }
                            else
                            {
                                //lazy loading
                                ri.FreeSlot = false;
                                int readCount = Math.Min(100, (int)(ri.Length - lsize - 1));
                                fs.Read(memBuffer, 0, readCount);
                                ms.Position = 0;
                                ri.ID = br.ReadString();
                            }
                            fs.Position = ri.Offset + ri.Length;
                            fc._logimgsInDB.Add(ri.ID, ri);
                        }
                    }

                    using (fs = File.Open(fc.GeocacheImagesFilename, FileMode.OpenOrCreate, FileAccess.Read))
                    {
                        fs.Position = 0;
                        eof = fs.Length;
                        while (fs.Position < eof)
                        {
                            RecordInfo ri = new RecordInfo();
                            ri.Offset = fs.Position;
                            fs.Read(memBuffer, 0, lsize + 1);
                            ms.Position = 0;
                            ri.Length = br.ReadInt64();
                            if (memBuffer[lsize] == 0)
                            {
                                //free
                                ri.FreeSlot = true;
                                ri.ID = string.Concat("_", ri.Offset.ToString());
                            }
                            else
                            {
                                //lazy loading
                                ri.FreeSlot = false;
                                int readCount = Math.Min(64, (int)(ri.Length - lsize - 1));
                                fs.Read(memBuffer, 0, readCount);
                                ms.Position = 0;
                                ri.ID = br.ReadString();
                            }
                            fs.Position = ri.Offset + ri.Length;
                            fc._geocacheimgsInDB.Add(ri.ID, ri);
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

        private bool copyToSave(FileCollection fc)
        {
            bool result = true;
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_SAVING, STR_SAVINGGEOCACHES, CopyToList.Count, 0))
            {
                byte[] memBuffer = new byte[10 * 1024 * 1024];
                byte notFree = 1;
                byte notFreeF = 2;

                List<RecordInfo> freeGeocacheRecords = (from RecordInfo ri in fc._geocachesInDB.Values where ri.FreeSlot select ri).OrderByDescending(x => x.Length).ToList();
                List<RecordInfo> freeLogImageRecords = (from RecordInfo ri in fc._logimgsInDB.Values where ri.FreeSlot select ri).OrderByDescending(x => x.Length).ToList();
                List<RecordInfo> freeLogRecords = (from RecordInfo ri in fc._logsInDB.Values where ri.FreeSlot select ri).OrderByDescending(x => x.Length).ToList();
                List<RecordInfo> freeWaypointRecords = (from RecordInfo ri in fc._wptsInDB.Values where ri.FreeSlot select ri).OrderByDescending(x => x.Length).ToList();
                List<RecordInfo> freeUserWaypointRecords = (from RecordInfo ri in fc._usrwptsInDB.Values where ri.FreeSlot select ri).OrderByDescending(x => x.Length).ToList();
                List<RecordInfo> freeGeocacheImageRecords = (from RecordInfo ri in fc._geocacheimgsInDB.Values where ri.FreeSlot select ri).OrderByDescending(x => x.Length).ToList();

                using (MemoryStream ms = new MemoryStream(memBuffer))
                using (BinaryWriter bw = new BinaryWriter(ms))
                using (FileStream fsLogImages = File.Open(fc.LogImagesFilename, FileMode.OpenOrCreate, FileAccess.Write))
                using (FileStream fsWaypoints = File.Open(fc.WaypointsFilename, FileMode.OpenOrCreate, FileAccess.Write))
                using (FileStream fsUserWaypoints = File.Open(fc.UserWaypointsFilename, FileMode.OpenOrCreate, FileAccess.Write))
                using (FileStream fsGeocacheImages = File.Open(fc.GeocacheImagesFilename, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    //**********************************************
                    //          GEOCACHES
                    //**********************************************
                    long recordLength = 0;
                    byte[] extraBuffer = new byte[200];

                    int index = 0;
                    int procStep = 0;
                    foreach (Framework.Data.Geocache gc in CopyToList)
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
                        bw.Write(gc.CustomCoords);
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
                        bw.Write(gc.ID ?? "");
                        bw.Write(gc.Lat);
                        bw.Write(gc.Lon);
                        bw.Write(gc.MemberOnly);
                        bw.Write(gc.Municipality ?? "");
                        bw.Write(gc.Name ?? "");
                        bw.Write(gc.Notes ?? "");
                        bw.Write(gc.Owner ?? "");
                        bw.Write(gc.OwnerId ?? "");
                        bw.Write(gc.PersonaleNote ?? "");
                        bw.Write(gc.PlacedBy ?? "");
                        bw.Write(((DateTime)gc.PublishedTime).ToString("s"));
                        bw.Write(gc.State ?? "");
                        bw.Write(gc.Terrain);
                        bw.Write(gc.Title ?? "");
                        bw.Write(gc.Url ?? "");
                        bw.Write(gc.DataFromDate.ToString("s"));
                        bw.Write(gc.Locked);

                        writeRecord(fc._geocachesInDB, gc.Code, ms, bw, fc._fsGeocaches, memBuffer, extraBuffer, freeGeocacheRecords);

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

                        writeRecord(fc._geocachesInDB, id, ms, bw, fc._fsGeocaches, memBuffer, extraBuffer, freeGeocacheRecords);

                        List<Framework.Data.Log> lglist = Utils.DataAccess.GetLogs(Core.Logs, gc.Code);
                        if (lglist.Count > 0)
                        {
                            recordLength = 0;
                            extraBuffer = new byte[50];
                            foreach (Framework.Data.Log l in lglist)
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

                                writeRecord(fc._logsInDB, l.ID, ms, bw, fc._fsLogs, memBuffer, extraBuffer, freeLogRecords);

                                id = string.Concat("F_", l.ID);
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

                                writeRecord(fc._logsInDB, id, ms, bw, fc._fsLogs, memBuffer, extraBuffer, freeLogRecords);

                                List<Framework.Data.LogImage> lgimglist = Utils.DataAccess.GetLogImages(Core.LogImages, l.ID);
                                if (lgimglist.Count > 0)
                                {
                                    recordLength = 0;
                                    extraBuffer = new byte[10];
                                    foreach (Framework.Data.LogImage li in lgimglist)
                                    {
                                        //write to block
                                        ms.Position = 0;

                                        //block header
                                        bw.Write(recordLength); //overwrite afterwards
                                        bw.Write(notFree);
                                        bw.Write(li.ID);
                                        bw.Write(li.DataFromDate.ToString("s"));
                                        bw.Write(li.LogID ?? "");
                                        bw.Write(li.Name ?? "");
                                        bw.Write(li.Url ?? "");

                                        writeRecord(fc._logimgsInDB, li.ID, ms, bw, fsLogImages, memBuffer, extraBuffer, freeLogImageRecords);

                                    }
                                }
                            }
                        }

                        List<Framework.Data.Waypoint> wptlist = Utils.DataAccess.GetWaypointsFromGeocache(Core.Waypoints, gc.Code);
                        if (wptlist.Count > 0)
                        {
                            recordLength = 0;
                            extraBuffer = new byte[10];
                            foreach (Framework.Data.Waypoint wp in wptlist)
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

                                writeRecord(fc._wptsInDB, wp.Code, ms, bw, fsWaypoints, memBuffer, extraBuffer, freeWaypointRecords);
                            }
                        }

                        List<Framework.Data.UserWaypoint> usrwptlist = Utils.DataAccess.GetUserWaypointsFromGeocache(Core.UserWaypoints, gc.Code);
                        if (usrwptlist.Count > 0)
                        {
                            recordLength = 0;
                            extraBuffer = new byte[10];
                            foreach (Framework.Data.UserWaypoint wp in usrwptlist)
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

                                writeRecord(fc._usrwptsInDB, wp.ID.ToString(), ms, bw, fsUserWaypoints, memBuffer, extraBuffer, freeUserWaypointRecords);
                            }
                        }

                        List<Framework.Data.GeocacheImage> geocacheimglist = Utils.DataAccess.GetGeocacheImages(Core.GeocacheImages, gc.Code);
                        if (geocacheimglist.Count > 0)
                        {
                            recordLength = 0;
                            extraBuffer = new byte[100];
                            foreach (Framework.Data.GeocacheImage li in geocacheimglist)
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

                                writeRecord(fc._geocacheimgsInDB, li.ID.ToString(), ms, bw, fsGeocacheImages, memBuffer, extraBuffer, freeGeocacheImageRecords);
                            }
                        }


                        index++;
                        procStep++;
                        if (procStep >= 1000)
                        {
                            progress.UpdateProgress(STR_SAVING, STR_SAVINGGEOCACHES, CopyToList.Count, index);
                            procStep = 0;
                        }
                    }
                }
            }

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
            txt = doc.CreateTextNode(Core.Version.ToString());
            el.AppendChild(txt);
            root.AppendChild(el);

            el = doc.CreateElement("StorageVersion");
            txt = doc.CreateTextNode("1");
            el.AppendChild(txt);
            root.AppendChild(el);

            el = doc.CreateElement("GeocacheCount");
            txt = doc.CreateTextNode((from RecordInfo ri in fc._geocachesInDB.Values where !ri.FreeSlot && !ri.ID.StartsWith("F_") select ri).Count().ToString());
            el.AppendChild(txt);
            root.AppendChild(el);

            el = doc.CreateElement("LogCount");
            txt = doc.CreateTextNode((from RecordInfo ri in fc._logsInDB.Values where !ri.FreeSlot && !ri.ID.StartsWith("F_") select ri).Count().ToString());
            el.AppendChild(txt);
            root.AppendChild(el);

            el = doc.CreateElement("LogImagesCount");
            txt = doc.CreateTextNode((from RecordInfo ri in fc._logimgsInDB.Values where !ri.FreeSlot select ri).Count().ToString());
            el.AppendChild(txt);
            root.AppendChild(el);

            el = doc.CreateElement("WaypointCount");
            txt = doc.CreateTextNode((from RecordInfo ri in fc._wptsInDB.Values where !ri.FreeSlot select ri).Count().ToString());
            el.AppendChild(txt);
            root.AppendChild(el);

            el = doc.CreateElement("UserWaypointCount");
            txt = doc.CreateTextNode((from RecordInfo ri in fc._usrwptsInDB.Values where !ri.FreeSlot select ri).Count().ToString());
            el.AppendChild(txt);
            root.AppendChild(el);

            el = doc.CreateElement("GeocacheImagesCount");
            txt = doc.CreateTextNode((from RecordInfo ri in fc._geocacheimgsInDB.Values where !ri.FreeSlot select ri).Count().ToString());
            el.AppendChild(txt);
            root.AppendChild(el);

            doc.Save(fc.DatabaseInfoFilename);
            return result;
        }

    }
}
