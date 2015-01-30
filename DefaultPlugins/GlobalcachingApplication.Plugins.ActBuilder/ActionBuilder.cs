using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.ActBuilder
{
    public class ActionBuilder : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Action builder|Editor";
        public const string ACTION_DOWNLOADANDPUBLISH = "Action builder|Download and publish";
        public const string ACTION_SEP = "Action builder|-";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            var p = new PluginSettings(core);

            AddAction(ACTION_SHOW);
            AddAction(ACTION_DOWNLOADANDPUBLISH);
            AddAction(ACTION_SEP);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionBuilderForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionBuilderForm.STR_ACTIONS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionBuilderForm.STR_CONDITIONS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionBuilderForm.STR_DELETE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionBuilderForm.STR_EXECUTE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionBuilderForm.STR_FLOW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionBuilderForm.STR_NEW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionBuilderForm.STR_RENAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionBuilderForm.STR_SAVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionBuilderForm.STR_COPY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionBuilderForm.STR_EXECUTEONCE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionBuilderForm.STR_FLOWEXECUTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionBuilderForm.STR_DONOTSHOWAGAIN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionBuilderForm.STR_OK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionBuilderForm.STR_OVERWRITE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionBuilderForm.STR_WARNING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionBuilderForm.STR_DOWNLOADANDPUBLISH));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionBuilderForm.STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionBuilderForm.STR_ERRORIMPORTINGFLOW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionBuilderForm.STR_START));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_ALLPUBLICFLOWS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_AUTHOR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_CREATED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_DELETE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_DESCRIPTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_DOWNLOAD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_FLOW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_MODIFIED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_NOSCRIPTS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_OK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_PUBLIC));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_SERVERERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_SERVERERROR_1));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_SERVERERROR_2));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_SERVERERROR_3));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_SERVERERROR_4));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_SERVERERROR_5));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_SERVERERROR_6));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_SERVERERROR_7));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_SERVERFLOWS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_TITLE1));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_TITLE2));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_UPLOAD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_YOURFLOWS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_YOURUPLOADED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_SUCCESS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ServerConnectionForm.STR_DOWNLOADSUCCESS));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_SHOWCONNECTIONLABEL));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionWaypointCounter.STR_COORDSONLY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionWaypointCounter.STR_ADDGEOCACHE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActionAttributes.STR_ALL));

            return await base.InitializeAsync(core);
        }

        public async override Task ApplicationInitializedAsync()
        {
            await base.ApplicationInitializedAsync();
            if (UIChildWindowForm != null)
            {
                (UIChildWindowForm as ActionBuilderForm).ApplicationInitialized();
            }
        }

        public override string FriendlyName
        {
            get
            {
                return "Action builder";
            }
        }

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new ActionBuilderForm(this, core));
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
                return ACTION_SHOW;
            }
        }

        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            foreach (System.Windows.Forms.UserControl uc in configPanels)
            {
                if (uc is SettingsPanel)
                {
                    (uc as SettingsPanel).Apply();
                    if (UIChildWindowForm != null)
                    {
                        (UIChildWindowForm as ActionBuilderForm).SettingsChanged();
                    }
                    break;
                }
            }
            return true;
        }

        public override List<System.Windows.Forms.UserControl> CreateConfigurationPanels()
        {
            List<System.Windows.Forms.UserControl> pnls = base.CreateConfigurationPanels();
            if (pnls == null) pnls = new List<System.Windows.Forms.UserControl>();
            pnls.Add(new SettingsPanel());
            return pnls;
        }

        public void AddNewAction(string action)
        {
            AddAction(string.Format("Action builder|{0}", action));
        }

        public void DeleteAction(string action)
        {
            RemoveAction(string.Format("Action builder|{0}", action));
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
                    if (action == ACTION_DOWNLOADANDPUBLISH)
                    {
                        if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, false))
                        {
                            using (ServerConnectionForm dlg = new ServerConnectionForm(this, Core))
                            {
                                dlg.ShowDialog();
                            }
                        }
                    }
                    else
                    {
                        (UIChildWindowForm as ActionBuilderForm).RunActionFlow(action.Substring(action.IndexOf('|') + 1), true);
                    }
                }
                result = true;
            }
            return result;
        }

    }
}
