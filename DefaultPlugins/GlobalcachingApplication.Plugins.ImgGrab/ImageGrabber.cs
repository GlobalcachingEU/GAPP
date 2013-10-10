using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using GlobalcachingApplication.Utils;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;

namespace GlobalcachingApplication.Plugins.ImgGrab
{
    public class ImageGrabber: Utils.BasePlugin.Plugin, Framework.Interfaces.IImageResource
    {
        public const string STR_GRABBING_IMAGES = "Grabbing images...";
        public const string STR_DELETING_GRABBED_IMAGES = "Deleting grabbed images";
        public const string STR_DELETING_IMAGES = "Deleting images";
        public const string STR_NOCACHESSELECTED = "No geocaches selected";
        public const string STR_GRABNEW = "Grab only images that are not grabbed before.";
        public const string STR_GRABIMAGES = "Offline images";
        public const string STR_CREATINGFOLDER = "Creating offline image folder";

        public const string ACTION_GRAB_ACTIVE = "Offline images|Grab for active";
        public const string ACTION_GRAB_SELECTED = "Offline images|Grab for selection";
        public const string ACTION_CREATE_ACTIVE = "Offline images|Create image folder for active";
        public const string ACTION_CREATE_SELECTED = "Offline images|Create image folder for selection";
        public const string ACTION_GRAB_SEP = "Offline images|-";
        public const string ACTION_GRAB_DELETE_ACTIVE = "Offline images|Delete for active";
        public const string ACTION_GRAB_DELETE_ALL = "Offline images|Delete for all";
        public const string ACTION_GRAB_DELETE_SELECTED = "Offline images|Delete for selection";

