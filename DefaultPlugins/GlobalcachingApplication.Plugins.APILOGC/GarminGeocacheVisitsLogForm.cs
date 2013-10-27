using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace GlobalcachingApplication.Plugins.APILOGC
{
    public partial class GarminGeocacheVisitsLogForm : Form
    {
        public const string STR_TITLE = "Garmin geocache_visits.txt";
        public const string STR_ERROR = "Error";
        public const string STR_YES = "Yes";
        public const string STR_NO = "No";
        public const string STR_IMPORTING = "Importing geocaches...";
        public const string STR_LOADFROMDEVICE = "Load from device...";
        public const string STR_GEOCACHEINFO = "Geocache info";
        public const string STR_SELECTFILE = "Select file...";
        public const string STR_IMPORTMISSING = "Import missing geocaches";
        public const string STR_LOGSELECTED = "Log selected geocache...";
        public const string STR_BATCHLOGSELECTED = "Batch log selected geocaches...";
        public const string STR_SELECTALL = "Select all geocaches";
        public const string STR_PERESENT = "Present";
        public const string STR_CODE = "Code";
        public const string STR_DATE = "Date";
        public const string STR_LOGTYPE = "Log type";
        public const string STR_COMMENT = "Comment";
        
        public class GeocacheVisitsItem
        {
            public string Code { get; set; }
            public DateTime LogDate { get; set; }
            public Framework.Data.LogType LogType { get; set; }
            public string Comment { get; set; }
            public bool InDatabase { get; set; }
        }

        private Framework.Interfaces.ICore _core = null;
        private Utils.API.GeocachingLiveV6 _client = null;
        private Utils.BasePlugin.Plugin _plugin = null;

        private ManualResetEvent _actionReady;
        private string _errormessage;
        private List<string> _geocacheCodes;
        private List<GeocacheVisitsItem> _geocacheVisitsItems = new List<GeocacheVisitsItem>();

        public GarminGeocacheVisitsLogForm()
        {
            InitializeComponent();
        }

        public GarminGeocacheVisitsLogForm(Utils.BasePlugin.Plugin plugin, Framework.Interfaces.ICore core, Utils.API.GeocachingLiveV6 client)
        {
            InitializeComponent();

            _core = core;
            _client = client;
            _plugin = plugin;

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOADFROMDEVICE);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GEOCACHEINFO);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTFILE);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IMPORTMISSING);
            this.button4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOGSELECTED);
            this.button6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_BATCHLOGSELECTED);
            this.button5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTALL);
            this.listView1.Columns[0].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PERESENT);
            this.listView1.Columns[1].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CODE);
            this.listView1.Columns[2].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DATE);
            this.listView1.Columns[3].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOGTYPE);
            this.listView1.Columns[4].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COMMENT);
        }

        private void processGeocacheVisitsFile(string[] lines)
        {
            listView1.Items.Clear();
            _geocacheVisitsItems.Clear();
            button3.Enabled = false;
            button5.Enabled = false;
            try
            {
                foreach (string s in lines)
                {
                    string[] parts = s.Replace("\0","").Split(new char[] { ',' }, 4);
                    if (parts.Length == 4)
                    {
                        GeocacheVisitsItem vi = new GeocacheVisitsItem();
                        vi.Code = parts[0];
                        vi.LogDate = DateTime.Parse(parts[1]).ToLocalTime();
                        vi.LogType = Utils.DataAccess.GetLogType(_core.LogTypes, parts[2]);
                        vi.Comment = parts[3].Replace("\"", "");
                        vi.InDatabase = Utils.DataAccess.GetGeocache(_core.Geocaches, vi.Code)!=null;

                        if (vi.Code.IndexOf("GC") > 0)
                        {
                            vi.Code = vi.Code.Substring(vi.Code.IndexOf("GC"));
                        }

                        _geocacheVisitsItems.Add(vi);
                    }
                }
            }
            catch
            {
            }
            if (_geocacheVisitsItems.Count > 0)
            {
                _geocacheVisitsItems.Sort(delegate(GeocacheVisitsItem t1, GeocacheVisitsItem t2)
                    { return (t1.LogDate.CompareTo(t2.LogDate)); }
                );
                foreach (GeocacheVisitsItem vi in _geocacheVisitsItems)
                {
                    ListViewItem lvi = new ListViewItem(new string[] {
                            vi.InDatabase?Utils.LanguageSupport.Instance.GetTranslation(STR_YES):Utils.LanguageSupport.Instance.GetTranslation(STR_NO),
                            vi.Code,
                            vi.LogDate.ToString("d"),
                            Utils.LanguageSupport.Instance.GetTranslation(vi.LogType.Name),
                            vi.Comment
                        });
                    lvi.Tag = vi;
                    listView1.Items.Add(lvi);
                }
            }
            button3.Enabled = (from a in _geocacheVisitsItems where !a.InDatabase select a).Count() > 0;
            button5.Enabled = (from a in _geocacheVisitsItems where a.InDatabase select a).Count() > 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    processGeocacheVisitsFile(System.IO.File.ReadAllLines(openFileDialog1.FileName));
                }
                catch
                {
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button4.Enabled = (listView1.SelectedItems.Count > 0);
            button6.Enabled = (listView1.SelectedItems.Count > 1);
        }


        private bool addGeocachesToDatabase(List<ListViewItem> lvis)
        {
            bool result = false;

            _geocacheCodes = new List<string>();
            foreach (ListViewItem lvi in lvis)
            {
                if (!(lvi.Tag as GeocacheVisitsItem).InDatabase)
                {
                    _geocacheCodes.Add((lvi.Tag as GeocacheVisitsItem).Code);
                }
            }
            if (_geocacheCodes.Count > 0)
            {
                this.ControlBox = false;
                this.groupBox1.Enabled = false;
                this.groupBox2.Enabled = false;
                this.panel1.Enabled = false;
                using (Utils.FrameworkDataUpdater upd = new Utils.FrameworkDataUpdater(_core))
                {
                    _actionReady = new ManualResetEvent(false);
                    Thread thrd = new Thread(new ThreadStart(this.getGeocachesThreadMethod));
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
                else
                {
                    result = true;
                }
                foreach (ListViewItem lvi in lvis)
                {
                    GeocacheVisitsItem vi = lvi.Tag as GeocacheVisitsItem;
                    if (!vi.InDatabase)
                    {
                        vi.InDatabase = Utils.DataAccess.GetGeocache(_core.Geocaches, vi.Code) != null;
                        if (vi.InDatabase)
                        {
                            lvi.SubItems[0].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_YES);
                        }
                    }
                }
                listView1_SelectedIndexChanged(this, null);
                button3.Enabled = (from a in _geocacheVisitsItems where !a.InDatabase select a).Count() > 0;
                button5.Enabled = (from a in _geocacheVisitsItems where a.InDatabase select a).Count() > 0;
                this.ControlBox = true;
                this.groupBox1.Enabled = true;
                this.groupBox2.Enabled = true;
                this.panel1.Enabled = true;
            }
            else
            {
                result = true;
            }
            return result;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            addGeocachesToDatabase((from ListViewItem a in listView1.Items select a).ToList());
        }

        private void getGeocachesThreadMethod()
        {
            try
            {
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock(_plugin, STR_IMPORTING, STR_IMPORTING, _geocacheCodes.Count, 0, true))
                {
                    int totalcount = _geocacheCodes.Count;
                    int index = 0;
                    int gcupdatecount;
                    TimeSpan interval = new TimeSpan(0, 0, 0, 2, 100);
                    DateTime prevCall = DateTime.MinValue;
                    bool dodelay;
                    gcupdatecount = 50;
                    dodelay = (_geocacheCodes.Count > 30);
                    while (_geocacheCodes.Count > 0)
                    {
                        if (dodelay)
                        {
                            TimeSpan ts = DateTime.Now - prevCall;
                            if (ts < interval)
                            {
                                Thread.Sleep(interval - ts);
                            }
                        }
                        Utils.API.LiveV6.SearchForGeocachesRequest req = new Utils.API.LiveV6.SearchForGeocachesRequest();
                        req.IsLite = _core.GeocachingComAccount.MemberTypeId == 1;
                        req.AccessToken = _client.Token;
                        req.CacheCode = new Utils.API.LiveV6.CacheCodeFilter();
                        req.CacheCode.CacheCodes = (from a in _geocacheCodes select a).Take(gcupdatecount).ToArray();
                        req.MaxPerPage = gcupdatecount;
                        req.GeocacheLogCount = 5;
                        index += req.CacheCode.CacheCodes.Length;
                        _geocacheCodes.RemoveRange(0, req.CacheCode.CacheCodes.Length);
                        prevCall = DateTime.Now;
                        var resp = _client.Client.SearchForGeocaches(req);
                        if (resp.Status.StatusCode == 0 && resp.Geocaches != null)
                        {
                            Utils.API.Import.AddGeocaches(_core, resp.Geocaches);
                        }
                        else
                        {
                            _errormessage = resp.Status.StatusMessage;
                            break;
                        }

                        if (!progress.UpdateProgress(STR_IMPORTING, STR_IMPORTING, totalcount, index))
                        {
                            break;
                        }
                    }
                }
            }
            catch
            {
            }
            _actionReady.Set();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0 && addGeocachesToDatabase((from ListViewItem a in listView1.SelectedItems select a).ToList()))
            {
                //right order
                List<ListViewItem> lvil = new List<ListViewItem>();
                foreach (ListViewItem lvi in listView1.Items)
                {
                    if (lvi.Selected)
                    {
                        lvil.Add(lvi);
                    }
                }
                foreach (ListViewItem lvi in lvil)
                {
                    Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(_core.Geocaches, (lvi.Tag as GeocacheVisitsItem).Code);
                    if (gc != null)
                    {
                        using (GeocacheLogForm dlg = new GeocacheLogForm(_core, _client, gc, lvil[0].Tag as GeocacheVisitsItem))
                        {
                            dlg.AskForNext = false;
                            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            using (Utils.FrameworkDataUpdater upd = new Utils.FrameworkDataUpdater(_core))
            {
                var list = from a in _geocacheVisitsItems
                           join Framework.Data.Geocache b in _core.Geocaches on a.Code equals b.Code
                           select b;
                foreach (var x in list)
                {
                    x.Selected = true;
                }
            }
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (GarminCommunicatorForm dlg = new GarminCommunicatorForm())
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        using (System.IO.TemporaryFile tmp = new System.IO.TemporaryFile(true))
                        {
                            System.IO.File.WriteAllText(tmp.Path, dlg.FileContents);
                            processGeocacheVisitsFile(System.IO.File.ReadAllLines((tmp.Path)));
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0 && addGeocachesToDatabase((from ListViewItem a in listView1.SelectedItems select a).ToList()))
            {
                GeocacheBatchLogForm dlg = new GeocacheBatchLogForm(_plugin, _core, _client, false);
                foreach (ListViewItem lvi in listView1.SelectedItems)
                {
                    Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(_core.Geocaches, (lvi.Tag as GeocacheVisitsItem).Code);
                    if (gc != null)
                    {
                        dlg.AddGeocache(gc, (lvi.Tag as GeocacheVisitsItem).LogDate);
                    }
                }
                dlg.ShowDialog();
            }
        }
    }
}
