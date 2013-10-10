using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalcachingApplication.Framework;
using GlobalcachingApplication.Framework.Data;
using GlobalcachingApplication.Framework.Interfaces;
using GlobalcachingApplication.Utils;
using GlobalcachingApplication.Utils.BasePlugin;
using GlobalcachingApplication.Utils.API;
using System.Threading;

public class Script
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

        if (GeocachingLiveV6.CheckAPIAccessAvailable(core, true))
        {
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
    }

    private void threadMethod()
    {
        try
        {
            using (ProgressBlock progress = new ProgressBlock(_plugin, "Update Favorite for Netherlands, Belgium and Italy", "Downloading data from globalcaching.eu", 1, 0))
            {
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    string doc = wc.DownloadString(string.Format("http://www.globalcaching.eu/Service/CacheFavorites.aspx?token={0}", System.Web.HttpUtility.UrlEncode(_core.GeocachingComAccount.APIToken)));
                    if (doc != null)
                    {
                        string[] lines = doc.Replace("\r","").Split(new char[]{'\n'});
                        progress.UpdateProgress("Update Favorite of geocaches", "Updating geocaches...", lines.Length, 0);
                        Geocache gc;
                        char[] sep = new char[]{','};
                        string[] parts;
                        foreach (string s in lines)
                        {
                            parts = s.Split(sep);
                            if (parts.Length>0)
                            {
                                gc = DataAccess.GetGeocache(_core.Geocaches, parts[0]);
                                if (gc != null)
                                {
                                    gc.Favorites = int.Parse(parts[1]);
                                }
                            }
                        }
                    }

                }
            }
        }
        catch (Exception e)
        {
            _errormessage = e.Message;
        }
        _actionReady.Set();
    }

}
