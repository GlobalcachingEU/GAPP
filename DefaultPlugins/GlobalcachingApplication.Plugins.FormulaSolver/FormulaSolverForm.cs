using System;
using System.Windows.Forms;
using GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Data.Common;

namespace GlobalcachingApplication.Plugins.FormulaSolver
{
    public partial class FormulaSolverForm : Utils.BasePlugin.BaseUIChildWindowForm, INotifyPropertyChanged
    {
        private FormulaSolver formulaSolver;
        private FormulaInterpreter.FormulaInterpreter interpreter;
        private string _databaseFile = null;
        private Utils.DBCon _dbcon = null;

        private string _activeCode = null;
        public string activeCode
        {
            get
            {
                return _activeCode;
            }
            set
            {
                if (_activeCode != value)
                {
                    _activeCode = value;
                    OnPropertyChanged("activeCode");
                    OnPropertyChanged("activeTitle");
                }
            }
        }

        private string _activeName = null;
        public string activeName
        {
            get
            {
                return _activeName;
            }
            set
            {
                if (_activeName != value)
                {
                    _activeName = value;
                    OnPropertyChanged("activeName");
                    OnPropertyChanged("activeTitle");
                }
            }
        }

        public string activeTitle
        {
            get
            {
                return ((activeCode != null) && (activeCode.Length > 0))
                    ? string.Format("{0}: {1}", activeCode, activeName)
                    : StrRes.GetString(StrRes.STR_NO_CACHE_SELECTED);
            }
            private set {}
        }

        public FormulaSolverForm()
        {
            InitializeComponent();
            UpdateControlsLanguage();
        }

        public FormulaSolverForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
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

            _databaseFile = System.IO.Path.Combine(core.PluginDataPath, "formulaSolver.db3");
            core.ActiveGeocacheChanged += new Framework.EventArguments.GeocacheEventHandler(core_ActiveGeocacheChanged);
            
            formulaSolverFormBindingSource.Add(this);

            InitGrammar();
            UpdateView();
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            base.SelectedLanguageChanged(sender, e);
            UpdateControlsLanguage();
        }

        private void UpdateControlsLanguage()
        {
            bnInsertFormula.Text = StrRes.GetString(StrRes.STR_INSERT_FORMULA);
            bnInsertWaypoint.Text = StrRes.GetString(StrRes.STR_INSERT_WAYPOINT);
            bnSolve.Text = StrRes.GetString(StrRes.STR_SOLVE);
            bnAsWaypoint.Text = StrRes.GetString(StrRes.STR_AS_WAYPOINT);
            bnAsCenter.Text = StrRes.GetString(StrRes.STR_AS_CENTER);
            OnPropertyChanged("activeTitle");
        }

