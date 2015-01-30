using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.APINotes
{
    public class GetNotesFromGeocachingCom : Utils.BasePlugin.BaseImportFilter
    {
        public const string STR_IMPORTING = "Importing geocaching.com field notes...";
        public const string STR_ERROR = "Error";
        public const string STR_IMPORTGEOCACHES = "geocaches are missing.\r\nImport the missing geocaches?";
        public const string STR_QUESTIONGEOCACHES = "Importing geocaches";
        public const string STR_IMPORTINGGEOCACHES = "Importing geocaches...";

        public const string ACTION_IMPORT = "Import Geocache Notes";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_IMPORT);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTGEOCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_QUESTIONGEOCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGGEOCACHES));

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
                return Framework.PluginType.LiveAPI;
            }
        }

        protected override void ImportMethod()
        {
            List<Utils.API.LiveV6.CacheNote> missingGeocaches = new List<Utils.API.LiveV6.CacheNote>();
            string errMessage = "";
            try
            {
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_IMPORTING, STR_IMPORTING, 1, 0))
                {
                    //clear all notes
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        gc.PersonaleNote = "";
                    }

                    using (Utils.API.GeocachingLiveV6 client = new Utils.API.GeocachingLiveV6(Core, string.IsNullOrEmpty(Core.GeocachingComAccount.APIToken)))
                    {
                        int maxPerRequest = 100;
                        int startIndex = 0;
                        var resp = client.Client.GetUsersCacheNotes(client.Token, startIndex, maxPerRequest);
                        while (resp.Status.StatusCode == 0)
                        {
                            foreach (var n in resp.CacheNotes)
                            {
                                Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(Core.Geocaches, n.CacheCode);
                                if (gc != null)
                                {
                                    string s = n.Note ?? "";
                                    s = s.Replace("\r", "");
                                    s = s.Replace("\n", "\r\n");
                                    gc.PersonaleNote = s;
                                }
                                else
                                {
                                    missingGeocaches.Add(n);
                                }
                            }
                            if (resp.CacheNotes.Count() >= maxPerRequest)
                            {
                                startIndex += resp.CacheNotes.Count();
                                Thread.Sleep(2100);
                                resp = client.Client.GetUsersCacheNotes(client.Token, startIndex, maxPerRequest);
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (resp.Status.StatusCode != 0)
                        {
                            if (!string.IsNullOrEmpty(resp.Status.StatusMessage))
                            {
                                errMessage = resp.Status.StatusMessage;
                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(errMessage) && missingGeocaches.Count > 0)
                {
                    List<string> gcList = (from a in missingGeocaches select a.CacheCode).ToList();
                    if (System.Windows.Forms.MessageBox.Show(string.Format("{0} {1}", missingGeocaches.Count, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_IMPORTGEOCACHES))), Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_QUESTIONGEOCACHES)), System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                    {
                        using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_IMPORTINGGEOCACHES, STR_IMPORTINGGEOCACHES, gcList.Count, 0, true))
                        {
                            int totalcount = gcList.Count;
                            using (Utils.API.GeocachingLiveV6 client = new Utils.API.GeocachingLiveV6(Core, string.IsNullOrEmpty(Core.GeocachingComAccount.APIToken)))
                            {
                                int index = 0;
                                int gcupdatecount;
                                TimeSpan interval = new TimeSpan(0, 0, 0, 2, 100);
                                DateTime prevCall = DateTime.MinValue;
                                bool dodelay;
                                gcupdatecount = 50;
                                dodelay = (gcList.Count > 30);
                                while (gcList.Count > 0)
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
                                    req.CacheCode.CacheCodes = (from a in gcList select a).Take(gcupdatecount).ToArray();
                                    req.MaxPerPage = gcupdatecount;
                                    req.GeocacheLogCount = 5;
                                    index += req.CacheCode.CacheCodes.Length;
                                    gcList.RemoveRange(0, req.CacheCode.CacheCodes.Length);
                                    prevCall = DateTime.Now;
                                    var resp = client.Client.SearchForGeocaches(req);
                                    if (resp.Status.StatusCode == 0 && resp.Geocaches != null)
                                    {
                                        Utils.API.Import.AddGeocaches(Core, resp.Geocaches);
                                    }
                                    else
                                    {
                                        errMessage = resp.Status.StatusMessage;
                                        break;
                                    }                                    
                                    if (!progress.UpdateProgress(STR_IMPORTINGGEOCACHES, STR_IMPORTINGGEOCACHES, totalcount, index))
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        foreach (var n in missingGeocaches)
                        {
                            Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(Core.Geocaches, n.CacheCode);
                            if (gc != null)
                            {
                                string s = n.Note ?? "";
                                s = s.Replace("\r", "");
                                s = s.Replace("\n", "\r\n");
                                gc.PersonaleNote = s;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                errMessage = e.Message;
            }
            if (!string.IsNullOrEmpty(errMessage))
            {
                System.Windows.Forms.MessageBox.Show(errMessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result && action == ACTION_IMPORT)
            {
                //get from goundspeak
                if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, false))
                {
                    PerformImport();
                }
            }
            return result;
        }


    }
}
