using GAPPSF.ActionSequence;
using GAPPSF.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GAPPSF.GCComBookmarks
{
    public class Manager
    {
        private static Manager _uniqueInstance = null;
        private static object _lockObject = new object();

        public ObservableCollection<Bookmark> Bookmarks { get; private set; }

        public Manager()
        {
#if DEBUG
            if (_uniqueInstance != null)
            {
                System.Diagnostics.Debugger.Break();
            }
#endif
            Bookmarks = new ObservableCollection<Bookmark>();
            loadBookmarks();
            foreach (var f in Bookmarks)
            {
                AddBookmarkToMenu(f);
            }
        }

        private void loadBookmarks()
        {
            try
            {
                List<Bookmark> bmList = Core.Settings.Default.LoadGCComBookmarks();
                foreach(var bm in bmList)
                {
                    Bookmarks.Add(bm);
                }
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
        }

        public static Manager Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            _uniqueInstance = new Manager();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        public void AddBookmarkToMenu(Bookmark seq)
        {
            MenuItem mi = new MenuItem();
            mi.Header = seq.Name;
            mi.Name = seq.ID;
            mi.Command = ExecuteBookmarkCommand;
            mi.CommandParameter = seq;
            Core.ApplicationData.Instance.MainWindow.gccombmmnu.Items.Add(mi);
        }

        public void RemoveBookmarkFromMenu(Bookmark seq)
        {
            MenuItem mi;
            for (int i = 0; i < Core.ApplicationData.Instance.MainWindow.gccombmmnu.Items.Count; i++)
            {
                mi = Core.ApplicationData.Instance.MainWindow.gccombmmnu.Items[i] as MenuItem;
                if (mi != null && mi.Name == seq.ID)
                {
                    Core.ApplicationData.Instance.MainWindow.gccombmmnu.Items.RemoveAt(i);
                    break;
                }
            }
        }

        public void RenameBookmark(Bookmark af, string newName)
        {
            af.Name = newName;
            MenuItem mi;
            for (int i = 0; i < Core.ApplicationData.Instance.MainWindow.gccombmmnu.Items.Count; i++)
            {
                mi = Core.ApplicationData.Instance.MainWindow.gccombmmnu.Items[i] as MenuItem;
                if (mi != null && mi.Name == af.ID)
                {
                    mi.Header = newName;
                    break;
                }
            }
        }

        private AsyncDelegateCommand _executeBookmarkCommand;
        public AsyncDelegateCommand ExecuteBookmarkCommand
        {
            get
            {
                if (_executeBookmarkCommand == null)
                {
                    _executeBookmarkCommand = new AsyncDelegateCommand(param => RunActionSequence(param as Bookmark),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null);
                }
                return _executeBookmarkCommand;
            }
        }

        public async Task RunActionSequence(Bookmark af)
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
                {
                    await Task.Run(() =>
                    {
                        List<string> gcCodes = Core.Settings.Default.LoadGCComBookmarkGeocaches(af);
                        foreach(var gc in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection)
                        {
                            gc.Selected = gcCodes.Contains(gc.Code);
                        }
                    });
                }
            }
        }


        public List<string> UpdateBookmarkList(Bookmark bm)
        {
            List<string> result = null;

            try
            {
                using (LiveAPI.GeocachingLiveV6 api = new LiveAPI.GeocachingLiveV6())
                {
                    Guid guid = Guid.Parse(bm.Guid);

                    var req = new LiveAPI.LiveV6.GetBookmarkListByGuidRequest();
                    req.AccessToken = api.Token;
                    req.BookmarkListGuid = guid;
                    var resp = api.Client.GetBookmarkListByGuid(req);
                    if (resp.Status.StatusCode == 0)
                    {
                        result = (from c in resp.BookmarkList select c.CacheCode).ToList();

                        Core.Settings.Default.SaveGCComBookmarkGeocaches(bm, result);
                    }
                    else
                    {
                        Core.ApplicationData.Instance.Logger.AddLog(this, new Exception(resp.Status.StatusMessage));
                    }
                }
            }
            catch (Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }

            return result;
        }

    }
}
