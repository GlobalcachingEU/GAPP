using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Utils.BasePlugin
{
    public partial class BaseUIMainWindowForm : Form
    {
        //object that can be hooked to an UI event like menu click
        //this on its term can be used to determine of menu items should disapear when plugin is removed
        public class PluginAction
        {
            public Framework.Interfaces.IPlugin plugin;
            public string action;
            public string subaction = null;
            public object Tag;

            public override string ToString()
            {
                return string.IsNullOrEmpty(subaction) ? Utils.LanguageSupport.Instance.GetTranslation(action) : string.Format("{0} {1}", Utils.LanguageSupport.Instance.GetTranslation(action), Utils.LanguageSupport.Instance.GetTranslation(subaction));
            }
        }
        protected List<PluginAction> _pluginActionList;
        private Framework.Interfaces.ICore _core = null;
        private BaseUIMainWindow _ownerPlugin = null;

        public BaseUIMainWindowForm()
        {
            InitializeComponent();

            _pluginActionList = new List<PluginAction>();
        }

        protected Framework.Interfaces.ICore Core
        {
            get { return _core; }
        }
        protected BaseUIMainWindow OwnerPlugin
        {
            get { return _ownerPlugin; }
        }

        public BaseUIMainWindowForm(BaseUIMainWindow ownerPlugin, Framework.Interfaces.ICore core): this()
        {
            _core = core;
            _ownerPlugin = ownerPlugin;
            LanguageSupport.Instance.UpdateLanguageSupportPlugins(core);
            core.PluginAdded += new Framework.EventArguments.PluginEventHandler(core_PluginAdded);
            core.SelectedLanguageChanged += new EventHandler(core_SelectedLanguageChanged);
            Utils.ImageSupport.Instance.UpdateImageSupportPlugins(core);
            Utils.GeometrySupport.Instance.UpdateGeometrySupportPlugins(core);
        }

        void core_SelectedLanguageChanged(object sender, EventArgs e)
        {
            SelectedLanguageChanged(sender, e);
        }

        void core_PluginAdded(object sender, Framework.EventArguments.PluginEventArgs e)
        {
            if (e.Plugin.PluginType == Framework.PluginType.LanguageSupport)
            {
                LanguageSupport.Instance.UpdateLanguageSupportPlugins(Core);
            }
            else if (e.Plugin.PluginType == Framework.PluginType.ImageResource)
            {
                Utils.ImageSupport.Instance.UpdateImageSupportPlugins(Core);
            }
            else if (e.Plugin.PluginType == Framework.PluginType.Geometry)
            {
                Utils.GeometrySupport.Instance.UpdateGeometrySupportPlugins(Core);
            }
        }

        protected virtual void SelectedLanguageChanged(object sender, EventArgs e)
        {
        }

        public virtual void RemoveAction(Framework.Interfaces.IPlugin plugin, string action)
        {
            PluginAction pa = (from p in _pluginActionList where p.plugin==plugin && p.action==action select p).FirstOrDefault();
            if (pa!=null)
            {
                RemovePluginAction(pa);
                _pluginActionList.Remove(pa);
            }
        }

        public virtual void RemoveAction(Framework.Interfaces.IPlugin plugin, string action, string subAction)
        {
            PluginAction pa = (from p in _pluginActionList where p.plugin == plugin && p.action == action && p.subaction == subAction select p).FirstOrDefault();
            if (pa != null)
            {
                RemovePluginAction(pa);
                _pluginActionList.Remove(pa);
            }
        }

        public virtual void AddAction(Framework.Interfaces.IPlugin plugin, string action)
        {
            PluginAction pa = new PluginAction();
            pa.plugin = plugin;
            pa.action = action;
            pa.Tag = null;
            _pluginActionList.Add(pa);

            plugin.Closing += new Framework.EventArguments.PluginEventHandler(plugin_Closing);
            AddPluginAction(pa);
        }

        public virtual void AddAction(Framework.Interfaces.IPlugin plugin, string action, string subaction)
        {
            PluginAction pa = new PluginAction();
            pa.plugin = plugin;
            pa.action = action;
            pa.subaction = subaction;
            pa.Tag = null;
            _pluginActionList.Add(pa);

            plugin.Closing += new Framework.EventArguments.PluginEventHandler(plugin_Closing);
            AddPluginAction(pa);
        }

        public virtual void AddPluginAction(PluginAction pa)
        {
        }

        public virtual void RemovePluginAction(PluginAction pa)
        {
        }

        void plugin_Closing(object sender, Framework.EventArguments.PluginEventArgs e)
        {
            for (int i = 0; i < _pluginActionList.Count; i++)
            {
                if (_pluginActionList[i].plugin == e.Plugin)
                {
                    RemovePluginAction(_pluginActionList[i]);
                    _pluginActionList.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        private void BaseUIMainWindowForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_core != null)
            {
                _core.SelectedLanguageChanged -= new EventHandler(core_SelectedLanguageChanged);
            }
        }

    }
}
