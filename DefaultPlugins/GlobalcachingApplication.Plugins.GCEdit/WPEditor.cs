using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.GCEdit
{
    public class WPEditor : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Waypoint Editor";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(WPEditorForm.STR_ADDNEW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(WPEditorForm.STR_CODE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(WPEditorForm.STR_COMMENT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(WPEditorForm.STR_DESCRIPTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(WPEditorForm.STR_ID));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(WPEditorForm.STR_LOCATION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(WPEditorForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(WPEditorForm.STR_REMOVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(WPEditorForm.STR_SAVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(WPEditorForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(WPEditorForm.STR_TYPE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(WPEditorForm.STR_URL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(WPEditorForm.STR_URLNAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(WPEditorForm.STR_WAYPOINT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(WPEditorForm.STR_WAYPOINTS));

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
            return (new WPEditorForm(this, core));
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
                            (UIChildWindowForm as WPEditorForm).UpdateView();
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
