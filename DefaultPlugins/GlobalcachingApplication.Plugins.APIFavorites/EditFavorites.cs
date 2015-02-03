using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.APIFavorites
{
    public class EditFavorites : Utils.BasePlugin.BaseImportFilter
    {
        public const string ACTION_GET = "Favorites|Get all your Favorites geocache codes";
        public const string ACTION_IMPORT = "Favorites|Import all your missing Favorites geocaches";
        public const string ACTION_ADD = "Favorites|Add active geocache to your Favorites";
        public const string ACTION_REMOVE = "Favorites|Remove active geocache from your Favorites";

        public const string STR_ERROR = "Error";
        public const string STR_NOACTIVE = "No active geocache selected";
        public const string STR_GETTINGDATA = "Retrieving data from geocaching.com";
        public const string STR_GET = "Get all your Favorites geocache codes";
        public const string STR_IMPORT = "Import all your missing Favorites geocaches";
        public const string STR_ADD = "Add active geocache to your Favorites";
        public const string STR_REMOVE = "Remove active geocache from your Favorites";
        public const string STR_IMPORTINGCACHES = "Importing geocaching.com geocaches...";

        private string _errormessage = null;
        private int _apiLimit = -1;
        private int _apiLeft = -1;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            if (PluginSettings.Instance == null)
            {
                var p = new PluginSettings(core);
            }

            AddAction(ACTION_GET);
            AddAction(ACTION_IMPORT);
            AddAction(ACTION_ADD);
            AddAction(ACTION_REMOVE);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOACTIVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_GETTINGDATA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_GET));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ADD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_REMOVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGCACHES));

            return await base.InitializeAsync(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.LiveAPI;
            }
        }

        public void AddToFavorites(string gccode)
        {
            Favorites.Instance(Core).AddGeocacheCode(gccode);
        }

        protected override void ImportMethod()
        {
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_IMPORT, STR_IMPORTINGCACHES, 1, 0, true))
            {
                using (Utils.API.GeocachingLiveV6 api = new Utils.API.GeocachingLiveV6(Core))
                {
                    try
                    {
                        var respC = api.Client.GetCacheIdsFavoritedByUser(api.Token);
                        if (respC.Status.StatusCode == 0)
                        {
                            Favorites.Instance(Core).Reset(respC.CacheCodes);
                            List<string> gcList = (from s in respC.CacheCodes where Utils.DataAccess.GetGeocache(Core.Geocaches, s) == null select s).ToList();

                            if (gcList.Count > 0)
                            {
                                if (progress.UpdateProgress(STR_IMPORT, STR_IMPORTINGCACHES, gcList.Count, 0))
                                {
                                    int index = 0;
                                    int max = gcList.Count;
                                    int gcupdatecount;
                                    TimeSpan interval = new TimeSpan(0, 0, 0, 2, 100);
                                    DateTime prevCall = DateTime.MinValue;
                                    bool dodelay;

                                    gcupdatecount = 20;
                                    dodelay = (gcList.Count > 30);

                                    while (gcList.Count > 0)
                                    {
                                        if (dodelay)
                                        {
                                            TimeSpan ts = DateTime.Now - prevCall;
                                            if (ts < interval)
                                            {
                                                System.Threading.Thread.Sleep(interval - ts);
                                            }
                                        }

                                        Utils.API.LiveV6.SearchForGeocachesRequest req = new Utils.API.LiveV6.SearchForGeocachesRequest();
                                        req.IsLite = Core.GeocachingComAccount.MemberTypeId == 1;
                                        req.AccessToken = api.Token;
                                        req.CacheCode = new Utils.API.LiveV6.CacheCodeFilter();
                                        req.CacheCode.CacheCodes = (from a in gcList select a).Take(gcupdatecount).ToArray();
                                        req.MaxPerPage = gcupdatecount;
                                        req.GeocacheLogCount = 0;
                                        index += req.CacheCode.CacheCodes.Length;
                                        gcList.RemoveRange(0, req.CacheCode.CacheCodes.Length);
                                        prevCall = DateTime.Now;
                                        var resp = api.Client.SearchForGeocaches(req);
                                        if (resp.Status.StatusCode == 0 && resp.Geocaches != null)
                                        {
                                            if (resp.CacheLimits != null)
                                            {
                                                _apiLimit = resp.CacheLimits.MaxCacheCount;
                                                _apiLeft = resp.CacheLimits.CachesLeft;
                                            }
                                            Utils.API.Import.AddGeocaches(Core, resp.Geocaches);

                                            if (!progress.UpdateProgress(STR_IMPORT, STR_IMPORTINGCACHES, max, max - gcList.Count))
                                            {
                                                break;
                                            }

                                        }
                                        else
                                        {
                                            _errormessage = resp.Status.StatusMessage;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _errormessage = respC.Status.StatusMessage;
                        }
                    }
                    catch(Exception e)
                    {
                        _errormessage = e.Message;
                    }
                }
            }

        }

        public async override Task<bool> ActionAsync(string action)
        {
            bool result = base.Action(action);
            if (result && Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, true))
            {
                using (Utils.API.GeocachingLiveV6 api = new Utils.API.GeocachingLiveV6(Core, false))
                {
                    if (action == ACTION_ADD)
                    {
                        if (Core.ActiveGeocache != null)
                        {
                            try
                            {
                                using (Utils.ProgressBlock prog = new Utils.ProgressBlock(this, STR_GET, STR_ADD, 0, 1))
                                {
                                    Application.DoEvents();

                                    var resp = api.Client.AddFavoritePointToCache(api.Token, Core.ActiveGeocache.Code);
                                    if (resp.Status.StatusCode == 0)
                                    {
                                        Favorites.Instance(Core).AddGeocacheCode(Core.ActiveGeocache.Code);
                                    }
                                    else
                                    {
                                        MessageBox.Show(resp.Status.StatusMessage, Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message, Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                            }
                        }
                        else
                        {
                            MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOACTIVE), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                        }
                    }
                    else if (action == ACTION_REMOVE)
                    {
                        if (Core.ActiveGeocache != null)
                        {
                            try
                            {
                                using (Utils.ProgressBlock prog = new Utils.ProgressBlock(this, STR_GET, STR_REMOVE, 0, 1))
                                {
                                    Application.DoEvents();

                                    var resp = api.Client.RemoveFavoritePointFromCache(api.Token, Core.ActiveGeocache.Code);
                                    Favorites.Instance(Core).RemoveGeocacheCode(Core.ActiveGeocache.Code);
                                    if (resp.Status.StatusCode != 0)
                                    {
                                        MessageBox.Show(resp.Status.StatusMessage, Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message, Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                            }
                        }
                        else
                        {
                            MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOACTIVE), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                        }
                    }
                    else if (action == ACTION_GET)
                    {
                        try
                        {
                            using (Utils.ProgressBlock prog = new Utils.ProgressBlock(this, STR_GET, STR_GETTINGDATA, 0, 1))
                            {
                                Application.DoEvents();

                                var resp = api.Client.GetCacheIdsFavoritedByUser(api.Token);
                                if (resp.Status.StatusCode == 0)
                                {
                                    Favorites.Instance(Core).Reset(resp.CacheCodes);
                                }
                                else
                                {
                                    MessageBox.Show(resp.Status.StatusMessage, Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                        }
                    }
                    else if (action == ACTION_IMPORT)
                    {
                        _errormessage = null;
                        _apiLimit = -1;
                        await PerformImport();
                        if (!string.IsNullOrEmpty(_errormessage))
                        {
                            System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        }
                        if (_apiLimit >= 0)
                        {
                            Utils.Dialogs.LiveAPICachesLeftForm.ShowMessage(_apiLimit, _apiLeft);
                        }
                    }
                }
            }
            return result;
        }

    }
}
