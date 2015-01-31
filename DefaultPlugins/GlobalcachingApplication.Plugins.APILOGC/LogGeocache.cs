using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.APILOGC
{
    public class LogGeocache: Utils.BasePlugin.Plugin
    {
        public const string STR_UNABLEACCESSAPI = "Unable to access the Live API or process its data";
        public const string STR_ERROR = "Error";

        public const string ACTION_SINGLE = "Log geocache|Single";
        public const string ACTION_BATCH = "Log geocache|Selected";
        public const string ACTION_OFFLINE = "Log geocache|Offline";
        public const string ACTION_GARMINVISIT = "Log geocache|Garmin geocache_visits.txt";
        public const string ACTION_CGEOVISIT = "Log geocache|c:geo visits file";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            var p = new PluginSettings(core);

            AddAction(ACTION_SINGLE);
            AddAction(ACTION_BATCH);
            AddAction(ACTION_GARMINVISIT);
            AddAction(ACTION_CGEOVISIT);
            AddAction(ACTION_OFFLINE);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_UNABLEACCESSAPI));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheLogForm.STR_DATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheLogForm.STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheLogForm.STR_FAIL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheLogForm.STR_FAILUNKNOWN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheLogForm.STR_GEOCACHECODE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheLogForm.STR_LOGTEXT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheLogForm.STR_LOGTYPE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheLogForm.STR_OKANOTHER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheLogForm.STR_SELECT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheLogForm.STR_SUBMIT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheLogForm.STR_SUCCESS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheLogForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheLogForm.STR_CLEARTALL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheLogForm.STR_SELECTALL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheLogForm.STR_TRACKABLES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheLogForm.STR_ADD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheLogForm.STR_REMOVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheLogForm.STR_ADDTOFAVORITES));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheBatchLogForm.STR_ADD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheBatchLogForm.STR_CLEARTALL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheBatchLogForm.STR_CODE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheBatchLogForm.STR_DATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheBatchLogForm.STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheBatchLogForm.STR_FAILUNKNOWN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheBatchLogForm.STR_GEOCACHECODE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheBatchLogForm.STR_LOGGING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheBatchLogForm.STR_LOGTEXT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheBatchLogForm.STR_LOGTYPE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheBatchLogForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheBatchLogForm.STR_SELECTALL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheBatchLogForm.STR_STARTFOUNDCNT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheBatchLogForm.STR_STOPATLOG));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheBatchLogForm.STR_SUBMIT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheBatchLogForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheBatchLogForm.STR_TRACKABLES));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageEditorForm.STR_BYTES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageEditorForm.STR_CAPTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageEditorForm.STR_DESCRIPTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageEditorForm.STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageEditorForm.STR_FILE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageEditorForm.STR_IMGTOADDTOLOG));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageEditorForm.STR_LIMITS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageEditorForm.STR_NOTSUPPORTEDIMAGETYPE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageEditorForm.STR_OK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageEditorForm.STR_ORIGINALIMAGE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageEditorForm.STR_SCALE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageEditorForm.STR_SIZEWH));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageEditorForm.STR_TILTE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ImageEditorForm.STR_QUALITY));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminGeocacheVisitsLogForm.STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminGeocacheVisitsLogForm.STR_GEOCACHEINFO));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminGeocacheVisitsLogForm.STR_IMPORTING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminGeocacheVisitsLogForm.STR_IMPORTMISSING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminGeocacheVisitsLogForm.STR_LOADFROMDEVICE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminGeocacheVisitsLogForm.STR_LOGSELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminGeocacheVisitsLogForm.STR_BATCHLOGSELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminGeocacheVisitsLogForm.STR_NO));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminGeocacheVisitsLogForm.STR_SELECTALL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminGeocacheVisitsLogForm.STR_SELECTFILE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminGeocacheVisitsLogForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminGeocacheVisitsLogForm.STR_PERESENT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminGeocacheVisitsLogForm.STR_CODE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminGeocacheVisitsLogForm.STR_DATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminGeocacheVisitsLogForm.STR_LOGTYPE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GarminGeocacheVisitsLogForm.STR_COMMENT));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(CGeoGeocacheVisitsLogForm.STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CGeoGeocacheVisitsLogForm.STR_GEOCACHEINFO));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CGeoGeocacheVisitsLogForm.STR_IMPORTING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CGeoGeocacheVisitsLogForm.STR_IMPORTMISSING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CGeoGeocacheVisitsLogForm.STR_LOADFROMDEVICE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CGeoGeocacheVisitsLogForm.STR_LOGSELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CGeoGeocacheVisitsLogForm.STR_BATCHLOGSELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CGeoGeocacheVisitsLogForm.STR_NO));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CGeoGeocacheVisitsLogForm.STR_SELECTALL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CGeoGeocacheVisitsLogForm.STR_SELECTFILE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CGeoGeocacheVisitsLogForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CGeoGeocacheVisitsLogForm.STR_PERESENT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CGeoGeocacheVisitsLogForm.STR_CODE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CGeoGeocacheVisitsLogForm.STR_DATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CGeoGeocacheVisitsLogForm.STR_LOGTYPE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(CGeoGeocacheVisitsLogForm.STR_COMMENT));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(OfflineLogForm.STR_ADD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(OfflineLogForm.STR_ADDALLSELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(OfflineLogForm.STR_ADDGEOCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(OfflineLogForm.STR_CODE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(OfflineLogForm.STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(OfflineLogForm.STR_GEOCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(OfflineLogForm.STR_LOGALL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(OfflineLogForm.STR_LOGDATE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(OfflineLogForm.STR_LOGSELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(OfflineLogForm.STR_LOGTEXT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(OfflineLogForm.STR_LOGTYPE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(OfflineLogForm.STR_OFFLINELOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(OfflineLogForm.STR_OK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(OfflineLogForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(OfflineLogForm.STR_LOGONLINE));

            return await base.InitializeAsync(core);
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SINGLE;
            }
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.LiveAPI;
            }
        }


        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                try
                {
                    //get from goundspeak
                    if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, false))
                    {
#if DEBUG
                        //using (var client = new Utils.API.GeocachingLiveV6(Core, true))
                        using (var client = new Utils.API.GeocachingLiveV6(Core))
#else
                        using (var client = new Utils.API.GeocachingLiveV6(Core))
#endif
                        {
                            if (action == ACTION_SINGLE)
                            {
                                using (GeocacheLogForm dlg = new GeocacheLogForm(Core, client, Core.ActiveGeocache))
                                {
                                    dlg.ShowDialog();
                                }
                            }
                            else if (action == ACTION_BATCH)
                            {
                                using (GeocacheBatchLogForm dlg = new GeocacheBatchLogForm(this, Core, client))
                                {
                                    dlg.ShowDialog();
                                }
                            }
                            else if (action == ACTION_GARMINVISIT)
                            {
                                using (GarminGeocacheVisitsLogForm dlg = new GarminGeocacheVisitsLogForm(this, Core, client))
                                {
                                    dlg.ShowDialog();
                                }
                            }
                            else if (action == ACTION_CGEOVISIT)
                            {
                                using (CGeoGeocacheVisitsLogForm dlg = new CGeoGeocacheVisitsLogForm(this, Core, client))
                                {
                                    dlg.ShowDialog();
                                }
                            }
                            else if (action == ACTION_OFFLINE)
                            {
                                using (OfflineLogForm dlg = new OfflineLogForm(this, Core, client))
                                {
                                    dlg.ShowDialog();
                                }
                            }
                        }
                    }
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_UNABLEACCESSAPI), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
            return result;
        }

    }
}
