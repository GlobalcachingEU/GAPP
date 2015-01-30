using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Gallery
{
    public class ImageGallery : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Image Gallery";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageGalleryForm.STR_ACTIVEGEOCACHE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageGalleryForm.STR_ALL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageGalleryForm.STR_ASC));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageGalleryForm.STR_DATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageGalleryForm.STR_DESC));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageGalleryForm.STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageGalleryForm.STR_IMPORTINGLOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageGalleryForm.STR_IMPORTINGMYF));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageGalleryForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageGalleryForm.STR_SHOW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageGalleryForm.STR_SORTON));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageGalleryForm.STR_SORTORDER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageGalleryForm.STR_TEXT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageGalleryForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageGalleryForm.STR_UPDATE));

            return await base.InitializeAsync(core);
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
            return (new ImageGalleryForm(this, core));
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
                                (UIChildWindowForm as ImageGalleryForm).UpdateView();
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
