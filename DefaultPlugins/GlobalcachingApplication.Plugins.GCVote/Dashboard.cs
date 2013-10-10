using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.GCVote
{
    class Dashboard : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "GCVote dashboard";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(DashboardForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(DashboardForm.STR_AVEGARE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(DashboardForm.STR_CLEARALLVOTES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(DashboardForm.STR_CLEARSAVEDDATA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(DashboardForm.STR_COUNT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(DashboardForm.STR_GEOCACHE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(DashboardForm.STR_LOADALLVOTES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(DashboardForm.STR_MAINTENANCE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(DashboardForm.STR_MEDIAN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(DashboardForm.STR_SETTINGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(DashboardForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(DashboardForm.STR_WARNDELETE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(DashboardForm.STR_WARNING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(DashboardForm.STR_YOURVOTE));

            return base.Initialize(core);
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
            return (new DashboardForm(this, core));
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
                            (UIChildWindowForm as DashboardForm).UpdateView();
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
