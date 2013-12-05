using GAPPSF.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GAPPSF.Clipboard
{
    public class ClipboardHandler: INotifyPropertyChanged
    {
        private static ClipboardHandler _uniqueInstance = null;
        private static object _lockObject = new object();

        public event PropertyChangedEventHandler PropertyChanged;

        /*
         * for now, just simple. Can be made more sofisticated later
         * also, now source database should be loaded already, future todo
         * //now: all in memory (not persistant)
         */
        private string _sourceDatabaseFilename = null;
        private List<string> _gcCodes = null;

        public ClipboardHandler()
        {
#if DEBUG
            if (_uniqueInstance != null)
            {
                //you used the wrong binding
                System.Diagnostics.Debugger.Break();
            }
#endif
            Core.ApplicationData.Instance.Databases.CollectionChanged += Databases_CollectionChanged;
        }

        void Databases_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //check if our source database still exists
            if (!string.IsNullOrEmpty(_sourceDatabaseFilename))
            {
                if ((from a in Core.ApplicationData.Instance.Databases where a.FileName == _sourceDatabaseFilename select a).FirstOrDefault() == null)
                {
                    _sourceDatabaseFilename = null;
                    _gcCodes = null;
                }
            }
        }


        public static ClipboardHandler Instance
        {
            get
            {
                if (_uniqueInstance==null)
                {
                    lock(_lockObject)
                    {
                        if (_uniqueInstance==null)
                        {
                            _uniqueInstance = new ClipboardHandler();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }

        AsyncDelegateCommand _copyCommand;
        public ICommand CopyCommand
        {
            get
            {
                if (_copyCommand == null)
                {
                    _copyCommand = new AsyncDelegateCommand(param => this.CopyActiveGeocache(), param => Core.ApplicationData.Instance.ActiveGeocache!=null);
                }
                return _copyCommand;
            }
        }

        async private Task CopyActiveGeocache()
        {
            if (Core.ApplicationData.Instance.ActiveGeocache!=null)
            {
                List<string> gcList = new List<string>();
                gcList.Add(Core.ApplicationData.Instance.ActiveGeocache.Code);
                await CopyGeocachesAsync(Core.ApplicationData.Instance.ActiveGeocache.Database, gcList);
            }
        }

        async private Task CopyGeocachesAsync(Core.Storage.Database db, List<string> gcList)
        {
            await Task.Run(() =>
                {
                    _sourceDatabaseFilename = db.FileName;
                    _gcCodes = new List<string>();
                    _gcCodes.AddRange(gcList);
                });
        }

        AsyncDelegateCommand _copySelectedCommand;
        public ICommand CopySelectedCommand
        {
            get
            {
                if (_copySelectedCommand == null)
                {
                    _copySelectedCommand = new AsyncDelegateCommand(param => this.CopySelectedGeocache(), param => Core.ApplicationData.Instance.MainWindow.GeocacheSelectionCount >0);
                }
                return _copySelectedCommand;
            }
        }

        async private Task CopySelectedGeocache()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                List<string> gcList = (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Selected select a.Code).ToList();
                await CopyGeocachesAsync(Core.ApplicationData.Instance.ActiveGeocache.Database, gcList);
            }
        }

        AsyncDelegateCommand _pasteCommand;
        public ICommand PasteCommand
        {
            get
            {
                if (_pasteCommand == null)
                {
                    _pasteCommand = new AsyncDelegateCommand(param => this.PasteGeocaches(), param =>
                        Core.ApplicationData.Instance.ActiveDatabase!=null &&
                        !string.IsNullOrEmpty(_sourceDatabaseFilename) &&
                        Core.ApplicationData.Instance.ActiveDatabase.FileName!=_sourceDatabaseFilename &&
                        _gcCodes != null 
                        && _gcCodes.Count>0
                        );
                }
                return _pasteCommand;
            }
        }

        async public Task PasteGeocaches()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null &&
                        !string.IsNullOrEmpty(_sourceDatabaseFilename) &&
                        Core.ApplicationData.Instance.ActiveDatabase.FileName != _sourceDatabaseFilename &&
                        _gcCodes != null
                        && _gcCodes.Count > 0)
            {
                Core.Storage.Database srcDb = (from a in Core.ApplicationData.Instance.Databases where a.FileName == _sourceDatabaseFilename select a).FirstOrDefault();
                if (srcDb != null)
                {
                    using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
                    {
                        await Task.Run(() =>
                        {
                            DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                            using (Utils.ProgressBlock prog = new Utils.ProgressBlock("Copying geocaches...", "Copying geocaches...", _gcCodes.Count, 0, true))
                            {
                                int index = 0;
                                Core.Data.GeocacheData gcData = new Core.Data.GeocacheData();
                                Core.Data.LogData lgData = new Core.Data.LogData();
                                Core.Data.WaypointData wpData = new Core.Data.WaypointData();
                                Core.Data.UserWaypointData uwpData = new Core.Data.UserWaypointData();
                                Core.Data.LogImageData lgiData = new Core.Data.LogImageData();
                                Core.Data.GeocacheImageData gciData = new Core.Data.GeocacheImageData();
                                foreach (string gcCode in _gcCodes)
                                {
                                    Core.Data.Geocache gc = srcDb.GeocacheCollection.GetGeocache(gcCode);
                                    if (gc!=null)
                                    {
                                        Core.Data.GeocacheData.Copy(gc, gcData);
                                        Utils.DataAccess.AddGeocache(Core.ApplicationData.Instance.ActiveDatabase, gcData);
                                        List<Core.Data.Log> lgs = Utils.DataAccess.GetLogs(srcDb, gc.Code);
                                        foreach(var l in lgs)
                                        {
                                            Core.Data.LogData.Copy(l, lgData);
                                            Utils.DataAccess.AddLog(Core.ApplicationData.Instance.ActiveDatabase, lgData);

                                            List<Core.Data.LogImage> lgis = Utils.DataAccess.GetLogImages(srcDb, l.ID);
                                            foreach(var li in lgis)
                                            {
                                                Core.Data.LogImageData.Copy(li, lgiData);
                                                Utils.DataAccess.AddLogImage(Core.ApplicationData.Instance.ActiveDatabase, lgiData);
                                            }
                                        }
                                        List<Core.Data.Waypoint> wps = Utils.DataAccess.GetWaypointsFromGeocache(srcDb, gc.Code);
                                        foreach(var wp in wps)
                                        {
                                            Core.Data.WaypointData.Copy(wp, wpData);
                                            Utils.DataAccess.AddWaypoint(Core.ApplicationData.Instance.ActiveDatabase, wpData);
                                        }
                                        List<Core.Data.UserWaypoint> uwps = Utils.DataAccess.GetUserWaypointsFromGeocache(srcDb, gc.Code);
                                        foreach (var wp in uwps)
                                        {
                                            Core.Data.UserWaypointData.Copy(wp, uwpData);
                                            Utils.DataAccess.AddUserWaypoint(Core.ApplicationData.Instance.ActiveDatabase, uwpData);
                                        }
                                        List<Core.Data.GeocacheImage> gcis = Utils.DataAccess.GetGeocacheImages(srcDb, gc.Code);
                                        foreach (var wp in gcis)
                                        {
                                            Core.Data.GeocacheImageData.Copy(wp, gciData);
                                            Utils.DataAccess.AddGeocacheImage(Core.ApplicationData.Instance.ActiveDatabase, gciData);
                                        }

                                        index++;
                                        if (DateTime.Now>=nextUpdate)
                                        {
                                            if (!prog.Update("Copying geocaches...", _gcCodes.Count, index))
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        });
                    }
                }
            }
        }

    }
}
