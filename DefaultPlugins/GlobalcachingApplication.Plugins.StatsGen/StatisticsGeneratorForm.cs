using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using CSScriptLibrary;
using System.Threading;
using System.Data.Common;

namespace GlobalcachingApplication.Plugins.StatsGen
{
    public partial class StatisticsGeneratorForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Statistics Generator";
        public const string STR_CREATE = "Create";
        public const string STR_GENERATE = "Generate";
        public const string STR_TEMPLATE = "Template";
        public const string STR_EXTENSION = "Extension";
        public const string STR_STATISTICS = "Statistics";
        public const string STR_SCRIPT = "Script";
        public const string STR_EDIT = "Edit";
        public const string STR_HTML = "Html";
        public const string STR_PREVIEW = "Preview";
        public const string STR_TYPE = "Type";
        public const string STR_NAME = "Name";
        public const string STR_SAVE = "Save";
        public const string STR_DELETE = "Delete";
        public const string STR_SKIN = "Skin";

        private List<ScriptInfo> _scripts = new List<ScriptInfo>();
        private TreeNode _templatesNode;
        private TreeNode _extensionsNode;
        private TreeNode _statsNode;
        private TreeNode _skinsNode;
        private TemporaryFile _scriptFile = null;
        private string _databaseFile;

        public StatisticsGeneratorForm()
        {
            InitializeComponent();
        }

