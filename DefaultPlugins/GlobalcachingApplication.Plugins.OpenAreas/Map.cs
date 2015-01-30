using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.OpenAreas
{
    public class Map : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Open areas";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(MapForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MapForm.STR_CUSTOM));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MapForm.STR_CUSTOMWAYPOINTS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MapForm.STR_FILLOPACITY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MapForm.STR_GEOCACHE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MapForm.STR_MYSTERYIFCORRECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MapForm.STR_RADIUS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MapForm.STR_SELECTEDGEOCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MapForm.STR_STROKEOPACITY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MapForm.STR_UPDATEMAP));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MapForm.STR_WAYPOINT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MapForm.STR_WAYPOINTS));

            return await base.InitializeAsync(core);
        }

        public override string DefaultAction
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

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new MapForm(this, core));
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
                }
                result = true;
            }
            return result;
        }
    }
}
