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
            }
            catch
            {
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
                    _dbcon.ExecuteNonQuery("create index idx_images on images (org_url_idx)");
                    _dbcon.ExecuteNonQuery("create index idx_gccodes on images (gcode_idx)");
                }
                result = true;
            }
            catch
            {
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
            catch
            {
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
                catch
                {
                }
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
            catch
            {
            }
            return result;
        }

        public async Task DownloadImagesAsync(List<Core.Data.Geocache> gcList, bool updateExisting)
        {
            bool cancel = false;
            ConcurrentQueue<Core.Data.Geocache> cq = new ConcurrentQueue<Core.Data.Geocache>();
            foreach(var gc in gcList)
            {
                cq.Enqueue(gc);
            }

            using (Utils.ProgressBlock prog = new ProgressBlock("Downloading images...", "Downloading images...", gcList.Count, 0, true))
            {
                Action actionUpdateProgress = () =>
                {
                    DateTime updateAt = DateTime.MinValue;
                    int cnt = cq.Count;
                    while (cnt>0)
                    {
                        if (DateTime.Now>=updateAt)
                        {
                            if (!prog.Update("Downloading images...", gcList.Count, gcList.Count - cnt))
                            {
                                cancel = true;
                                break;
                            }
                        }
                        cnt = cq.Count;
                    };
                };

                Action actionDownload = () =>
                {
                    Core.Data.Geocache gc;
                    while (!cancel && cq.TryDequeue(out gc))
                    {
                        using (System.Net.WebClient wc = new System.Net.WebClient())
                        {
                            string fnp = System.IO.Path.Combine(_imageFolder, IMG_SUBFOLDER);
                            bool grabOnlyNew = !updateExisting;
                            try
                            {
                                StringBuilder sb = new StringBuilder();
                                lock (_lockObject)
                                {
                                    if (gc.ShortDescriptionInHtml && gc.ShortDescription != null)
                                    {
                                        sb.Append(gc.ShortDescription);
                                    }
                                    if (gc.LongDescriptionInHtml && gc.LongDescription != null)
                                    {
                                        sb.Append(gc.LongDescription);
                                    }
                                }
                                if (sb.Length > 0)
                                {
                                    List<string> linkList = new List<string>();

                                    Regex r = new Regex(@"</?\w+\s+[^>]*>", RegexOptions.Multiline);
                                    MatchCollection mc = r.Matches(sb.ToString());
                                    foreach (Match m in mc)
                                    {
                                        string s = m.Value.Substring(1).Replace('\r', ' ').Replace('\n', ' ').Trim();
                                        if (s.StartsWith("img ", StringComparison.OrdinalIgnoreCase))
                                        {
                                            int pos = s.IndexOf(" src", StringComparison.OrdinalIgnoreCase);
                                            pos = s.IndexOfAny(new char[] { '\'', '"' }, pos);
                                            int pos2 = s.IndexOfAny(new char[] { '\'', '"' }, pos + 1);
                                            linkList.Add(s.Substring(pos + 1, pos2 - pos - 1));
                                        }
                                    }

                                    List<Core.Data.GeocacheImage> imgList = DataAccess.GetGeocacheImages(gc.Database, gc.Code);
                                    if (imgList != null)
                                    {
                                        foreach (Core.Data.GeocacheImage img in imgList)
                                        {
                                            if (!linkList.Contains(img.Url))
                                            {
                                                linkList.Add(img.Url);
                                            }
                                        }
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
                                        catch
                                        {
                                        }
                                    }
                                }

                            }
                            catch
                            {
                                //skip and go to next one
                            }
                        }
                    };
                };

                List<Task> tasks = new List<Task>();
                tasks.Add(new Task(actionUpdateProgress));
                for (int i = 0; i < 4 && i < gcList.Count; i++)
                {
                    tasks.Add(new Task(actionDownload));
                }
                await Task.WhenAll(tasks.ToArray());
            }
        }

    }
}
