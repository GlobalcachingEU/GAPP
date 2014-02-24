using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.Xml;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;

namespace GlobalcachingApplication.Plugins.ExportGPX
{
    public class GgzExport : Utils.BasePlugin.BaseExportFilter
    {
        public const string STR_NOGEOCACHESELECTED = "No geocache selected for export";
        public const string STR_ERROR = "Error";
        public const string STR_EXPORTINGGPX = "Exporting GPX...";
        public const string STR_CREATINGFILE = "Creating file...";

        public const string ACTION_EXPORT_ALL = "Export GGZ|All";
        public const string ACTION_EXPORT_SELECTED = "Export GGZ|Selected";
        public const string ACTION_EXPORT_ACTIVE = "Export GGZ|Active";

        private string _filename = null;
        private Utils.GPXGenerator _gpxGenerator = null;
        private List<Framework.Data.Geocache> _gcList = null;

        public class GeocacheEntryInfo
        {
            public Framework.Data.Geocache GC { get; set; }
            public int FileLen { get; set; }
        }

        public override string FriendlyName
        {
            get { return "Export GGZ"; }
        }

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            AddAction(ACTION_EXPORT_ALL);
            AddAction(ACTION_EXPORT_SELECTED);
            AddAction(ACTION_EXPORT_ACTIVE);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_EXPORTINGGPX));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CREATINGFILE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_GPXVERSION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_ZIPFILE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_ADDWPTTODESCR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_USEHINTSDESCR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_INCLNOTES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_ADDWAYPOINTS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_EXTRACOORDNAMEPREFIX));

            return base.Initialize(core);
        }

        public void ExportToGGZ(string filename, List<Framework.Data.Geocache> gcList, Utils.GPXGenerator gpxGenerator)
        {
            _filename = filename;
            _gcList = gcList;
            _gpxGenerator = gpxGenerator;
            PerformExport();
            _gcList = null;
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_EXPORT_ALL || action == ACTION_EXPORT_SELECTED || action == ACTION_EXPORT_ACTIVE)
                {
                    if (action == ACTION_EXPORT_ALL)
                    {
                        _gcList = (from Framework.Data.Geocache a in Core.Geocaches select a).ToList();
                    }
                    else if (action == ACTION_EXPORT_SELECTED)
                    {
                        _gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                    }
                    else
                    {
                        if (Core.ActiveGeocache != null)
                        {
                            _gcList = new List<Framework.Data.Geocache>();
                            _gcList.Add(Core.ActiveGeocache);
                        }
                    }
                    if (_gcList == null || _gcList.Count == 0)
                    {
                        System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                    else
                    {
                        using (System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog())
                        {
                            dlg.FileName = "";
                            dlg.Filter = "*.ggz|*.ggz";
                            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                _filename = dlg.FileName;
                                _gpxGenerator = new Utils.GPXGenerator(Core, _gcList, string.IsNullOrEmpty(Properties.Settings.Default.GPXVersionStr) ? Utils.GPXGenerator.V101 : Version.Parse(Properties.Settings.Default.GPXVersionStr));
                                _gpxGenerator.MaxNameLength = Properties.Settings.Default.MaxGeocacheNameLength;
                                _gpxGenerator.MinStartOfname = Properties.Settings.Default.MinStartOfGeocacheName;
                                _gpxGenerator.UseNameForGCCode = Properties.Settings.Default.UseNameAndNotCode;
                                _gpxGenerator.AddAdditionWaypointsToDescription = Properties.Settings.Default.AddWaypointsToDescription;
                                _gpxGenerator.UseHintsForDescription = Properties.Settings.Default.UseHintsForDescription;
                                _gpxGenerator.AddFieldnotesToDescription = Properties.Settings.Default.AddFieldnotesToDescription;
                                _gpxGenerator.ExtraCoordPrefix = Properties.Settings.Default.CorrectedNamePrefix;
                                PerformExport();
                            }
                        }
                    }
                    _gcList = null;
                }
            }
            return result;
        }

        protected override void ExportMethod()
        {
            using (ZipOutputStream s = new ZipOutputStream(System.IO.File.Create(_filename)))
            {
                s.SetLevel(9); // 0-9, 9 being the highest compression
                s.UseZip64 = UseZip64.Off;

                DateTime dt = DateTime.Now.AddSeconds(2);
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_EXPORTINGGPX, STR_CREATINGFILE, _gcList.Count, 0))
                {
                    int totalGeocaches = _gcList.Count;
                    int totalProcessed = 0;
                    int fileIndex = 1;
                    int geocacheIndex = 0;
                    int gpxSizeLimit = 4500000; //appr. 4.5MB

                    XmlDocument doc = new XmlDocument();
                    XmlDeclaration pi = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                    doc.InsertBefore(pi, doc.DocumentElement);
                    XmlElement root = doc.CreateElement("ggz");
                    doc.AppendChild(root);
                    XmlAttribute attr = doc.CreateAttribute("xmlns");
                    XmlText txt = doc.CreateTextNode("http://www.opencaching.com/xmlschemas/ggz/1/0");
                    attr.AppendChild(txt);
                    root.Attributes.Append(attr);

                    XmlElement el = doc.CreateElement("time");
                    txt = doc.CreateTextNode(string.Format("{0}Z", DateTime.Now.ToUniversalTime().ToString("s")));
                    el.AppendChild(txt);
                    root.AppendChild(el);

                    //Utils.Crc16 crc16 = new Utils.Crc16();

                    while (_gcList.Count>0)
                    {
                        XmlElement elFile = doc.CreateElement("file");
                        root.AppendChild(elFile);

                        el = doc.CreateElement("name");
                        txt = doc.CreateTextNode(string.Format("{0}_{1}.gpx", System.IO.Path.GetFileNameWithoutExtension(_filename), fileIndex));
                        el.AppendChild(txt);
                        elFile.AppendChild(el);

                        XmlElement elCRC = doc.CreateElement("crc");
                        elFile.AppendChild(elCRC);

                        el = doc.CreateElement("time");
                        txt = doc.CreateTextNode(string.Format("{0}Z", DateTime.Now.ToUniversalTime().ToString("s")));
                        el.AppendChild(txt);
                        elFile.AppendChild(el);

                        //create GPX wpt entries until max size is reached
                        List<Framework.Data.Geocache> gpxBatchList = new List<Framework.Data.Geocache>();
                        List<GeocacheEntryInfo> geiList = new List<GeocacheEntryInfo>();
                        geocacheIndex = 0;
                        _gpxGenerator.SetGeocacheList(_gcList);
                        StringBuilder sb = new StringBuilder();
                        _gpxGenerator.Start();
                        while (sb.Length < gpxSizeLimit && geocacheIndex < _gpxGenerator.Count)
                        {
                            gpxBatchList.Add(_gcList[geocacheIndex]);
                            string gpxText = _gpxGenerator.Next();

                            GeocacheEntryInfo gei = new GeocacheEntryInfo();
                            gei.GC = _gcList[geocacheIndex];
                            gei.FileLen = System.Text.UTF8Encoding.UTF8.GetBytes(gpxText).Length + 2;
                            geiList.Add(gei);

                            sb.AppendLine(gpxText);

                            totalProcessed++;
                            geocacheIndex++;

                            if (DateTime.Now >= dt)
                            {
                                progress.UpdateProgress(STR_EXPORTINGGPX, STR_CREATINGFILE, totalGeocaches, totalProcessed);
                                dt = DateTime.Now.AddSeconds(2);
                            }
                        }
                        sb.AppendLine(_gpxGenerator.Finish());
                        //insert gpx header
                        _gpxGenerator.SetGeocacheList(gpxBatchList);
                        string gpxHeader = _gpxGenerator.Start();
                        sb.Insert(0, gpxHeader);
                        _gcList.RemoveRange(0, gpxBatchList.Count);

                        //add gpx to zip
                        byte[] data;
                        using (System.IO.TemporaryFile tmp = new System.IO.TemporaryFile(true))
                        {
                            using (System.IO.StreamWriter sw = System.IO.File.CreateText(tmp.Path))
                            {
                                sw.Write(sb.ToString());
                            }
                            data = File.ReadAllBytes(tmp.Path);
                        }
                        string fn = string.Format("data/{0}_{1}.gpx", System.IO.Path.GetFileNameWithoutExtension(_filename), fileIndex);
                        ZipEntry entry = new ZipEntry(fn);
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);
                        s.Write(data, 0, data.Length);

                        Crc32 crc = new Crc32();
                        crc.Update(data);
                        //txt = doc.CreateTextNode(crc16.ComputeChecksum(data).ToString("X8"));
                        txt = doc.CreateTextNode(crc.Value.ToString("X8"));
                        elCRC.AppendChild(txt);

                        int curPos = System.Text.UTF8Encoding.UTF8.GetBytes(gpxHeader).Length;
                        for (int i = 0; i < geiList.Count; i++ )
                        {
                            GeocacheEntryInfo gei = geiList[i];

                            XmlElement chgEl = doc.CreateElement("gch");
                            elFile.AppendChild(chgEl);

                            el = doc.CreateElement("code");
                            txt = doc.CreateTextNode(gei.GC.Code ?? "");
                            el.AppendChild(txt);
                            chgEl.AppendChild(el);

                            el = doc.CreateElement("name");
                            txt = doc.CreateTextNode(_gpxGenerator.validateXml(gei.GC.Name ?? ""));
                            el.AppendChild(txt);
                            chgEl.AppendChild(el);

                            el = doc.CreateElement("type");
                            txt = doc.CreateTextNode(gei.GC.GeocacheType.GPXTag);
                            el.AppendChild(txt);
                            chgEl.AppendChild(el);

                            el = doc.CreateElement("lat");
                            if (gei.GC.ContainsCustomLatLon)
                            {
                                txt = doc.CreateTextNode(gei.GC.CustomLat.ToString().Replace(',', '.'));
                            }
                            else
                            {
                                txt = doc.CreateTextNode(gei.GC.Lat.ToString().Replace(',', '.'));
                            }
                            el.AppendChild(txt);
                            chgEl.AppendChild(el);

                            el = doc.CreateElement("lon");
                            if (gei.GC.ContainsCustomLatLon)
                            {
                                txt = doc.CreateTextNode(gei.GC.CustomLon.ToString().Replace(',', '.'));
                            }
                            else
                            {
                                txt = doc.CreateTextNode(gei.GC.Lon.ToString().Replace(',', '.'));
                            }
                            el.AppendChild(txt);
                            chgEl.AppendChild(el);

                            el = doc.CreateElement("file_pos");
                            txt = doc.CreateTextNode(curPos.ToString());
                            curPos += gei.FileLen;
                            el.AppendChild(txt);
                            chgEl.AppendChild(el);

                            el = doc.CreateElement("file_len");
                            txt = doc.CreateTextNode(gei.FileLen.ToString());
                            el.AppendChild(txt);
                            chgEl.AppendChild(el);

                            XmlElement ratingsEl = doc.CreateElement("ratings");
                            chgEl.AppendChild(ratingsEl);

                            el = doc.CreateElement("awesomeness");
                            txt = doc.CreateTextNode("3.0");
                            el.AppendChild(txt);
                            ratingsEl.AppendChild(el);

                            el = doc.CreateElement("difficulty");
                            txt = doc.CreateTextNode(gei.GC.Difficulty.ToString("0.#").Replace(',', '.'));
                            el.AppendChild(txt);
                            ratingsEl.AppendChild(el);

                            //size=1 should not be used
                            //according to garmin (forum) the tag should be left out in case the size=1
                            if (gei.GC.Container.ID == 1 ||
                                gei.GC.Container.ID == 5 ||
                                gei.GC.Container.ID == 6)
                            {
                                //no tag
                            }
                            else
                            {
                                el = doc.CreateElement("size");
                                switch (gei.GC.Container.ID)
                                {
                                    case 2:
                                        txt = doc.CreateTextNode("2.0");
                                        break;
                                    case 3:
                                        txt = doc.CreateTextNode("4.0");
                                        break;
                                    case 4:
                                        txt = doc.CreateTextNode("5.0");
                                        break;
                                    case 8:
                                        txt = doc.CreateTextNode("3.0");
                                        break;
                                    default:
                                        txt = doc.CreateTextNode("3.0");
                                        break;
                                }
                                el.AppendChild(txt);
                                ratingsEl.AppendChild(el);
                            }

                            el = doc.CreateElement("terrain");
                            txt = doc.CreateTextNode(gei.GC.Terrain.ToString("0.#").Replace(',', '.'));
                            el.AppendChild(txt);
                            ratingsEl.AppendChild(el);

                            if (gei.GC.Found)
                            {
                                el = doc.CreateElement("found");
                                txt = doc.CreateTextNode("true");
                                el.AppendChild(txt);
                                chgEl.AppendChild(el);
                            }
                        }

                        fileIndex++;
                    }

                    //add index file
                    // index\com\garmin\geocaches\v0\index.xml
                    /*
                    <gch>
                      <code>GC12345</code>
                      <name>Cache name</name>
                      <type>Traditional Cache</type>
                      <lat>33.550217</lat>
                      <lon>-117.660617</lon>
                      <file_pos>5875</file_pos>
                      <file_len>5783</file_len>
                      <ratings>
                         <awesomeness>3.0</awesomeness>
                         <difficulty>1.5</difficulty>
                         <size>5.0</size>
                         <terrain>1.5</terrain>
                      </ratings>
                      <found>true</found>
                    </gch>
                     * 
                     1 = Nano (not supported, unfortunately, by GC.com yet)
                    2 = Micro
                    3 = Small
                    4 = Regular
                    5 = Large 
                     * 
                     */
                    using (System.IO.TemporaryFile tmp = new System.IO.TemporaryFile(true))
                    {
                        using (TextWriter sw = new StreamWriter(tmp.Path, false, Encoding.UTF8)) //Set encoding
                        {
                            doc.Save(sw);
                        }
                        byte[] data = File.ReadAllBytes(tmp.Path);
                        ZipEntry entry = new ZipEntry("index/com/garmin/geocaches/v0/index.xml");
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);
                        s.Write(data, 0, data.Length);
                    }

                    s.Finish();
                    s.Close();
                }
            }
        }
    }
}
