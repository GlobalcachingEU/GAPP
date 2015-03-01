using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.Win32;
using System.Threading.Tasks;
using GlobalcachingApplication.Framework;

namespace GlobalcachingApplication.Core
{
    public class Engine: Framework.Interfaces.ICore, IDisposable
    {
        private List<Framework.Interfaces.IPlugin> _plugins = null;
        private List<string> _detectedPlugins = null;
        private List<string> _internalStoragePlugins = null;
        private List<System.Configuration.ApplicationSettingsBase> _allSettings = null;
        private AppDomain _currentDomain;
        private CultureInfo _selectedLanguage;
        private bool _pluginDataFolderSelected;

        public event Framework.EventArguments.PluginEventHandler PluginAdded;
        public event EventHandler SelectedLanguageChanged;
        public event Framework.EventArguments.GeocacheEventHandler ActiveGeocacheChanged;
        public event Framework.EventArguments.GeocacheComAccountEventHandler GeocachingComAccountChanged;
        public event Framework.EventArguments.GeocachingAccountNamesEventHandler GeocachingAccountNamesChanged;
        public event Framework.EventArguments.DebugLogEventHandler DebugLogAdded;
        public event EventHandler ShortcutInfoChanged;

        private Framework.Data.GeocachingAccountNames _geocachingAccountNames = null;
        private Framework.Data.GeocachingComAccountInfo _geocachingComAccount = null;
        private Framework.Data.Geocache _activeGeocache = null;
        private Framework.Data.GeocacheCollection _geocaches = null;
        private Framework.Data.WaypointCollection _waypoints = null;
        private Framework.Data.UserWaypointCollection _userWaypoints = null;
        private Framework.Data.LogCollection _logs = null;
        private Framework.Data.LogImageCollection _logImages = null;
        private Framework.Data.GeocacheImageCollection _geocacheImages = null;
        private Framework.Data.GeocacheAttributeCollection _geocacheAttributes;
        private Framework.Data.GeocacheTypeCollection _geocacheTypes;
        private Framework.Data.GeocacheContainerCollection _geocacheContainers;
        private Framework.Data.LogTypeCollection _logTypes;
        private Framework.Data.WaypointTypeCollection _waypointTypes;
        private Framework.Data.Location _homeLocation = null;
        private Framework.Data.Location _centerLocation = null;
        private Framework.Data.GPSLocation _gpsLocation = null;
        private Framework.Data.LanguageItemCollection _languageItems = null;
        private List<Framework.Data.ShortcutInfo> _shortcuts = null;

        private SettingsProvider _settingsProvider = null;

        public delegate void LoadingAssemblyHandler(object sender, string e);
        public event LoadingAssemblyHandler LoadingAssembly;
        public event LoadingAssemblyHandler InitializingPlugin;

