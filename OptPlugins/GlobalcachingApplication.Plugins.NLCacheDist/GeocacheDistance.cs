using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.Xml;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.NLCacheDist
{
    public class GeocacheDistance : Utils.BasePlugin.BaseImportFilter
    {
        public const string ACTION_IMPORT = "Import NL Geocache distance";
        public const string CUSTOM_ATTRIBUTE = "Afstand";

        public const string STR_IMPORT = "Import NL Geocache distance...";
        public const string STR_DOWNLOAD = "Downloading data...";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_IMPORT);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_DOWNLOAD));

            return await base.InitializeAsync(core);
        }

        public async override Task<bool> ActionAsync(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_IMPORT)
                {
                    await PerformImport();
                }
            }
            return result;
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_IMPORT;
            }
        }

        protected override void ImportMethod()
        {
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_IMPORT, STR_DOWNLOAD, 1, 0))
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
                                Core.Geocaches.AddCustomAttribute(CUSTOM_ATTRIBUTE);
                                foreach (XmlNode n in nl)
                                {
                                    var gc = Utils.DataAccess.GetGeocache(Core.Geocaches, n.Attributes["code"].InnerText);
                                    if (gc != null)
                                    {
                                        string dist = Utils.Conversion.StringToDouble(n.Attributes["dist"].InnerText).ToString("000.0");
                                        gc.SetCustomAttribute(CUSTOM_ATTRIBUTE, dist);
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }

    }
}
