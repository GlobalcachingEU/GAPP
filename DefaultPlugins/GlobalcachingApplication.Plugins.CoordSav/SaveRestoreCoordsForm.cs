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

namespace GlobalcachingApplication.Plugins.CoordSav
{
    public partial class SaveRestoreCoordsForm : Form
    {
        public const string STR_TITLE = "Backup/Restore Coordinates";
        public const string STR_INFO = "With the Live API, the coordinates of the geocache are not provided. This means that importing an archived geocache using the Live API will result in an invalid coordinate. Here you can backup all coordinates and restore them.";
        public const string STR_MISSINGCOORD = "Missing coordinates";
        public const string STR_CODE = "Code";
        public const string STR_NAME = "Name";
        public const string STR_RESTORE = "restore all missing";
        public const string STR_BACKUP = "Backup all coordinates";
        public const string STR_SAVE = "Save";
        public const string STR_COORDINATE = "Coordinate";

        private Framework.Data.Geocache _gc = null;
        private Framework.Interfaces.ICore _core = null;
        private SynchronizationContext _context = null;
        private ManualResetEvent _actionReady;

        public SaveRestoreCoordsForm(): this(null)
        {
        }

        public SaveRestoreCoordsForm(Framework.Interfaces.ICore core)
        {
            InitializeComponent();

            _core = core;
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_INFO);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_MISSINGCOORD);
            listView1.Columns[0].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CODE);
            listView1.Columns[1].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RESTORE);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_BACKUP);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SAVE);
            this.label3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COORDINATE);

            updateList();

            _context = SynchronizationContext.Current;
            if (_context == null)
            {
                _context = new SynchronizationContext();
            }
        }


        private void updateList()
        {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            var gcList = (from Framework.Data.Geocache g in _core.Geocaches where Math.Abs(g.Lat) < 0.0001 && Math.Abs(g.Lon) < 0.0001 select g);
            foreach (var gc in gcList)
            {
                ListViewItem lvi = new ListViewItem(new string[] { gc.Code, gc.Name });
                lvi.Tag = gc;
                listView1.Items.Add(lvi);
            }
            listView1.EndUpdate();
            listView1_SelectedIndexChanged(this, EventArgs.Empty);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            linkLabel1.Links.Clear();
            if (listView1.SelectedItems.Count == 0)
            {
                linkLabel1.Text = "";
                pictureBox1.Image = null;
                textBox1.Text = "";
                textBox1.Enabled = false;
            }
            else
            {
                _gc = listView1.SelectedItems[0].Tag as Framework.Data.Geocache;
                pictureBox1.ImageLocation = Utils.ImageSupport.Instance.GetImagePath(_core, Framework.Data.ImageSize.Default, _gc.GeocacheType);
                linkLabel1.Text = string.Format("{0}, {1}", _gc.Code, _gc.Name);
                linkLabel1.Links.Add(0, _gc.Code.Length, _gc.Url);
                textBox1.Text = Utils.Conversion.GetCoordinatesPresentation(_gc.Lat, _gc.Lon);
                textBox1.Enabled = true;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            var l = Utils.Conversion.StringToLocation(textBox1.Text);
            button1.Enabled = (l != null && (Math.Abs(l.Lat) > 0.0001 || Math.Abs(l.Lon) > 0.0001));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                using (Utils.DBCon dbcon = initDatabase())
                {
                    Framework.Data.Location l = Utils.Conversion.StringToLocation(textBox1.Text);
                    if (l != null && listView1.SelectedItems.Count > 0)
                    {
                        Framework.Data.Geocache gc = listView1.SelectedItems[0].Tag as Framework.Data.Geocache;
                        if (dbcon.ExecuteNonQuery(string.Format("update coord set lat={0}, lon={1} where code='{2}'", l.SLat, l.SLon, gc.Code.Replace("'", "''"))) == 0)
                        {
                            dbcon.ExecuteNonQuery(string.Format("insert into coord (lat, lon, code) values ({0}, {1}, '{2}')", l.SLat, l.SLon, gc.Code.Replace("'", "''")));
                        }
                        gc.BeginUpdate();
                        gc.Lat = l.Lat;
                        gc.Lon = l.Lon;
                        gc.EndUpdate();
                        listView1.Items.Remove(listView1.SelectedItems[0]);
                    }
                }
            }
            catch
            {
            }
            this.Cursor = Cursors.Default;
        }

        private Utils.DBCon initDatabase()
        {
            Utils.DBCon result = null;
            try
            {
                string fn = System.IO.Path.Combine(_core.PluginDataPath, "coordsav.db3" );
                if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(fn)))
                {
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fn));
                }
                result = new Utils.DBConComSqlite(fn);
                object o = result.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='coord'");
                if (o == null || o.GetType() == typeof(DBNull))
                {
                    result.ExecuteNonQuery("create table 'coord' (code text, lat real, lon real)");
                    result.ExecuteNonQuery("create unique index idx_coord on coord (code)");
                }
            }
            catch
            {
            }
            return result;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.ControlBox = false;
            panel1.Enabled = false;
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Maximum = _core.Geocaches.Count;
            toolStripProgressBar1.Visible = true;
            _actionReady = new ManualResetEvent(false);
            Thread thrd = new Thread(new ThreadStart(this.backupThreadMethod));
            thrd.Start();
            while (!_actionReady.WaitOne(100))
            {
                System.Windows.Forms.Application.DoEvents();
            }
            thrd.Join();
            toolStripProgressBar1.Visible = false;
            panel1.Enabled = true;
            this.ControlBox = true;
        }

        private void backupThreadMethod()
        {
            try
            {
                using (Utils.DBCon dbcon = initDatabase())
                {
                    int index = 0;
                    foreach (Framework.Data.Geocache gc in _core.Geocaches)
                    {
                        if (Math.Abs(gc.Lat) > 0.0001 || Math.Abs(gc.Lon) > 0.0001)
                        {
                            if (dbcon.ExecuteNonQuery(string.Format("update coord set lat={0}, lon={1} where code='{2}'", gc.Lat.ToString().Replace(',', '.'), gc.Lon.ToString().Replace(',', '.'), gc.Code.Replace("'", "''"))) == 0)
                            {
                                dbcon.ExecuteNonQuery(string.Format("insert into coord (lat, lon, code) values ({0}, {1}, '{2}')", gc.Lat.ToString().Replace(',', '.'), gc.Lon.ToString().Replace(',', '.'), gc.Code.Replace("'", "''")));
                            }
                            index++;
                            if (index % 1000 == 0)
                            {
                                _context.Send(new SendOrPostCallback(delegate(object state)
                                {
                                    toolStripProgressBar1.Value = index;
                                }), null);
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

        private void button2_Click(object sender, EventArgs e)
        {
            _core.Geocaches.BeginUpdate();
            this.ControlBox = false;
            panel1.Enabled = false;
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Maximum = listView1.Items.Count;
            toolStripProgressBar1.Visible = true;
            _actionReady = new ManualResetEvent(false);
            Thread thrd = new Thread(new ThreadStart(this.restoreThreadMethod));
            thrd.Start();
            while (!_actionReady.WaitOne(100))
            {
                System.Windows.Forms.Application.DoEvents();
            }
            thrd.Join();
            toolStripProgressBar1.Visible = false;
            panel1.Enabled = true;
            this.ControlBox = true;
            _core.Geocaches.EndUpdate();
            updateList();
        }

        private void restoreThreadMethod()
        {
            try
            {
                var gcList = (from Framework.Data.Geocache g in _core.Geocaches where Math.Abs(g.Lat) < 0.0001 && Math.Abs(g.Lon) < 0.0001 select g).ToArray();
                _context.Send(new SendOrPostCallback(delegate(object state)
                {
                    toolStripProgressBar1.Maximum = gcList.Length;
                }), null);

                using (Utils.DBCon dbcon = initDatabase())
                {
                    int index = 0;
                    foreach (Framework.Data.Geocache gc in gcList)
                    {
                        DbDataReader dr = dbcon.ExecuteReader(string.Format("select lat, lon from coord where code='{0}'", gc.Code.Replace("'", "''")));
                        if (dr.Read())
                        {
                            gc.Lat = (double)dr["lat"];
                            gc.Lon = (double)dr["lon"];
                        }
                        index++;
                        if (index % 100 == 0)
                        {
                            _context.Send(new SendOrPostCallback(delegate(object state)
                            {
                                toolStripProgressBar1.Value = index;
                            }), null);
                        }
                    }
                }
            }
            catch
            {
            }
            _actionReady.Set();
        }
    }

    public class CoordSaveRestore : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SHOW = "Backup/Restore Coordinates";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SaveRestoreCoordsForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SaveRestoreCoordsForm.STR_BACKUP));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SaveRestoreCoordsForm.STR_CODE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SaveRestoreCoordsForm.STR_COORDINATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SaveRestoreCoordsForm.STR_INFO));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SaveRestoreCoordsForm.STR_MISSINGCOORD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SaveRestoreCoordsForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SaveRestoreCoordsForm.STR_RESTORE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SaveRestoreCoordsForm.STR_SAVE));

            return base.Initialize(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Action;
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result && ACTION_SHOW==action)
            {
                using (SaveRestoreCoordsForm dlg = new SaveRestoreCoordsForm(Core))
                {
                    dlg.ShowDialog();
                }
            }
            return result;
        }
    }
}
