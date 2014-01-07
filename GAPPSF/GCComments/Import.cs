using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace GAPPSF.GCComments
{
    public class Import
    {
        public async Task ImportGCComments(Core.Storage.Database db, bool importMissing)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".gcc"; // Default file extension
            dlg.Filter = "*.gcc|*.gcc|*.*|*.*"; // Filter files by extension 

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document
                await Task.Run(() =>
                {
                    using (Utils.DataUpdater upd = new Utils.DataUpdater(db))
                    {
                        try
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.Load(dlg.FileName);

                            XmlNodeList nl = doc.SelectNodes("/gccomment/comment");
                            if (nl != null)
                            {
                                if (importMissing)
                                {
                                    List<string> gcList = new List<string>();
                                    foreach (XmlNode n in nl)
                                    {
                                        string gcCode = n.SelectSingleNode("gccode").InnerText;
                                        Core.Data.Geocache gc = db.GeocacheCollection.GetGeocache(gcCode);
                                        if (gc == null)
                                        {
                                            gcList.Add(gcCode);
                                        }
                                    }
                                    if (gcList.Count > 0)
                                    {
                                        Core.Settings.Default.filterIgnoredGeocacheCodes(gcList);
                                    }
                                    if (gcList.Count > 0)
                                    {
                                        LiveAPI.Import.ImportGeocaches(db, gcList);
                                    }
                                }
                                foreach (XmlNode n in nl)
                                {
                                    string gcCode = n.SelectSingleNode("gccode").InnerText;
                                    Core.Data.Geocache gc = db.GeocacheCollection.GetGeocache(gcCode);
                                    if (gc != null)
                                    {
                                        gc.Notes = HttpUtility.HtmlEncode(n.SelectSingleNode("content").InnerText).Replace("\r", "").Replace("\n", "<br />");
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Core.ApplicationData.Instance.Logger.AddLog(this, e);
                        }
                    }
                });
            }

        }
    }
}
