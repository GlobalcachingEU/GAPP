using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.LogImagesViewer
{
    public partial class ViewerForm : Utils.BasePlugin.BaseUIChildWindowForm, IImageDownloaderCallback
    {
        public const string STR_TITLE = "Log image viewer";
        public const string STR_SLIDESHOW = "Slideshow";
        public const string STR_DATE = "Date";
        public const string STR_NAME = "Name";
        public const string STR_LOG = "Log";
        public const string STR_BY = "By";
        public const string STR_TEXT = "Text";

        private string _currentActiveImageUrl = null;
        private ImageDownloader _imageDownloader = null;
        private string _cacheFolder = null;

        public ViewerForm()
        {
            InitializeComponent();
        }

        public ViewerForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            if (Properties.Settings.Default.WindowPos != null && !Properties.Settings.Default.WindowPos.IsEmpty)
            {
                this.Bounds = Properties.Settings.Default.WindowPos;
                this.StartPosition = FormStartPosition.Manual;
            }

            checkBox1.Checked = Properties.Settings.Default.Slideshow;
            numericUpDown1.Value = Properties.Settings.Default.SlideshowNextDelay;
            timerSlideShow.Interval = Properties.Settings.Default.SlideshowNextDelay * 1000;

            _cacheFolder = System.IO.Path.Combine(core.PluginDataPath, "LogImageCache");
            try
            {
                if (!System.IO.Directory.Exists(_cacheFolder))
                {
                    System.IO.Directory.CreateDirectory(_cacheFolder);
                }
            }
            catch
            {
                _cacheFolder = null;
            }

            SelectedLanguageChanged(this, EventArgs.Empty);
            _imageDownloader = new ImageDownloader(this);

            core.ActiveGeocacheChanged += new Framework.EventArguments.GeocacheEventHandler(core_ActiveGeocacheChanged);
            core.LogImages.ListDataChanged += new EventHandler(LogImages_ListDataChanged);
            core.Logs.ListDataChanged += new EventHandler(LogImages_ListDataChanged);
        }

        void LogImages_ListDataChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                UpdateView();
            }
        }

        void core_ActiveGeocacheChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (this.Visible)
            {
                UpdateView();
            }
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            base.SelectedLanguageChanged(sender, e);
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SLIDESHOW);
            this.listView1.Columns[0].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DATE);
            this.listView1.Columns[1].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOG);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_BY);
            this.label8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TEXT);
        }

        public void UpdateView()
        {
            listView1.Items.Clear();
            if (Core.ActiveGeocache != null)
            {
                List<Framework.Data.LogImage> logImages = Utils.DataAccess.GetLogImages(Core.LogImages, Core.Logs, Core.ActiveGeocache.Code);
                foreach (var li in logImages)
                {
                    Framework.Data.Log l = Core.Logs.GetLog(li.LogID);
                    if (l != null)
                    {
                        ListViewItem lvi = new ListViewItem(new string[] { l.Date.ToString("yyyy-MM-dd"), li.Name ?? "" });
                        lvi.Tag = li;
                        listView1.Items.Add(lvi);
                    }
                }
            }
            checkImageToLoad(this, EventArgs.Empty);
            checkSlideshowTimer();
            if (timerSlideShow.Enabled)
            {
                timerSlideShow_Tick(this, EventArgs.Empty);
            }
        }

        private void ViewerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (_imageDownloader != null)
                {
                    _imageDownloader.Stop();
                }
                e.Cancel = true;
                Hide();
            }
            else
            {
                if (_imageDownloader != null)
                {
                    _imageDownloader.Dispose();
                    _imageDownloader = null;
                }
            }
        }

        private void ViewerForm_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void ViewerForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            timer1.Enabled = true;
        }

        private void checkImageToLoad(object sender, EventArgs e)
        {
            string targetImageUrl = null;
            if (listView1.SelectedItems.Count > 0)
            {
                Framework.Data.LogImage li = listView1.SelectedItems[0].Tag as Framework.Data.LogImage;
                if (li != null)
                {
                    Framework.Data.Log l = Core.Logs.GetLog(li.LogID);
                    if (l != null)
                    {
                        linkLabel1.Links.Clear();
                        linkLabel1.Text = l.ID;
                        label4.Text = l.Finder;
                        textBox1.Text = l.Text;
                    }
                    else
                    {
                        linkLabel1.Links.Clear();
                        linkLabel1.Text = "-";
                        label4.Text = "-";
                        textBox1.Text = "";
                    }
                    if (!Properties.Settings.Default.CacheImages && _cacheFolder != null)
                    {
                        targetImageUrl = li.Url;
                    }
                    else
                    {
                        string fn = System.IO.Path.Combine(_cacheFolder, System.IO.Path.GetFileName(li.Url));
                        if (System.IO.File.Exists(fn))
                        {
                            _currentActiveImageUrl = li.Url;
                            LoadCompleted(System.IO.File.ReadAllBytes(fn), li.Url, false);
                            return;
                        }
                        else
                        {
                            targetImageUrl = li.Url;
                        }
                    }
                }
                else
                {
                    linkLabel1.Links.Clear();
                    linkLabel1.Text = "-";
                    label4.Text = "-";
                    textBox1.Text = "";
                }
            }

            if (targetImageUrl == _currentActiveImageUrl)
            {
                //ignore
            }
            else
            {
                _imageDownloader.Stop();
                if (targetImageUrl == null)
                {
                    Image prevImage = pictureBox1.Image;
                    pictureBox1.Image = null;
                    if (prevImage != null)
                    {
                        prevImage.Dispose();
                    }
                }
                else
                {
                    _imageDownloader.LoadAsync(targetImageUrl);
                    toolStripProgressBar1.Value = 0;
                    toolStripProgressBar1.Visible = true;
                }
                _currentActiveImageUrl = targetImageUrl;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            if (this.Visible)
            {
                checkImageToLoad(this, EventArgs.Empty);
            }
        }


        public void LoadProgressChanged(int progress)
        {
            if (!this.IsDisposed && !toolStripProgressBar1.IsDisposed)
            {
                toolStripProgressBar1.Value = Math.Min(progress, 100);
            }
        }

        public void LoadCompleted(byte[] data, string url)
        {
            LoadCompleted(data, url, true);
        }

        public void LoadCompleted(byte[] data, string url, bool overwrite)
        {
            if (_currentActiveImageUrl == url)
            {
                try
                {
                    Image newImage = null;
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream(data))
                    {
                        newImage = new Bitmap(ms);
                    }
                    Image prevImage = pictureBox1.Image;
                    pictureBox1.Image = newImage;
                    if (prevImage != null)
                    {
                        prevImage.Dispose();
                    }

                    if (Properties.Settings.Default.CacheImages && _cacheFolder!=null)
                    {
                        string fn = System.IO.Path.Combine(_cacheFolder, System.IO.Path.GetFileName(url));
                        if (System.IO.File.Exists(fn))
                        {
                            if (!overwrite)
                            {
                                checkSlideshowTimer();
                                return;
                            }
                            System.IO.File.Delete(fn);
                        }
                        System.IO.File.WriteAllBytes(fn, data);
                    }
                }
                catch
                {
                }
                checkSlideshowTimer();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Slideshow = checkBox1.Checked;
            Properties.Settings.Default.Save();
            checkSlideshowTimer();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SlideshowNextDelay = (int)numericUpDown1.Value;
            Properties.Settings.Default.Save();
            checkSlideshowTimer();
        }

        private void checkSlideshowTimer()
        {
            timerSlideShow.Interval = Properties.Settings.Default.SlideshowNextDelay * 1000;
            timerSlideShow.Enabled = Properties.Settings.Default.Slideshow && listView1.Items.Count>0 && this.Visible;
        }

        private void timerSlideShow_Tick(object sender, EventArgs e)
        {
            timerSlideShow.Enabled = false;
            if (Properties.Settings.Default.Slideshow && listView1.Items.Count > 0 && this.Visible)
            {
                int curIndex = -1;
                if (listView1.SelectedIndices.Count > 0 && listView1.Items.Count>1)
                {
                    curIndex = listView1.SelectedIndices[0];
                    listView1.Items[curIndex].Selected = false;
                }
                curIndex++;
                if (curIndex >= listView1.Items.Count)
                {
                    curIndex = 0;
                }
                listView1.Items[curIndex].Selected = true;
            }
        }
    }
}
