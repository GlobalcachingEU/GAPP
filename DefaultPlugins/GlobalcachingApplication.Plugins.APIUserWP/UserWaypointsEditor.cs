using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.APIUserWP
{
    public class UserWaypointsEditor : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "User Waypoint Editor";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(UserWaypointsEditorForm.STR_ADDNEW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(UserWaypointsEditorForm.STR_COPY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(UserWaypointsEditorForm.STR_DATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(UserWaypointsEditorForm.STR_DECOUPLE_WINDOW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(UserWaypointsEditorForm.STR_DELETE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(UserWaypointsEditorForm.STR_DESCRIPTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(UserWaypointsEditorForm.STR_DOCK_WINDOW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(UserWaypointsEditorForm.STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(UserWaypointsEditorForm.STR_FAILED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(UserWaypointsEditorForm.STR_ID));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(UserWaypointsEditorForm.STR_LOCATION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(UserWaypointsEditorForm.STR_SAVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(UserWaypointsEditorForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(UserWaypointsEditorForm.STR_WAYPOINT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(UserWaypointsEditorForm.STR_WAYPOINTS));

            return await base.InitializeAsync(core);
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.LiveAPI;
            }
        }

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new UserWaypointsEditorForm(this, core));
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
                            (UIChildWindowForm as UserWaypointsEditorForm).UpdateView();
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
