using GAPPSF.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GAPPSF.ImageGrabber
{
    public class OfflineImagesManager
    {
        private const string DATABASE_FILENAME = "grabbedimg.db3";
        private const string IMG_SUBFOLDER = "GrabbedImages";

        private static OfflineImagesManager _uniqueInstance = null;
        private static object _lockObject = new object();

        private string _imageFolder = null;
        private DBCon _dbcon = null;

        private OfflineImagesManager()
        {
            try
            {
                _imageFolder = Path.Combine(Core.Settings.Default.SettingsFolder, "OfflineImages");
                if (!System.IO.Directory.Exists(_imageFolder))
                {
                    System.IO.Directory.CreateDirectory(_imageFolder);
                }
                string sf = Path.Combine(_imageFolder, IMG_SUBFOLDER);
                if (!System.IO.Directory.Exists(sf))
                {
                    System.IO.Directory.CreateDirectory(sf);
                }
                InitDatabase();
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
        }

        public static OfflineImagesManager Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance==null)
                        {
                            _uniqueInstance = new OfflineImagesManager();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        private void CloseDatabase()
        {
            if (_dbcon != null)
            {
                _dbcon.Dispose();
                _dbcon = null;
            }
        }

        private string DatabaseFilename
        {
            get { return System.IO.Path.Combine(_imageFolder, DATABASE_FILENAME); }
        }

        private bool InitDatabase()
        {
            CloseDatabase();
            bool result = false;
            try
            {
                _dbcon = new DBConComSqlite(DatabaseFilename);
                object o = _dbcon.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='images'");
                if (o == null || o.GetType() == typeof(DBNull))
                {
                    _dbcon.ExecuteNonQuery("create table 'images' (org_url text, gccode text, local_file text)");
                    _dbcon.ExecuteNonQuery("create index idx_images on images (org_url)");
                    _dbcon.ExecuteNonQuery("create index idx_gccodes on images (gcode)");
                }
                result = true;
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
            return result;
        }


        public string GetImagePath(string orgUrl)
        {
            string result = null;
            try
            {
                object o = _dbcon.ExecuteScalar(string.Format("select local_file from images where org_url = '{0}' limit 1", orgUrl.Replace("'", "''")));
                if (o != null && o.GetType() != typeof(DBNull))
                {
                    result = System.IO.Path.Combine(new string[] { _imageFolder, IMG_SUBFOLDER, o as string });
                }
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
            return result;
        }

        public string GetImagePath(Core.Data.Geocache gc, string orgUrl)
        {
            string result = null;
            if (gc == null)
            {
                result = GetImagePath(orgUrl);
            }
            else
            {
                try
                {
                    object o = _dbcon.ExecuteScalar(string.Format("select local_file from images where org_url = '{0}' and gccode = '{1}' limit 1", orgUrl.Replace("'", "''"), gc.Code.Replace("'", "''")));
                    if (o != null && o.GetType() != typeof(DBNull))
                    {
                        result = System.IO.Path.Combine(new string[] { _imageFolder, IMG_SUBFOLDER, o as string });
                    }
                }
                catch(Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
            }
            return result;
        }

        public string GetImageUri(string orgUri)
        {
            string result;
            string filename = GetImagePath(orgUri);
            if (!string.IsNullOrEmpty(filename))
            {
                result = GetEmbeddedImage(filename);
            }
            else
            {
                result = orgUri;
            }
            return result;
        }

        public string GetImageUri(Core.Data.Geocache gc, string orgUri)
        {
            string result;
            string filename = GetImagePath(gc, orgUri);
            if (!string.IsNullOrEmpty(filename))
            {
                result = GetEmbeddedImage(filename);
            }
            else
            {
                result = orgUri;
            }
            return result;
        }

        private string GetEmbeddedImage(string filename)
        {
            string result;
            try
            {
                byte[] buffer = File.ReadAllBytes(filename);
                result = string.Concat("data:image/", filename.Substring(filename.LastIndexOf('.') + 1), ";base64,", Convert.ToBase64String(buffer));
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
                result = filename;
            }
            return result;
        }

        public Dictionary<string, string> GetImages(Core.Data.Geocache gc)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            try
            {
                DbDataReader dr = _dbcon.ExecuteReader(string.Format("select  org_url, local_file from images where gccode = '{0}'", gc.Code.Replace("'", "''")));
                while (dr.Read())
                {
                    result.Add(dr["org_url"] as string, System.IO.Path.Combine(new string[] { _imageFolder, IMG_SUBFOLDER, dr["local_file"] as string }));
                }
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
            return result;
        }

        public async Task DownloadImagesAsync(List<Core.Data.Geocache> gcList, bool updateExisting)
        {
            await Task.Run(async () =>
            {
                bool cancel = false;
                ConcurrentQueue<Core.Data.Geocache> cq = new ConcurrentQueue<Core.Data.Geocache>();
                foreach (var gc in gcList)
                {
                    cq.Enqueue(gc);
                }

                using (Utils.ProgressBlock prog = new ProgressBlock("DownloadingImages", "DownloadingImages", gcList.Count, 0, true))
                {
                    Action actionUpdateProgress = () =>
                    {
                        DateTime updateAt = DateTime.MinValue;
                        int cnt = cq.Count;
                        while (cnt > 0)
                        {
                            if (DateTime.Now >= updateAt)
                            {
                                if (!prog.Update("DownloadingImages", gcList.Count, gcList.Count - cnt))
                                {
                                    cancel = true;
                                    break;
                                }
                                updateAt = DateTime.Now.AddSeconds(1);
                            }
                            System.Threading.Thread.Sleep(200);
                            cnt = cq.Count;
                        };
                    };

                    Action actionDownload = () =>
                    {
                        Core.Data.Geocache gc;
                        using (System.Net.WebClient wc = new System.Net.WebClient())
                        {
                            while (!cancel && cq.TryDequeue(out gc))
                            {
                                string fnp = System.IO.Path.Combine(_imageFolder, IMG_SUBFOLDER);
                                bool grabOnlyNew = !updateExisting;
                                try
                                {
                                    List<string> linkList;
                                    lock (_lockObject)
                                    {
                                        linkList = DataAccess.GetImagesOfGeocache(gc.Database, gc.Code);
                                    }

                                    foreach (string link in linkList)
                                    {
                                        string fn = string.Format("{0}.jpg", Guid.NewGuid().ToString("N"));
                                        bool skipInsert = false;
                                        //if it fails, just ignore this image
                                        try
                                        {
                                            //check if link already is in database
                                            //if so, use this filename
                                            lock (_lockObject)
                                            {
                                                object o = _dbcon.ExecuteScalar(string.Format("select local_file from images where gccode='{0}' and org_url='{1}'", gc.Code.Replace("'", "''"), link.Replace("'", "''")));
                                                if (o != null && o.GetType() != typeof(DBNull))
                                                {
                                                    fn = (string)o;
                                                    skipInsert = true;
                                                }
                                            }
                                            if (grabOnlyNew && skipInsert)
                                            {
                                                if (System.IO.File.Exists(System.IO.Path.Combine(fnp, fn)))
                                                {
                                                    continue;
                                                }
                                            }
                                            using (System.IO.TemporaryFile tmp = new System.IO.TemporaryFile(true))
                                            {
                                                wc.DownloadFile(link, tmp.Path);
                                                if (new FileInfo(tmp.Path).Length < 10 * 1024 * 1024)
                                                {
                                                    using (System.Drawing.Image img = System.Drawing.Image.FromFile(tmp.Path))
                                                    {
                                                        img.Save(System.IO.Path.Combine(fnp, fn), System.Drawing.Imaging.ImageFormat.Jpeg);
                                                        if (!skipInsert)
                                                        {
                                                            lock (_lockObject)
                                                            {
                                                                _dbcon.ExecuteNonQuery(string.Format("insert into images (gccode, org_url, local_file) values ('{0}', '{1}', '{2}')", gc.Code.Replace("'", "''"), link.Replace("'", "''"), fn));
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

                                }
                                catch (Exception e)
                                {
                                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                                    //skip and go to next one
                                }
                            }
                        };
                    };

                    List<Task> tasks = new List<Task>();
                    tasks.Add(Task.Factory.StartNew(actionUpdateProgress));
                    for (int i = 0; i < 4 && i < gcList.Count; i++)
                    {
                        tasks.Add(Task.Factory.StartNew(actionDownload));
                    }
                    await Task.WhenAll(tasks.ToArray());
                }
            });
        }

    }
}
