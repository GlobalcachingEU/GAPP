using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace GlobalcachingApplication.Plugins.Browser
{
    public class BrowserScript
    {
        public const string STR_IMPORTING = "Importing geocaches...";
        public const string STR_ERROR = "Error";

        public WebBrowser Browser { get; private set; }
        public Framework.Interfaces.ICore Core { get; private set; }
        public Utils.BasePlugin.Plugin OwnerPlugin { get; private set; }
        public string Name { get; private set; }
        public bool HasControls { get; private set; }
        public WebbrowserForm.BrowserTab BrowserTab { get; private set; }

        private string _errormessage = null;
        private List<string> _gcList = null;
        private ManualResetEvent _actionReady = null;

        public BrowserScript(WebbrowserForm.BrowserTab browserTab, Utils.BasePlugin.Plugin ownerPlugin, string name, WebBrowser webBrowser, Framework.Interfaces.ICore core, bool hasControls)
        {
            BrowserTab = browserTab;
            Name = name;
            Browser = webBrowser;
            Core = core;
            HasControls = hasControls;
            OwnerPlugin = ownerPlugin;

            core.LanguageItems.Add(new Framework.Data.LanguageItem(Name));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
        }

        public virtual void LanguageChanged()
        {
        }

        public virtual void CreateControls(Control.ControlCollection collection)
        {
        }

        public virtual void Pause()
        {
        }

        public virtual void Resume()
        {
        }

        public virtual void Stop()
        {
        }

        public virtual void Rework()
        {
        }

        protected bool IsValidGCCode(string code)
        {
            const string ValidGCCodeCharacters = "0123456789ABCDEFGHJKMNPQRTVWXYZ";

            bool result = false;
            code = code.ToUpper();
            if (!string.IsNullOrEmpty(code) && code.StartsWith("GC") && code.Length>3 && code.Length<=8)
            {
                result = true;
                for (int i = 2; i < code.Length; i++)
                {
                    if (ValidGCCodeCharacters.IndexOf(code[i]) < 0)
                    {
                        result = false;
                        break;
                    }
                }
            }
            return result;
        }

        protected List<string> GetAllGCCodes()
        {
            char[] SepChars = new char[] { ' ', '>', '<', '&', '"', '\'', '(', ')', '[', ']', '\t', ';', ',', '|', '/', '\\', '\r', '\n' };

            List<string> result = new List<string>();
            if (Browser.Document.Body != null)
            {
                string doc = Browser.Document.Body.InnerHtml;
                if (doc != null)
                {
                    int pos = doc.IndexOf("GC", StringComparison.InvariantCultureIgnoreCase);
                    int pos2;
                    while (pos > 0)
                    {
                        if (SepChars.Contains(doc[pos - 1]))
                        {
                            pos2 = doc.IndexOfAny(SepChars, pos);
                            if (pos2 > 0)
                            {
                                string code = doc.Substring(pos, pos2 - pos).ToUpper();
                                if (IsValidGCCode(code))
                                {
                                    if (!result.Contains(code))
                                    {
                                        result.Add(code);
                                    }
                                }
                            }
                        }
                        pos = doc.IndexOf("GC", pos + 1, StringComparison.InvariantCultureIgnoreCase);
                    }
                }
            }
            return result;
        }

        protected void ImportGeocaches(List<string> gcCodes)
        {
            if (gcCodes != null && gcCodes.Count > 0)
            {
                _gcList = new List<string>();
                _gcList.AddRange(gcCodes);
                using (Utils.FrameworkDataUpdater d = new Utils.FrameworkDataUpdater(Core))
                {
                    _errormessage = null;
                    _actionReady = new ManualResetEvent(false);
                    Thread thrd = new Thread(new ThreadStart(this.importGeocachesThreadMethod));
                    thrd.Start();
                    while (!_actionReady.WaitOne(500))
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                    thrd.Join();
                    _actionReady.Dispose();
                    if (!string.IsNullOrEmpty(_errormessage))
                    {
                        System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void importGeocachesThreadMethod()
        {
            int max = _gcList.Count;
            int gcupdatecount = 20;
            int index = 0;
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(OwnerPlugin, STR_IMPORTING, STR_IMPORTING, max, 0, true))
            {
                try
                {
                    using (Utils.API.GeocachingLiveV6 client = new Utils.API.GeocachingLiveV6(Core, string.IsNullOrEmpty(Core.GeocachingComAccount.APIToken)))
                    {
                        while (_gcList.Count > 0)
                        {
                            Utils.API.LiveV6.SearchForGeocachesRequest req = new Utils.API.LiveV6.SearchForGeocachesRequest();
                            req.IsLite = Core.GeocachingComAccount.MemberTypeId == 1;
                            req.AccessToken = client.Token;
                            req.CacheCode = new Utils.API.LiveV6.CacheCodeFilter();
                            req.CacheCode.CacheCodes = _gcList.Take(gcupdatecount).ToArray();
                            req.MaxPerPage = gcupdatecount;
                            req.GeocacheLogCount = 5;
                            index += req.CacheCode.CacheCodes.Length;
                            _gcList.RemoveRange(0, req.CacheCode.CacheCodes.Length);
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

                            if (_gcList.Count > 0)
                            {
                                Thread.Sleep(3000);
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            _actionReady.Set();
        }

    }
}
