using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Framework.Interfaces
{
    public interface IPlugin
    {
        event EventArguments.PluginEventHandler Closing;
        event EventArguments.ProgressEventHandler StartProgress;
        event EventArguments.ProgressEventHandler UpdateProgress;
        event EventArguments.ProgressEventHandler EndProgress;
        event EventArguments.NotificationEventHandler Notification;

        bool Prerequisite { get; }
        Task<bool> InitializeAsync(ICore core);
        void Close();
        PluginType PluginType { get; }
        Version Version { get; }
        CultureInfo CultureInfo { get; }
        string FriendlyName { get; }
        string DefaultAction { get; }
        bool IsHelpAvailable { get; }

        List<System.Windows.Forms.UserControl> CreateConfigurationPanels();
        bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels);
        Task<bool> ActionAsync(string action);
        bool ActionEnabled(string action, int selectCount, bool active);
        List<string> GetActionSubactionList(char subActionSeperator);

        void ShowHelp();

        Task PluginsInitializedAsync();
        Task ApplicationInitializedAsync();
        void ApplicationClosing();
    }
}