        public StatisticsGeneratorForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CREATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_GENERATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_TEMPLATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_EXTENSION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_STATISTICS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SCRIPT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_EDIT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_HTML));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_PREVIEW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_TYPE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SAVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_DELETE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_SKIN));

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            try
            {
                _databaseFile = System.IO.Path.Combine(new string[] { core.PluginDataPath, "StatsGen.db3" });
            }
            catch
            {
            }

            if (Properties.Settings.Default.WindowPos != null && !Properties.Settings.Default.WindowPos.IsEmpty)
            {
                this.Bounds = Properties.Settings.Default.WindowPos;
                this.StartPosition = FormStartPosition.Manual;
            }

            SelectedLanguageChanged(this, EventArgs.Empty);
        }

        private Utils.DBCon initDatabase()
        {
            Utils.DBCon dbcon = new Utils.DBConComSqlite(_databaseFile);
            object o = dbcon.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='scripts'");
            if (o == null || o.GetType() == typeof(DBNull))
            {
                dbcon.ExecuteNonQuery("create table 'scripts' (id text, name text, scripttype text, content text)");
            }
            o = dbcon.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='settings'");
            if (o == null || o.GetType() == typeof(DBNull))
            {
                dbcon.ExecuteNonQuery("create table 'settings' (scriptid text, enable integer, pos integer)");
            }
            return dbcon;
        }

        private void deleteScriptData(ScriptInfo si)
        {
            try
            {
                using (Utils.DBCon dbcon = initDatabase())
                {
                    dbcon.ExecuteNonQuery(string.Format("delete from scripts where id='{0}'", si.ID));
                    dbcon.ExecuteNonQuery(string.Format("delete from settings where scriptid='{0}'", si.ID));
                }
            }
            catch
            {
            }
        }
        private void saveScriptData(ScriptInfo si)
        {
            try
            {
                using (Utils.DBCon dbcon = initDatabase())
                {
                    if (dbcon.ExecuteNonQuery(string.Format("update scripts set name='{1}', content='{2}' where id='{0}'", si.ID, si.Name.Replace("'", "''"), si.Content.Replace("'", "''"))) == 0)
                    {
                        dbcon.ExecuteNonQuery(string.Format("insert into scripts (id, name, scripttype, content) values ('{0}', '{1}', '{2}', '{3}')", si.ID, si.Name.Replace("'", "''"), si.ScriptType.ToString(), si.Content.Replace("'", "''")));
                    }
                }
            }
            catch
            {
            }
        }
        private void saveSettingsData()
        {
            try
            {
                using (Utils.DBCon dbcon = initDatabase())
                {
                    foreach (ScriptInfo si in _scripts)
                    {
                        if (si.ScriptType == ScriptType.Statistics)
                        {
                            if (dbcon.ExecuteNonQuery(string.Format("update settings set enable={1}, pos={0} where scriptid='{2}'", si.Order, si.Enabled ? 1 : 0, si.ID)) == 0)
                            {
                                dbcon.ExecuteNonQuery(string.Format("insert into settings (scriptid, enable, pos) values ('{2}', {1}, {0})", si.Order, si.Enabled ? 1 : 0, si.ID));
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void addNodes(ScriptType st, TreeNode tn)
        {
            ScriptInfo[] sil = (from t in _scripts where t.ScriptType == st orderby t.Order select t).ToArray();
            foreach (ScriptInfo si in sil)
            {
                TreeNode n = new TreeNode(si.Name);
                n.Tag = si;
                n.Checked = si.Enabled;
                tn.Nodes.Add(n);
            }
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CREATE);
            this.buttonGenerate.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GENERATE);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TEMPLATE);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXTENSION);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_STATISTICS);
            this.button4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SKIN);
            this.tabControl1.TabPages[0].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SCRIPT);
            this.tabControl1.TabPages[1].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EDIT);
            this.tabControl1.TabPages[2].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_HTML);
            this.tabControl1.TabPages[3].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PREVIEW);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TYPE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.buttonSaveScript.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SAVE);
            this.buttonDeleteScript.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELETE);
        }

        private void StatisticsGeneratorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                if (_scriptFile != null)
                {
                    _scriptFile.Dispose();
                    _scriptFile = null;
                }
            }
        }

        private void StatisticsGeneratorForm_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void StatisticsGeneratorForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void GenerateScript()
        {
            string result = "";

            ScriptInfo template = (from t in _scripts where t.ScriptType == ScriptType.Template && t.Enabled select t).FirstOrDefault();
            if (template != null)
            {
                result = template.Content;

                StringBuilder sb = new StringBuilder();
                ScriptInfo[] ext = (from t in _scripts where t.ScriptType == ScriptType.Extension && t.Enabled orderby t.Order select t).ToArray();
                foreach (ScriptInfo si in ext)
                {
                    sb.AppendLine(si.Content);
                    sb.AppendLine();
                    sb.AppendLine();
                }
                result = result.Replace("//[EXTENSIONS]", sb.ToString());

                sb = new StringBuilder();
                ext = (from t in _scripts where t.ScriptType == ScriptType.Skin && t.Enabled orderby t.Order select t).ToArray();
                foreach (ScriptInfo si in ext)
                {
                    sb.AppendLine(si.Content);
                    sb.AppendLine();
                    sb.AppendLine();
                }
                result = result.Replace("//[SKINS]", sb.ToString());

                StringBuilder sbCalls = new StringBuilder();
                StringBuilder sbStats = new StringBuilder();
                ScriptInfo[] stats = (from t in _scripts where t.ScriptType == ScriptType.Statistics && t.Enabled orderby t.Order select t).ToArray();
                int funcIndex = 1;
                foreach (ScriptInfo si in stats)
                {
                    sbCalls.AppendLine(string.Format("sb.Append(statisticsBlock_{0}());", funcIndex));
                    sbStats.AppendLine(string.Format("private string statisticsBlock_{0}()",funcIndex));
                    sbStats.AppendLine("{");
                    sbStats.AppendLine(si.Content);
                    sbStats.AppendLine("}");
                    sbStats.AppendLine();

                    funcIndex++;
                }
                result = result.Replace("//[CALLS]", sbCalls.ToString());
                result = result.Replace("//[STATISTICS]", sbStats.ToString());

            }

            textBoxGeneratedScript.Text = result;
        }

        private void treeView1_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Tag == null)
            {
                e.Cancel = true;
            }
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            ScriptInfo si = e.Node.Tag as  ScriptInfo;
            if (si != null)
            {
                si.Enabled = e.Node.Checked;
                saveSettingsData();
                GenerateScript();
            }
        }

        private void StatisticsGeneratorForm_Shown(object sender, EventArgs e)
        {
            _scriptFile = new TemporaryFile(true);

            _templatesNode = new TreeNode("Templates");
            _templatesNode.Checked = true;
            _extensionsNode = new TreeNode("Extensions");
            _extensionsNode.Checked = true;
            _skinsNode = new TreeNode("Skins");
            _skinsNode.Checked = true;
            _statsNode = new TreeNode("Statistics");
            _statsNode.Checked = true;

            treeView1.Nodes.Add(_templatesNode);
            treeView1.Nodes.Add(_extensionsNode);
            treeView1.Nodes.Add(_skinsNode);
            treeView1.Nodes.Add(_statsNode);

            Assembly assembly = Assembly.GetExecutingAssembly();
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.StatsGen.Scripts.DefaultTemplate.txt")))
            {
                ScriptInfo si = new ScriptInfo();
                si.Content = textStreamReader.ReadToEnd();
                si.ID = "templ_1";
                si.Name = "Default Template";
                si.ReadOnly = true;
                si.Order = 1;
                si.ScriptType = ScriptType.Template;
                si.Enabled = true;
                _scripts.Add(si);
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.StatsGen.Scripts.DefaultExtension.txt")))
            {
                ScriptInfo si = new ScriptInfo();
                si.Content = textStreamReader.ReadToEnd();
                si.ID = "ext_1";
                si.Name = "Default Extension";
                si.ReadOnly = true;
                si.Order = 1;
                si.ScriptType = ScriptType.Extension;
                si.Enabled = true;
                _scripts.Add(si);
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.StatsGen.Scripts.DefaultSkin.txt")))
            {
                ScriptInfo si = new ScriptInfo();
                si.Content = textStreamReader.ReadToEnd();
                si.ID = "skin_1";
                si.Name = "Default Skin";
                si.ReadOnly = true;
                si.Order = 1;
                si.ScriptType = ScriptType.Skin;
                si.Enabled = true;
                _scripts.Add(si);
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.StatsGen.Scripts.GoogleChartAPI.txt")))
            {
                ScriptInfo si = new ScriptInfo();
                si.Content = textStreamReader.ReadToEnd();
                si.ID = "ext_2";
                si.Name = "Google Chart API";
                si.ReadOnly = true;
                si.Order = 1;
                si.ScriptType = ScriptType.Extension;
                si.Enabled = true;
                _scripts.Add(si);
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.StatsGen.Scripts.foundCaches.txt")))
            {
                ScriptInfo si = new ScriptInfo();
                si.Content = textStreamReader.ReadToEnd();
                si.ID = "FoundCaches";
                si.Name = "Found caches";
                si.ReadOnly = true;
                si.Order = 1;
                si.ScriptType = ScriptType.Statistics;
                si.Enabled = true;
                _scripts.Add(si);
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.StatsGen.Scripts.DaysCached.txt")))
            {
                ScriptInfo si = new ScriptInfo();
                si.Content = textStreamReader.ReadToEnd();
                si.ID = "DaysCached";
                si.Name = "Days cached";
                si.ReadOnly = true;
                si.Order = 1;
                si.ScriptType = ScriptType.Statistics;
                si.Enabled = true;
                _scripts.Add(si);
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.StatsGen.Scripts.History.txt")))
            {
                ScriptInfo si = new ScriptInfo();
                si.Content = textStreamReader.ReadToEnd();
                si.ID = "History";
                si.Name = "History";
                si.ReadOnly = true;
                si.Order = 1;
                si.ScriptType = ScriptType.Statistics;
                si.Enabled = true;
                _scripts.Add(si);
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.StatsGen.Scripts.DTMatrix.txt")))
            {
                ScriptInfo si = new ScriptInfo();
                si.Content = textStreamReader.ReadToEnd();
                si.ID = "DTMatrix";
                si.Name = "DT Matrix";
                si.ReadOnly = true;
                si.Order = 1;
                si.ScriptType = ScriptType.Statistics;
                si.Enabled = true;
                _scripts.Add(si);
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.StatsGen.Scripts.DiffTerr.txt")))
            {
                ScriptInfo si = new ScriptInfo();
                si.Content = textStreamReader.ReadToEnd();
                si.ID = "DiffTerr";
                si.Name = "Difficulty and terrain";
                si.ReadOnly = true;
                si.Order = 1;
                si.ScriptType = ScriptType.Statistics;
                si.Enabled = true;
                _scripts.Add(si);
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.StatsGen.Scripts.Milestones.txt")))
            {
                ScriptInfo si = new ScriptInfo();
                si.Content = textStreamReader.ReadToEnd();
                si.ID = "Milestones";
                si.Name = "Milestones";
                si.ReadOnly = true;
                si.Order = 1;
                si.ScriptType = ScriptType.Statistics;
                si.Enabled = true;
                _scripts.Add(si);
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.StatsGen.Scripts.CacheSizeAndType.txt")))
            {
                ScriptInfo si = new ScriptInfo();
                si.Content = textStreamReader.ReadToEnd();
                si.ID = "CacheST";
                si.Name = "Cache size and type";
                si.ReadOnly = true;
                si.Order = 2;
                si.ScriptType = ScriptType.Statistics;
                si.Enabled = true;
                _scripts.Add(si);
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.StatsGen.Scripts.LocationsTable.txt")))
            {
                ScriptInfo si = new ScriptInfo();
                si.Content = textStreamReader.ReadToEnd();
                si.ID = "LocationsTable";
                si.Name = "Locations table";
                si.ReadOnly = true;
                si.Order = 3;
                si.ScriptType = ScriptType.Statistics;
                si.Enabled = true;
                _scripts.Add(si);
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.StatsGen.Scripts.LogLengthTable.txt")))
            {
                ScriptInfo si = new ScriptInfo();
                si.Content = textStreamReader.ReadToEnd();
                si.ID = "LogLengthTable";
                si.Name = "Log length table";
                si.ReadOnly = true;
                si.Order = 3;
                si.ScriptType = ScriptType.Statistics;
                si.Enabled = false;
                _scripts.Add(si);
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.StatsGen.Scripts.myGeoToolsBadges.txt")))
            {
                ScriptInfo si = new ScriptInfo();
                si.Content = textStreamReader.ReadToEnd();
                si.ID = "myGeoToolsBadges";
                si.Name = "myGeoTools Badges";
                si.ReadOnly = true;
                si.Order = 10;
                si.ScriptType = ScriptType.Statistics;
                si.Enabled = false;
                _scripts.Add(si);
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.StatsGen.Scripts.WorldMap66.txt")))
            {
                ScriptInfo si = new ScriptInfo();
                si.Content = textStreamReader.ReadToEnd();
                si.ID = "WorldMap66";
                si.Name = "World map";
                si.ReadOnly = true;
                si.Order = 4;
                si.ScriptType = ScriptType.Statistics;
                si.Enabled = true;
                _scripts.Add(si);
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.StatsGen.Scripts.EuropeMap66.txt")))
            {
                ScriptInfo si = new ScriptInfo();
                si.Content = textStreamReader.ReadToEnd();
                si.ID = "EuropeMap66";
                si.Name = "Europe map";
                si.ReadOnly = true;
                si.Order = 5;
                si.ScriptType = ScriptType.Statistics;
                si.Enabled = true;
                _scripts.Add(si);
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.StatsGen.Scripts.TotalFoundsPerMonthGraph.txt")))
            {
                ScriptInfo si = new ScriptInfo();
                si.Content = textStreamReader.ReadToEnd();
                si.ID = "TotalFoundsPerMonthGraph";
                si.Name = "Total founds per month graph";
                si.ReadOnly = true;
                si.Order = 50;
                si.ScriptType = ScriptType.Statistics;
                si.Enabled = true;
                _scripts.Add(si);
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.StatsGen.Scripts.DiffTerrPie.txt")))
            {
                ScriptInfo si = new ScriptInfo();
                si.Content = textStreamReader.ReadToEnd();
                si.ID = "DiffTerrPie";
                si.Name = "Difficulty and terrain pie";
                si.ReadOnly = true;
                si.Order = 51;
                si.ScriptType = ScriptType.Statistics;
                si.Enabled = true;
                _scripts.Add(si);
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.StatsGen.Scripts.CacheTypeRatioGraph.txt")))
            {
                ScriptInfo si = new ScriptInfo();
                si.Content = textStreamReader.ReadToEnd();
                si.ID = "CacheTypeRatioGraph";
                si.Name = "Cache type ratio graph";
                si.ReadOnly = true;
                si.Order = 52;
                si.ScriptType = ScriptType.Statistics;
                si.Enabled = true;
                _scripts.Add(si);
            }
            using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("GlobalcachingApplication.Plugins.StatsGen.Scripts.CacheTravelDistanceGraph.txt")))
            {
                ScriptInfo si = new ScriptInfo();
                si.Content = textStreamReader.ReadToEnd();
                si.ID = "CacheTravelDistanceGraph";
                si.Name = "Cache travel distance graph";
                si.ReadOnly = true;
                si.Order = 53;
                si.ScriptType = ScriptType.Statistics;
                si.Enabled = true;
                _scripts.Add(si);
            }

            try
            {
                using (Utils.DBCon dbcon = initDatabase())
                {
                    DbDataReader dr = dbcon.ExecuteReader("select * from scripts");
                    while (dr.Read())
                    {
                        ScriptInfo si = new ScriptInfo();
                        si.Content = dr["content"] as string;
                        si.ID = dr["id"] as string;
                        si.Name = dr["name"] as string;
                        si.ReadOnly = false;
                        si.ScriptType = (ScriptType)Enum.Parse(typeof(ScriptType), dr["scripttype"] as string);
                        _scripts.Add(si);
                    }
                    dr = dbcon.ExecuteReader("select * from settings");
                    while (dr.Read())
                    {
                        ScriptInfo si = (from s in _scripts where s.ID == (string)dr["scriptid"] select s).FirstOrDefault();
                        if (si != null)
                        {
                            si.Order = (int)dr["pos"];
                            si.Enabled = (int)dr["enable"] != 0;
                        }
                    }
                }
            }
            catch
            {
            }

            addNodes(ScriptType.Template, _templatesNode);
            addNodes(ScriptType.Extension, _extensionsNode);
            addNodes(ScriptType.Skin, _skinsNode);
            addNodes(ScriptType.Statistics, _statsNode);

            treeView1.ExpandAll();

            GenerateScript();
        }

        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                GenerateScript();
                //todo: background!
                File.WriteAllText(_scriptFile.Path, textBoxGeneratedScript.Text);

                AsmHelper scriptAsm = new AsmHelper(CSScript.Load(_scriptFile.Path));
                string result = (string)scriptAsm.Invoke("StatisticsTemplate.Run", new object[] { this.OwnerPlugin, Core });

                textBoxHtml.Text = result;
                DisplayHtml(string.Format("<html><head></head><body>{0}</body></html>", result));
                tabControl1.SelectedIndex = 3;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error");
            }
            this.Cursor = Cursors.Default;
        }

        private void DisplayHtml(string html)
        {
            webBrowser1.Navigate("about:blank");
            if (webBrowser1.Document != null)
            {
                webBrowser1.Document.Write(string.Empty);
            }
            webBrowser1.DocumentText = html;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ScriptInfo si = e.Node.Tag as ScriptInfo;
            if (si != null)
            {
                labelScriptType.Text = si.ScriptType.ToString();
                textBoxScriptName.Text = si.Name;
                textBoxScript.Text = si.Content;

                textBoxScriptName.ReadOnly = si.ReadOnly;
                textBoxScript.ReadOnly = si.ReadOnly;

                buttonDeleteScript.Enabled = !si.ReadOnly;
                buttonSaveScript.Enabled = !si.ReadOnly;

                tabControl1.SelectedIndex = 1;
            }
            else
            {
                labelScriptType.Text = "-";
                textBoxScriptName.Text = "";
                textBoxScript.Text = "";

                buttonDeleteScript.Enabled = false;
                buttonSaveScript.Enabled = false;
            }
        }

        private void treeView1_DragOver(object sender, DragEventArgs e)
        {
            Point pt = treeView1.PointToClient(new Point(e.X, e.Y));
            TreeNode dn = treeView1.GetNodeAt(pt);

            e.Effect = DragDropEffects.None;
            if (dn == _statsNode)
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                TreeNode sn = e.Data.GetData(typeof(TreeNode)) as TreeNode;
                if (sn != null)
                {
                    ScriptInfo si = sn.Tag as ScriptInfo;
                    if (si != null)
                    {
                        ScriptInfo di = dn.Tag as ScriptInfo;
                        if (di != null && di.ScriptType == si.ScriptType && di != si)
                        {
                            e.Effect = DragDropEffects.Move;
                        }
                    }
                }
            }
        }

        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            TreeNode tn = e.Item as TreeNode;
            if (tn != null)
            {
                ScriptInfo si = tn.Tag as ScriptInfo;
                if (si != null && si.ScriptType== ScriptType.Statistics && tn.Parent.Nodes.Count>1)
                {
                    DoDragDrop(e.Item, DragDropEffects.Move);
                }
            }
        }

        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode sn = e.Data.GetData(typeof(TreeNode)) as TreeNode;
            if (sn != null)
            {
                ScriptInfo si = sn.Tag as ScriptInfo;
                if (si != null)
                {
                    Point pt = treeView1.PointToClient(new Point(e.X, e.Y));
                    TreeNode dn = treeView1.GetNodeAt(pt);
                    TreeNode parent = sn.Parent;

                    if (dn == _statsNode)
                    {
                        //set as first
                        if (sn.Parent.FirstNode == sn)
                        {
                            //nah
                        }
                        else
                        {
                            parent.Nodes.Remove(sn);
                            parent.Nodes.Insert(0, sn);
                            reorderStatistics();
                        }
                    }
                    else if (parent.LastNode == dn)
                    {
                        //set as last
                        parent.Nodes.Remove(sn);
                        parent.Nodes.Add(sn);
                        reorderStatistics();
                    }
                    else
                    {
                        ScriptInfo di = dn.Tag as ScriptInfo;
                        if (di != null)
                        {
                            //moving up -> insert before item
                            //moving down -> inserting after item
                            if (si.Order < di.Order)
                            {
                                //moving down
                                parent.Nodes.Remove(sn);
                                parent.Nodes.Insert(parent.Nodes.IndexOf(dn) + 1, sn);
                                reorderStatistics();
                            }
                            else
                            {
                                //moving up
                                parent.Nodes.Remove(sn);
                                parent.Nodes.Insert(parent.Nodes.IndexOf(dn), sn);
                                reorderStatistics();
                            }
                        }
                    }
                }
            }
        }

        private void reorderStatistics()
        {
            for (int i = 0; i < _statsNode.Nodes.Count; i++)
            {
                ScriptInfo di = _statsNode.Nodes[i].Tag as ScriptInfo;
                di.Order = i + 1;
            }
            saveSettingsData();
            GenerateScript();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ScriptInfo si = new ScriptInfo();
            si.ID = Guid.NewGuid().ToString("N");
            si.Name = "Custom script";
            si.ReadOnly = false;
            si.ScriptType = ScriptType.Statistics;
            si.Content = "StringBuilder sb = new StringBuilder();\r\n\r\nreturn sb.ToString();";
            si.Enabled = false;
            _scripts.Add(si);
            saveScriptData(si);

            TreeNode tn = new TreeNode(si.Name);
            tn.Tag = si;
            _statsNode.Nodes.Add(tn);
            reorderStatistics();

            treeView1.SelectedNode = tn;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ScriptInfo si = new ScriptInfo();
            si.ID = Guid.NewGuid().ToString("N");
            si.Name = "Custom extension";
            si.ReadOnly = false;
            si.ScriptType = ScriptType.Extension;
            si.Content = "";
            si.Enabled = false;
            _scripts.Add(si);
            saveScriptData(si);

            TreeNode tn = new TreeNode(si.Name);
            tn.Tag = si;
            _extensionsNode.Nodes.Add(tn);

            treeView1.SelectedNode = tn;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ScriptInfo si = new ScriptInfo();
            si.ID = Guid.NewGuid().ToString("N");
            si.Name = "Custom template";
            si.ReadOnly = false;
            si.ScriptType = ScriptType.Template;
            si.Content = "";
            si.Enabled = false;
            _scripts.Add(si);
            saveScriptData(si);

            TreeNode tn = new TreeNode(si.Name);
            tn.Tag = si;
            _templatesNode.Nodes.Add(tn);

            treeView1.SelectedNode = tn;
        }

        private void buttonSaveScript_Click(object sender, EventArgs e)
        {
            TreeNode tn = treeView1.SelectedNode;
            if (tn != null)
            {
                ScriptInfo si = tn.Tag as ScriptInfo;
                if (si != null)
                {
                    si.Name = textBoxScriptName.Text;
                    si.Content = textBoxScript.Text;
                    tn.Text = si.Name;

                    saveScriptData(si);
                }
            }
        }

        private void buttonDeleteScript_Click(object sender, EventArgs e)
        {
            TreeNode tn = treeView1.SelectedNode;
            if (tn != null)
            {
                ScriptInfo si = tn.Tag as ScriptInfo;
                if (si != null)
                {
                    tn.Parent.Nodes.Remove(tn);
                    _scripts.Remove(si);
                    deleteScriptData(si);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ScriptInfo si = new ScriptInfo();
            si.ID = Guid.NewGuid().ToString("N");
            si.Name = "Custom skin";
            si.ReadOnly = false;
            si.ScriptType = ScriptType.Skin;
            si.Content = "";
            si.Enabled = false;
            _scripts.Add(si);
            saveScriptData(si);

            TreeNode tn = new TreeNode(si.Name);
            tn.Tag = si;
            _skinsNode.Nodes.Add(tn);

            treeView1.SelectedNode = tn;
        }
    }
}
