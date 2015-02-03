using System;
using System.Windows.Forms;
using GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Data.Common;
using GlobalcachingApplication.Framework.Interfaces;

namespace GlobalcachingApplication.Plugins.FormulaSolver
{
    public partial class FormulaSolverForm : Utils.BasePlugin.BaseUIChildWindowForm, INotifyPropertyChanged, ITempDirProvider
    {
        private FormulaInterpreter.FormulaInterpreter interpreter;

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

        public string GetPluginTempDirectory()
        {
            string dir = System.IO.Path.Combine(Core.PluginDataPath, "FormulaSolver");
            System.IO.Directory.CreateDirectory(dir);
            return dir;
        }

        public class FormulaPoco
        {
            public string formula { get; set; }
            public string code { get; set; }
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

            var p = PluginSettings.Instance.WindowPos;
            if (p != null && !p.IsEmpty)
            {
                this.Bounds = p;
                this.StartPosition = FormStartPosition.Manual;
            }

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
            string cacheName = (activeCode != null) ? activeCode : "NOTES";
            try
            {
                lock (Core.SettingsProvider)
                {
                    FormulaPoco poco = Core.SettingsProvider.Database.FirstOrDefault<FormulaPoco>(string.Format("select * from {0} where code=@0", Core.SettingsProvider.GetFullTableName("formulas")), cacheName);
                    if (poco!=null)
                    {
                        UTF8Encoding enc = new UTF8Encoding();
                        byte[] decoded = Convert.FromBase64String(poco.formula);
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

        public static void InitDatabase(ICore core)
        {
            lock (core.SettingsProvider)
            {
                if (!core.SettingsProvider.TableExists(core.SettingsProvider.GetFullTableName("formulas")))
                {
                    core.SettingsProvider.Database.Execute(string.Format("create table '{0}' (code text, formula text)", core.SettingsProvider.GetFullTableName("formulas")));
                }
            }
        }

        private void StoreFormula()
        {
            string cacheName = (activeCode != null) ? activeCode : "NOTES";
            UTF8Encoding enc = new UTF8Encoding();
            try
            {
                lock (Core.SettingsProvider)
                {
                    FormulaPoco poco = Core.SettingsProvider.Database.FirstOrDefault<FormulaPoco>(string.Format("select * from {0} where code=@0", Core.SettingsProvider.GetFullTableName("formulas")), cacheName);

                    byte[] decoded = enc.GetBytes(tbFormula.Text);
                    string encoded = Convert.ToBase64String(decoded);
                    string fmt = "DELETE FROM {2} WHERE code='{0}'";
                    if (tbFormula.Text.Length > 0)
                    {
                        fmt = (poco != null)
                            ? "UPDATE {2} SET formula='{1}' WHERE code='{0}'"
                            : "INSERT INTO {2} (code, formula) VALUES('{0}', '{1}')";
                    }
                    string cmd = string.Format(fmt, cacheName, encoded, Core.SettingsProvider.GetFullTableName("formulas"));
                    Core.SettingsProvider.Database.Execute(cmd);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            if (WindowState == FormWindowState.Normal && this.Visible)
            {
                PluginSettings.Instance.WindowPos = this.Bounds;
            }
        }

        private void bnWaypoint_Click(object sender, EventArgs e)
        {
            WaypointSelectorForm wps = new WaypointSelectorForm(Core);
            if (wps.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                string wp = "WP(\"" + wps.WaypointName + "\")";
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
            Framework.Data.Location ll = null;
            if (tbSolutions.SelectionLength > 0)
            {
                ll = Utils.Conversion.StringToLocation(tbSolutions.SelectedText);
            }
            if (ll != null)
            {
                Core.CenterLocation.SetLocation(ll.Lat, ll.Lon);
                Core.Geocaches.BeginUpdate();
                foreach (Framework.Data.Geocache gc in Core.Geocaches)
                {
                    Utils.Calculus.SetDistanceAndAngleGeocacheFromLocation(gc, Core.CenterLocation);
                }
                Core.Geocaches.EndUpdate();
            }
            else
            {
                MessageBox.Show(
                    StrRes.GetString(StrRes.NO_PROPER_COORDINATES_SELECTED),
                    "Formula Solver",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged( this, new PropertyChangedEventArgs(propertyName) );
            }
        }

        public void ShowHelp()
        {
            new UserHelp(this, Core).Show();
        }
    }
}
