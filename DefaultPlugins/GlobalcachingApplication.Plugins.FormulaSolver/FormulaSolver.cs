using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.FormulaSolver
{
    public class FormulaSolver: Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Formula Solver";
        private FormulaSolverForm frm;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);
            StrRes.InitializeCoreLanguageItems(core);
            return await base.InitializeAsync(core);
        }

        public override string FriendlyName
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            frm = new FormulaSolverForm(this, core);
            return frm;
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (UIChildWindowForm != null)
                {
                    if (action == ACTION_SHOW)
                    {
                        if (!UIChildWindowForm.Visible)
                        {
                            (UIChildWindowForm as FormulaSolverForm).UpdateView();
                            UIChildWindowForm.Show();
                        }
                        if (UIChildWindowForm.WindowState == FormWindowState.Minimized)
                        {
                            UIChildWindowForm.WindowState = FormWindowState.Normal;
                        }
                        UIChildWindowForm.BringToFront();
                    }
                }
                result = true;
            }
            return result;
        }

        public override bool IsHelpAvailable
        {
            get
            {
                return true;
            }
        }

        public override void ShowHelp()
        {
            frm.ShowHelp();
        }
    }
}
