using GAPPSF.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace GAPPSF.PurgeLogs
{
    /// <summary>
    /// Interaction logic for PurgerWindow.xaml
    /// </summary>
    public partial class PurgerWindow : Window
    {
        public ObservableCollection<string> KeepLogsOfUsers { get; private set; }
        public ObservableCollection<string> RemoveAllLogsOfUsers { get; private set; }

        public PurgerWindow()
        {
            InitializeComponent();

            KeepLogsOfUsers = new ObservableCollection<string>();
            RemoveAllLogsOfUsers = new ObservableCollection<string>();

            if (!string.IsNullOrEmpty(Core.Settings.Default.PurgeLogsKeepLogsOfUsers))
            {
                string[] usrs = Core.Settings.Default.PurgeLogsKeepLogsOfUsers.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach(string s in usrs)
                {
                    KeepLogsOfUsers.Add(s);
                }
            }
            if (!string.IsNullOrEmpty(Core.Settings.Default.PurgeLogsRemoveAllLogsOfUsers))
            {
                string[] usrs = Core.Settings.Default.PurgeLogsRemoveAllLogsOfUsers.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in usrs)
                {
                    RemoveAllLogsOfUsers.Add(s);
                }
            }

            KeepLogsOfUsers.CollectionChanged += KeepLogsOfUsers_CollectionChanged;
            RemoveAllLogsOfUsers.CollectionChanged += RemoveAllLogsOfUsers_CollectionChanged;

            DataContext = this;
        }

        void RemoveAllLogsOfUsers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in RemoveAllLogsOfUsers)
            {
                sb.AppendLine(s);
            }
            Core.Settings.Default.PurgeLogsRemoveAllLogsOfUsers = sb.ToString();
        }

        void KeepLogsOfUsers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in KeepLogsOfUsers)
            {
                sb.AppendLine(s);
            }
            Core.Settings.Default.PurgeLogsKeepLogsOfUsers = sb.ToString();
        }

        private AsyncDelegateCommand _purgeLogsCommand;
        public AsyncDelegateCommand PurgeLogsCommand
        {
            get
            {
                if (_purgeLogsCommand==null)
                {
                    _purgeLogsCommand = new AsyncDelegateCommand(param => PerformPurgeLogs());
                }
                return _purgeLogsCommand;
            }
        }
        public async Task PerformPurgeLogs()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                Purger p = new Purger();
                await p.PurgeWithDefaultSettings(Core.ApplicationData.Instance.ActiveDatabase);
            }
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (keepList.SelectedItem!=null)
            {
                KeepLogsOfUsers.Remove(keepList.SelectedItem as string);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (removeList.SelectedItem != null)
            {
                RemoveAllLogsOfUsers.Remove(removeList.SelectedItem as string);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string s = (newKeepLogsOfUser.Text ?? "").Trim().ToLower();
            if (s.Length>0)
            {
                if (!KeepLogsOfUsers.Contains(s))
                {
                    KeepLogsOfUsers.Add(s);
                }
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            string s = (newRemoveLogsOfUser.Text ?? "").Trim().ToLower();
            if (s.Length > 0)
            {
                if (!RemoveAllLogsOfUsers.Contains(s))
                {
                    RemoveAllLogsOfUsers.Add(s);
                }
            }
        }

    }
}
