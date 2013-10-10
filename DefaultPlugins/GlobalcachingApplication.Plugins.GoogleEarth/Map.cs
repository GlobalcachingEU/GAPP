using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.GoogleEarth
{
    public class Map : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Google Earth";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_FLYTOSPEED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_ALTITUDE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_FIXEDVIEW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_TILT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MapForm.STR_TITLE));

            return base.Initialize(core);
        }

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new MapForm(this, core));
        }

        public override string FriendlyName
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
                return Framework.PluginType.Map;
            }
        }

        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            foreach (System.Windows.Forms.UserControl uc in configPanels)
            {
                if (uc is SettingsPanel)
                {
                    (uc as SettingsPanel).Apply();
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

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
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
                            (UIChildWindowForm as MapForm).UpdateView();
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
