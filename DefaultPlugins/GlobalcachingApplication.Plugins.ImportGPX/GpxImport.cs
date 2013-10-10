using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.ImportGPX
{
    public class GpxImport: Utils.BasePlugin.BaseImportFilter
    {
        public const string STR_IMPORTING = "Importing GPX...";
        public const string STR_IMPORTINGDATA = "Importing file...";
        public const string STR_IMPORTINGGEOCACHES = "Importing geocaches...";
        public const string STR_IMPORTINGLOGS = "Importing logs...";
        public const string STR_IMPORTINGLOGIMAGES = "Importing log images...";
        public const string STR_IMPORTINGWAYPOINTS = "Importing waypoints...";


        protected const string ACTION_IMPORT = "Import GPX";
        private string[] _filenames = null;

        public override string FriendlyName
        {
            get { return "Import GPX"; }
        }

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_IMPORT);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGDATA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGGEOCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGLOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGLOGIMAGES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGWAYPOINTS));

            return base.Initialize(core);
        }

        protected override void InitUIMainWindow(Framework.Interfaces.IPluginUIMainWindow mainWindowPlugin)
        {
            base.InitUIMainWindow(mainWindowPlugin);

            mainWindowPlugin.FileDrop += new Framework.EventArguments.FileDropEventHandler(mainWindowPlugin_FileDrop);
            mainWindowPlugin.CommandLineArguments += new Framework.EventArguments.CommandLineEventHandler(mainWindowPlugin_CommandLineArguments);
        }

        void mainWindowPlugin_CommandLineArguments(object sender, Framework.EventArguments.CommandLineEventArgs e)
        {
            if (e.Arguments != null && e.Arguments.Length > 0)
            {
                _filenames = (from s in e.Arguments where s.ToLower().EndsWith(".gpx") || s.ToLower().EndsWith(".zip") select s).ToArray();
                if (_filenames.Length > 0)
                {
                    PerformImport();
                }
            }
        }

        void mainWindowPlugin_FileDrop(object sender, Framework.EventArguments.FileDropEventArgs e)
        {
            _filenames = (from s in e.FilePath where s.ToLower().EndsWith(".gpx") || s.ToLower().EndsWith(".zip") select s).ToArray();
            if (_filenames.Length > 0)
            {
                PerformImport();
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_IMPORT)
                {
                    using (System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog())
                    {
                        dlg.FileName = "";
                        dlg.Filter = "*.zip|*.zip|*.gpx|*.gpx";
                        dlg.Multiselect = true;
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            _filenames = dlg.FileNames;
                            PerformImport();
                        }
                    }
                }
            }
            return result;
        }

        protected override void ImportMethod()
        {
            using (Utils.ProgressBlock fixpr = new Utils.ProgressBlock(this, STR_IMPORTING, STR_IMPORTINGDATA, _filenames.Length, 0))
            {
                for (int fileindex = 0; fileindex < _filenames.Length; fileindex++)
                {
                    Utils.GPXProcessor gpxProcessor = new Utils.GPXProcessor(Core);
                    Utils.GPXProcessor.ResultData res = gpxProcessor.ProcessGeocachingComGPXFile(_filenames[fileindex]);
                    List<string> ignoredGeocaches = new List<string>();
                    List<string> ignoredLogs = new List<string>();
                    if (res != null && res.Geocaches.Count > 0)
                    {
                        using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_IMPORTING, STR_IMPORTINGGEOCACHES, res.Geocaches.Count, 0))
                        {
                            int index = 0;
                            int procStep = 0;
                            foreach (Framework.Data.Geocache gc in res.Geocaches)
                            {
                                if (!AddGeocache(gc, gpxProcessor.CachesGPXVersion))
                                {
                                    ignoredGeocaches.Add(gc.Code);
                                }
                                index++;
                                procStep++;
                                if (procStep >= 100)
                                {
                                    progress.UpdateProgress(STR_IMPORTING, STR_IMPORTINGGEOCACHES, res.Geocaches.Count, index);
                                    procStep = 0;
                                }
                            }
                        }
                    }
                    if (res != null && res.Waypoints.Count > 0)
                    {
                        using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_IMPORTING, STR_IMPORTINGWAYPOINTS, res.Geocaches.Count, 0))
                        {
                            int index = 0;
                            int procStep = 0;
                            foreach (Framework.Data.Waypoint wp in res.Waypoints)
                            {
                                if (!ignoredGeocaches.Contains(wp.GeocacheCode))
                                {
                                    AddWaypoint(wp);
                                }
                                index++;
                                procStep++;
                                if (procStep >= 200)
                                {
                                    progress.UpdateProgress(STR_IMPORTING, STR_IMPORTINGWAYPOINTS, res.Waypoints.Count, index);
                                    procStep = 0;
                                }
                            }
                        }
                    }
                    if (res != null && res.Logs.Count > 0)
                    {
                        using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_IMPORTING, STR_IMPORTINGLOGS, res.Logs.Count, 0))
                        {
                            int index = 0;
                            int procStep = 0;
                            foreach (Framework.Data.Log l in res.Logs)
                            {
                                if (!ignoredGeocaches.Contains(l.GeocacheCode))
                                {
                                    AddLog(l);
                                }
                                else
                                {
                                    ignoredLogs.Add(l.ID);
                                }
                                index++;
                                procStep++;
                                if (procStep >= 500)
                                {
                                    progress.UpdateProgress(STR_IMPORTING, STR_IMPORTINGLOGS, res.Logs.Count, index);
                                    procStep = 0;
                                }
                            }
                        }
                    }
                    if (res != null && res.LogImages.Count > 0)
                    {
                        using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_IMPORTING, STR_IMPORTINGLOGIMAGES, res.LogImages.Count, 0))
                        {
                            int index = 0;
                            int procStep = 0;
                            foreach (Framework.Data.LogImage l in res.LogImages)
                            {
                                if (!ignoredLogs.Contains(l.LogID))
                                {
                                    AddLogImage(l);
                                }
                                index++;
                                procStep++;
                                if (procStep >= 100)
                                {
                                    progress.UpdateProgress(STR_IMPORTING, STR_IMPORTINGLOGIMAGES, res.LogImages.Count, index);
                                    procStep = 0;
                                }
                            }
                        }
                    }
                    fixpr.UpdateProgress(STR_IMPORTING, STR_IMPORTINGDATA, _filenames.Length, fileindex + 1);
                }
            }
        }
    }
}
