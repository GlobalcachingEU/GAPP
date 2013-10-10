using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace GlobalcachingApplication.Plugins.UIMainWindow
{
    public partial class FormProgress : Form
    {
        private bool _cancelled;

        public class ProgressInfo
        {
            public string ActionTitle;
            public string ActionText;
            public int Max;
            public int Pos;
        }
        public List<ProgressInfo> _progressStack = new List<ProgressInfo>();

        public FormProgress()
        {
            InitializeComponent();
            _cancelled = false;
        }

        private void FormProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = (e.CloseReason == CloseReason.UserClosing);
            if (!e.Cancel)
            {
                if (Owner != null)
                {
                    Owner.Activated -= new EventHandler(Owner_Activated);
                }
            }
        }

        private void UpdateProgressInfo(ProgressInfo pi)
        {
            try
            {
                this.Text = pi.ActionTitle;
                this.labelProgressTitle.Text = pi.ActionText;
                this.progressBar1.Maximum = pi.Max;
                this.progressBar1.Value = Math.Min(pi.Pos, pi.Max);
                this.labelMin.Text = "1";
                this.labelMax.Text = pi.Max.ToString();
                this.labelPos.Text = pi.Pos.ToString();
            }
            catch
            {
            }
        }

        public void StartProgress(object sender, Framework.EventArguments.ProgressEventArgs e)
        {
            _cancelled = false;
            buttonCancel.Text = Utils.LanguageSupport.Instance.GetTranslation("Cancel");
            buttonCancel.Visible = e.CanCancel;
            buttonCancel.Enabled = true;
            ProgressInfo pi = new ProgressInfo();
            pi.ActionText = Utils.LanguageSupport.Instance.GetTranslation(e.ActionText);
            pi.ActionTitle = Utils.LanguageSupport.Instance.GetTranslation(e.ActionTitle);
            pi.Max = e.Max;
            pi.Pos = e.Position;
            _progressStack.Add(pi);
            UpdateProgressInfo(pi);
        }

        public void EndProgress(object sender, Framework.EventArguments.ProgressEventArgs e)
        {
            _progressStack.RemoveAt(_progressStack.Count-1);
            if (_progressStack.Count > 0)
            {
                ProgressInfo pi = _progressStack[_progressStack.Count - 1];
                UpdateProgressInfo(pi);
            }
        }

        public void UpdateProgress(object sender, Framework.EventArguments.ProgressEventArgs e)
        {
            if (_progressStack.Count > 0)
            {
                ProgressInfo pi = _progressStack[_progressStack.Count - 1];
                pi.ActionText = Utils.LanguageSupport.Instance.GetTranslation(e.ActionText);
                pi.ActionTitle = Utils.LanguageSupport.Instance.GetTranslation(e.ActionTitle);
                pi.Max = e.Max;
                pi.Pos = e.Position;
                e.Cancel = _cancelled;
                UpdateProgressInfo(pi);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _cancelled = true;
            buttonCancel.Enabled = false;
        }

        private void FormProgress_Shown(object sender, EventArgs e)
        {
            if (Owner != null)
            {
                Owner.Activated += new EventHandler(Owner_Activated);
                Owner.Deactivate += new EventHandler(Owner_Deactivate);
            }
        }

        void Owner_Deactivate(object sender, EventArgs e)
        {
            //
        }

        void Owner_Activated(object sender, EventArgs e)
        {
            this.BringToFront();
            this.Focus();
        }
    }

    public class FormProgressHandler: IDisposable
    {
        private static FormProgressHandler _uniqueInstance = null;
        private Framework.Interfaces.ICore _core = null;
        private SynchronizationContext _context = null;
        private FormProgress _frmDlg = null;
        private Form _owner = null;

        private FormProgressHandler(Framework.Interfaces.ICore core, Form owner)
        {
            _owner = owner;
            _context = SynchronizationContext.Current;
            if (_context == null)
            {
                _context = new SynchronizationContext();
            }

            List<Framework.Interfaces.IPlugin> pl = core.GetPlugins();
            foreach (Framework.Interfaces.IPlugin p in pl)
            {
                p.EndProgress += new Framework.EventArguments.ProgressEventHandler(p_EndProgress);
                p.StartProgress += new Framework.EventArguments.ProgressEventHandler(p_StartProgress);
                p.UpdateProgress += new Framework.EventArguments.ProgressEventHandler(p_UpdateProgress);
            }
            core.PluginAdded += new Framework.EventArguments.PluginEventHandler(core_PluginAdded);
        }

        public static FormProgressHandler Create(Framework.Interfaces.ICore core, Form owner)
        {
            if (_uniqueInstance == null)
            {
                _uniqueInstance = new FormProgressHandler(core, owner);
            }
            return (_uniqueInstance);
        }

        void core_PluginAdded(object sender, Framework.EventArguments.PluginEventArgs e)
        {
            e.Plugin.EndProgress += new Framework.EventArguments.ProgressEventHandler(p_EndProgress);
            e.Plugin.StartProgress += new Framework.EventArguments.ProgressEventHandler(p_StartProgress);
            e.Plugin.UpdateProgress += new Framework.EventArguments.ProgressEventHandler(p_UpdateProgress);            
        }

        void p_UpdateProgress(object sender, Framework.EventArguments.ProgressEventArgs e)
        {
            _context.Send(new SendOrPostCallback(delegate(object state)
            {
                if (_frmDlg != null)
                {
                    _frmDlg.UpdateProgress(sender, e);
                }
            }), null);
        }

        void p_StartProgress(object sender, Framework.EventArguments.ProgressEventArgs e)
        {
            _context.Send(new SendOrPostCallback(delegate(object state)
            {
                if (_frmDlg == null)
                {
                    _frmDlg = new FormProgress();
                    _frmDlg.Owner = _owner;
                    _frmDlg.StartProgress(sender, e);
                    _frmDlg.Show();
                }
                else
                {
                    _frmDlg.StartProgress(sender, e);
                }
            }), null);
        }

        void p_EndProgress(object sender, Framework.EventArguments.ProgressEventArgs e)
        {
            _context.Send(new SendOrPostCallback(delegate(object state)
            {
                if (_frmDlg != null)
                {
                    _frmDlg.EndProgress(sender, e);
                    if (_frmDlg._progressStack.Count == 0)
                    {
                        _frmDlg.Dispose();
                        _frmDlg = null;
                    }
                }
            }), null);
        }

        public void Dispose()
        {
            if (_uniqueInstance == this)
            {
                _uniqueInstance = null;
            }
            if (_frmDlg != null)
            {
                _frmDlg.Dispose();
                _frmDlg = null;
            }
            if (_core != null)
            {
                List<Framework.Interfaces.IPlugin> pl = _core.GetPlugins();
                foreach (Framework.Interfaces.IPlugin p in pl)
                {
                    p.EndProgress -= new Framework.EventArguments.ProgressEventHandler(p_EndProgress);
                    p.StartProgress -= new Framework.EventArguments.ProgressEventHandler(p_StartProgress);
                    p.UpdateProgress -= new Framework.EventArguments.ProgressEventHandler(p_UpdateProgress);
                }
            }
        }
    }
}
