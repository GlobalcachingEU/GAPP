using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.GCEdit
{
    public class MassGCEditor : Utils.BasePlugin.Plugin
    {
        public const string STR_NOGEOCACHESELECTED = "No geocache selected for export";
        public const string STR_ERROR = "Error";

        public const string ACTION_EDIT_SELECTED = "Edit Geocaches|Selected";
        public const string ACTION_EDIT_ALL = "Edit Geocaches|All";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_EDIT_SELECTED);
            AddAction(ACTION_EDIT_ALL);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_ARCHIVED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_AVAILABLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_CACHETYPE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_CITY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_CONTAINER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_COORDINATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_COUNTRY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_CUSTOMCOORD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_DIFFICULTY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_FAVORITES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_FOUND));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_MEMBERONY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_MUNICIPALITY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_OWNER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_OWNERID));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_PERSONALENOTES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_PLACEDBY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_PUBLISHED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_STATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_TERRAIN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_URL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_HINTS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_LOCKED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_NEW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(MassGCEditorForm.STR_NONAME));

            return base.Initialize(core);
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_EDIT_SELECTED;
            }
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Action;
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_EDIT_ALL || action==ACTION_EDIT_SELECTED)
                {
                    List<Framework.Data.Geocache> gcList = null;
                    if (action == ACTION_EDIT_ALL)
                    {
                        gcList = (from Framework.Data.Geocache a in Core.Geocaches select a).ToList();
                    }
                    else if (action == ACTION_EDIT_SELECTED)
                    {
                        gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                    }
                    if (gcList == null || gcList.Count == 0)
                    {
                        System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                    else
                    {
                        using (MassGCEditorForm dlg = new MassGCEditorForm(Core, gcList))
                        {
                            dlg.ShowDialog();
                        }
                    }
                }
            }
            return result;
        }
    }
}
