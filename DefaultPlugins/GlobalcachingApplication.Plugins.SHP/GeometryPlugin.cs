using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.SHP
{
    public class GeometryPlugin : Utils.BasePlugin.Plugin, Framework.Interfaces.IGeometry
    {
        private ShapeFilesManager _shapeFilesManager = null;

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Geometry;
            }
        }

        public override string FriendlyName
        {
            get
            {
                return "Shapefiles";
            }
        }

        public override List<System.Windows.Forms.UserControl> CreateConfigurationPanels()
        {
            List<System.Windows.Forms.UserControl> pnls = base.CreateConfigurationPanels();
            if (pnls == null) pnls = new List<System.Windows.Forms.UserControl>();
            pnls.Add(new SettingsPanel(this));
            return pnls;
        }


        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            SettingsPanel panel = (from p in configPanels where p.GetType() == typeof(SettingsPanel) select p).FirstOrDefault() as SettingsPanel;
            panel.Apply();
            _shapeFilesManager.Initialize();
            return true;
        }

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            bool result = false;

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }
            if (Properties.Settings.Default.ShapeFiles == null)
            {
                Properties.Settings.Default.ShapeFiles = new System.Collections.Specialized.StringCollection();
                Properties.Settings.Default.Save();
            }

            try
            {
                string p = core.PluginDataPath;
                if (!System.IO.Directory.Exists(p))
                {
                    System.IO.Directory.CreateDirectory(p);
                }
                p = System.IO.Path.Combine(new string[] { p, "Shapefiles" });
                if (!System.IO.Directory.Exists(p))
                {
                    System.IO.Directory.CreateDirectory(p);
                }
                if (string.IsNullOrEmpty(Properties.Settings.Default.DefaultShapeFilesFolder))
                {
                    Properties.Settings.Default.DefaultShapeFilesFolder = p;
                    Properties.Settings.Default.Save();
                }
            }
            catch
            {
            }


            if (base.Initialize(core))
            {
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_ADD));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_CITY));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_COUNTRY));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_DELETE));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_DUTCHGRID));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_ERROR));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_FORMAT));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_MUNICIPALITY));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_NAMEFIELD));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_NAMEPREFIX));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_OTHER));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_PARSEERROR));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_SHAPEFILE));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_SHAPEFILESTOUSE));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_STATE));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_TYPE));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_WGS84));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_DOWNLOADMORE));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_ENCODING));

                core.LanguageItems.Add(new Framework.Data.LanguageItem(DownloadShapefileForm.STR_DOWNLOAD));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(DownloadShapefileForm.STR_DOWNLOADINGLIST));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(DownloadShapefileForm.STR_DOWNLOADINGSHAPEFILE));
                core.LanguageItems.Add(new Framework.Data.LanguageItem(DownloadShapefileForm.STR_TITLE));

                _shapeFilesManager = new ShapeFilesManager();
                _shapeFilesManager.Initialize();
                result = true;
            }
            return result;
        }

        public List<Framework.Data.AreaInfo> GetAreasOfLocation(Framework.Data.Location loc)
        {
            return _shapeFilesManager.GetAreasOfLocation(loc);
        }

        public List<Framework.Data.AreaInfo> GetAreasOfLocation(Framework.Data.Location loc, List<Framework.Data.AreaInfo> inAreas)
        {
            return _shapeFilesManager.GetAreasOfLocation(loc, inAreas);
        }

        public List<Framework.Data.AreaInfo> GetEnvelopAreasOfLocation(Framework.Data.Location loc)
        {
            return _shapeFilesManager.GetEnvelopAreasOfLocation(loc);
        }

        public List<Framework.Data.AreaInfo> GetEnvelopAreasOfLocation(Framework.Data.Location loc, List<Framework.Data.AreaInfo> inAreas)
        {
            return _shapeFilesManager.GetEnvelopAreasOfLocation(loc, inAreas);
        }

        public List<Framework.Data.AreaInfo> GetAreasByName(string name)
        {
            return _shapeFilesManager.GetAreasByName(name);
        }

        public List<Framework.Data.AreaInfo> GetAreasByName(string name, Framework.Data.AreaType level)
        {
            return _shapeFilesManager.GetAreasByName(name, level);
        }

        public List<Framework.Data.AreaInfo> GetAreasByID(object id)
        {
            return _shapeFilesManager.GetAreasByID(id);
        }

        public List<Framework.Data.AreaInfo> GetAreasByParentID(object parentid)
        {
            //not supported
            List<Framework.Data.AreaInfo> result = new List<Framework.Data.AreaInfo>();
            return result;
        }

        public List<Framework.Data.AreaInfo> GetAreasByLevel(Framework.Data.AreaType level)
        {
            return _shapeFilesManager.GetAreasByLevel(level);
        }

        public void GetPolygonOfArea(Framework.Data.AreaInfo area)
        {
            _shapeFilesManager.GetPolygonOfArea(area);
        }
    }
}
