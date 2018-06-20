using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace GlobalcachingApplication.Plugins.CheckForUpdates
{
    public class UpdateChecker : Utils.BasePlugin.Plugin
    {
        public const string STR_ERROR = "Error";
        public const string STR_ERRORDOWNLOADFILE = "Unable to download package file";

        private SynchronizationContext _context = null;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            _context = SynchronizationContext.Current;
            if (_context == null)
            {
                _context = new SynchronizationContext();
            }

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERRORDOWNLOADFILE));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(NewCoreVersionAvailableNotification.STR_DOWNLOADINSTALL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(NewCoreVersionAvailableNotification.STR_DOWNLOADPAGE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(NewCoreVersionAvailableNotification.STR_NEWVERSIONAVAILABLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(NewCoreVersionAvailableNotification.STR_DOWNLOADING));

            return await base.InitializeAsync(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Process;
            }
        }

        public async override Task ApplicationInitializedAsync()
        {
            await base.ApplicationInitializedAsync();

            //start check thread
            Thread thrd = new Thread(new ThreadStart(this.checkForNewVersionThreadMethod));
            thrd.IsBackground = true;
            thrd.Start();
        }

        private void checkForNewVersionThreadMethod()
        {
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                try
                {
                    string xmldoc = wc.DownloadString("https://www.4geocaching.eu/downloads/_files/gapp/version.xml");
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xmldoc);
                    var root = doc.DocumentElement;
                    var vs = root.SelectSingleNode("version").InnerText.Substring(1);
#if DEBUG
                    if (Version.Parse(vs) > System.Reflection.Assembly.GetEntryAssembly().GetName().Version)
#else
                    if (Version.Parse(vs) > System.Reflection.Assembly.GetEntryAssembly().GetName().Version)
#endif
                    {
                        _context.Post(new SendOrPostCallback(delegate(object state)
                        {
                            NewCoreVersionAvailableNotification n = new NewCoreVersionAvailableNotification();
                            n.GAPPVersion = vs;
                            OnNotification(n);
                            n.DownloadAndInstall += new EventHandler<EventArgs>(n_DownloadAndInstall);
                        }), null);
                    }
                }
                catch
                {
                }
            }
        }

        void n_DownloadAndInstall(object sender, EventArgs e)
        {
            string updatePath = System.IO.Path.Combine(new string[] { System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GAPP" });
            try
            {
                if (!Directory.Exists(updatePath))
                {
                    Directory.CreateDirectory(updatePath);
                }
            }
            catch
            {
            }
            updatePath = System.IO.Path.Combine(new string[] { updatePath, "Update" });
            try
            {
                if (!Directory.Exists(updatePath))
                {
                    Directory.CreateDirectory(updatePath);
                }
            }
            catch
            {
            }
            string[] files = Directory.GetFiles(updatePath);
            if (files != null)
            {
                foreach (string s in files)
                {
                    File.Delete(s);
                }
            }
            //todo:
            string vs = (sender as NewCoreVersionAvailableNotification).GAPPVersion;
            string url = string.Format("https://www.4geocaching.eu/downloads/_files/gapp/V{0}/{1}/setup.msi", vs.Replace('.', '_'), Environment.Is64BitProcess ? "x64" : "x86");
            try
            {
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    wc.DownloadFile(url, Path.Combine(updatePath, "setup.msi"));
                    Core.InitiateUpdaterAndExit();
                }
            }
            catch
            {
                MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_ERRORDOWNLOADFILE), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            (sender as NewCoreVersionAvailableNotification).Visible = false;
        }

    }
}
