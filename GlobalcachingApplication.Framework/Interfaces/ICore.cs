using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace GlobalcachingApplication.Framework.Interfaces
{
    public interface ICore
    {
        Version Version { get; }
        List<IPlugin> GetPlugin(PluginType pluginType);
        List<IPlugin> GetPlugins();
        event EventArguments.PluginEventHandler PluginAdded;
        List<string> GetAllDetectedPlugins();
        bool SetDisabledPlugins(string[] plugins);
        void PrepareClosingApplication();
        void InitiateUpdaterAndExit();
        List<string> GetAvailableInternalStoragePlugins();
        string ActiveInternalStoragePlugin { get; set; }
        string CSScriptsPath { get; set; }
        bool AutoSaveOnClose { get; set; }

        string PluginDataPath { get; set; }
        string[] AvailablePluginDataPaths { get; set; }
        bool EnablePluginDataPathAtStartup { get; set; }
        bool CreateSettingsInFolder(string folder, bool useCurrentSettings);

        event EventArguments.GeocacheEventHandler ActiveGeocacheChanged;
        Framework.Data.Geocache ActiveGeocache { get; set; }

        event EventArguments.GeocacheComAccountEventHandler GeocachingComAccountChanged;
        Framework.Data.GeocachingComAccountInfo GeocachingComAccount { get; }

        Framework.Data.GeocacheCollection Geocaches { get; }
        Framework.Data.WaypointCollection Waypoints { get; }
        Framework.Data.LogCollection Logs { get; }
        Framework.Data.LogImageCollection LogImages { get; }
        Framework.Data.GeocacheImageCollection GeocacheImages { get; }
        Framework.Data.UserWaypointCollection UserWaypoints { get; }
        Framework.Data.GeocacheAttributeCollection GeocacheAttributes { get; }
        Framework.Data.GeocacheTypeCollection GeocacheTypes { get; }
        Framework.Data.GeocacheContainerCollection GeocacheContainers { get; }
        Framework.Data.LogTypeCollection LogTypes { get; }
        Framework.Data.WaypointTypeCollection WaypointTypes { get; }
        Framework.Data.Location HomeLocation { get; }
        Framework.Data.Location CenterLocation { get; }
        Framework.Data.GPSLocation GPSLocation { get; }
        Framework.Data.LanguageItemCollection LanguageItems { get; }

        List<Framework.Data.ShortcutInfo> ShortcutInfo { get; }
        event EventHandler ShortcutInfoChanged;
        void OnShortcutInfoChanged();

        CultureInfo SelectedLanguage { get; set; }
        event EventHandler SelectedLanguageChanged;

        bool RetrieveAPIKey(Data.APIKey keyType);
        void ShowAboutDialog();

        void DebugLog(Data.DebugLogLevel level, Interfaces.IPlugin p, Exception e, string msg);
        event EventArguments.DebugLogEventHandler DebugLogAdded;

        bool LoadLogsInBackground { get; set; }
    }
}
