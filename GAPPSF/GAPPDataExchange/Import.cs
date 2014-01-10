using GAPPSF.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.GAPPDataExchange
{
    public class Import
    {
        public async Task ImportFile(Core.Storage.Database db)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".gde"; // Default file extension
            dlg.Filter = "GAPP Data Exchange (*.gde)|*.gde"; // Filter files by extension 

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                await ImportFile(db, dlg.FileName);
            }
        }

        public async Task ImportFile(Core.Storage.Database db, string filename)
        {
            using (Utils.DataUpdater upd = new DataUpdater(db))
            {
                await Task.Run(() =>
                {
                    try
                    {
                        DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                        using (FileStream fs = File.OpenRead(filename))
                        using (BinaryReader br = new BinaryReader(fs))
                        {
                            string tag = br.ReadString();
                            if (tag == "GAPP")
                            {
                                bool IsLittleEndian = br.ReadBoolean();
                                int version = br.ReadInt32();
                                if (IsLittleEndian == BitConverter.IsLittleEndian && version <= 3)
                                {
                                    int count = br.ReadInt32();
                                    using (Utils.ProgressBlock prog = new Utils.ProgressBlock("ImportGAPPDataExchangeFile", "ImportGeocaches", count, 0))
                                    {
                                        for (int index = 0; index < count; index++)
                                        {
                                            Core.Data.GeocacheData gc = new Core.Data.GeocacheData();
                                            gc.Archived = br.ReadBoolean();
                                            gc.AttributeIds = ReadIntegerArray(br);
                                            gc.Available = br.ReadBoolean();
                                            gc.City = br.ReadString();
                                            gc.Code = br.ReadString();
                                            gc.Container = Utils.DataAccess.GetGeocacheContainer(br.ReadInt32());
                                            bool dummyb = br.ReadBoolean();
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
                                            gc.GeocacheType = Utils.DataAccess.GetGeocacheType(br.ReadInt32());
                                            string dummystr = br.ReadString();
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
                                            gc.PersonalNote = br.ReadString();
                                            gc.PlacedBy = br.ReadString();
                                            gc.PublishedTime = DateTime.Parse(br.ReadString());
                                            dummyb = br.ReadBoolean();
                                            gc.ShortDescription = br.ReadString();
                                            gc.ShortDescriptionInHtml = br.ReadBoolean();
                                            gc.State = br.ReadString();
                                            gc.Terrain = br.ReadDouble();
                                            dummystr = br.ReadString();
                                            gc.Url = br.ReadString();
                                            gc.DataFromDate = DateTime.Parse(br.ReadString());
                                            if (version > 1)
                                            {
                                                gc.Locked = br.ReadBoolean();
                                            }

                                            bool gcAdded = Utils.DataAccess.AddGeocache(db, gc);

                                            int logcount = br.ReadInt32();
                                            for (int lc = 0; lc < logcount; lc++)
                                            {
                                                Core.Data.LogData log = new Core.Data.LogData();
                                                log.DataFromDate = DateTime.Parse(br.ReadString());
                                                log.Date = DateTime.Parse(br.ReadString());
                                                log.Encoded = br.ReadBoolean();
                                                log.Finder = br.ReadString();
                                                log.FinderId = br.ReadString();
                                                log.GeocacheCode = br.ReadString();
                                                log.ID = br.ReadString();
                                                log.LogType = Utils.DataAccess.GetLogType(br.ReadInt32());
                                                log.TBCode = br.ReadString();
                                                log.Text = br.ReadString();

                                                if (gcAdded)
                                                {
                                                    Utils.DataAccess.AddLog(db, log);
                                                }

                                                int logimgcount = br.ReadInt32();
                                                for (int lic = 0; lic < logimgcount; lic++)
                                                {
                                                    Core.Data.LogImageData li = new Core.Data.LogImageData();
                                                    li.DataFromDate = DateTime.Parse(br.ReadString());
                                                    li.ID = br.ReadString();
                                                    li.LogId = br.ReadString();
                                                    li.Name = br.ReadString();
                                                    li.Url = br.ReadString();

                                                    if (gcAdded)
                                                    {
                                                        Utils.DataAccess.AddLogImage(db, li);
                                                    }
                                                }
                                            }

                                            int wpcount = br.ReadInt32();
                                            for (int wpc = 0; wpc < wpcount; wpc++)
                                            {
                                                Core.Data.WaypointData wp = new Core.Data.WaypointData();
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
                                                wp.WPType = Utils.DataAccess.GetWaypointType(br.ReadInt32());

                                                if (gcAdded)
                                                {
                                                    Utils.DataAccess.AddWaypoint(db, wp);
                                                }
                                            }

                                            if (version > 2)
                                            {
                                                int usrwpcount = br.ReadInt32();
                                                for (int wpc = 0; wpc < usrwpcount; wpc++)
                                                {
                                                    Core.Data.UserWaypointData wp = new Core.Data.UserWaypointData();
                                                    wp.ID = br.ReadInt32().ToString();
                                                    wp.Description = br.ReadString();
                                                    wp.GeocacheCode = br.ReadString();
                                                    wp.Lat = br.ReadDouble();
                                                    wp.Lon = br.ReadDouble();
                                                    wp.Date = DateTime.Parse(br.ReadString());

                                                    if (gcAdded)
                                                    {
                                                        Utils.DataAccess.AddUserWaypoint(db, wp);
                                                    }
                                                }
                                            }

                                            index++;
                                            if (DateTime.Now >= nextUpdate)
                                            {
                                                prog.Update("ImportGeocaches", count, index);
                                                nextUpdate = DateTime.Now.AddSeconds(1);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Core.ApplicationData.Instance.Logger.AddLog(this, Core.Logger.Level.Error, "Version not supported");
                                }
                            }
                            else
                            {
                                Core.ApplicationData.Instance.Logger.AddLog(this, Core.Logger.Level.Error, "Wrong file signature");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Core.ApplicationData.Instance.Logger.AddLog(this, e);
                    }
                });
            }
        }

        public List<int> ReadIntegerArray(BinaryReader fs)
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
