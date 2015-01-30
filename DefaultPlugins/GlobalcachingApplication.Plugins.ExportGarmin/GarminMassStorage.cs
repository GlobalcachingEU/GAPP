using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.ExportGarmin
{
    public class GarminMassStorage : Utils.BasePlugin.BaseExportFilter
    {
        public const string ACTION_EXPORT_ALL = "Export to Garmin GPSr|All";
        public const string ACTION_EXPORT_SELECTED = "Export to Garmin GPSr|Selected";
        public const string ACTION_EXPORT_ACTIVE = "Export to Garmin GPSr|Active";

        public const string STR_NOGEOCACHESELECTED = "No geocache selected for export";
        public const string STR_ERROR = "Error";
        public const string STR_EXPORTINGGPX = "Exporting GPX...";
        public const string STR_CREATINGFILE = "Creating file...";
        public const string STR_COPYINGFILE = "Copying file to device...";

        private bool _oneGeocachePerFile = false;
        private bool _addChildWaypoints = true;
        private bool _useName = false;
        private string _drive = null;
        private List<Framework.Data.Geocache> _gcList = null;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_EXPORT_ALL);
            AddAction(ACTION_EXPORT_SELECTED);
            AddAction(ACTION_EXPORT_ACTIVE);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_EXPORTINGGPX));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CREATINGFILE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectDeviceForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectDeviceForm.STR_SELECTDEVICE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectDeviceForm.STR_OK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectDeviceForm.STR_ADDCHILDWAYPOINTS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectDeviceForm.STR_SEPFILEPERGEOCACHE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectDeviceForm.STR_USENAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectDeviceForm.STR_ADDWPTTODESCR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectDeviceForm.STR_USEHINTSDESCR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectDeviceForm.STR_USEDATABASENAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectDeviceForm.STR_CREATEGGZFILE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectDeviceForm.STR_INCLNOTES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectDeviceForm.STR_GPXVERSION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectDeviceForm.STR_ADDIMAGES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectDeviceForm.STR_MAXLOGS));

            return await base.InitializeAsync(core);
        }

        protected override void ExportMethod()
        {
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_EXPORTINGGPX, STR_CREATINGFILE, _gcList.Count, 0))
            {
                if (_oneGeocachePerFile)
                {
                    Utils.GPXGenerator gpxGenerator = new Utils.GPXGenerator(Core, _gcList, string.IsNullOrEmpty(Properties.Settings.Default.GPXVersionStr) ? Utils.GPXGenerator.V101 : Version.Parse(Properties.Settings.Default.GPXVersionStr));
                    gpxGenerator.UseNameForGCCode = _useName;
                    gpxGenerator.AddAdditionWaypointsToDescription = Properties.Settings.Default.AddWaypointsToDescription;
                    gpxGenerator.UseHintsForDescription = Properties.Settings.Default.UseHintsForDescription;
                    gpxGenerator.AddFieldnotesToDescription = Properties.Settings.Default.AddFieldNotesToDescription;
                    gpxGenerator.MaxNameLength = Properties.Settings.Default.MaxGeocacheNameLength;
                    gpxGenerator.MinStartOfname = Properties.Settings.Default.MinStartOfGeocacheName;
                    gpxGenerator.ExtraCoordPrefix = Properties.Settings.Default.CorrectedNamePrefix;
                    gpxGenerator.AddExtraInfoToDescription = Properties.Settings.Default.AddExtraInfoToDescription;
                    gpxGenerator.MaxLogCount = Properties.Settings.Default.MaximumNumberOfLogs;
                    using (System.IO.TemporaryFile gpxFile = new System.IO.TemporaryFile(false))
                    {
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(gpxFile.Path, false, Encoding.UTF8))
                        {
                            int block = 0;
                            //generate header
                            sw.Write(gpxGenerator.Start());
                            //preserve mem and do for each cache the export
                            for (int i = 0; i < gpxGenerator.Count; i++)
                            {
                                sw.WriteLine(gpxGenerator.Next());
                                if (_addChildWaypoints)
                                {
                                    string s = gpxGenerator.WaypointData();
                                    if (!string.IsNullOrEmpty(s))
                                    {
                                        sw.WriteLine(s);
                                    }
                                }
                                block++;
                                if (block > 10)
                                {
                                    block = 0;
                                    progress.UpdateProgress(STR_EXPORTINGGPX, STR_CREATINGFILE, gpxGenerator.Count, i + 1);
                                }
                            }
                            //finalize
                            sw.Write(gpxGenerator.Finish());
                        }

                        progress.UpdateProgress(STR_EXPORTINGGPX, STR_COPYINGFILE, 1, 0);
                        string filename = "geocaches.gpx";
                        if (Properties.Settings.Default.UseDatabaseNameForFileName)
                        {
                            Framework.Interfaces.IPluginInternalStorage storage = (from Framework.Interfaces.IPluginInternalStorage a in Core.GetPlugin(Framework.PluginType.InternalStorage) select a).FirstOrDefault();
                            if (storage!=null)
                            {
                                var si = storage.ActiveStorageDestination;
                                if (si != null)
                                {
                                    string s = storage.ActiveStorageDestination.Name;
                                    int pos = s.LastIndexOf('.');
                                    if (pos < 0)
                                    {
                                        filename = string.Format("{0}.gpx",s);
                                    }
                                    else
                                    {
                                        filename = string.Format("{0}.gpx",s.Substring(0,pos));
                                    }
                                }
                            }
                        }
                        System.IO.File.Copy(gpxFile.Path, System.IO.Path.Combine(new string[] { _drive, "garmin", "gpx", filename }), true);
                    }
                }
                else
                {
                    List<Framework.Data.Geocache> gcl = new List<Framework.Data.Geocache>();
                    for (int i = 0; i < _gcList.Count; i++)
                    {
                        gcl.Clear();
                        gcl.Add(_gcList[i]);
                        Utils.GPXGenerator gpxGenerator = new Utils.GPXGenerator(Core, gcl, string.IsNullOrEmpty(Properties.Settings.Default.GPXVersionStr) ? Utils.GPXGenerator.V101 : Version.Parse(Properties.Settings.Default.GPXVersionStr));
                        gpxGenerator.UseNameForGCCode = _useName;
                        gpxGenerator.AddAdditionWaypointsToDescription = Properties.Settings.Default.AddWaypointsToDescription;
                        gpxGenerator.UseHintsForDescription = Properties.Settings.Default.UseHintsForDescription;
                        gpxGenerator.AddFieldnotesToDescription = Properties.Settings.Default.AddFieldNotesToDescription;
                        gpxGenerator.MaxNameLength = Properties.Settings.Default.MaxGeocacheNameLength;
                        gpxGenerator.MinStartOfname = Properties.Settings.Default.MinStartOfGeocacheName;
                        gpxGenerator.ExtraCoordPrefix = Properties.Settings.Default.CorrectedNamePrefix;
                        gpxGenerator.AddExtraInfoToDescription = Properties.Settings.Default.AddExtraInfoToDescription;
                        gpxGenerator.MaxLogCount = Properties.Settings.Default.MaximumNumberOfLogs;
                        using (System.IO.TemporaryFile gpxFile = new System.IO.TemporaryFile(true))
                        {
                            int block = 0;

                            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(gpxFile.Path, false, Encoding.ASCII))
                            {
                                //generate header
                                sw.Write(gpxGenerator.Start());
                                //preserve mem and do for each cache the export
                                sw.WriteLine(gpxGenerator.Next());
                                if (_addChildWaypoints)
                                {
                                    string s = gpxGenerator.WaypointData();
                                    if (!string.IsNullOrEmpty(s))
                                    {
                                        sw.WriteLine(s);
                                    }
                                }
                                //finalize
                                sw.Write(gpxGenerator.Finish());
                            }
                            System.IO.File.Copy(gpxFile.Path, System.IO.Path.Combine(new string[] { _drive, "garmin", "gpx", string.Format("{0}.gpx", gcl[0].Code) }), true);

                            block++;
                            if (block > 10)
                            {
                                block = 0;
                                progress.UpdateProgress(STR_EXPORTINGGPX, STR_CREATINGFILE, _gcList.Count, i + 1);
                            }
                        }
                    }
                }
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_EXPORT_ALL || action == ACTION_EXPORT_SELECTED || action == ACTION_EXPORT_ACTIVE)
                {
                    if (action == ACTION_EXPORT_ALL)
                    {
                        _gcList = (from Framework.Data.Geocache a in Core.Geocaches select a).ToList();
                    }
                    else if (action == ACTION_EXPORT_SELECTED)
                    {
                        _gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                    }
                    else
                    {
                        if (Core.ActiveGeocache != null)
                        {
                            _gcList = new List<Framework.Data.Geocache>();
                            _gcList.Add(Core.ActiveGeocache);
                        }
                    }
                    if (_gcList == null || _gcList.Count == 0)
                    {
                        System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                    else
                    {
                        using (SelectDeviceForm dlg = new SelectDeviceForm())
                        {
                            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                if (Properties.Settings.Default.CreateGGZFile)
                                {
                                    Utils.BasePlugin.Plugin p = Utils.PluginSupport.PluginByName(Core, "GlobalcachingApplication.Plugins.ExportGPX.GgzExport") as Utils.BasePlugin.Plugin;
                                    if (p != null)
                                    {
                                        var m = p.GetType().GetMethod("ExportToGGZ");
                                        if (m != null)
                                        {
                                            Utils.GPXGenerator gpxGenerator = new Utils.GPXGenerator(Core, _gcList, string.IsNullOrEmpty(Properties.Settings.Default.GPXVersionStr) ? Utils.GPXGenerator.V101 : Version.Parse(Properties.Settings.Default.GPXVersionStr));
                                            gpxGenerator.UseNameForGCCode = _useName;
                                            gpxGenerator.AddAdditionWaypointsToDescription = Properties.Settings.Default.AddWaypointsToDescription;
                                            gpxGenerator.UseHintsForDescription = Properties.Settings.Default.UseHintsForDescription;
                                            gpxGenerator.AddFieldnotesToDescription = Properties.Settings.Default.AddFieldNotesToDescription;
                                            gpxGenerator.MaxNameLength = Properties.Settings.Default.MaxGeocacheNameLength;
                                            gpxGenerator.MinStartOfname = Properties.Settings.Default.MinStartOfGeocacheName;
                                            gpxGenerator.ExtraCoordPrefix = Properties.Settings.Default.CorrectedNamePrefix;
                                            gpxGenerator.AddExtraInfoToDescription = Properties.Settings.Default.AddExtraInfoToDescription;
                                            gpxGenerator.MaxLogCount = Properties.Settings.Default.MaximumNumberOfLogs;

                                            string filename = "geocaches.ggz";
                                            if (Properties.Settings.Default.UseDatabaseNameForFileName)
                                            {
                                                Framework.Interfaces.IPluginInternalStorage storage = (from Framework.Interfaces.IPluginInternalStorage a in Core.GetPlugin(Framework.PluginType.InternalStorage) select a).FirstOrDefault();
                                                if (storage != null)
                                                {
                                                    var si = storage.ActiveStorageDestination;
                                                    if (si != null)
                                                    {
                                                        string s = storage.ActiveStorageDestination.Name;
                                                        int pos = s.LastIndexOf('.');
                                                        if (pos < 0)
                                                        {
                                                            filename = string.Format("{0}.ggz", s);
                                                        }
                                                        else
                                                        {
                                                            filename = string.Format("{0}.ggz", s.Substring(0, pos));
                                                        }
                                                    }
                                                }
                                            }
                                            try
                                            {
                                                if (!System.IO.Directory.Exists(System.IO.Path.Combine(new string[] { dlg.SelectedDrive, "garmin", "ggz" })))
                                                {
                                                    System.IO.Directory.CreateDirectory(System.IO.Path.Combine(new string[] { dlg.SelectedDrive, "garmin", "ggz" }));
                                                }
                                            }
                                            catch
                                            {
                                            }
                                            m.Invoke(p, new object[] { System.IO.Path.Combine(new string[] { dlg.SelectedDrive, "garmin", "ggz", filename }), _gcList, gpxGenerator });
                                        }
                                    }
                                }
                                else
                                {
                                    _oneGeocachePerFile = !dlg.SeperateFilePerGeocache;
                                    _drive = dlg.SelectedDrive;
                                    _addChildWaypoints = dlg.AddChildWaypoints;
                                    _useName = dlg.UseName;
                                    if (!string.IsNullOrEmpty(_drive))
                                    {
                                        PerformExport();
                                    }
                                }

                                if (Properties.Settings.Default.AddImages)
                                {
                                    Utils.BasePlugin.Plugin p = Utils.PluginSupport.PluginByName(Core, "GlobalcachingApplication.Plugins.ImgGrab.ImageGrabber") as Utils.BasePlugin.Plugin;
                                    if (p != null)
                                    {
                                        var m = p.GetType().GetMethod("CreateImageFolderForGeocaches");
                                        if (m != null)
                                        {
                                            m.Invoke(p, new object[] { _gcList, System.IO.Path.Combine(new string[] { dlg.SelectedDrive, "garmin" }) });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

    }
}
