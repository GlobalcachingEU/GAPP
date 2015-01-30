using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Browser
{
    public class BrowserPlugin : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Web browser";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(WebbrowserForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(WebbrowserForm.STR_COMPMODECHANGED));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsForm.STR_DELETE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsForm.STR_HOMEPAGE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsForm.STR_NAMESPACE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsForm.STR_NEW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsForm.STR_OK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsForm.STR_RENAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsForm.STR_SYSTEMSCRIPTS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsForm.STR_USERSCRIPTS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsForm.STR_SUPPRESSSCRIPTERRORS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsForm.STR_COMPATIBILITY));

            return await base.InitializeAsync(core);
        }

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new WebbrowserForm(this, core));
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.UIChildWindow;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        public void OpenNewBrowser(string url)
        {
            if (UIChildWindowForm != null)
            {
                if (!UIChildWindowForm.Visible)
                {
                    UIChildWindowForm.Show();
                    (UIChildWindowForm as WebbrowserForm).UpdateView();
                }
                if (UIChildWindowForm.WindowState == FormWindowState.Minimized)
                {
                    UIChildWindowForm.WindowState = FormWindowState.Normal;
                }
                UIChildWindowForm.BringToFront();
                (UIChildWindowForm as WebbrowserForm).OpenNewBrowser(url);
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
                            (UIChildWindowForm as WebbrowserForm).UpdateView();
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
