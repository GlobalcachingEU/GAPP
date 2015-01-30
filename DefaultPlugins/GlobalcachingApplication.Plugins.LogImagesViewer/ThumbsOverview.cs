using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.LogImagesViewer
{
    public class ThumbsOverview : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Log image thumbs overview";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(ThumbsOverviewForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ThumbsOverviewForm.STR_APPLY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ThumbsOverviewForm.STR_FINDER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ThumbsOverviewForm.STR_GCCODE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ThumbsOverviewForm.STR_GCNAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ThumbsOverviewForm.STR_GCOWNER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ThumbsOverviewForm.STR_NAME));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(ThumbInfoItem.STR_BY));

            return await base.InitializeAsync(core);
        }

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new ThumbsOverviewForm(this, core));
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
                            (UIChildWindowForm as ThumbsOverviewForm).UpdateView();
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
