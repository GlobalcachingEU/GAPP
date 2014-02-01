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
                        wc.DownloadFile("http://www.globalcaching.eu/service/cachedistance.aspx?country=Netherlands&prefix=GC", tmp.Path);

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
    }
}
