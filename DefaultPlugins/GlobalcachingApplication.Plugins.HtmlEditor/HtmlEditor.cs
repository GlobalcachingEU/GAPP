using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.HtmlEditor
{
    public class HtmlEditor : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "HTML Editor";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            var p = new PluginSettings(core);

            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(HtmlEditorForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(HtmlEditorForm.STR_CKEDITORONLINE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(HtmlEditorForm.STR_COPYTOCLIPBOARD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(HtmlEditorForm.STR_EDITOR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(HtmlEditorForm.STR_HTML));

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
            return (new HtmlEditorForm(this, core));
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
                            (UIChildWindowForm as HtmlEditorForm).UpdateView();
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
