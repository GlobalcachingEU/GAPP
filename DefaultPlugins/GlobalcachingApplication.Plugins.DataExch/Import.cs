using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.DataExch
{
    public class Import: Utils.BasePlugin.BaseImportFilter
    {
        protected const string ACTION_IMPORT = "Import GAPP Data Exchange file";
        private string _filename = null;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_IMPORT);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(DataFile.STR_EXPORT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(DataFile.STR_EXPORT_CACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(DataFile.STR_IMPORT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(DataFile.STR_IMPORT_CACHES));

            return await base.InitializeAsync(core);
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_IMPORT)
                {
                    using (System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog())
                    {
                        dlg.FileName = "";
                        dlg.Filter = "GAPP Data Exchange (*.gde)|*.gde";
                        dlg.Multiselect = false;
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            _filename = dlg.FileName;
                            PerformImport();
                        }
                    }
                }
            }
            return result;
        }

        public bool AddImportedGeocache(Framework.Data.Geocache gc)
        {
            return AddGeocache(gc, null);
        }

        public bool AddImportedLog(Framework.Data.Log l)
        {
            return base.AddLog(l);
        }

        public bool AddImportedWaypoint(Framework.Data.Waypoint wp)
        {
            return base.AddWaypoint(wp);
        }

        public bool AddImportedUserWaypoint(Framework.Data.UserWaypoint wp)
        {
            return base.AddUserWaypoint(wp);
        }

        public bool AddImportedLogImage(Framework.Data.LogImage li)
        {
            return base.AddLogImage(li);
        }

        protected override void ImportMethod()
        {
            DataFile df = new DataFile();
            df.Import(Core, this, _filename);
        }
    }
}
