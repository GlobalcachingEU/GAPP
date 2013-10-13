using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public class ImportByGCCodes : Utils.BasePlugin.BaseImportFilter
    {
        public const string STR_IMPORTINGCACHES = "Importing opencaching geocaches...";
        public const string STR_ERROR = "Error";

        public const string ACTION_IMPORT = "Import geocaches by codes";

        private string _errormessage = null;
        private List<string> _gcList = null;

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_IMPORT);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportByGCCodesForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportByGCCodesForm.STR_LIST));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImportByGCCodesForm.STR_OK));

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
            try
            {
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_IMPORTINGCACHES, STR_IMPORTINGCACHES, _gcList.Count, 0, true))
                {
                    int gcupdatecount = 1;
                    int max = _gcList.Count;
                    while (_gcList.Count > 0)
                    {
                        List<string> lcs = _gcList.Take(gcupdatecount).ToList();
                        _gcList.RemoveRange(0, lcs.Count);
                        List<OKAPIService.Geocache> caches = OKAPIService.GetGeocaches(SiteManager.Instance.ActiveSite, lcs);
                        Import.AddGeocaches(Core, caches);

                        if (!progress.UpdateProgress(STR_IMPORTINGCACHES, STR_IMPORTINGCACHES, max, max - _gcList.Count))
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

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result && action == ACTION_IMPORT)
            {
                if (SiteManager.Instance.CheckAPIAccess())
                {
                    using (ImportByGCCodesForm dlg = new ImportByGCCodesForm())
                    {
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            _gcList = dlg.GCCodes;
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
