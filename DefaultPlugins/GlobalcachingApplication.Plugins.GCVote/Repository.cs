using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Data.Common;
using System.Globalization;
using System.Threading;

namespace GlobalcachingApplication.Plugins.GCVote
{
    public class Repository
    {
        private static Repository _uniqueInstance = null;
        private static object _lockObject = new object();

        private Framework.Interfaces.ICore _core = null;
        private Utils.DBCon _dbcon = null;
        private Hashtable _availableWaypoints = null;
        private Utils.BasePlugin.Plugin _activePlugin = null;
        private ManualResetEvent _actionDone = null;
        private bool _gcVoteActivated = false;

        private Repository()
        {
        }

        public static Repository Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            _uniqueInstance = new Repository();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        public void Initialize(Framework.Interfaces.ICore core)
        {
            _core = core;
            _availableWaypoints = new Hashtable();

            try
            {
                _dbcon = new Utils.DBConComSqlite(Path.Combine(core.PluginDataPath,"gcvote.db3"));

                object o = _dbcon.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='votes'");
                if (o == null || o.GetType() == typeof(DBNull))
                {
                    _dbcon.ExecuteNonQuery("create table 'votes' (Waypoint text, VoteMedian float, VoteAvg float, VoteCnt integer, VoteUser double)");
                    _dbcon.ExecuteNonQuery("create unique index idx_votess on votes (Waypoint)");
                }

                DbDataReader dr = _dbcon.ExecuteReader("select Waypoint from votes");
                while (dr.Read())
                {
                    _availableWaypoints.Add(dr[0], true);
                }
            }
            catch
            {
                _dbcon = null;
                _availableWaypoints.Clear();
            }
        }

        public void ActivateGCVote(Utils.BasePlugin.Plugin p)
        {
            if (!_gcVoteActivated)
            {
                _gcVoteActivated = true;
                _core.Geocaches.BeginUpdate();
                try
                {
                    _core.Geocaches.AddCustomAttribute(Import.CUSTOM_ATTRIBUTE);
                    LoadGCVoteData(p);
                }
                catch
                {
                }
                _core.Geocaches.EndUpdate();
            }
        }

        public void DeactivateGCVote()
        {
            if (_gcVoteActivated)
            {
                _gcVoteActivated = false;
                _core.Geocaches.BeginUpdate();
                try
                {
                    _core.Geocaches.DeleteCustomAttribute(Import.CUSTOM_ATTRIBUTE);
                }
                catch
                {
                }
                _core.Geocaches.EndUpdate();
            }
        }

        public void ClearAllData()
        {
            try
            {
                _dbcon.ExecuteNonQuery("delete from votes");
                _availableWaypoints.Clear();
            }
            catch
            {
            }
            if (_gcVoteActivated)
            {
                _core.Geocaches.BeginUpdate();
                try
                {
                    foreach (Framework.Data.Geocache gc in _core.Geocaches)
                    {
                        bool saved = gc.Saved;
                        gc.SetCustomAttribute(Import.CUSTOM_ATTRIBUTE, "");
                        gc.Saved = saved;
                    }
                }
                catch
                {
                }
                _core.Geocaches.EndUpdate();
            }
        }

        public void LoadGCVoteData(Utils.BasePlugin.Plugin p)
        {
            _activePlugin = p;
            _actionDone = new ManualResetEvent(false);
            _core.Geocaches.BeginUpdate();
            Thread thrd = new Thread(new ThreadStart(this.LoadGCVoteDataThreadMethod));
            thrd.Start();
            while (!_actionDone.WaitOne(100))
            {
                System.Windows.Forms.Application.DoEvents();
            }
            thrd.Join();
            _actionDone.Dispose();
            _actionDone = null;
            _core.Geocaches.EndUpdate();
        }

        public void LoadGCVoteDataThreadMethod()
        {
            try
            {
                using (Utils.ProgressBlock prog = new Utils.ProgressBlock(_activePlugin,Import.STR_IMPORT,Import.STR_IMPORT,_availableWaypoints.Count,0))
                {
                    int pos = 0;
                    DbDataReader dr = _dbcon.ExecuteReader("select Waypoint, VoteAvg, VoteCnt, VoteUser from votes");
                    while (dr.Read())
                    {
                        pos++;
                        string wp = (string)dr["Waypoint"];
                        Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(_core.Geocaches, wp);
                        if (gc != null)
                        {
                            double avg = (double)dr["VoteAvg"];
                            double usrVote = (double)dr["VoteUser"];
                            int cnt = (int)dr["VoteCnt"];

                            bool saved = gc.Saved;
                            if (usrVote > 0.1)
                            {
                                gc.SetCustomAttribute(Import.CUSTOM_ATTRIBUTE, string.Format("{0:0.0}/{1} ({2:0.0})", avg, cnt, usrVote));
                            }
                            else
                            {
                                gc.SetCustomAttribute(Import.CUSTOM_ATTRIBUTE, string.Format("{0:0.0}/{1}", avg, cnt));
                            }
                            gc.Saved = saved;
                            if (pos % 500 == 0)
                            {
                                prog.UpdateProgress(Import.STR_IMPORT, Import.STR_IMPORT, _availableWaypoints.Count, pos);
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            _actionDone.Set();
        }

        public bool GetGCVote(string Waypoint, out double VoteMedian, out double VoteAvg, out int VoteCnt, out double VoteUser)
        {
            bool result = false;
            VoteMedian = 0;
            VoteAvg = 0;
            VoteCnt = 0;
            VoteUser = 0;
            try
            {
                DbDataReader dr = _dbcon.ExecuteReader(string.Format("select VoteMedian, VoteAvg, VoteCnt, VoteUser from votes where Waypoint='{0}'", Waypoint));
                if (dr.Read())
                {
                    VoteMedian = (double)dr["VoteMedian"];
                    VoteAvg = (double)dr["VoteAvg"];
                    VoteCnt = (int)dr["VoteCnt"];
                    VoteUser = (double)dr["VoteUser"];
                    result = true;
                }
            }
            catch
            {
            }
            return result;
        }

        public void StoreGCVote(string Waypoint, double VoteMedian, double VoteAvg, int VoteCnt, double VoteUser)
        {
            if (_dbcon != null)
            {
                try
                {
                    if (_availableWaypoints[Waypoint] == null)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into votes (Waypoint, VoteMedian, VoteAvg, VoteCnt, VoteUser) values ('{0}', {1}, {2}, {3}, {4})", Waypoint, VoteMedian.ToString(CultureInfo.InvariantCulture), VoteAvg.ToString(CultureInfo.InvariantCulture), VoteCnt, VoteUser.ToString(CultureInfo.InvariantCulture)));
                        _availableWaypoints.Add(Waypoint, true);
                    }
                    else
                    {
                        _dbcon.ExecuteNonQuery(string.Format("update votes Set VoteMedian={1}, VoteAvg={2}, VoteCnt={3}, VoteUser={4} where Waypoint='{0}'", Waypoint, VoteMedian.ToString(CultureInfo.InvariantCulture), VoteAvg.ToString(CultureInfo.InvariantCulture), VoteCnt, VoteUser.ToString(CultureInfo.InvariantCulture)));
                    }
                }
                catch
                {
                }
            }
        }
    }
}
