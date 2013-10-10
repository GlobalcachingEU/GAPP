using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;

namespace GlobalcachingApplication.Plugins.SHP
{
    public partial class DownloadShapefileForm : Form
    {
        public const string STR_TITLE = "Download shapefile";
        public const string STR_DOWNLOADINGLIST = "Downloading list...";
        public const string STR_DOWNLOAD = "Download";
        public const string STR_DOWNLOADINGSHAPEFILE = "Downloading shapefile...";

        private XmlDocument _shapefiles = null;
        private string _downloadUrl = null;
        private ManualResetEvent _actionReady = null;
        private Utils.BasePlugin.Plugin _plugin = null;
        private int _zipFileSize = 0;
        private bool _error = false;

        public string ShapeFilePath { get; private set; }
        public string ShapeFileFieldName { get; private set; }
        public string ShapeFileType { get; private set; }
        public string ShapeFileFormat { get; private set; }
        public string ShapeFilePrefix { get; private set; }

        public DownloadShapefileForm()
        {
            InitializeComponent();

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DOWNLOAD);

            toolStripStatusLabel1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DOWNLOADINGLIST);
            toolStripStatusLabel1.Visible = true;
        }

        public DownloadShapefileForm(Utils.BasePlugin.Plugin plugin)
            : this()
        {
            _plugin = plugin;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                _shapefiles = new XmlDocument();
                using (WebClient wc = new WebClient())
                {
                    using (MemoryStream ms = new MemoryStream(wc.DownloadData("http://application.globalcaching.eu/pkg/shapefiles/shapefiles.xml")))
                    {
                        _shapefiles.Load(ms);
                    }
                }
                XmlElement root = _shapefiles.DocumentElement;
                XmlNodeList files = root.SelectNodes("file");
                if (files != null)
                {
                    foreach (XmlNode f in files)
                    {
                        listBox1.Items.Add(f.Attributes["description"].Value);
                    }
                }
            }
            catch
            {
            }
            toolStripStatusLabel1.Visible = false;
            this.Cursor = Cursors.Default;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = listBox1.SelectedIndex >= 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            XmlElement root = _shapefiles.DocumentElement;
            XmlNodeList files = root.SelectNodes("file");
            if (files != null)
            {
                foreach (XmlNode f in files)
                {
                    if (f.Attributes["description"].Value == listBox1.SelectedItem.ToString())
                    {
                        _error = false;
                        _downloadUrl = f.Attributes["url"].Value;
                        _zipFileSize = int.Parse(f.Attributes["urlsize"].Value);
                        _actionReady = new ManualResetEvent(false);
                        _actionReady.Reset();
                        Thread thrd = new Thread(new ThreadStart(this.downloadShapefileThreadMethod));
                        thrd.Start();
                        while (!_actionReady.WaitOne(100))
                        {
                            System.Windows.Forms.Application.DoEvents();
                        }
                        thrd.Join();
                        _actionReady.Close();
                        if (!_error)
                        {
                            ShapeFileFieldName = f.Attributes["tablename"].Value;
                            ShapeFileFormat = f.Attributes["format"].Value;
                            ShapeFileType = f.Attributes["level"].Value;
                            ShapeFilePrefix = f.Attributes["prefix"].Value;
                            ShapeFilePath = Path.Combine(Properties.Settings.Default.DefaultShapeFilesFolder, string.Concat(Path.GetFileNameWithoutExtension(_downloadUrl), ".shp"));
                            DialogResult = System.Windows.Forms.DialogResult.OK;
                            Close();
                        }
                    }
                }
            }
        }

        private void downloadShapefileThreadMethod()
        {
            try
            {
                //download zip file
                using (TemporaryFile tmpFile = new TemporaryFile(false))
                {
                    using (Utils.ProgressBlock prog = new Utils.ProgressBlock(_plugin, STR_DOWNLOADINGSHAPEFILE, _downloadUrl, _zipFileSize, 0))
                    {
                        byte[] buffer = new byte[20*1024];
                        int totalRead = 0;
                        HttpWebRequest wr = (HttpWebRequest)HttpWebRequest.Create(_downloadUrl);
                        using (System.IO.BinaryReader sr = new System.IO.BinaryReader(wr.GetResponse().GetResponseStream()))
                        using (FileStream fs = File.OpenWrite(tmpFile.Path))
                        {
                            while (totalRead < _zipFileSize)
                            {
                                int bytesRead = sr.Read(buffer, 0, buffer.Length);
                                if (bytesRead > 0)
                                {
                                    fs.Write(buffer, 0, bytesRead);
                                }
                                totalRead += bytesRead;
                                prog.UpdateProgress(STR_DOWNLOADINGSHAPEFILE, _downloadUrl, _zipFileSize, totalRead);
                            }
                        }
                    }
                    UnZipFiles(tmpFile.Path, Properties.Settings.Default.DefaultShapeFilesFolder, false);
                }
            }
            catch
            {
                _error = true;
            }
            _actionReady.Set();
        }

        public static void UnZipFiles(string zipPathAndFile, string outputFolder, bool deleteZipFile)
        {
            ZipInputStream s = new ZipInputStream(File.OpenRead(zipPathAndFile));
            ZipEntry theEntry;
            string tmpEntry = String.Empty;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = outputFolder;
                string fileName = Path.GetFileName(theEntry.Name);
                // create directory 
                if (directoryName != "")
                {
                    Directory.CreateDirectory(directoryName);
                }
                if (fileName != String.Empty)
                {
                    string fullPath = Path.Combine(directoryName, theEntry.Name);
                    fullPath = fullPath.Replace("\\ ", "\\");
                    string fullDirPath = Path.GetDirectoryName(fullPath);
                    if (!Directory.Exists(fullDirPath)) Directory.CreateDirectory(fullDirPath);
                    FileStream streamWriter = File.Create(fullPath);
                    int size = 2048;
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        size = s.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }
                    streamWriter.Close();
                }
            }
            s.Close();
            if (deleteZipFile)
                File.Delete(zipPathAndFile);
        }
    }
}
