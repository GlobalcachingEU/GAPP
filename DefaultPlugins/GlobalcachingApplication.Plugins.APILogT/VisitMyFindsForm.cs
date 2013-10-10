using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace GlobalcachingApplication.Plugins.APILogT
{
    public partial class VisitMyFindsForm : Form
    {
        public const string STR_ERROR = "Error";
        public const string STR_TITLE = "Log visit My Finds for a trackable";
        public const string STR_GETTINGLOGS = "Getting trackable logs...";
        public const string STR_LOGGING = "Logging trackable...";
        public const string STR_LOGGINGOK = "successfully logged";
        public const string STR_LOGDATEUNKNOWN = "Not all log dates are known.";
        public const string STR_EXPLANATION = "Explanation";
        public const string STR_EXPLANATIONTXT = "This feature will allow you to take a trackable and add a Visit log to each geocache you have found.\r\nThe logdate will be the same as the geocache found log date.\r\nThis way it is possible to track your finds by using the trackable map on geocaching.com.\r\n\r\nFirst get the trackable you own, select a trackable which you want to use.";
        public const string STR_SELECTTRACKABLE = "Select trackable";
        public const string STR_TRACKINGNUMBER = "Tracking number";
        public const string STR_CHECK = "Check";
        public const string STR_OR = "or";
        public const string STR_GETOWNED = "Get owned";
        public const string STR_ICON = "Icon";
        public const string STR_CODE = "Code";
        public const string STR_NAME = "Name";
        public const string STR_AVAILABLELOGS = "Available My Finds logs for selected trackable";
        public const string STR_MISSING = "Missing";
        public const string STR_PRESENT = "Present";
        public const string STR_GETALLLOGS = "Get all trackable logs";
        public const string STR_LOGTEXT = "Log text";
        public const string STR_BETWEEN = "Between";
        public const string STR_DOLOG = "Add visits logs for missing";
        public const string STR_MESSAGES = "Messages";

#if DEBUG
        //private const bool _useTestSite = true;
        private const bool _useTestSite = false;
#else
        private const bool _useTestSite = false;
#endif
        private Utils.BasePlugin.Plugin _plugin;
        private Framework.Interfaces.ICore _core;
        private SynchronizationContext _context = null;
        private Utils.API.LiveV6.Trackable _activeTb = null;
        private List<Utils.API.LiveV6.TrackableLog> _activeTbLogs = null;
        private ManualResetEvent _actionReady;
        private string _errormessage;
        private string _logText = "";
        private List<string> _logGeocaches = null;

        public class MyGeocacheFind
        {
            public Framework.Data.Geocache gc;
            public Framework.Data.Log lg;
            public DateTime logDate;
            public DateTime logDateForSort;
        }
        private List<MyGeocacheFind> _myFinds = null;

        public VisitMyFindsForm()
        {
            InitializeComponent();
        }
        public VisitMyFindsForm(Utils.BasePlugin.Plugin plugin, Framework.Interfaces.ICore core): this()
        {
            _plugin = plugin;
            _core = core;

            _context = SynchronizationContext.Current;
            if (_context == null)
            {
                _context = new SynchronizationContext();
            }
            dateTimePicker1.Value = new DateTime(2000, 1, 1);

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXPLANATION);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXPLANATIONTXT);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTTRACKABLE);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TRACKINGNUMBER);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CHECK);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OR);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GETOWNED);
            this.listView1.Columns[0].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ICON);
            this.listView1.Columns[1].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CODE);
            this.listView1.Columns[2].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.groupBox3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_AVAILABLELOGS);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MISSING);
            this.label7.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PRESENT);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GETALLLOGS);
            this.label8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOGTEXT);
            this.label9.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_BETWEEN);
            this.button4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DOLOG);
            this.groupBox3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MESSAGES);
        }

        private void getOwnedTrackables()
        {
            try
            {
                using (Utils.API.GeocachingLiveV6 api = new Utils.API.GeocachingLiveV6(_core, _useTestSite))
                {
                    var req = new Utils.API.LiveV6.GetTrackablesByUserRequest();
                    req.AccessToken = api.Token;
                    req.MaxPerPage = 10;
                    req.StartIndex = 0;
                    req.TrackableLogsCount = 0;
                    var resp = api.Client.GetUsersTrackables(req);
                    while (resp.Status.StatusCode == 0 && resp.Trackables != null)
                    {
                        _context.Send(new SendOrPostCallback(delegate(object state)
                        {
                            foreach (Utils.API.LiveV6.Trackable tb in resp.Trackables)
                            {
                                if (!tb.Archived && tb.CurrentOwner.UserName == _core.GeocachingComAccount.AccountName)
                                {
                                    if (!imageList1.Images.ContainsKey(tb.IconUrl))
                                    {
                                        Image img = LoadImage(tb.IconUrl);
                                        if (img != null)
                                        {
                                            imageList1.Images.Add(tb.IconUrl, img);
                                        }
                                    }
                                    ListViewItem lv = new ListViewItem(new string[] { "", tb.Code, tb.Name ?? "" }, tb.IconUrl);
                                    lv.Tag = tb;
                                    listView1.Items.Add(lv);
                                }
                            }
                        }), null);
                        if (resp.Trackables.Count() < req.MaxPerPage)
                        {
                            break;
                        }
                        else
                        {
                            req.StartIndex += req.MaxPerPage;
                            Thread.Sleep(2000);
                            resp = api.Client.GetUsersTrackables(req);
                        }
                    }
                    if (resp.Status.StatusCode!=0)
                    {
                        _errormessage = resp.Status.StatusMessage;
                    }
                }
            }
            catch
            {
            }
        }

        private Image LoadImage(string url)
        {
            Image img = null;
            try
            {
                System.Net.WebRequest request = System.Net.WebRequest.Create(url);
                using (System.Net.WebResponse response = request.GetResponse())
                using (System.IO.Stream responseStream = response.GetResponseStream())
                {
                    img = Image.FromStream(responseStream);
                }
            }
            catch
            {
            }
            return img;
        }
        private void label5_Click(object sender, EventArgs e)
        {
            groupBox1.Visible = !groupBox1.Visible;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                _errormessage = "";
                listView1.Items.Clear();
                imageList1.Images.Clear();
                getOwnedTrackables();
                if (!string.IsNullOrEmpty(_errormessage))
                {
                    System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
            catch
            {
            }
            this.Cursor = Cursors.Default;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = textBox1.Text.Length > 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                using (Utils.API.GeocachingLiveV6 api = new Utils.API.GeocachingLiveV6(_core, _useTestSite))
                {
                    Utils.API.LiveV6.GetTrackableResponse resp;
                    if (textBox1.Text.ToUpper().StartsWith("TB"))
                    {
                        resp = api.Client.GetTrackablesByTBCode(api.Token, textBox1.Text.ToUpper(), 0);
                    }
                    else
                    {
                        resp = api.Client.GetTrackablesByTrackingNumber(api.Token, textBox1.Text.ToUpper(), 0);
                    }
                    if (resp.Status.StatusCode == 0)
                    {
                        if (resp.Trackables.Count() > 0)
                        {
                            Utils.API.LiveV6.Trackable tb = resp.Trackables[0];
                            if (!tb.Archived && tb.CurrentOwner.UserName == _core.GeocachingComAccount.AccountName)
                            {
                                if (!imageList1.Images.ContainsKey(tb.IconUrl))
                                {
                                    Image img = LoadImage(tb.IconUrl);
                                    if (img != null)
                                    {
                                        imageList1.Images.Add(tb.IconUrl, img);
                                    }
                                }
                                ListViewItem lv = new ListViewItem(new string[] { "", tb.Code, tb.Name ?? "" }, tb.IconUrl);
                                lv.Tag = tb;
                                listView1.Items.Add(lv);
                            }
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(resp.Status.StatusMessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                }
            }
            catch
            {
            }
            this.Cursor = Cursors.Default;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _activeTbLogs = null;
            _activeTb = null;
            updateTrackableInfo();
            if (listView1.SelectedItems.Count > 0)
            {
                _activeTb = listView1.SelectedItems[0].Tag as Utils.API.LiveV6.Trackable;
            }
            button3.Enabled = _activeTb!=null;
        }

        private void checkLoggingPossible()
        {
            button4.Enabled = (_activeTb != null && _activeTbLogs != null && listBox1.Items.Count > 0 && textBox2.Text.Length>0);
        }

        private bool GetMyFinds()
        {
            List<Framework.Data.Geocache> gcList = Utils.DataAccess.GetFoundGeocaches(_core.Geocaches, _core.Logs, _core.GeocachingComAccount);
            _myFinds = new List<MyGeocacheFind>();
            string tl = _core.GeocachingComAccount.AccountName.ToLower();
            foreach (Framework.Data.Geocache gc in gcList)
            {
                if (gc.Code.StartsWith("GC"))
                {
                    MyGeocacheFind mf = new MyGeocacheFind();
                    mf.gc = gc;
                    mf.lg = (from Framework.Data.Log l in _core.Logs where l.GeocacheCode == gc.Code && l.Finder.ToLower() == tl && l.LogType.AsFound orderby l.Date descending select l).FirstOrDefault();
                    if (mf.lg != null)
                    {
                        mf.logDate = mf.lg.Date;
                        mf.logDateForSort = mf.lg.Date;
                        int logid = 0;
                        if (mf.lg.ID.StartsWith("GL"))
                        {
                            logid = Utils.Conversion.GetCacheIDFromCacheCode(mf.lg.ID);
                        }
                        else
                        {
                            int.TryParse(mf.lg.ID, out logid);
                        }
                        mf.logDateForSort.AddMilliseconds(logid);
                    }
                    else
                    {
                        _myFinds = null;
                        return false;
                    }
                    _myFinds.Add(mf);
                }
            }
            _myFinds.Sort(delegate(MyGeocacheFind a, MyGeocacheFind b)
            {
                int cv = a.logDate.CompareTo(b.logDate);
                if (cv == 0) { return a.logDateForSort.CompareTo(b.logDateForSort); }
                else return cv;
            });
            return true;
        }

        private void updateTrackableInfo()
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            if (_activeTb != null && _activeTbLogs != null)
            {
                dateTimePicker1.Enabled = true;
                dateTimePicker2.Enabled = true;

                if (_myFinds == null)
                {
                    GetMyFinds();
                }
                if (_myFinds != null)
                {
                    List<Framework.Data.Geocache> _foundList = (from g in _myFinds where g.logDate >= dateTimePicker1.Value && g.logDate <= dateTimePicker2.Value select g.gc).Distinct().ToList();

                    listBox2.BeginUpdate();
                    foreach (var l in _activeTbLogs)
                    {
                        if (!l.IsArchived && l.CacheID != null)
                        {
                            if (l.LogType.WptLogTypeId == 75 || l.LogType.WptLogTypeId == 14 || l.LogType.WptLogTypeId == 13)
                            {
                                string gcCode = Utils.Conversion.GetCacheCodeFromCacheID((int)l.CacheID);
                                var gc = (from g in _foundList where gcCode == g.Code select g).FirstOrDefault();
                                if (gc != null)
                                {
                                    listBox2.Items.Add(gcCode);
                                    _foundList.Remove(gc);
                                }
                            }
                        }
                    }
                    listBox2.EndUpdate();
                    listBox1.Items.AddRange((from g in _foundList select g.Code).ToArray());
                }
                else
                {
                    MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_LOGDATEUNKNOWN), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                }
            }
            else
            {
                dateTimePicker1.Enabled = false;
                dateTimePicker2.Enabled = false;
            }
            checkLoggingPossible();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            groupBox2.Enabled = false;
            groupBox3.Enabled = false;
            _activeTbLogs = new List<Utils.API.LiveV6.TrackableLog>();
            _errormessage = null;
            _actionReady = new ManualResetEvent(false);
            Thread thrd = new Thread(new ThreadStart(this.getAllTrackableLogsThreadMethod));
            thrd.Start();
            while (!_actionReady.WaitOne(100))
            {
                System.Windows.Forms.Application.DoEvents();
            }
            thrd.Join();
            _actionReady.Dispose();
            _actionReady = null;
            if (!string.IsNullOrEmpty(_errormessage))
            {
                System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                _activeTbLogs = null;
            }
            updateTrackableInfo();
            groupBox2.Enabled = true;
            groupBox3.Enabled = true;
        }

        private void getAllTrackableLogsThreadMethod()
        {
            try
            {
                using (Utils.ProgressBlock progr = new Utils.ProgressBlock(_plugin as Utils.BasePlugin.Plugin, STR_GETTINGLOGS, STR_GETTINGLOGS, 30, 0, true))
                {
                    using (var api = new Utils.API.GeocachingLiveV6(_core, _useTestSite))
                    {
                        int maxPageSize = 30;
                        while (true)
                        {
                            var resp = api.Client.GetTrackableLogsByTBCode(api.Token, _activeTb.Code, _activeTbLogs.Count, maxPageSize);
                            if (resp.Status.StatusCode == 0)
                            {
                                if (resp.TrackableLogs != null)
                                {
                                    _activeTbLogs.AddRange(resp.TrackableLogs);
                                    if (resp.TrackableLogs.Count() < maxPageSize)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        if (!progr.UpdateProgress(STR_GETTINGLOGS, STR_GETTINGLOGS, _activeTbLogs.Count + maxPageSize, _activeTbLogs.Count))
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            Thread.Sleep(2000);
                                        }
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                _errormessage = resp.Status.StatusMessage;
                                break;
                            }
                        }

                    }
                }
            }
            catch
            {
            }
            _actionReady.Set();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            updateTrackableInfo();
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            updateTrackableInfo();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            groupBox2.Enabled = false;
            groupBox3.Enabled = false;
            _activeTbLogs = new List<Utils.API.LiveV6.TrackableLog>();
            _errormessage = null;
            _logGeocaches = (from string s in listBox1.Items select s).ToList();
            _logText = textBox2.Text;
            _actionReady = new ManualResetEvent(false);
            Thread thrd = new Thread(new ThreadStart(this.logMissingGeocachesThreadMethod));
            thrd.Start();
            while (!_actionReady.WaitOne(100))
            {
                System.Windows.Forms.Application.DoEvents();
            }
            thrd.Join();
            _actionReady.Dispose();
            _actionReady = null;
            if (!string.IsNullOrEmpty(_errormessage))
            {
                System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            _activeTbLogs = null;
            updateTrackableInfo();
            groupBox2.Enabled = true;
            groupBox3.Enabled = true;
        }

        private void logMissingGeocachesThreadMethod()
        {
            try
            {
                using (Utils.ProgressBlock progr = new Utils.ProgressBlock(_plugin as Utils.BasePlugin.Plugin, STR_LOGGING, STR_LOGGING, _logGeocaches.Count, 0, true))
                {
                    using (Utils.API.GeocachingLiveV6 api = new Utils.API.GeocachingLiveV6(_core, _useTestSite))
                    {
                        var req = new Utils.API.LiveV6.CreateTrackableLogRequest();
                        req.AccessToken = api.Token;
                        req.Note = _logText;
                        req.LogType = 75;
                        int index = 0;
                        foreach (string gcCode in _logGeocaches)
                        {
                            Framework.Data.Geocache gc = (from Framework.Data.Geocache g in _core.Geocaches where g.Code == gcCode select g).FirstOrDefault();
                            req.UTCDateLogged = ((DateTime)gc.FoundDate).Date.AddHours(12).ToUniversalTime();
                            if (!string.IsNullOrEmpty(_activeTb.TrackingCode))
                            {
                                req.TrackingNumber = _activeTb.TrackingCode;
                            }
                            req.TravelBugCode = _activeTb.Code;
                            req.CacheCode = gcCode;
                            var resp = api.Client.CreateTrackableLog(req);
                            if (resp.Status.StatusCode == 0)
                            {
                                _context.Send(new SendOrPostCallback(delegate(object state)
                                {
                                    textBox3.Text += string.Format("\r\n{0} {1}", gcCode, Utils.LanguageSupport.Instance.GetTranslation(STR_LOGGINGOK));
                                    textBox3.SelectionStart = textBox3.Text.Length;
                                    textBox3.ScrollToCaret();
                                    listBox1.Items.Remove(gcCode);
                                    listBox2.Items.Add(gcCode);
                                }), null);
                            }
                            else
                            {
                                //_errormessage = resp.Status.StatusMessage;
                                _context.Send(new SendOrPostCallback(delegate(object state)
                                {
                                    textBox3.Text += string.Format("\r\n{0}", resp.Status.StatusMessage);
                                    textBox3.SelectionStart = textBox3.Text.Length;
                                    textBox3.ScrollToCaret();
                                }), null);
                            }
                            index++;
                            if (progr.UpdateProgress(STR_LOGGING, STR_LOGGING, _logGeocaches.Count, index))
                            {
                                Thread.Sleep(2000);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _errormessage = e.Message;
            }
            _actionReady.Set();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            checkLoggingPossible();
        }
    }
}
