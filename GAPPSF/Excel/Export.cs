using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Excel
{
    public class Export
    {
        public async Task ExportToExcel(string filename, List<Sheet> sheets, List<Core.Data.Geocache> gcList)
        {
            await Task.Run(() =>
            {
                try
                {
                    FileInfo fi = new FileInfo(filename);
                    if (fi.Exists)
                    {
                        fi.Delete();
                    }
                    int totalToDo = sheets.Count * gcList.Count;
                    int totalDone = 0;
                    DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                    using (Utils.ProgressBlock prog = new Utils.ProgressBlock("ExportExcel", "CreatingFile", totalToDo, totalDone))
                    {
                        using (ExcelPackage package = new ExcelPackage(fi))
                        {
                            foreach (Sheet sheet in sheets)
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
                                foreach (Core.Data.Geocache gc in gcList)
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

                                    if (DateTime.Now>=nextUpdate)
                                    {
                                        prog.Update("CreatingFile", totalToDo, totalDone);
                                        nextUpdate = DateTime.Now.AddSeconds(1);
                                    }
                                }
                            }
                            package.Workbook.Properties.Title = "GAPP SF Geocache Export";
                            package.Workbook.Properties.Author = "Globalcaching.eu";
                            package.Workbook.Properties.Comments = "This document has been created by GlobalcachingApplication (GAPP SF)";
                            package.Workbook.Properties.Company = "Globalcaching.eu";
                            package.Save();
                        }
                    }
                }
                catch(Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
            });
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

    }
}
