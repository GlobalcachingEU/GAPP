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
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.APILogT
{
    public partial class BatchLogForm : Form
    {
        public const string STR_ERROR = "Error";
        public const string STR_TITLE = "Batch logging for trackables";
        public const string STR_TBLIST = "Trackables to log (seperated by space, comma or tab)";
        public const string STR_ADD = "Add";
        public const string STR_TRACKABLES = "Trackables";
        public const string STR_LOGTYPE = "Type";
        public const string STR_DATE = "Date";
        public const string STR_NOTE = "Text";
        public const string STR_STOPATLOG = "Stop after each log";
        public const string STR_LOG = "Log";
        public const string STR_CANCEL = "Cancel";
        public const string STR_GEOCACHE = "Geocache";
        public const string STR_CHECK = "Check";
        public const string STR_INPOSSESSION = "In possession";
        
        private Utils.BasePlugin.Plugin _plugin;
        private Framework.Interfaces.ICore _core;
        private SynchronizationContext _context = null;
        private Hashtable _tbTrackingNumbers = new Hashtable();

        //settings for thread:
        private bool _stopAfterEachLog;
        private DateTime _visitDate;
        private string _logText;
        private string _gcCode;
        private List<string> _tbs;
        private volatile bool _cancelled;
        private Framework.Data.LogType _logType;
        private string _errormessage;

        public BatchLogForm()
        {
            InitializeComponent();
        }

        public BatchLogForm(Utils.BasePlugin.Plugin plugin, Framework.Interfaces.ICore core)
        {
            InitializeComponent();

            _plugin = plugin;
            _core = core;

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TBLIST);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADD);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TRACKABLES);
            this.label5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOGTYPE);
            this.label3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DATE);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NOTE);
            this.checkBoxStopAfterEachLog.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_STOPATLOG);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOG);
            this.buttonCancel.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CANCEL);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GEOCACHE);
            this.buttonCheck.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CHECK);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_INPOSSESSION);

            this.linkLabel1.Text = "";

            int[] ids = new int[] { 4, 13, 14, 19, 48, 75 };
            comboBoxLogType1.SetLogTypes(core, (from Framework.Data.LogType l in core.LogTypes join a in ids on l.ID equals a select l).ToList());

            _context = SynchronizationContext.Current;
            if (_context == null)
            {
                _context = new SynchronizationContext();
            }

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = textBox1.Text.Length > 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] tbs = textBox1.Text.ToUpper().Split(new char[] { ' ',',','\t','"','\'',':',';','\r','\n','-'}, StringSplitOptions.RemoveEmptyEntries);
            if (tbs != null)
            {
                foreach (string tb in tbs)
                {
                    if (!checkedListBox1.Items.Contains(tb))
                    {
                        checkedListBox1.Items.Add(tb, true);
                    }
                }
            }
            checkReadyForSubmit();
        }

        private void checkReadyForSubmit()
        {
            bool result = false;
            if (checkedListBox1.Items.Count > 0 && comboBoxLogType1.SelectedItem as Framework.Data.LogType != null)
            {
                int id = (comboBoxLogType1.SelectedItem as Framework.Data.LogType).ID;
                if (id == 13 || id == 14)
                {
                    result = textBox3.Text.ToUpper().StartsWith("GC");
                }
                else
                {
                    result = true;
                }
            }

            button2.Enabled = result;
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            this.ControlBox = false;
            panel1.Enabled = false;
            buttonCancel.Visible = true;
            _cancelled = false;
            _logType = comboBoxLogType1.SelectedItem as Framework.Data.LogType;
            _visitDate = dateTimePicker1.Value.Date;
            _logText = textBox2.Text;
            _gcCode = textBox3.Text.ToUpper().Trim();
            _stopAfterEachLog = checkBoxStopAfterEachLog.Checked;
            _errormessage = null;
            _tbs = new List<string>();
            _tbs.AddRange((from int a in checkedListBox1.CheckedIndices orderby a select (string)checkedListBox1.Items[a]).ToArray());

            if (_tbs.Count > 0)
            {
                toolStripProgressBar1.Value = 0;
                toolStripProgressBar1.Maximum = _tbs.Count;
                await Task.Run(() =>
                    {
                        this.logThreadMethod();
                    });
                if (!string.IsNullOrEmpty(_errormessage))
                {
                    System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }

            buttonCancel.Visible = false;
            this.ControlBox = true;
            panel1.Enabled = true;
        }

        private void getOwnedTrackables()
        {
            _tbTrackingNumbers.Clear();
            try
            {
                using (Utils.API.GeocachingLiveV6 api = new Utils.API.GeocachingLiveV6(_core))
                {
                    var req = new Utils.API.LiveV6.GetTrackablesByUserRequest();
                    req.AccessToken = api.Token;
                    req.MaxPerPage = 10;
                    req.StartIndex = 0;
                    req.TrackableLogsCount = 0;
                    var resp = api.Client.GetUsersTrackables(req);
                    while (resp.Status.StatusCode == 0)
                    {
                        if (resp.Trackables != null)
                        {
                            foreach (Utils.API.LiveV6.Trackable tb in resp.Trackables)
                            {
                                _tbTrackingNumbers.Add(tb.Code, tb.TrackingCode);
                            }
                            if (resp.Trackables.Count() < req.MaxPerPage)
                            {
                                break;
                            }
                            else
                            {
                                req.StartIndex += req.MaxPerPage;
                                resp = api.Client.GetUsersTrackables(req);
                            }
                        }
                        else
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

        private void logThreadMethod()
        {
            try
            {
                using (Utils.API.GeocachingLiveV6 api = new Utils.API.GeocachingLiveV6(_core))
                {
                    var req = new Utils.API.LiveV6.CreateTrackableLogRequest();
                    req.AccessToken = api.Token;
                    req.Note = _logText;
                    req.LogType = _logType.ID;
                    req.UTCDateLogged = _visitDate.AddHours(12).ToUniversalTime();
                    bool getOwnedDone = false;
                    foreach (string tb in _tbs)
                    {
                        if (tb.StartsWith("TB"))
                        {
                            if (_tbTrackingNumbers[tb] as string == null)
                            {
                                if (!getOwnedDone)
                                {
                                    getOwnedTrackables();
                                }
                            }
                            req.TrackingNumber = _tbTrackingNumbers[tb] as string;
                        }
                        else
                        {
                            req.TrackingNumber = tb;
                        }
                        req.CacheCode = _gcCode;
                        var resp = api.Client.CreateTrackableLog(req);
                        if (resp.Status.StatusCode == 0)
                        {
                            _context.Send(new SendOrPostCallback(delegate(object state)
                            {
                                checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf(tb), false);
                                toolStripProgressBar1.Value++;
                            }), null);
                        }
                        else
                        {
                            _errormessage = resp.Status.StatusMessage;
                        }
                        if (_stopAfterEachLog || _cancelled)
                        {
                            break;
                        }
                        else
                        {
                            Thread.Sleep(2000);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _errormessage = e.Message;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _cancelled = true;
        }

        private void comboBoxLogType1_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkReadyForSubmit();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            checkReadyForSubmit();
            buttonCheck.Enabled = textBox3.Text.ToUpper().StartsWith("GC");
        }

        private void buttonCheck_Click(object sender, EventArgs e)
        {
            this.linkLabel1.Text = "";
            this.Cursor = Cursors.WaitCursor;
            try
            {
                using (Utils.API.GeocachingLiveV6 api = new Utils.API.GeocachingLiveV6(_core))
                {
                    var req = new Utils.API.LiveV6.SearchForGeocachesRequest();
                    req.AccessToken = api.Token;
                    req.CacheCode = new Utils.API.LiveV6.CacheCodeFilter();
                    req.CacheCode.CacheCodes = new string[] { textBox3.Text.ToUpper().Trim() };
                    req.IsLite = true;
                    req.MaxPerPage = 1;
                    req.GeocacheLogCount = 0;
                    var resp = api.Client.SearchForGeocaches(req);
                    if (resp.Status.StatusCode == 0)
                    {
                        if (resp.Geocaches != null && resp.Geocaches.Count() > 0)
                        {
                            linkLabel1.Text = string.Format("{0}, {1}", resp.Geocaches[0].Code, resp.Geocaches[0].Name);
                            linkLabel1.Links.Add(0, resp.Geocaches[0].Code.Length, resp.Geocaches[0].Url);
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

        private void button3_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                getOwnedTrackables();
                foreach (string tb in _tbTrackingNumbers.Keys)
                {
                    textBox1.Text += string.Format(",{0}", tb);
                }
            }
            catch
            {
            }
            this.Cursor = Cursors.Default;
        }
        
    }
}
