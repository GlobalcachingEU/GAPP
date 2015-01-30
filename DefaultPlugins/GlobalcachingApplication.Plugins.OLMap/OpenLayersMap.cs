using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.OLMap
{
    public class OpenLayersMap : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "OpenLayers Map";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(OpenLayersMapForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(OpenLayersMapForm.STR_ACTIVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(OpenLayersMapForm.STR_ALL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(OpenLayersMapForm.STR_SELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(OpenLayersMapForm.STR_SHOWGEOCACHES));

            return await base.InitializeAsync(core);
        }

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new OpenLayersMapForm(this, core));
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Map;
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
                            (UIChildWindowForm as OpenLayersMapForm).UpdateView();
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
