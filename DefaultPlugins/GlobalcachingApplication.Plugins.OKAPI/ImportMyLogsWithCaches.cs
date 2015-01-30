using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public class ImportMyLogsWithCaches : Utils.BasePlugin.BaseImportFilter
    {
        public const string STR_IMPORTINGMYF = "Importing opencaching logs...";
        public const string STR_IMPORTINGLOGS = "Importing opencaching logs...";
        public const string STR_IMPORTINGCACHES = "Importing opencaching geocaches...";
        public const string STR_ERROR = "Error";

        public const string ACTION_IMPORT = "Import all my logs";

        private string _errormessage = null;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_IMPORT);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGMYF));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGLOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));

            return await base.InitializeAsync(core);
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_IMPORT;
            }
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.OKAPI;
            }
        }

        protected override void ImportMethod()
        {
            bool cancelled = false;
            using (Utils.ProgressBlock blockprogress = new Utils.ProgressBlock(this, STR_IMPORTINGMYF, STR_IMPORTINGMYF, 1, 0))
            {
                try
                {
                    int stepSize = 100;
                    List<OKAPIService.Log> logs = new List<OKAPIService.Log>();

                    using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_IMPORTINGMYF, STR_IMPORTINGLOGS, 100, 0, true))
                    {
                        List<OKAPIService.Log> pageLogs = OKAPIService.GetLogsOfUserID(SiteManager.Instance.ActiveSite, SiteManager.Instance.ActiveSite.UserID, stepSize, 0);

                        while (pageLogs.Count() > 0)
                        {
                            logs.AddRange(pageLogs);
                            if (!progress.UpdateProgress(STR_IMPORTINGMYF, STR_IMPORTINGLOGS, logs.Count + stepSize, logs.Count))
                            {
                                cancelled = true;
                                break;
                            }
                            else if (pageLogs.Count() < stepSize)
                            {
                                break;
                            }
                            pageLogs = OKAPIService.GetLogsOfUserID(SiteManager.Instance.ActiveSite, SiteManager.Instance.ActiveSite.UserID, stepSize, logs.Count);
                        }
                    }

                    if (!cancelled)
                    {
                        //we download the geocaches that are not present. But for the ones we have, we need to add the log and mark it as found
                        List<string> gcList = (from a in logs where Utils.DataAccess.GetGeocache(Core.Geocaches, a.cache_code) != null select a.cache_code).ToList();
                        foreach (string s in gcList)
                        {
                            Utils.DataAccess.GetGeocache(Core.Geocaches, s).Found = true;
                            var ls = (from a in logs where a.cache_code == s select a).ToList();
                            foreach (var l in ls)
                            {
                                AddLog(Convert.Log(Core, l, SiteManager.Instance.ActiveSite.Username, SiteManager.Instance.ActiveSite.UserID));
                            }
                        }

                        gcList = (from a in logs where Utils.DataAccess.GetGeocache(Core.Geocaches, a.cache_code) == null select a.cache_code).ToList();
                        using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_IMPORTINGMYF, STR_IMPORTINGCACHES, gcList.Count, 0, true))
                        {
                            int gcupdatecount = 1;

                            while (gcList.Count > 0)
                            {
                                List<string> lcs = gcList.Take(gcupdatecount).ToList();
                                gcList.RemoveRange(0, lcs.Count);
                                List<OKAPIService.Geocache> caches = OKAPIService.GetGeocaches(SiteManager.Instance.ActiveSite, lcs);
                                Import.AddGeocaches(Core, caches);

                                foreach (var g in caches)
                                {
                                    var ls = (from a in logs where a.cache_code == g.code select a).ToList();
                                    foreach (var l in ls)
                                    {
                                        AddLog(Convert.Log(Core, l, SiteManager.Instance.ActiveSite.Username, SiteManager.Instance.ActiveSite.UserID));
                                    }
                                }

                                if (!progress.UpdateProgress(STR_IMPORTINGMYF, STR_IMPORTINGCACHES, logs.Count, logs.Count - gcList.Count))
                                {
                                    cancelled = true;
                                    break;
                                }
                            }

                        }

                    }
                }
                catch (Exception e)
                {
                    _errormessage = e.Message;
                }
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result && action == ACTION_IMPORT)
            {
                if (SiteManager.Instance.CheckAPIAccess())
                {
                    PerformImport();
                    if (!string.IsNullOrEmpty(_errormessage))
                    {
                        System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                }
            }
            return result;
        }

    }
}
