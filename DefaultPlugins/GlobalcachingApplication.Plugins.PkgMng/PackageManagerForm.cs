using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using System.Threading;

namespace GlobalcachingApplication.Plugins.PkgMng
{
    public partial class PackageManagerForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_PACKAGEMANAGER = "Package Manager";
        public const string STR_INSTALL = "Install";
        public const string STR_DOWNLOAD = "Download";
        public const string STR_PACKAGEVERSION = "Package version";
        public const string STR_PACKAGEDATE = "Package date";
        public const string STR_UPDATEAVAILABLE = "Update available!";
        public const string STR_RETREIVINGPKGLIST = "Retrieving package list...";
        public const string STR_ERRORDOWNLOAD = "Unable to download package list";
        public const string STR_ERROR = "Error";
        public const string STR_ERRORDOWNLOADFILE = "Unable to download package file";
        public const string STR_SCRIPTINSTALLED = "Script installed";

        public class PackageItem
        {
            public string link { get; set; }
            public string packagetype { get; set; }
            public string plugintype { get; set; }
            public Version minCoreVersion { get; set; }
            public Version version { get; set; }
            public string description { get; set; }
            public string author { get; set; }
            public DateTime date { get; set; }
        }

        private Version _pkgVersion;
        private DateTime _pkgDate;
        private string[] _columnHeaders;
        private SynchronizationContext _context = null;

