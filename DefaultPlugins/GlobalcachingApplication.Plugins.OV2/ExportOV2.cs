using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.OV2
{
    public class ExportOV2 : Utils.BasePlugin.BaseExportFilter
    {
        public const string ACTION_EXPORT_ALL = "Export OV2|All";
        public const string ACTION_EXPORT_SELECTED = "Export OV2|Selected";

        public const string STR_NOGEOCACHESELECTED = "No geocache selected for export";
        public const string STR_ERROR = "Error";
        public const string STR_EXPORTINGOV2 = "Exporting OV2...";
        public const string STR_CREATINGFILE = "Creating file...";

        List<Framework.Data.Geocache> _gcList = null;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            var p = new PluginSettings(core);

            AddAction(ACTION_EXPORT_ALL);
            AddAction(ACTION_EXPORT_SELECTED);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportOV2Form.STR_CACHETYPE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportOV2Form.STR_CODE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportOV2Form.STR_CONTAINER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportOV2Form.STR_COORDS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportOV2Form.STR_DIFFICULTY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportOV2Form.STR_FAVORITES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportOV2Form.STR_FILE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportOV2Form.STR_HINTS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportOV2Form.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportOV2Form.STR_NOTE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportOV2Form.STR_OK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportOV2Form.STR_OWNER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportOV2Form.STR_TERRAIN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportOV2Form.STR_TITLE));

            return await base.InitializeAsync(core);
        }

        public async override Task<bool> ActionAsync(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_EXPORT_ALL || action == ACTION_EXPORT_SELECTED)
                {
                    if (action == ACTION_EXPORT_ALL)
                    {
                        _gcList = (from Framework.Data.Geocache a in Core.Geocaches select a).ToList();
                    }
                    else if (action == ACTION_EXPORT_SELECTED)
                    {
                        _gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                    }
                    else
                    {
                        if (Core.ActiveGeocache != null)
                        {
                            _gcList = new List<Framework.Data.Geocache>();
                            _gcList.Add(Core.ActiveGeocache);
                        }
                    }
                    if (_gcList == null || _gcList.Count == 0)
                    {
                        System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                    else
                    {
                        using (ExportOV2Form dlg = new ExportOV2Form())
                        {
                            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                await PerformExport();
                            }
                        }
                    }
                }
            }
            return result;
        }

        protected override void ExportMethod()
        {
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_EXPORTINGOV2, STR_CREATINGFILE, _gcList.Count, 0))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(PluginSettings.Instance.LastSavedFile))
                {
                    int block = 0;
                    int index = 0;
                    foreach (Framework.Data.Geocache gc in _gcList)
                    {
                        StringBuilder sb = new StringBuilder();
                        if (PluginSettings.Instance.gcCoord)
                        {
                            if (sb.Length > 0) sb.Append(",");
                            sb.Append(Utils.Conversion.GetCoordinatesPresentation(gc.Lat, gc.Lon));
                        }
                        if (PluginSettings.Instance.gcCode)
                        {
                            if (sb.Length > 0) sb.Append(",");
                            sb.Append(gc.Code ?? "");
                        }
                        if (PluginSettings.Instance.gcCacheType)
                        {
                            if (sb.Length > 0) sb.Append(",");
                            sb.Append(gc.GeocacheType.Name);
                        }
                        if (PluginSettings.Instance.gcName)
                        {
                            if (sb.Length > 0) sb.Append(",");
                            sb.Append(gc.Name ?? "");
                        }
                        if (PluginSettings.Instance.gcContainer)
                        {
                            if (sb.Length > 0) sb.Append(",");
                            sb.Append(gc.Container.Name);
                        }
                        if (PluginSettings.Instance.gcHint)
                        {
                            if (sb.Length > 0) sb.Append(",");
                            sb.Append(gc.EncodedHints ?? "");
                        }
                        if (PluginSettings.Instance.gcFavorites)
                        {
                            if (sb.Length > 0) sb.Append(",");
                            sb.Append(gc.Favorites.ToString());
                        }
                        if (PluginSettings.Instance.gcOwner)
                        {
                            if (sb.Length > 0) sb.Append(",");
                            sb.Append(gc.Owner ?? "");
                        }
                        if (PluginSettings.Instance.gcDifficulty)
                        {
                            if (sb.Length > 0) sb.Append(",");
                            sb.Append(gc.Difficulty.ToString("0.#"));
                        }
                        if (PluginSettings.Instance.gcTerrain)
                        {
                            if (sb.Length > 0) sb.Append(",");
                            sb.Append(gc.Terrain.ToString("0.#"));
                        }

                        byte[] data;
                        string s = sb.ToString();
                        data = new byte[s.Length];
                        for (int i=0; i<s.Length; i++)
                        {
                            data[i] = BitConverter.GetBytes(s[i])[0];
                        }
                        //data = System.Text.UnicodeEncoding.UTF8.GetBytes(sb.ToString());
                        fs.WriteByte(2);
                        Int32 len = 13 + data.Length+1;
                        byte[] arr;
                        arr = BitConverter.GetBytes(len);
                        fs.Write(arr, 0, 4);
                        Int32 x;
                        Int32 y;
                        if (gc.CustomLat != null && gc.CustomLon != null)
                        {
                            x = (Int32)(gc.CustomLon * 100000.0);
                            y = (Int32)(gc.CustomLat * 100000.0);
                        }
                        else
                        {
                            x = (Int32)(gc.Lon * 100000.0);
                            y = (Int32)(gc.Lat * 100000.0);
                        }
                        fs.Write(BitConverter.GetBytes(x), 0, 4);
                        fs.Write(BitConverter.GetBytes(y), 0, 4);
                        fs.Write(data, 0, data.Length);
                        fs.WriteByte(0);

                        block++;
                        index++;
                        if (block >= 50)
                        {
                            block = 0;
                            progress.UpdateProgress(STR_EXPORTINGOV2, STR_CREATINGFILE, _gcList.Count, index);
                        }
                    }
                }
            }
        }
    }
}
