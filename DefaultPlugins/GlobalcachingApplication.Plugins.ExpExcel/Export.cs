using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using System.IO;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.ExpExcel
{
    public class Export : Utils.BasePlugin.BaseExportFilter
    {
        public const string ACTION_EXPORT_ALL = "Export to Excel|All";
        public const string ACTION_EXPORT_SELECTED = "Export to Excel|Selected";

        public const string STR_NOGEOCACHESELECTED = "No geocache selected for export";
        public const string STR_ERROR = "Error";
        public const string STR_EXPORTINGPOI = "Exporting to Excel...";
        public const string STR_CREATINGFILE = "Creating file...";

        private List<Framework.Data.Geocache> _gcList = null;
        private List<ExportForm.Sheet> _sheets = null;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            var p = new PluginSettings(core);

            AddAction(ACTION_EXPORT_ALL);
            AddAction(ACTION_EXPORT_SELECTED);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_EXPORTINGPOI));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CREATINGFILE));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportForm.STR_ADD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportForm.STR_DELETE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportForm.STR_EXPORT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportForm.STR_FIELDS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportForm.STR_FILE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportForm.STR_SHEETS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ExportForm.STR_TITLE));

            PropertyItem ppi;
            ppi = new PropertyItemCode(core);
            ppi = new PropertyItemName(core);
            ppi = new PropertyItemPublished(core);
            ppi = new PropertyItemLat(core);
            ppi = new PropertyItemLon(core);
            ppi = new PropertyItemCoordinate(core);
            ppi = new PropertyItemAvailable(core);
            ppi = new PropertyItemArchived(core);
            ppi = new PropertyItemCountry(core);
            ppi = new PropertyItemState(core);
            ppi = new PropertyItemMunicipality(core);
            ppi = new PropertyItemCity(core);
            ppi = new PropertyItemType(core);
            ppi = new PropertyItemPlacedBy(core);
            ppi = new PropertyItemOwner(core);
            ppi = new PropertyItemContainer(core);
            ppi = new PropertyItemTerrain(core);
            ppi = new PropertyItemDifficulty(core);
            ppi = new PropertyItemDescriptionText(core);
            ppi = new PropertyItemDescriptionHTML(core);
            ppi = new PropertyItemUrl(core);
            ppi = new PropertyItemMemberOnly(core);
            ppi = new PropertyItemCustomLat(core);
            ppi = new PropertyItemCustomLon(core);
            ppi = new PropertyItemCustomCoordinate(core);
            ppi = new PropertyItemAutoCoordinate(core);
            ppi = new PropertyItemFavorites(core);
            ppi = new PropertyItemPersonalNotes(core);
            ppi = new PropertyItemFlagged(core);
            ppi = new PropertyItemFound(core);
            ppi = new PropertyItemFoundDate(core);
            ppi = new PropertyItemHints(core);
            ppi = new PropertyItemGCVote(core);
#if DEBUG
            ppi = new PropertyItemRDx(core);
            ppi = new PropertyItemRDy(core);
            ppi = new PropertyItemEnvelopAreaOther(core);
            ppi = new PropertyItemInAreaOther(core);
            ppi = new PropertyItemGlobalcachingUrl(core);
