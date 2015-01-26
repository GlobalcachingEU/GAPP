using CefSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication
{
    public partial class FormMain : Form
    {
        private Core.Engine _engine = null;

        public FormMain()
        {
            InitializeComponent();
        }

        private void timerStart_Tick(object sender, EventArgs e)
        {
            timerStart.Enabled = false;

            _engine = new Core.Engine();
            _engine.LoadingAssembly +=new Core.Engine.LoadingAssemblyHandler(_engine_LoadingAssembly);
            _engine.InitializingPlugin += new Core.Engine.LoadingAssemblyHandler(_engine_InitializingPlugin);
            _engine.PluginAdded += new Framework.EventArguments.PluginEventHandler(_engine_PluginAdded);
            if (_engine.Initialize())
            {
                List<Framework.Interfaces.IPlugin> p = _engine.GetPlugin(Framework.PluginType.UIMainWindow);
                if (p == null || p.Count == 0)
                {
                    Close();
                }
                else
                {
                    Hide();
                    _engine.ApplicationInitialized();
                    string[] args = Environment.GetCommandLineArgs();
                    ProcessCommandLine((from s in args select s).Skip(1).ToArray());
                }
            }
            else
            {
                _engine.RestoreDefaultSettings();
                Close();
            }
        }

        void _engine_InitializingPlugin(object sender, string e)
        {
            label1.Text = string.Format("Initializing {0}", System.IO.Path.GetFileName(e));
            label1.Refresh();
        }

        void _engine_LoadingAssembly(object sender, string e)
        {
            label1.Text = string.Format("loading {0}",System.IO.Path.GetFileName(e));
            label1.Refresh();
        }

        public void ProcessCommandLine(string[] cmdLine)
        {
            List<Framework.Interfaces.IPlugin> p = _engine.GetPlugin(Framework.PluginType.UIMainWindow);
            (p[0] as Framework.Interfaces.IPluginUIMainWindow).OnCommandLineArguments(cmdLine);
        }

        void _engine_PluginAdded(object sender, Framework.EventArguments.PluginEventArgs e)
        {
            if (e.Plugin is Framework.Interfaces.IPluginUIMainWindow)
            {
                e.Plugin.Closing += new Framework.EventArguments.PluginEventHandler(Plugin_Closing);
            }
        }

        void Plugin_Closing(object sender, Framework.EventArguments.PluginEventArgs e)
        {
            _engine.PluginAdded -= new Framework.EventArguments.PluginEventHandler(_engine_PluginAdded);
            List<Framework.Interfaces.IPlugin> p = _engine.GetPlugin(Framework.PluginType.UIMainWindow);
            if (p == null || p.Count == 0 || (p.Count==1 && p[0]==e.Plugin))
            {
                Close();
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cef.Shutdown();
        }
    }
}
