using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;

namespace GlobalcachingApplication.Plugins.GAPPDataStorage
{
    public partial class InternalStorage
    {
        [XmlType("BackupItem")] // define Type
        public class BackupItem
        {
            [XmlElement("BackupFile")]
            public string BackupFile { get; set; }

            [XmlElement("OriginalPath")]
            public string OriginalPath { get; set; }

            //might use proxy for dateTime
            [XmlElement("BackupDate")]
            public DateTime BackupDate { get; set; }

            public BackupItem() { }

            public BackupItem(string backupFile, string originalPath, DateTime backupDate) 
            {
                this.BackupFile = backupFile;
                this.OriginalPath = originalPath;
                this.BackupDate = backupDate;
            }
        }

        [XmlRoot("BackupItems")]
        [XmlInclude(typeof(BackupItem))] // include type class Person
        public class BackupItemList
        {
            [XmlArray("BackupItemArray")]
            [XmlArrayItem("BackupItem")]
            public List<BackupItem> BackupItems = new List<BackupItem>();

            public BackupItemList() { }

            public void AddBackupItem(BackupItem item)
            {
                BackupItems.Add(item);
            }

            public void RemoveBackupItem(BackupItem item)
            {
                BackupItems.Remove(item);
            }
        }

        internal BackupItemList _backupItemList = null;
        private string _backupDataInfoFile = null;
        private BackupItem _selectedBackupItem;
        private string _restorePath;

