using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.LiveAPILogGeocaches
{
    public class CGeoGeocacheVisits
    {
        public void SelectAndLog()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.Filter = "c:geo visits files|*.txt"; // Filter files by extension 

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                try
                {
                    List<GeocacheVisitsItem> lst = processGeocacheVisitsFile(System.IO.File.ReadAllLines(dlg.FileName));
                    LogWindow dlg2 = new LogWindow(lst);
                    dlg2.ShowDialog();
                }
                catch (Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
            }
        }

        private List<GeocacheVisitsItem> processGeocacheVisitsFile(string[] lines)
        {
            List<GeocacheVisitsItem> result = new List<GeocacheVisitsItem>();
            try
            {
                int i = 0;
                while (i < lines.Length)
                {
                    string s = lines[i];
                    string[] parts = s.Replace("\0", "").Split(new char[] { ',' }, 4);
                    if (parts.Length == 4)
                    {
                        DateTime dt;
                        if (parts[0].StartsWith("GC") && DateTime.TryParse(parts[1], out dt))
                        {
                            GeocacheVisitsItem vi = new GeocacheVisitsItem();
                            vi.Code = parts[0];
                            vi.LogDate = dt.ToLocalTime();
                            vi.LogType = Utils.DataAccess.GetLogType(parts[2]);

                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine(parts[3].Replace("\"", ""));

                            //read till next
                            i++;
                            while (i < lines.Length)
                            {
                                s = lines[i];
                                parts = s.Replace("\0", "").Split(new char[] { ',' }, 4);
                                if (parts.Length == 4)
                                {
                                    if (parts[0].StartsWith("GC") && DateTime.TryParse(parts[1], out dt))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        sb.AppendLine(s.Trim().Replace("\"", ""));
                                        i++;
                                    }
                                }
                                else
                                {
                                    sb.AppendLine(s.Trim().Replace("\"", ""));
                                    i++;
                                }
                            }
                            vi.Comment = sb.ToString();
                            result.Add(vi);
                        }
                        else
                        {
                            i++;
                        }
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            catch (Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
            return result;
        }

    }
}
