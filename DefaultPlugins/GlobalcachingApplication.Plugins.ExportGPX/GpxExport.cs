using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.ExportGPX
{
    public class GpxExport: Utils.BasePlugin.BaseExportFilter
    {
        public const string STR_NOGEOCACHESELECTED = "No geocache selected for export";
        public const string STR_ERROR = "Error";
        public const string STR_EXPORTINGGPX = "Exporting GPX...";
        public const string STR_CREATINGFILE = "Creating file...";

        public const string ACTION_EXPORT_ALL = "Export GPX|All";
        public const string ACTION_EXPORT_SELECTED = "Export GPX|Selected";
        public const string ACTION_EXPORT_ACTIVE = "Export GPX|Active";

        private string _filename = null;
        private Utils.GPXGenerator _gpxGenerator = null;

        public override string FriendlyName
        {
            get { return "Export GPX"; }
        }

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            if (PluginSettings.Instance == null)
            {
                var p = new PluginSettings(core);
            }

            AddAction(ACTION_EXPORT_ALL);
            AddAction(ACTION_EXPORT_SELECTED);
            AddAction(ACTION_EXPORT_ACTIVE);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_EXPORTINGGPX));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CREATINGFILE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_GPXVERSION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_ZIPFILE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_ADDWPTTODESCR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_USEHINTSDESCR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_INCLNOTES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_ADDWAYPOINTS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_EXTRACOORDNAMEPREFIX));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_EXTRAINFO));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_MAXLOGS));

            return await base.InitializeAsync(core);
        }

        public async override Task<bool> ActionAsync(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_EXPORT_ALL || action == ACTION_EXPORT_SELECTED || action == ACTION_EXPORT_ACTIVE)
                {
                    List<Framework.Data.Geocache> gcList = null;
                    if (action == ACTION_EXPORT_ALL)
                    {
                        gcList = (from Framework.Data.Geocache a in Core.Geocaches select a).ToList();
                    }
                    else if (action == ACTION_EXPORT_SELECTED)
                    {
                        gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                    }
                    else
                    {
                        if (Core.ActiveGeocache != null)
                        {
                            gcList = new List<Framework.Data.Geocache>();
                            gcList.Add(Core.ActiveGeocache);
                        }
                    }
                    if (gcList == null || gcList.Count == 0)
                    {
                        System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                    else
                    {
                        using (System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog())
                        {
                            dlg.FileName = "";
                            if (PluginSettings.Instance.ZipFile)
                            {
                                dlg.Filter = "*.zip|*.zip";
                            }
                            else
                            {
                                dlg.Filter = "*.gpx|*.gpx";
                            }
                            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                _filename = dlg.FileName;
                                _gpxGenerator = new Utils.GPXGenerator(Core, gcList, string.IsNullOrEmpty(PluginSettings.Instance.GPXVersionStr) ? Utils.GPXGenerator.V101: Version.Parse(PluginSettings.Instance.GPXVersionStr));
                                _gpxGenerator.MaxNameLength = PluginSettings.Instance.MaxGeocacheNameLength;
                                _gpxGenerator.MinStartOfname = PluginSettings.Instance.MinStartOfGeocacheName;
                                _gpxGenerator.UseNameForGCCode = PluginSettings.Instance.UseNameAndNotCode;
                                _gpxGenerator.AddAdditionWaypointsToDescription = PluginSettings.Instance.AddWaypointsToDescription;
                                _gpxGenerator.UseHintsForDescription = PluginSettings.Instance.UseHintsForDescription;
                                _gpxGenerator.AddFieldnotesToDescription = PluginSettings.Instance.AddFieldNotesToDescription;
                                _gpxGenerator.ExtraCoordPrefix = PluginSettings.Instance.CorrectedNamePrefix;
                                _gpxGenerator.AddExtraInfoToDescription = PluginSettings.Instance.AddExtraInfoToDescription;
                                _gpxGenerator.MaxLogCount = PluginSettings.Instance.MaximumNumberOfLogs;
                                await PerformExport();
                            }
                        }
                    }
                }
            }
            return result;
        }

        protected override void ExportMethod()
        {
            string gpxFile;
            string wptFile;
            System.IO.TemporaryFile tmp = null;
            System.IO.TemporaryFile tmpwpt = null;
            if (PluginSettings.Instance.ZipFile)
            {
                tmp = new System.IO.TemporaryFile(false);
                gpxFile = tmp.Path;
                tmpwpt = new System.IO.TemporaryFile(false);
                wptFile = tmpwpt.Path;
            }
            else
            {
                gpxFile = _filename;
                wptFile = string.Format("{0}-wpts.gpx", gpxFile.Substring(0,gpxFile.Length-4));
            }
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_EXPORTINGGPX, STR_CREATINGFILE, _gpxGenerator.Count, 0))
            {
                DateTime nextUpdate = DateTime.Now.AddSeconds(2);
                //create file stream (if not zipped actual file and if zipped tmp file
                using (System.IO.StreamWriter sw = System.IO.File.CreateText(gpxFile))
                using (System.IO.StreamWriter swwp = System.IO.File.CreateText(wptFile))
                {
                    //generate header
                    sw.Write(_gpxGenerator.Start());
                    if (PluginSettings.Instance.AddWaypoints)
                    {
                        swwp.Write(_gpxGenerator.WaypointData());
                    }
                    //preserve mem and do for each cache the export
                    for (int i = 0; i < _gpxGenerator.Count; i++)
                    {
                        sw.WriteLine(_gpxGenerator.Next());
                        if (PluginSettings.Instance.AddWaypoints)
                        {
                            string s = _gpxGenerator.WaypointData();
                            if (!string.IsNullOrEmpty(s))
                            {
                                swwp.WriteLine(s);
                            }
                        }
                        if (DateTime.Now>=nextUpdate)
                        {
                            progress.UpdateProgress(STR_EXPORTINGGPX, STR_CREATINGFILE, _gpxGenerator.Count, i + 1);
                            nextUpdate = DateTime.Now.AddSeconds(2);
                        }
                    }
                    //finalize
                    sw.Write(_gpxGenerator.Finish());
                    if (PluginSettings.Instance.AddWaypoints)
                    {
                        swwp.Write(_gpxGenerator.Finish());
                    }
                }

                if (PluginSettings.Instance.ZipFile)
                {
                    try
                    {
                        List<string> filenames = new List<string>();
                        filenames.Add(gpxFile);
                        if (PluginSettings.Instance.AddWaypoints)
                        {
                            filenames.Add(wptFile);
                        }

                        using (ZipOutputStream s = new ZipOutputStream(System.IO.File.Create(_filename)))
                        {
                            s.SetLevel(9); // 0-9, 9 being the highest compression

                            byte[] buffer = new byte[4096];
                            bool wpt = false;

                            foreach (string file in filenames)
                            {

                                ZipEntry entry = new ZipEntry(System.IO.Path.GetFileName(wpt ? _filename.ToLower().Replace(".zip", "-wpts.gpx") : _filename.ToLower().Replace(".zip", ".gpx")));

                                entry.DateTime = DateTime.Now;
                                s.PutNextEntry(entry);

                                using (System.IO.FileStream fs = System.IO.File.OpenRead(file))
                                {
                                    int sourceBytes;
                                    do
                                    {
                                        sourceBytes = fs.Read(buffer, 0,
                                        buffer.Length);

                                        s.Write(buffer, 0, sourceBytes);

                                    } while (sourceBytes > 0);
                                }

                                wpt = true;
                            }
                            s.Finish();
                            s.Close();
                        }
                    }
                    catch
                    {
                    }
                }
                else
                {
                    if (!PluginSettings.Instance.AddWaypoints)
                    {
                        System.IO.File.Delete(wptFile);
                    }
                }
            }
            if (tmp != null)
            {
                tmp.Dispose();
                tmp = null;
            }
            if (tmpwpt != null)
            {
                tmpwpt.Dispose();
                tmpwpt = null;
            }
        }

        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            foreach (System.Windows.Forms.UserControl uc in configPanels)
            {
                if (uc is SettingsPanel)
                {
                    (uc as SettingsPanel).Apply();
                    break;
                }
            }
            return true;
        }

        public override List<System.Windows.Forms.UserControl> CreateConfigurationPanels()
        {
            List<System.Windows.Forms.UserControl> pnls = base.CreateConfigurationPanels();
            if (pnls == null) pnls = new List<System.Windows.Forms.UserControl>();
            pnls.Add(new SettingsPanel());
            return pnls;
        }

    }
}
