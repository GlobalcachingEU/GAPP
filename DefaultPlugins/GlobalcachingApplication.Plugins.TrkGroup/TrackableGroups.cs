using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.TrkGroup
{
    public class TrackableGroups : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Trackable groups";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            return await base.InitializeAsync(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return  Framework.PluginType.LiveAPI;
            }
        }

        public override string FriendlyName
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        public override List<System.Windows.Forms.UserControl> CreateConfigurationPanels()
        {
            List<System.Windows.Forms.UserControl> pnls = base.CreateConfigurationPanels();
            if (pnls == null) pnls = new List<System.Windows.Forms.UserControl>();
            pnls.Add(new SettingsPanel());
            return pnls;
        }


        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            SettingsPanel panel = (from p in configPanels where p.GetType() == typeof(SettingsPanel) select p).FirstOrDefault() as SettingsPanel;
            panel.Apply();
            return true;
        }

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new TrackableGroupsForm(this, core));
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        public override bool ActionEnabled(string action, int selectCount, bool active)
        {
            bool result = base.ActionEnabled(action, selectCount, active);
            if (result)
            {
                result = !string.IsNullOrEmpty(Core.GeocachingComAccount.APIToken);
            }
            return result;
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
                        if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, false))
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
                }
                result = true;
            }
            return result;
        }
    }
}
