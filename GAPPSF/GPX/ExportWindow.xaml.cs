using GAPPSF.Commands;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace GAPPSF.GPX
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        public class GeocacheEntryInfo
        {
            public Core.Data.Geocache GC { get; set; }
            public int FileLen { get; set; }
        }

        private List<Core.Data.Geocache> _gcList;
        private Devices.GarminMassStorage _garminStorage = null;
        public ObservableCollection<Devices.GarminMassStorage.DeviceInfo> GarminDevices { get; set; }
        public Devices.GarminMassStorage.DeviceInfo SelectedGarminDevice { get; set; }
        public List<string> GPXVersions { get; set; }

        public ExportWindow()
        {
            InitializeComponent();
        }

        public ExportWindow(List<Core.Data.Geocache> gcList):this()
        {
            _gcList = gcList;
            GarminDevices = new ObservableCollection<Devices.GarminMassStorage.DeviceInfo>();
            GPXVersions = new List<string>();
            _garminStorage = new Devices.GarminMassStorage();
            _garminStorage.DeviceAddedEvent += _garminStorage_DeviceAddedEvent;
            _garminStorage.DeviceRemovedEvent += _garminStorage_DeviceRemovedEvent;
            GPXVersions.Add(Export.V100.ToString());
            GPXVersions.Add(Export.V101.ToString());
            GPXVersions.Add(Export.V102.ToString());
            DataContext = this;
        }

        void _garminStorage_DeviceRemovedEvent(object sender, Devices.GarminMassStorage.DeviceInfoEventArgs e)
        {
            GarminDevices.Remove(e.Device);
        }

        void _garminStorage_DeviceAddedEvent(object sender, Devices.GarminMassStorage.DeviceInfoEventArgs e)
        {
            GarminDevices.Add(e.Device);
        }

        private AsyncDelegateCommand _exportCommand;
        public AsyncDelegateCommand ExportCommand
        {
            get
            {
                if (_exportCommand==null)
                {
                    _exportCommand = new AsyncDelegateCommand(param=>PerformExportAsync(), param=> canExport());
                }
                return _exportCommand;
            }
        }
        private async Task PerformExportAsync()
        {
            await Task.Run(() =>
                {
                    if (Core.Settings.Default.GPXExportGGZ)
                    {
                        exportToGGZ();
                    }
                    else
                    {
                        exportToGPX();
                    }
                });
        }

        private void exportToGPX()
        {
            string filename;
            if (Core.Settings.Default.GPXFileName.ToLower().EndsWith(".gpx"))
            {
                filename = Core.Settings.Default.GPXFileName;
            }
            else if (Core.Settings.Default.GPXFileName.ToLower().EndsWith("."))
            {
                filename = string.Concat(Core.Settings.Default.GPXFileName, "gpx");
            }
            else
            {
                filename = string.Concat(Core.Settings.Default.GPXFileName, ".gpx");
            }
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock("ExportingGPX", "CreatingFile", _gcList.Count, 0))
            {
                using (System.IO.TemporaryFile gpxFile = new System.IO.TemporaryFile(false))
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(gpxFile.Path, false, Encoding.UTF8))
                    {
                        GPX.Export gpxGenerator = new Export(_gcList, Version.Parse(Core.Settings.Default.GPXVersion));

                        DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                        //generate header
                        sw.Write(gpxGenerator.Start());
                        //preserve mem and do for each cache the export
                        for (int i = 0; i < gpxGenerator.Count; i++)
                        {
                            sw.WriteLine(gpxGenerator.Next());
                            if (Core.Settings.Default.GPXAddChildWaypoints)
                            {
                                string s = gpxGenerator.WaypointData();
                                if (!string.IsNullOrEmpty(s))
                                {
                                    sw.WriteLine(s);
                                }
                            }
                            if (DateTime.Now >= nextUpdate)
                            {
                                progress.Update("CreatingFile", gpxGenerator.Count, i + 1);
                                nextUpdate = DateTime.Now.AddSeconds(1);
                            }
                        }
                        //finalize
                        sw.Write(gpxGenerator.Finish());
                    }

                    if (Core.Settings.Default.GPXTargetDevice == TargetDevice.Garmin)
                    {
                        progress.Update("CopyingFileToDevice", 1, 0);
                        System.IO.File.Copy(gpxFile.Path, System.IO.Path.Combine(new string[] { SelectedGarminDevice.DriveName, "garmin", "gpx", filename }), true);
                    }
                    else
                    {
                        System.IO.File.Copy(gpxFile.Path, System.IO.Path.Combine(Core.Settings.Default.GPXTargetFolder, filename), true);
                    }
                }
            }
        }

        private void exportToGGZ()
        {
            string filename;
            if (Core.Settings.Default.GPXFileName.ToLower().EndsWith(".ggz"))
            {
                filename = Core.Settings.Default.GPXFileName;
            }
            else if (Core.Settings.Default.GPXFileName.ToLower().EndsWith("."))
            {
                filename = string.Concat(Core.Settings.Default.GPXFileName, "ggz");
            }
            else
            {
                filename = string.Concat(Core.Settings.Default.GPXFileName, ".ggz");
            }
            DateTime dt = DateTime.Now.AddSeconds(2);
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock("ExportingGPX", "CreatingFile", _gcList.Count, 0))
            {
                using (System.IO.TemporaryFile gpxFile = new System.IO.TemporaryFile(false))
                {
                    using (ZipOutputStream s = new ZipOutputStream(System.IO.File.Create(gpxFile.Path)))
                    {
                        s.SetLevel(9); // 0-9, 9 being the highest compression
                        s.UseZip64 = UseZip64.Off;

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
                        GPX.Export gpxGenerator = new Export(_gcList, Version.Parse(Core.Settings.Default.GPXVersion));

                        while (_gcList.Count > 0)
                        {
                            XmlElement elFile = doc.CreateElement("file");
                            root.AppendChild(elFile);

                            el = doc.CreateElement("name");
                            txt = doc.CreateTextNode(string.Format("{0}_{1}.gpx", System.IO.Path.GetFileNameWithoutExtension(filename), fileIndex));
                            el.AppendChild(txt);
                            elFile.AppendChild(el);

                            XmlElement elCRC = doc.CreateElement("crc");
                            elFile.AppendChild(elCRC);

                            el = doc.CreateElement("time");
                            txt = doc.CreateTextNode(string.Format("{0}Z", DateTime.Now.ToUniversalTime().ToString("s")));
                            el.AppendChild(txt);
                            elFile.AppendChild(el);

                            //create GPX wpt entries until max size is reached
                            List<Core.Data.Geocache> gpxBatchList = new List<Core.Data.Geocache>();
                            List<GeocacheEntryInfo> geiList = new List<GeocacheEntryInfo>();
                            geocacheIndex = 0;
                            gpxGenerator.SetGeocacheList(_gcList);
                            StringBuilder sb = new StringBuilder();
                            gpxGenerator.Start();
                            while (sb.Length < gpxSizeLimit && geocacheIndex < gpxGenerator.Count)
                            {
                                gpxBatchList.Add(_gcList[geocacheIndex]);
                                string gpxText = gpxGenerator.Next();

                                GeocacheEntryInfo gei = new GeocacheEntryInfo();
                                gei.GC = _gcList[geocacheIndex];
                                gei.FileLen = System.Text.UTF8Encoding.UTF8.GetBytes(gpxText).Length + 2;
                                geiList.Add(gei);

                                sb.AppendLine(gpxText);

                                totalProcessed++;
                                geocacheIndex++;

                                if (DateTime.Now >= dt)
                                {
                                    progress.Update("CreatingFile", totalGeocaches, totalProcessed);
                                    dt = DateTime.Now.AddSeconds(2);
                                }
                            }
                            sb.AppendLine(gpxGenerator.Finish());
                            //insert gpx header
                            gpxGenerator.SetGeocacheList(gpxBatchList);
                            string gpxHeader = gpxGenerator.Start();
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
                            string fn = string.Format("data/{0}_{1}.gpx", System.IO.Path.GetFileNameWithoutExtension(filename), fileIndex);
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
                            for (int i = 0; i < geiList.Count; i++)
                            {
                                GeocacheEntryInfo gei = geiList[i];

                                XmlElement chgEl = doc.CreateElement("gch");
                                elFile.AppendChild(chgEl);

                                el = doc.CreateElement("code");
                                txt = doc.CreateTextNode(gei.GC.Code ?? "");
                                el.AppendChild(txt);
                                chgEl.AppendChild(el);

                                el = doc.CreateElement("name");
                                txt = doc.CreateTextNode(gpxGenerator.validateXml(gei.GC.Name ?? ""));
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

                                el = doc.CreateElement("size");
                                switch (gei.GC.Container.ID)
                                {
                                    case 1:
                                        txt = doc.CreateTextNode("3.0");
                                        break;
                                    case 2:
                                        txt = doc.CreateTextNode("1.0");
                                        break;
                                    case 3:
                                        txt = doc.CreateTextNode("4.0");
                                        break;
                                    case 4:
                                        txt = doc.CreateTextNode("5.0");
                                        break;
                                    case 5:
                                        txt = doc.CreateTextNode("1.0");
                                        break;
                                    case 6:
                                        txt = doc.CreateTextNode("1.0");
                                        break;
                                    case 8:
                                        txt = doc.CreateTextNode("2.0");
                                        break;
                                    default:
                                        txt = doc.CreateTextNode("3.0");
                                        break;
                                }
                                el.AppendChild(txt);
                                ratingsEl.AppendChild(el);


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

                    if (Core.Settings.Default.GPXTargetDevice == TargetDevice.Garmin)
                    {
                        progress.Update("CopyingFileToDevice", 1, 0);
                        string p = System.IO.Path.Combine(new string[] { SelectedGarminDevice.DriveName, "garmin", "ggz" });
                        if (!System.IO.Directory.Exists(p))
                        {
                            System.IO.Directory.CreateDirectory(p);
                        }
                        System.IO.File.Copy(gpxFile.Path, System.IO.Path.Combine(p, filename), true);
                    }
                    else
                    {
                        System.IO.File.Copy(gpxFile.Path, System.IO.Path.Combine(Core.Settings.Default.GPXTargetFolder, filename), true);
                    }
                }
            }
        }

        private bool canExport()
        {
            bool result = false;
            if (!string.IsNullOrEmpty(Core.Settings.Default.GPXFileName))
            {
                if (Core.Settings.Default.GPXTargetDevice == TargetDevice.Folder)
                {
                    if (!string.IsNullOrEmpty(Core.Settings.Default.GPXTargetFolder))
                    {
                        if (System.IO.Directory.Exists(Core.Settings.Default.GPXTargetFolder))
                        {
                            result = true;
                        }
                    }
                }
                else
                {
                    if (SelectedGarminDevice != null)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_garminStorage!=null)
            {
                _garminStorage.Dispose();
                _garminStorage = null;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //select folder
            var dlg = new GAPPSF.Dialogs.FolderPickerDialog();
            if (dlg.ShowDialog()==true)
            {
                Core.Settings.Default.GPXTargetFolder = dlg.SelectedPath;
                Core.Settings.Default.GPXTargetDevice = TargetDevice.Folder;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Core.Settings.Default.GPXTargetDevice = TargetDevice.Garmin;
        }

    }
}
