using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Utils.BasePlugin
{
    public class BaseImportFilter: Plugin
    {
        public override Framework.PluginType PluginType
        {
            get { return Framework.PluginType.ImportData; }
        }

        //On UI Context
        async public virtual Task PerformImport()
        {
            using (FrameworkDataUpdater d = new FrameworkDataUpdater(Core))
            {
                GeocacheIgnoreSupport.Instance.IgnoreCounter = 0;
                await Task.Run(() =>
                {
                    try
                    {
                        ImportMethod();
                    }
                    catch (Exception e)
                    {
                        System.Windows.Forms.MessageBox.Show(e.Message, Utils.LanguageSupport.Instance.GetTranslation("Error"), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                });
                if (GeocacheIgnoreSupport.Instance.IgnoreCounter > 0)
                {
                    using (Dialogs.GeocachesIgnoredMessageForm dlg = new Dialogs.GeocachesIgnoredMessageForm(Core, GeocacheIgnoreSupport.Instance.IgnoreCounter))
                    {
                        dlg.ShowDialog();
                    }
                }
            }
        }

        protected virtual void ImportMethod()
        {
        }

        protected virtual bool AddGeocache(Framework.Data.Geocache gc, Version gpxVersion)
        {
            bool result = false;

            Framework.Data.Geocache oldgc = Utils.DataAccess.GetGeocache(Core.Geocaches, gc.Code);
            if (oldgc == null)
            {
                if (!Utils.GeocacheIgnoreSupport.Instance.IgnoreGeocache(gc))
                {
                    Core.Geocaches.Add(gc);
                    result = true;
                }
            }
            else
            {
                result = true;
                if (gc.DataFromDate >= oldgc.DataFromDate)
                {
                    Utils.DataAccess.UpdateGeocacheData(oldgc, gc, gpxVersion);
                }
            }

            return result;
        }

        protected virtual bool AddWaypoint(Framework.Data.Waypoint wp)
        {
            bool result = false;

            Framework.Data.Waypoint oldwp = Utils.DataAccess.GetWaypoint(Core.Waypoints, wp.Code);
            if (oldwp == null)
            {
                Core.Waypoints.Add(wp);
                result = true;
            }
            else
            {
                if (wp.DataFromDate >= oldwp.DataFromDate)
                {
                    Utils.DataAccess.UpdateWaypointData(oldwp, wp);
                }
            }
            return result;
        }

        protected virtual bool AddUserWaypoint(Framework.Data.UserWaypoint wp)
        {
            bool result = false;

            Framework.Data.UserWaypoint oldwp = Utils.DataAccess.GetUserWaypoint(Core.UserWaypoints, wp.ID);
            if (oldwp == null)
            {
                Core.UserWaypoints.Add(wp);
                result = true;
            }
            else
            {
                Utils.DataAccess.UpdateUserWaypointData(oldwp, wp);
            }
            return result;
        }

        protected virtual bool AddLog(Framework.Data.Log l)
        {
            bool result = false;

            Framework.Data.Log oldwp = Utils.DataAccess.GetLog(Core.Logs, l.ID);
            if (oldwp == null)
            {
                Core.Logs.Add(l);
                result = true;
            }
            else
            {
                if (l.DataFromDate >= oldwp.DataFromDate)
                {
                    Utils.DataAccess.UpdateLogData(oldwp, l);
                }
            }
            if (l.LogType.AsFound && Core.GeocachingAccountNames.GetAccountName(l.GeocacheCode).ToLower() == l.Finder.ToLower())
            {
                //found
                Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(Core.Geocaches, l.GeocacheCode);
                if (gc != null)
                {
                    gc.Found = true;
                }
            }

            return result;
        }

        protected virtual bool AddLogImage(Framework.Data.LogImage l)
        {
            bool result = false;

            Framework.Data.LogImage oldwp = Utils.DataAccess.GetLogImage(Core.LogImages, l.ID);
            if (oldwp == null)
            {
                Core.LogImages.Add(l);
                result = true;
            }
            else
            {
                if (l.DataFromDate >= oldwp.DataFromDate)
                {
                    Utils.DataAccess.UpdateLogImageData(oldwp, l);
                }
            }

            return result;
        }

        protected virtual bool AddGeocacheImage(Framework.Data.GeocacheImage l)
        {
            bool result = false;

            Framework.Data.GeocacheImage oldwp = DataAccess.GetGeocacheImage(Core.GeocacheImages, l.ID);
            if (oldwp == null)
            {
                Core.GeocacheImages.Add(l);
                result = true;
            }
            else
            {
                if (l.DataFromDate >= oldwp.DataFromDate)
                {
                    DataAccess.UpdateGeocacheImageData(oldwp, l);
                }
            }

            return result;
        }

    }


}
