using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data.Common;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.APIPQ
{
    public class ImportPQ : Utils.BasePlugin.BaseImportFilter
    {
        public const string STR_DOWNLOADING_PQ = "Downloading PQ...";
        public const string STR_IMPORTING = "Importing GPX...";
        public const string STR_IMPORTINGDATA = "Importing file...";
        public const string STR_IMPORTINGGEOCACHES = "Importing geocaches...";
        public const string STR_IMPORTINGLOGS = "Importing logs...";
        public const string STR_IMPORTINGLOGIMAGES = "Importing log images...";
        public const string STR_IMPORTINGWAYPOINTS = "Importing waypoints...";
        public const string STR_UNABLEACCESSAPI = "Unable to access the Live API or process its data";
        public const string STR_ERROR = "Error";

        public const string ACTION_IMPORT = "Import Pocket Queries";

        private string _databaseFile = null;

        private List<Utils.API.LiveV6.PQData> _pqs = null;
        private Utils.API.GeocachingLiveV6 _client = null;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_IMPORT);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectPQForm.STR_Count));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectPQForm.STR_DOWNLOADABLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectPQForm.STR_GENERATED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectPQForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectPQForm.STR_OK));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectPQForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectPQForm.STR_TYPE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectPQForm.STR_PROCESSED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectPQForm.STR_UNSELECTALL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SelectPQForm.STR_SELECTALL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_DOWNLOADING_PQ));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGDATA));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGGEOCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGLOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGLOGIMAGES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_IMPORTINGWAYPOINTS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_UNABLEACCESSAPI));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));

            try
            {
                _databaseFile = System.IO.Path.Combine(new string[] { core.PluginDataPath, "importPQ.db3" });
            }
            catch
            {
                _databaseFile = null;
            }


            return await base.InitializeAsync(core);
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_IMPORT;
            }
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.LiveAPI;
            }
        }

        private void initDatabase(Utils.DBCon dbcon)
        {
            object o = dbcon.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='processedpq'");
            if (o == null || o.GetType() == typeof(DBNull))
            {
                dbcon.ExecuteNonQuery("create table 'processedpq' (pqname text, processdate text)");
            }
        }

        private Hashtable getProcessedPq()
        {
            Hashtable result = new Hashtable();
            if (_databaseFile != null)
            {
                try
                {
                    using (Utils.DBCon dbcon = new Utils.DBConComSqlite(_databaseFile))
                    {
                        initDatabase(dbcon);
                        DbDataReader dr = dbcon.ExecuteReader("select * from processedpq");
                        while (dr.Read())
                        {
                            result.Add(dr["pqname"], DateTime.Parse((string)dr["processdate"]));
                        }
                    }
                }
                catch
                {
                }
            }
            return result;
        }

        private void updateProcessedPq(string pqName)
        {
            if (_databaseFile != null)
            {
                try
                {
                    using (Utils.DBCon dbcon = new Utils.DBConComSqlite(_databaseFile))
                    {
                        initDatabase(dbcon);

                        //delete older than 8 days
                        DateTime weekAgo = DateTime.Now.AddDays(-8);
                        dbcon.ExecuteNonQuery(string.Format("delete from processedpq where processdate<'{0}'", weekAgo.ToString("s")));

                        if (dbcon.ExecuteNonQuery(string.Format("update processedpq set processdate='{0}' where pqname='{1}'", DateTime.Now.ToString("s"), pqName.Replace("'","''")))==0)
                        {
                            dbcon.ExecuteNonQuery(string.Format("insert into processedpq (pqname, processdate) values ('{1}', '{0}')", DateTime.Now.ToString("s"), pqName.Replace("'", "''")));
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private Utils.API.LiveV6.PQData[] getAvailablePocketQueries(Utils.API.GeocachingLiveV6 client)
        {
            Utils.API.LiveV6.PQData[] result = null;
            try
            {
                Utils.API.LiveV6.GetPocketQueryListResponse resp = client.Client.GetPocketQueryList(client.Token);
                if (resp.Status.StatusCode == 0)
                {
                    result = resp.PocketQueryList;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(resp.Status.StatusMessage, Utils.LanguageSupport.Instance.GetTranslation("Error"), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
            catch(Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message, Utils.LanguageSupport.Instance.GetTranslation("Error"), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            return result;
        }


        protected override void ImportMethod()
        {
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_DOWNLOADING_PQ, STR_DOWNLOADING_PQ, _pqs.Count, 0))
            {
                int index = 0;
                try
                {
                    foreach (Utils.API.LiveV6.PQData pq in _pqs)
                    {
                        progress.UpdateProgress(STR_DOWNLOADING_PQ, STR_DOWNLOADING_PQ, _pqs.Count, index);
                        Utils.API.LiveV6.GetPocketQueryZippedFileResponse resp = _client.Client.GetPocketQueryZippedFile(_client.Token, pq.GUID);
                        if (resp.Status.StatusCode == 0)
                        {
                            using (System.IO.TemporaryFile tf = new System.IO.TemporaryFile(true))
                            {
                                System.IO.File.WriteAllBytes(tf.Path, Convert.FromBase64String(resp.ZippedFile));
                                processFile(tf.Path);
                                updateProcessedPq(pq.Name);
                            }
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show(resp.Status.StatusMessage, Utils.LanguageSupport.Instance.GetTranslation("Error"), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                            break;
                        }
                        index++;
                    }
                }
                catch(Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message, Utils.LanguageSupport.Instance.GetTranslation("Error"), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
        }


        protected void processFile(string filename)
        {
            using (Utils.ProgressBlock fixpr = new Utils.ProgressBlock(this, STR_IMPORTING, STR_IMPORTINGDATA, 1, 0))
            {
                Utils.GPXProcessor gpxProcessor = new Utils.GPXProcessor(Core);
                Utils.GPXProcessor.ResultData res = gpxProcessor.ProcessGeocachingComGPXFile(filename);
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
            }
        }


        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result && action == ACTION_IMPORT)
            {
                try
                {
                    //get from goundspeak
                    if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, true))
                    {
                        using (Utils.API.GeocachingLiveV6 client = new Utils.API.GeocachingLiveV6(Core, false))
                        {
                            Utils.API.LiveV6.PQData[] pqData = getAvailablePocketQueries(client);
                            if (pqData!=null)
                            {
                                using (SelectPQForm dlg = new SelectPQForm(pqData, getProcessedPq()))
                                {
                                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                    {
                                        _pqs = dlg.SelectedPQs;
                                        _client = client;
                                        PerformImport();
                                    }
                                }
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message, Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
            return result;
        }
    }
}
