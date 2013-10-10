using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Framework.Interfaces
{
    public interface IPluginUIMainWindow: IPlugin
    {
        event EventArguments.FileDropEventHandler FileDrop;
        event EventArguments.CommandLineEventHandler CommandLineArguments;

        void AddAction(IPlugin plugin, string action);
        void AddAction(IPlugin plugin, string action, string subAction);
        void RemoveAction(IPlugin plugin, string action);
        void RemoveAction(IPlugin plugin, string action, string subAction);
        Form MainForm { get; }
        void OnCommandLineArguments(string[] args);
    }
}