        public Engine()
        {
            try
            {
                _settingsProvider = new SettingsProvider(null);

                string[] args = Environment.GetCommandLineArgs();
                if (EnablePluginDataPathAtStartup || (args != null && args.Contains("/f")))
                {
                    using (SelectSettingsForm dlg = new SelectSettingsForm(this))
                    {
                        _pluginDataFolderSelected = dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK;
                    }
                }
                else
                {
                    _pluginDataFolderSelected = true;
                }

                if (_pluginDataFolderSelected)
                {
                    _geocachingAccountNames = new Framework.Data.GeocachingAccountNames();
                    var p = _settingsProvider.GetSettingsValueStringCollection("Core.GeocachingAccountNames", null);
                    if (p != null)
                    {
                        foreach (string s in p)
                        {
                            string[] parts = s.Split("|".ToArray(), 2);
                            if (parts.Length == 2)
                            {
                                _geocachingAccountNames.SetAccountName(parts[0], parts[1]);
                            }
                        }
                    }
                    _geocachingAccountNames.Changed += new Framework.EventArguments.GeocachingAccountNamesEventHandler(_geocachingAccountNames_Changed);

                    _geocachingComAccount = new Framework.Data.GeocachingComAccountInfo();
                    _geocachingComAccount.AccountName = _settingsProvider.GetSettingsValue("Core.GCComAccountName", null);
                    _geocachingComAccount.APIToken = _settingsProvider.GetSettingsValue("Core.GCComAccountToken", null);
                    _geocachingComAccount.APITokenStaging = _settingsProvider.GetSettingsValue("Core.GCComAccountTokenStaging", null);
                    _geocachingComAccount.MemberType = _settingsProvider.GetSettingsValue("Core.GCComAccountMemberType", null);
                    _geocachingComAccount.MemberTypeId = _settingsProvider.GetSettingsValueInt("Core.GCComAccountMemberTypeId", 0);
                    _geocachingComAccount.Changed += new Framework.EventArguments.GeocacheComAccountEventHandler(_geocachingComAccount_Changed);
                    GeocachingAccountNames.SetAccountName("GC", _settingsProvider.GetSettingsValue("Core.GCComAccountName", null) ?? "");

                    _logs = new Framework.Data.LogCollection();
                    _userWaypoints = new Framework.Data.UserWaypointCollection();
                    _waypoints = new Framework.Data.WaypointCollection();
                    _geocaches = new Framework.Data.GeocacheCollection(this);
                    _logImages = new Framework.Data.LogImageCollection();
                    _geocacheImages = new Framework.Data.GeocacheImageCollection();
                    _geocacheAttributes = new Framework.Data.GeocacheAttributeCollection();
                    _geocacheTypes = new Framework.Data.GeocacheTypeCollection();
                    _geocacheContainers = new Framework.Data.GeocacheContainerCollection();
                    _logTypes = new Framework.Data.LogTypeCollection();
                    _waypointTypes = new Framework.Data.WaypointTypeCollection();
                    _homeLocation = new Framework.Data.Location();
                    _centerLocation = new Framework.Data.Location();
                    _gpsLocation = new Framework.Data.GPSLocation();
                    _languageItems = new Framework.Data.LanguageItemCollection();

                    _detectedPlugins = new List<string>();
                    _internalStoragePlugins = new List<string>();
                    _selectedLanguage = System.Globalization.CultureInfo.CurrentCulture;
                    _plugins = new List<Framework.Interfaces.IPlugin>();
                    _currentDomain = AppDomain.CurrentDomain;
                    _currentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromSameFolder);

                    //set initial data
                    //default location settings
                    _centerLocation.SetLocation(_settingsProvider.GetSettingsValueDouble("Core.CenterLat", 51.5), _settingsProvider.GetSettingsValueDouble("Core.CenterLon", 5.5));
                    _centerLocation.Changed += new Framework.EventArguments.LocationEventHandler(_centerLocation_Changed);
                    _homeLocation.SetLocation(_settingsProvider.GetSettingsValueDouble("Core.HomeLat", 51.5), _settingsProvider.GetSettingsValueDouble("Core.HomeLon", 5.5));
                    _homeLocation.Changed += new Framework.EventArguments.LocationEventHandler(_homeLocation_Changed);

                    //default (unknown) cache- ,container etc. types. Position 0 means unknown
                    Framework.Data.GeocacheType ct = new Framework.Data.GeocacheType();
                    ct.ID = 0;
                    ct.Name = "Not present";
                    _geocacheTypes.Add(ct);
                    Framework.Data.GeocacheAttribute attr = new Framework.Data.GeocacheAttribute();
                    attr.ID = 0;
                    attr.Name = "Unknown";
                    _geocacheAttributes.Add(attr);
                    Framework.Data.GeocacheContainer cont = new Framework.Data.GeocacheContainer();
                    cont.ID = 0;
                    cont.Name = "Unknown";
                    _geocacheContainers.Add(cont);
                    Framework.Data.LogType lt = new Framework.Data.LogType();
                    lt.ID = 0;
                    lt.Name = "Unknown";
                    lt.AsFound = false;
                    _logTypes.Add(lt);
                    Framework.Data.WaypointType wpt = new Framework.Data.WaypointType();
                    wpt.ID = 0;
                    wpt.Name = "Unknown";
                    _waypointTypes.Add(wpt);

                    _shortcuts = new List<Framework.Data.ShortcutInfo>();
                }
            }
            catch
            {
                RestoreDefaultSettings();
            }
        }

        public Framework.Interfaces.ISettings SettingsProvider 
        {
            get { return _settingsProvider; }
        }

