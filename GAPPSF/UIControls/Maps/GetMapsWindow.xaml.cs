using GAPPSF.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
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

namespace GAPPSF.UIControls.Maps
{
    /// <summary>
    /// Interaction logic for GetMapsWindow.xaml
    /// </summary>
    public partial class GetMapsWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public class FTPDirectoryItemInfo
        {
            public string Name { get; set; }
            public bool Folder { get; set; }
            public long Size { get; set; }
            public string SizeText
            {
                get
                {
                    if (Folder)
                    {
                        return "";
                    }
                    else
                    {
                        return string.Format("{0:0.0} MB", ((double)Size / 1000000.0));
                    }
                }
            }

            public override string ToString()
            {
                return Name;
            }
        }
        private List<string> _folder = new List<string>();


        public ObservableCollection<FTPDirectoryItemInfo> FTPDirectoryItems { get; private set; }
        private string _performingAction;
        public string PerformingAction
        {
            get { return _performingAction; }
            set { SetProperty(ref _performingAction, value); }
        }

        private string _currentPath;
        public string CurrentPath
        {
            get { return _currentPath; }
            set { SetProperty(ref _currentPath, value); }
        }

        public string DownloadedFilePath { get; set; }

        public GetMapsWindow()
        {
            InitializeComponent();

            FTPDirectoryItems = new ObservableCollection<FTPDirectoryItemInfo>();

            DataContext = this;

            Dispatcher.BeginInvoke(new Action(async () =>
            {
                List<FTPDirectoryItemInfo> fls = await GetFileListAsync("ftp://download.mapsforge.org/maps/");
                if (fls != null)
                {
                    _folder.Add("maps");
                    listFolderContent(true, fls);
                }
            }));

        }

        private AsyncDelegateCommand _downloadCommand;
        public AsyncDelegateCommand DownloadCommand
        {
            get
            {
                if (_downloadCommand==null)
                {
                    _downloadCommand = new AsyncDelegateCommand(param => DownloadMap(),
                        param => canDownload());
                }
                return _downloadCommand;
            }
        }
        public bool canDownload()
        {
            bool result = false;
            FTPDirectoryItemInfo si = listItems.SelectedItem as FTPDirectoryItemInfo;
            if (si!=null)
            {
                if (!si.Folder)
                {
                    result = true;
                }
            }
            return result;
        }

        public async Task DownloadMap()
        {
            DownloadedFilePath = null;
            FTPDirectoryItemInfo si = listItems.SelectedItem as FTPDirectoryItemInfo;
            if (si!=null && !si.Folder)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        using (TemporaryFile DownloadedFile = new TemporaryFile(false))
                        {
                            string fn = "ftp://download.mapsforge.org/";
                            for (int i = 0; i < _folder.Count; i++)
                            {
                                fn = string.Format("{0}{1}/", fn, _folder[i]);
                            }
                            fn = string.Format("{0}{1}", fn, si.Name);
                            DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                            using (Utils.ProgressBlock prog = new Utils.ProgressBlock("DownloadingFile", fn, (int)si.Size, 0))
                            {
                                FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri(fn));
                                ftpRequest.Credentials = new NetworkCredential("anonymous", "gapp@globalcaching.eu");
                                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                                int bytesTotal = 0;
                                using (FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
                                using (Stream responseStream = ftpResponse.GetResponseStream())
                                using (FileStream writeStream = new FileStream(DownloadedFile.Path, FileMode.Create))
                                {
                                    int Length = 2048 * 1000;
                                    Byte[] buffer = new Byte[Length];
                                    int bytesRead = responseStream.Read(buffer, 0, Length);

                                    while (bytesRead > 0)
                                    {
                                        bytesTotal += bytesRead;// don't forget to increment bytesRead !
                                        writeStream.Write(buffer, 0, bytesRead);
                                        bytesRead = responseStream.Read(buffer, 0, Length);

                                        if (DateTime.Now >= nextUpdate)
                                        {
                                            prog.Update(fn, (int)si.Size, bytesTotal);
                                            nextUpdate = DateTime.Now.AddSeconds(1);
                                        }
                                    }
                                }

                                if (bytesTotal == (int)si.Size)
                                {
                                    //copy to correct location
                                    string p = System.IO.Path.Combine(Core.Settings.Default.SettingsFolder, "Maps");
                                    if (!Directory.Exists(p))
                                    {
                                        Directory.CreateDirectory(p);
                                    }
                                    DownloadedFilePath = System.IO.Path.Combine(p, si.Name);
                                    File.Copy(DownloadedFile.Path, DownloadedFilePath, true);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        DownloadedFilePath = null;
                        Core.ApplicationData.Instance.Logger.AddLog(this, e);
                    }
                });
            }
            if (!string.IsNullOrEmpty(DownloadedFilePath))
            {
                DialogResult = true;
                Close();
            }
        }

        private void listFolderContent(bool isRoot, List<FTPDirectoryItemInfo> dil)
        {
            FTPDirectoryItems.Clear();
            StringBuilder sb = new StringBuilder();
            foreach (string s in _folder)
            {
                sb.AppendFormat("/{0}", s);
            }
            CurrentPath = sb.ToString();

            if (!isRoot)
            {
                FTPDirectoryItemInfo fdi = new FTPDirectoryItemInfo();
                fdi.Folder = true;
                fdi.Name = "..";
                FTPDirectoryItems.Add(fdi);
            }
            var li = from l in dil where l.Folder orderby l.Name select l;
            foreach (var l in li)
            {
                FTPDirectoryItems.Add(l);
            }
            li = from l in dil where !l.Folder orderby l.Name select l;
            foreach (var l in li)
            {
                FTPDirectoryItems.Add(l);
            }
        }


        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }

        private async Task<List<FTPDirectoryItemInfo>> GetFileListAsync(string fullPath)
        {
            List<FTPDirectoryItemInfo> result = null;
            PerformingAction = Localization.TranslationManager.Instance.Translate("GettingFileList") as string;
            await Task.Run(() =>
            {
                result = GetFileList(fullPath);

            });
            PerformingAction = "";
            return result;
        }


        private List<FTPDirectoryItemInfo> GetFileList(string fullPath)
        {
            List<FTPDirectoryItemInfo> result = new List<FTPDirectoryItemInfo>();
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
                        di.Name = line.Substring(line.LastIndexOf(' ') + 1);
                        di.Size = 0;
                        string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
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
            catch (Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
                result = null;
            }
            return result;
        }

        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FTPDirectoryItemInfo si = listItems.SelectedItem as FTPDirectoryItemInfo;
            if (si!=null)
            {
                if (si.Name=="..")
                {
                    //move up
                    string folder = "ftp://download.mapsforge.org/";
                    for (int i = 0; i < _folder.Count - 1; i++)
                    {
                        folder = string.Format("{0}{1}/", folder, _folder[i]);
                    }
                    List<FTPDirectoryItemInfo> fls = await GetFileListAsync(folder);
                    if (fls != null)
                    {
                        _folder.RemoveAt(_folder.Count - 1);
                        listFolderContent(_folder.Count < 2, fls);
                    }
                }
                else if (si.Folder)
                {
                    string folder = "ftp://download.mapsforge.org/";
                    for (int i = 0; i < _folder.Count; i++)
                    {
                        folder = string.Format("{0}{1}/", folder, _folder[i]);
                    }
                    folder = string.Format("{0}{1}/", folder, si.Name);
                    List<FTPDirectoryItemInfo> fls = await GetFileListAsync(folder);
                    if (fls != null)
                    {
                        _folder.Add(si.Name);
                        listFolderContent(false, fls);
                    }
                }
            }
        }

    }
}
