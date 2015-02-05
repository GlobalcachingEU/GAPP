using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace GlobalcachingApplication.Plugins.LogImagesViewer
{
    public partial class ThumbsOverviewForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Log image thumbs overview";
        public const string STR_FINDER = "Finder";
        public const string STR_NAME = "Name";
        public const string STR_GCCODE = "GC Code";
        public const string STR_GCNAME = "GC Name";
        public const string STR_GCOWNER = "GC Owner";
        public const string STR_APPLY = "Apply";

        private const int MAX_PAGESIZE = 100;

        private string _cacheFolder = null;
        private List<ThumbInfo> _allThumbInfos = new List<ThumbInfo>();
        private List<ThumbInfo> _allFilteredThumbInfos = new List<ThumbInfo>();
        private int _currentPage = 0;
        private bool _updatingPage = false;

        private List<ThumbInfo> _logImagesToGrab = null;
        private List<ThumbInfo> _logImagesToGrabbed = null;

        public ThumbsOverviewForm()
        {
            InitializeComponent();
        }

        public ThumbsOverviewForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            var p = PluginSettings.Instance.WindowPos2;
            if (p != null && !p.IsEmpty)
            {
                this.Bounds = p;
                this.StartPosition = FormStartPosition.Manual;
            }

            _cacheFolder = System.IO.Path.Combine(core.PluginDataPath, "LogImageCache");
            try
            {
                if (!System.IO.Directory.Exists(_cacheFolder))
                {
                    System.IO.Directory.CreateDirectory(_cacheFolder);
                }
                _cacheFolder = System.IO.Path.Combine(_cacheFolder, "Thumbs");
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

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            base.SelectedLanguageChanged(sender, e);
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FINDER);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GCCODE);
            this.label8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GCNAME);
            this.label10.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GCOWNER);
            this.button6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_APPLY);
            if (this.Visible)
            {
                updatePageView();
            }
        }

        private void updatePageView()
        {
            flowLayoutPanel1.SuspendLayout();

            ThumbInfo[] tii = _allFilteredThumbInfos.Skip(_currentPage * MAX_PAGESIZE).Take(MAX_PAGESIZE).ToArray();

            while (flowLayoutPanel1.Controls.Count > tii.Length)
            {
                flowLayoutPanel1.Controls.RemoveAt(flowLayoutPanel1.Controls.Count - 1);
            }
            while (flowLayoutPanel1.Controls.Count < tii.Length)
            {
                flowLayoutPanel1.Controls.Add(new ThumbInfoItem(null, Core));
            }
            for (int i = 0; i < tii.Length; i++)
            {
                tii[i].ImageFileLocation = null;
                (flowLayoutPanel1.Controls[i] as ThumbInfoItem).ImgInfo = tii[i];
            }

            flowLayoutPanel1.ResumeLayout(true);


            //get the images
            _logImagesToGrab = tii.ToList();
            _logImagesToGrabbed = new List<ThumbInfo>();
            int cnt = _logImagesToGrab.Count;
            Thread[] thrd = new Thread[6];
            for (int i = 0; i < thrd.Length; i++)
            {
                thrd[i] = new Thread(new ThreadStart(this.getImagesThreadMethod));
                thrd[i].Start();
            }
            while (cnt > 0)
            {
                Thread.Sleep(50);
                Application.DoEvents();//todo: ERROR
                lock (_logImagesToGrab)
                {
                    cnt = _logImagesToGrab.Count;
                }
                lock (_logImagesToGrabbed)
                {
                    foreach (var li in _logImagesToGrabbed)
                    {
                        if (!string.IsNullOrEmpty(li.ImageFileLocation))
                        {
                            foreach (ThumbInfoItem thumb in flowLayoutPanel1.Controls)
                            {
                                if (thumb.ImgInfo == li)
                                {
                                    thumb.SetImage(thumb.ImgInfo.ImageFileLocation);
                                }
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < thrd.Length; i++)
            {
                thrd[i].Join();
            }
            foreach (var li in _logImagesToGrabbed)
            {
                if (!string.IsNullOrEmpty(li.ImageFileLocation))
                {
                    foreach (ThumbInfoItem thumb in flowLayoutPanel1.Controls)
                    {
                        if (thumb.ImgInfo == li)
                        {
                            thumb.SetImage(thumb.ImgInfo.ImageFileLocation);
                        }
                    }
                }
            }
        }

        private void getImagesThreadMethod()
        {
            ThumbInfo li = null;
            lock (_logImagesToGrab)
            {
                if (_logImagesToGrab.Count > 0)
                {
                    li = _logImagesToGrab[0];
                    _logImagesToGrab.RemoveAt(0);
                }
            }
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                string fnp = System.IO.Path.Combine(Core.PluginDataPath, "LogImageCache", "Thumbs");
                while (li != null)
                {
                    try
                    {
                        string fn = System.IO.Path.Combine(fnp, System.IO.Path.GetFileName(li.LogImage.Url));
                        if (!System.IO.File.Exists(fn))
                        {
                            wc.DownloadFile(li.LogImage.Url.Replace("/log/", "/log/thumb/"), fn);
                        }
                        li.ImageFileLocation = fn;
                        lock (_logImagesToGrabbed)
                        {
                            _logImagesToGrabbed.Add(li);
                        }
                    }
                    catch
                    {
                    }

                    li = null;
                    lock (_logImagesToGrab)
                    {
                        if (_logImagesToGrab.Count > 0)
                        {
                            li = _logImagesToGrab[0];
                            _logImagesToGrab.RemoveAt(0);
                        }
                    }
                }
            }
        }


        private void setPageIndex(int index)
        {
            if (!_updatingPage)
            {
                _updatingPage = true;
                groupBox1.Enabled = false;
                panel2.Enabled = false;
                this.Cursor = Cursors.WaitCursor;
                _currentPage = index;
                numericUpDown1.Value = 1;
                int maxIndex = _allFilteredThumbInfos.Count / MAX_PAGESIZE;
                if ((_allFilteredThumbInfos.Count % MAX_PAGESIZE > 0) || maxIndex==0 )
                {
                    maxIndex++;
                }
                numericUpDown1.Maximum = maxIndex;
                numericUpDown1.Value = _currentPage+1;
                button1.Enabled = index > 0;
                button2.Enabled = index > 0;
                button4.Enabled = index < maxIndex && _allFilteredThumbInfos.Count > MAX_PAGESIZE;
                button3.Enabled = index < maxIndex && _allFilteredThumbInfos.Count > MAX_PAGESIZE;
                updatePageView();
                groupBox1.Enabled = true;
                panel2.Enabled = true;
                this.Cursor = Cursors.Default;
                _updatingPage = false;
            }
        }

        private void applyFilter()
        {
            _allFilteredThumbInfos = (from a in _allThumbInfos where
                                          (textBox1.Text.Length==0 || (a.Log.Finder!=null && a.Log.Finder.IndexOf(textBox1.Text, StringComparison.InvariantCultureIgnoreCase)>=0)) &&
                                          (textBox2.Text.Length == 0 || (a.LogImage.Name != null && a.LogImage.Name.IndexOf(textBox2.Text, StringComparison.InvariantCultureIgnoreCase) >= 0)) &&
                                          (textBox3.Text.Length == 0 || (a.Geocache.Code.IndexOf(textBox3.Text, StringComparison.InvariantCultureIgnoreCase) >= 0)) &&
                                          (textBox4.Text.Length == 0 || (a.Geocache.Name != null && a.Geocache.Name.IndexOf(textBox4.Text, StringComparison.InvariantCultureIgnoreCase) >= 0)) &&
                                          (textBox5.Text.Length == 0 || (a.Geocache.Owner != null && a.Geocache.Owner.IndexOf(textBox5.Text, StringComparison.InvariantCultureIgnoreCase) >= 0))
                                      select a).ToList();
        }

        public void UpdateView()
        {
            _allThumbInfos = (from Framework.Data.LogImage li in Core.LogImages
                       join Framework.Data.Log l in Core.Logs on li.LogID equals l.ID
                       join Framework.Data.Geocache g in Core.Geocaches on l.GeocacheCode equals g.Code
                       orderby l.Date descending
                       select new ThumbInfo { Geocache = g, Log = l, LogImage = li }).ToList();

            applyFilter();
            setPageIndex(0);
        }

        private void ThumbsOverviewForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void ThumbsOverviewForm_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal && this.Visible)
            {
                PluginSettings.Instance.WindowPos2 = this.Bounds;
            }
        }

        private void ThumbsOverviewForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal && this.Visible)
            {
                PluginSettings.Instance.WindowPos2 = this.Bounds;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            setPageIndex(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            setPageIndex(_currentPage-1);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            setPageIndex(_currentPage + 1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            setPageIndex((int)numericUpDown1.Maximum-1);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            setPageIndex((int)numericUpDown1.Value-1);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int collapsed = button5.Height+5;
            int expanded = groupBox1.Height+5;
            if (panel1.Height > collapsed)
            {
                panel1.Height = collapsed;
                groupBox1.Visible = false;
                button5.Text = "v";
            }
            else
            {
                panel1.Height = expanded;
                groupBox1.Visible = true;
                button5.Text = "^";
            }
        }

        private void flowLayoutPanel1_MouseEnter(object sender, EventArgs e)
        {
            flowLayoutPanel1.Focus();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            applyFilter();
            setPageIndex(0);
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                button6_Click(sender, EventArgs.Empty);
            }
        }
    }
}
