using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GlobalcachingApplication.Plugins.ExportHTML
{
    public class Export : Utils.BasePlugin.BaseExportFilter
    {
        public const string ACTION_EXPORT_ALL = "Export to HTML|All";
        public const string ACTION_EXPORT_SELECTED = "Export to HTML|Selected";

        public const string STR_NOGEOCACHESELECTED = "No geocache selected for export";
        public const string STR_ERROR = "Error";
        public const string STR_EXPORTINGPOI = "Exporting to HTML...";
        public const string STR_CREATINGFILE = "Creating file...";

        private List<Framework.Data.Geocache> _gcList = null;
        private List<ExportForm.Sheet> _sheets = null;

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_EXPORT_ALL);
            AddAction(ACTION_EXPORT_SELECTED);

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }
            if (Properties.Settings.Default.ExportFields == null)
            {
                Properties.Settings.Default.ExportFields = new System.Collections.Specialized.StringCollection();
                Properties.Settings.Default.Save();
            }

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
            return base.Initialize(core);
        }

        protected override void ExportMethod()
        {
            /*
             * index.html -> list of all sheets with link
             * sheet1.html, sheet2.html...sheetN.html -> header contains list of all sheets with link
             * */
            int totalToDo = _sheets.Count * _gcList.Count;
            int totalDone = 0;
            StringBuilder sb = new StringBuilder();
            using (Utils.ProgressBlock prog = new Utils.ProgressBlock(this, STR_EXPORTINGPOI, STR_CREATINGFILE, totalToDo, totalDone))
            {
                string fn = Path.Combine(Properties.Settings.Default.FilePath, "index.html");
                if (File.Exists(fn))
                {
                    File.Delete(fn);
                }
                sb.AppendLine("<html><header><title>GAPP HTML Export</title></header><body><h1><center>Index</center></h1>");
                for (int i = 0; i < _sheets.Count; i++ )
                {
                    sb.AppendFormat("<a href=\"sheet{1}.html\">{0}</a><br />", HttpUtility.HtmlEncode(_sheets[i].Name), i);
                }
                sb.AppendLine("</body></html>");
                File.WriteAllText(fn, sb.ToString());

                for (int i = 0; i < _sheets.Count; i++)
                {
                    fn = Path.Combine(Properties.Settings.Default.FilePath, string.Format("sheet{0}.html", i));
                    if (File.Exists(fn))
                    {
                        File.Delete(fn);
                    }
                    sb.Length = 0;

                    sb.AppendLine(string.Format("<html><header><title>GAPP HTML Export - {0}</title></header><body><h1><center>{0}</center></h1>", HttpUtility.HtmlEncode(_sheets[i].Name)));
                    for (int c = 0; c < _sheets.Count; c++)
                    {
                        sb.AppendFormat("<a href=\"sheet{1}.html\">{0}</a><br />", HttpUtility.HtmlEncode(_sheets[c].Name), c);
                    }

                    sb.AppendLine("<br /><br /><table><tr><td></td>");
                    foreach (PropertyItem pi in _sheets[i].SelectedItems)
                    {
                        sb.AppendFormat("<td><strong>{0}</strong></td>", HttpUtility.HtmlEncode(pi.Name));
                    }
                    sb.AppendLine("</tr>");
                    int index = 0;
                    foreach (Framework.Data.Geocache gc in _gcList)
                    {
                        index++;
                        sb.AppendLine("<tr>");
                        sb.AppendFormat("<td>{0}</td>", index);
                        foreach (PropertyItem pi in _sheets[i].SelectedItems)
                        {
                            object o = pi.GetValue(gc);
                            if (o==null)
                            {
                                sb.Append("<td></td>");
                            }
                            if (pi is PropertyItemDescriptionHTML)
                            {
                                sb.AppendFormat("<td><div>{0}/<div></td>", o);
                            }
                            else if (pi is PropertyItemDescriptionText)
                            {
                                sb.AppendFormat("<td>{0}</td>", HttpUtility.HtmlEncode(o.ToString()).Replace("\r", "<br />"));
                            }
                            else
                            {
                                sb.AppendFormat("<td>{0}</td>", o);
                            }
                        }
                        sb.AppendLine("</tr>");
                        totalDone++;

                        if (totalDone % 100 == 0)
                        {
                            prog.UpdateProgress(STR_EXPORTINGPOI, STR_CREATINGFILE, totalToDo, totalDone);
                        }
                    }

                    sb.AppendLine("</table></body></html>");
                    File.WriteAllText(fn, sb.ToString());
                    try
                    {
                        System.Diagnostics.Process.Start(Path.Combine(Properties.Settings.Default.FilePath, "index.html"));
                    }
                    catch
                    {

                    }
                }
            }
        }

        public override bool Action(string action)
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
                                PerformExport();
                            }
                        }

                    }
                }
            }
            return result;
        }

    }
}
