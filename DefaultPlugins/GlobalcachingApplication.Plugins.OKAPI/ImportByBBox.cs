using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public class ImportByBBox : Utils.BasePlugin.BaseImportFilter
    {
        public const string STR_IMPORTINGCACHES = "Importing opencaching geocaches...";
        public const string STR_ERROR = "Error";

        public const string ACTION_IMPORT = "Import geocaches in area on map";

        private string _errormessage = null;
        private double _minLat;
        private double _minLon;
        private double _maxLat;
        private double _maxLon;


        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_IMPORT);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectByAreaForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectByAreaForm.STR_SELECTWHOLEAREA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectByAreaForm.STR_GO));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectByAreaForm.STR_LOCATION));

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
            using (Utils.ProgressBlock blockprogress = new Utils.ProgressBlock(this, STR_IMPORTINGCACHES, STR_IMPORTINGCACHES, 1, 0))
            {
                try
                {
                    //first get a list of geocache codes
                    List<string> gcList = OKAPIService.GetGeocachesInBBox(SiteManager.Instance.ActiveSite, _minLat, _minLon, _maxLat, _maxLon);
                    using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_IMPORTINGCACHES, STR_IMPORTINGCACHES, gcList.Count, 0, true))
                    {
                        int gcupdatecount = 1;
                        int max = gcList.Count;
                        while (gcList.Count > 0)
                        {
                            List<string> lcs = gcList.Take(gcupdatecount).ToList();
                            gcList.RemoveRange(0, lcs.Count);
                            List<OKAPIService.Geocache> caches = OKAPIService.GetGeocaches(SiteManager.Instance.ActiveSite, lcs);
                            Import.AddGeocaches(Core, caches);

                            if (!progress.UpdateProgress(STR_IMPORTINGCACHES, STR_IMPORTINGCACHES, max, max - gcList.Count))
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
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result && action == ACTION_IMPORT)
            {
                if (SiteManager.Instance.CheckAPIAccess())
                {
                    using (SelectByAreaForm dlg = new SelectByAreaForm(Core))
                    {
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            _minLat = dlg.MinLat;
                            _minLon = dlg.MinLon;
                            _maxLat = dlg.MaxLat;
                            _maxLon = dlg.MaxLon;

                            PerformImport();
                            if (!string.IsNullOrEmpty(_errormessage))
                            {
                                System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            return result;
        }

    }
}
