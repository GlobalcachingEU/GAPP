using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.APIUPD
{
    public class Updater : Utils.BasePlugin.BaseImportFilter
    {
        public const string STR_NOGEOCACHESELECTED = "No geocache selected for update";
        public const string STR_ERROR = "Error";
        public const string STR_UNABLELIVE = "Unable to access the Live API or process its data";
        public const string STR_UPDATINGGEOCACHES = "Updating geocaches...";
        public const string STR_UPDATINGGEOCACHE = "Updating geocache...";

        public const string ACTION_UPDATESTATUS_ALL = "Update status|All";
        public const string ACTION_UPDATESTATUS_SELECTED = "Update status|Selected";
        public const string ACTION_UPDATESTATUS_ACTIVE = "Update status|Active";

        public const string ACTION_UPDATEFULL_ALL = "Refresh|All";
        public const string ACTION_UPDATEFULL_SELECTED = "Refresh|Selected";
        public const string ACTION_UPDATEFULL_ACTIVE = "Refresh|Active";

        private List<Framework.Data.Geocache> _gcList = null;
        private bool _updateStatusOnly = false;
        private string _errormessage = null;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_UPDATESTATUS_ALL);
            AddAction(ACTION_UPDATESTATUS_SELECTED);
            AddAction(ACTION_UPDATESTATUS_ACTIVE);
            AddAction(ACTION_UPDATEFULL_ALL);
            AddAction(ACTION_UPDATEFULL_SELECTED);
            AddAction(ACTION_UPDATEFULL_ACTIVE);

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_UNABLELIVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_UPDATINGGEOCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_UPDATINGGEOCACHE));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_DESELECT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_EXTRADELAY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_EXTRADELAYLOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_MAXLOGS));

            return await base.InitializeAsync(core);
        }

        public override string FriendlyName
        {
            get
            {
                return "Update geocaches";
            }
        }

        public override List<System.Windows.Forms.UserControl> CreateConfigurationPanels()
        {
            List<System.Windows.Forms.UserControl> pnls = base.CreateConfigurationPanels();
            if (pnls == null) pnls = new List<System.Windows.Forms.UserControl>();
            pnls.Add(new SettingsPanel());
            return pnls;
        }


        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            SettingsPanel panel = (from p in configPanels where p.GetType() == typeof(SettingsPanel) select p).FirstOrDefault() as SettingsPanel;
            if (panel != null)
            {
                panel.Apply();
            }
            return true;
        }


        protected override void ImportMethod()
        {
            try
            {
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_UPDATINGGEOCACHES, STR_UPDATINGGEOCACHE, _gcList.Count, 0, true))
                {
                    int totalcount = _gcList.Count;
                    using (Utils.API.GeocachingLiveV6 client = new Utils.API.GeocachingLiveV6(Core, string.IsNullOrEmpty(Core.GeocachingComAccount.APIToken)))
                    {
                        int index = 0;
                        int gcupdatecount;
                        TimeSpan interval = new TimeSpan(0, 0, 0, 2, 100);
                        DateTime prevCall = DateTime.MinValue;
                        bool dodelay;
                        if (_updateStatusOnly)
                        {
                            gcupdatecount = 109;
                            dodelay = (_gcList.Count / gcupdatecount > 30);
                        }
                        else
                        {
                            gcupdatecount = 30;
                            dodelay = (_gcList.Count > 30);
                        }
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
                            if (_updateStatusOnly)
                            {
                                var req = new Utils.API.LiveV6.GetGeocacheStatusRequest();
                                req.AccessToken = client.Token;
                                req.CacheCodes = (from a in _gcList select a.Code).Take(gcupdatecount).ToArray();
                                _gcList.RemoveRange(0, req.CacheCodes.Length);
                                index += req.CacheCodes.Length;
                                prevCall = DateTime.Now;
                                var resp = client.Client.GetGeocacheStatus(req);
                                if (resp.Status.StatusCode == 0 && resp.GeocacheStatuses != null)
                                {
                                    foreach (var gs in resp.GeocacheStatuses)
                                    {
                                        Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(Core.Geocaches, gs.CacheCode);
                                        if (gc != null)
                                        {
                                            gc.DataFromDate = DateTime.Now;
                                            gc.Archived = gs.Archived;
                                            gc.Available = gs.Available;
                                            gc.Name = gs.CacheName;
                                            gc.Title = gs.CacheName;
                                            gc.MemberOnly = gs.Premium;
                                            if (Properties.Settings.Default.DeselectGeocacheAfterUpdate)
                                            {
                                                gc.Selected = false;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (resp.Status.StatusCode != 0)
                                    {
                                        _errormessage = resp.Status.StatusMessage;
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                Utils.API.LiveV6.SearchForGeocachesRequest req = new Utils.API.LiveV6.SearchForGeocachesRequest();
                                req.IsLite = Core.GeocachingComAccount.MemberTypeId == 1;
                                req.AccessToken = client.Token;
                                req.CacheCode = new Utils.API.LiveV6.CacheCodeFilter();
                                req.CacheCode.CacheCodes = (from a in _gcList select a.Code).Take(gcupdatecount).ToArray();
                                req.MaxPerPage = gcupdatecount;
                                req.GeocacheLogCount = 5;
                                index += req.CacheCode.CacheCodes.Length;
                                _gcList.RemoveRange(0, req.CacheCode.CacheCodes.Length);
                                prevCall = DateTime.Now;
                                var resp = client.Client.SearchForGeocaches(req);
                                if (resp.Status.StatusCode == 0 && resp.Geocaches != null)
                                {
                                    Utils.API.Import.AddGeocaches(Core, resp.Geocaches);
                                    if (Properties.Settings.Default.DeselectGeocacheAfterUpdate)
                                    {
                                        foreach (var g in resp.Geocaches)
                                        {
                                            Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(Core.Geocaches, g.Code);
                                            if (gc != null)
                                            {
                                                gc.Selected = false;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    _errormessage = resp.Status.StatusMessage;
                                    break;
                                }
                            }
                            if (!progress.UpdateProgress(STR_UPDATINGGEOCACHES, STR_UPDATINGGEOCACHE, totalcount, index))
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                _errormessage = e.Message;
            }
        }


        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.LiveAPI;
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result && 
                (
                 action == ACTION_UPDATESTATUS_ALL ||
                 action == ACTION_UPDATESTATUS_ACTIVE ||
                 action == ACTION_UPDATESTATUS_SELECTED ||
                 action == ACTION_UPDATEFULL_ALL ||
                 action == ACTION_UPDATEFULL_ACTIVE ||
                 action == ACTION_UPDATEFULL_SELECTED
                ))
            {
                try
                {
                    //get from goundspeak
                    if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, false))
                    {
                        _gcList = null;
                        _updateStatusOnly = (action == ACTION_UPDATESTATUS_ALL ||
                                            action == ACTION_UPDATESTATUS_ACTIVE ||
                                            action == ACTION_UPDATESTATUS_SELECTED);
                        if (action == ACTION_UPDATESTATUS_ALL || action == ACTION_UPDATEFULL_ALL)
                        {
                            _gcList = (from Framework.Data.Geocache a in Core.Geocaches select a).ToList();
                        }
                        else if (action == ACTION_UPDATESTATUS_SELECTED || action == ACTION_UPDATEFULL_SELECTED)
                        {
                            _gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                        }
                        else if (Core.ActiveGeocache!=null)
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
