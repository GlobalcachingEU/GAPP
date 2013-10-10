using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.LogView
{
    public class LogViewer : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "View logs";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogViewerForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogViewerForm.STR_DATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogViewerForm.STR_GEOCACHECODE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogViewerForm.STR_SHOW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogViewerForm.STR_SHOWFORACTIVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogViewerForm.STR_SHOWFORUSER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogViewerForm.STR_TEXT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogViewerForm.STR_USER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogViewerForm.STR_USERNAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogViewerForm.STR_DELETE));

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
            return (new LogViewerForm(this, core));
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
                            (UIChildWindowForm as LogViewerForm).UpdateView();
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