        void core_ActiveGeocacheChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (this.Visible)
            {
                StoreFormula();
                UpdateView();
            }
        }

        public void UpdateView()
        {
            tbFormula.Text = "";
            tbSolutions.Text = "";
            if (Core.ActiveGeocache != null)
            {
                activeCode = Core.ActiveGeocache.Code;
                activeName = Core.ActiveGeocache.Name;
            }
            else
            {
                activeCode = null;
                activeName = null;
            }
            LoadFormula();
        }

        private void LoadFormula()
        {
            if (initDatabase() != null)
            {
                string cacheName = (activeCode != null) ? activeCode : "NOTES";
                try
                {
                    string cmd = string.Format("SELECT formula FROM formulas WHERE code='{0}'", cacheName.Replace("'", "''"));
                    using (DbDataReader dr = _dbcon.ExecuteReader(cmd))
                    {
                        if (dr.Read())
                        {
                            UTF8Encoding enc = new UTF8Encoding();
                            string encoded = dr["formula"] as string;
                            byte[] decoded = Convert.FromBase64String(encoded);
                            string formula = enc.GetString(decoded);
                            tbFormula.Text = formula;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private Utils.DBCon initDatabase()
        {
            if (_dbcon == null)
            {
                try
                {
                    _dbcon = new Utils.DBConComSqlite(_databaseFile);
                    object o = _dbcon.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='formulas'");
                    if (o == null || o.GetType() == typeof(DBNull))
                    {
                        _dbcon.ExecuteNonQuery("create table 'formulas' (code text, formula text)");
                        _dbcon.ExecuteNonQuery("create unique index idx_formulas on formulas (code)");
                    }
                }
                catch
                {
                    _dbcon = null;
                }
            }
            return _dbcon;
        }

        private void StoreFormula()
        {
            if (initDatabase() != null)
            {
                string cacheName = (activeCode != null) ? activeCode : "NOTES";
                UTF8Encoding enc = new UTF8Encoding();
                try
                {
                    int count = Convert.ToInt32(
                        _dbcon.ExecuteScalar(
                            string.Format("SELECT COUNT() FROM formulas WHERE code='{0}'", cacheName)).ToString());

                    byte[] decoded = enc.GetBytes(tbFormula.Text);
                    string encoded = Convert.ToBase64String(decoded);
                    string fmt = "DELETE FROM formulas WHERE code='{0}'";
                    if (tbFormula.Text.Length > 0) {
                        fmt = (count > 0)
                            ? "UPDATE formulas SET formula='{1}' WHERE code='{0}'"
                            : "INSERT INTO formulas (code, formula) VALUES('{0}', '{1}')";
                    }
                    string cmd = string.Format(fmt, cacheName, encoded);
                    _dbcon.ExecuteNonQuery(cmd);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        
        private void InitGrammar()
        {
            using (var strm = Assembly.GetExecutingAssembly().GetManifestResourceStream("GlobalcachingApplication.Plugins.FormulaSolver.Grammar.SingleLineFormula.egt"))
            using (System.IO.BinaryReader br = new System.IO.BinaryReader(strm))
            {
                interpreter = new FormulaInterpreter.FormulaInterpreter(br);
            }
        }

        private void FormulaSolverForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StoreFormula();
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void FormulaSolverForm_LocationOrSizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void bnWaypoint_Click(object sender, EventArgs e)
        {
            WaypointSelectorForm wps = new WaypointSelectorForm(Core);
            if (wps.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                string wp = "\"" + wps.WaypointName + "\"";
                UpdateFormulaText(wp, 0);
            }
        }

        private void bnInsertFormula_Click(object sender, EventArgs e)
        {
            InsertFormulaForm frm = new InsertFormulaForm();
            if (frm.ShowDialog(this) == DialogResult.OK)
            {
                string frmName = frm.SelectedFunction;
                UpdateFormulaText(frmName, -1);
            }
        }

        private void UpdateFormulaText(string txt, int moveCursorTo)
        {
            tbFormula.Focus();
            var selectionIndex = tbFormula.SelectionStart;
            tbFormula.Text = tbFormula.Text.Insert(selectionIndex, txt);
            tbFormula.SelectionStart = selectionIndex + Math.Max(0, txt.Length + moveCursorTo);
        }

        private void bnSolve_Click(object sender, EventArgs e)
        {
            StoreFormula();
            String formula = tbFormula.Text;
            tbSolutions.Text = "";

            if (formula.Length > 0)
            {
                string[] lines = formula.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                StringBuilder sb = new StringBuilder();
                ExecutionContext ctx = new ExecutionContext(Core);

                foreach (string line in lines)
                {
                    if (line.Length == 0)
                    {
                        sb.AppendLine("");
                    }
                    else
                    {
                        string res = Convert.ToString(interpreter.Exec(line, ctx), new System.Globalization.CultureInfo(""));
                        sb.AppendLine(res);
                    }
                }

                if (ctx.HasMissingVariables())
                {
                    string text = string.Format(StrRes.GetString(StrRes.STR_MISSING_VARIABLES), String.Join(", ", ctx.GetMissingVariableNames()));
                    if (MessageBox.Show(text, "Formula Solver", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        StringBuilder sbInput = new StringBuilder();
                        foreach (string name in ctx.GetMissingVariableNames())
                        {
                            sbInput.AppendLine(name + "=");
                            }
                        foreach (string line in lines)
                        {
                            sbInput.AppendLine(line);
                    }

                        tbFormula.Text = sbInput.ToString();
                }
                    sb.Clear();
                }
                tbSolutions.Text = sb.ToString();
            }
        }

        private void bnAsWaypoint_Click(object sender, EventArgs e)
        {

        }

        private void bnAsCenter_Click(object sender, EventArgs e)
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged( this, new PropertyChangedEventArgs(propertyName) );
            }
        }

        private void FormulaSolverForm_Load(object sender, EventArgs e)
        {

        }
    }
}
