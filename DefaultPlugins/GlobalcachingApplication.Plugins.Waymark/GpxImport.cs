using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;

namespace GlobalcachingApplication.Plugins.Waymark
{
    public class GpxImport: Utils.BasePlugin.BaseImportFilter
    {
        public const string STR_IMPORTING = "Importing GPX...";
        public const string STR_IMPORTINGDATA = "Importing file...";
        public const string STR_IMPORTINGGEOCACHES = "Importing geocaches...";
        public const string STR_IMPORTINGLOGS = "Importing logs...";
        public const string STR_IMPORTINGLOGIMAGES = "Importing log images...";
        public const string STR_IMPORTINGWAYPOINTS = "Importing waypoints...";


        protected const string ACTION_IMPORT = "Import Waymark GPX";
        private string[] _filenames = null;

        public override string FriendlyName
        {
            get { return "Import Waymark GPX"; }
        }

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_IMPORT);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGDATA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGGEOCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGLOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGLOGIMAGES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGWAYPOINTS));

            return base.Initialize(core);
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_IMPORT)
                {
                    using (System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog())
                    {
                        dlg.FileName = "";
                        dlg.Filter = "*.zip|*.zip|*.gpx|*.gpx";
                        dlg.Multiselect = true;
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            _filenames = dlg.FileNames;
                            PerformImport();
                        }
                    }
                }
            }
            return result;
        }

        protected override void ImportMethod()
        {
            using (Utils.ProgressBlock fixpr = new Utils.ProgressBlock(this, STR_IMPORTING, STR_IMPORTINGDATA, _filenames.Length, 0))
            {
                for (int fileindex = 0; fileindex < _filenames.Length; fileindex++)
                {
                    List<Framework.Data.Geocache> res = ProcessWaymarkComGPXFile(_filenames[fileindex]);
                    if (res != null && res.Count > 0)
                    {
                        using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_IMPORTING, STR_IMPORTINGGEOCACHES, res.Count, 0))
                        {
                            int index = 0;
                            int procStep = 0;
                            foreach (Framework.Data.Geocache gc in res)
                            {
                                AddGeocache(gc, Framework.Data.Geocache.V1);
                                index++;
                                procStep++;
                                if (procStep >= 100)
                                {
                                    progress.UpdateProgress(STR_IMPORTING, STR_IMPORTINGGEOCACHES, res.Count, index);
                                    procStep = 0;
                                }
                            }
                        }
                    }
                    fixpr.UpdateProgress(STR_IMPORTING, STR_IMPORTINGDATA, _filenames.Length, fileindex + 1);
                }
            }
        }


        public List<Framework.Data.Geocache> ProcessWaymarkComGPXFile(string filename)
        {
            List<Framework.Data.Geocache> result = new List<Framework.Data.Geocache>();
            //zip or gpx?
            try
            {
                if (filename.ToLower().EndsWith(".gpx"))
                {
                    result.AddRange( ProcessWaymarkComGPXCaches(System.IO.File.ReadAllText(filename)));
                }
                else
                {
                    //zip - containts waypoints and caches
                    using (var fs = System.IO.File.OpenRead(filename))
                    using (ZipInputStream s = new ZipInputStream(fs))
                    {
                        ZipEntry theEntry = s.GetNextEntry();
                        byte[] data = new byte[1024];
                        while (theEntry != null)
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

                            result.AddRange(ProcessWaymarkComGPXCaches(sb.ToString()));

                            theEntry = s.GetNextEntry();
                        }
                    }
                }
            }
            catch
            {
                result = null;
            }
            return result;
        }

        public string SafeAttributeInnerText(XmlNode n, string attr, string defaultValue)
        {
            if (n != null)
            {
                XmlAttribute res = n.Attributes[attr];
                if (res != null)
                {
                    return res.InnerText;
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }
        public string SafeInnerText(XmlNode n, string defaultValue)
        {
            if (n != null)
            {
                return n.InnerText;
            }
            else
            {
                return defaultValue;
            }
        }

        public List<Framework.Data.Geocache> ProcessWaymarkComGPXCaches(string gpxDoc)
        {
            List<Framework.Data.Geocache> result = new List<Framework.Data.Geocache>();
            try
            {
                string firstPart;
                if (gpxDoc.Length < 2000)
                {
                    firstPart = gpxDoc;
                }
                else
                {
                    firstPart = gpxDoc.Substring(0, 2000);
                }
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(gpxDoc);
                XmlElement root = doc.DocumentElement;
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);

                nsmgr.AddNamespace("x", "http://www.topografix.com/GPX/1/0");

                DateTime gpxDataTime = DateTime.Parse(root.SelectSingleNode("x:time", nsmgr).InnerText);

                XmlNodeList wps = root.SelectNodes("x:wpt", nsmgr);
                if (wps != null)
                {
                    Version V102 = new Version(1, 0, 2);
                    foreach (XmlNode wp in wps)
                    {
                        Framework.Data.Geocache gc = new Framework.Data.Geocache();

                        gc.Lat = Utils.Conversion.StringToDouble(SafeAttributeInnerText(wp, "lat", "0.0"));
                        gc.Lon = Utils.Conversion.StringToDouble(SafeAttributeInnerText(wp, "lon", "0.0"));
                        gc.Code = wp.SelectSingleNode("x:name", nsmgr).InnerText;
                        gc.Title = wp.SelectSingleNode("x:urlname", nsmgr).InnerText;
                        gc.DataFromDate = gpxDataTime;
                        gc.PublishedTime = DateTime.MinValue;
                        gc.Url = SafeInnerText(wp.SelectSingleNode("x:url", nsmgr), "");
                        gc.Available = true;
                        gc.Archived = false;
                        gc.Country = "";
                        gc.State = "";
                        gc.ID = gc.Code;
                        gc.OwnerId = "";
                        gc.GeocacheType = Utils.DataAccess.GetGeocacheType(Core.GeocacheTypes, 63542);
                        gc.Container = Utils.DataAccess.GetGeocacheContainer(Core.GeocacheContainers, -1);
                        gc.Favorites = 0;
                        gc.MemberOnly = false;
                        gc.CustomCoords = false;
                        gc.PersonaleNote = "";
                        gc.PlacedBy = "";
                        gc.Owner = "";
                        gc.Terrain = 1;
                        gc.Difficulty = 1;
                        gc.ShortDescription = SafeInnerText(wp.SelectSingleNode("x:desc", nsmgr), "");
                        gc.ShortDescriptionInHtml = false;
                        gc.LongDescription = "";
                        gc.LongDescriptionInHtml = false;
                        gc.EncodedHints = "";

                        Utils.Calculus.SetDistanceAndAngleGeocacheFromLocation(gc, Core.CenterLocation);

                        result.Add(gc);
                    }
                }
            }
            catch
            {
            }
            return result;
        }

    }
}
