using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public class ImportByRadius : Utils.BasePlugin.BaseImportFilter
    {
        public const string STR_IMPORTINGCACHES = "Importing opencaching geocaches...";
        public const string STR_ERROR = "Error";

        public const string ACTION_IMPORT = "Import geocaches within radius";

        private string _errormessage = null;
        private string _filter = null;
        private double _radiusKm =  30.0;
        private Framework.Data.Location _centerLoc = null;

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_IMPORT);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportByRadiusForm.STR_AREA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportByRadiusForm.STR_KM));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportByRadiusForm.STR_LOCATION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportByRadiusForm.STR_MILES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportByRadiusForm.STR_MODIFIEDSINCE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportByRadiusForm.STR_OK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportByRadiusForm.STR_RADIUS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportByRadiusForm.STR_TITLE));

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
                    List<string> gcList = OKAPIService.GetGeocachesWithinRadius(SiteManager.Instance.ActiveSite, _centerLoc, _radiusKm, _filter);
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
                    using (ImportByRadiusForm dlg = new ImportByRadiusForm(Core))
                    {
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            _filter = dlg.Filter;
                            _radiusKm = dlg.RadiusKm;
                            _centerLoc = dlg.Center;
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