        private string BackupDataInfoFile
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(_backupDataInfoFile))
                    {
                        if (string.IsNullOrEmpty(PluginSettings.Instance.BackupFolder))
                        {
                            PluginSettings.Instance.BackupFolder = Path.Combine(Core.PluginDataPath, "StorageBackup");
                        }
                    }
                    if (!Directory.Exists(PluginSettings.Instance.BackupFolder))
                    {
                        Directory.CreateDirectory(PluginSettings.Instance.BackupFolder);
                    }
                    _backupDataInfoFile = Path.Combine(PluginSettings.Instance.BackupFolder, "backupinfo.xml");
                }
                catch
                {
                }
                return _backupDataInfoFile;
            }
        }

        public void ReadBackupItemList()
        {
            try
            {
                _backupItemList = new BackupItemList();
                if (File.Exists(BackupDataInfoFile))
                {
                    using (FileStream fs = new FileStream(BackupDataInfoFile, FileMode.Open))
                    {
                        Type[] itemTypes = { typeof(BackupItem) };
                        XmlSerializer serializer = new XmlSerializer(typeof(BackupItemList), itemTypes);
                        _backupItemList = (BackupItemList)serializer.Deserialize(fs);
                        serializer.Serialize(Console.Out, _backupItemList);
                        Console.ReadLine();
                    }
                }
            }
            catch
            {
            }
        }

        private void SaveBackupItemList()
        {
            if (_backupItemList != null)
            {
                try
                {
                    Type[] itemTypes = { typeof(BackupItem) };
                    XmlSerializer serializer = new XmlSerializer(typeof(BackupItemList), itemTypes);
                    using (FileStream fs = new FileStream(BackupDataInfoFile, FileMode.Create))
                    {
                        serializer.Serialize(fs, _backupItemList);
                    }
                }
                catch
                {
                }
            }
        }

        protected override bool SupportsBackupRestoreDatabase
        {
            get
            {
                return true;
            }
        }

        public override bool PrepareBackup()
        {
            bool result;
            ReadBackupItemList();
            result = _fileCollection!=null && _backupItemList != null && !string.IsNullOrEmpty(_backupDataInfoFile);
            return result;
        }

        public override bool Backup()
        {
            bool result = true;
            _fileCollection.StartReleaseForCopy();
            try
            {
                BackupItem bi = new BackupItem();
                bi.BackupFile = Path.Combine(PluginSettings.Instance.BackupFolder, string.Concat(Path.GetFileNameWithoutExtension(_fileCollection.BaseFilename), "_", DateTime.Now.ToString("s").Replace(" ", "").Replace("T", "").Replace(":", "").Replace("-", ""),".zip"));
                bi.BackupDate = DateTime.Now;
                bi.OriginalPath = _fileCollection.BaseFilename;
                //zip all files
                string[] files = Directory.GetFiles(Path.GetDirectoryName(_fileCollection.BaseFilename), string.Concat(Path.GetFileNameWithoutExtension(_fileCollection.BaseFilename), ".*"));
                List<FileInfo> fil = new List<FileInfo>();
                long totalBytes = 0;
                foreach (string f in files)
                {
                    FileInfo fi = new FileInfo(f);
                    fil.Add(fi);
                    totalBytes += fi.Length;
                }
                int max = (int)Math.Max(1, totalBytes / (1024 * 1024));
                long processed = 0;
                DateTime progUpdate = DateTime.Now.AddSeconds(2);
                byte[] buffer = new byte[4 * 1024 * 1024];
                using (Utils.ProgressBlock prog = new Utils.ProgressBlock(this, STR_BACKINGUPDATA, STR_BACKINGUPDATA, max, 0))
                {
                    using (ZipOutputStream s = new ZipOutputStream(System.IO.File.Create(bi.BackupFile)))
                    {
                        s.SetLevel(9); // 0-9, 9 being the highest compression

                        foreach (FileInfo fi in fil)
                        {
                            ZipEntry entry = new ZipEntry(Path.GetFileName(fi.FullName));
                            entry.DateTime = DateTime.Now;
                            entry.Size = fi.Length;
                            s.PutNextEntry(entry);

                            using (FileStream fs = fi.OpenRead())
                            {
                                int i;
                                do
                                {
                                    i = fs.Read(buffer, 0, buffer.Length);
                                    if (i > 0)
                                    {
                                        s.Write(buffer, 0, i);
                                        processed += i;
                                    }

                                    if (DateTime.Now >= progUpdate)
                                    {
                                        int pos = (int)(processed / (1024 * 1024));
                                        prog.UpdateProgress(STR_BACKINGUPDATA, STR_BACKINGUPDATA, max, pos);
                                        progUpdate = DateTime.Now.AddSeconds(2);
                                    }
                                } while (i == buffer.Length);
                            }

                            if (DateTime.Now >= progUpdate)
                            {
                                int pos = (int)(processed / (1024 * 1024));
                                prog.UpdateProgress(STR_BACKINGUPDATA, STR_BACKINGUPDATA, max, pos);
                                progUpdate = DateTime.Now.AddSeconds(2);
                            }

                        }

                        s.Finish();
                        s.Close();
                    }
                }


                //check backup(s) te remove
                try
                {
                    if (PluginSettings.Instance.BackupKeepMaxDays > 0)
                    {
                        DateTime dt = DateTime.Now.AddDays(-1 * PluginSettings.Instance.BackupKeepMaxDays).Date;
                        List<BackupItem> bil = (from b in _backupItemList.BackupItems where b.BackupDate.Date < dt select b).ToList();
                        foreach (BackupItem b in bil)
                        {
                            if (File.Exists(b.BackupFile))
                            {
                                File.Delete(b.BackupFile);
                            }
                            _backupItemList.RemoveBackupItem(b);
                        }
                    }
                    if (PluginSettings.Instance.BackupKeepMaxCount > 0)
                    {
                        List<BackupItem> bil = (from b in _backupItemList.BackupItems where b.OriginalPath == bi.OriginalPath select b).OrderByDescending(x => x.BackupDate).Skip(PluginSettings.Instance.BackupKeepMaxCount-1).ToList();
                        foreach (BackupItem b in bil)
                        {
                            if (File.Exists(b.BackupFile))
                            {
                                File.Delete(b.BackupFile);
                            }
                            _backupItemList.RemoveBackupItem(b);
                        }
                    }
                }
                catch
                {
                }

                //save new backup info
                _backupItemList.AddBackupItem(bi);
                SaveBackupItemList();
            }
            finally
            {
                _fileCollection.EndReleaseForCopy();
            }
            return result;
        }




        public override bool PrepareRestore()
        {
            bool result = false;
            using (RestoreForm dlg = new RestoreForm(this))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _selectedBackupItem = dlg.SelectedBackupItem;
                    _restorePath = dlg.RestoreFolder;

                    string directoryName = _restorePath;
                    if (!Directory.Exists(_restorePath)) Directory.CreateDirectory(_restorePath);

                    if (_fileCollection != null)
                    {
                        _fileCollection.Dispose();
                        _fileCollection = null;
                    }

                    PluginSettings.Instance.ActiveDataFile = Path.Combine(_restorePath, Path.GetFileName(_selectedBackupItem.OriginalPath));
                    SetDataSourceName(PluginSettings.Instance.ActiveDataFile);

                    Core.Geocaches.Clear();
                    Core.Logs.Clear();
                    Core.Waypoints.Clear();
                    Core.LogImages.Clear();
                    Core.UserWaypoints.Clear();

                    result = true;
                }
            }
            return result;
        }

        public override bool Restore(bool geocachesOnly)
        {
            bool result = false;

            using (Utils.ProgressBlock prog = new Utils.ProgressBlock(this, STR_RESTORINGDATA, STR_RESTORINGDATA, 1, 0))
            {
                using (ZipInputStream s = new ZipInputStream(File.OpenRead(_selectedBackupItem.BackupFile)))
                {
                    ZipEntry theEntry;
                    string tmpEntry = String.Empty;
                    while ((theEntry = s.GetNextEntry()) != null)
                    {
                        string fileName = Path.GetFileName(theEntry.Name);
                        string fullPath = Path.Combine(_restorePath, theEntry.Name);
                        using (FileStream streamWriter = File.Create(fullPath))
                        {
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
                }
            }

            result = Open(geocachesOnly);

            return result;
        }
    }
}
