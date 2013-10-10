using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;

namespace GlobalcachingApplication.Plugins.Maps
{
    public partial class GetMapsForm : Form
    {
        public const string STR_TITLE = "Maps download";
        public const string STR_RETRIEVINGLIST = "getting file list...";
        public const string STR_DOWNLOADINGFILE = "Downloading file...";
        public const string STR_NAME = "Name";
        public const string STR_SIZE = "Size";
        public const string STR_DOWNLOAD = "Download";

        private Utils.BasePlugin.Plugin _plugin = null;
        private List<string> _folder = new List<string>();

        public TemporaryFile DownloadedFile { get; private set; }
        public string DownloadedFileName { get; private set; }

        public class FTPDirectoryItemInfo
        {
            public string Name { get; set; }
            public bool Folder { get; set; }
            public long Size { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        public GetMapsForm()
        {
            InitializeComponent();
        }
        public GetMapsForm(Utils.BasePlugin.Plugin plugin)
            : this()
        {
            _plugin = plugin;
            label2.Text = "";
            button1.Enabled = false;

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.listView1.Columns[0].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.listView1.Columns[1].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SIZE);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DOWNLOAD);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            List<FTPDirectoryItemInfo> fls = GetFileList("ftp://download.mapsforge.org/maps/");
            if (fls != null)
            {
                _folder.Add("maps");
                listFolderContent(true, fls);
            }
        }

        private void listFolderContent(bool isRoot, List<FTPDirectoryItemInfo> dil)
        {
            listView1.Items.Clear();
            StringBuilder sb = new StringBuilder();
            foreach (string s in _folder)
            {
                sb.AppendFormat("/{0}", s);
            }
            label1.Text = sb.ToString();

            if (!isRoot)
            {
                listView1.Items.Add(new ListViewItem(new string[]{"..", ""}));
            }
            var li = from l in dil where l.Folder orderby l.Name select l;
            foreach (var l in li)
            {
                ListViewItem lvi = new ListViewItem(new string[] { l.Name, "" });
                lvi.Tag = l;
                listView1.Items.Add(lvi);
            }
            li = from l in dil where !l.Folder orderby l.Name select l;
            foreach (var l in li)
            {
                ListViewItem lvi = new ListViewItem(new string[] { l.Name, string.Format("{0:0.0} MB", ((double)l.Size / 1000000.0)) });
                lvi.Tag = l;
                listView1.Items.Add(lvi);
            }
        }

        private List<FTPDirectoryItemInfo> GetFileList(string fullPath)
        {
            label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RETRIEVINGLIST);
            label2.Refresh();

            List<FTPDirectoryItemInfo> result = new List<FTPDirectoryItemInfo>();
            this.Cursor = Cursors.WaitCursor;
            try
            {
                FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri(fullPath));
                ftpRequest.Credentials = new NetworkCredential("anonymous", "gapp@globalcaching.eu");
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                ftpRequest.KeepAlive = false;

                string line;
                using (FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
                using (StreamReader sr = new StreamReader(ftpResponse.GetResponseStream()))
                {
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        FTPDirectoryItemInfo di = new FTPDirectoryItemInfo();
                        di.Folder = line.StartsWith("d");
                        di.Name = line.Substring(line.LastIndexOf(' ')+1);
                        di.Size = 0;
                        string[] parts = line.Split(new char[] {' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        try
                        {
                            di.Size = long.Parse(parts[4]);
                        }
                        catch
                        {
                        }
                        if (di.Name != "0.2.4-archive")
                        {
                            result.Add(di);
                        }
                    }
                }
            }
            catch
            {
                result = null;
            }
            this.Cursor = Cursors.Default;
            label2.Text = "";
            return result;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = listView1.SelectedIndices.Count > 0;
        }

        private void listView1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                if (listView1.SelectedItems[0].Tag == null)
                {
                    //move up
                    string folder = "ftp://download.mapsforge.org/";
                    for (int i = 0; i < _folder.Count - 1; i++)
                    {
                        folder = string.Format("{0}{1}/", folder, _folder[i]);
                    }
                    List<FTPDirectoryItemInfo> fls = GetFileList(folder);
                    if (fls != null)
                    {
                        _folder.RemoveAt(_folder.Count - 1);
                        listFolderContent(_folder.Count<2, fls);
                    }
                    button1.Enabled = false;
                }
                else
                {
                    FTPDirectoryItemInfo fi = listView1.SelectedItems[0].Tag as FTPDirectoryItemInfo;
                    if (fi.Folder)
                    {
                        string folder = "ftp://download.mapsforge.org/";
                        for (int i = 0; i < _folder.Count; i++)
                        {
                            folder = string.Format("{0}{1}/", folder, _folder[i]);
                        }
                        folder = string.Format("{0}{1}/", folder, fi.Name);
                        List<FTPDirectoryItemInfo> fls = GetFileList(folder);
                        if (fls != null)
                        {
                            _folder.Add(fi.Name);
                            listFolderContent(false, fls);
                        }
                        button1.Enabled = false;
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                FTPDirectoryItemInfo fi = listView1.SelectedItems[0].Tag as FTPDirectoryItemInfo;
                if (fi != null)
                {
                    button1.Enabled = false;
                    listView1.Enabled = false;
                    this.backgroundWorker1.RunWorkerAsync(fi);
                }
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            listView1.Enabled = true;
            if (DownloadedFile != null)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
            else
            {
                button1.Enabled = true;
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            FTPDirectoryItemInfo fi = e.Argument as FTPDirectoryItemInfo;
            DownloadedFileName = fi.Name;
            try
            {
                DownloadedFile = new TemporaryFile(false);

                string fn = "ftp://download.mapsforge.org/";
                for (int i = 0; i < _folder.Count; i++)
                {
                    fn = string.Format("{0}{1}/", fn, _folder[i]);
                }
                fn = string.Format("{0}{1}", fn, fi.Name);
                using (Utils.ProgressBlock prog = new Utils.ProgressBlock(_plugin, STR_DOWNLOADINGFILE, fn, (int)fi.Size, 0))
                {
                    FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri(fn));
                    ftpRequest.Credentials = new NetworkCredential("anonymous", "gapp@globalcaching.eu");
                    ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                    int bytesTotal = 0;
                    using (FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
                    using (Stream responseStream = ftpResponse.GetResponseStream())
                    using (FileStream writeStream = new FileStream(DownloadedFile.Path, FileMode.Create))
                    {
                        int Length = 2048*1000;
                        Byte[] buffer = new Byte[Length];
                        int bytesRead = responseStream.Read(buffer, 0, Length);

                        while (bytesRead > 0)
                        {
                            bytesTotal += bytesRead;// don't forget to increment bytesRead !
                            writeStream.Write(buffer, 0, bytesRead);
                            bytesRead = responseStream.Read(buffer, 0, Length);
                            prog.UpdateProgress(STR_DOWNLOADINGFILE, fn, (int)fi.Size, bytesTotal);
                        }
                    }

                    if (bytesTotal != (int)fi.Size)
                    {
                        DownloadedFile = null;
                        DownloadedFileName = "";
                    }
                }
            }
            catch
            {
                DownloadedFile = null;
                DownloadedFileName = "";
            }
        }
    }
}