        public PackageManagerForm()
        {
            InitializeComponent();
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            base.SelectedLanguageChanged(sender, e);

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PACKAGEMANAGER);
            for (int i = 0; i < _columnHeaders.Length; i++)
            {
                listView1.Columns[i].Text = Utils.LanguageSupport.Instance.GetTranslation(_columnHeaders[i]);
            }
            this.buttonInstall.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_INSTALL);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DOWNLOAD);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PACKAGEVERSION);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PACKAGEDATE);
            this.labelUpdateAvailable.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_UPDATEAVAILABLE);
        }

        public PackageManagerForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            _context = SynchronizationContext.Current;
            if (_context == null)
            {
                _context = new SynchronizationContext();
            }

            _columnHeaders = (from ColumnHeader c in listView1.Columns select c.Text).ToArray();
            foreach (string s in _columnHeaders)
            {
                core.LanguageItems.Add(new Framework.Data.LanguageItem(s));                
            }

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            listView1.ListViewItemSorter = new Utils.ListViewColumnSorter();
            (listView1.ListViewItemSorter as Utils.ListViewColumnSorter).SortColumn = 0;
            (listView1.ListViewItemSorter as Utils.ListViewColumnSorter).Order = SortOrder.Ascending;

            SelectedLanguageChanged(this, EventArgs.Empty);
        }

        private void PackageManagerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
            }
        }


        public void UpdateView()
        {
            _pkgVersion = null;
            List<PackageItem> pkgs = getPackageItems(false);
            listView1.Items.Clear();
            foreach (PackageItem pi in pkgs)
            {
                ListViewItem lvi = new ListViewItem(new string[] { pi.packagetype, pi.plugintype, pi.version==null ? "":pi.version.ToString(), pi.description, pi.author, pi.date.ToShortDateString() });
                lvi.Tag = pi;
                listView1.Items.Add(lvi);
            }
            if (_pkgVersion != null)
            {
                labelPackageVersion.Text = string.Format("V{0}",_pkgVersion.ToString());
                labelPackageDate.Text = _pkgDate.ToString("d");

                if (Properties.Settings.Default.LastSeenPkgDate > new DateTime(2012, 5, 1) && Properties.Settings.Default.LastSeenPkgDate < _pkgDate)
                {
                    labelUpdateAvailable.Visible = true;
                }
                else
                {
                    labelUpdateAvailable.Visible = false;
                }
                Properties.Settings.Default.LastSeenPkgDate = DateTime.Now;
                Properties.Settings.Default.Save();
            }
        }


        public void checkForNewVersion()
        {
            Thread thrd = new Thread(new ThreadStart(this.checkForNewVersionThreadMethod));
            thrd.IsBackground = true;
            thrd.Start();
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
            string fn = Path.GetFileName((sender as NewCoreVersionAvailableNotification).DownloadLink);
            try
            {
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    wc.DownloadFile((sender as NewCoreVersionAvailableNotification).DownloadLink, Path.Combine(updatePath, fn));
                    Core.InitiateUpdaterAndExit();
                }
            }
            catch
            {
                MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_ERRORDOWNLOADFILE), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            (sender as NewCoreVersionAvailableNotification).Visible = false;
        }

        private void checkForNewVersionThreadMethod()
        {
            List<PackageItem> result = getPackageItems(true);
            PackageItem pi = (from p in result where p.packagetype.ToLower() == "core" select p).FirstOrDefault();
            if (pi != null)
            {
#if DEBUG
                if (pi.version > System.Reflection.Assembly.GetEntryAssembly().GetName().Version)
#else
                if (pi.version > System.Reflection.Assembly.GetEntryAssembly().GetName().Version)
#endif
                {
                    _context.Post(new SendOrPostCallback(delegate(object state)
                    {
                        NewCoreVersionAvailableNotification n = new NewCoreVersionAvailableNotification();
                        n.DownloadLink = pi.link;
                        (OwnerPlugin as PackageManager).OnNotification(n);
                        n.DownloadAndInstall += new EventHandler<EventArgs>(n_DownloadAndInstall);
                    }), null);
                }
            }
        }

        private string getPackageXml(bool updateui)
        {
            string result = null;
            if (updateui)
            {
                toolStripStatusLabelAction.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RETREIVINGPKGLIST);
                statusStrip1.Refresh();
            }
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                try
                {
                    result = wc.DownloadString(string.Format("http://application.globalcaching.eu/pkg/{0}/gapkg.xml", Environment.Is64BitProcess ? "x64" : "x86"));
                }
                catch
                {
                    if (updateui)
                    {
                        MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_ERRORDOWNLOAD), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            if (updateui)
            {
                toolStripStatusLabelAction.Text = "";
                statusStrip1.Refresh();
            }
            return result;
        }

        private List<PackageItem> getPackageItems(bool silent)
        {
            List<PackageItem> result = new List<PackageItem>();
            string ddoc = getPackageXml(!silent);
            if (!string.IsNullOrEmpty(ddoc))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(ddoc);
                    XmlElement root = doc.DocumentElement;
                    XmlNodeList pkgs = root.SelectNodes("package");
                    if (pkgs != null)
                    {
                        foreach (XmlNode pk in pkgs)
                        {
                            PackageItem pi = new PackageItem();
                            pi.author = pk.Attributes["author"].InnerText;
                            pi.date = DateTime.Parse(pk.Attributes["date"].InnerText);
                            pi.description = pk.Attributes["description"].InnerText;
                            pi.link = pk.Attributes["link"].InnerText;
                            pi.minCoreVersion = Version.Parse(pk.Attributes["mincoreversion"].InnerText.Substring(1));
                            pi.packagetype = pk.Attributes["type"].InnerText;
                            pi.plugintype = pk.Attributes["plugintype"].InnerText;
                            pi.version = Version.Parse(pk.Attributes["version"].InnerText.Substring(1));
                            result.Add(pi);
                        }
                        if (!silent)
                        {
                            _pkgVersion = Version.Parse(root.Attributes["version"].InnerText.Substring(1));
                            _pkgDate = DateTime.Parse(root.Attributes["date"].InnerText);

                            Properties.Settings.Default.LastCheckDateTime = DateTime.Now;
                            Properties.Settings.Default.Save();
                        }
                    }
                }
                catch
                {
                }
            }
            return result;
        }

        private void buttonInstall_Click(object sender, EventArgs e)
        {
            //download package
            PackageItem pkg = null;
            if (listView1.SelectedItems != null && listView1.SelectedItems.Count > 0)
            {
                pkg = listView1.SelectedItems[0].Tag as PackageItem;
                if (pkg != null)
                {
                    toolStripStatusLabelAction.Text = Utils.LanguageSupport.Instance.GetTranslation("Downloading package...");
                    statusStrip1.Refresh();
                    using (System.Net.WebClient wc = new System.Net.WebClient())
                    {
                        if (pkg.packagetype == "script")
                        {
                            //download to a temp file
                            using (TemporaryFile tmp = new TemporaryFile(false))
                            {
                                //unzip to appdata scripts folder
                                string fn = Path.GetFileName(pkg.link);
                                try
                                {
                                    wc.DownloadFile(pkg.link, tmp.Path);

                                    string scriptsPath = Core.CSScriptsPath;
                                    UnZipFiles(tmp.Path, scriptsPath, false);
                                    MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_SCRIPTINSTALLED));
                                }
                                catch
                                {
                                    MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_ERRORDOWNLOADFILE), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                //done
                            }
                        }
                        else
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
                            string fn = Path.GetFileName(pkg.link);
                            try
                            {
                                wc.DownloadFile(pkg.link, Path.Combine(updatePath, fn));
                                Core.InitiateUpdaterAndExit();
                            }
                            catch
                            {
                                MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_ERRORDOWNLOADFILE), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            toolStripStatusLabelAction.Text = "";
        }

        public static void UnZipFiles(string zipPathAndFile, string outputFolder, bool deleteZipFile)
        {
            ZipInputStream s = new ZipInputStream(File.OpenRead(zipPathAndFile));
            ZipEntry theEntry;
            string tmpEntry = String.Empty;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = outputFolder;
                string fileName = Path.GetFileName(theEntry.Name);
                // create directory 
                if (directoryName != "")
                {
                    Directory.CreateDirectory(directoryName);
                }
                if (fileName != String.Empty)
                {
                    string fullPath = Path.Combine(directoryName, theEntry.Name);
                    fullPath = fullPath.Replace("\\ ", "\\");
                    string fullDirPath = Path.GetDirectoryName(fullPath);
                    if (!Directory.Exists(fullDirPath)) Directory.CreateDirectory(fullDirPath);
                    FileStream streamWriter = File.Create(fullPath);
                    int size = 2048;
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        size = s.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }
                    streamWriter.Close();
                }
            }
            s.Close();
            if (deleteZipFile)
                File.Delete(zipPathAndFile);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonInstall.Enabled = (listView1.SelectedItems != null && listView1.SelectedItems.Count > 0);
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListView lv = (sender as ListView);
            if (lv != null)
            {
                Utils.ListViewColumnSorter lvcs = (lv.ListViewItemSorter as Utils.ListViewColumnSorter);
                if (lvcs != null)
                {
                    // Determine if clicked column is already the column that is being sorted.
                    if (e.Column == lvcs.SortColumn)
                    {
                        // Reverse the current sort direction for this column.
                        if (lvcs.Order == SortOrder.Ascending)
                        {
                            lvcs.Order = SortOrder.Descending;
                        }
                        else
                        {
                            lvcs.Order = SortOrder.Ascending;
                        }
                    }
                    else
                    {
                        // Set the column number that is to be sorted; default to ascending.
                        lvcs.SortColumn = e.Column;
                        lvcs.Order = SortOrder.Ascending;
                    }

                    // Perform the sort with these new sort options.
                    lv.Sort();
                }
            }
        }

        private void buttonInstall_EnabledChanged(object sender, EventArgs e)
        {
            button1.Enabled = buttonInstall.Enabled;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //dialog where to install
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    PackageItem pkg = null;
                    if (listView1.SelectedItems != null && listView1.SelectedItems.Count > 0)
                    {
                        pkg = listView1.SelectedItems[0].Tag as PackageItem;
                        if (pkg != null)
                        {
                            toolStripStatusLabelAction.Text = Utils.LanguageSupport.Instance.GetTranslation("Downloading package...");
                            statusStrip1.Refresh();
                            using (System.Net.WebClient wc = new System.Net.WebClient())
                            {
                                string fn = Path.GetFileName(pkg.link);
                                string dstFilename = Path.Combine(folderBrowserDialog1.SelectedPath, fn);
                                if (File.Exists(dstFilename))
                                {
                                    int index = 1;
                                    dstFilename = Path.Combine(folderBrowserDialog1.SelectedPath, string.Format("{0}({1}){2}", Path.GetFileNameWithoutExtension(fn), index, Path.GetExtension(fn)));
                                    while (File.Exists(dstFilename))
                                    {
                                        index++;
                                        dstFilename = Path.Combine(folderBrowserDialog1.SelectedPath, string.Format("{0}({1}){2}", Path.GetFileNameWithoutExtension(fn), index, Path.GetExtension(fn)));
                                    }
                                }
                                wc.DownloadFile(pkg.link, dstFilename);
                            }
                        }
                    }
                }
                catch
                {
                    MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_ERRORDOWNLOADFILE), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            toolStripStatusLabelAction.Text = "";
        }
    }

    public class PackageManager : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Package manager";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(PackageManagerForm.STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(PackageManagerForm.STR_ERRORDOWNLOAD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(PackageManagerForm.STR_ERRORDOWNLOADFILE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(PackageManagerForm.STR_INSTALL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(PackageManagerForm.STR_PACKAGEDATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(PackageManagerForm.STR_PACKAGEMANAGER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(PackageManagerForm.STR_PACKAGEVERSION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(PackageManagerForm.STR_RETREIVINGPKGLIST));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(PackageManagerForm.STR_SCRIPTINSTALLED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(PackageManagerForm.STR_UPDATEAVAILABLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(PackageManagerForm.STR_DOWNLOAD));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(NewCoreVersionAvailableNotification.STR_DOWNLOADINSTALL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(NewCoreVersionAvailableNotification.STR_DOWNLOADPAGE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(NewCoreVersionAvailableNotification.STR_NEWVERSIONAVAILABLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(NewCoreVersionAvailableNotification.STR_DOWNLOADING));

            return base.Initialize(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.PluginManager;
            }
        }

        public override void ApplicationInitialized()
        {
            base.ApplicationInitialized();
            (UIChildWindowForm as PackageManagerForm).checkForNewVersion();
        }

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new PackageManagerForm(this, core));
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (UIChildWindowForm != null)
                {
                    if (action == ACTION_SHOW)
                    {
                        if (!UIChildWindowForm.Visible)
                        {
                            UIChildWindowForm.Show();
                            (UIChildWindowForm as PackageManagerForm).UpdateView();
                        }
                        if (UIChildWindowForm.WindowState == FormWindowState.Minimized)
                        {
                            UIChildWindowForm.WindowState = FormWindowState.Normal;
                        }
                        UIChildWindowForm.BringToFront();
                    }
                }
                result = true;
            }
            return result;
        }
    }

}
