using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Commands
{
    public class ForSelectedGeocachesCommand : AsyncDelegateCommand
    {
        private Action<Core.Data.Geocache> _geocacheAction;

        private bool _showProgress;

        public ForSelectedGeocachesCommand(Action<Core.Data.Geocache> geocacheAction)
            : this(geocacheAction, false)
        {
        }
        public ForSelectedGeocachesCommand(Action<Core.Data.Geocache> geocacheAction, bool showProgress)
            : base(null, param => Core.ApplicationData.Instance.ActiveDatabase != null && Core.ApplicationData.Instance.MainWindow.GeocacheSelectionCount>0)
        {
            _showProgress = showProgress;
            _geocacheAction = geocacheAction;
        }

        public override async Task ExecuteAsync(object parameter)
        {
            Core.ApplicationData.Instance.BeginActiviy();
            Core.Storage.Database db = Core.ApplicationData.Instance.ActiveDatabase;
            if (db != null)
            {
                using (Utils.DataUpdater upd = new Utils.DataUpdater(db))
                {
                    await Task.Run(() =>
                    {
                        Utils.ProgressBlock prog = null;
                        int max = Core.ApplicationData.Instance.MainWindow.GeocacheSelectionCount;
                        DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                        if (_showProgress)
                        {
                            prog = new Utils.ProgressBlock("PerfomingAction", "PerfomingAction", max, 0, true);
                        }
                        try
                        {
                            int index = 0;
                            foreach (Core.Data.Geocache gc in db.GeocacheCollection)
                            {
                                if (gc.Selected)
                                {
                                    _geocacheAction(gc);

                                    if (prog != null)
                                    {
                                        index++;
                                        if (DateTime.Now >= nextUpdate)
                                        {
                                            if (!prog.Update("PerfomingAction", max, index))
                                            {
                                                break;
                                            }
                                            nextUpdate = DateTime.Now.AddSeconds(1);
                                        }
                                    }
                                }
                            }
                            if (prog != null)
                            {
                                prog.Dispose();
                                prog = null;
                            }
                        }
                        catch(Exception e)
                        {
                            Core.ApplicationData.Instance.Logger.AddLog(this, e);
                            if (prog!=null)
                            {
                                prog.Dispose();
                                prog = null;
                            }
                        }
                    });
                }
            };
            Core.ApplicationData.Instance.EndActiviy();
        }

    }
}
