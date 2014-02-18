using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.LiveAPILogGeocaches
{
    public class GarminGeocacheVisits
    {
        public void SelectAndLog()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.Filter = "geocache_visits.txt|geocache_visits.txt"; // Filter files by extension 

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
                foreach (string s in lines)
                {
                    string[] parts = s.Replace("\0", "").Split(new char[] { ',' }, 4);
                    if (parts.Length == 4)
                    {
                        GeocacheVisitsItem vi = new GeocacheVisitsItem();
                        vi.Code = parts[0];
                        vi.LogDate = DateTime.Parse(parts[1]).ToLocalTime();
                        vi.LogType = Utils.DataAccess.GetLogType(parts[2]);
                        vi.Comment = parts[3].Replace("\"", "");

                        if (vi.Code.IndexOf("GC") > 0)
                        {
                            vi.Code = vi.Code.Substring(vi.Code.IndexOf("GC"));
                        }
                        result.Add(vi);
                    }
                }
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
            return (from a in result select a).OrderBy(x => x.LogDate).ToList();
        }


    }
}
