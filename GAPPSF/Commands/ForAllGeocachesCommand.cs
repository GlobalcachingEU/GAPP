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

        public ForAllGeocachesCommand(Action<Core.Data.Geocache> geocacheAction)
            : base(null, null)
        {
            _geocacheAction = geocacheAction;
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            Core.ApplicationData.Instance.BeginActiviy();
            Core.Storage.Database db = Core.ApplicationData.Instance.ActiveDatabase;
            if (db != null)
            {
                using (Utils.DataUpdater upd = new Utils.DataUpdater(db))
                {
                    await Task.Run(() =>
                    {
                        foreach (Core.Data.Geocache gc in db.GeocacheCollection)
                        {
                            _geocacheAction(gc);
                        }
                    });
                }
            };
            Core.ApplicationData.Instance.EndActiviy();
        }

    }
}
