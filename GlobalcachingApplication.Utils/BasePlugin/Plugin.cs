using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace GlobalcachingApplication.Utils.BasePlugin
{
    public class Plugin: Framework.Interfaces.IPlugin
    {
        public  const char SubActionSep = '|';

        private CultureInfo _cultureInfo = null;
        private Framework.Interfaces.ICore _core = null;
        private List<string> _actions = new List<string>();

        public virtual bool Prerequisite 
        {
            get { return false; } 
        }

        public virtual bool Initialize(Framework.Interfaces.ICore core)
        {
            return Initialize(core, null);
        }

        public virtual void ApplicationInitialized()
        {
        }

        public virtual void ApplicationClosing()
        {
        }

        public virtual List<string> GetActionSubactionList(char subActionSeperator)
        {
            return (from s in _actions where s!="-" && !s.EndsWith("|-") select s.Replace(SubActionSep,subActionSeperator)).ToList();
        }

        public virtual bool Initialize(Framework.Interfaces.ICore core, string[] actions)
        {
            bool result = false;
            _core = core;
            if (actions != null && actions.Length > 0)
            {
                _actions.AddRange(actions);
            }
            if (_actions != null && _actions.Count > 0)
            {
                foreach (string s in _actions)
                {
                    string[] parts = s.Split(new char[] { SubActionSep });
                    _core.LanguageItems.AddTextRange(parts.ToList());
                }
            }
            _core.LanguageItems.AddText(FriendlyName);
            if (RequiredCoreVersion <= core.Version)
            {
                List<Framework.Interfaces.IPlugin> p = core.GetPlugin(Framework.PluginType.UIMainWindow);
                if (p != null)
                {
                    foreach (Framework.Interfaces.IPluginUIMainWindow mwp in p)
                    {
                        InitUIMainWindow(mwp);
                    }
                }
                core.PluginAdded += new Framework.EventArguments.PluginEventHandler(_core_PluginAdded);
                result = true;
            }
            return result;
        }

        void _core_PluginAdded(object sender, Framework.EventArguments.PluginEventArgs e)
        {
            PluginAdded(e.Plugin);
        }

        protected virtual void PluginAdded(Framework.Interfaces.IPlugin plugin)
        {
            if (plugin != null && plugin.PluginType == Framework.PluginType.UIMainWindow)
            {
                InitUIMainWindow(plugin as Framework.Interfaces.IPluginUIMainWindow);
            }
        }

        public virtual string FriendlyName 
        {
            get { return System.Reflection.Assembly.GetAssembly(this.GetType()).GetName().Name; } 
        }

        public virtual string DefaultAction
        {
            get { return ""; }
        }


        protected virtual Version RequiredCoreVersion
        {
            get { return new Version(1, 0, 0, 0); }
        }

        protected Framework.Interfaces.ICore Core
        {
            get { return _core; }
        }

        public virtual CultureInfo CultureInfo 
        {
            get
            {
                if (_cultureInfo == null)
                {
                    _cultureInfo = new CultureInfo(""); //invariant
                }
                return _cultureInfo;
            }
        }

        public virtual void Close()
        {
            Core.PluginAdded -= new Framework.EventArguments.PluginEventHandler(_core_PluginAdded);
            OnClosing();
        }

        protected void AddAction(string action)
        {
            _actions.Add(action);
        }

        protected void RemoveAction(string action)
        {
            if (_actions.Contains(action))
            {
                _actions.Remove(action);
                Framework.Interfaces.IPluginUIMainWindow mainWindowPlugin = (from p in Core.GetPlugin(Framework.PluginType.UIMainWindow) select p).FirstOrDefault() as Framework.Interfaces.IPluginUIMainWindow;
                if (mainWindowPlugin != null)
                {
                    string[] parts = action.Split(new char[] { SubActionSep });
                    if (parts.Length == 1)
                    {
                        mainWindowPlugin.RemoveAction(this, parts[0]);
                    }
                    else
                    {
                        mainWindowPlugin.RemoveAction(this, parts[0], parts[1]);
                    }
                }
            }
        }

        protected virtual void InitUIMainWindow(Framework.Interfaces.IPluginUIMainWindow mainWindowPlugin)
        {
            //add menu items
            foreach (string s in _actions)
            {
                string[] parts = s.Split(new char[] { SubActionSep });
                if (parts.Length == 1)
                {
                    mainWindowPlugin.AddAction(this, parts[0]);
                }
                else
                {
                    mainWindowPlugin.AddAction(this, parts[0], parts[1]);
                }
            }
        }

        public virtual Framework.PluginType PluginType
        {
            get { return Framework.PluginType.Unknown; }
        }

        public virtual Version Version
        {
            get { return new Version(1,0,0,0); }
        }

        public event Framework.EventArguments.PluginEventHandler Closing;

        protected virtual void OnClosing()
        {
            if (Closing != null)
            {
                Closing(this, new Framework.EventArguments.PluginEventArgs(this));
            }
        }

        public virtual bool Action(string action)
        {
            return true;
        }

        public virtual bool ActionEnabled(string action, int selectCount, bool active)
        {
            bool result = true;
            if (action.EndsWith("|Selected"))
            {
                result = selectCount > 0;
            }
            else if (action.EndsWith("|Active"))
            {
                result = active;
            }
            else if (action.EndsWith("|All"))
            {
                result = Core.Geocaches.Count > 0;
            }
            else if (this.GetType().ToString().StartsWith("GlobalcachingApplication.Plugins.API"))
            {
                result = !string.IsNullOrEmpty(Core.GeocachingComAccount.APIToken);
            }
            return result;
        }

        public virtual List<System.Windows.Forms.UserControl> CreateConfigurationPanels()
        {
            return null;
        }
        public virtual bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            return true;
        }


        public event Framework.EventArguments.ProgressEventHandler StartProgress;
        public event Framework.EventArguments.ProgressEventHandler UpdateProgress;
        public event Framework.EventArguments.ProgressEventHandler EndProgress;
        public event Framework.EventArguments.NotificationEventHandler Notification;

        public virtual void OnNotification(Controls.NotificationMessageBox msgBox)
        {
            if (Notification != null)
            {
                Notification(this, new Framework.EventArguments.NotificationEventArgs(this, msgBox));
            }
            else
            {
                if (msgBox != null)
                {
                    msgBox.Dispose();
                }
            }
        }

        public virtual void OnStartProgress(string actionTitle, string actionText, int max, int position)
        {
            OnStartProgress(actionTitle, actionText, max, position, false);
        }
        public virtual void OnStartProgress(string actionTitle, string actionText, int max, int position, bool canCancel)
        {
            if (StartProgress != null)
            {
                StartProgress(this, new Framework.EventArguments.ProgressEventArgs(this, actionTitle, actionText, max, position, canCancel));
            }
        }
        public virtual bool OnUpdateProgress(string actionTitle, string actionText, int max, int position)
        {
            bool result = true;
            if (UpdateProgress != null)
            {
                Framework.EventArguments.ProgressEventArgs e = new Framework.EventArguments.ProgressEventArgs(this, actionTitle, actionText, max, position);
                UpdateProgress(this, e);
                result = !e.Cancel;
            }
            return result;
        }
        public virtual void OnEndProgress(string actionTitle, string actionText, int max, int position)
        {
            if (EndProgress != null)
            {
                EndProgress(this, new Framework.EventArguments.ProgressEventArgs(this, actionTitle, actionText, max, position));
            }
        }

        public virtual bool IsHelpAvailable {
            get {
                return false;
            }
        }

        public virtual void ShowHelp()
        {
        }

    }
}
