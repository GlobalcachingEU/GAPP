using GAPPSF.Core.Data;
using GAPPSF.Core.Storage;
using GAPPSF.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GAPPSF.GAPPDataStorage
{
    public class Importer
    {
        async public static Task<bool> PerformAction(Database database, string filename)
        {
            bool result = false;
            using (DataUpdater upd = new DataUpdater(database))
            {
                await Task.Run(() => { result = Import(database, filename); });
            }
            return result;
        }

        private static bool Import(Database database, string filename)
        {
            bool result = false;
            try
            {
                byte[] memBuffer = new byte[10 * 1024 * 1024];

                int gcCount = 0;
                int logCount = 0;
                int logimgCount = 0;
                int geocacheimgCount = 0;
                int wptCount = 0;
                int usrwptCount = 0;
                int index = 0;

                XmlDocument doc = new XmlDocument();
                doc.Load(filename);
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

                DateTime nextUpdateTime = DateTime.Now.AddSeconds(1);

                using (Utils.ProgressBlock prog = new ProgressBlock("Importing database", "Importing...", 1, 0))
                {
                    List<RecordInfo> records = new List<RecordInfo>();
                    Hashtable f_records = new Hashtable();

                    using (Utils.ProgressBlock subProg = new ProgressBlock("Importing geocaches...", gcCount, 0))
                    {

                        //GEOCACHES
                        //first all record
                        using (FileStream fs = File.Open(Path.Combine(Path.GetDirectoryName(filename), string.Format("{0}.cch", Path.GetFileNameWithoutExtension(filename))), FileMode.OpenOrCreate, FileAccess.Read))
                        using (BinaryReader br = new BinaryReader(fs))
                        {
                            fs.Position = 0;
                            long eof = fs.Length;
                            while (fs.Position < eof)
                            {
                                RecordInfo ri = new RecordInfo();
                                ri.Offset = fs.Position;
                                ri.Length = br.ReadInt64();
                                byte slotType = br.ReadByte();
                                if (slotType == 0)
                                {
                                    //free
                                }
                                else
                                {
                                    //read
                                    ri.ID = br.ReadString();
                                    if (slotType == 1)
                                    {
                                        records.Add(ri);
                                    }
                                    else
                                    {
                                        f_records.Add(ri.ID, ri);
                                    }
                                }
                                fs.Position = ri.Offset + ri.Length;
                            }
                            foreach (RecordInfo ri in records)
                            {
                                GeocacheData gc = new GeocacheData();
                                gc.Code = ri.ID;

                                fs.Position = ri.Offset + 9;

                                string dummyString = br.ReadString(); //id
                                gc.Archived = br.ReadBoolean();
                                gc.AttributeIds = ReadIntegerArray(br);
                                gc.Available = br.ReadBoolean();
                                gc.City = br.ReadString();
                                gc.Container = Utils.DataAccess.GetGeocacheContainer(br.ReadInt32());
                                bool dummyBool = br.ReadBoolean();
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
                                gc.GeocacheType = Utils.DataAccess.GetGeocacheType(br.ReadInt32());
                                dummyString = br.ReadString();
                                gc.Lat = br.ReadDouble();
                                gc.Lon = br.ReadDouble();
                                gc.MemberOnly = br.ReadBoolean();
                                gc.Municipality = br.ReadString();
                                gc.Name = br.ReadString();
                                gc.Notes = br.ReadString();
                                gc.Owner = br.ReadString();
                                gc.OwnerId = br.ReadString();
                                gc.PersonalNote = br.ReadString();
                                gc.PlacedBy = br.ReadString();
                                gc.PublishedTime = DateTime.Parse(br.ReadString());
                                gc.State = br.ReadString();
                                gc.Terrain = br.ReadDouble();
                                gc.Name = br.ReadString();
                                gc.Url = br.ReadString();
                                gc.DataFromDate = DateTime.Parse(br.ReadString());
                                gc.Locked = br.ReadBoolean();

                                RecordInfo rf = f_records[string.Format("F_{0}", ri.ID)] as RecordInfo;
                                if (rf != null)
                                {
                                    fs.Position = rf.Offset + 9;

                                    br.ReadString(); //id
                                    gc.ShortDescription = br.ReadString();
                                    gc.ShortDescriptionInHtml = br.ReadBoolean();
                                    gc.LongDescription = br.ReadString();
                                    gc.LongDescriptionInHtml = br.ReadBoolean();
                                }
                                DataAccess.AddGeocache(database, gc);
                                index++;
                                if (DateTime.Now >= nextUpdateTime)
                                {
                                    subProg.Update("Importing geocaches...", gcCount, index);
                                    nextUpdateTime = DateTime.Now.AddSeconds(1);
                                }
                            }
                        }
                    }
                    records.Clear();
                    f_records.Clear();

                    using (Utils.ProgressBlock subProg = new ProgressBlock("Importing logs...", logCount, 0))
                    {
                        index = 0;
                        //LOGS
                        //first all record
                        using (FileStream fs = File.Open(Path.Combine(Path.GetDirectoryName(filename), string.Format("{0}.lgs", Path.GetFileNameWithoutExtension(filename))), FileMode.OpenOrCreate, FileAccess.Read))
                        using (BinaryReader br = new BinaryReader(fs))
                        {
                            fs.Position = 0;
                            long eof = fs.Length;
                            while (fs.Position < eof)
                            {
                                RecordInfo ri = new RecordInfo();
                                ri.Offset = fs.Position;
                                ri.Length = br.ReadInt64();
                                byte slotType = br.ReadByte();
                                if (slotType == 0)
                                {
                                    //free
                                }
                                else
                                {
                                    //read
                                    ri.ID = br.ReadString();
                                    if (slotType == 1)
                                    {
                                        records.Add(ri);
                                    }
                                    else
                                    {
                                        f_records.Add(ri.ID, ri);
                                    }
                                }
                                fs.Position = ri.Offset + ri.Length;
                            }
                            foreach (RecordInfo ri in records)
                            {
                                LogData gc = new LogData();
                                gc.ID = ri.ID;

                                fs.Position = ri.Offset + 9;
                                string dummyString = br.ReadString(); //id
                                gc.DataFromDate = DateTime.Parse(br.ReadString());
                                gc.Date = DateTime.Parse(br.ReadString());
                                gc.Finder = br.ReadString();
                                gc.GeocacheCode = br.ReadString();
                                gc.ID = br.ReadString();
                                gc.LogType = Utils.DataAccess.GetLogType(br.ReadInt32());

                                RecordInfo rf = f_records[string.Format("F_{0}", ri.ID)] as RecordInfo;
                                if (rf != null)
                                {
                                    fs.Position = rf.Offset + 9;

                                    br.ReadString(); //id
                                    gc.TBCode = br.ReadString();
                                    gc.FinderId = br.ReadString();
                                    gc.Text = br.ReadString();
                                    gc.Encoded = br.ReadBoolean();
                                }
                                DataAccess.AddLog(database, gc);
                                index++;
                                if (DateTime.Now >= nextUpdateTime)
                                {
                                    subProg.Update("Importing geocaches...", gcCount, index);
                                    nextUpdateTime = DateTime.Now.AddSeconds(1);
                                }
                            }
                        }
                    }
                    result = true;
                }
            }
            catch
            {
            }
            return result;
        }

        private static List<int> ReadIntegerArray(BinaryReader fs)
        {
            List<int> result = new List<int>();
            int count = fs.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                result.Add(fs.ReadInt32());
            }
            return result;
        }

    }
}
