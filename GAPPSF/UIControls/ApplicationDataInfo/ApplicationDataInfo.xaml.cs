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
