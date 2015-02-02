using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSScriptLibrary;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.Script
{
    public class ScriptRunner : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SETTINGS = "Select Folder";
        public const string ACTION_REFRESH = "Refresh";
        public const string ACTION_SEP = "-";

        private Hashtable _availableScripts = new Hashtable();
        private System.IO.FileSystemWatcher _fsWatcher = null;
        private SynchronizationContext _context = null;
        private bool _allAssembliesLoaded = false;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            var p = new PluginSettings(core);

            //AddAction(ACTION_SETTINGS);
            AddAction(ACTION_REFRESH);
            AddAction(ACTION_SEP);

            _context = SynchronizationContext.Current;
            if (_context == null)
            {
                _context = new SynchronizationContext();
            }

            if (string.IsNullOrEmpty(PluginSettings.Instance.UserScriptsFolder))
            {
                try
                {
                    //first use
                    //copy the scripts to appdata
                    string srcFolder = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Scripts");
                    string destFolder = core.CSScriptsPath;
                    if (!Directory.Exists(destFolder))
                    {
                        Directory.CreateDirectory(destFolder);
                    }
                    if (Directory.Exists(srcFolder))
                    {
                        string[] files = Directory.GetFiles(srcFolder, "*.cs");
                        foreach (string f in files)
                        {
                            File.Copy(f, Path.Combine(destFolder, Path.GetFileName(f)), true);
                        }
                    }
                    PluginSettings.Instance.UserScriptsFolder = destFolder;
                }
                catch
                {
                }
            }

            CSScriptLibrary.CSScript.ShareHostRefAssemblies = true;
            CSScriptLibrary.CSScript.CacheEnabled = true;

            return await base.InitializeAsync(core);
        }

        private void InitFileSystemWatcher()
        {
            if (_fsWatcher != null)
            {
                _fsWatcher.Dispose();
                _fsWatcher = null;
            }
            _fsWatcher = new FileSystemWatcher(PluginSettings.Instance.UserScriptsFolder, "*.cs");
            _fsWatcher.IncludeSubdirectories = false;
            _fsWatcher.Created += new FileSystemEventHandler(_fsWatcher_Created);
            _fsWatcher.Renamed += new RenamedEventHandler(_fsWatcher_Renamed);
            _fsWatcher.Deleted += new FileSystemEventHandler(_fsWatcher_Deleted);
            _fsWatcher.EnableRaisingEvents = true;
        }

        void _fsWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            _context.Send(new SendOrPostCallback(delegate(object state)
            {
                string s = e.FullPath;
                string actionName = string.Format("{0}", Path.GetFileNameWithoutExtension(s));
                if (_availableScripts[actionName] != null)
                {
                    RemoveAction(actionName);
                    Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
                    main.RemoveAction(this, actionName);
                    _availableScripts.Remove(actionName);
                }
            }), null);
        }

        void _fsWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            _context.Send(new SendOrPostCallback(delegate(object state)
            {
                string s = e.OldFullPath;
                string actionName = string.Format("{0}", Path.GetFileNameWithoutExtension(s));
                Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
                if (_availableScripts[actionName] != null)
                {
                    RemoveAction(actionName);
                    main.RemoveAction(this, actionName);
                    _availableScripts.Remove(actionName);
                }

                s = e.FullPath;
                if (!Path.GetFileName(s).StartsWith("_"))
                {
                    actionName = string.Format("{0}", Path.GetFileNameWithoutExtension(s));
                    if (_availableScripts[actionName] == null)
                    {
                        AddAction(actionName);
                        main.AddAction(this, actionName);

                        _availableScripts.Add(actionName, s);
                    }
                }
            }), null);
        }

        void _fsWatcher_Created(object sender, FileSystemEventArgs e)
        {
            _context.Send(new SendOrPostCallback(delegate(object state)
            {
                string s = e.FullPath;
                if (!Path.GetFileName(s).StartsWith("_"))
                {
                    string actionName = string.Format("{0}", Path.GetFileNameWithoutExtension(s));
                    if (_availableScripts[actionName] == null)
                    {
                        AddAction(actionName);
                        Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
                        main.AddAction(this, actionName);

                        _availableScripts.Add(actionName, s);
                    }
                }
            }), null);
        }

        public async override Task ApplicationInitializedAsync()
        {
            LoadFolder(PluginSettings.Instance.UserScriptsFolder);
            InitFileSystemWatcher();
            await base.ApplicationInitializedAsync();
        }

        public override void ApplicationClosing()
        {
            if (_fsWatcher != null)
            {
                _fsWatcher.Dispose();
                _fsWatcher = null;
            }
            base.ApplicationClosing();
        }

        private void ClearActions()
        {
            Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
            foreach (var s in _availableScripts.Keys)
            {
                RemoveAction(s.ToString());
                main.RemoveAction(this, s.ToString());
            }
            _availableScripts.Clear();
        }

        private void LoadFolder(string folder)
        {
            try
            {
                string[] files = Directory.GetFiles(folder, "*.cs");
                if (files != null)
                {
                    Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
                    foreach (string s in files)
                    {
                        if (!Path.GetFileName(s).StartsWith("_"))
                        {
                            string actionName = string.Format("{0}",Path.GetFileNameWithoutExtension(s));

                            AddAction(actionName);
                            main.AddAction(this, actionName);

                            _availableScripts.Add(actionName,s);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Script;
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result && action == ACTION_SETTINGS)
            {
                using (System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog())
                {
                    if (dlg.ShowDialog()== System.Windows.Forms.DialogResult.OK)
                    {
                        ClearActions();
                        PluginSettings.Instance.UserScriptsFolder = dlg.SelectedPath;
                        Core.CSScriptsPath = PluginSettings.Instance.UserScriptsFolder;
                        LoadFolder(PluginSettings.Instance.UserScriptsFolder);
                        InitFileSystemWatcher();
                    }
                }
            }
            else if (result && action == ACTION_REFRESH)
            {
                ClearActions();
                LoadFolder(PluginSettings.Instance.UserScriptsFolder);
            }
            else
            {
                try
                {
                    if (!_allAssembliesLoaded)
                    {
                        using (Utils.API.GeocachingLiveV6 client = new Utils.API.GeocachingLiveV6(Core))
                        {
                            //to load assembly
                        }
                        string s = System.Web.HttpUtility.HtmlEncode("hallo");
                        s = "";
                        _allAssembliesLoaded = true;
                    }
                    AsmHelper scriptAsm = new AsmHelper(CSScript.Load(_availableScripts[action].ToString()));
                    result = (bool)scriptAsm.Invoke("Script.Run", new object[] { this, Core });
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message, "Error");
                }
            }
            return result;
        }
    }
}
