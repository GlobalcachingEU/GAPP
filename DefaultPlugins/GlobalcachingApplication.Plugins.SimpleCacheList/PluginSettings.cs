using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.SimpleCacheList
{
    public class PluginSettings
    {
        public static PluginSettings _uniqueInstance = null;
        private ICore _core = null;

        public PluginSettings()
        {
        }

        public PluginSettings(ICore core)
        {
            _uniqueInstance = this;
            _core = core;
        }

        public static PluginSettings Default
        {
            get { return _uniqueInstance; }
        }

        public static PluginSettings Instance
        {
            get { return _uniqueInstance; }
        }

        public Rectangle WindowPos
        {
            get { return _core.SettingsProvider.GetSettingsValueRectangle("SimpleCacheList.WindowPos", Rectangle.Empty); }
            set { _core.SettingsProvider.SetSettingsValueRectangle("SimpleCacheList.WindowPos", value); }
        }

        public Color BkColorArchived
        {
            get { return _core.SettingsProvider.GetSettingsValueColor("SimpleCacheList.BkColorArchived", Color.Red); }
            set { _core.SettingsProvider.SetSettingsValueColor("SimpleCacheList.BkColorArchived", value); }
        }

        public Color BkColorNotAvailable
        {
            get { return _core.SettingsProvider.GetSettingsValueColor("SimpleCacheList.BkColorNotAvailable", Color.WhiteSmoke); }
            set { _core.SettingsProvider.SetSettingsValueColor("SimpleCacheList.BkColorNotAvailable", value); }
        }

        public Color BkColorAvailable
        {
            get { return _core.SettingsProvider.GetSettingsValueColor("SimpleCacheList.BkColorAvailable", Color.White); }
            set { _core.SettingsProvider.SetSettingsValueColor("SimpleCacheList.BkColorAvailable", value); }
        }

        public Color BkColorFound
        {
            get { return _core.SettingsProvider.GetSettingsValueColor("SimpleCacheList.BkColorFound", Color.FromArgb(192, 255, 192)); }
            set { _core.SettingsProvider.SetSettingsValueColor("SimpleCacheList.BkColorFound", value); }
        }

        public Color BkColorOwned
        {
            get { return _core.SettingsProvider.GetSettingsValueColor("SimpleCacheList.BkColorOwned", Color.FromArgb(255, 255, 128)); }
            set { _core.SettingsProvider.SetSettingsValueColor("SimpleCacheList.BkColorOwned", value); }
        }

        public Color BkColorExtraCoord
        {
            get { return _core.SettingsProvider.GetSettingsValueColor("SimpleCacheList.BkColorExtraCoord", Color.White); }
            set { _core.SettingsProvider.SetSettingsValueColor("SimpleCacheList.BkColorExtraCoord", value); }
        }

        public int ColumnNameWidth
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("SimpleCacheList.ColumnNameWidth", 200); }
            set { _core.SettingsProvider.SetSettingsValueInt("SimpleCacheList.ColumnNameWidth", value); }
        }

        public int ColumnOwnerWidth
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("SimpleCacheList.ColumnOwnerWidth", 100); }
            set { _core.SettingsProvider.SetSettingsValueInt("SimpleCacheList.ColumnOwnerWidth", value); }
        }

        public int ColumnCountryWidth
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("SimpleCacheList.ColumnCountryWidth", 100); }
            set { _core.SettingsProvider.SetSettingsValueInt("SimpleCacheList.ColumnCountryWidth", value); }
        }

        public int ColumnStateWidth
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("SimpleCacheList.ColumnStateWidth", 100); }
            set { _core.SettingsProvider.SetSettingsValueInt("SimpleCacheList.ColumnStateWidth", value); }
        }

        public int ColumnCityWidth
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("SimpleCacheList.ColumnCityWidth", 100); }
            set { _core.SettingsProvider.SetSettingsValueInt("SimpleCacheList.ColumnCityWidth", value); }
        }

        public int SortOnColumnIndex
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("SimpleCacheList.SortOnColumnIndex", -1); }
            set { _core.SettingsProvider.SetSettingsValueInt("SimpleCacheList.SortOnColumnIndex", value); }
        }

        public int SortDirection
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("SimpleCacheList.SortDirection", 0); }
            set { _core.SettingsProvider.SetSettingsValueInt("SimpleCacheList.SortDirection", value); }
        }

        public int ColumnPersonalNoteWidth
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("SimpleCacheList.ColumnPersonalNoteWidth", 100); }
            set { _core.SettingsProvider.SetSettingsValueInt("SimpleCacheList.ColumnPersonalNoteWidth", value); }
        }

        public int ColumnHintsWidth
        {
            get { return _core.SettingsProvider.GetSettingsValueInt("SimpleCacheList.ColumnHintsWidth", 300); }
            set { _core.SettingsProvider.SetSettingsValueInt("SimpleCacheList.ColumnHintsWidth", value); }
        }

        public string VisibleColumns
        {
            get { return _core.SettingsProvider.GetSettingsValue("SimpleCacheList.VisibleColumns", null); }
            set { _core.SettingsProvider.SetSettingsValue("SimpleCacheList.VisibleColumns", value); }
        }

        public string ColumnOrder
        {
            get { return _core.SettingsProvider.GetSettingsValue("SimpleCacheList.ColumnOrder", null); }
            set { _core.SettingsProvider.SetSettingsValue("SimpleCacheList.ColumnOrder", value); }
        }

        public bool DeferredScrolling
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("SimpleCacheList.DeferredScrolling", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("SimpleCacheList.DeferredScrolling", value); }
        }

        public bool EnableAutomaticSorting
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("SimpleCacheList.EnableAutomaticSorting", true); }
            set { _core.SettingsProvider.SetSettingsValueBool("SimpleCacheList.EnableAutomaticSorting", value); }
        }

        public bool AutoTopPanel
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("SimpleCacheList.AutoTopPanel", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("SimpleCacheList.AutoTopPanel", value); }
        }


    }
}
