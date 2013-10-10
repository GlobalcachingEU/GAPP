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

namespace GlobalcachingApplication.Core
{
    public class Engine: Framework.Interfaces.ICore
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
        public event Framework.EventArguments.DebugLogEventHandler DebugLogAdded;
        public event EventHandler ShortcutInfoChanged;

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

        public delegate void LoadingAssemblyHandler(object sender, string e);
        public event LoadingAssemblyHandler LoadingAssembly;
        public event LoadingAssemblyHandler InitializingPlugin;

        public Engine()
        {
            try
            {
                if (Properties.Settings.Default.UpgradeNeeded)
                {
                    Properties.Settings.Default.Upgrade();
                    Properties.Settings.Default.UpgradeNeeded = false;
                    Properties.Settings.Default.Save();
                }

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
                    _allSettings = new List<System.Configuration.ApplicationSettingsBase>();
                    _allSettings.Add(Properties.Settings.Default);

                    PortableSettings.LoadSettings(PluginDataPath, Properties.Settings.Default);
                    PortableSettings.SaveSettings(PluginDataPath, Properties.Settings.Default);
                    Properties.Settings.Default.SettingsSaving += new System.Configuration.SettingsSavingEventHandler(Default_SettingsSaving);

                    _geocachingComAccount = new Framework.Data.GeocachingComAccountInfo();
                    _geocachingComAccount.AccountName = Properties.Settings.Default.GCComAccountName;
                    _geocachingComAccount.APIToken = Properties.Settings.Default.GCComAccountToken;
                    _geocachingComAccount.APITokenStaging = Properties.Settings.Default.GCComAccountTokenStaging;
                    _geocachingComAccount.MemberType = Properties.Settings.Default.GCComAccountMemberType;
                    _geocachingComAccount.MemberTypeId = Properties.Settings.Default.GCComAccountMemberTypeId;
                    _geocachingComAccount.Changed += new Framework.EventArguments.GeocacheComAccountEventHandler(_geocachingComAccount_Changed);

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
                    _centerLocation.SetLocation(Properties.Settings.Default.CenterLat, Properties.Settings.Default.CenterLon);
                    _centerLocation.Changed += new Framework.EventArguments.LocationEventHandler(_centerLocation_Changed);
                    _homeLocation.SetLocation(Properties.Settings.Default.HomeLat, Properties.Settings.Default.HomeLon);
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
                    Process.GetCurrentProcess().Kill();
                }
                catch
                {
                }
            }
        }

        void Default_SettingsSaving(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Configuration.ApplicationSettingsBase  settings = sender as System.Configuration.ApplicationSettingsBase;
            if (settings != null)
            {
                PortableSettings.SaveSettings(PluginDataPath, settings);
            }
        }

        public bool CreateSettingsInFolder(string folder, bool useCurrentSettings)
        {
            bool result = false;
            try
            {
                foreach (System.Configuration.ApplicationSettingsBase settings in _allSettings)
                {
                    PortableSettings.CopySettings(PluginDataPath, folder, settings, useCurrentSettings);
                }
                result = true;
            }
            catch
            {
            }
            return result;
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
                if (string.IsNullOrEmpty(Properties.Settings.Default.CSScriptsPath))
                {
                    string p = System.IO.Path.Combine(PluginDataPath, "Scripts");
                    Properties.Settings.Default.CSScriptsPath = p;
                }
                if (!System.IO.Directory.Exists(Properties.Settings.Default.CSScriptsPath))
                {
                    System.IO.Directory.CreateDirectory(Properties.Settings.Default.CSScriptsPath);
                }
                return Properties.Settings.Default.CSScriptsPath;
            }
            set { ; } 
        }

        public bool AutoSaveOnClose
        {
            get { return Properties.Settings.Default.AutoSaveOnClose; }
            set
            {
                if (Properties.Settings.Default.AutoSaveOnClose != value)
                {
                    Properties.Settings.Default.AutoSaveOnClose = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        public string[] AvailablePluginDataPaths 
        {
            get
            {
                if (Properties.Settings.Default.AvailablePluginDataPaths == null)
                {
                    return new string[0];
                }
                else
                {
                    string[] result = new string[Properties.Settings.Default.AvailablePluginDataPaths.Count];
                    Properties.Settings.Default.AvailablePluginDataPaths.CopyTo(result,0);
                    return result;
                }
            }
            set
            {
                Properties.Settings.Default.AvailablePluginDataPaths = new System.Collections.Specialized.StringCollection();
                if (value != null && value.Length > 0)
                {
                    Properties.Settings.Default.AvailablePluginDataPaths.AddRange(value);
                }
                Properties.Settings.Default.Save();
            }
        }

        public bool EnablePluginDataPathAtStartup 
        {
            get { return Properties.Settings.Default.EnablePluginDataPathAtStartup; }
            set
            {
                Properties.Settings.Default.EnablePluginDataPathAtStartup = value;
                Properties.Settings.Default.Save();
            }
        }


        public string PluginDataPath
        {
            get 
            {
                if (string.IsNullOrEmpty(Properties.Settings.Default.PluginDataPath))
                {
                    string p = System.IO.Path.Combine(new string[] { System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GlobalcachingApplication" });
                    Properties.Settings.Default.PluginDataPath = p.TrimEnd(new char[] { '\\', '/' });
                }
                if (!System.IO.Directory.Exists(Properties.Settings.Default.PluginDataPath))
                {
                    System.IO.Directory.CreateDirectory(Properties.Settings.Default.PluginDataPath);
                }
                return Properties.Settings.Default.PluginDataPath; 
            }
            set
            {
                if (Properties.Settings.Default.PluginDataPath != value)
                {
                    if (Directory.Exists(value))
                    {
                        //assuming plugin saves data etc.
                        //engine contains no logic but to restart
                        PrepareClosingApplication();

                        foreach (System.Configuration.ApplicationSettingsBase settings in _allSettings)
                        {
                            settings.SettingsSaving -= new System.Configuration.SettingsSavingEventHandler(Default_SettingsSaving);
                        }

                        Properties.Settings.Default.PluginDataPath = value;
                        Properties.Settings.Default.Save();

                        System.Windows.Forms.Application.Restart();
                    }
                }
            }
        }

        public bool LoadLogsInBackground
        {
            get { return Properties.Settings.Default.LoadLogsInBackground; }
            set
            {
                Properties.Settings.Default.LoadLogsInBackground = value;
                Properties.Settings.Default.Save();
            }
        }

        public List<string> GetAvailableInternalStoragePlugins()
        {
            return _internalStoragePlugins;
        }

        public string ActiveInternalStoragePlugin
        {
            get { return Properties.Settings.Default.InternalStorageClass; }
            set
            {
                if (Properties.Settings.Default.InternalStorageClass != value)
                {
                    Properties.Settings.Default.InternalStorageClass = value;
                    Properties.Settings.Default.Save();
                }
            }
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
            Properties.Settings.Default.GCComAccountName = e.AccountInfo.AccountName;
            Properties.Settings.Default.GCComAccountToken = e.AccountInfo.APIToken;
            Properties.Settings.Default.GCComAccountTokenStaging = e.AccountInfo.APITokenStaging;
            Properties.Settings.Default.GCComAccountMemberType = e.AccountInfo.MemberType;
            Properties.Settings.Default.GCComAccountMemberTypeId = e.AccountInfo.MemberTypeId;
            Properties.Settings.Default.Save();
            OnGeocachingComAccountChanged(this);
        }

        public void InitiateUpdaterAndExit()
        {
            //check the type of update
            string updatePath = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GlobalcachingApplication", "Update");
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
            Properties.Settings.Default.HomeLat = e.Location.Lat;
            Properties.Settings.Default.HomeLon = e.Location.Lon;
            Properties.Settings.Default.Save();
        }

        void _centerLocation_Changed(object sender, Framework.EventArguments.LocationEventArgs e)
        {
            Properties.Settings.Default.CenterLat = e.Location.Lat;
            Properties.Settings.Default.CenterLon = e.Location.Lon;
            Properties.Settings.Default.Save();
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
            Properties.Settings.Default.CultureID = _selectedLanguage.LCID;
            Properties.Settings.Default.Save();
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

        public void ApplicationInitialized()
        {
            foreach (Framework.Interfaces.IPlugin plugin in _plugins)
            {
                try
                {
                    plugin.ApplicationInitialized();
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

        private bool checkDotNetRequirements()
        {
            bool result = false;

            try
            {
                using (RegistryKey v4 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full"))
                {
                    if (v4 != null)
                    {
                        object o = v4.GetValue("Install");
                        result = (int)o == 1;
                    }
                }
            }
            catch
            {
            }
            if (!result)
            {
                result = System.Windows.Forms.MessageBox.Show("This program requires .NET 4 Full, but has not been found on your machine. The program will not run properly. You will get error messages related to 'System.Web'.\r\nContinue anyway?", "Error", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Exclamation, System.Windows.Forms.MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes;
            }
            return result;
        }

        public bool Initialize()
        {
            bool result = false;
            try
            {
                if (_pluginDataFolderSelected && checkDotNetRequirements())
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
                                    result = plugin.Initialize(this);
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
                                        result = plugin.Initialize(this);
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
                    if (Properties.Settings.Default.CultureID != 0)
                    {
                        _selectedLanguage = new CultureInfo(Properties.Settings.Default.CultureID);
                    }
                    if (Properties.Settings.Default.CultureID != 127)
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
                    bool settingsRetrieved = false;
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
                            if (!isInternalStorage || Properties.Settings.Default.InternalStorageClass == t.FullName)
                            {
                                //if (Properties.Settings.Default.DisabledPlugins == null || !Properties.Settings.Default.DisabledPlugins.Contains(t.FullName))
                                {
                                    ConstructorInfo constructor = t.GetConstructor(new Type[0]);
                                    object[] parameters = new object[0];
                                    Framework.Interfaces.IPlugin plugin = (Framework.Interfaces.IPlugin)constructor.Invoke(parameters);
                                    pins.Add(plugin);

                                    if (!settingsRetrieved)
                                    {
                                        Type setType = asmbly.GetType(string.Format("{0}.Properties.Settings", asmbly.GetName().Name));
                                        if (setType != null)
                                        {
                                            PropertyInfo pi = setType.GetProperty("Default", BindingFlags.Public | BindingFlags.Static);
                                            if (pi != null)
                                            {
                                                System.Configuration.ApplicationSettingsBase settings = pi.GetValue(null, null) as System.Configuration.ApplicationSettingsBase;
                                                if (settings != null)
                                                {
                                                    //settings.
                                                    _allSettings.Add(settings);
                                                    PortableSettings.LoadSettings(PluginDataPath, settings);
                                                    PortableSettings.SaveSettings(PluginDataPath, settings);
                                                    settings.SettingsSaving += new System.Configuration.SettingsSavingEventHandler(Default_SettingsSaving);
                                                }
                                            }
                                        }
                                        settingsRetrieved = true;
                                    }
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
            System.Collections.Specialized.StringCollection sc = Properties.Settings.Default.DisabledPlugins;
            if (sc==null)
            {
                sc = new System.Collections.Specialized.StringCollection();
            }
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
                Properties.Settings.Default.DisabledPlugins = sc;
                Properties.Settings.Default.Save();
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
    }
}
