using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using GlobalcachingApplication.Framework;
using GlobalcachingApplication.Framework.Data;
using GlobalcachingApplication.Framework.Interfaces;
using GlobalcachingApplication.Utils;
using GlobalcachingApplication.Utils.BasePlugin;
using System.Threading;

class Script
{
    private string _errormessage = null;
    private ManualResetEvent _actionReady;
    private Plugin _plugin;
    private ICore _core;

    public static bool Run(Plugin plugin, ICore core)
    {
        Script scr = new Script();
        scr.RunMethod(plugin, core);
        return true;
    }

    public void RunMethod(Plugin plugin, ICore core)
    {
        _plugin = plugin;
        _core = core;

        using (FrameworkDataUpdater updater = new FrameworkDataUpdater(core))
        {
            _actionReady = new ManualResetEvent(false);
            Thread thrd = new Thread(new ThreadStart(this.threadMethod));
            thrd.Start();
            while (!_actionReady.WaitOne(100))
            {
                System.Windows.Forms.Application.DoEvents();
            }
            thrd.Join();
            if (!string.IsNullOrEmpty(_errormessage))
            {
                System.Windows.Forms.MessageBox.Show(_errormessage, LanguageSupport.Instance.GetTranslation(LanguageSupport.Instance.GetTranslation("Error")), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }

    private void threadMethod()
    {
        try
        {
            using (ProgressBlock progress = new ProgressBlock(_plugin, "Update archived geocaches", "Downloading data from globalcaching.eu", 1, 0))
            {
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    byte[] data = wc.DownloadData(string.Format("https://www.4geocaching.eu/Service/Archived.aspx?country=Belgium&prefix=GC&token={0}", System.Web.HttpUtility.UrlEncode(_core.GeocachingComAccount.APIToken)));
                    string sdoc = CompressText.UnzipText(data);
                    //_core.DebugLog(DebugLogLevel.Info, _plugin, null, sdoc);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(sdoc);
                    XmlElement root = doc.DocumentElement;
                    XmlNodeList strngs = root.SelectNodes("wp");
                    if (strngs != null)
                    {
                        progress.UpdateProgress("Update archived geocaches", "Updating geocaches...", strngs.Count, 0);
                        Geocache gc;
                        int index = 0;
                        foreach (XmlNode sn in strngs)
                        {
                            gc = DataAccess.GetGeocache(_core.Geocaches, sn.InnerText);
                            if (gc != null)
                            {
                                gc.Archived = true;
                                gc.Available = false;
                            }
                            index++;
                            if (index % 50 == 0)
                            {
                                //_core.DebugLog(DebugLogLevel.Info, _plugin, null, index.ToString());
                                progress.UpdateProgress("Update archived geocaches", "Updating geocaches...", strngs.Count, index);
                            }
                        }
                    }

                }
            }
        }
        catch(Exception e)
        {
            _errormessage = e.Message;
        }
        _actionReady.Set();
    }
}
