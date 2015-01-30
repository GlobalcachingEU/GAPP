using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Solver
{
    public class Solver : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Solver";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SolverForm.STR_ANGLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SolverForm.STR_ANGLEANDDEGREES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SolverForm.STR_CONVERSION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SolverForm.STR_COORDINATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SolverForm.STR_DEGREES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SolverForm.STR_DISTANCE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SolverForm.STR_DUTCHFRIDINFO));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SolverForm.STR_DUTCHGRID));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SolverForm.STR_METERS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SolverForm.STR_MILES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SolverForm.STR_PROJECTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SolverForm.STR_PROJFROMCOOR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SolverForm.STR_PROJLOC));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SolverForm.STR_RESULT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SolverForm.STR_SOLVER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SolverForm.STR_TEXT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SolverForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(TextSolverRot13.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(TextSolverWordValue.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(TextSolverWordValue.STR_WORDLENGTHNOSPACE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(TextSolverWordValue.STR_WORDLENGTHSPACE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(TextSolverASCII.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(TextSolverASCII.STR_ASCIIDEC));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(TextSolverASCII.STR_ASCIIHEX));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(TextSolverFrequency.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(TextSolverCipher.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(TextSolverCipher.STR_SHIFTCOUNT));

            return await base.InitializeAsync(core);
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
            return (new SolverForm(this, core));
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

    }
}
