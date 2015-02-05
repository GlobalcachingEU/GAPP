using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.AutoUpdater
{
    public class Updater : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SHOW = "Automatic update and download geocaches";
        
        public const string STR_ERROR = "Error";
        public const string STR_UPDATING = "Update status of geocaches and import new geocaches";
        public const string STR_NONEW = "There are no new geocaches found";
        public const string STR_INFO = "Info";
        public const string STR_ASKUPDATE = "There are {0} new geocaches found\r\nImport these?";
        public const string STR_IMPORTING = "Importing geocaches...";

        private string _errormessage;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            var p = new PluginSettings(core);

            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ASKUPDATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_INFO));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NONEW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_UPDATING));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_AUTODOWNLOAD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_BE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_LU));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_NL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_SHOWDIALOG));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_UPDATEANDDOWNLOAD));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsForm.STR_OK));

            return await base.InitializeAsync(core);
        }

        public override string FriendlyName
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Action;
            }
        }

        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            foreach (System.Windows.Forms.UserControl uc in configPanels)
            {
                if (uc is SettingsPanel)
                {
                    (uc as SettingsPanel).Apply();
                    break;
                }
            }
            return true;
        }

        public override List<System.Windows.Forms.UserControl> CreateConfigurationPanels()
        {
            List<System.Windows.Forms.UserControl> pnls = base.CreateConfigurationPanels();
            if (pnls == null) pnls = new List<System.Windows.Forms.UserControl>();
            pnls.Add(new SettingsPanel());
            return pnls;
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        public async override Task<bool> ActionAsync(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_SHOW)
                {
                    if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, false))
                    {
                        bool perform = true;
                        if (PluginSettings.Instance.ShowSettingsDialog)
                        {
                            using (SettingsForm dlg = new SettingsForm())
                            {
                                perform = dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK;
                            }
                        }
                        if (perform)
                        {
                            using (Utils.FrameworkDataUpdater updater = new Utils.FrameworkDataUpdater(Core))
                            {
                                _errormessage = "";
                                await Task.Run(() =>
                                    {
                                        this.threadMethod();
                                    });
                                if (!string.IsNullOrEmpty(_errormessage))
                                {
                                    System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                }
                result = true;
            }
            return result;
        }


        private void updateGeocachesFromGlobalcachingEU(string country, List<string> missingGcList)
        {
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                string doc = wc.DownloadString(string.Format("http://www.globalcaching.eu/Service/GeocacheCodes.aspx?country={0}", country));
                if (doc != null)
                {
                    string[] lines = doc.Replace("\r", "").Split(new char[] { '\n' });
                    Framework.Data.Geocache gc;
                    char[] sep = new char[] { ',' };
                    string[] parts;
                    foreach (string s in lines)
                    {
                        parts = s.Split(sep);
                        if (parts.Length > 2)
                        {
                            gc = Utils.DataAccess.GetGeocache(Core.Geocaches, parts[0]);
                            if (gc != null)
                            {
                                gc.Archived = parts[1] != "0";
                                gc.Available = parts[2] != "0";
                            }
                            else if (parts[1] == "0") //add only none archived
                            {
                                missingGcList.Add(parts[0]);
                            }
                        }
                    }
                }
            }
        }

        private void threadMethod()
        {
            try
            {
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_UPDATING, STR_UPDATING, 1, 0, true))
                {
                    List<string> gcList = new List<string>();
                    if (PluginSettings.Instance.UpdateNL)
                    {
                        updateGeocachesFromGlobalcachingEU("Netherlands", gcList);
                    }
                    if (PluginSettings.Instance.UpdateBE)
                    {
                        updateGeocachesFromGlobalcachingEU("Belgium", gcList);
                    }
                    if (PluginSettings.Instance.UpdateLU)
                    {
                        updateGeocachesFromGlobalcachingEU("Luxembourg", gcList);
                    }

                    if (gcList.Count > 0)
                    {
                        Utils.GeocacheIgnoreSupport.Instance.FilterGeocaches(gcList);
                    }
                    if (gcList.Count == 0)
                    {
                        if (!PluginSettings.Instance.AutomaticDownloadGeocaches)
                        {
                            System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NONEW), Utils.LanguageSupport.Instance.GetTranslation(STR_INFO), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        if (PluginSettings.Instance.AutomaticDownloadGeocaches || System.Windows.Forms.MessageBox.Show(string.Format(Utils.LanguageSupport.Instance.GetTranslation(STR_ASKUPDATE), gcList.Count), Utils.LanguageSupport.Instance.GetTranslation(STR_INFO), System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.Yes)
                        {
                            progress.UpdateProgress(STR_UPDATING, STR_IMPORTING, gcList.Count, 0);

                            using (Utils.API.GeocachingLiveV6 client = new Utils.API.GeocachingLiveV6(Core, string.IsNullOrEmpty(Core.GeocachingComAccount.APIToken)))
                            {
                                int index = 0;
                                int total = gcList.Count;
                                int gcupdatecount = 20;
                                while (gcList.Count > 0)
                                {
                                    GlobalcachingApplication.Utils.API.LiveV6.SearchForGeocachesRequest req = new GlobalcachingApplication.Utils.API.LiveV6.SearchForGeocachesRequest();
                                    req.IsLite = Core.GeocachingComAccount.MemberTypeId == 1;
                                    req.AccessToken = client.Token;
                                    req.CacheCode = new GlobalcachingApplication.Utils.API.LiveV6.CacheCodeFilter();
                                    req.CacheCode.CacheCodes = (from a in gcList select a).Take(gcupdatecount).ToArray();
                                    req.MaxPerPage = gcupdatecount;
                                    req.GeocacheLogCount = 5;
                                    index += req.CacheCode.CacheCodes.Length;
                                    gcList.RemoveRange(0, req.CacheCode.CacheCodes.Length);
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
                                    if (!progress.UpdateProgress(STR_UPDATING, STR_IMPORTING, total, index))
                                    {
                                        break;
                                    }

                                    if (gcList.Count > 0)
                                    {
                                        Thread.Sleep(3000);
                                    }
                                }
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
}
