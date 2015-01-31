using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.GCEdit
{
    public class GCEditor : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Geocache Editor";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_ARCHIVED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_AVAILABLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_CACHETYPE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_CITY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_CODE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_CONTAINER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_COORDINATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_COUNTRY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_CUSTOMCOORD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_DIFFICULTY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_FAVORITES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_FOUND));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_ID));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_MEMBERONY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_MUNICIPALITY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_OWNER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_OWNERID));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_PERSONALENOTES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_PLACEDBY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_PUBLISHED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_SAVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_STATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_TERRAIN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_URL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_HINTS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_LOCKED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_NEW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GCEditorForm.STR_NONAME));

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
            return (new GCEditorForm(this, core));
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
                            (UIChildWindowForm as GCEditorForm).UpdateView();
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
