using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GlobalcachingApplication.Plugins.QuickAc
{
    public class Actions : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SELECTION = "Delete|Selection";
        public const string ACTION_ACTIVE = "Delete|Active";
        public const string ACTION_CLEARFLAGS_SELECTED = "Clear flags|Selected";
        public const string ACTION_CLEARFLAGS_ALL = "Clear flags|All";

        public const string STR_NOCACHES_SELECTED = "No geocaches selected";
        public const string STR_INFORMATION = "Information";
        public const string STR_WARNING = "Warning";
        public const string STR_ABOUTTODELETE = "You are about to delete";
        public const string STR_GEOCACHES = "geocaches";
        public const string STR_DELETINGGEOCACHES = "Deleting geocaches...";

        private ManualResetEvent _actionReady = null;
        List<Framework.Data.Geocache> _gcList = null;

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SELECTION);
            AddAction(ACTION_ACTIVE);
            AddAction(ACTION_CLEARFLAGS_SELECTED);
            AddAction(ACTION_CLEARFLAGS_ALL);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOCACHES_SELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_INFORMATION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_WARNING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ABOUTTODELETE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_GEOCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_DELETINGGEOCACHES));

            return base.Initialize(core);
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
            if (result && action == ACTION_CLEARFLAGS_SELECTED)
            {
                List<Framework.Data.Geocache> gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                if (gcList == null || gcList.Count == 0)
                {
                    System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOCACHES_SELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_INFORMATION), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                else
                {
                    using (Utils.FrameworkDataUpdater upd = new Utils.FrameworkDataUpdater(Core))
                    {
                        foreach (Framework.Data.Geocache gc in gcList)
                        {
                            gc.Flagged = false;
                        }
                    }
                }
            }
            else if (result && action == ACTION_CLEARFLAGS_ALL)
            {
                using (Utils.FrameworkDataUpdater upd = new Utils.FrameworkDataUpdater(Core))
                {
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        gc.Flagged = false;
                    }
                }
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
                            _actionReady = new ManualResetEvent(false);
                            Thread thrd = new Thread(new ThreadStart(this.deleteSelectionThreadMethod));
                            thrd.Start();
                            while (!_actionReady.WaitOne(10))
                            {
                                System.Windows.Forms.Application.DoEvents();
                            }
                            thrd.Join();
                            _actionReady.Dispose();
                            _actionReady = null;
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
                        using (Utils.FrameworkDataUpdater upd = new Utils.FrameworkDataUpdater(Core))
                        {
                            Utils.DataAccess.DeleteGeocache(Core, Core.ActiveGeocache);
                            Core.ActiveGeocache = null;
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
                DateTime dt = DateTime.Now.AddSeconds(2);
                using (Utils.ProgressBlock prog = new Utils.ProgressBlock(this, STR_DELETINGGEOCACHES, STR_DELETINGGEOCACHES, _gcList.Count, 0, true))
                {
                    int index = 0;
                    foreach (Framework.Data.Geocache gc in _gcList)
                    {
                        Utils.DataAccess.DeleteGeocache(Core, gc);

                        index++;
                        if (DateTime.Now >= dt)
                        {
                            if (!prog.UpdateProgress(STR_DELETINGGEOCACHES, STR_DELETINGGEOCACHES, _gcList.Count, index))
                            {
                                break;
                            }
                            dt = DateTime.Now.AddSeconds(2);
                        }
                    }
                }
            }
            catch
            {
            }
            _actionReady.Set();
        }
    }
}