#endif
            return await base.InitializeAsync(core);
        }

        protected override void ExportMethod()
        {
            FileInfo fi = new FileInfo(PluginSettings.Instance.FilePath);
            if (fi.Exists)
            {
                fi.Delete();
            }
            int totalToDo = _sheets.Count * _gcList.Count;
            int totalDone = 0;
            using (Utils.ProgressBlock prog = new Utils.ProgressBlock(this, STR_EXPORTINGPOI, STR_CREATINGFILE, totalToDo, totalDone))
            {
                using (ExcelPackage package = new ExcelPackage(fi))
                {
                    foreach (ExportForm.Sheet sheet in _sheets)
                    {
                        // add a new worksheet to the empty workbook
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sheet.Name);
                        //Add the headers
                        int col = 1;
                        foreach (PropertyItem pi in sheet.SelectedItems)
                        {
                            worksheet.Cells[1, col].Value = pi.Name;
                            col++;
                        }
                        int row = 2;
                        foreach (Framework.Data.Geocache gc in _gcList)
                        {
                            col = 1;
                            foreach (PropertyItem pi in sheet.SelectedItems)
                            {
                                /*
                                object o = pi.GetValue(gc);
                                if (o != null && o.GetType() == typeof(string))
                                {
                                    o = validateXml((string)o);
                                }
                                worksheet.Cells[row, col].Value = o;
                                 * */
                                /*
                                object o = pi.GetValue(gc);
                                if (o != null)
                                {
                                    string s = o.ToString();
                                    worksheet.Cells[row, col].Value = s;
                                }
                                */
                                worksheet.Cells[row, col].Value = pi.GetValue(gc);
                                col++;
                            }
                            row++;
                            totalDone++;

                            if (totalDone % 100 == 0)
                            {
                                prog.UpdateProgress(STR_EXPORTINGPOI, STR_CREATINGFILE, totalToDo, totalDone);
                            }
                        }
                    }
                    package.Workbook.Properties.Title = "GAPP Geocache Export";
                    package.Workbook.Properties.Author = "Globalcaching.eu";
                    package.Workbook.Properties.Comments = "This document has been created by GlobalcachingApplication (GAPP)";
                    package.Workbook.Properties.Company = "Globalcaching.eu";
                    package.Save();
                }
            }
        }

        private string validateXml(string doc)
        {
            string result = doc;
            result = result.Replace("&auml;", "&#228;");
            result = result.Replace("&uuml;", "&#252;");
            result = result.Replace("&szlig;", "&#223;");
            result = result.Replace("&Auml;", "&#196;");
            result = result.Replace("&Ouml;", "&#214;");
            result = result.Replace("&Uuml;", "&#220;");
            result = result.Replace("&nbsp;", "&#160;");
            result = result.Replace("&Agrave;", "&#192;");
            result = result.Replace("&Egrave;", "&#200;");
            result = result.Replace("&Eacute;", "&#201;");
            result = result.Replace("&Ecirc;", "&#202;");
            result = result.Replace("&egrave;", "&#232;");
            result = result.Replace("&eacute;", "&#233;");
            result = result.Replace("&ecirc;", "&#234;");
            result = result.Replace("&agrave;", "&#224;");
            result = result.Replace("&iuml;", "&#239;");
            result = result.Replace("&ugrave;", "&#249;");
            result = result.Replace("&ucirc;", "&#251;");
            result = result.Replace("&uuml;", "&#252;");
            result = result.Replace("&ccedil;", "&#231;");
            result = result.Replace("&AElig;", "&#198;");
            result = result.Replace("&aelig;", "&#330;");
            result = result.Replace("&OElig;", "&#338;");
            result = result.Replace("&oelig;", "&#339;");
            result = result.Replace("&euro;", "&#8364;");
            result = result.Replace("&laquo;", "&#171;");
            result = result.Replace("&raquo;", "&#187;");

            result = result.Replace("&#xE4;", "&#228;");
            result = result.Replace("&#xE5;", "&#229;");
            result = result.Replace("&#xF6;", "&#246;");
            result = result.Replace("&#xFC;", "&#252;");

            try
            {
                int pos;
                pos = result.IndexOf("&#x");
                while (pos >= 0)
                {
                    //for now, just forget the characters
                    //todo: convert to decimal
                    string subs = result.Substring(pos, result.IndexOf(';', pos) - pos + 1);
                    result = result.Replace(subs, "");
                    pos = result.IndexOf("&#x");
                }
            }
            catch
            {
            }

            return result;
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

                        using (ExportForm dlg = new ExportForm())
                        {
                            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                _sheets = dlg.Sheets;
                                await PerformExport();
                            }
                        }

                    }
                }
            }
            return result;
        }
    }
}