        private const string DATABASE_FILENAME = "grabbedimg.db3";
        private const string IMG_SUBFOLDER = "GrabbedImages";
        private DBCon _dbcon = null;
        private object _lockDBObject = new object();
        private List<Framework.Data.Geocache> _gcList;
        private int _orgListCount;
        private volatile bool _grabOnlyNew = false;
        private ManualResetEvent _threadReady = null;

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_GRAB_ACTIVE);
            AddAction(ACTION_GRAB_SELECTED);
            AddAction(ACTION_CREATE_ACTIVE);
            AddAction(ACTION_CREATE_SELECTED);
            AddAction(ACTION_GRAB_SEP);
            AddAction(ACTION_GRAB_DELETE_ACTIVE);
            AddAction(ACTION_GRAB_DELETE_SELECTED);
            AddAction(ACTION_GRAB_DELETE_ALL);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_GRABBING_IMAGES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_DELETING_GRABBED_IMAGES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_DELETING_IMAGES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOCACHESSELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_GRABNEW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_GRABIMAGES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CREATINGFOLDER));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(CopyToFolderForm.STR_DOWNLOAD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CopyToFolderForm.STR_FOLDER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CopyToFolderForm.STR_NOTINDESCR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CopyToFolderForm.STR_OK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CopyToFolderForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CopyToFolderForm.STR_CLEAR));

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }
            Properties.Settings.Default.ActiveDataPath = System.IO.Path.Combine(core.PluginDataPath, "ImgGrab" );
            Properties.Settings.Default.Save();
            try
            {
                initFolder();
            }
            catch
            {
            }
            return base.Initialize(core);
        }

        private void initFolder()
        {
            string fld = Properties.Settings.Default.ActiveDataPath;
            if (!System.IO.Directory.Exists(fld))
            {
                System.IO.Directory.CreateDirectory(fld);
            }
            fld = System.IO.Path.Combine(fld, IMG_SUBFOLDER);
            if (!System.IO.Directory.Exists(fld))
            {
                System.IO.Directory.CreateDirectory(fld);
            }

            InitDatabase();
        }

        public override void Close()
        {
            CloseDatabase();
            base.Close();
        }

        private string DatabaseFilename
        {
            get { return System.IO.Path.Combine(Properties.Settings.Default.ActiveDataPath, DATABASE_FILENAME); }
        }

        private void CloseDatabase()
        {
            if (_dbcon != null)
            {
                _dbcon.Dispose();
                _dbcon = null;
            }
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
                }
                result = true;
            }
            catch
            {
            }
            return result;
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.ImageResource;
            }
        }

        public void CreateImageFolderForGeocaches(List<Framework.Data.Geocache> gcList, string folder)
        {
            using (CopyToFolderForm dlg = new CopyToFolderForm(folder))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _gcList = (from a in gcList select a).ToList();
                    if (Properties.Settings.Default.DownloadBeforeCreate)
                    {
                        List<Framework.Data.Geocache> tmpList = (from a in _gcList select a).ToList();
                        System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_GRABNEW), Utils.LanguageSupport.Instance.GetTranslation(STR_GRABIMAGES), System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                        _grabOnlyNew = (dr == System.Windows.Forms.DialogResult.Yes);
                        if (dr != System.Windows.Forms.DialogResult.Cancel)
                        {
                            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_GRABBING_IMAGES, STR_GRABBING_IMAGES, _gcList.Count, 0, true))
                            {
                                int cnt = _gcList.Count;
                                _orgListCount = cnt;
                                Thread[] thrd = new Thread[4];
                                for (int i = 0; i < thrd.Length; i++)
                                {
                                    thrd[i] = new Thread(new ThreadStart(this.getImagesThreadMethod));
                                    thrd[i].Start();
                                }
                                while (cnt > 0)
                                {
                                    Thread.Sleep(500);
                                    System.Windows.Forms.Application.DoEvents();
                                    lock (_gcList)
                                    {
                                        cnt = _gcList.Count;
                                    }
                                    if (!progress.UpdateProgress(STR_GRABBING_IMAGES, STR_GRABBING_IMAGES, _orgListCount, _orgListCount - cnt))
                                    {
                                        lock (_gcList)
                                        {
                                            _gcList.Clear();
                                            cnt = 0;
                                        }
                                    }
                                }
                                for (int i = 0; i < thrd.Length; i++)
                                {
                                    thrd[i].Join();
                                }
                            }
                        }
                        _gcList = tmpList;
                    }
                    _threadReady = new ManualResetEvent(false);
                    Thread thrd2 = new Thread(new ThreadStart(this.copyToFolderThreadMethod));
                    thrd2.Start();
                    while (!_threadReady.WaitOne(100))
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                    thrd2.Join();
                }
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_GRAB_ACTIVE || action == ACTION_GRAB_DELETE_ACTIVE || action == ACTION_CREATE_ACTIVE)
                {
                    if (Core.ActiveGeocache != null)
                    {
                        _gcList = new List<Framework.Data.Geocache>();
                        _gcList.Add(Core.ActiveGeocache);
                    }
                }
                else if (action == ACTION_GRAB_SELECTED || action == ACTION_GRAB_DELETE_SELECTED || action == ACTION_CREATE_SELECTED)
                {
                    _gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                }
                else if (action == ACTION_GRAB_DELETE_ALL)
                {
                    _gcList = (from Framework.Data.Geocache w in Core.Geocaches select w).ToList();
                }
                if (_gcList == null || _gcList.Count == 0)
                {
                    System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOCACHESSELECTED), Utils.LanguageSupport.Instance.GetTranslation("Information"), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);                    
                }
                else
                {
                    if (action == ACTION_CREATE_ACTIVE || action == ACTION_CREATE_SELECTED)
                    {
                        using (CopyToFolderForm dlg = new CopyToFolderForm())
                        {
                            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                if (Properties.Settings.Default.DownloadBeforeCreate)
                                {
                                    List<Framework.Data.Geocache> tmpList = (from a in _gcList select a).ToList();
                                    Action(action == ACTION_CREATE_ACTIVE ? ACTION_GRAB_ACTIVE : ACTION_GRAB_SELECTED);
                                    _gcList = tmpList;
                                }
                                _threadReady = new ManualResetEvent(false);
                                Thread thrd = new Thread(new ThreadStart(this.copyToFolderThreadMethod));
                                thrd.Start();
                                while (!_threadReady.WaitOne(100))
                                {
                                    System.Windows.Forms.Application.DoEvents();
                                }
                                thrd.Join();
                            }
                        }
                    }
                    else if (action == ACTION_GRAB_ACTIVE || action == ACTION_GRAB_SELECTED)
                    {
                        System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_GRABNEW), Utils.LanguageSupport.Instance.GetTranslation(STR_GRABIMAGES), System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                        _grabOnlyNew = (dr == System.Windows.Forms.DialogResult.Yes);                        
                        if (dr != System.Windows.Forms.DialogResult.Cancel)
                        {
                            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_GRABBING_IMAGES, STR_GRABBING_IMAGES, _gcList.Count, 0, true))
                            {
                                int cnt = _gcList.Count;
                                _orgListCount = cnt;
                                Thread[] thrd = new Thread[4];
                                for (int i = 0; i < thrd.Length; i++)
                                {
                                    thrd[i] = new Thread(new ThreadStart(this.getImagesThreadMethod));
                                    thrd[i].Start();
                                }
                                while (cnt > 0)
                                {
                                    Thread.Sleep(500);
                                    System.Windows.Forms.Application.DoEvents();
                                    lock (_gcList)
                                    {
                                        cnt = _gcList.Count;
                                    }
                                    if (!progress.UpdateProgress(STR_GRABBING_IMAGES, STR_GRABBING_IMAGES, _orgListCount, _orgListCount - cnt))
                                    {
                                        lock (_gcList)
                                        {
                                            _gcList.Clear();
                                            cnt = 0;
                                        }
                                    }
                                }
                                for (int i = 0; i < thrd.Length; i++)
                                {
                                    thrd[i].Join();
                                }
                            }
                        }
                    }
                    else if (action == ACTION_GRAB_DELETE_ACTIVE || action == ACTION_GRAB_DELETE_SELECTED || action == ACTION_GRAB_DELETE_ALL)
                    {
                        _threadReady = new ManualResetEvent(false);
                        Thread thrd = new Thread(new ThreadStart(this.deleteImagesThreadMethod));
                        thrd.Start();
                        while (!_threadReady.WaitOne(100))
                        {
                            System.Windows.Forms.Application.DoEvents();
                        }
                        thrd.Join();
                    }
                }
            }
            return result;
        }

        private void copyToFolderThreadMethod()
        {
            DateTime nextUpdate = DateTime.Now.AddSeconds(2);
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_CREATINGFOLDER, STR_CREATINGFOLDER, _gcList.Count, 0, true))
            {
                try
                {
                    if (!Directory.Exists(Properties.Settings.Default.CreateFolderPath))
                    {
                        Directory.CreateDirectory(Properties.Settings.Default.CreateFolderPath);
                    }
                    string imgFolder = Path.Combine(Properties.Settings.Default.CreateFolderPath, "GeocachePhotos");
                    if (!Directory.Exists(imgFolder))
                    {
                        Directory.CreateDirectory(imgFolder);
                    }
                    if (Properties.Settings.Default.ClearBeforeCopy)
                    {
                        string[] fls = Directory.GetFiles(imgFolder);
                        if (fls != null)
                        {
                            foreach (string f in fls)
                            {
                                File.Delete(f);
                            }
                        }
                        fls = Directory.GetDirectories(imgFolder);
                        if (fls != null)
                        {
                            foreach (string f in fls)
                            {
                                Directory.Delete(f, true);
                            }
                        }
                    }

                    int index = 0;
                    foreach (Framework.Data.Geocache gc in _gcList)
                    {
                        List<string> linkList = new List<string>();
                        bool folderCreated = false;

                        List<Framework.Data.GeocacheImage> imgList = DataAccess.GetGeocacheImages(Core.GeocacheImages, gc.Code);
                        if (imgList != null)
                        {
                            foreach (Framework.Data.GeocacheImage img in imgList)
                            {
                                linkList.Add(img.Url);
                            }
                        }

                        StringBuilder sb = new StringBuilder();
                        lock (_lockDBObject)
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

                                    string lnk = s.Substring(pos + 1, pos2 - pos - 1);
                                    if (!linkList.Contains(lnk))
                                    {
                                        if (!Properties.Settings.Default.CopyNotInDescription)
                                        {
                                            linkList.Add(lnk);
                                        }
                                    }
                                    else if (Properties.Settings.Default.CopyNotInDescription)
                                    {
                                        //remove the entries that are within the description
                                        linkList.Remove(lnk);
                                    }

                                }
                            }
                        }

                        if (linkList.Count > 0)
                        {
                            int imgIndex = 1;
                            string cacheFolder = "";
                            foreach (string link in linkList)
                            {
                                string fn = Utils.ImageSupport.Instance.GetImagePath(link);
                                if (link != fn)
                                {
                                    if (!folderCreated)
                                    {
                                        cacheFolder = Path.Combine(imgFolder, gc.Code[gc.Code.Length - 1].ToString());
                                        if (!Directory.Exists(cacheFolder))
                                        {
                                            Directory.CreateDirectory(cacheFolder);
                                        }
                                        cacheFolder = Path.Combine(cacheFolder, gc.Code[gc.Code.Length - 2].ToString());
                                        if (!Directory.Exists(cacheFolder))
                                        {
                                            Directory.CreateDirectory(cacheFolder);
                                        }
                                        cacheFolder = Path.Combine(cacheFolder, gc.Code);
                                        if (!Directory.Exists(cacheFolder))
                                        {
                                            Directory.CreateDirectory(cacheFolder);
                                        }
                                        folderCreated = true;
                                    }

                                    string imgname = imgIndex.ToString();
                                    if (imgList != null)
                                    {
                                        Framework.Data.GeocacheImage img = (from a in imgList where a.Url == link select a).FirstOrDefault();
                                        if (img != null)
                                        {
                                            imgname = string.Format("{1}{0}", imgIndex,getValidNameForFile(img.Name));
                                        }
                                    }
                                    string dst = Path.Combine(cacheFolder, string.Format("{0}{1}", imgname, Path.GetExtension(fn)));
                                    if (!File.Exists(dst))
                                    {
                                        File.Copy(fn, dst);
                                    }
                                }

                                imgIndex++;
                            }
                        }

                        index++;
                        if (DateTime.Now>=nextUpdate)
                        {
                            if (!progress.UpdateProgress(STR_CREATINGFOLDER, STR_CREATINGFOLDER, _gcList.Count, index))
                            {
                                break;
                            }
                            nextUpdate = DateTime.Now.AddSeconds(2);
                        }
                    }
                }
                catch
                {
                }
            }
            _threadReady.Set();
        }

        private string getValidNameForFile(string name)
        {
            string validChars = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890";
            StringBuilder sb = new StringBuilder();
            for (int i=0; i<name.Length; i++)
            {
                if (validChars.Contains(name[i]))
                {
                    sb.Append(name[i]);
                }
                else
                {
                    //nope
                }
            }
            return sb.ToString();
        }

        private void deleteImagesThreadMethod()
        {
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_DELETING_GRABBED_IMAGES, STR_DELETING_IMAGES, _gcList.Count, 0, true))
            {
                try
                {
                    int block = 0;
                    string fnp = System.IO.Path.Combine(Properties.Settings.Default.ActiveDataPath, IMG_SUBFOLDER);
                    for (int i = 0; i < _gcList.Count; i++)
                    {
                        DbDataReader dr = _dbcon.ExecuteReader(string.Format("select local_file from images where gccode='{0}'", _gcList[i].Code.Replace("'", "''")));
                        while (dr.Read())
                        {
                            string fn = System.IO.Path.Combine(fnp, dr.GetString(0));
                            try
                            {
                                if (System.IO.File.Exists(fn))
                                {
                                    System.IO.File.Delete(fn);
                                }
                            }
                            catch
                            {
                            }
                        }
                        _dbcon.ExecuteNonQuery(string.Format("delete from images where gccode='{0}'", _gcList[i].Code.Replace("'", "''")));

                        block++;
                        if (block > 10)
                        {
                            block = 0;
                            if (!progress.UpdateProgress(STR_DELETING_GRABBED_IMAGES, STR_DELETING_IMAGES, _gcList.Count, i))
                            {
                                break;
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            _threadReady.Set();
        }

        private void getImagesThreadMethod()
        {
            Framework.Data.Geocache gc = null;
            lock (_gcList)
            {
                if (_gcList.Count > 0)
                {
                    gc = _gcList[0];
                    _gcList.RemoveAt(0);
                }
            }
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                string fnp = System.IO.Path.Combine(Properties.Settings.Default.ActiveDataPath, IMG_SUBFOLDER);
                bool grabOnlyNew = _grabOnlyNew;
                while (gc != null)
                {
                    //todo: get images
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        lock (_lockDBObject)
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

                            List<Framework.Data.GeocacheImage> imgList = DataAccess.GetGeocacheImages(Core.GeocacheImages, gc.Code);
                            if (imgList != null)
                            {
                                foreach (Framework.Data.GeocacheImage img in imgList)
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
                                    lock (_lockDBObject)
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
                                                lock (_lockDBObject)
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
                    }

                    gc = null;
                    lock (_gcList)
                    {
                        if (_gcList.Count > 0)
                        {
                            gc = _gcList[0];
                            _gcList.RemoveAt(0);
                        }
                    }
                }
            }
        }


        public string GetImagePath(Framework.Data.ImageSize imageSize, Framework.Data.GeocacheType geocacheType)
        {
            return null;
        }

        public string GetImagePath(Framework.Data.ImageSize imageSize, Framework.Data.GeocacheType geocacheType, bool corrected)
        {
            return null;
        }

        public string GetImagePath(Framework.Data.ImageSize imageSize, Framework.Data.GeocacheAttribute attr, Framework.Data.GeocacheAttribute.State state)
        {
            return null;
        }

        public string GetImagePath(Framework.Data.ImageSize imageSize, Framework.Data.LogType logType)
        {
            return null;
        }

        public string GetImagePath(Framework.Data.ImageSize imageSize, Framework.Data.WaypointType waypointType)
        {
            return null;
        }

        public string GetImagePath(Framework.Data.ImageSize imageSize, Framework.Data.GeocacheContainer container)
        {
            return null;
        }

        public string GetImagePath(Framework.Data.ImageSize imageSize, Framework.Data.CompassDirection compassDir)
        {
            return null;
        }

        public bool GrabImages(Framework.Data.Geocache gc)
        {
            bool result = false;
            try
            {
                //todo
                //get img urls from gc

                //download the image
                //load img and save as jpg (use guid for name)
                //update database
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
                object o = _dbcon.ExecuteScalar(string.Format("select local_file from images where org_url = '{0}' limit 1", orgUrl.Replace("'","''")));
                if (o != null && o.GetType() != typeof(DBNull))
                {
                    result = System.IO.Path.Combine(new string[] { Properties.Settings.Default.ActiveDataPath, IMG_SUBFOLDER, o as string });
                }
            }
            catch
            {
            }
            return result;
        }

        public string GetImagePath(Framework.Data.Geocache gc,string orgUrl)
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
                        result = System.IO.Path.Combine(new string[] { Properties.Settings.Default.ActiveDataPath, IMG_SUBFOLDER, o as string });
                    }
                }
                catch
                {
                }
            }
            return result;
        }
    }
}
