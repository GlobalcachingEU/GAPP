using GlobalcachingApplication.Framework;
using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.UIMainWindow
{
    public class PluginSettings
    {
        public static PluginSettings _uniqueInstance = null;
        private ICore _core = null;

        public PluginSettings(ICore core)
        {
            _uniqueInstance = this;
            _core = core;
        }

        public static PluginSettings Instance
        {
            get { return _uniqueInstance; }
        }

        public Rectangle WindowPos
        {
            get { return _core.SettingsProvider.GetSettingsValueRectangle("UIMainWindow.WindowPos", Rectangle.Empty); }
            set { _core.SettingsProvider.SetSettingsValueRectangle("UIMainWindow.WindowPos", value); }
        }

        public bool WindowMaximized
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("UIMainWindow.WindowMaximized", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("UIMainWindow.WindowMaximized", value); }
        }
        public bool ToolbarFile
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("UIMainWindow.ToolbarFile", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("UIMainWindow.ToolbarFile", value); }
        }
        public bool ToolbarSearch
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("UIMainWindow.ToolbarSearch", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("UIMainWindow.ToolbarSearch", value); }
        }
        public bool ToolbarScripts
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("UIMainWindow.ToolbarScripts", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("UIMainWindow.ToolbarScripts", value); }
        }
        public bool ToolbarWindows
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("UIMainWindow.ToolbarWindows", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("UIMainWindow.ToolbarWindows", value); }
        }
        public bool ToolbarMaps
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("UIMainWindow.ToolbarMaps", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("UIMainWindow.ToolbarMaps", value); }
        }
        public bool ToolbarLiveAPI
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("UIMainWindow.ToolbarLiveAPI", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("UIMainWindow.ToolbarLiveAPI", value); }
        }
        public bool ToolbarCustom
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("UIMainWindow.ToolbarCustom", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("UIMainWindow.ToolbarCustom", value); }
        }
        public bool ToolbarActions
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("UIMainWindow.ToolbarActions", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("UIMainWindow.ToolbarActions", value); }
        }
        public bool ToolbarActionSequence
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("UIMainWindow.ToolbarActionSequence", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("UIMainWindow.ToolbarActionSequence", value); }
        }
        public bool ShowOKAPIMenu
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("UIMainWindow.ShowOKAPIMenu", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("UIMainWindow.ShowOKAPIMenu", value); }
        }

        public Point ToolbarFileLocation
        {
            get { return _core.SettingsProvider.GetSettingsValuePoint("UIMainWindow.ToolbarFileLocation", Point.Empty); }
            set { _core.SettingsProvider.SetSettingsValuePoint("UIMainWindow.ToolbarFileLocation", value); }
        }

        public Point ToolbarSearchLocation
        {
            get { return _core.SettingsProvider.GetSettingsValuePoint("UIMainWindow.ToolbarSearchLocation", new Point(100, 0)); }
            set { _core.SettingsProvider.SetSettingsValuePoint("UIMainWindow.ToolbarSearchLocation", value); }
        }
        public Point ToolbarScriptsLocation
        {
            get { return _core.SettingsProvider.GetSettingsValuePoint("UIMainWindow.ToolbarScriptsLocation", Point.Empty); }
            set { _core.SettingsProvider.SetSettingsValuePoint("UIMainWindow.ToolbarScriptsLocation", value); }
        }
        public Point ToolbarWindowsLocation
        {
            get { return _core.SettingsProvider.GetSettingsValuePoint("UIMainWindow.ToolbarWindowsLocation", Point.Empty); }
            set { _core.SettingsProvider.SetSettingsValuePoint("UIMainWindow.ToolbarWindowsLocation", value); }
        }
        public Point ToolbarMapsLocation
        {
            get { return _core.SettingsProvider.GetSettingsValuePoint("UIMainWindow.ToolbarMapsLocation", Point.Empty); }
            set { _core.SettingsProvider.SetSettingsValuePoint("UIMainWindow.ToolbarMapsLocation", value); }
        }
        public Point ToolbarLiveAPIlocation
        {
            get { return _core.SettingsProvider.GetSettingsValuePoint("UIMainWindow.ToolbarLiveAPIlocation", Point.Empty); }
            set { _core.SettingsProvider.SetSettingsValuePoint("UIMainWindow.ToolbarLiveAPIlocation", value); }
        }
        public Point ToolbarCustomLocation
        {
            get { return _core.SettingsProvider.GetSettingsValuePoint("UIMainWindow.ToolbarCustomLocation", Point.Empty); }
            set { _core.SettingsProvider.SetSettingsValuePoint("UIMainWindow.ToolbarCustomLocation", value); }
        }
        public Point ToolbarActionsLocation
        {
            get { return _core.SettingsProvider.GetSettingsValuePoint("UIMainWindow.ToolbarActionsLocation", Point.Empty); }
            set { _core.SettingsProvider.SetSettingsValuePoint("UIMainWindow.ToolbarActionsLocation", value); }
        }
        public Point ToolbarActionSequenceLocation
        {
            get { return _core.SettingsProvider.GetSettingsValuePoint("UIMainWindow.ToolbarActionSequenceLocation", Point.Empty); }
            set { _core.SettingsProvider.SetSettingsValuePoint("UIMainWindow.ToolbarActionSequenceLocation", value); }
        }

        public InterceptedStringCollection ToolbarCustomButtons
        {
            get { return _core.SettingsProvider.GetSettingsValueStringCollection("UIMainWindow.ToolbarCustomButtons", null); }
            set { _core.SettingsProvider.SetSettingsValueStringCollection("UIMainWindow.ToolbarCustomButtons", value); }
        }

        public string SelectedTranslater
        {
            get { return _core.SettingsProvider.GetSettingsValue("UIMainWindow.SelectedTranslater", "en"); }
            set { _core.SettingsProvider.SetSettingsValue("UIMainWindow.SelectedTranslater", value); }
        }
    }
}
