using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GAPPSF.GlobalcachingEU
{
    public class Import
    {
        public async Task ImportGeocacheDistanceAsync(Core.Storage.Database db)
        {
            using (Utils.DataUpdater upd = new Utils.DataUpdater(db))
            {
                await Task.Run(new Action(() => ImportGeocacheDistance(db)));
            }
        }

        public void ImportGeocacheDistance(Core.Storage.Database db)
        {
            try
            {
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock("ImportNLGeocacheDistance", "DownloadingData", 1, 0))
                {
                    using (System.IO.TemporaryFile tmp = new System.IO.TemporaryFile(true))
                    using (System.Net.WebClient wc = new System.Net.WebClient())
                    {
                        wc.DownloadFile(string.Format("https://www.4geocaching.eu/service/cachedistance.aspx?country=Netherlands&prefix=GC&token{0}", System.Web.HttpUtility.UrlEncode(Core.Settings.Default.LiveAPIToken ?? "")), tmp.Path);

                        using (var fs = System.IO.File.OpenRead(tmp.Path))
                        using (ZipInputStream s = new ZipInputStream(fs))
                        {
                            ZipEntry theEntry = s.GetNextEntry();
                            byte[] data = new byte[1024];
                            if (theEntry != null)
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

                                XmlDocument doc = new XmlDocument();
                                doc.LoadXml(sb.ToString());
                                XmlElement root = doc.DocumentElement;
                                XmlNodeList nl = root.SelectNodes("wp");
                                if (nl != null)
                                {
                                    progress.Update("SavingGeocaches", nl.Count, 0);
                                    DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                                    int index = 0;
                                    foreach (XmlNode n in nl)
                                    {
                                        var gc = db.GeocacheCollection.GetGeocache(n.Attributes["code"].InnerText);
                                        if (gc != null)
                                        {
                                            gc.GeocacheDistance = Utils.Conversion.StringToDouble(n.Attributes["dist"].InnerText);
                                        }
                                        else
                                        {
                                            Core.Settings.Default.SetGeocacheDistance(n.Attributes["code"].InnerText, Utils.Conversion.StringToDouble(n.Attributes["dist"].InnerText));
                                        }
                                        index++;
                                        if (DateTime.Now>=nextUpdate)
                                        {
                                            progress.Update("SavingGeocaches", nl.Count, index);
                                            nextUpdate = DateTime.Now.AddSeconds(1);
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


        public async Task ImportFavoritesAsync(Core.Storage.Database db)
        {
            using (Utils.DataUpdater upd = new Utils.DataUpdater(db))
            {
                await Task.Run(new Action(() => ImportFavorites(db)));
            }
        }

        public void ImportFavorites(Core.Storage.Database db)
        {
            try
            {
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock("GetFavoritesFromGlobalcaching", "DownloadingData", 1, 0))
                {

                    using (System.Net.WebClient wc = new System.Net.WebClient())
                    {
                        string doc = wc.DownloadString(string.Format("https://www.4geocaching.eu/Service/CacheFavorites.aspx?token={0}", System.Web.HttpUtility.UrlEncode(Core.Settings.Default.LiveAPIToken ?? "")));
                        if (doc != null)
                        {
                            string[] lines = doc.Replace("\r", "").Split(new char[] { '\n' });
                            progress.Update("SavingGeocaches", lines.Length, 0);
                            Core.Data.Geocache gc;
                            char[] sep = new char[] { ',' };
                            string[] parts;
                            DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                            int index = 0;
                            foreach (string s in lines)
                            {
                                parts = s.Split(sep);
                                if (parts.Length > 0)
                                {
                                    gc = db.GeocacheCollection.GetGeocache(parts[0]);
                                    if (gc != null)
                                    {
                                        gc.Favorites = int.Parse(parts[1]);
                                    }
                                }
                                index++;
                                if (DateTime.Now >= nextUpdate)
                                {
                                    progress.Update("SavingGeocaches", lines.Length, index);
                                    nextUpdate = DateTime.Now.AddSeconds(1);
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
        }


        private void updateGeocachesFromGlobalcachingEU(Core.Storage.Database db, string country, List<string> missingGcList)
        {
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                string doc = wc.DownloadString(string.Format("https://www.4geocaching.eu/Service/GeocacheCodes.aspx?country={0}&token={1}", country, System.Web.HttpUtility.UrlEncode(Core.Settings.Default.LiveAPIToken ?? "")));
                if (doc != null)
                {
                    string[] lines = doc.Replace("\r", "").Split(new char[] { '\n' });
                    Core.Data.Geocache gc;
                    char[] sep = new char[] { ',' };
                    string[] parts;
                    foreach (string s in lines)
                    {
                        parts = s.Split(sep);
                        if (parts.Length > 2)
                        {
                            gc = db.GeocacheCollection.GetGeocache(parts[0]);
                            if (gc != null)
                            {
                                gc.Archived = parts[1] != "0";
                                gc.Available = parts[2] != "0";
                            }
                            else if (parts[1] == "0") //add only none archived
                            {
                                missingGcList.Add(parts[0]);
                            }
                        }
                    }
                }
            }
        }

        public async Task UpdateGeocachesAsync(Core.Storage.Database db)
        {
            using (Utils.DataUpdater upd = new Utils.DataUpdater(db))
            {
                await Task.Run(new Action(() => UpdateGeocaches(db)));
            }
        }

        public void UpdateGeocaches(Core.Storage.Database db)
        {
            try
            {
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock("UpdateStatusOfGeocachesAndImportNewGeocaches", "UpdateStatusOfGeocachesAndImportNewGeocaches", 1, 0, true))
                {
                    List<string> gcList = new List<string>();
                    if (Core.Settings.Default.GlobalcachingEUNetherlands)
                    {
                        updateGeocachesFromGlobalcachingEU(db, "Netherlands", gcList);
                    }
                    if (Core.Settings.Default.GlobalcachingEUBelgium)
                    {
                        updateGeocachesFromGlobalcachingEU(db, "Belgium", gcList);
                    }
                    if (Core.Settings.Default.GlobalcachingEULuxembourg)
                    {
                        updateGeocachesFromGlobalcachingEU(db, "Luxembourg", gcList);
                    }
                    if (Core.Settings.Default.GlobalcachingEUItaly)
                    {
                        updateGeocachesFromGlobalcachingEU(db, "Italy", gcList);
                    }
                    if (Core.Settings.Default.GlobalcachingEUImportMissing)
                    {

                        if (Core.Settings.Default.GlobalcachingEUImportMissing && gcList.Count > 0)
                        {
                            Core.Settings.Default.filterIgnoredGeocacheCodes(gcList);
                        }
                        if (Core.Settings.Default.GlobalcachingEUImportMissing && gcList.Count > 0)
                        {
                            LiveAPI.Import.ImportGeocaches(db, gcList);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
        }


    }
}
