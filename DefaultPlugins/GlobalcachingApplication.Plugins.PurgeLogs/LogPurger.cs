using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.PurgeLogs
{
    public class LogPurger : Utils.BasePlugin.Plugin
    {
        public const string STR_PURGINGLOGS = "Purge logs";

        public const string ACTION_FILTER = "Purge logs|Filter...";
        public const string ACTION_QUICKPURGE = "Purge logs|Keep last 5";

        private ManualResetEvent _actionReady = null;

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Action;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_QUICKPURGE;
            }
        }

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_FILTER);
            AddAction(ACTION_QUICKPURGE);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogPurgerForm.STR_DAYS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogPurgerForm.STR_KEEPATLEAST));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogPurgerForm.STR_KEEPLOGSOF));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogPurgerForm.STR_KEEPOFOWNED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogPurgerForm.STR_KEEPOWNLOGS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogPurgerForm.STR_MONTHS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogPurgerForm.STR_OLDERTHAN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogPurgerForm.STR_REMOVELOGSFROM));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(LogPurgerForm.STR_TITLE));

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            return await base.InitializeAsync(core);
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result && action == ACTION_FILTER)
            {
                using (LogPurgerForm dlg = new LogPurgerForm())
                {
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        using (Utils.FrameworkDataUpdater upd = new Utils.FrameworkDataUpdater(Core))
                        {
                            _actionReady = new ManualResetEvent(false);
                            Thread thrd = new Thread(new ThreadStart(this.purgeLogsWithFilterThreadMethod));
                            thrd.Start();
                            while (!_actionReady.WaitOne(10))
                            {
                                System.Windows.Forms.Application.DoEvents();
                            }
                            thrd.Join();
                            _actionReady.Dispose();
                            _actionReady = null;
                        }
                    }
                }
            }
            else if (result && action == ACTION_QUICKPURGE)
            {
                using (Utils.FrameworkDataUpdater upd = new Utils.FrameworkDataUpdater(Core))
                {
                    _actionReady = new ManualResetEvent(false);
                    Thread thrd = new Thread(new ThreadStart(this.purgeLogsThreadMethod));
                    thrd.Start();
                    while (!_actionReady.WaitOne(10))
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                    thrd.Join();
                    _actionReady.Dispose();
                    _actionReady = null;
                }
            }
            return result;
        }

        private void purgeLogsWithFilterThreadMethod()
        {
            try
            {
                int max = Core.Geocaches.Count;
                int index = 0;
                using (Utils.ProgressBlock prog = new Utils.ProgressBlock(this, STR_PURGINGLOGS, STR_PURGINGLOGS, max, index, true))
                {
                    //first remove logs of a certain logger
                    if (Properties.Settings.Default.RemoveAllLogsFrom != null && Properties.Settings.Default.RemoveAllLogsFrom.Count > 0)
                    {
                        string[] names = (from string s in Properties.Settings.Default.RemoveAllLogsFrom select s.ToLower()).ToArray();
                        List<Framework.Data.Log> logs = (from Framework.Data.Log l in Core.Logs
                                                         where names.Contains(l.Finder.ToLower())
                                                         select l).ToList();
                        foreach (var l in logs)
                        {
                            Utils.DataAccess.DeleteLog(Core, l);
                        }
                    }
                    string me = Core.GeocachingComAccount.AccountName;
                    string[] keeps;
                    bool keepMine = Properties.Settings.Default.KeepOwnLogs;
                    if (Properties.Settings.Default.KeepLogsOf == null)
                    {
                        keeps = new string[0];
                    }
                    else
                    {
                        keeps = (from string s in Properties.Settings.Default.KeepLogsOf select s.ToLower()).ToArray();
                    }
                    DateTime dt;
                    if (Properties.Settings.Default.DaysMonths == 0)
                    {
                        dt = DateTime.Now.AddDays(-Properties.Settings.Default.DaysMonthsCount);
                    }
                    else
                    {
                        dt = DateTime.Now.AddMonths(-Properties.Settings.Default.DaysMonthsCount);
                    }
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        if (!Properties.Settings.Default.KeepAllOfOwned || gc.Owner != me)
                        {
                            List<Framework.Data.Log> lgs = Utils.DataAccess.GetLogs(Core.Logs, gc.Code);
                            if (lgs != null)
                            {
                                var logs = (from l in lgs
                                            where (!keepMine || l.Finder != me) && !keeps.Contains(l.Finder.ToLower())
                                            select l).Skip(Properties.Settings.Default.KeepAtLeast);
                                foreach (var l in logs)
                                {
                                    if (l.Date < dt)
                                    {
                                        Utils.DataAccess.DeleteLog(Core, l);
                                    }
                                }
                            }
                        }
                        index++;
                        if (index % 50 == 0)
                        {
                            if (!prog.UpdateProgress(STR_PURGINGLOGS, STR_PURGINGLOGS, max, index))
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            _actionReady.Set();
        }

        private void purgeLogsThreadMethod()
        {
            try
            {
                int max = Core.Geocaches.Count;
                int index = 0;
                using (Utils.ProgressBlock prog = new Utils.ProgressBlock(this, STR_PURGINGLOGS, STR_PURGINGLOGS, max, index, true))
                {
                    string me = Core.GeocachingComAccount.AccountName;
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        /*
                        List<Framework.Data.Log> logs = (from Framework.Data.Log l in Core.Logs
                                                         where l.GeocacheCode == gc.Code && l.Finder != Core.GeocachingComAccount.AccountName
                                                         orderby l.Date descending
                                                         select l).Skip(5).ToList();
                         */
                        List<Framework.Data.Log> lgs = Utils.DataAccess.GetLogs(Core.Logs, gc.Code);
                        if (lgs != null)
                        {
                            var logs = (from l in lgs
                                        where l.Finder != me
                                        select l).Skip(5);
                            foreach (var l in logs)
                            {
                                Utils.DataAccess.DeleteLog(Core, l);
                            }
                        }
                        index++;
                        if (index % 50 == 0)
                        {
                            if (!prog.UpdateProgress(STR_PURGINGLOGS, STR_PURGINGLOGS, max, index))
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            _actionReady.Set();
        }
    }
}
