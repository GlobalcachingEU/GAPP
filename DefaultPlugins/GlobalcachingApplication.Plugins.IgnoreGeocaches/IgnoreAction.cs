using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.IgnoreGeocaches
{
    public class IgnoreAction : Utils.BasePlugin.Plugin
    {
        public const string ACTION_EDIT = "Delete and ignore geocache|Edit";
        public const string ACTION_SEP = "Delete and ignore geocache|-";
        public const string ACTION_ACTIVE = "Delete and ignore geocache|Active";
        public const string ACTION_SELECTION = "Delete and ignore geocache|Selected";

        public const string STR_NOCACHES_SELECTED = "No geocaches selected";
        public const string STR_INFORMATION = "Information";
        public const string STR_WARNING = "Warning";
        public const string STR_ABOUTTODELETE = "You are about to delete";
        public const string STR_GEOCACHES = "geocaches";
        public const string STR_DELETINGGEOCACHES = "Deleting geocaches...";

        List<Framework.Data.Geocache> _gcList = null;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_EDIT);
            AddAction(ACTION_SEP);
            AddAction(ACTION_ACTIVE);
            AddAction(ACTION_SELECTION);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOCACHES_SELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_INFORMATION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_WARNING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ABOUTTODELETE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_GEOCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_DELETINGGEOCACHES));

            return await base.InitializeAsync(core);
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_ACTIVE;
            }
        }


        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Action;
            }
        }

        public async override Task<bool> ActionAsync(string action)
        {
            bool result = base.Action(action);
            Editor ed = Utils.PluginSupport.PluginByName(Core, "GlobalcachingApplication.Plugins.IgnoreGeocaches.Editor") as Editor;
            if (ed != null)
            {
                EditorForm ef = ed.ChildForm as EditorForm;
                if (ef != null)
                {
                    if (result && action == ACTION_EDIT)
                    {
                        ed.Action(ed.DefaultAction);
                    }
                    else if (result && action == ACTION_SELECTION)
                    {
                        List<Framework.Data.Geocache> gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                        if (gcList == null || gcList.Count == 0)
                        {
                            System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOCACHES_SELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_INFORMATION), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                        }
                        else
                        {
                            string msg = string.Format("{0} {1} {2}", Utils.LanguageSupport.Instance.GetTranslation(STR_ABOUTTODELETE), gcList.Count, Utils.LanguageSupport.Instance.GetTranslation(STR_GEOCACHES));
                            if (System.Windows.Forms.MessageBox.Show(msg, Utils.LanguageSupport.Instance.GetTranslation(STR_WARNING), System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Warning, System.Windows.Forms.MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.OK)
                            {
                                using (Utils.FrameworkDataUpdater upd = new Utils.FrameworkDataUpdater(Core))
                                {
                                    if (Core.ActiveGeocache != null && gcList.Contains(Core.ActiveGeocache))
                                    {
                                        Core.ActiveGeocache = null;
                                    }

                                    _gcList = gcList;
                                    await Task.Run(() =>
                                        {
                                            this.deleteSelectionThreadMethod();
                                        });
                                }
                            }
                        }
                    }
                    else if (result && action == ACTION_ACTIVE)
                    {
                        if (Core.ActiveGeocache == null)
                        {
                            System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOCACHES_SELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_INFORMATION), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                        }
                        else
                        {
                            string msg = string.Format("{0} {1} {2}", Utils.LanguageSupport.Instance.GetTranslation(STR_ABOUTTODELETE), 1, Utils.LanguageSupport.Instance.GetTranslation(STR_GEOCACHES));
                            if (System.Windows.Forms.MessageBox.Show(msg, Utils.LanguageSupport.Instance.GetTranslation(STR_WARNING), System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Warning, System.Windows.Forms.MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.OK)
                            {
                                ef.AddFilter(IgnoreService.FilterField.GeocacheCode, Core.ActiveGeocache.Code);
                                Utils.DataAccess.DeleteGeocache(Core, Core.ActiveGeocache);
                                Core.ActiveGeocache = null;
                            }
                        }
                    }
                }
            }
            return result;
        }


        private void deleteSelectionThreadMethod()
        {
            try
            {
                Editor ed = Utils.PluginSupport.PluginByName(Core, "GlobalcachingApplication.Plugins.IgnoreGeocaches.Editor") as Editor;
                if (ed != null)
                {
                    EditorForm ef = ed.ChildForm as EditorForm;
                    DateTime dt = DateTime.Now.AddSeconds(2);
                    using (Utils.ProgressBlock prog = new Utils.ProgressBlock(this, STR_DELETINGGEOCACHES, STR_DELETINGGEOCACHES, _gcList.Count, 0, true))
                    {
                        int index = 0;
                        List<string> gcCodesDeleted = new List<string>();
                        foreach (Framework.Data.Geocache gc in _gcList)
                        {
                            Utils.DataAccess.DeleteGeocache(Core, gc);
                            gcCodesDeleted.Add(gc.Code);

                            index++;
                            if (DateTime.Now >= dt)
                            {
                                ef.AddCodes(gcCodesDeleted);
                                gcCodesDeleted.Clear();
                                if (!prog.UpdateProgress(STR_DELETINGGEOCACHES, STR_DELETINGGEOCACHES, _gcList.Count, index))
                                {
                                    break;
                                }
                                dt = DateTime.Now.AddSeconds(2);
                            }
                        }
                        if (gcCodesDeleted.Count > 0)
                        {
                            ef.AddCodes(gcCodesDeleted);
                            gcCodesDeleted.Clear();
                        }
                    }
                }
            }
            catch
            {
            }
        }

    }
}
