using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GAPPSF.HTML
{
    public class Export
    {
        public async Task ExportToHTML(string filename, List<Sheet> sheets, List<Core.Data.Geocache> gcList)
        {
            await Task.Run(() =>
            {
                try
                {
                    /*
                     * index.html -> list of all sheets with link
                     * sheet1.html, sheet2.html...sheetN.html -> header contains list of all sheets with link
                     * */
                    int totalToDo = sheets.Count * gcList.Count;
                    int totalDone = 0;
                    DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                    StringBuilder sb = new StringBuilder();
                    using (Utils.ProgressBlock prog = new Utils.ProgressBlock("ExportHTML", "CreatingFile", totalToDo, totalDone))
                    {
                        string fn = Path.Combine(Core.Settings.Default.HTMLTargetPath, "index.html");
                        if (File.Exists(fn))
                        {
                            File.Delete(fn);
                        }
                        sb.AppendLine("<html><header><meta http-equiv='Content-Type' content='text/html;charset=UTF-8'><title>GAPP HTML Export</title></header><body><h1><center>Index</center></h1>");
                        for (int i = 0; i < sheets.Count; i++)
                        {
                            sb.AppendFormat("<a href=\"sheet{1}.html\">{0}</a><br />", HttpUtility.HtmlEncode(sheets[i].Name), i);
                        }
                        sb.AppendLine("</body></html>");
                        File.WriteAllText(fn, sb.ToString());

                        for (int i = 0; i < sheets.Count; i++)
                        {
                            fn = Path.Combine(Core.Settings.Default.HTMLTargetPath, string.Format("sheet{0}.html", i));
                            if (File.Exists(fn))
                            {
                                File.Delete(fn);
                            }
                            sb.Length = 0;

                            sb.AppendLine(string.Format("<html><header><meta http-equiv='Content-Type' content='text/html;charset=UTF-8'><title>GAPP HTML Export - {0}</title></header><body><h1><center>{0}</center></h1>", HttpUtility.HtmlEncode(sheets[i].Name)));
                            for (int c = 0; c < sheets.Count; c++)
                            {
                                sb.AppendFormat("<a href=\"sheet{1}.html\">{0}</a><br />", HttpUtility.HtmlEncode(sheets[c].Name), c);
                            }

                            sb.AppendLine("<br /><br /><table><tr><td></td>");
                            foreach (PropertyItem pi in sheets[i].SelectedItems)
                            {
                                sb.AppendFormat("<td><strong>{0}</strong></td>", HttpUtility.HtmlEncode(pi.Name));
                            }
                            sb.AppendLine("</tr>");
                            int index = 0;
                            foreach (var gc in gcList)
                            {
                                index++;
                                sb.AppendLine("<tr>");
                                sb.AppendFormat("<td>{0}</td>", index);
                                foreach (PropertyItem pi in sheets[i].SelectedItems)
                                {
                                    object o = pi.GetValue(gc);
                                    if (o == null)
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

                                if (DateTime.Now >= nextUpdate)
                                {
                                    prog.Update("CreatingFile", totalToDo, totalDone);
                                    nextUpdate = DateTime.Now.AddSeconds(1);
                                }
                            }

                            sb.AppendLine("</table></body></html>");
                            File.WriteAllText(fn, sb.ToString());
                            try
                            {
                                System.Diagnostics.Process.Start(Path.Combine(Core.Settings.Default.HTMLTargetPath, "index.html"));
                            }
                            catch (Exception e)
                            {
                                Core.ApplicationData.Instance.Logger.AddLog(this, e);
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
            });
        }

    }
}
