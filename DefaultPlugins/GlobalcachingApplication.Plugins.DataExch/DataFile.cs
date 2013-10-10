using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GlobalcachingApplication.Utils;

namespace GlobalcachingApplication.Plugins.DataExch
{
    public class DataFile
    {
        public const string STR_EXPORT = "Export to GAPP Data Exchange file";
        public const string STR_EXPORT_CACHES = "Export geocaches...";
        public const string STR_IMPORT = "Import GAPP Data Exchange file";
        public const string STR_IMPORT_CACHES = "Import geocaches...";

        public DataFile()
        {
        }

        public void Export(Framework.Interfaces.ICore core, Utils.BasePlugin.Plugin owner, string fileName, List<Framework.Data.Geocache> gcList)
        {
            using (Utils.ProgressBlock upd = new Utils.ProgressBlock(owner, STR_EXPORT, STR_EXPORT, 1, 0))
            {
                using (FileStream fs = File.OpenWrite(fileName))
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write("GAPP");
                    bw.Write(BitConverter.IsLittleEndian);
                    int version = 3;
                    bw.Write(version);
                    using (Utils.ProgressBlock prog = new Utils.ProgressBlock(owner, STR_EXPORT, STR_EXPORT_CACHES, gcList.Count, 0))
                    {
                        bw.Write(gcList.Count);
                        int index = 0;
                        foreach (Framework.Data.Geocache gc in gcList)
                        {
                            bw.Write(gc.Archived);
                            WriteIntegerArray(bw, gc.AttributeIds);
                            bw.Write(gc.Available);
                            bw.Write(gc.City ?? "");
                            bw.Write(gc.Code ?? "");
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
                            if (gc.FoundDate == null)
                            {
                                bw.Write("");
                            }
                            else
                            {
                                bw.Write(((DateTime)gc.FoundDate).ToString("s"));
                            }
                            bw.Write(gc.GeocacheType.ID);
                            bw.Write(gc.ID ?? "");
                            bw.Write(gc.Lat);
                            bw.Write(gc.Lon);
                            bw.Write(gc.LongDescription ?? "");
                            bw.Write(gc.LongDescriptionInHtml);
                            bw.Write(gc.MemberOnly);
                            bw.Write(gc.Municipality ?? "");
                            bw.Write(gc.Name ?? "");
                            bw.Write(gc.Notes ?? "");
                            bw.Write(gc.Owner ?? "");
                            bw.Write(gc.OwnerId ?? "");
                            bw.Write(gc.PersonaleNote ?? "");
                            bw.Write(gc.PlacedBy ?? "");
                            bw.Write(((DateTime)gc.PublishedTime).ToString("s"));
                            bw.Write(gc.Selected);
                            bw.Write(gc.ShortDescription ?? "");
                            bw.Write(gc.ShortDescriptionInHtml);
                            bw.Write(gc.State ?? "");
                            bw.Write(gc.Terrain);
                            bw.Write(gc.Title ?? "");
                            bw.Write(gc.Url ?? "");
                            bw.Write(gc.DataFromDate.ToString("s"));
                            bw.Write(gc.Locked);

                            //logs
                            List<Framework.Data.Log> logs = Utils.DataAccess.GetLogs(core.Logs, gc.Code);
                            bw.Write(logs.Count);
                            foreach (Framework.Data.Log l in logs)
                            {
                                bw.Write(l.DataFromDate.ToString("s"));
                                bw.Write(l.Date.ToString("s"));
                                bw.Write(l.Encoded);
                                bw.Write(l.Finder ?? "");
                                bw.Write(l.FinderId ?? "");
                                bw.Write(l.GeocacheCode ?? "");
                                bw.Write(l.ID ?? "");
                                bw.Write(l.LogType.ID);
                                bw.Write(l.TBCode ?? "");
                                bw.Write(l.Text ?? "");

                                List<Framework.Data.LogImage> logImages = Utils.DataAccess.GetLogImages(core.LogImages, l.ID);
                                bw.Write(logImages.Count);
                                foreach (Framework.Data.LogImage li in logImages)
                                {
                                    bw.Write(li.DataFromDate.ToString("s"));
                                    bw.Write(li.ID ?? "");
                                    bw.Write(li.LogID ?? "");
                                    bw.Write(li.Name ?? "");
                                    bw.Write(li.Url ?? "");
                                }
                            }

                            //waypoints
                            List<Framework.Data.Waypoint> wps = Utils.DataAccess.GetWaypointsFromGeocache(core.Waypoints, gc.Code);
                            bw.Write(wps.Count);
                            foreach (Framework.Data.Waypoint wp in wps)
                            {
                                bw.Write(wp.Code ?? "");
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
                            }

                            //user waypoints
                            List<Framework.Data.UserWaypoint> usrwps = Utils.DataAccess.GetUserWaypointsFromGeocache(core.UserWaypoints, gc.Code);
                            bw.Write(usrwps.Count);
                            foreach (Framework.Data.UserWaypoint wp in usrwps)
                            {
                                bw.Write(wp.ID);
                                bw.Write(wp.Description ?? "");
                                bw.Write(wp.GeocacheCode ?? "");
                                bw.Write(wp.Lat);
                                bw.Write(wp.Lon);
                                bw.Write(wp.Date.ToString("s"));
                            }

                            index++;
                            if (index % 200 == 0)
                            {
                                prog.UpdateProgress(STR_EXPORT, STR_EXPORT_CACHES, gcList.Count, index);
                            }
                        }
                    }
                }
            }
        }

