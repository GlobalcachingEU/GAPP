using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.DataExch
{
    public class Export: Utils.BasePlugin.BaseExportFilter
    {
        protected const string ACTION_EXPORT_ALL = "Export GAPP Data Exchange file|All";
        protected const string ACTION_EXPORT_SELECTED = "Export GAPP Data Exchange file|Selected";

        public const string STR_NOGEOCACHESELECTED = "No geocache selected for export";
        public const string STR_ERROR = "Error";
        
        private string _filename = null;
        private List<Framework.Data.Geocache> _gcList = null;

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_EXPORT_ALL);
            AddAction(ACTION_EXPORT_SELECTED);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));

            return base.Initialize(core);
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_EXPORT_ALL || action==ACTION_EXPORT_SELECTED)
                {
                    _gcList = null;
                    if (action == ACTION_EXPORT_ALL)
                    {
                        _gcList = (from Framework.Data.Geocache a in Core.Geocaches select a).ToList();
                    }
                    else if (action == ACTION_EXPORT_SELECTED)
                    {
                        _gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                    }
                    if (_gcList == null || _gcList.Count == 0)
                    {
                        System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                    else
                    {

                        using (System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog())
                        {
                            dlg.FileName = "";
                            dlg.Filter = "GAPP Data Exchange (*.gde)|*.gde";
                            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                _filename = dlg.FileName;
                                PerformExport();
                            }
                        }
                    }
                }
            }
            return result;
        }


        protected override void ExportMethod()
        {
            DataFile df = new DataFile();
            df.Export(Core, this, _filename, _gcList);
        }
    }
}
