using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Demo
{
    public class AutoGeocachePicker : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Random geocache selector";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(AutoGeocachePickerForm.STR_TITLE));

            return base.Initialize(core);
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

        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            foreach (System.Windows.Forms.UserControl uc in configPanels)
            {
                if (uc is SettingsPanel)
                {
                    (uc as SettingsPanel).Apply();
                    if (UIChildWindowForm != null)
                    {
                        ((AutoGeocachePickerForm)UIChildWindowForm).UpdateView();
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

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new AutoGeocachePickerForm(this, core));
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, true))
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
                            ((AutoGeocachePickerForm)UIChildWindowForm).UpdateView();
                        }
                    }
                    result = true;
                }
            }
            return result;
        }
    }
}