        void _geocachingAccountNames_Changed(object sender, Framework.EventArguments.GeocachingAccountNamesEventArgs e)
        {
            var p = _settingsProvider.GetSettingsValueStringCollection("Core.GeocachingAccountNames", null);
            string[] prefixes = e.AccountNames.GeocachePrefixes;
            foreach (string s in prefixes)
            {
                p.Add(String.Format("{0}|{1}", s, e.AccountNames.GetAccountName(s)));
            }
            _settingsProvider.SetSettingsValueStringCollection("Core.GeocachingAccountNames", p);
            OnGeocachingAccountNamesChanged(this);            
        }

        public void RestoreDefaultSettings()
        {
            if (System.Windows.Forms.MessageBox.Show("Failed to initialize the application. Some settings files or data files might have been corrupted.\r\nDo you want to restore the default settings?", "Failed to start GAPP", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Error, System.Windows.Forms.MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
            {
                string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = Path.Combine(exePath, "RestoreDefaultSettings.exe");
                psi.UseShellExecute = true;
                psi.WorkingDirectory = exePath;
                Process.Start(psi);

                try
                {
                    _settingsProvider.Dispose();
                    _settingsProvider = null;
                    Process.GetCurrentProcess().Kill();
                }
                catch
                {
                }
            }
        }

        public void ShowAboutDialog()
        {
            using (AboutBoxForm dlg = new AboutBoxForm())
            {
                dlg.ShowDialog();
            }
        }

        public string CSScriptsPath
        {
            get
            {
                string p = System.IO.Path.Combine(PluginDataPath, "Scripts");
                if (!System.IO.Directory.Exists(p))
                {
                    System.IO.Directory.CreateDirectory(p);
                }
                return p;
            }
            set { ; }
        }

        public bool AutoSaveOnClose
        {
            get { if (_settingsProvider==null) return false; else return _settingsProvider.GetSettingsValueBool("Core.AutoSaveOnClose", false); }
            set { _settingsProvider.SetSettingsValueBool("Core.AutoSaveOnClose", value); }
        }

        public bool EnablePluginDataPathAtStartup 
        {
            get { return bool.Parse(_settingsProvider.GetScopelessSettingsValue("Core.EnablePluginDataPathAtStartup", false.ToString())); }
            set { _settingsProvider.SetScopelessSettingsValue("Core.EnablePluginDataPathAtStartup", value.ToString()); }
        }


        public string PluginDataPath
        {
            get 
            {
                string p = System.IO.Path.Combine(new string[] { System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GAPP" });
                p = p.TrimEnd(new char[] { '\\', '/' });
                if (!System.IO.Directory.Exists(p))
                {
                    System.IO.Directory.CreateDirectory(p);
                }
                return p; 
            }
        }

        public bool LoadLogsInBackground
        {
            get { return _settingsProvider.GetSettingsValueBool("Core.LoadLogsInBackground", true); }
            set { _settingsProvider.SetSettingsValueBool("Core.LoadLogsInBackground", value); }
        }

        public List<string> GetAvailableInternalStoragePlugins()
        {
            return _internalStoragePlugins;
        }

        public string ActiveInternalStoragePlugin
        {
            get { return _settingsProvider.GetSettingsValue("Core.InternalStorageClass", "GlobalcachingApplication.Plugins.GAPPDataStorage.InternalStorage"); }
            set { _settingsProvider.SetSettingsValue("Core.InternalStorageClass", value); }
        }

        public void OnShortcutInfoChanged()
        {
            if (ShortcutInfoChanged != null)
            {
                ShortcutInfoChanged(this, EventArgs.Empty);
            }
        }


        public List<Framework.Data.ShortcutInfo> ShortcutInfo 
        {
            get { return _shortcuts; } 
        }


        public bool RetrieveAPIKey(Framework.Data.APIKey keyType)
        {
            bool result = false;

            //code to generate key pair
            /*
            RSA rsa = new RSACryptoServiceProvider(2048); // Generate a new 2048 bit RSA key
            string publicPrivateKeyXML = rsa.ToXmlString(true);
            string publicOnlyKeyXML = rsa.ToXmlString(false);
            System.IO.File.WriteAllText("privateKey.xml", publicPrivateKeyXML);
            System.IO.File.WriteAllText("publicKey.xml", publicOnlyKeyXML);
            */

            using (KeyRequestForm dlg = new KeyRequestForm(keyType))
            {
                dlg.ShowDialog();
                if (!string.IsNullOrEmpty(dlg.Token))
                {
                    if (keyType == Framework.Data.APIKey.GeocachingLive)
                    {
                        _geocachingComAccount.APIToken = dlg.Token;
                    }
                    else if (keyType == Framework.Data.APIKey.GeocachingLiveTest)
                    {
                        _geocachingComAccount.APITokenStaging = dlg.Token;
                    }
                    result = true;
                }
            }
            return result;
        }


        public void DebugLog(Framework.Data.DebugLogLevel level, Framework.Interfaces.IPlugin p, Exception e, string msg)
        {
            if (DebugLogAdded != null)
            {
                DebugLogAdded(this, new Framework.EventArguments.DebugLogEventArgs(level, p, e, msg));
            }
        }

        void _geocachingComAccount_Changed(object sender, Framework.EventArguments.GeocacheComAccountEventArgs e)
        {
            _settingsProvider.SetSettingsValue("Core.GCComAccountName", e.AccountInfo.AccountName);
            _settingsProvider.SetSettingsValue("Core.GCComAccountToken", e.AccountInfo.APIToken);
            _settingsProvider.SetSettingsValue("Core.GCComAccountTokenStaging", e.AccountInfo.APITokenStaging);
            _settingsProvider.SetSettingsValue("Core.GCComAccountMemberType", e.AccountInfo.MemberType);
            _settingsProvider.SetSettingsValueInt("Core.GCComAccountMemberTypeId", e.AccountInfo.MemberTypeId);
            GeocachingAccountNames.SetAccountName("GC", e.AccountInfo.AccountName);
            OnGeocachingComAccountChanged(this);
        }

        public void InitiateUpdaterAndExit()
        {
            //check the type of update
            string updatePath = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GAPP", "Update");
            string[] files = Directory.GetFiles(updatePath);
            if (files != null && files.Length>0)
            {
                if (files[0].ToLower().EndsWith(".zip"))
                {
                    PrepareClosingApplication();

                    string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = Path.Combine(exePath, "GlobalcachingApplication.Updater.exe");
                    psi.UseShellExecute = true;
                    psi.WorkingDirectory = exePath;
                    try
                    {
                        if (Environment.OSVersion.Version.Major > 5)
                        {
                            psi.Verb = "runas";
                        }
                    }
                    catch
                    {
                        psi.Verb = "runas";
                    }
                    Process.Start(psi);

                    try
                    {
                        _settingsProvider.Dispose();
                        _settingsProvider = null;
                        Process.GetCurrentProcess().Kill();
                    }
                    catch
                    {
                    }
                    //System.Windows.Forms.Application.Exit();
                }
                else if (files[0].ToLower().EndsWith(".msi"))
                {
                    PrepareClosingApplication();

                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = "msiexec";
                    //psi.UseShellExecute = true;
                    psi.WorkingDirectory = updatePath;
                    psi.Arguments = string.Format("/i \"{0}\"", files[0]);
                    try
                    {
                        if (Environment.OSVersion.Version.Major > 5)
                        {
                            psi.Verb = "runas";
                        }
                    }
                    catch
                    {
                        psi.Verb = "runas";
                    }
                    Process.Start(psi);

                    try
                    {
                        _settingsProvider.Dispose();
                        _settingsProvider = null;
                        Process.GetCurrentProcess().Kill();
                    }
                    catch
                    {
                    }
                    //System.Windows.Forms.Application.Exit();
                }
            }
        }


        void _homeLocation_Changed(object sender, Framework.EventArguments.LocationEventArgs e)
        {
            _settingsProvider.SetSettingsValueDouble("Core.HomeLat", e.Location.Lat);
            _settingsProvider.SetSettingsValueDouble("Core.HomeLon", e.Location.Lon);
        }

        void _centerLocation_Changed(object sender, Framework.EventArguments.LocationEventArgs e)
        {
            _settingsProvider.SetSettingsValueDouble("Core.CenterLat", e.Location.Lat);
            _settingsProvider.SetSettingsValueDouble("Core.CenterLon", e.Location.Lon);
        }

        public Framework.Data.GeocachingComAccountInfo GeocachingComAccount
        {
            get { return _geocachingComAccount; }
            set
            {
                if (_geocachingComAccount != value)
                {
                    _geocachingComAccount = value;
                    OnGeocachingComAccountChanged(this);
                }
            }
        }

        public Framework.Data.GeocachingAccountNames GeocachingAccountNames
        {
            get { return _geocachingAccountNames; }
            set
            {
                if (_geocachingAccountNames != value)
                {
                    _geocachingAccountNames = value;
                    OnGeocachingAccountNamesChanged(this);
                }
            }
        }

        public Framework.Data.Geocache ActiveGeocache
        {
            get { return _activeGeocache; }
            set
            {
                if (_activeGeocache != value)
                {
                    _activeGeocache = value;
                    OnActiveGeocacheChanged(this);
                }
            }
        }
        public void OnActiveGeocacheChanged(object sender)
        {
            if (ActiveGeocacheChanged != null)
            {
                ActiveGeocacheChanged(sender, new Framework.EventArguments.GeocacheEventArgs(ActiveGeocache)); 
            }
        }

        public void OnGeocachingComAccountChanged(object sender)
        {
            if (GeocachingComAccountChanged!=null)
            {
                GeocachingComAccountChanged(sender, new Framework.EventArguments.GeocacheComAccountEventArgs(GeocachingComAccount));
            }
        }

        public void OnGeocachingAccountNamesChanged(object sender)
        {
            if (GeocachingAccountNamesChanged != null)
            {
                GeocachingAccountNamesChanged(sender, new Framework.EventArguments.GeocachingAccountNamesEventArgs(GeocachingAccountNames));
            }
        }

        public Framework.Data.GeocacheCollection Geocaches
        {
            get { return _geocaches; }
        }
        public Framework.Data.WaypointCollection Waypoints
        {
            get { return _waypoints; }
        }
        public Framework.Data.UserWaypointCollection UserWaypoints
        {
            get { return _userWaypoints; }
        }
        public Framework.Data.LogCollection Logs
        {
            get { return _logs; }
        }
        public Framework.Data.LogImageCollection LogImages
        {
            get { return _logImages; }
        }
        public Framework.Data.GeocacheImageCollection GeocacheImages
        {
            get { return _geocacheImages; }
        }
        public Framework.Data.GeocacheAttributeCollection GeocacheAttributes
        {
            get { return _geocacheAttributes; }
        }
        public Framework.Data.GeocacheTypeCollection GeocacheTypes
        {
            get { return _geocacheTypes; }
        }
        public Framework.Data.GeocacheContainerCollection GeocacheContainers
        {
            get { return _geocacheContainers; }
        }
        public Framework.Data.LogTypeCollection LogTypes
        {
            get { return _logTypes; }
        }
        public Framework.Data.WaypointTypeCollection WaypointTypes
        {
            get { return _waypointTypes; }
        }
        public Framework.Data.LanguageItemCollection LanguageItems
        {
            get { return _languageItems; }
        }

        public Framework.Data.Location HomeLocation
        {
            get { return _homeLocation; }
        }
        public Framework.Data.Location CenterLocation
        {
            get { return _centerLocation; }
        }
        public Framework.Data.GPSLocation GPSLocation
        {
            get { return _gpsLocation; }
        }


        static Assembly LoadFromSameFolder(object sender, ResolveEventArgs args)
        {
            Assembly result = null;

            string filename = string.Format("{0}.dll", new AssemblyName(args.Name).Name);
            string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string assemblyPath = Path.Combine(folderPath, filename);
            if (File.Exists(assemblyPath))
            {
                result = Assembly.LoadFrom(assemblyPath);
            }
            else
            {
                assemblyPath = SearchForFile(Path.Combine(folderPath, "PluginDependencies"), filename);
                if (!string.IsNullOrEmpty(assemblyPath))
                {
                    if (File.Exists(assemblyPath))
                    {
                        result = Assembly.LoadFrom(assemblyPath);
                    }
                }
            }
            return result;
        }

        static string SearchForFile(string folder, string fn)
        {
            string result = null;
            string assemblyPath = Path.Combine(folder, fn);
            if (File.Exists(assemblyPath))
            {
                result = assemblyPath;
            }
            else
            {
                string[] folders = Directory.GetDirectories(folder);
                if (folders != null)
                {
                    foreach (string f in folders)
                    {
                        result = SearchForFile(f, fn);
                        if (result != null)
                        {
                            break;
                        }
                    }
                }
            }
            return result;
        }

        public Version Version
        {
            get { return System.Reflection.Assembly.GetAssembly(typeof(Engine)).GetName().Version; }
        }

        public CultureInfo SelectedLanguage 
        {
            get { return _selectedLanguage; }
            set
            {
                if (_selectedLanguage.LCID != value.LCID)
                {
                    _selectedLanguage = value;
                    OnSelectedLanguage();
                }
            }
        }
        public void OnSelectedLanguage()
        {
            _settingsProvider.SetSettingsValueInt("Core.CultureID", _selectedLanguage.LCID);
            if (SelectedLanguageChanged != null)
            {
                SelectedLanguageChanged(this, EventArgs.Empty);
            }
        }

        public List<Framework.Interfaces.IPlugin> GetPlugin(Framework.PluginType pluginType)
        {
            List<Framework.Interfaces.IPlugin> result = new List<Framework.Interfaces.IPlugin>();
            foreach (Framework.Interfaces.IPlugin p in _plugins)
            {
                if (p.PluginType == pluginType)
                {
                    result.Add(p);
                }
            }
            return result;
        }

        public List<Framework.Interfaces.IPlugin> GetPlugins()
        {
            List<Framework.Interfaces.IPlugin> result = new List<Framework.Interfaces.IPlugin>();
            result.AddRange(_plugins.ToArray());
            return result;
        }

        public List<string> GetAllDetectedPlugins()
        {
            List<string> result = new List<string>();
            result.AddRange(_detectedPlugins.ToArray());
            return result;
        }

        public async Task ApplicationInitialized()
        {
            foreach (Framework.Interfaces.IPlugin plugin in _plugins)
            {
                try
                {
                    await plugin.PluginsInitializedAsync();
                }
                catch
                {
                }
            }
            foreach (Framework.Interfaces.IPlugin plugin in _plugins)
            {
                try
                {
                    await plugin.ApplicationInitializedAsync();
                }
                catch
                {
                }
            }
        }

        public void PrepareClosingApplication()
        {
            try
            {
                foreach (Framework.Interfaces.IPlugin plugin in _plugins)
                {
                    try
                    {
                        plugin.ApplicationClosing();
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        public async Task<bool> Initialize()
        {
            bool result = false;
            try
            {
                if (_pluginDataFolderSelected)
                {
                    List<Framework.Interfaces.IPlugin> pins = new List<Framework.Interfaces.IPlugin>();
                    result = LoadAssemblyFolder(pins, Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Plugins"));
                    if (result)
                    {
                        //first the Prerequisites
                        foreach (Framework.Interfaces.IPlugin plugin in pins)
                        {
                            if (plugin.Prerequisite)
                            {
                                try
                                {
                                    if (InitializingPlugin != null)
                                    {
                                        InitializingPlugin(this, plugin.FriendlyName);
                                    }
                                    result = await plugin.InitializeAsync(this);
                                    if (result)
                                    {
                                        _plugins.Add(plugin);
                                        plugin.Closing += new Framework.EventArguments.PluginEventHandler(plugin_Closing);
                                        OnPluginAdded(plugin);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                catch
                                {
                                    //plugin failed to load
                                    result = false;
                                    break;
                                }
                            }
                        }
                        //then the non-prerequisites
                        if (result)
                        {
                            foreach (Framework.Interfaces.IPlugin plugin in pins)
                            {
                                if (!plugin.Prerequisite)
                                {
                                    try
                                    {
                                        if (InitializingPlugin != null)
                                        {
                                            InitializingPlugin(this, plugin.FriendlyName);
                                        }
                                        result = await plugin.InitializeAsync(this);
                                        if (result)
                                        {
                                            _plugins.Add(plugin);
                                            plugin.Closing += new Framework.EventArguments.PluginEventHandler(plugin_Closing);
                                            OnPluginAdded(plugin);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    catch// (Exception e)
                                    {
                                        //plugin failed to load
                                        //result = false;
                                        //break;
                                    }
                                }
                            }
                        }
                    }
                    if (_settingsProvider.GetSettingsValueInt("Core.CultureID", 0) != 0)
                    {
                        _selectedLanguage = new CultureInfo(_settingsProvider.GetSettingsValueInt("Core.CultureID", 0));
                    }
                    if (_settingsProvider.GetSettingsValueInt("Core.CultureID", 0) != 127)
                    {
                        //check if culture exists and if not, try match
                        List<Framework.Interfaces.IPlugin> lpins = (from p in pins where p.PluginType == Framework.PluginType.LanguageSupport select p).ToList();
                        foreach (Framework.Interfaces.ILanguageSupport p in lpins)
                        {
                            if ((from w in p.GetSupportedCultures() where w.LCID == _selectedLanguage.LCID select w).FirstOrDefault() == null)
                            {
                                //ok, no match
                                CultureInfo tmp = (from w in p.GetSupportedCultures() where w.Name.Substring(0, 2) == _selectedLanguage.Name.Substring(0, 2) select w).FirstOrDefault();
                                if (tmp != null)
                                {
                                    //ok, base matches
                                    _selectedLanguage = tmp;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    try
                    {
                        OnSelectedLanguage();
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
                RestoreDefaultSettings();
            }
            return result;
        }

        protected bool LoadAssemblyFolder(List<Framework.Interfaces.IPlugin> pins, string folder)
        {
            bool result = true;
            if (folder.StartsWith("file:"))
            {
                folder = folder.Substring(6);
            }
            string[] files = Directory.GetFiles(folder);
            if (files != null)
            {
                Array.Sort(files);
                foreach (string f in files)
                {
                    if (!LoadAssembly(pins, f))
                    {
                        result = false;
                        break;
                    }
                }
            }
            if (result)
            {
                string[] folders = Directory.GetDirectories(folder);
                if (folders != null)
                {
                    foreach (string f in folders)
                    {
                        result = LoadAssemblyFolder(pins, f);
                    }
                }
            }
            return result;
        }

        protected bool LoadAssembly(List<Framework.Interfaces.IPlugin> pins, string fn)
        {
            bool result = true;
            fn = fn.ToLower();
            if (fn.EndsWith(".dll") || fn.EndsWith(".exe"))
            {
                try
                {
                    Assembly asmbly = Assembly.LoadFrom(fn);
                    Type[] types = asmbly.GetTypes();
                    if (LoadingAssembly != null)
                    {
                        LoadingAssembly(this, fn);
                    }
                    foreach (Type t in types)
                    {
                        // Only select classes that implemented IPlugin
                        if (t.IsClass && ((IList)t.GetInterfaces()).Contains(typeof(Framework.Interfaces.IPlugin)))
                        {
                            bool isInternalStorage = ((IList)t.GetInterfaces()).Contains(typeof(Framework.Interfaces.IPluginInternalStorage));
                            if (isInternalStorage)
                            {
                                _internalStoragePlugins.Add(t.FullName);
                            }
                            else
                            {
                                _detectedPlugins.Add(t.FullName);
                            }
                            if (!isInternalStorage || ActiveInternalStoragePlugin == t.FullName)
                            {
                                //var dpil = _settingsProvider.GetSettingsValueStringCollection("Core.DisabledPlugins", null);
                                //if (dpil == null || !dpil.Contains(t.FullName))
                                {
                                    ConstructorInfo constructor = t.GetConstructor(new Type[0]);
                                    object[] parameters = new object[0];
                                    Framework.Interfaces.IPlugin plugin = (Framework.Interfaces.IPlugin)constructor.Invoke(parameters);
                                    pins.Add(plugin);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    //not an assembly
                }
            }
            return result;
        }

        public bool SetDisabledPlugins(string[] plugins)
        {
            InterceptedStringCollection sc = _settingsProvider.GetSettingsValueStringCollection("Core.DisabledPlugins", null);
            bool restart = false;
            if (plugins.Length != sc.Count)
            {
                restart = true;
            }
            else
            {
                foreach (string s in sc)
                {
                    if (!plugins.Contains(s))
                    {
                        restart = true;
                        break;
                    }
                }
            }
            if (restart)
            {
                sc.Clear();
                sc.AddRange(plugins);
                _settingsProvider.SetSettingsValueStringCollection("Core.DisabledPlugins", sc);
            }
            return restart;
        }

        void plugin_Closing(object sender, Framework.EventArguments.PluginEventArgs e)
        {
            if (_plugins.Contains(e.Plugin))
            {
                _plugins.Remove(e.Plugin);
            }
        }

        public void OnPluginAdded(Framework.Interfaces.IPlugin p)
        {
            if (PluginAdded!=null)
            {
                PluginAdded(this, new Framework.EventArguments.PluginEventArgs(p));
            }
        }

        public void Dispose()
        {
            if (_settingsProvider != null)
            {
                _settingsProvider.Dispose();
                _settingsProvider = null;
            }
        }
    }
}
