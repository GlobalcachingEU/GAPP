using GAPPSF.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace GAPPSF.FindsOfUser
{
    /// <summary>
    /// Interaction logic for ImportWindow.xaml
    /// </summary>
    public partial class ImportWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ImportWindow()
        {
            InitializeComponent();

            long[] lt = new long[] { 1, 2, 3, 4, 5, 6, 7, 9, 10, 11, 12, 22, 23, 24, 45, 46, 47 };
            var at = (from a in logtypes.AvailableTypes where !lt.Contains(a.Item.ID) select a).ToList();
            foreach(var b in at)
            {
                logtypes.AvailableTypes.Remove(b);
            }
            if (!string.IsNullOrEmpty(Core.Settings.Default.FindLogsOfUserLogTypes))
            {
                string[] parts = Core.Settings.Default.FindLogsOfUserLogTypes.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in parts)
                {
                    int id = int.Parse(s);
                    (from a in logtypes.AvailableTypes where a.Item.ID == id select a).FirstOrDefault().IsChecked = true;
                }
            }

            UserNames = new ObservableCollection<string>();
            if (!string.IsNullOrEmpty(Core.Settings.Default.FindLogsOfUserUsers))
            {
                string[] parts = Core.Settings.Default.FindLogsOfUserUsers.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach(string s in parts)
                {
                    UserNames.Add(s);
                }
            }

            UserNames.CollectionChanged += UserNames_CollectionChanged;
            foreach (var a in logtypes.AvailableTypes)
            {
                a.PropertyChanged += a_PropertyChanged;
            }

            DataContext = this;
        }

        void UserNames_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            saveUsernames();
        }
        void a_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            saveSelectedLogTypes();
        }

        public ObservableCollection<string> UserNames { get; private set; }
        private string _selectedUser;
        public string SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                SetProperty(ref _selectedUser, value);
                IsUserSelected = !string.IsNullOrEmpty(value);
            }
        }
        private bool _isUserSelected = false;
        public bool IsUserSelected
        {
            get { return _isUserSelected; }
            set { SetProperty(ref _isUserSelected, value); }
        }

        private void saveSelectedLogTypes()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var a in logtypes.AvailableTypes)
            {
                if (a.IsChecked)
                {
                    sb.AppendLine(a.Item.ID.ToString());
                }
            }
            Core.Settings.Default.FindLogsOfUserLogTypes = sb.ToString();
        }
        private void saveUsernames()
        {
            StringBuilder sb = new StringBuilder();
            foreach(string s in UserNames)
            {
                sb.AppendLine(s);
            }
            Core.Settings.Default.FindLogsOfUserUsers = sb.ToString();
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string s = newUser.Text.Trim();
            if (s.Length>0)
            {
                if ((from a in UserNames where a.ToLower()==s.ToLower() select a).Count()==0)
                {
                    UserNames.Add(s);
                    newUser.Text = "";
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SelectedUser))
            {
                UserNames.Remove(SelectedUser);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            foreach (var a in logtypes.AvailableTypes)
            {
                a.IsChecked = true;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            foreach (var a in logtypes.AvailableTypes)
            {
                a.IsChecked = false;
            }
        }

        private AsyncDelegateCommand _removeLogsOfSelectedUserCommand;
        public AsyncDelegateCommand RemoveLogsOfSelectedUserCommand
        {
            get
            {
                if (_removeLogsOfSelectedUserCommand == null)
                {
                    _removeLogsOfSelectedUserCommand = new AsyncDelegateCommand(param => RemoveLogsOfSelectedUser(),
                        param => !string.IsNullOrEmpty(SelectedUser));
                }
                return _removeLogsOfSelectedUserCommand;
            }
        }
        public async Task RemoveLogsOfSelectedUser()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null && !string.IsNullOrEmpty(SelectedUser))
            {
                using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
                {
                    Import imp = new Import();
                    List<Core.Data.Log> lgs = await imp.GetLogsOfUser(Core.ApplicationData.Instance.ActiveDatabase, SelectedUser);
                    foreach(var l in lgs)
                    {
                        Utils.DataAccess.DeleteLog(Core.ApplicationData.Instance.ActiveDatabase, l);
                    }
                }
            }
        }


        private AsyncDelegateCommand _importLogsOfSelectedUserCommand;
        public AsyncDelegateCommand ImportLogsOfSelectedUserCommand
        {
            get
            {
                if (_importLogsOfSelectedUserCommand==null)
                {
                    _importLogsOfSelectedUserCommand = new AsyncDelegateCommand(param => ImportLogsOfSelectedUser(),
                        param => !string.IsNullOrEmpty(SelectedUser) && (from a in logtypes.AvailableTypes where a.IsChecked select a).Count() > 0);
                }
                return _importLogsOfSelectedUserCommand;
            }
        }
        public async Task ImportLogsOfSelectedUser()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null 
                && !string.IsNullOrEmpty(SelectedUser)
                && (from a in logtypes.AvailableTypes where a.IsChecked select a).Count() > 0)
            {
                Import imp = new Import();
                await imp.ImportLogsOfUsers(
                    Core.ApplicationData.Instance.ActiveDatabase,
                    new string[] { SelectedUser }.ToList(),
                    Core.Settings.Default.FindLogsOfUserBetweenDates,
                    Core.Settings.Default.FindLogsOfUserMinDate,
                    Core.Settings.Default.FindLogsOfUserMaxDate,
                    (from a in logtypes.AvailableTypes where a.IsChecked select a.Item).ToList(),
                    Core.Settings.Default.FindLogsOfUserImportMissing
                    );
            }
        }

        private AsyncDelegateCommand _importLogsCommand;
        public AsyncDelegateCommand ImportLogsCommand
        {
            get
            {
                if (_importLogsCommand == null)
                {
                    _importLogsCommand = new AsyncDelegateCommand(param => ImportLogs(),
                        param => UserNames.Count > 0 && (from a in logtypes.AvailableTypes where a.IsChecked select a).Count()>0);
                }
                return _importLogsCommand;
            }
        }
        public async Task ImportLogs()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null && (from a in logtypes.AvailableTypes where a.IsChecked select a).Count() > 0)
            {
                Import imp = new Import();
                await imp.ImportLogsOfUsers(
                    Core.ApplicationData.Instance.ActiveDatabase,
                    UserNames.ToList(),
                    Core.Settings.Default.FindLogsOfUserBetweenDates,
                    Core.Settings.Default.FindLogsOfUserMinDate,
                    Core.Settings.Default.FindLogsOfUserMaxDate,
                    (from a in logtypes.AvailableTypes where a.IsChecked select a.Item).ToList(),
                    Core.Settings.Default.FindLogsOfUserImportMissing
                    );
            }
        }

    }
}
