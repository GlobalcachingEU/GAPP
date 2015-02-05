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

namespace GlobalcachingApplication.Plugins.APILOGC
{
    public partial class GeocacheBatchLogForm : Form
    {
        public const string STR_ERROR = "Error";
        public const string STR_FAILUNKNOWN = "Unable to log the geocache. Reason unknown";
        public const string STR_LOGGING = "Logging...";
        public const string STR_TITLE = "Geocache batch logging";
        public const string STR_GEOCACHECODE = "Geocache code";
        public const string STR_ADD = "Add";
        public const string STR_CODE = "Code";
        public const string STR_NAME = "Name";
        public const string STR_STOPATLOG = "Stop after each log";
        public const string STR_STARTFOUNDCNT = "Start #foundcount";
        public const string STR_LOGTYPE = "Log type";
        public const string STR_DATE = "Date";
        public const string STR_LOGTEXT = "Log text";
        public const string STR_SUBMIT = "Submit";
        public const string STR_TRACKABLES = "Trackables";
        public const string STR_SELECTALL = "Select all";
        public const string STR_CLEARTALL = "Clear all";

        private Framework.Interfaces.ICore _core = null;
        private Utils.API.GeocachingLiveV6 _client = null;
        private Utils.BasePlugin.Plugin _plugin = null;
        private Hashtable _tbTrackingNumbers = new Hashtable();

        private SynchronizationContext _context = null;
        private string _errormessage;
        private Framework.Data.LogType _logType;
        private List<string> _tbs75;
        private DateTime _logDate;
        private string _logText;
        private List<Framework.Data.Geocache> _gcList;
        private int _foundcount;


        public GeocacheBatchLogForm()
        {
            InitializeComponent();
        }

        private void checkSubmitButton()
        {
            buttonSubmit.Enabled = (listView1.CheckedIndices.Count > 0 && textBox2.Text.Length > 0);
        }

        public GeocacheBatchLogForm(Utils.BasePlugin.Plugin plugin, Framework.Interfaces.ICore core, Utils.API.GeocachingLiveV6 client):
            this(plugin, core, client, true)
        {
        }

