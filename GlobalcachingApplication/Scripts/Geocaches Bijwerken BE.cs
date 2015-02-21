using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalcachingApplication.Framework;
using GlobalcachingApplication.Framework.Data;
using GlobalcachingApplication.Framework.Interfaces;
using GlobalcachingApplication.Utils;
using GlobalcachingApplication.Utils.API;
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
            _actionReady.Dispose();
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
            bool cancel = false;
            using (ProgressBlock progress = new ProgressBlock(_plugin, "Bijwerken van status en nieuwe geocaches", "Download gegevens van globalcaching.eu", 1, 0, true))
            {
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    string doc = wc.DownloadString("http://www.globalcaching.eu/Service/GeocacheCodes.aspx?country=Belgium");
                    if (doc != null)
                    {
                        List<string> gcList = new List<string>();

                        string[] lines = doc.Replace("\r", "").Split(new char[] { '\n' });
                        progress.UpdateProgress("Bijwerken van status en nieuwe geocaches", "Bijwerken van de status...", lines.Length, 0);
                        Geocache gc;
                        char[] sep = new char[] { ',' };
                        string[] parts;
                        foreach (string s in lines)
                        {
                            parts = s.Split(sep);
                            if (parts.Length > 2)
                            {
                                gc = DataAccess.GetGeocache(_core.Geocaches, parts[0]);
                                if (gc != null)
                                {
                                    gc.Archived = parts[1] != "0";
                                    gc.Available = parts[2] != "0";
                                }
                                else if (parts[1] == "0") //add only none archived
                                {
                                    gcList.Add(parts[0]);
                                }
                            }
                        }

                        if (gcList.Count == 0)
                        {
                            System.Windows.Forms.MessageBox.Show("Er zijn geen nieuwe geocaches gevonden", "Bericht", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                        }
                        else
                        {
                            if (System.Windows.Forms.MessageBox.Show(string.Format("Er zijn {0} nieuwe geocaches gevonden\r\nDeze downloaden?", gcList.Count), "Bericht", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.Yes)
                            {
                                progress.UpdateProgress("Bijwerken van status en nieuwe geocaches", "Importeren van geocaches...", gcList.Count, 0);

                                using (GeocachingLiveV6 client = new GeocachingLiveV6(_core, string.IsNullOrEmpty(_core.GeocachingComAccount.APIToken)))
                                {
                                    int index = 0;
                                    int total = gcList.Count;
                                    int gcupdatecount;
                                    TimeSpan interval = new TimeSpan(0, 0, 0, 2, 100);
                                    DateTime prevCall = DateTime.MinValue;
                                    bool dodelay;
                                    gcupdatecount = 30;
                                    dodelay = (gcList.Count > 30);
                                    while (gcList.Count > 0)
                                    {
                                        if (dodelay)
                                        {
                                            TimeSpan ts = DateTime.Now - prevCall;
                                            if (ts < interval)
                                            {
                                                Thread.Sleep(interval - ts);
                                            }
                                        }
                                        GlobalcachingApplication.Utils.API.LiveV6.SearchForGeocachesRequest req = new GlobalcachingApplication.Utils.API.LiveV6.SearchForGeocachesRequest();
                                        req.IsLite = false;
                                        req.AccessToken = client.Token;
                                        req.CacheCode = new GlobalcachingApplication.Utils.API.LiveV6.CacheCodeFilter();
                                        req.CacheCode.CacheCodes = (from a in gcList select a).Take(gcupdatecount).ToArray();
                                        req.MaxPerPage = gcupdatecount;
                                        req.GeocacheLogCount = 5;
                                        index += req.CacheCode.CacheCodes.Length;
                                        gcList.RemoveRange(0, req.CacheCode.CacheCodes.Length);
                                        prevCall = DateTime.Now;
                                        var resp = client.Client.SearchForGeocaches(req);
                                        if (resp.Status.StatusCode == 0 && resp.Geocaches != null)
                                        {
                                            Import.AddGeocaches(_core, resp.Geocaches);
                                        }
                                        else
                                        {
                                            _errormessage = resp.Status.StatusMessage;
                                            break;
                                        }
                                        if (!progress.UpdateProgress("Bijwerken van status en nieuwe geocaches", "Importeren van geocaches...", total, index))
                                        {
                                            break;
                                        }
                                    }
                                }
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