        public void Import(Framework.Interfaces.ICore core, Utils.BasePlugin.Plugin owner, string fileName)
        {
            using (Utils.ProgressBlock upd = new Utils.ProgressBlock(owner, STR_IMPORT, STR_IMPORT, 1, 0))
            {
                using (FileStream fs = File.OpenRead(fileName))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    string tag = br.ReadString();
                    if (tag == "GAPP")
                    {
                        bool IsLittleEndian = br.ReadBoolean();
                        int version = br.ReadInt32();
                        if (IsLittleEndian == BitConverter.IsLittleEndian && version<=3)
                        {
                            int count = br.ReadInt32();
                            using (Utils.ProgressBlock prog = new Utils.ProgressBlock(owner, STR_IMPORT, STR_IMPORT_CACHES, count, 0))
                            {
                                for (int index = 0; index < count; index++)
                                {
                                    Framework.Data.Geocache gc = new Framework.Data.Geocache();
                                    gc.Archived = br.ReadBoolean();
                                    gc.AttributeIds = ReadIntegerArray(br);
                                    gc.Available = br.ReadBoolean();
                                    gc.City = br.ReadString();
                                    gc.Code = br.ReadString();
                                    gc.Container = Utils.DataAccess.GetGeocacheContainer(core.GeocacheContainers, br.ReadInt32());
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
                                    string s = br.ReadString();
                                    gc.GeocacheType = Utils.DataAccess.GetGeocacheType(core.GeocacheTypes, br.ReadInt32());
                                    gc.ID = br.ReadString();
                                    gc.Lat = br.ReadDouble();
                                    gc.Lon = br.ReadDouble();
                                    gc.LongDescription = br.ReadString();
                                    gc.LongDescriptionInHtml = br.ReadBoolean();
                                    gc.MemberOnly = br.ReadBoolean();
                                    gc.Municipality = br.ReadString();
                                    gc.Name = br.ReadString();
                                    gc.Notes = br.ReadString();
                                    gc.Owner = br.ReadString();
                                    gc.OwnerId = br.ReadString();
                                    gc.PersonaleNote = br.ReadString();
                                    gc.PlacedBy = br.ReadString();
                                    gc.PublishedTime = DateTime.Parse(br.ReadString());
                                    gc.Selected = br.ReadBoolean();
                                    gc.ShortDescription = br.ReadString();
                                    gc.ShortDescriptionInHtml = br.ReadBoolean();
                                    gc.State = br.ReadString();
                                    gc.Terrain = br.ReadDouble();
                                    gc.Title = br.ReadString();
                                    gc.Url = br.ReadString();
                                    gc.DataFromDate = DateTime.Parse(br.ReadString());
                                    if (version > 1)
                                    {
                                        gc.Locked = br.ReadBoolean();
                                    }

                                    Calculus.SetDistanceAndAngleGeocacheFromLocation(gc, core.CenterLocation);
                                    (owner as Import).AddImportedGeocache(gc);

                                    int logcount = br.ReadInt32();
                                    for (int lc = 0; lc < logcount; lc++)
                                    {
                                        Framework.Data.Log log = new Framework.Data.Log();
                                        log.DataFromDate = DateTime.Parse(br.ReadString());
                                        log.Date = DateTime.Parse(br.ReadString());
                                        log.Encoded = br.ReadBoolean();
                                        log.Finder = br.ReadString();
                                        log.FinderId = br.ReadString();
                                        log.GeocacheCode = br.ReadString();
                                        log.ID = br.ReadString();
                                        log.LogType = Utils.DataAccess.GetLogType(core.LogTypes, br.ReadInt32());
                                        log.TBCode = br.ReadString();
                                        log.Text = br.ReadString();

                                        (owner as Import).AddImportedLog(log);

                                        int logimgcount = br.ReadInt32();
                                        for (int lic = 0; lic < logimgcount; lic++)
                                        {
                                            Framework.Data.LogImage li = new Framework.Data.LogImage();
                                            li.DataFromDate = DateTime.Parse(br.ReadString());
                                            li.ID = br.ReadString();
                                            li.LogID = br.ReadString();
                                            li.Name = br.ReadString();
                                            li.Url = br.ReadString();

                                            (owner as Import).AddImportedLogImage(li);
                                        }
                                    }

                                    int wpcount = br.ReadInt32();
                                    for (int wpc = 0; wpc < wpcount; wpc++)
                                    {
                                        Framework.Data.Waypoint wp = new Framework.Data.Waypoint();
                                        wp.Code = br.ReadString();
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
                                        wp.WPType = Utils.DataAccess.GetWaypointType(core.WaypointTypes, br.ReadInt32());

                                        (owner as Import).AddImportedWaypoint(wp);
                                    }

                                    if (version > 2)
                                    {
                                        int usrwpcount = br.ReadInt32();
                                        for (int wpc = 0; wpc < usrwpcount; wpc++)
                                        {
                                            Framework.Data.UserWaypoint wp = new Framework.Data.UserWaypoint();
                                            wp.ID = br.ReadInt32();
                                            wp.Description = br.ReadString();
                                            wp.GeocacheCode = br.ReadString();
                                            wp.Lat = br.ReadDouble();
                                            wp.Lon = br.ReadDouble();
                                            wp.Date = DateTime.Parse(br.ReadString());

                                            (owner as Import).AddImportedUserWaypoint(wp);
                                        }
                                    }

                                    if (index % 200 == 0)
                                    {
                                        prog.UpdateProgress(STR_IMPORT, STR_IMPORT_CACHES, count, index);
                                    }
                                }
                            }
                        }
                        else
                        {
                            //message
                        }
                    }
                    else
                    {
                        //message
                    }
                }
            }
        }

        public void WriteIntegerArray(BinaryWriter fs, List<int> values)
        {
            fs.Write(values.Count);
            foreach (int i in values)
            {
                fs.Write(i);
            }
        }

        public List<int> ReadIntegerArray(BinaryReader fs)
        {
            List<int> result = new List<int>();
            int count = fs.ReadInt32();
            for (int i =0; i<count; i++)
            {
                result.Add(fs.ReadInt32());
            }
            return result;
        }
    }
}
