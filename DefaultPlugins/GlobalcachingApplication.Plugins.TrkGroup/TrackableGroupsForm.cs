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
using System.Reflection;
using System.IO;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.TrkGroup
{
    public partial class TrackableGroupsForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Trackable groups";
        public const string STR_ERROR = "Error";
        public const string STR_TRACKABLES = "Trackables";
        public const string STR_SHOWGROUPONMAP = "Show group on map";
        public const string STR_SHOWSELECTEDONMAP = "Show selected on map";
        public const string STR_SHOWROUTEOFSELECTEDTRACKABLE = "Show route of selected trackable";
        public const string STR_UPDATESELECTEDTRACKABLES = "Update selected trackables";
        public const string STR_UPDATEALLTRACKABLES = "Update all trackables in group";
        public const string STR_DELETESELECTEDTRACKABLES = "Delete selected trackables";
        public const string STR_GOTOTRACKABLEPAGE = "Go to trackable page";
        public const string STR_GOTOTGEOCAGE = "Go to geocache";
        public const string STR_SHOWROUTE = "Show route";
        public const string STR_DELETE = "Delete";
        public const string STR_TRACKABLEGROUP = "Trackable group";
        public const string STR_TRACKABLECODES = "Trackable code(s)";
        public const string STR_CREATE = "Create";
        public const string STR_RENAME = "Rename";
        public const string STR_ADD = "Add";
        public const string STR_ADDYOUROWN = "Add your own";
        public const string STR_NAME = "Name";
        public const string STR_OWNER = "Owner";
        public const string STR_CREATEDON = "Created on";
        public const string STR_INGEOCACHE = "In geocache";
        public const string STR_TRAVELLEDDISTANCE = "Travelled distance";
        public const string STR_STEP = "Step";
        public const string STR_DATE = "Date";
        public const string STR_GEOCACHE = "Geocache";

        private class TrackableGroup
        {
            public int ID { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        private class TrackableItem
        {
            public bool? AllowedToBeCollected{ get; set; }
            public bool Archived { get; set; }
            public long BugTypeID { get; set; }
            public string Code { get; set; }
            public string CurrentGeocacheCode { get; set; }
            public string CurrentGoal { get; set; }
            public DateTime DateCreated { get; set; }
            public string Description { get; set; }
            public string IconUrl { get; set; }
            public long Id { get; set; }
            public bool InCollection { get; set; }
            public string Name { get; set; }
            public string TBTypeName { get; set; }
            public string Url { get; set; }
            public long WptTypeID { get; set; }
            public string Owner { get; set; }

            //calculated
            public int HopCount { get; set; }
            public int DiscoverCount { get; set; }
            public int InCacheCount { get; set; }
            public double DistanceKm { get; set; }
            public double? Lon { get; set; }
            public double? Lat { get; set; }
        }

        private class TravelItem
        {
            public string TrackableCode { get; set; }
            public string GeocacheCode { get; set; }
            public double Lat { get; set; }
            public double Lon { get; set; }
            public DateTime DateLogged { get; set; }
        }

        private class LogItem
        {
            public string TrackableCode { get; set; }
            public string LogCode { get; set; }
            public string GeocacheCode { get; set; }
            public int ID  { get; set; }
            public bool IsArchived  { get; set; }
            public string LoggedBy  { get; set; }
            public string LogGuid { get; set; }
            public bool LogIsEncoded { get; set; }
            public string LogText { get; set; }
            public int WptLogTypeId { get; set; }
            public string Url { get; set; }
            public DateTime UTCCreateDate { get; set; }
            public DateTime VisitDate { get; set; }
        }

        private Utils.DBCon _dbcon = null;
        private List<TrackableGroup> _trackableGroups = new List<TrackableGroup>();
        private string _errormessage = null;
        private TrackableGroup _activeTrackableGroup = null;
        private List<string> _tbList = new List<string>();
        private List<string> _columnHeaderList = new List<string>();

        public TrackableGroupsForm()
        {
            InitializeComponent();
        }

        public TrackableGroupsForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            listView1.ListViewItemSorter = new Utils.ListViewColumnSorter();
            (listView1.ListViewItemSorter as Utils.ListViewColumnSorter).SortColumn = 0;
            (listView1.ListViewItemSorter as Utils.ListViewColumnSorter).Order = SortOrder.Ascending;

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ADD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ADDYOUROWN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CREATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CREATEDON));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_DATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_DELETE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_DELETESELECTEDTRACKABLES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_GEOCACHE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_GOTOTGEOCAGE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_GOTOTRACKABLEPAGE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_INGEOCACHE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_OWNER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_RENAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SHOWGROUPONMAP));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SHOWROUTE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SHOWROUTEOFSELECTEDTRACKABLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SHOWSELECTEDONMAP));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_STEP));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_TRACKABLECODES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_TRACKABLEGROUP));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_TRACKABLES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_TRAVELLEDDISTANCE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_UPDATEALLTRACKABLES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_UPDATESELECTEDTRACKABLES));
            foreach (ColumnHeader c in listView1.Columns)
            {
                core.LanguageItems.Add(new Framework.Data.LanguageItem(c.Text));
                _columnHeaderList.Add(c.Text);
            }

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_DELAY));
        }

        private void initDatabase(Utils.DBCon dbcon)
        {
            object o = dbcon.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='groups'");
            if (o == null || o.GetType() == typeof(DBNull))
            {
                dbcon.ExecuteNonQuery("create table 'groups' (id integer, name text)");
            }
            o = dbcon.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='images'");
            if (o == null || o.GetType() == typeof(DBNull))
            {
                dbcon.ExecuteNonQuery("create table 'images' (url text, imagedata blob)");
            }
            o = dbcon.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='trackables'");
            if (o == null || o.GetType() == typeof(DBNull))
            {
                dbcon.ExecuteNonQuery("create table 'trackables' (groupid integer, AllowedToBeCollected integer, Archived integer, BugTypeID integer, Code text, CurrentGeocacheCode text, CurrentGoal text, DateCreated text, Description text, IconUrl text, Id integer, InCollection integer, Name text, TBTypeName text, Url text, WptTypeID integer, Owner text, HopCount integer, DiscoverCount integer, InCacheCount integer, DistanceKm real, Lat real, Lon real)");
                dbcon.ExecuteNonQuery("create index idx_trackablesgroup on trackables (groupid)");
                dbcon.ExecuteNonQuery("create index idx_trackablescode on trackables (code)");
            }
            o = dbcon.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='travels'");
            if (o == null || o.GetType() == typeof(DBNull))
            {
                dbcon.ExecuteNonQuery("create table 'travels' (pos integer, TrackableCode text, GeocacheCode text, lat real, lon real, DateLogged text)");
                dbcon.ExecuteNonQuery("create index idx_travels on travels (TrackableCode)");
            }
            o = dbcon.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='logs'");
            if (o == null || o.GetType() == typeof(DBNull))
            {
                dbcon.ExecuteNonQuery("create table 'logs' (TrackableCode text, ID integer, LogCode text, GeocacheCode text, IsArchived integer, LoggedBy text, LogGuid text, LogIsEncoded integer, LogText text, WptLogTypeId integer, Url text, UTCCreateDate text, VisitDate text)");
                dbcon.ExecuteNonQuery("create index idx_logstb on logs (TrackableCode)");
                dbcon.ExecuteNonQuery("create index idx_logsid on logs (ID)");
            }
        }

        private void initImageList()
        {
            imageList1.Images.Clear();
            DbDataReader dr = _dbcon.ExecuteReader("select * from images");
            while (dr.Read())
            {
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream((byte[])dr["imagedata"]))
                {
                    Image img = Image.FromStream(ms, true);
                    imageList1.Images.Add((string)dr["url"], img);
                }
            }
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            base.SelectedLanguageChanged(sender, e);
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.trackablesToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TRACKABLES);
            this.showAllOnMapToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHOWGROUPONMAP);
            this.showSelectedOnMapToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHOWSELECTEDONMAP);
            this.showRouteOfSelectedTrackableToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHOWROUTEOFSELECTEDTRACKABLE);
            this.updateSelectedTrackablesToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_UPDATESELECTEDTRACKABLES);
            this.updateAllTrackablesInGroupToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_UPDATEALLTRACKABLES);
            this.deleteSelectedTrackablesToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELETESELECTEDTRACKABLES);
            this.goToTrackablePageToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GOTOTRACKABLEPAGE);
            this.goToGeocacheToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GOTOTGEOCAGE);
            this.showRouteOfSelectedTrackableToolStripMenuItem1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SHOWROUTE);
            this.deleteToolStripMenuItem.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELETE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TRACKABLEGROUP);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TRACKABLECODES);
            this.buttonGroupDelete.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELETE);
            this.buttonGroupCreate.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CREATE);
            this.buttonGroupRename.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RENAME);
            this.buttonAddTrackables.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADD);
            this.buttonAddYouOwn.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDYOUROWN);

            for (int i=0; i<_columnHeaderList.Count; i++)
            {
                listView1.Columns[i].Text = Utils.LanguageSupport.Instance.GetTranslation(_columnHeaderList[i]);
            }
        }

        private void TrackableGroupsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void comboBoxGroup_SelectedValueChanged(object sender, EventArgs e)
        {
            TrackableGroup tg = comboBoxGroup.SelectedItem as TrackableGroup;
            _activeTrackableGroup = tg;
            listView1.Items.Clear();
            if (tg == null)
            {
                buttonGroupDelete.Enabled = false;
                buttonGroupRename.Enabled = false;
                buttonAddYouOwn.Enabled = false;
                buttonAddTrackables.Enabled = false;
                textBoxTBCodes.Enabled = false;
                showAllOnMapToolStripMenuItem.Enabled = false;
                updateAllTrackablesInGroupToolStripMenuItem.Enabled = false;
                textBoxGroupName.Text = "";
            }
            else
            {
                buttonGroupDelete.Enabled = true;
                buttonGroupRename.Enabled = true;
                buttonAddYouOwn.Enabled = true;
                textBoxTBCodes.Enabled = true;
                updateAllTrackablesInGroupToolStripMenuItem.Enabled = true;
                showAllOnMapToolStripMenuItem.Enabled = true;
                textBoxGroupName.Text = tg.Name;
                textBoxTBCodes_TextChanged(sender, e);

                listView1.BeginUpdate();
                try
                {
                    DbDataReader dr = _dbcon.ExecuteReader(string.Format("select * from trackables where groupid={0}", tg.ID));
                    while (dr.Read())
                    {
                        TrackableItem trk = new TrackableItem();
                        trk.Code = (string)dr["Code"];
                        if (dr["AllowedToBeCollected"] != null && dr["AllowedToBeCollected"].GetType() != typeof(DBNull))
                        {
                            trk.AllowedToBeCollected = (int)dr["AllowedToBeCollected"]!=0;
                        }
                        else
                        {
                            trk.AllowedToBeCollected = null;
                        }
                        trk.Archived = (int)dr["Archived"] != 0;
                        trk.BugTypeID = (int)dr["BugTypeID"];
                        trk.CurrentGeocacheCode = (string)dr["CurrentGeocacheCode"];
                        trk.CurrentGoal = (string)dr["CurrentGoal"];
                        trk.DateCreated = DateTime.Parse((string)dr["DateCreated"]);
                        trk.Description = (string)dr["Description"];
                        trk.IconUrl = (string)dr["IconUrl"];
                        trk.Id = (int)dr["Id"];
                        trk.InCollection = (int)dr["InCollection"] != 0;
                        trk.Name = (string)dr["Name"];
                        trk.TBTypeName = (string)dr["TBTypeName"];
                        trk.Url = (string)dr["Url"];
                        trk.WptTypeID = (int)dr["WptTypeID"];
                        trk.Owner = (string)dr["Owner"];

                        if (dr["HopCount"] != null && dr["HopCount"].GetType() != typeof(DBNull))
                        {
                            trk.HopCount = (int)dr["HopCount"];
                        }
                        else
                        {
                            trk.HopCount = 0;
                        }
                        if (dr["DiscoverCount"] != null && dr["DiscoverCount"].GetType() != typeof(DBNull))
                        {
                            trk.DiscoverCount = (int)dr["DiscoverCount"];
                        }
                        else
                        {
                            trk.DiscoverCount = 0;
                        }
                        if (dr["InCacheCount"] != null && dr["InCacheCount"].GetType() != typeof(DBNull))
                        {
                            trk.InCacheCount = (int)dr["InCacheCount"];
                        }
                        else
                        {
                            trk.InCacheCount = 0;
                        }
                        if (dr["DistanceKm"] != null && dr["DistanceKm"].GetType() != typeof(DBNull))
                        {
                            trk.DistanceKm = (double)dr["DistanceKm"];
                        }
                        else
                        {
                            trk.DistanceKm = 0.0;
                        }
                        if (dr["Lat"] != null && dr["Lat"].GetType() != typeof(DBNull))
                        {
                            trk.Lat = (double)dr["Lat"];
                        }
                        else
                        {
                            trk.Lat = null;
                        }
                        if (dr["Lon"] != null && dr["Lon"].GetType() != typeof(DBNull))
                        {
                            trk.Lon = (double)dr["Lon"];
                        }
                        else
                        {
                            trk.Lon = null;
                        }

                        ListViewItem lv = new ListViewItem(new string[] { trk.IconUrl, trk.Code, trk.Name, trk.Owner, trk.CurrentGeocacheCode, trk.HopCount.ToString().PadLeft(5), trk.InCacheCount.ToString().PadLeft(5), trk.DiscoverCount.ToString().PadLeft(5), trk.DistanceKm.ToString("0.0").PadLeft(9) }, trk.IconUrl);
                        lv.Tag = trk;
                        listView1.Items.Add(lv);
                    }
                }
                catch
                {
                }
                listView1.EndUpdate();
            }
        }

        private void textBoxGroupName_TextChanged(object sender, EventArgs e)
        {
            string s = textBoxGroupName.Text.Trim();
            TrackableGroup tg = comboBoxGroup.SelectedItem as TrackableGroup;
            if (string.IsNullOrEmpty(s))
            {
                buttonGroupRename.Enabled = false;
                buttonGroupCreate.Enabled = false;
            }
            else
            {
                int cnt = (from t in _trackableGroups where t.Name.ToLower() == s select t).Count();
                buttonGroupRename.Enabled = (tg != null && cnt == 0);
                buttonGroupCreate.Enabled = cnt == 0;
            }
        }

        private void buttonGroupCreate_Click(object sender, EventArgs e)
        {
            try
            {
                string s = textBoxGroupName.Text.Trim();
                if (!string.IsNullOrEmpty(s))
                {
                    if ((long)_dbcon.ExecuteScalar(string.Format("select count(1) from groups where name='{0}'", s.Replace("'", "''"))) == 0)
                    {
                        int maxId = 1;
                        if (_trackableGroups.Count > 0)
                        {
                            _trackableGroups.Max(x => x.ID);
                            maxId++;
                        }
                        TrackableGroup tg = new TrackableGroup();
                        tg.ID = maxId;
                        tg.Name = s;

                        _dbcon.ExecuteNonQuery(string.Format("insert into groups (id, name) values ({0}, '{1}')",tg.ID,tg.Name.Replace("'","''")));
                        _trackableGroups.Add(tg);
                        comboBoxGroup.Items.Add(tg);
                        comboBoxGroup.SelectedItem = tg;
                        textBoxGroupName_TextChanged(this, EventArgs.Empty);
                    }
                }
            }
            catch
            {
            }
        }

        private void buttonGroupDelete_Click(object sender, EventArgs e)
        {
            try
            {
                TrackableGroup tg = comboBoxGroup.SelectedItem as TrackableGroup;
                if (tg != null)
                {
                    _dbcon.ExecuteNonQuery(string.Format("delete from trackables where groupid={0}", tg.ID));
                    _dbcon.ExecuteNonQuery(string.Format("delete from groups where id={0}", tg.ID));
                    _trackableGroups.Remove(tg);
                    comboBoxGroup.Items.Remove(tg);
                    comboBoxGroup_SelectedValueChanged(this, EventArgs.Empty);
                    textBoxGroupName_TextChanged(this, EventArgs.Empty);
                }
            }
            catch
            {
            }
        }

        private void buttonGroupRename_Click(object sender, EventArgs e)
        {
            try
            {
                string s = textBoxGroupName.Text.Trim();
                if (!string.IsNullOrEmpty(s))
                {
                    TrackableGroup tg = comboBoxGroup.SelectedItem as TrackableGroup;
                    if (tg != null)
                    {
                        int cnt = (from t in _trackableGroups where t.Name.ToLower() == s select t).Count();
                        if (cnt == 0)
                        {
                            _dbcon.ExecuteNonQuery(string.Format("update groups set name='{1}' where id={0}", tg.ID, s.Replace("'", "''")));
                            tg.Name = s;
                            comboBoxGroup.Items.Clear();
                            comboBoxGroup.Items.AddRange(_trackableGroups.ToArray());
                            comboBoxGroup.SelectedItem = tg;
                            textBoxGroupName_TextChanged(this, EventArgs.Empty);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private async void buttonAddYouOwn_Click(object sender, EventArgs e)
        {
            _errormessage = null;
            await Task.Run(() =>
                {
                    this.addOwnTrackablesThreadMethod();
                });
            if (!string.IsNullOrEmpty(_errormessage))
            {
                System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            //reload data
            listView1.Items.Clear();
            initImageList();
            comboBoxGroup_SelectedValueChanged(sender, e);
        }

        private void AddTrackableToDatabase(TrackableItem t)
        {
            if (!string.IsNullOrEmpty(t.IconUrl))
            {
                DbParameter par;

                long cnt = (long)_dbcon.ExecuteScalar(string.Format("select count(1) from images where url='{0}'", t.IconUrl));
                if (cnt == 0)
                {
                    try
                    {
                        using (System.Net.WebClient wc = new System.Net.WebClient())
                        {
                            byte[] data = wc.DownloadData(t.IconUrl);
                            if (data != null)
                            {
                                _dbcon.Command.Parameters.Clear();
                                par = _dbcon.Command.CreateParameter();
                                par.ParameterName = "@data";
                                par.DbType = DbType.Binary;
                                par.Value = data;
                                _dbcon.Command.Parameters.Add(par);
                                _dbcon.ExecuteNonQuery(string.Format("insert into images (url, imagedata) values ('{0}', @data)", t.IconUrl));
                                _dbcon.Command.Parameters.Clear();
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                _dbcon.Command.Parameters.Clear();
                par = _dbcon.Command.CreateParameter();
                par.ParameterName = "@groupid";
                par.DbType = DbType.Int32;
                par.Value = _activeTrackableGroup.ID;
                _dbcon.Command.Parameters.Add(par);
                par = _dbcon.Command.CreateParameter();
                par.ParameterName = "@AllowedToBeCollected";
                par.DbType = DbType.Int32;
                if (t.AllowedToBeCollected == null)
                {
                    par.Value = DBNull.Value;
                }
                else
                {
                    par.Value = t.AllowedToBeCollected == true ? 1 : 0;
                }
                _dbcon.Command.Parameters.Add(par);
                par = _dbcon.Command.CreateParameter();
                par.ParameterName = "@Archived";
                par.DbType = DbType.Int32;
                par.Value = t.Archived?1:0;
                _dbcon.Command.Parameters.Add(par);
                par = _dbcon.Command.CreateParameter();
                par.ParameterName = "@BugTypeID";
                par.DbType = DbType.Int32;
                par.Value = t.BugTypeID;
                _dbcon.Command.Parameters.Add(par);
                par = _dbcon.Command.CreateParameter();
                par.ParameterName = "@Code";
                par.DbType = DbType.String;
                par.Value = t.Code;
                _dbcon.Command.Parameters.Add(par);
                par = _dbcon.Command.CreateParameter();
                par.ParameterName = "@CurrentGeocacheCode";
                par.DbType = DbType.String;
                par.Value = t.CurrentGeocacheCode??"";
                _dbcon.Command.Parameters.Add(par);
                par = _dbcon.Command.CreateParameter();
                par.ParameterName = "@CurrentGoal";
                par.DbType = DbType.String;
                par.Value = t.CurrentGoal ?? "";
                _dbcon.Command.Parameters.Add(par);
                par = _dbcon.Command.CreateParameter();
                par.ParameterName = "@DateCreated";
                par.DbType = DbType.String;
                par.Value = t.DateCreated.ToString("u");
                _dbcon.Command.Parameters.Add(par);
                par = _dbcon.Command.CreateParameter();
                par.ParameterName = "@Description";
                par.DbType = DbType.String;
                par.Value = t.Description??"";
                _dbcon.Command.Parameters.Add(par);
                par = _dbcon.Command.CreateParameter();
                par.ParameterName = "@IconUrl";
                par.DbType = DbType.String;
                par.Value = t.IconUrl ?? "";
                _dbcon.Command.Parameters.Add(par);
                par = _dbcon.Command.CreateParameter();
                par.ParameterName = "@Id";
                par.DbType = DbType.Int32;
                par.Value = t.Id;
                _dbcon.Command.Parameters.Add(par);
                par = _dbcon.Command.CreateParameter();
                par.ParameterName = "@InCollection";
                par.DbType = DbType.Int32;
                par.Value = t.InCollection?1:0;
                _dbcon.Command.Parameters.Add(par);
                par = _dbcon.Command.CreateParameter();
                par.ParameterName = "@Name";
                par.DbType = DbType.String;
                par.Value = t.Name ?? "";
                _dbcon.Command.Parameters.Add(par);
                par = _dbcon.Command.CreateParameter();
                par.ParameterName = "@TBTypeName";
                par.DbType = DbType.String;
                par.Value = t.TBTypeName ?? "";
                _dbcon.Command.Parameters.Add(par);
                par = _dbcon.Command.CreateParameter();
                par.ParameterName = "@Url";
                par.DbType = DbType.String;
                par.Value = t.Url ?? "";
                _dbcon.Command.Parameters.Add(par);
                par = _dbcon.Command.CreateParameter();
                par.ParameterName = "@WptTypeID";
                par.DbType = DbType.Int32;
                par.Value = t.WptTypeID;
                _dbcon.Command.Parameters.Add(par);
                par = _dbcon.Command.CreateParameter();
                par.ParameterName = "@Owner";
                par.DbType = DbType.String;
                par.Value = t.Owner ?? "";
                _dbcon.Command.Parameters.Add(par);

                if (_dbcon.ExecuteNonQuery("update trackables set AllowedToBeCollected=@AllowedToBeCollected, Archived=@Archived, BugTypeID=@BugTypeID, CurrentGeocacheCode=@CurrentGeocacheCode, CurrentGoal=@CurrentGoal, DateCreated=@DateCreated, Description=@Description, IconUrl=@IconUrl, Id=@Id, InCollection=@InCollection, Name=@Name, TBTypeName=@TBTypeName, Url=@Url, WptTypeID=@WptTypeID, Owner=@Owner where groupid=@groupid and Code=@Code") == 0)
                {
                    _dbcon.ExecuteNonQuery("insert into trackables (groupid, AllowedToBeCollected, Archived, BugTypeID, Code, CurrentGeocacheCode, CurrentGoal, DateCreated, Description, IconUrl, Id, InCollection, Name, TBTypeName, Url, WptTypeID, Owner) values (@groupid, @AllowedToBeCollected, @Archived, @BugTypeID, @Code, @CurrentGeocacheCode, @CurrentGoal, @DateCreated, @Description, @IconUrl, @Id, @InCollection, @Name, @TBTypeName, @Url, @WptTypeID, @Owner)");
                }
                _dbcon.Command.Parameters.Clear();
            }
        }

        private void AddTravelListToDatabase(string tbCode, List<TravelItem> travelList)
        {
            _dbcon.ExecuteNonQuery(string.Format("delete from travels where TrackableCode='{0}'", tbCode));

            DbParameter par;
            _dbcon.Command.Parameters.Clear();
            par = _dbcon.Command.CreateParameter();
            par.ParameterName = "@pos";
            par.DbType = DbType.Int32;
            _dbcon.Command.Parameters.Add(par);
            par = _dbcon.Command.CreateParameter();
            par.ParameterName = "@TrackableCode";
            par.DbType = DbType.String;
            _dbcon.Command.Parameters.Add(par);
            par = _dbcon.Command.CreateParameter();
            par.ParameterName = "@GeocacheCode";
            par.DbType = DbType.String;
            _dbcon.Command.Parameters.Add(par);
            par = _dbcon.Command.CreateParameter();
            par.ParameterName = "@lat";
            par.DbType = DbType.Double;
            _dbcon.Command.Parameters.Add(par);
            par = _dbcon.Command.CreateParameter();
            par.ParameterName = "@lon";
            par.DbType = DbType.Double;
            _dbcon.Command.Parameters.Add(par);
            par = _dbcon.Command.CreateParameter();
            par.ParameterName = "@DateLogged";
            par.DbType = DbType.String;
            _dbcon.Command.Parameters.Add(par);
            double DistanceKm = 0.0;
            double? LastLat = null;
            double? LastLon = null;
            for (int i = 0; i < travelList.Count; i++)
            {
                _dbcon.Command.Parameters["@pos"].Value = i;
                _dbcon.Command.Parameters["@TrackableCode"].Value = travelList[i].TrackableCode;
                _dbcon.Command.Parameters["@GeocacheCode"].Value = travelList[i].GeocacheCode??"";
                _dbcon.Command.Parameters["@lat"].Value = travelList[i].Lat;
                _dbcon.Command.Parameters["@lon"].Value = travelList[i].Lon;
                _dbcon.Command.Parameters["@DateLogged"].Value = travelList[i].DateLogged.ToString("u");
                _dbcon.ExecuteNonQuery("insert into travels (pos, TrackableCode, GeocacheCode, lat, lon, DateLogged) values (@pos, @TrackableCode, @GeocacheCode, @lat, @lon, @DateLogged)");

                LastLat = travelList[i].Lat;
                LastLon = travelList[i].Lon;
                if (i > 0)
                {
                    DistanceKm += (double)Utils.Calculus.CalculateDistance(travelList[i-1].Lat, travelList[i-1].Lon, travelList[i].Lat, travelList[i].Lon).EllipsoidalDistance / 1000.0;
                }
            }
            _dbcon.Command.Parameters.Clear();

            if (LastLat != null && LastLon != null)
            {
                _dbcon.ExecuteNonQuery(string.Format("update trackables set DistanceKm={0}, Lat={2}, Lon={3} where Code='{1}'", DistanceKm.ToString().Replace(',', '.'), tbCode, LastLat.ToString().Replace(',', '.'), LastLon.ToString().Replace(',', '.')));
            }
        }


        private void AddLogListToDatabase(string tbCode, List<LogItem> logList)
        {
            List<int> logsIndb = new List<int>();
            DbDataReader dr = _dbcon.ExecuteReader(string.Format("select ID from logs where TrackableCode='{0}'", tbCode));
            while (dr.Read())
            {
                logsIndb.Add((int)dr["ID"]);
            }

            DbParameter par;
            _dbcon.Command.Parameters.Clear();
            par = _dbcon.Command.CreateParameter();
            par.ParameterName = "@TrackableCode";
            par.DbType = DbType.String;
            _dbcon.Command.Parameters.Add(par);
            par = _dbcon.Command.CreateParameter();
            par.ParameterName = "@ID";
            par.DbType = DbType.Int32;
            _dbcon.Command.Parameters.Add(par);
            par = _dbcon.Command.CreateParameter();
            par.ParameterName = "@LogCode";
            par.DbType = DbType.String;
            _dbcon.Command.Parameters.Add(par);
            par = _dbcon.Command.CreateParameter();
            par.ParameterName = "@GeocacheCode";
            par.DbType = DbType.String;
            _dbcon.Command.Parameters.Add(par);
            par = _dbcon.Command.CreateParameter();
            par.ParameterName = "@IsArchived";
            par.DbType = DbType.Int32;
            _dbcon.Command.Parameters.Add(par);
            par = _dbcon.Command.CreateParameter();
            par.ParameterName = "@LoggedBy";
            par.DbType = DbType.String;
            _dbcon.Command.Parameters.Add(par);
            par = _dbcon.Command.CreateParameter();
            par.ParameterName = "@LogGuid";
            par.DbType = DbType.String;
            _dbcon.Command.Parameters.Add(par);
            par = _dbcon.Command.CreateParameter();
            par.ParameterName = "@LogText";
            par.DbType = DbType.String;
            _dbcon.Command.Parameters.Add(par);
            par = _dbcon.Command.CreateParameter();
            par.ParameterName = "@LogIsEncoded";
            par.DbType = DbType.Int32;
            _dbcon.Command.Parameters.Add(par);
            par = _dbcon.Command.CreateParameter();
            par.ParameterName = "@WptLogTypeId";
            par.DbType = DbType.Int32;
            _dbcon.Command.Parameters.Add(par);
            par = _dbcon.Command.CreateParameter();
            par.ParameterName = "@Url";
            par.DbType = DbType.String;
            _dbcon.Command.Parameters.Add(par);
            par = _dbcon.Command.CreateParameter();
            par.ParameterName = "@UTCCreateDate";
            par.DbType = DbType.String;
            _dbcon.Command.Parameters.Add(par);
            par = _dbcon.Command.CreateParameter();
            par.ParameterName = "@VisitDate";
            par.DbType = DbType.String;
            _dbcon.Command.Parameters.Add(par);

            int HopCount = 0;
            int DiscoverCount = 0;
            int InCacheCount = 0;
            for (int i=0; i<logList.Count; i++)
            {
                _dbcon.Command.Parameters["@TrackableCode"].Value = logList[i].TrackableCode;
                _dbcon.Command.Parameters["@ID"].Value = logList[i].ID;
                _dbcon.Command.Parameters["@LogCode"].Value = logList[i].LogCode;
                _dbcon.Command.Parameters["@GeocacheCode"].Value = logList[i].GeocacheCode??"";
                _dbcon.Command.Parameters["@LogIsEncoded"].Value = logList[i].LogIsEncoded ? 1 : 0;
                _dbcon.Command.Parameters["@IsArchived"].Value = logList[i].IsArchived ? 1 : 0;
                _dbcon.Command.Parameters["@LoggedBy"].Value = logList[i].LoggedBy ?? "";
                _dbcon.Command.Parameters["@LogGuid"].Value = logList[i].LogGuid ?? "";
                _dbcon.Command.Parameters["@LogText"].Value = logList[i].LogText ?? "";
                _dbcon.Command.Parameters["@WptLogTypeId"].Value = logList[i].WptLogTypeId;
                _dbcon.Command.Parameters["@Url"].Value = logList[i].Url??"";
                _dbcon.Command.Parameters["@UTCCreateDate"].Value = logList[i].UTCCreateDate.ToString("u");
                _dbcon.Command.Parameters["@VisitDate"].Value = logList[i].VisitDate.ToString("u");

                if (logsIndb.Contains(logList[i].ID))
                {
                    //for performance reasons, do not update. Assume nothing changed
                    //_dbcon.ExecuteNonQuery("update logs....");
                    logsIndb.Remove(logList[i].ID);
                }
                else
                {
                    _dbcon.ExecuteNonQuery("insert into logs (TrackableCode, ID, LogCode, GeocacheCode, IsArchived, LoggedBy, LogGuid, LogIsEncoded, LogText, WptLogTypeId, Url, UTCCreateDate, VisitDate) values (@TrackableCode, @ID, @LogCode, @GeocacheCode, @IsArchived, @LoggedBy, @LogGuid, @LogIsEncoded, @LogText, @WptLogTypeId, @Url, @UTCCreateDate, @VisitDate)");
                }

                switch (logList[i].WptLogTypeId)
                {
                    case 75: //visit
                        HopCount++;
                        break;
                    case 14: //dropped
                        HopCount++;
                        InCacheCount++;
                        break;
                    case 48: //disc
                        DiscoverCount++;
                        break;
                }
            }
            _dbcon.ExecuteNonQuery(string.Format("update trackables set HopCount={0}, InCacheCount={1}, DiscoverCount={2} where Code='{3}'", HopCount, InCacheCount, DiscoverCount, tbCode));
            foreach (int id in logsIndb)
            {
                _dbcon.ExecuteNonQuery(string.Format("delete from logs where ID={0}", id));
            }
        }

        private TrackableItem GetTrackableItemFromLiveAPI(Utils.API.LiveV6.Trackable t)
        {
            TrackableItem trk = new TrackableItem();
            trk.Code = t.Code;
            trk.AllowedToBeCollected = t.AllowedToBeCollected;
            trk.Archived = t.Archived;
            trk.BugTypeID = t.BugTypeID;
            trk.CurrentGeocacheCode = t.CurrentGeocacheCode;
            trk.CurrentGoal = t.CurrentGoal;
            trk.DateCreated = t.DateCreated;
            trk.Description = t.Description;
            trk.IconUrl = t.IconUrl;
            trk.Id = t.Id;
            trk.InCollection = t.InCollection;
            trk.Name = t.Name;
            trk.TBTypeName = t.TBTypeName;
            trk.Url = t.Url;
            trk.WptTypeID = t.WptTypeID;
            if (t.OriginalOwner != null)
            {
                trk.Owner = t.OriginalOwner.UserName;
            }
            else
            {
                trk.Owner = "";
            }
            return trk;
        }

        private void addOwnTrackablesThreadMethod()
        {
            try
            {
                using (Utils.ProgressBlock progr = new Utils.ProgressBlock(this.OwnerPlugin as Utils.BasePlugin.Plugin, "Insert own trackables...", "Fetching data from geocaching.com...", 1, 0))
                {
                    using (var api = new Utils.API.GeocachingLiveV6(Core))
                    {
                        var req = new Utils.API.LiveV6.GetTrackablesByOwnerRequest();
                        req.AccessToken = api.Token;
                        req.TrackableLogsCount = 0;
                        req.StartIndex = 0;
                        req.MaxPerPage = 10;
                        int total = 0;
                        while (true)
                        {
                            var resp = api.Client.GetOwnedTrackables(req);
                            if (resp.Status.StatusCode == 0)
                            {
                                if (resp.Trackables != null)
                                {
                                    foreach (var t in resp.Trackables)
                                    {
                                        AddTrackableToDatabase(GetTrackableItemFromLiveAPI(t));
                                        total++;
                                    }
                                    progr.UpdateProgress("Insert own trackables...", "Fetching data from geocaching.com...", total, 2 * total);
                                }
                                if (resp.Trackables.Count() < req.MaxPerPage)
                                {
                                    break;
                                }
                                else
                                {
                                    req.StartIndex = total;
                                    Thread.Sleep(2000);
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
        }

        private void addTrackablesThreadMethod()
        {
            try
            {
                using (Utils.ProgressBlock progr = new Utils.ProgressBlock(this.OwnerPlugin as Utils.BasePlugin.Plugin, "Get trackable data...", "Fetching data from geocaching.com...", _tbList.Count, 0))
                {
                    using (var api = new Utils.API.GeocachingLiveV6(Core))
                    {
                        for (int i = 0; i < _tbList.Count; i++)
                        {
                            if (_tbList[i].ToUpper().StartsWith("TB"))
                            {
                                var resp = api.Client.GetTrackablesByTBCode(api.Token, _tbList[i].ToUpper(), 0);
                                if (resp.Status.StatusCode == 0)
                                {
                                    if (resp.Trackables != null)
                                    {
                                        foreach (var t in resp.Trackables)
                                        {
                                            AddTrackableToDatabase(GetTrackableItemFromLiveAPI(t));
                                        }
                                        progr.UpdateProgress("Get trackable data...", "Fetching data from geocaching.com...", _tbList.Count, i);
                                        Thread.Sleep(2000);
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
            }
            catch
            {
            }
        }


        private void GetAllTrackableDataThreadMethod()
        {
            try
            {
                using (Utils.ProgressBlock progr = new Utils.ProgressBlock(this.OwnerPlugin as Utils.BasePlugin.Plugin, "Get trackable data...", "Fetching data from geocaching.com...", _tbList.Count, 0, true))
                {
                    bool cancel = false;
                    using (var api = new Utils.API.GeocachingLiveV6(Core))
                    {
                        for (int i = 0; i < _tbList.Count; i++)
                        {
                            if (_tbList[i].ToUpper().StartsWith("TB"))
                            {
                                Core.DebugLog(Framework.Data.DebugLogLevel.Info, OwnerPlugin, null, string.Format("{0} - GetTrackablesByTBCode - ", DateTime.Now.ToString("HH:mm:ss.fff")));
                                var resp = api.Client.GetTrackablesByTBCode(api.Token, _tbList[i].ToUpper(), 0);
                                if (resp.Status.StatusCode == 0)
                                {
                                    if (resp.Trackables != null)
                                    {
                                        foreach (var t in resp.Trackables)
                                        {
                                            TrackableItem trk = GetTrackableItemFromLiveAPI(t);
                                            AddTrackableToDatabase(trk);

                                            Core.DebugLog(Framework.Data.DebugLogLevel.Info, OwnerPlugin, null, string.Format("{0} - GetTrackableTravelList - ", DateTime.Now.ToString("HH:mm:ss.fff")));
                                            var resp2 = api.Client.GetTrackableTravelList(api.Token, trk.Code);
                                            if (resp2.Status.StatusCode == 0)
                                            {
                                                if (resp2.TrackableTravels != null)
                                                {
                                                    List<TravelItem> travelList = new List<TravelItem>();
                                                    foreach (var tt in resp2.TrackableTravels)
                                                    {
                                                        if (tt.Latitude != null && tt.Longitude != null)
                                                        {
                                                            TravelItem ti = new TravelItem();
                                                            ti.TrackableCode = trk.Code;
                                                            if (tt.CacheID != null)
                                                            {
                                                                ti.GeocacheCode = Utils.Conversion.GetCacheCodeFromCacheID((int)tt.CacheID);
                                                            }
                                                            else
                                                            {
                                                                ti.GeocacheCode = "";
                                                            }
                                                            ti.DateLogged = tt.DateLogged;
                                                            ti.Lat = (double)tt.Latitude;
                                                            ti.Lon = (double)tt.Longitude;
                                                            travelList.Add(ti);
                                                        }
                                                    }
                                                    AddTravelListToDatabase(trk.Code, travelList);
                                                }

                                                //get all logs
                                                List<LogItem> logs = new List<LogItem>();
                                                int maxPageSize = 30;
                                                while (true)
                                                {
                                                    Core.DebugLog(Framework.Data.DebugLogLevel.Info, OwnerPlugin, null, string.Format("{0} - GetTrackableLogsByTBCode - ", DateTime.Now.ToString("HH:mm:ss.fff")));
                                                    var resp3 = api.Client.GetTrackableLogsByTBCode(api.Token, trk.Code, logs.Count, maxPageSize);
                                                    if (resp3.Status.StatusCode == 0)
                                                    {
                                                        if (resp3.TrackableLogs != null)
                                                        {
                                                            foreach (var tl in resp3.TrackableLogs)
                                                            {
                                                                LogItem li = new LogItem();
                                                                li.TrackableCode = trk.Code;
                                                                if (tl.CacheID != null)
                                                                {
                                                                    li.GeocacheCode = Utils.Conversion.GetCacheCodeFromCacheID((int)tl.CacheID);
                                                                }
                                                                else
                                                                {
                                                                    li.GeocacheCode = "";
                                                                }
                                                                li.LogCode = tl.Code;
                                                                li.ID = tl.ID;
                                                                li.IsArchived = tl.IsArchived;
                                                                li.LoggedBy = tl.LoggedBy==null?"":tl.LoggedBy.UserName;
                                                                li.LogGuid = tl.LogGuid.ToString();
                                                                li.LogIsEncoded = tl.LogIsEncoded;
                                                                li.LogText = tl.LogText;
                                                                li.WptLogTypeId = tl.LogType == null ? -1 : (int)tl.LogType.WptLogTypeId;
                                                                li.Url = tl.Url;
                                                                li.UTCCreateDate = tl.UTCCreateDate;
                                                                li.VisitDate = tl.VisitDate;
                                                                logs.Add(li);
                                                            }
                                                            if (resp3.TrackableLogs.Count() < maxPageSize)
                                                            {
                                                                break;
                                                            }
                                                            Thread.Sleep(2000 + PluginSettings.Instance.TimeBetweenTrackableUpdates);
                                                        }
                                                        else
                                                        {
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _errormessage = resp3.Status.StatusMessage;
                                                        break;
                                                    }
                                                }
                                                if (string.IsNullOrEmpty(_errormessage))
                                                {
                                                    AddLogListToDatabase(trk.Code, logs);
                                                }
                                            }
                                            else
                                            {
                                                _errormessage = resp2.Status.StatusMessage;
                                                break;
                                            }
                                            if (!progr.UpdateProgress("Get trackable data...", "Fetching data from geocaching.com...", _tbList.Count, i))
                                            {
                                                cancel = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    _errormessage = resp.Status.StatusMessage;
                                }
                            }
                            if (cancel || !string.IsNullOrEmpty(_errormessage))
                            {
                                break;
                            }
                            if (PluginSettings.Instance.TimeBetweenTrackableUpdates > 0)
                            {
                                System.Threading.Thread.Sleep(PluginSettings.Instance.TimeBetweenTrackableUpdates);
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void TrackableGroupsForm_Shown(object sender, EventArgs e)
        {
            PluginSettings.Instance.DatabaseFileName = System.IO.Path.Combine(Core.PluginDataPath, "TrkGroup.db3" );

            try
            {
                _dbcon = new Utils.DBConComSqlite(PluginSettings.Instance.DatabaseFileName);
                initDatabase(_dbcon);
                DbDataReader dr = _dbcon.ExecuteReader("select * from groups");
                while (dr.Read())
                {
                    TrackableGroup tg = new TrackableGroup();
                    tg.ID = (int)dr["id"];
                    tg.Name = (string)dr["name"];
                    _trackableGroups.Add(tg);
                }
                initImageList();
                comboBoxGroup.Items.AddRange(_trackableGroups.ToArray());
            }
            catch
            {
                _dbcon = null;
            }

            comboBoxGroup_SelectedValueChanged(this, EventArgs.Empty);
            SelectedLanguageChanged(this, EventArgs.Empty);
        }

        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            //e.DrawDefault = true;
        }

        private void listView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                Image img = imageList1.Images[e.SubItem.Text];
                if (img != null)
                {
                    e.Graphics.DrawImageUnscaled(img, e.Bounds.X, e.Bounds.Y);
                }
            }
            else
            {
                e.DrawDefault = true;
            }
        }

        private async void buttonAddTrackables_Click(object sender, EventArgs e)
        {
            string[] s = textBoxTBCodes.Text.Split(new char[]{' ','\t',',',':',';'}, StringSplitOptions.RemoveEmptyEntries);
            if (s.Length > 0)
            {
                _tbList.Clear();
                _tbList.AddRange(s);
                _errormessage = null;
                await Task.Run(() =>
                    {
                        this.addTrackablesThreadMethod();
                    });
                if (!string.IsNullOrEmpty(_errormessage))
                {
                    System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
                //reload data
                listView1.Items.Clear();
                initImageList();
                comboBoxGroup_SelectedValueChanged(sender, e);
            }
        }

        private void textBoxTBCodes_TextChanged(object sender, EventArgs e)
        {
            buttonAddTrackables.Enabled = (textBoxTBCodes.Text.ToUpper().StartsWith("TB") && buttonAddYouOwn.Enabled);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            deleteSelectedTrackablesToolStripMenuItem.Enabled = (listView1.SelectedIndices != null && listView1.SelectedIndices.Count > 0);
            updateSelectedTrackablesToolStripMenuItem.Enabled = deleteSelectedTrackablesToolStripMenuItem.Enabled;
            showSelectedOnMapToolStripMenuItem.Enabled = deleteSelectedTrackablesToolStripMenuItem.Enabled;
            showRouteOfSelectedTrackableToolStripMenuItem.Enabled = deleteSelectedTrackablesToolStripMenuItem.Enabled;
        }

        private void deleteSelectedTrackablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices != null && listView1.SelectedIndices.Count > 0)
            {
                try
                {
                    foreach (int index in listView1.SelectedIndices)
                    {
                        TrackableItem t = listView1.Items[index].Tag as TrackableItem;
                        if (t != null)
                        {
                            long cnt = (long)_dbcon.ExecuteScalar(string.Format("select count(1) from trackables", t.Code));
                            if (cnt < 2)
                            {
                                //todo delete all data of trackables
                                //todo: delete logs
                            }
                            _dbcon.ExecuteNonQuery(string.Format("delete from trackables where groupid={0} and Code='{1}'", _activeTrackableGroup.ID, t.Code));
                        }
                    }
                }
                catch
                {
                }
                comboBoxGroup_SelectedValueChanged(sender, e);
            }
        }

        private async void updateSelectedTrackablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices != null && listView1.SelectedIndices.Count > 0)
            {
                _tbList.Clear();
                _tbList.AddRange((from int l in listView1.SelectedIndices select (listView1.Items[l].Tag as TrackableItem).Code).ToArray());
                _errormessage = null;
                await Task.Run(() =>
                    {
                        this.GetAllTrackableDataThreadMethod();
                    });
                if (!string.IsNullOrEmpty(_errormessage))
                {
                    System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
                //reload data
                listView1.Items.Clear();
                initImageList();
                comboBoxGroup_SelectedValueChanged(sender, e);
            }
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListView lv = (sender as ListView);
            if (lv != null)
            {
                Utils.ListViewColumnSorter lvcs = (lv.ListViewItemSorter as Utils.ListViewColumnSorter);
                if (lvcs != null)
                {
                    // Determine if clicked column is already the column that is being sorted.
                    if (e.Column == lvcs.SortColumn)
                    {
                        // Reverse the current sort direction for this column.
                        if (lvcs.Order == SortOrder.Ascending)
                        {
                            lvcs.Order = SortOrder.Descending;
                        }
                        else
                        {
                            lvcs.Order = SortOrder.Ascending;
                        }
                    }
                    else
                    {
                        // Set the column number that is to be sorted; default to ascending.
                        lvcs.SortColumn = e.Column;
                        lvcs.Order = SortOrder.Ascending;
                    }

                    // Perform the sort with these new sort options.
                    lv.Sort();
                }
            }
        }

        private async void updateAllTrackablesInGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count>0)
            {
                _tbList.Clear();
                _tbList.AddRange((from ListViewItem l in listView1.Items select (l.Tag as TrackableItem).Code).ToArray());
                _errormessage = null;
                await Task.Run(() =>
                {
                    this.GetAllTrackableDataThreadMethod();
                });
                if (!string.IsNullOrEmpty(_errormessage))
                {
                    System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
                //reload data
                listView1.Items.Clear();
                initImageList();
                comboBoxGroup_SelectedValueChanged(sender, e);
            }
        }


        private void ShowTrackablesOnMap(List<TrackableItem> tbs)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.TrkGroup.trackablesmap.html")))
                {
                    string htmlcontent = textStreamReader.ReadToEnd();
                    StringBuilder sb = new StringBuilder();

                    foreach (var tb in tbs)
                    {
                        StringBuilder bln = new StringBuilder();
                        bln.AppendFormat("<a href=\"http://www.geocaching.com/track/details.aspx?tracker={0}\" target=\"_blank\">{0}</a>", tb.Code);
                        bln.AppendFormat("<br />{0}: {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_NAME), tb.Name??"");
                        bln.AppendFormat("<br />{0}: {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_OWNER), tb.Owner ?? "");
                        bln.AppendFormat("<br />{0}: {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_CREATEDON), tb.DateCreated.ToLongDateString());
                        if (!string.IsNullOrEmpty(tb.CurrentGeocacheCode))
                        {
                            bln.AppendFormat("<br />{0}: <a href=\"http://coord.info/{1}\" target=\"_blank\">{1}</a>", Utils.LanguageSupport.Instance.GetTranslation(STR_INGEOCACHE), tb.CurrentGeocacheCode);
                        }
                        bln.AppendFormat("<br />{0}: {1} km", Utils.LanguageSupport.Instance.GetTranslation(STR_TRAVELLEDDISTANCE), tb.DistanceKm.ToString("0.0"));

                        sb.AppendFormat("createMarker('{0}', new google.maps.LatLng({1}, {2}), {3}, '{4}');", tb.Code, tb.Lat.ToString().Replace(',', '.'), tb.Lon.ToString().Replace(',', '.'), string.IsNullOrEmpty(tb.CurrentGeocacheCode) ? "redIcon" : "blueIcon", bln.ToString().Replace("'",""));
                    }

                    string html = htmlcontent.Replace("//patchwork", sb.ToString());
                    string fn = System.IO.Path.Combine(Core.PluginDataPath, "trackablesmap.html");
                    File.WriteAllText(fn, html);
                    System.Diagnostics.Process.Start(fn);
                }
            }
            catch
            {
            }
        }

        private void ShowRouteOnMap(TrackableItem tb)
        {
            if (tb != null)
            {
                try
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.TrkGroup.trackablesmap.html")))
                    {
                        string htmlcontent = textStreamReader.ReadToEnd();
                        StringBuilder sb = new StringBuilder();
                        List<TravelItem> til = new List<TravelItem>();
                        DbDataReader dr = _dbcon.ExecuteReader(string.Format("select GeocacheCode, lat, lon, DateLogged from travels where TrackableCode='{0}' order by pos", tb.Code));
                        while (dr.Read())
                        {
                            TravelItem ti = new TravelItem();
                            ti.DateLogged = DateTime.Parse((string)dr["DateLogged"]);
                            ti.GeocacheCode = (string)dr["GeocacheCode"];
                            ti.Lat = (double)dr["lat"];
                            ti.Lon = (double)dr["lon"];
                            til.Add(ti);
                        }
                        for (int i=0; i<til.Count; i++)
                        {
                            StringBuilder bln = new StringBuilder();
                            bln.AppendFormat("{0}: {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_STEP), i + 1);
                            bln.AppendFormat("<br />{0}: {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_DATE), til[i].DateLogged.ToLongDateString());
                            bln.AppendFormat("<br />{0}: <a href=\"http://coord.info/{1}\" target=\"_blank\">{1}</a>", Utils.LanguageSupport.Instance.GetTranslation(STR_GEOCACHE), til[i].GeocacheCode);

                            string iconColor;
                            if (i == 0)
                            {
                                iconColor = "yellowIcon";
                            }
                            else if (i == til.Count - 1)
                            {
                                iconColor = "redIcon";
                            }
                            else
                            {
                                iconColor = "blueIcon";
                            }
                            sb.AppendFormat("createMarker('{5}-{0}', new google.maps.LatLng({1}, {2}), {3}, '{4}');", til[i].GeocacheCode, til[i].Lat.ToString().Replace(',', '.'), til[i].Lon.ToString().Replace(',', '.'), iconColor, bln.ToString().Replace("'", ""), i+1);
                        }

                        if (til.Count > 1)
                        {
                            sb.AppendLine();
                            sb.Append("var polylineA = new google.maps.Polyline({map: map, path: [");
                            for (int i = 0; i < til.Count; i++)
                            {
                                if (i > 0)
                                {
                                    sb.Append(",");
                                }
                                sb.AppendFormat("new google.maps.LatLng({0}, {1})", til[i].Lat.ToString().Replace(',', '.'), til[i].Lon.ToString().Replace(',', '.'));
                            }
                            sb.Append("], strokeColor: '#8A2BE2', strokeWeight: 4, strokeOpacity: .9});");
                        }
                        string html = htmlcontent.Replace("//patchwork", sb.ToString());
                        string fn = System.IO.Path.Combine(Core.PluginDataPath, "trackablesmap.html" );
                        File.WriteAllText(fn, html);
                        System.Diagnostics.Process.Start(fn);
                    }
                }
                catch
                {
                }
            }
        }

        private void showAllOnMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count > 0)
            {
                ShowTrackablesOnMap((from ListViewItem l in listView1.Items where (l.Tag as TrackableItem).Lat != null && (l.Tag as TrackableItem).Lon != null select l.Tag as TrackableItem).ToList());
            }
        }

        private void showSelectedOnMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices != null && listView1.SelectedIndices.Count > 0)
            {
                ShowTrackablesOnMap((from int l in listView1.SelectedIndices where (listView1.Items[l].Tag as TrackableItem).Lat != null && (listView1.Items[l].Tag as TrackableItem).Lon != null select listView1.Items[l].Tag as TrackableItem).ToList());
            }
        }

        private void showRouteOfSelectedTrackableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices != null && listView1.SelectedIndices.Count > 0)
            {
                ShowRouteOnMap((from int l in listView1.SelectedIndices where (listView1.Items[l].Tag as TrackableItem).Lat != null && (listView1.Items[l].Tag as TrackableItem).Lon != null select listView1.Items[l].Tag as TrackableItem).FirstOrDefault());
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            showRouteOfSelectedTrackableToolStripMenuItem1.Enabled = showRouteOfSelectedTrackableToolStripMenuItem.Enabled;
            goToTrackablePageToolStripMenuItem.Enabled = showRouteOfSelectedTrackableToolStripMenuItem.Enabled;
            deleteToolStripMenuItem.Enabled = showRouteOfSelectedTrackableToolStripMenuItem.Enabled;
            goToGeocacheToolStripMenuItem.Enabled = showRouteOfSelectedTrackableToolStripMenuItem1.Enabled && (from int l in listView1.SelectedIndices where !string.IsNullOrEmpty((listView1.Items[l].Tag as TrackableItem).CurrentGeocacheCode) select listView1.Items[l].Tag as TrackableItem).FirstOrDefault()!=null;
        }

        private void goToGeocacheToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices != null && listView1.SelectedIndices.Count > 0)
            {
                TrackableItem tb = (from int l in listView1.SelectedIndices where !string.IsNullOrEmpty((listView1.Items[l].Tag as TrackableItem).CurrentGeocacheCode) select listView1.Items[l].Tag as TrackableItem).FirstOrDefault();
                if (tb != null)
                {
                    Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(Core.Geocaches, tb.CurrentGeocacheCode);
                    if (gc != null)
                    {
                        Core.ActiveGeocache = gc;
                    }
                    try
                    {
                        System.Diagnostics.Process.Start(string.Format("http://coord.info/{0}", tb.CurrentGeocacheCode));
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void goToTrackablePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices != null && listView1.SelectedIndices.Count > 0)
            {
                TrackableItem tb = (from int l in listView1.SelectedIndices select listView1.Items[l].Tag as TrackableItem).FirstOrDefault();
                if (tb != null)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(tb.Url);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
