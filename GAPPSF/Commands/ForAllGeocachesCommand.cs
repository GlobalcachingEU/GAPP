using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Commands
{
    public class ForAllGeocachesCommand : AsyncDelegateCommand
    {
        private Action<Core.Data.Geocache> _geocacheAction;

        private bool _showProgress;

        public ForAllGeocachesCommand(Action<Core.Data.Geocache> geocacheAction)
            : this(geocacheAction, false)
        {
        }
        public ForAllGeocachesCommand(Action<Core.Data.Geocache> geocacheAction, bool showProgress)
            : base(null, param => Core.ApplicationData.Instance.ActiveDatabase != null)
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
                        DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                        if (_showProgress)
                        {
                            prog = new Utils.ProgressBlock("PerfomingAction", "PerfomingAction", db.GeocacheCollection.Count, 0, true);
                        }
                        try
                        {
                            int index = 0;
                            foreach (Core.Data.Geocache gc in db.GeocacheCollection)
                            {
                                _geocacheAction(gc);

                                if (prog!=null)
                                {
                                    index++;
                                    if (DateTime.Now>=nextUpdate)
                                    {
                                        if (!prog.Update("PerfomingAction", db.GeocacheCollection.Count, index))
                                        {
                                            break;
                                        }
                                        nextUpdate = DateTime.Now.AddSeconds(1);
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
