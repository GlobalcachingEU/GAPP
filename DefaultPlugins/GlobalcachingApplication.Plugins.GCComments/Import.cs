using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.GCComments
{
    public class Import : Utils.BasePlugin.BaseImportFilter
    {
        public const string ACTION_IMPORT = "Import GCComments";

        public const string STR_IMPORTMISSING = "Import missing geocaches";
        public const string STR_IMPORTING = "Importing geocaches...";
        public const string STR_ERROR = "Error";

        private string _filename = null;
        private bool _importMissing = false;
        private string _errormessage;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_IMPORT);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTMISSING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));

            return await base.InitializeAsync(core);
        }

        public async override Task<bool> ActionAsync(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_IMPORT)
                {
                    using (System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog())
                    {
                        dlg.FileName = "";
                        dlg.Filter = "*.gcc|*.gcc|*.*|*.*";
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            _errormessage = "";
                            _filename = dlg.FileName;
                            _importMissing = System.Windows.Forms.MessageBox.Show(string.Concat(Utils.LanguageSupport.Instance.GetTranslation(STR_IMPORTMISSING),"?"), Utils.LanguageSupport.Instance.GetTranslation(ACTION_IMPORT), System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question, System.Windows.Forms.MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes;
                            await PerformImport();
                            if (!string.IsNullOrEmpty(_errormessage))
                            {
                                System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            return result;
        }

        protected override void ImportMethod()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(_filename);

            XmlNodeList nl = doc.SelectNodes("/gccomment/comment");
            if (nl != null)
            {
                if (_importMissing)
                {
                    List<string> gcList = new List<string>();
                    foreach (XmlNode n in nl)
                    {
                        string gcCode = n.SelectSingleNode("gccode").InnerText;
                        Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(Core.Geocaches, gcCode);
                        if (gc == null)
                        {
                            gcList.Add(gcCode);
                        }
                    }
                    if (gcList.Count > 0)
                    {
                        Utils.GeocacheIgnoreSupport.Instance.FilterGeocaches(gcList);
                    }
                    if (gcList.Count > 0)
                    {
                        using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_IMPORTING, STR_IMPORTING, gcList.Count, 0, true))
                        {
                            using (Utils.API.GeocachingLiveV6 client = new Utils.API.GeocachingLiveV6(Core, string.IsNullOrEmpty(Core.GeocachingComAccount.APIToken)))
                            {
                                int index = 0;
                                int total = gcList.Count;
                                int gcupdatecount;
                                TimeSpan interval = new TimeSpan(0, 0, 0, 2, 100);
                                DateTime prevCall = DateTime.MinValue;
                                bool dodelay;
                                gcupdatecount = 30;
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
                                    GlobalcachingApplication.Utils.API.LiveV6.SearchForGeocachesRequest req = new GlobalcachingApplication.Utils.API.LiveV6.SearchForGeocachesRequest();
                                    req.IsLite = Core.GeocachingComAccount.MemberTypeId == 1;
                                    req.AccessToken = client.Token;
                                    req.CacheCode = new GlobalcachingApplication.Utils.API.LiveV6.CacheCodeFilter();
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
                                        _errormessage = resp.Status.StatusMessage;
                                        break;
                                    }
                                    if (!progress.UpdateProgress(STR_IMPORTING, STR_IMPORTING, total, index))
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (XmlNode n in nl)
                {
                    string gcCode = n.SelectSingleNode("gccode").InnerText;
                    Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(Core.Geocaches, gcCode);
                    if (gc != null)
                    {
                        gc.Notes = HttpUtility.HtmlEncode(n.SelectSingleNode("content").InnerText).Replace("\r","").Replace("\n","<br />");
                    }
                }
            }
        }
    }
}
