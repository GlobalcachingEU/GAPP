using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.GAPPDataExchange
{
    public class Export
    {
        public async Task ExportFile(List<Core.Data.Geocache> gcList)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".gde"; // Default file extension
            dlg.Filter = "GAPP Data Exchange (*.gde)|*.gde"; // Filter files by extension 

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                await ExportFile(gcList, dlg.FileName);
            }
        }

        public async Task ExportFile(List<Core.Data.Geocache> gcList, string filename)
        {
            await Task.Run(() =>
            {
                try
                {
                    DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                    using (Utils.ProgressBlock prog = new Utils.ProgressBlock("ExportToGAPPDataExchangeFile", "SavingGeocaches", gcList.Count, 0))
                    {
                        using (FileStream fs = File.OpenWrite(filename))
                        using (BinaryWriter bw = new BinaryWriter(fs))
                        {
                            bw.Write("GAPP");
                            bw.Write(BitConverter.IsLittleEndian);
                            int version = 3;
                            bw.Write(version);
                                bw.Write(gcList.Count);
                                int index = 0;
                                foreach (var gc in gcList)
                                {
                                    bw.Write(gc.Archived);
                                    WriteIntegerArray(bw, gc.AttributeIds);
                                    bw.Write(gc.Available);
                                    bw.Write(gc.City ?? "");
                                    bw.Write(gc.Code ?? "");
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
                                    if (gc.FoundDate == null)
                                    {
                                        bw.Write("");
                                    }
                                    else
                                    {
                                        bw.Write(((DateTime)gc.FoundDate).ToString("s"));
                                    }
                                    bw.Write(gc.GeocacheType.ID);
                                    bw.Write(Utils.Conversion.GetCacheIDFromCacheCode(gc.Code).ToString());
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
                                    bw.Write(gc.PersonalNote ?? "");
                                    bw.Write(gc.PlacedBy ?? "");
                                    bw.Write(((DateTime)gc.PublishedTime).ToString("s"));
                                    bw.Write(gc.Selected);
                                    bw.Write(gc.ShortDescription ?? "");
                                    bw.Write(gc.ShortDescriptionInHtml);
                                    bw.Write(gc.State ?? "");
                                    bw.Write(gc.Terrain);
                                    bw.Write(gc.Name ?? "");
                                    bw.Write(gc.Url ?? "");
                                    bw.Write(gc.DataFromDate.ToString("s"));
                                    bw.Write(gc.Locked);

                                    //logs
                                    List<Core.Data.Log> logs = Utils.DataAccess.GetLogs(gc.Database, gc.Code);
                                    bw.Write(logs.Count);
                                    foreach (var l in logs)
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

                                        List<Core.Data.LogImage> logImages = Utils.DataAccess.GetLogImages(gc.Database, l.ID);
                                        bw.Write(logImages.Count);
                                        foreach (var li in logImages)
                                        {
                                            bw.Write(li.DataFromDate.ToString("s"));
                                            bw.Write(li.ID ?? "");
                                            bw.Write(li.LogId ?? "");
                                            bw.Write(li.Name ?? "");
                                            bw.Write(li.Url ?? "");
                                        }
                                    }

                                    //waypoints
                                    List<Core.Data.Waypoint> wps = Utils.DataAccess.GetWaypointsFromGeocache(gc.Database, gc.Code);
                                    bw.Write(wps.Count);
                                    foreach (var wp in wps)
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
                                    List<Core.Data.UserWaypoint> usrwps = Utils.DataAccess.GetUserWaypointsFromGeocache(gc.Database, gc.Code);
                                    bw.Write(usrwps.Count);
                                    foreach (var wp in usrwps)
                                    {
                                        bw.Write(wp.ID);
                                        bw.Write(wp.Description ?? "");
                                        bw.Write(wp.GeocacheCode ?? "");
                                        bw.Write(wp.Lat);
                                        bw.Write(wp.Lon);
                                        bw.Write(wp.Date.ToString("s"));
                                    }

                                    index++;
                                    if (DateTime.Now>=nextUpdate)
                                    {
                                        prog.Update("SavingGeocaches", gcList.Count, index);
                                        nextUpdate = DateTime.Now.AddSeconds(1);
                                    }
                                }
                            
                        }
                    }
                }
                catch(Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
            });
        }

        public void WriteIntegerArray(BinaryWriter fs, List<int> values)
        {
            fs.Write(values.Count);
            foreach (int i in values)
            {
                fs.Write(i);
            }
        }

    }
}
