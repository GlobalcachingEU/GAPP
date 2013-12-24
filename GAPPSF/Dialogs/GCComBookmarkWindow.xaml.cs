using GAPPSF.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GAPPSF.Dialogs
{
    /// <summary>
    /// Interaction logic for GCComBookmarkWindow.xaml
    /// </summary>
    public partial class GCComBookmarkWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _bmUrl;
        public string BMUrl 
        {
            get { return _bmUrl; }
            set { SetProperty(ref _bmUrl, value); } 
        }

        private string _bmName;
        public string BMName
        {
            get { return _bmName; }
            set { SetProperty(ref _bmName, value); }
        }

        private GCComBookmarks.Bookmark _activeBookmark;
        public GCComBookmarks.Bookmark ActiveBookmark
        {
            get { return _activeBookmark; }
            set { SetProperty(ref _activeBookmark, value); }
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

        public GCComBookmarkWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        private Guid getGuid()
        {
            if (!string.IsNullOrEmpty(BMUrl))
            {
                int pos = BMUrl.IndexOf('=');
                if (pos > 0 && pos < BMUrl.Length - 1)
                {
                    Guid guid = new Guid();
                    string s = BMUrl.Substring(pos + 1);
                    if (Guid.TryParse(s, out guid))
                    {
                        return guid;
                    }
                }
            }
            return Guid.Empty;
        }

        private bool newBookmarkValid
        {
            get
            {
                bool result = false;
                if (Core.ApplicationData.Instance.ActiveDatabase != null)
                {
                    if (!string.IsNullOrEmpty(BMName))
                    {
                        if (getGuid() != Guid.Empty)
                        {
                            result = true;
                        }
                    }
                }
                return result;
            }
        }

        private RelayCommand _executeRemoveBookmarkCommand;
        public RelayCommand ExecuteRemoveBookmarkCommand
        {
            get
            {
                if (_executeRemoveBookmarkCommand == null)
                {
                    _executeRemoveBookmarkCommand = new RelayCommand(param => RemoveBookmark(),
                        param => ActiveBookmark != null);
                }
                return _executeRemoveBookmarkCommand;
            }
        }
        public void RemoveBookmark()
        {
            if (ActiveBookmark != null)
            {
                Core.Settings.Default.DeleteGCComBookmark(ActiveBookmark);
                GCComBookmarks.Manager.Instance.RemoveBookmarkFromMenu(ActiveBookmark);
                GCComBookmarks.Manager.Instance.Bookmarks.Remove(ActiveBookmark);

                ActiveBookmark = null;
            }
        }

        private AsyncDelegateCommand _executeImportBookmarkCommand;
        public AsyncDelegateCommand ExecuteImportBookmarkCommand
        {
            get
            {
                if (_executeImportBookmarkCommand == null)
                {
                    _executeImportBookmarkCommand = new AsyncDelegateCommand(param => ImportBookmark(false),
                        param => ActiveBookmark!=null);
                }
                return _executeImportBookmarkCommand;
            }
        }
        private AsyncDelegateCommand _executeImportMissingBookmarkCommand;
        public AsyncDelegateCommand ExecuteImportMissingBookmarkCommand
        {
            get
            {
                if (_executeImportMissingBookmarkCommand == null)
                {
                    _executeImportMissingBookmarkCommand = new AsyncDelegateCommand(param => ImportBookmark(true),
                        param => ActiveBookmark != null);
                }
                return _executeImportMissingBookmarkCommand;
            }
        }


        private AsyncDelegateCommand _executeImportNewBookmarkCommand;
        public AsyncDelegateCommand ExecuteImportNewBookmarkCommand
        {
            get
            {
                if (_executeImportNewBookmarkCommand == null)
                {
                    _executeImportNewBookmarkCommand = new AsyncDelegateCommand(param => ImportNewBookmark(false),
                        param => newBookmarkValid);
                }
                return _executeImportNewBookmarkCommand;
            }
        }
        private AsyncDelegateCommand _executeImportMissingNewBookmarkCommand;
        public AsyncDelegateCommand ExecuteImportMissingNewBookmarkCommand
        {
            get
            {
                if (_executeImportMissingNewBookmarkCommand == null)
                {
                    _executeImportMissingNewBookmarkCommand = new AsyncDelegateCommand(param => ImportNewBookmark(true),
                        param => newBookmarkValid);
                }
                return _executeImportMissingNewBookmarkCommand;
            }
        }

        public async Task ImportNewBookmark(bool missingOnly)
        {
            if (newBookmarkValid)
            {
                using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            GCComBookmarks.Bookmark bm = new GCComBookmarks.Bookmark();
                            bm.Guid = getGuid().ToString();
                            Core.Settings.Default.GCComBookmarksID++;
                            bm.ID = string.Format("gccombm{0}", Core.Settings.Default.GCComBookmarksID);
                            bm.Name = BMName;
                            Core.Settings.Default.AddGCComBookmark(bm);
                            GCComBookmarks.Manager.Instance.Bookmarks.Add(bm);
                            GCComBookmarks.Manager.Instance.AddBookmarkToMenu(bm);
                            List<string> gcCodes = GCComBookmarks.Manager.Instance.UpdateBookmarkList(bm);

                            if (missingOnly)
                            {
                                List<string> present = (from a in gcCodes
                                                        join g in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection on a equals g.Code
                                                        select a).ToList();
                                foreach (string s in present)
                                {
                                    gcCodes.Remove(s);
                                }
                            }

                            LiveAPI.Import.ImportGeocaches(Core.ApplicationData.Instance.ActiveDatabase, gcCodes);

                            BMName = "";
                            BMUrl = "";
                            ActiveBookmark = bm;
                        }
                        catch(Exception e)
                        {
                            Core.ApplicationData.Instance.Logger.AddLog(this, e);
                        }
                    });
                }
            }
        }

        public async Task ImportBookmark(bool missingOnly)
        {
            if (ActiveBookmark != null)
            {
                using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            List<string> gcCodes = GCComBookmarks.Manager.Instance.UpdateBookmarkList(ActiveBookmark);

                            if (missingOnly)
                            {
                                List<string> present = (from a in gcCodes
                                                        join g in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection on a equals g.Code
                                                        select a).ToList();
                                foreach (string s in present)
                                {
                                    gcCodes.Remove(s);
                                }
                            }

                            LiveAPI.Import.ImportGeocaches(Core.ApplicationData.Instance.ActiveDatabase, gcCodes);
                        }
                        catch (Exception e)
                        {
                            Core.ApplicationData.Instance.Logger.AddLog(this, e);
                        }
                    });
                }
            }
        }

    }
}
