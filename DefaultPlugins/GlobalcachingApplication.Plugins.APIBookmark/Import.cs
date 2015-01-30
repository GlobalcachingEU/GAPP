using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.APIBookmark
{
    public class Import : Utils.BasePlugin.BaseImportFilter
    {
        public const string STR_IMPORTING = "Importing geocaches...";
        public const string STR_ERROR = "Error";

        public const string ACTION_SHOW = "Import bookmark";

        private List<string> _gcList = null;

        private string _errormessage = null;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTING));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_BOOKMARKS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_EG));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_IMPORT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_IMPORTMISSING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_NEWBOOKMARK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_REMOVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportForm.STR_URL));

            return await base.InitializeAsync(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return  Framework.PluginType.LiveAPI;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        protected override void ImportMethod()
        {
            int max = _gcList.Count;
            int gcupdatecount = 50;
            int index = 0;
            TimeSpan interval = new TimeSpan(0, 0, 0, 2, 100);
            DateTime prevCall = DateTime.MinValue;
            bool dodelay = (_gcList.Count > 30);
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_IMPORTING, STR_IMPORTING, max, 0, true))
            {
                try
                {
                    using (Utils.API.GeocachingLiveV6 client = new Utils.API.GeocachingLiveV6(Core, string.IsNullOrEmpty(Core.GeocachingComAccount.APIToken)))
                    {
                        while (_gcList.Count > 0)
                        {
                            if (dodelay)
                            {
                                TimeSpan ts = DateTime.Now - prevCall;
                                if (ts < interval)
                                {
                                    Thread.Sleep(interval - ts);
                                }
                            }

                            Utils.API.LiveV6.SearchForGeocachesRequest req = new Utils.API.LiveV6.SearchForGeocachesRequest();
                            req.IsLite = Core.GeocachingComAccount.MemberTypeId == 1;
                            req.AccessToken = client.Token;
                            req.CacheCode = new Utils.API.LiveV6.CacheCodeFilter();
                            req.CacheCode.CacheCodes = _gcList.Take(gcupdatecount).ToArray();
                            req.MaxPerPage = gcupdatecount;
                            req.GeocacheLogCount = 5;
                            index += req.CacheCode.CacheCodes.Length;
                            _gcList.RemoveRange(0, req.CacheCode.CacheCodes.Length);
                            prevCall = DateTime.Now;
                            var resp = client.Client.SearchForGeocaches(req);
                            if (resp.Status.StatusCode == 0 && resp.Geocaches != null)
                            {
                                Utils.API.Import.AddGeocaches(Core, resp.Geocaches);
                            }
                            else
                            {
                                _errormessage = resp.Status.StatusMessage;
                                break;
                            }
                            if (!progress.UpdateProgress(STR_IMPORTING, STR_IMPORTING, max, index))
                            {
                                break;
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    _errormessage = e.Message;
                }
            }
        }

        public void ImportBookmark(string name, string url)
        {
            if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, true))
            {
                using (ImportForm dlg = new ImportForm(Core, name, url))
                {
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        _gcList = dlg.SelectedGCCodes;
                        if (_gcList != null && _gcList.Count > 0)
                        {
                            _errormessage = null;
                            PerformImport();
                            if (!string.IsNullOrEmpty(_errormessage))
                            {
                                System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, true))
                {
                    using (ImportForm dlg = new ImportForm(Core))
                    {
                        if (dlg.ShowDialog()== System.Windows.Forms.DialogResult.OK)
                        {
                            _gcList = dlg.SelectedGCCodes;
                            if (_gcList != null && _gcList.Count > 0)
                            {
                                _gcList = Utils.GeocacheIgnoreSupport.Instance.FilterGeocaches(_gcList);

                                if (_gcList != null && _gcList.Count > 0)
                                {
                                    _errormessage = null;
                                    PerformImport();
                                    if (!string.IsNullOrEmpty(_errormessage))
                                    {
                                        System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
