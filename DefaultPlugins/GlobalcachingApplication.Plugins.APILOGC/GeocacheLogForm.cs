using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;

namespace GlobalcachingApplication.Plugins.APILOGC
{
    public partial class GeocacheLogForm : Form
    {
        public const string STR_TITLE = "Log a geocache";
        public const string STR_GEOCACHECODE = "Geocache code";
        public const string STR_SELECT = "Select";
        public const string STR_LOGTYPE = "Log type";
        public const string STR_DATE = "Date";
        public const string STR_LOGTEXT = "Log text";
        public const string STR_SUBMIT = "Submit";
        public const string STR_OKANOTHER = "Geocache logged successfully, Log another geocache?";
        public const string STR_SUCCESS = "Success";
        public const string STR_FAIL = "Unable to log the geocache. Reason:";
        public const string STR_ERROR = "Error";
        public const string STR_FAILUNKNOWN = "Unable to log the geocache. Reason unknown";
        public const string STR_TRACKABLES = "Trackables";
        public const string STR_SELECTALL = "Select all";
        public const string STR_CLEARTALL = "Clear all";
        public const string STR_ADD = "Add";
        public const string STR_REMOVE = "Remove";
        public const string STR_ADDTOFAVORITES = "Add to your Favorites";

        private Framework.Interfaces.ICore _core = null;
        private Utils.API.GeocachingLiveV6 _client = null;
        private Framework.Data.Geocache _gc = null;
        private Hashtable _tbTrackingNumbers = new Hashtable();

        private SynchronizationContext _context = null;
        private ManualResetEvent _actionReady;
        private string _errormessage;
        private Framework.Data.LogType _logType;
        private List<string> _tbs14;
        private List<string> _tbs75;
        private DateTime _logDate;
        private string _logText;
        private bool _addToFavorites;

        public bool AskForNext { get; set; }

        public class LogImage
        {
            public System.IO.TemporaryFile imgFile { get; set; }
            public string Caption { get; set; }
            public string Description { get; set; }
            public string FileName { get; set; }
            public PictureBox PB { get; set; }
        }
        private List<LogImage> _logImages = new List<LogImage>();
        private PictureBox _activepb = null;

        public GeocacheLogForm()
        {
            InitializeComponent();
        }

