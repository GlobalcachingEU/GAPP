using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Utils.BasePlugin
{
    public class BaseUIChildWindow: Plugin, Framework.Interfaces.IPluginUIChildWindow
    {
        private BaseUIChildWindowForm _uiChildWindowForm = null;

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            bool result = false;
            _uiChildWindowForm = CreateUIChildWindowForm(core);
            if (_uiChildWindowForm != null)
            {
                _uiChildWindowForm.FormClosed += new System.Windows.Forms.FormClosedEventHandler(_uiChildWindowForm_FormClosed);
                result = base.Initialize(core);
            }
            return result;
        }

        void _uiChildWindowForm_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            Close();
        }

        protected override void InitUIMainWindow(Framework.Interfaces.IPluginUIMainWindow mainWindowPlugin)
        {
            base.InitUIMainWindow(mainWindowPlugin);
            if (Properties.Settings.Default.DecoupledChildWindows != null)
            {
                if (Properties.Settings.Default.DecoupledChildWindows.Contains(_uiChildWindowForm.GetType().FullName))
                {
                    //_uiChildWindowForm.MdiParent = null;
                }
                else
                {
                    _uiChildWindowForm.MdiParent = mainWindowPlugin.MainForm;
                }
            }
            else
            {
                _uiChildWindowForm.MdiParent = mainWindowPlugin.MainForm;
            }
            if (Properties.Settings.Default.TopMostWindows != null)
            {
                if (Properties.Settings.Default.TopMostWindows.Contains(_uiChildWindowForm.GetType().FullName))
                {
                    _uiChildWindowForm.TopMost = true;
                }
            }
        }

        public System.Windows.Forms.Form ChildForm
        {
            get { return _uiChildWindowForm; }
        }

        protected BaseUIChildWindowForm UIChildWindowForm
        {
            get { return _uiChildWindowForm; }
        }

        protected virtual BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new BaseUIChildWindowForm(this, core));
        }

        public override void Close()
        {
            if (_uiChildWindowForm != null)
            {
                _uiChildWindowForm.Dispose();
                _uiChildWindowForm = null;
            }
            base.Close();
        }

        public override Framework.PluginType PluginType
        {
            get { return Framework.PluginType.UIChildWindow; }
        }

    }
}
