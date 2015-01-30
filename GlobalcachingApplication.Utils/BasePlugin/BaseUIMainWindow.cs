using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Utils.BasePlugin
{
    public class BaseUIMainWindow: Plugin, Framework.Interfaces.IPluginUIMainWindow
    {
        private BaseUIMainWindowForm _uiMainWindowForm = null;

        public event Framework.EventArguments.FileDropEventHandler FileDrop;
        public event Framework.EventArguments.CommandLineEventHandler CommandLineArguments;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            bool result = await base.InitializeAsync(core);
            if (result)
            {
                _uiMainWindowForm = CreateUIMainWindowForm(core);
                if (_uiMainWindowForm != null)
                {
                    _uiMainWindowForm.Show();
                    _uiMainWindowForm.FormClosed += new System.Windows.Forms.FormClosedEventHandler(_uiMainWindowForm_FormClosed);
                }
            }
            return result;
        }

        public async override Task ApplicationInitializedAsync()
        {
            ImageSupport.Instance.UpdateImageSupportPlugins(Core);
            LanguageSupport.Instance.UpdateLanguageSupportPlugins(Core);
            GeometrySupport.Instance.UpdateGeometrySupportPlugins(Core);
            GeocacheIgnoreSupport.Instance.UpdateIgnoreGeocachesSupportPlugins(Core);
            GeocacheCollectionSupport.Instance.UpdateIgnoreGeocachesCollectionPlugins(Core);
            await base.ApplicationInitializedAsync();
        }

        public virtual void OnDropFile(string[] filename)
        {
            if (FileDrop != null)
            {
                FileDrop(this, new Framework.EventArguments.FileDropEventArgs(filename));
            }
        }

        public virtual void OnCommandLineArguments(string[] args)
        {
            if (CommandLineArguments != null)
            {
                CommandLineArguments(this, new Framework.EventArguments.CommandLineEventArgs(args));
            }
        }

        void _uiMainWindowForm_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            Close();
        }

        protected virtual BaseUIMainWindowForm CreateUIMainWindowForm(Framework.Interfaces.ICore core)
        {
            return (new BaseUIMainWindowForm(this, core));
        }

        public System.Windows.Forms.Form MainForm 
        {
            get { return _uiMainWindowForm; } 
        }

        public override void Close()
        {
            if (_uiMainWindowForm != null)
            {
                _uiMainWindowForm.Dispose();
                _uiMainWindowForm = null;
            }
            base.Close();
        }

        public override Framework.PluginType PluginType
        {
            get { return Framework.PluginType.UIMainWindow; }
        }

        public virtual void AddAction(Framework.Interfaces.IPlugin plugin, string action)
        {
            if (_uiMainWindowForm != null)
            {
                _uiMainWindowForm.AddAction(plugin, action);
            }
        }

        public virtual void AddAction(Framework.Interfaces.IPlugin plugin, string action, string subaction)
        {
            if (_uiMainWindowForm != null)
            {
                _uiMainWindowForm.AddAction(plugin, action, subaction);
            }
        }

        public virtual void RemoveAction(Framework.Interfaces.IPlugin plugin, string action)
        {
            if (_uiMainWindowForm != null)
            {
                _uiMainWindowForm.RemoveAction(plugin, action);
            }
        }

        public virtual void RemoveAction(Framework.Interfaces.IPlugin plugin, string action, string subAction)
        {
            if (_uiMainWindowForm != null)
            {
                _uiMainWindowForm.RemoveAction(plugin, action, subAction);
            }
        }

    }
}
