using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public class Updater : Utils.BasePlugin.BaseImportFilter
    {
        public const string STR_NOGEOCACHESELECTED = "No geocache selected for update";
        public const string STR_ERROR = "Error";
        public const string STR_UNABLELIVE = "Unable to access the OKAPI or process its data";
        public const string STR_UPDATINGGEOCACHES = "Updating geocaches...";

        public const string ACTION_UPDATEFULL_ALL = "Refresh|All";
        public const string ACTION_UPDATEFULL_SELECTED = "Refresh|Selected";
        public const string ACTION_UPDATEFULL_ACTIVE = "Refresh|Active";

        private List<Framework.Data.Geocache> _gcList = null;
        private string _errormessage = null;

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_UPDATEFULL_ALL);
            AddAction(ACTION_UPDATEFULL_SELECTED);
            AddAction(ACTION_UPDATEFULL_ACTIVE);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_UNABLELIVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_UPDATINGGEOCACHES));

            return base.Initialize(core);
        }

        protected override void ImportMethod()
        {
            //for now, we just use the active site
            //howver, in the future it is better to automatically switcg according geocode prefix
            //at this moment, the user is responsible for selecting the geocaches and the active site
            try
            {
                //first get a list of geocache codes
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_UPDATINGGEOCACHES, STR_UPDATINGGEOCACHES, _gcList.Count, 0, true))
                {
                    int gcupdatecount = 1;
                    int max = _gcList.Count;
                    while (_gcList.Count > 0)
                    {
                        List<string> lcs = (from Framework.Data.Geocache g in _gcList select g.Code).Take(gcupdatecount).ToList();
                        _gcList.RemoveRange(0, lcs.Count);
                        List<OKAPIService.Geocache> caches = OKAPIService.GetGeocaches(SiteManager.Instance.ActiveSite, lcs);
                        Import.AddGeocaches(Core, caches);

                        if (!progress.UpdateProgress(STR_UPDATINGGEOCACHES, STR_UPDATINGGEOCACHES, max, max - _gcList.Count))
                        {
                            break;
                        }
                    }

                }
            }
            catch (Exception e)
            {
                _errormessage = e.Message;
            }
        }


        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.OKAPI;
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result &&
                (
                 action == ACTION_UPDATEFULL_ALL ||
                 action == ACTION_UPDATEFULL_ACTIVE ||
                 action == ACTION_UPDATEFULL_SELECTED
                ))
            {
                try
                {
                    //get from goundspeak
                    if (SiteManager.Instance.CheckAPIAccess())
                    {
                        _gcList = null;
                        if (action == ACTION_UPDATEFULL_ALL)
                        {
                            _gcList = (from Framework.Data.Geocache a in Core.Geocaches select a).ToList();
                        }
                        else if (action == ACTION_UPDATEFULL_SELECTED)
                        {
                            _gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                        }
                        else if (Core.ActiveGeocache != null)
                        {
                            _gcList = new List<Framework.Data.Geocache>();
                            _gcList.Add(Core.ActiveGeocache);
                        }
                        if (_gcList != null && _gcList.Count > 0)
                        {
                            _errormessage = null;
                            PerformImport();
                            if (!string.IsNullOrEmpty(_errormessage))
                            {
                                System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        }
                    }
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_UNABLELIVE), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
            return result;
        }
    }
}