        public GeocacheLogForm(Framework.Interfaces.ICore core, Utils.API.GeocachingLiveV6 client, Framework.Data.Geocache gc)
        {
            InitializeComponent();

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GEOCACHECODE);
            this.buttonSelect.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECT);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOGTYPE);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DATE);
            this.label3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOGTEXT);
            this.buttonSubmit.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SUBMIT);
            this.button1.Text = string.Format("{0} >>", Utils.LanguageSupport.Instance.GetTranslation(STR_TRACKABLES));
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTALL);
            this.button5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTALL);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CLEARTALL);
            this.button4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CLEARTALL);
            this.button6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADD);
            this.removeToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_REMOVE);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDTOFAVORITES);

            _context = SynchronizationContext.Current;
            if (_context == null)
            {
                _context = new SynchronizationContext();
            }

            _core = core;
            _client = client;
            _gc = gc;
            AskForNext = true;
            int[] ids = new int[] { 2, 3, 4, 7, 45, 9, 10 };
            comboBoxLogType1.SetLogTypes(core, (from Framework.Data.LogType l in core.LogTypes join a in ids on l.ID equals a select l).ToList());

            ids = new int[] { 14 };
            comboBoxLogType2.SetLogTypes(core, (from Framework.Data.LogType l in core.LogTypes join a in ids on l.ID equals a select l).ToList());
            comboBoxLogType2.SelectedIndex = 0;

            ids = new int[] { 75 };
            comboBoxLogType3.SetLogTypes(core, (from Framework.Data.LogType l in core.LogTypes join a in ids on l.ID equals a select l).ToList());
            comboBoxLogType3.SelectedIndex = 0;

            updateGCInfo();
        }

        public GeocacheLogForm(Framework.Interfaces.ICore core, Utils.API.GeocachingLiveV6 client, Framework.Data.Geocache gc, GarminGeocacheVisitsLogForm.GeocacheVisitsItem data)
            : this(core, client, gc)
        {
            if (data.LogType.ID > 0)
            {
                comboBoxLogType1.SelectedItem = data.LogType;
                dateTimePicker1.Value = data.LogDate;
                textBox1.Text = data.Comment;
            }
        }

        public GeocacheLogForm(Framework.Interfaces.ICore core, Utils.API.GeocachingLiveV6 client, Framework.Data.Geocache gc, CGeoGeocacheVisitsLogForm.GeocacheVisitsItem data)
            : this(core, client, gc)
        {
            if (data.LogType.ID > 0)
            {
                comboBoxLogType1.SelectedItem = data.LogType;
                dateTimePicker1.Value = data.LogDate;
                textBox1.Text = data.Comment;
            }
        }

        public GeocacheLogForm(Framework.Interfaces.ICore core, Utils.API.GeocachingLiveV6 client, Framework.Data.Geocache gc, OfflineLogForm.OfflineLogInfo data)
            : this(core, client, gc)
        {
            if (data.LogType.ID > 0)
            {
                comboBoxLogType1.SelectedItem = data.LogType;
                dateTimePicker1.Value = data.LogDate;
                textBox1.Text = data.LogText;
                textBoxGC.ReadOnly = true;
            }
        }

        private void updateGCInfo()
        {
            linkLabelGC.Links.Clear();
            if (_gc != null)
            {
                pictureBoxGC.ImageLocation = Utils.ImageSupport.Instance.GetImagePath(_core, Framework.Data.ImageSize.Default, _gc.GeocacheType);
                linkLabelGC.Text = string.Format("{0}, {1}", _gc.Code, _gc.Name);
                linkLabelGC.Links.Add(0, _gc.Code.Length, _gc.Url);
                textBoxGC.Text = _gc.Code;
                buttonSubmit.Enabled = false;
                comboBoxLogType1.SelectedIndex = -1;
                comboBoxLogType1.Enabled = true;
            }
            else
            {
                pictureBoxGC.Image = null;
                linkLabelGC.Text = "-";
                textBoxGC.Text = "";
                buttonSubmit.Enabled = comboBoxLogType1.SelectedIndex>=0;
                comboBoxLogType1.Enabled = false;
            }
        }

        private void linkLabelGC_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }

        private void comboBoxLogType1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Framework.Data.LogType gt = comboBoxLogType1.SelectedItem as Framework.Data.LogType;
            if (gt == null)
            {
                buttonSubmit.Enabled = false;
                checkBox1.Enabled = false;
            }
            else
            {
                buttonSubmit.Enabled = true;
                checkBox1.Enabled = gt.AsFound;
            }
            if (!checkBox1.Enabled)
            {
                checkBox1.Checked = false;
            }
        }

        private void buttonSelect_Click(object sender, EventArgs e)
        {
            _gc = null;
            if (textBoxGC.Text.Length > 0)
            {
                _gc = Utils.DataAccess.GetGeocache(_core.Geocaches, textBoxGC.Text.ToUpper());
                if (_gc == null)
                {
                    //not in our system, try geocaching.com
                    if (textBoxGC.Text.ToUpper().StartsWith("GC"))
                    {
                        Cursor = Cursors.WaitCursor;
                        try
                        {
                            var req = new Utils.API.LiveV6.SearchForGeocachesRequest();
                            req.AccessToken = _client.Token;
                            req.CacheCode = new Utils.API.LiveV6.CacheCodeFilter() { CacheCodes = new string[] { textBoxGC.Text.ToUpper() } };
                            req.IsLite = _core.GeocachingComAccount.MemberTypeId == 1;
                            req.MaxPerPage = 1;
                            req.GeocacheLogCount = 0;
                            var resp = _client.Client.SearchForGeocaches(req);
                            if (resp.Status.StatusCode == 0)
                            {
                                if (resp.Geocaches != null && resp.Geocaches.Length > 0)
                                {
                                    _gc = Utils.API.Convert.Geocache(_core, resp.Geocaches[0]);
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(resp.Status.StatusMessage))
                                {
                                    MessageBox.Show(resp.Status.StatusMessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        catch
                        {
                            System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_FAILUNKNOWN), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        }
                        Cursor = Cursors.Default;
                    }
                }
            }
            updateGCInfo();
        }


        private void logThreadMethod()
        {
            try
            {
                _context.Send(new SendOrPostCallback(delegate(object state)
                {
                    toolStripStatusLabel1.Visible = true;
                    toolStripStatusLabel1.Text = _gc.Code;
                }), null);

                var req = new Utils.API.LiveV6.CreateFieldNoteAndPublishRequest();
                req.AccessToken = _client.Token;
                req.CacheCode = _gc.Code;
                req.EncryptLogText = false;
                req.FavoriteThisCache = _addToFavorites;
                req.Note = _logText;
                req.PromoteToLog = true;
                req.WptLogTypeId = _logType.ID;
                req.UTCDateLogged = _logDate.AddHours(12).ToUniversalTime();
                var resp = _client.Client.CreateFieldNoteAndPublish(req);
                if (resp.Status.StatusCode == 0)
                {
                    if (Utils.DataAccess.GetGeocache(_core.Geocaches, _gc.Code) != null)
                    {
                        //add log tot database
                        //if found, update cache as found
                        if (_logType.AsFound)
                        {
                            _gc.Found = true;
                        }
                        _core.Logs.Add(Utils.API.Convert.Log(_core, resp.Log));
                        _gc.ResetCachedLogData();
                    }

                    //log the trackables
                    if (_tbTrackingNumbers.Count > 0)
                    {
                        List<Utils.API.LiveV6.Trackable> tb = new List<Utils.API.LiveV6.Trackable>();
                        foreach (string t in _tbs14)
                        {
                            tb.Add((Utils.API.LiveV6.Trackable)_tbTrackingNumbers[t]);
                        }
                        logTrackables(tb, 14);

                        if (string.IsNullOrEmpty(_errormessage))
                        {
                            tb = new List<Utils.API.LiveV6.Trackable>();
                            foreach (string t in _tbs75)
                            {
                                tb.Add((Utils.API.LiveV6.Trackable)_tbTrackingNumbers[t]);
                            }
                            logTrackables(tb, 75);
                        }
                    }

                    //add the images to the logs
                    foreach (LogImage li in _logImages)
                    {
                        var uplReq = new Utils.API.LiveV6.UploadImageToGeocacheLogRequest();
                        uplReq.AccessToken = _client.Token;
                        uplReq.LogGuid = resp.Log.Guid;
                        uplReq.ImageData = new Utils.API.LiveV6.UploadImageData();
                        uplReq.ImageData.FileCaption = li.Caption;
                        uplReq.ImageData.FileDescription = li.Description;
                        uplReq.ImageData.FileName = li.FileName;
                        uplReq.ImageData.base64ImageData = System.Convert.ToBase64String(System.IO.File.ReadAllBytes(li.imgFile.Path));
                        var resp2 = _client.Client.UploadImageToGeocacheLog(uplReq);
                        if (resp2.Status.StatusCode != 0)
                        {
                            _errormessage = resp.Status.StatusMessage;
                            break;
                        }
                    }

                    //check favorite
                    if (_addToFavorites)
                    {
                        //not critical
                        try
                        {
                            Framework.Interfaces.IPlugin p = Utils.PluginSupport.PluginByName(_core, "GlobalcachingApplication.Plugins.APIFavorites.EditFavorites");
                            if (p != null)
                            {
                                MethodInfo mi = p.GetType().GetMethod("AddToFavorites");
                                if (mi != null)
                                {
                                    mi.Invoke(p, new object[] { _gc.Code });
                                }
                            }
                        }
                        catch
                        {
                        }

                    }

                }
                else
                {
                    _errormessage = resp.Status.StatusMessage;
                }
            }
            catch(Exception e)
            {
                _errormessage = e.Message;
            }
            _actionReady.Set();
        }

        private void buttonSubmit_Click(object sender, EventArgs e)
        {
            if (_gc != null && (comboBoxLogType1.SelectedItem as Framework.Data.LogType) != null)
            {
                panel1.Enabled = false;
                panel2.Enabled = false;
                panel3.Enabled = false;

                Cursor = Cursors.WaitCursor;
                _logDate = dateTimePicker1.Value.Date;
                _logType = (comboBoxLogType1.SelectedItem as Framework.Data.LogType);
                _tbs14 = (from string a in checkedListBox1.CheckedItems select a.Split(new char[] { ',' }, 2)[0]).ToList();
                _tbs75 = (from string a in checkedListBox2.CheckedItems select a.Split(new char[] { ',' }, 2)[0]).ToList();
                _logText = textBox1.Text;
                _addToFavorites = checkBox1.Checked;

                using (Utils.FrameworkDataUpdater upd = new Utils.FrameworkDataUpdater(_core))
                {
                    _actionReady = new ManualResetEvent(false);
                    Thread thrd = new Thread(new ThreadStart(this.logThreadMethod));
                    thrd.Start();
                    while (!_actionReady.WaitOne(100))
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                    thrd.Join();
                    _actionReady.Dispose();
                    _actionReady = null;
                }
                if (!string.IsNullOrEmpty(_errormessage))
                {
                    System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
                else if (!AskForNext || textBoxGC.ReadOnly || System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_OKANOTHER), Utils.LanguageSupport.Instance.GetTranslation(STR_SUCCESS), System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No)
                {
                    DialogResult = System.Windows.Forms.DialogResult.OK;
                    Close();
                }

                panel3.Enabled = true;
                panel2.Enabled = true;
                panel1.Enabled = true;
                toolStripStatusLabel1.Visible = false;
                toolStripStatusLabel1.Text = "";
            }
            Cursor = Cursors.Default;

        }

        private void logTrackables(List<Utils.API.LiveV6.Trackable> tbs, int logtypeid)
        {
            var req = new Utils.API.LiveV6.CreateTrackableLogRequest();
            req.AccessToken = _client.Token;
            req.LogType = logtypeid;
            req.UTCDateLogged = _logDate.AddHours(12).ToUniversalTime();
            foreach (Utils.API.LiveV6.Trackable tb in tbs)
            {
                _context.Send(new SendOrPostCallback(delegate(object state)
                {
                    toolStripStatusLabel1.Visible = true;
                    toolStripStatusLabel1.Text = tb.Code;
                }), null);

                req.TrackingNumber = tb.TrackingCode;
                req.CacheCode = _gc.Code;
                var resp = _client.Client.CreateTrackableLog(req);
                if (resp.Status.StatusCode != 0)
                {
                    _errormessage = resp.Status.StatusMessage;
                    break;
                }
                else
                {
                    System.Threading.Thread.Sleep(2000);
                }
            }
        }
    


        private void button1_Click(object sender, EventArgs e)
        {
            if (this.Width != 1032)
            {
                this.Width = 1032;

                this.Cursor = Cursors.WaitCursor;
                try
                {
                    getOwnedTrackables();
                    foreach (Utils.API.LiveV6.Trackable tb in _tbTrackingNumbers.Values)
                    {
                        checkedListBox1.Items.Add(string.Format("{0}, {1}", tb.Code, tb.Name));
                        checkedListBox2.Items.Add(string.Format("{0}, {1}", tb.Code, tb.Name));
                    }
                }
                catch
                {
                }
                this.Cursor = Cursors.Default;
            }
        }

        private void getOwnedTrackables()
        {
            _tbTrackingNumbers.Clear();
            try
            {
                var req = new Utils.API.LiveV6.GetTrackablesByUserRequest();
                req.AccessToken = _client.Token;
                req.MaxPerPage = 10;
                req.StartIndex = 0;
                req.TrackableLogsCount = 0;
                var resp = _client.Client.GetUsersTrackables(req);
                while (resp.Status.StatusCode == 0)
                {
                    if (resp.Trackables != null)
                    {
                        foreach (Utils.API.LiveV6.Trackable tb in resp.Trackables)
                        {
                            _tbTrackingNumbers.Add(tb.Code, tb);
                        }
                        if (resp.Trackables.Count() < req.MaxPerPage)
                        {
                            break;
                        }
                        else
                        {
                            req.StartIndex += req.MaxPerPage;
                            resp = _client.Client.GetUsersTrackables(req);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch
            {
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, true);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox2.Items.Count; i++)
            {
                checkedListBox2.SetItemChecked(i, true);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, false);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox2.Items.Count; i++)
            {
                checkedListBox2.SetItemChecked(i, false);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            using (ImageEditorForm dlg = new ImageEditorForm())
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LogImage li = new LogImage();
                    li.FileName = dlg.Filename;
                    li.imgFile = dlg.ImageFilePath;
                    li.Caption = dlg.Caption;
                    li.Description = dlg.Description;
                    _logImages.Add(li);

                    PictureBox pb = new PictureBox();
                    li.PB = pb;
                    pb.Tag = li;

                    pb.Size = new Size(50, 50);
                    pb.SizeMode = PictureBoxSizeMode.CenterImage;
                    pb.Image = Utils.ImageUtilities.ResizeImage(Image.FromFile(li.imgFile.Path), pb.Width, pb.Height);
                    toolTip1.SetToolTip(pb, li.Caption);
                    pb.ContextMenuStrip = this.contextMenuStrip1;
                    pb.MouseDown += new MouseEventHandler(pb_MouseDown);

                    flowLayoutPanel1.Controls.Add(pb);
                }
                else
                {
                    dlg.Clear();
                }
            }
        }

        void pb_MouseDown(object sender, MouseEventArgs e)
        {
            _activepb = sender as PictureBox;
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_activepb != null)
            {
                flowLayoutPanel1.Controls.Remove(_activepb);
                LogImage li = _activepb.Tag as LogImage;
                _logImages.Remove(li);
                li.imgFile.Dispose();
                _activepb.Dispose();
                _activepb = null;
            }
        }

    }
}
