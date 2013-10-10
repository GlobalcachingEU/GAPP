using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.ActionSequence
{
    public class ActionList : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_EDIT = "Action sequence|Edit";
        public const string ACTION_SEP = "Action sequence|-";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_EDIT);
            AddAction(ACTION_SEP);

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionListForm.STR_ACTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionListForm.STR_DELETE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionListForm.STR_EXECUTE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionListForm.STR_EXECUTING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionListForm.STR_NEW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionListForm.STR_PLUGIN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionListForm.STR_RENAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionListForm.STR_SEQUENCE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionListForm.STR_SUBACTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionListForm.STR_TITLE));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectActionForm.STR_ACTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectActionForm.STR_PLUGIN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectActionForm.STR_SELECT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectActionForm.STR_SUBACTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectActionForm.STR_TITLE));

            return base.Initialize(core);
        }

        public override void ApplicationInitialized()
        {
            base.ApplicationInitialized();

            (UIChildWindowForm as ActionListForm).ApplicationInitialized();
        }

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new ActionListForm(this, core));
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Action;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_EDIT;
            }
        }

        public void AddNewAction(string action)
        {
            AddAction(string.Format("Action sequence|{0}", action));
        }

        public void DeleteAction(string action)
        {
            RemoveAction(string.Format("Action sequence|{0}", action));
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (UIChildWindowForm != null)
                {
                    if (action == ACTION_EDIT)
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
                    else
                    {
                        int pos = action.IndexOf('|');
                        if (pos > 0)
                        {
                            (UIChildWindowForm as ActionListForm).Execute(action.Substring(pos + 1));
                        }
                    }
                }
            }
            return result;
        }
    }
}
