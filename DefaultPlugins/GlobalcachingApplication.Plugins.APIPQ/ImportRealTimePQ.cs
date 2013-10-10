using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
/*
namespace GlobalcachingApplication.Plugins.APIPQ
{
    public class ImportRealTimePQ : Utils.BasePlugin.BaseImportFilter
    {
        public const string ACTION_IMPORT = "Import Real Time Pocket Queries";

        public const string STR_IMPORTINGGEOCACHES = "Importing geocaches...";
        public const string STR_UNABLEACCESSAPI = "Unable to access the Live API or process its data";
        public const string STR_ERROR = "Error";

        private List<Utils.API.LiveV6.PQData> _pqs = null;
        private Utils.API.GeocachingLiveV6 _client = null;
        private string _message = null;

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_IMPORT);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGGEOCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_UNABLEACCESSAPI));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));

            return base.Initialize(core);
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
            bool cancel = false;
            int totalcaches = _pqs.Sum(x => x.PQCount);
            int totalcacheindex = 0;
            int maxItems = 30;
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_IMPORTINGGEOCACHES, STR_IMPORTINGGEOCACHES, totalcaches, 0, true))
            {
                foreach (Utils.API.LiveV6.PQData pq in _pqs)
                {
                    if (progress.UpdateProgress(STR_IMPORTINGGEOCACHES, STR_IMPORTINGGEOCACHES, totalcaches, Math.Min(totalcacheindex, totalcaches)))
                    {
                        int thisPqIndex = 0;
                        var resp = _client.Client.GetPocketQueryData(_client.Token, pq.GUID, thisPqIndex, maxItems, false);
                        while (resp.Status.StatusCode == 0 && resp.Geocaches!=null && resp.Geocaches.Count()>0)
                        {
                            Utils.API.Import.AddGeocaches(Core, resp.Geocaches);

                            totalcacheindex += resp.Geocaches.Count();
                            thisPqIndex += resp.Geocaches.Count();
                            if (!progress.UpdateProgress(STR_IMPORTINGGEOCACHES, STR_IMPORTINGGEOCACHES, totalcaches, Math.Min(totalcacheindex, totalcaches)))
                            {
                                cancel = true;
                                break;
                            }
                            resp = _client.Client.GetPocketQueryData(_client.Token, pq.GUID, thisPqIndex, maxItems, false);
                        }
                        if (resp.Status.StatusCode!=0)
                        {
                            _message = resp.Status.StatusMessage;
                            cancel = true;
                            break;
                        }
                        if (cancel)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private Utils.API.LiveV6.PQData[] getAvailablePocketQueries(Utils.API.GeocachingLiveV6 client)
        {
            Utils.API.LiveV6.PQData[] result = null;
            Utils.API.LiveV6.GetPocketQueryListResponse resp = client.Client.GetPocketQueryList(client.Token);
            if (resp.Status.StatusCode == 0)
            {
                result = resp.PocketQueryList;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show(resp.Status.StatusMessage, Utils.LanguageSupport.Instance.GetTranslation("Error"), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            return result;
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result && action == ACTION_IMPORT)
            {
                try
                {
                    //get from goundspeak
                    if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, true))
                    {
                        using (Utils.API.GeocachingLiveV6 client = new Utils.API.GeocachingLiveV6(Core, false))
                        {
                            Utils.API.LiveV6.PQData[] pqData = getAvailablePocketQueries(client);
                            if (pqData != null)
                            {
                                using (SelectPQForm dlg = new SelectPQForm(pqData, new Hashtable()))
                                {
                                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                    {
                                        _message = "";
                                        _pqs = dlg.SelectedPQs;
                                        _client = client;
                                        PerformImport();
                                        if (!string.IsNullOrEmpty(_message))
                                        {
                                            System.Windows.Forms.MessageBox.Show(_message, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_UNABLEACCESSAPI), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
            return result;
        }

    }
}
*/