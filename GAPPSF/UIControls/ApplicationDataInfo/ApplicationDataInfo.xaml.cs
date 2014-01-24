using GAPPSF.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GAPPSF.UIControls
{
    /// <summary>
    /// Interaction logic for ApplicationDataInfo.xaml
    /// </summary>
    public partial class ApplicationDataInfo : UserControl, IUIControl
    {
        public ApplicationDataInfo()
        {
            DataContext = this;

            InitializeComponent();
        }

        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate("ApplicationInfo") as string;
        }

        private RelayCommand _removeDatabaseCommand = null;
        public RelayCommand RemoveDatabaseCommand
        {
            get
            {
                if (_removeDatabaseCommand==null)
                {
                    _removeDatabaseCommand = new RelayCommand(param => RemoveDatabase(param));
                }
                return _removeDatabaseCommand;
            }
        }
        public void RemoveDatabase(object database)
        {
            Core.Storage.Database db = database as Core.Storage.Database;
            if (db!=null)
            {
                if (Core.ApplicationData.Instance.ActiveDatabase==db)
                {
                    Core.ApplicationData.Instance.ActiveDatabase = null;
                }
                Core.ApplicationData.Instance.Databases.Remove(db);
            }
        }

        private AsyncDelegateCommand _backupDatabaseCommand = null;
        public AsyncDelegateCommand BackupDatabaseCommand
        {
            get
            {
                if (_backupDatabaseCommand == null)
                {
                    _backupDatabaseCommand = new AsyncDelegateCommand(param => BackupDatabase(param));
                }
                return _backupDatabaseCommand;
            }
        }
        public async Task BackupDatabase(object database)
        {
            Core.Storage.Database db = database as Core.Storage.Database;
            if (db != null)
            {
                await db.BackupAsync();
            }
        }

        private AsyncDelegateCommand _restoreDatabaseCommand = null;
        public AsyncDelegateCommand RestoreDatabaseCommand
        {
            get
            {
                if (_restoreDatabaseCommand == null)
                {
                    _restoreDatabaseCommand = new AsyncDelegateCommand(param => RestoreDatabase(param));
                }
                return _restoreDatabaseCommand;
            }
        }
        public async Task RestoreDatabase(object database)
        {
            Core.Storage.Database db = database as Core.Storage.Database;
            if (db != null)
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(db.FileName);
                dlg.FileName = ""; // Default file name
                dlg.Filter = string.Format("GAPP SF backup ({0}.gsf.bak)|{0}.bak*", System.IO.Path.GetFileName(db.FileName)); // Filter files by extension 

                // Show open file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process open file dialog box results 
                if (result == true)
                {
                    int pos = dlg.FileName.ToLower().LastIndexOf(".bak");
                    string orgFn = dlg.FileName.Substring(0, pos);

                    //if database is open at the moment, close it.
                    db = (from a in Core.ApplicationData.Instance.Databases where string.Compare(a.FileName, orgFn, true) == 0 select a).FirstOrDefault();
                    if (db != null)
                    {
                        if (Core.ApplicationData.Instance.ActiveDatabase == db)
                        {
                            Core.ApplicationData.Instance.ActiveDatabase = null;
                        }
                        Core.ApplicationData.Instance.Databases.Remove(db);
                    }

                    //now, delete index file
                    string indexFile = string.Concat(orgFn, ".gsx");
                    if (System.IO.File.Exists(indexFile))
                    {
                        System.IO.File.Delete(indexFile);
                    }

                    if (System.IO.File.Exists(orgFn))
                    {
                        System.IO.File.Delete(orgFn);
                    }
                    System.IO.File.Move(dlg.FileName, orgFn);

                    //load database
                    bool success;
                    db = new Core.Storage.Database(orgFn);
                    using (Utils.DataUpdater upd = new Utils.DataUpdater(db, true))
                    {
                        success = await db.InitializeAsync();
                    }
                    if (success)
                    {
                        Core.ApplicationData.Instance.Databases.Add(db);
                        Core.ApplicationData.Instance.ActiveDatabase = db;
                    }
                    else
                    {
                        db.Dispose();
                    }
                }
            }
        }


        public int WindowWidth
        {
            get
            {
                return Core.Settings.Default.AppDataInfoWindowWidth;
            }
            set
            {
                Core.Settings.Default.AppDataInfoWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.AppDataInfoWindowHeight;
            }
            set
            {
                Core.Settings.Default.AppDataInfoWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.AppDataInfoWindowLeft;
            }
            set
            {
                Core.Settings.Default.AppDataInfoWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.AppDataInfoWindowTop;
            }
            set
            {
                Core.Settings.Default.AppDataInfoWindowTop = value;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Dialogs.GetLocationWindow dlg = new Dialogs.GetLocationWindow(Core.ApplicationData.Instance.HomeLocation);
            if (dlg.ShowDialog()==true)
            {
                Core.ApplicationData.Instance.HomeLocation.Lat = dlg.Location.Lat;
                Core.ApplicationData.Instance.HomeLocation.Lon = dlg.Location.Lon;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Dialogs.GetLocationWindow dlg = new Dialogs.GetLocationWindow(Core.ApplicationData.Instance.CenterLocation);
            if (dlg.ShowDialog() == true)
            {
                Utils.DataAccess.SetCenterLocation(dlg.Location.Lat, dlg.Location.Lon);
            }
        }
    }
}