        public GeocacheBatchLogForm(Utils.BasePlugin.Plugin plugin, Framework.Interfaces.ICore core, Utils.API.GeocachingLiveV6 client, bool addSelected)
        {
            InitializeComponent();

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GEOCACHECODE);
            this.buttonAdd.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADD);
            this.listView1.Columns[0].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CODE);
            this.listView1.Columns[1].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_STOPATLOG);
            this.label3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_STARTFOUNDCNT);
            this.label5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOGTYPE);
            this.label8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DATE);
            this.label9.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOGTEXT);
            this.buttonSubmit.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SUBMIT);
            this.button6.Text = string.Format("{0} >>", Utils.LanguageSupport.Instance.GetTranslation(STR_TRACKABLES));
            this.button9.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTALL);
            this.button8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CLEARTALL);

            _context = SynchronizationContext.Current;
            if (_context == null)
            {
                _context = new SynchronizationContext();
            }

            _core = core;
            _client = client;
            _plugin = plugin;
            int[] ids = new int[] { 2 };
            comboBoxLogType1.SetLogTypes(core, (from Framework.Data.LogType l in core.LogTypes join a in ids on l.ID equals a select l).ToList());
            comboBoxLogType1.SelectedIndex = 0;

            ids = new int[] { 75 };
            comboBoxLogType2.SetLogTypes(core, (from Framework.Data.LogType l in core.LogTypes join a in ids on l.ID equals a select l).ToList());
            comboBoxLogType2.SelectedIndex = 0;

            if (addSelected)
            {
                listView1.BeginUpdate();
                List<Framework.Data.Geocache> gcList = Utils.DataAccess.GetSelectedGeocaches(core.Geocaches);
                foreach (Framework.Data.Geocache gc in gcList)
                {
                    addGeocacheToList(gc);
                }
                listView1.EndUpdate();
            }
            numericUpDown1.Value = Utils.DataAccess.GetFoundGeocaches(core.Geocaches, core.Logs, core.GeocachingComAccount).Count + 1;
            checkSubmitButton();
        }

        public void AddGeocache(Framework.Data.Geocache gc, DateTime logDate)
        {
            addGeocacheToList(gc);
            dateTimePicker1.Value = logDate;
        }

        private void addGeocacheToList(Framework.Data.Geocache gc)
        {
            if ((from ListViewItem l in listView1.Items where (l.Tag as Framework.Data.Geocache).Code == gc.Code select l).FirstOrDefault() == null)
            {
                ListViewItem lvi = new ListViewItem(new string[] { gc.Code, gc.Name ?? "" });
                lvi.Tag = gc;
                lvi.Checked = true;
                listView1.Items.Add(lvi);
                checkSubmitButton();
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonRemove.Enabled = (listView1.SelectedIndices.Count > 0);
            buttonUp.Enabled = (listView1.SelectedIndices.Count > 0 && listView1.SelectedIndices[0]>0);
            buttonDown.Enabled = (listView1.SelectedIndices.Count > 0 && listView1.SelectedIndices[0] < listView1.Items.Count-1);
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                listView1.Items.RemoveAt(listView1.SelectedIndices[0]);
                checkSubmitButton();
            }
        }

        private void buttonUp_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0 && listView1.SelectedIndices[0] > 0)
            {
                int index = listView1.SelectedIndices[0];
                ListViewItem lvi = listView1.Items[index];
                listView1.Items.RemoveAt(index);
                listView1.Items.Insert(index - 1, lvi);
            }
        }

        private void buttonDown_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0 && listView1.SelectedIndices[0] < listView1.Items.Count - 1)
            {
                int index = listView1.SelectedIndices[0];
                ListViewItem lvi = listView1.Items[index];
                listView1.Items.RemoveAt(index);
                listView1.Items.Insert(index +1, lvi);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            buttonAdd.Enabled = (textBox1.Text.Length > 2 && textBox1.Text.Trim().ToUpper().StartsWith("GC"));
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            Framework.Data.Geocache gc = null;
            if (textBox1.Text.Length > 0)
            {
                gc = Utils.DataAccess.GetGeocache(_core.Geocaches, textBox1.Text.ToUpper());
                if (gc == null)
                {
                    //not in our system, try geocaching.com
                    if (textBox1.Text.ToUpper().StartsWith("GC"))
                    {
                        Cursor = Cursors.WaitCursor;
                        try
                        {
                            var req = new Utils.API.LiveV6.SearchForGeocachesRequest();
                            req.AccessToken = _client.Token;
                            req.CacheCode = new Utils.API.LiveV6.CacheCodeFilter() { CacheCodes = new string[] { textBox1.Text.ToUpper() } };
                            req.IsLite = _core.GeocachingComAccount.MemberTypeId == 1;
                            req.MaxPerPage = 1;
                            req.GeocacheLogCount = 0;
                            var resp = _client.Client.SearchForGeocaches(req);
                            if (resp.Status.StatusCode == 0)
                            {
                                if (resp.Geocaches != null && resp.Geocaches.Length > 0)
                                {
                                    gc = Utils.API.Convert.Geocache(_core, resp.Geocaches[0]);
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
                if (gc != null)
                {
                    addGeocacheToList(gc);
                    checkSubmitButton();
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            checkSubmitButton();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (this.Width != 560)
            {
                this.Width = 742;

                this.Cursor = Cursors.WaitCursor;
                try
                {
                    getOwnedTrackables();
                    foreach (Utils.API.LiveV6.Trackable tb in _tbTrackingNumbers.Values)
                    {
                        checkedListBox1.Items.Add(string.Format("{0}, {1}", tb.Code, tb.Name));
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

        private void button9_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, true);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, false);
            }
        }

        private async void buttonSubmit_Click(object sender, EventArgs e)
        {
            panel1.Enabled = false;
            panel2.Enabled = false;

            this.ControlBox = false;
            _logDate = dateTimePicker1.Value.Date;
            _logType = (comboBoxLogType1.SelectedItem as Framework.Data.LogType);
            _tbs75 = (from string a in checkedListBox1.CheckedItems select a.Split(new char[] { ',' }, 2)[0]).ToList();
            _logText = textBox2.Text;
            _foundcount = (int)numericUpDown1.Value;
            if (checkBox1.Checked)
            {
                _gcList = ((from int a in listView1.CheckedIndices orderby a select (Framework.Data.Geocache)listView1.Items[a].Tag).Take(1).ToList());
            }
            else
            {
                _gcList = ((from int a in listView1.CheckedIndices orderby a select (Framework.Data.Geocache)listView1.Items[a].Tag).ToList());
            }

            using (Utils.FrameworkDataUpdater upd = new Utils.FrameworkDataUpdater(_core))
            {
                await Task.Run(() =>
                {
                    this.logThreadMethod();
                });
            }
            if (!string.IsNullOrEmpty(_errormessage))
            {
                System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            numericUpDown1.Value = _foundcount;
            panel2.Enabled = true;
            panel1.Enabled = true;
            toolStripStatusLabel1.Visible = false;
            toolStripStatusLabel1.Text = "";
            this.ControlBox = true;
        }

        private void logThreadMethod()
        {
            try
            {
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock(_plugin, STR_LOGGING, STR_LOGGING, _gcList.Count, 0, true))
                {
                    int index = 0;

                    foreach (var gc in _gcList)
                    {
                        _context.Send(new SendOrPostCallback(delegate(object state)
                        {
                            toolStripStatusLabel1.Visible = true;
                            toolStripStatusLabel1.Text = gc.Code;
                        }), null);

                        var req = new Utils.API.LiveV6.CreateFieldNoteAndPublishRequest();
                        req.AccessToken = _client.Token;
                        req.CacheCode = gc.Code;
                        req.EncryptLogText = false;
                        req.FavoriteThisCache = false;
                        req.Note = _logText.Replace("#foundcount",_foundcount.ToString());
                        req.PromoteToLog = true;
                        req.WptLogTypeId = _logType.ID;
                        req.UTCDateLogged = _logDate.AddHours(12).ToUniversalTime();
                        var resp = _client.Client.CreateFieldNoteAndPublish(req);
                        if (resp.Status.StatusCode == 0)
                        {
                            if (Utils.DataAccess.GetGeocache(_core.Geocaches, gc.Code) != null)
                            {
                                //add log tot database
                                //if found, update cache as found
                                if (_logType.AsFound)
                                {
                                    gc.Found = true;
                                }
                                _core.Logs.Add(Utils.API.Convert.Log(_core, resp.Log));
                                gc.ResetCachedLogData();
                            }

                            _foundcount++;
                            //log the trackables
                            if (_tbTrackingNumbers.Count > 0)
                            {
                                List<Utils.API.LiveV6.Trackable> tb = new List<Utils.API.LiveV6.Trackable>();
                                foreach (string t in _tbs75)
                                {
                                    tb.Add((Utils.API.LiveV6.Trackable)_tbTrackingNumbers[t]);
                                }
                                logTrackables(tb, 75, gc);
                            }
                            _context.Send(new SendOrPostCallback(delegate(object state)
                            {
                                (from ListViewItem a in listView1.Items where (a.Tag as Framework.Data.Geocache)==gc select a).FirstOrDefault().Checked = false;
                            }), null);

                        }
                        else
                        {
                            _errormessage = resp.Status.StatusMessage;
                            break;
                        }
                        index++;
                        if (!progress.UpdateProgress(STR_LOGGING, STR_LOGGING, _gcList.Count, index))
                        {
                            break;
                        }

                        Thread.Sleep(3000);
                    }
                }
            }
            catch (Exception e)
            {
                _errormessage = e.Message;
            }
        }

        private void logTrackables(List<Utils.API.LiveV6.Trackable> tbs, int logtypeid, Framework.Data.Geocache gc)
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
                req.CacheCode = gc.Code;
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

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            checkSubmitButton();
        }

    }
}
