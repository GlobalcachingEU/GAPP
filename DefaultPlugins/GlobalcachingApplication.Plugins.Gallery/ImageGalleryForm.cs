using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace GlobalcachingApplication.Plugins.Gallery
{
    public partial class ImageGalleryForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_ERROR = "Error";
        public const string STR_IMPORTINGMYF = "Importing geocaching.com my finds...";
        public const string STR_IMPORTINGLOGS = "Importing geocaching.com logs...";
        public const string STR_TITLE = "Image Gallery";
        public const string STR_UPDATE = "Update";
        public const string STR_SHOW = "Show";
        public const string STR_SORTON = "Sort on";
        public const string STR_SORTORDER = "Sort order";
        public const string STR_ALL = "All";
        public const string STR_ACTIVEGEOCACHE = "Active geocache";
        public const string STR_TEXT = "Text";
        public const string STR_DATE = "Date";
        public const string STR_NAME = "Name";
        public const string STR_ASC = "Ascending";
        public const string STR_DESC = "Descending";

        private ManualResetEvent _actionReady = null;
        private string _errormessage = null;
        private List<ImageInfo> _imageInfoList = new List<ImageInfo>();
        private string _databaseFile;
        private string _imgFolder;
        private string _thumbFolder;

        public ImageGalleryForm()
        {
            InitializeComponent();
        }

        public ImageGalleryForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            try
            {
                _databaseFile = System.IO.Path.Combine(new string[] { core.PluginDataPath, "gallery.db3" });
                string p = System.IO.Path.Combine(new string[] { core.PluginDataPath, "ImageGallery" });
                if (!System.IO.Directory.Exists(p))
                {
                    System.IO.Directory.CreateDirectory(p);
                }
                _imgFolder = p;
                p = System.IO.Path.Combine(new string[] { p, "Thumps" });
                if (!System.IO.Directory.Exists(p))
                {
                    System.IO.Directory.CreateDirectory(p);
                }
                _thumbFolder = p;
            }
            catch
            {
            }

            SelectedLanguageChanged(this, EventArgs.Empty);
            core.ActiveGeocacheChanged += new Framework.EventArguments.GeocacheEventHandler(core_ActiveGeocacheChanged);
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            base.SelectedLanguageChanged(sender, e);

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.buttonUpdate.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_UPDATE);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHOW);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SORTON);
            this.groupBox3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SORTORDER);
            this.radioButtonAll.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ALL);
            this.radioButtonActive.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ACTIVEGEOCACHE);
            this.radioButtonText.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TEXT);
            this.radioButtonDate.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DATE);
            this.radioButtonName.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.radioButtonAscending.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ASC);
            this.radioButtonDescending.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DESC);
        }

        void core_ActiveGeocacheChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (radioButtonActive.Checked)
            {
                UpdateView();
            }
        }

        public void UpdateView()
        {
            if (this.Visible)
            {
                ShowImageThumbnails();
            }
        }

        private void ImageGalleryForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void ShowImageThumbnails()
        {
            ImageInfo[] imgInfoList = null;

            this.Cursor = Cursors.WaitCursor;
            if (radioButtonAll.Checked)
            {
                imgInfoList = (from a in _imageInfoList orderby radioButtonDate.Checked? a.LogVisitDate.ToString("s"): a.Name ascending select a).ToArray();
            }
            else if (radioButtonActive.Checked)
            {
                if (Core.ActiveGeocache != null)
                {
                    imgInfoList = (from a in _imageInfoList where a.LogCacheCode == Core.ActiveGeocache.Code orderby radioButtonDate.Checked ? a.LogVisitDate.ToString("s") : a.Name ascending select a).ToArray();
                }
            }
            else if (radioButtonText.Checked)
            {
                string s = textBoxFilter.Text;
                imgInfoList = (from a in _imageInfoList where a.Name.ToLower().IndexOf(s, StringComparison.OrdinalIgnoreCase)>=0 orderby radioButtonDate.Checked ? a.LogVisitDate.ToString("s") : a.Name ascending select a).ToArray();
            }
            if (imgInfoList==null)
            {
                imgInfoList = new ImageInfo[0];
            }

            flowLayoutPanel1.SuspendLayout();
            while (flowLayoutPanel1.Controls.Count > imgInfoList.Length)
            {
                flowLayoutPanel1.Controls.RemoveAt(flowLayoutPanel1.Controls.Count -1);
            }
            while (flowLayoutPanel1.Controls.Count < imgInfoList.Length)
            {
                flowLayoutPanel1.Controls.Add(new ImageInfoItem(null));
            }
            if (radioButtonDescending.Checked)
            {
                for (int i = 0; i < imgInfoList.Length; i++)
                {
                    (flowLayoutPanel1.Controls[i] as ImageInfoItem).ImgInfo = imgInfoList[imgInfoList.Length-i-1];
                }
            }
            else
            {
                for (int i = 0; i < imgInfoList.Length; i++)
                {
                    (flowLayoutPanel1.Controls[i] as ImageInfoItem).ImgInfo = imgInfoList[i];
                }
            }
            flowLayoutPanel1.ResumeLayout(true);
            this.Cursor = Cursors.Default;
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            _errormessage = null;
            _actionReady = new ManualResetEvent(false);
            Thread thrd = new Thread(new ThreadStart(this.getImagesThreadMethod));
            thrd.Start();
            while (!_actionReady.WaitOne(100))
            {
                System.Windows.Forms.Application.DoEvents();
            }
            thrd.Join();
            if (!string.IsNullOrEmpty(_errormessage))
            {
                System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            ShowImageThumbnails();
        }

        private void getImagesThreadMethod()
        {
            try
            {
                using (Utils.ProgressBlock blockprogress = new Utils.ProgressBlock(OwnerPlugin as Utils.BasePlugin.Plugin, STR_IMPORTINGMYF, STR_IMPORTINGMYF, 1, 0))
                {
                    try
                    {
                        Utils.DBCon dbcon = initDatabase();

                        var logs = new List<Utils.API.LiveV6.GeocacheLog>();

                        using (System.Net.WebClient wc = new System.Net.WebClient())
                        using (var api = new Utils.API.GeocachingLiveV6(Core))
                        {
                            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(OwnerPlugin as Utils.BasePlugin.Plugin, STR_IMPORTINGMYF, STR_IMPORTINGLOGS, 100, 0, true))
                            {
                                var req = new Utils.API.LiveV6.GetUsersGeocacheLogsRequest();
                                req.AccessToken = api.Token;
                                req.ExcludeArchived = false;
                                req.MaxPerPage = 50;
                                req.StartIndex = 0;
                                req.LogTypes = (from a in Core.LogTypes select (long)a.ID).ToArray();
                                var resp = api.Client.GetUsersGeocacheLogs(req);
                                while (resp.Status.StatusCode == 0)
                                {
                                    logs.AddRange(resp.Logs);
                                    foreach (var log in resp.Logs)
                                    {
                                        if (log.Images != null && !log.IsArchived)
                                        {
                                            foreach (var li in log.Images)
                                            {
                                                string imgGuid = li.ImageGuid.ToString("N");
                                                ImageInfo ii = (from a in _imageInfoList where a.ImageGuid==imgGuid select a).FirstOrDefault();
                                                if (ii == null)
                                                {
                                                    ii = new ImageInfo();
                                                    ii.Description = li.Description;
                                                    ii.ImageGuid = imgGuid;
                                                    ii.MobileUrl = li.MobileUrl ?? "";
                                                    ii.Name = li.Name ?? "";
                                                    ii.ThumbUrl = li.ThumbUrl;
                                                    ii.Url = li.Url;
                                                    ii.LogCacheCode = log.CacheCode;
                                                    ii.LogCode = log.Code;
                                                    ii.LogUrl = log.Url;
                                                    ii.LogVisitDate = log.VisitDate;

                                                    ii.ThumbFile = Path.Combine(_thumbFolder, Path.GetFileName(li.ThumbUrl));
                                                    ii.ImgFile = Path.Combine(_imgFolder, Path.GetFileName(li.Url));

                                                    _imageInfoList.Add(ii);

                                                    dbcon.ExecuteNonQuery(string.Format("insert into gallery (ImageGuid, ThumbUrl, Description, Name, MobileUrl, Url, LogCacheCode, LogCode, LogUrl, LogVisitDate, ThumbFile, ImgFile) values ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}')", ii.ImageGuid.Replace("'", "''"), ii.ThumbUrl.Replace("'", "''"), ii.Description.Replace("'", "''"), ii.Name.Replace("'", "''"), ii.MobileUrl.Replace("'", "''"), ii.Url.Replace("'", "''"), ii.LogCacheCode.Replace("'", "''"), ii.LogCode.Replace("'", "''"), ii.LogUrl.Replace("'", "''"), ii.LogVisitDate.ToString("s").Replace("'", "''"), ii.ThumbFile.Replace("'", "''"), ii.ImgFile.Replace("'", "''")));
                                                }
                                                //check if local file(s) exist(s)
                                                //not fail on img download!
                                                try
                                                {
                                                    if (!File.Exists(ii.ThumbFile))
                                                    {
                                                        wc.DownloadFile(li.ThumbUrl, ii.ThumbFile);
                                                    }
                                                    if (!File.Exists(ii.ImgFile))
                                                    {
                                                        wc.DownloadFile(li.Url, ii.ImgFile);
                                                    }
                                                }
                                                catch
                                                {
                                                }
                                            }
                                        }
                                    }

                                    if (resp.Logs.Count() >= req.MaxPerPage)
                                    {
                                        req.StartIndex = logs.Count;
                                        if (!progress.UpdateProgress(STR_IMPORTINGMYF, STR_IMPORTINGLOGS, logs.Count + req.MaxPerPage, logs.Count))
                                        {
                                            break;
                                        }
                                        Thread.Sleep(1500);
                                        resp = api.Client.GetUsersGeocacheLogs(req);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                if (resp.Status.StatusCode != 0)
                                {
                                    _errormessage = resp.Status.StatusMessage;
                                }
                            }

                        }
                        if (dbcon != null)
                        {
                            dbcon.Dispose();
                            dbcon = null;
                        }
                    }
                    catch (Exception e)
                    {
                        _errormessage = e.Message;
                    }
                }
            }
            catch
            {
            }
            _actionReady.Set();
        }

        private void ImageGalleryForm_Shown(object sender, EventArgs e)
        {
            //get from database
            Utils.DBCon dbcon = initDatabase();
            if (dbcon!=null)
            {
                DbDataReader dr = dbcon.ExecuteReader("select * from gallery");
                while (dr.Read())
                {
                    ImageInfo ai = new ImageInfo();
                    ai.Description = dr["Description"] as string;
                    ai.ImageGuid = dr["ImageGuid"] as string;
                    ai.ImgFile = dr["ImgFile"] as string;
                    ai.LogCacheCode = dr["LogCacheCode"] as string;
                    ai.LogCode = dr["LogCode"] as string;
                    ai.LogUrl = dr["LogUrl"] as string;
                    ai.LogVisitDate = DateTime.Parse(dr["LogVisitDate"] as string);
                    ai.MobileUrl = dr["MobileUrl"] as string;
                    ai.Name = dr["Name"] as string;
                    ai.ThumbFile = dr["ThumbFile"] as string;
                    ai.ThumbUrl = dr["ThumbUrl"] as string;
                    ai.Url = dr["Url"] as string;
                    _imageInfoList.Add(ai);
                }
                dbcon.Dispose();
                dbcon = null;
            }
            ShowImageThumbnails();
        }

        private Utils.DBCon initDatabase()
        {
            Utils.DBCon dbcon;
            try
            {
                dbcon = new Utils.DBConComSqlite(_databaseFile);
                object o = dbcon.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='gallery'");
                if (o == null || o.GetType() == typeof(DBNull))
                {
                    dbcon.ExecuteNonQuery("create table 'gallery' (ImageGuid text, ThumbUrl text, Description text, Name text, MobileUrl text, Url text, LogCacheCode text, LogCode text, LogUrl text, LogVisitDate text, ThumbFile text, ImgFile text)");
                }
            }
            catch
            {
                dbcon = null;
            }
            return dbcon;
        }

        private void radioButtonAll_CheckedChanged(object sender, EventArgs e)
        {
            ShowImageThumbnails();
        }

        private void textBoxFilter_TextChanged(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            if (radioButtonText.Checked)
            {
                timer1.Enabled = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            ShowImageThumbnails();
        }

        private void radioButtonDate_CheckedChanged(object sender, EventArgs e)
        {
            ShowImageThumbnails();
        }

        private void radioButtonDescending_CheckedChanged(object sender, EventArgs e)
        {
            ShowImageThumbnails();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (panel1.Height > 100)
            {
                panel1.Height = 32;
                button1.Text = "v";
            }
            else
            {
                panel1.Height = 115;
                button1.Text = "^";
            }
        }

        private void flowLayoutPanel1_MouseEnter(object sender, EventArgs e)
        {
            flowLayoutPanel1.Focus();
        }

    }
}
