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

namespace GAPPSF.Dialogs
{
    /// <summary>
    /// Interaction logic for SettingsFolderWindow.xaml
    /// </summary>
    public partial class SettingsFolderWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string SelectedSettingsPath { get; private set; }
        public ObservableCollection<string> AvailableFolders { get; private set; }

        private string _selectedFolder;
        public string SelectedFolder
        {
            get { return _selectedFolder; }
            set
            {
                SetProperty(ref _selectedFolder, value);
                _isFolderSelected = !string.IsNullOrEmpty(_selectedFolder);
            }
        }

        private bool _isFolderSelected = false;
        public bool IsFolderSelected
        {
            get { return _isFolderSelected; }
            set { SetProperty(ref _isFolderSelected, value); }
        }

        public SettingsFolderWindow()
        {
            AvailableFolders = new ObservableCollection<string>();
            if (GAPPSF.Properties.Settings.Default.AvailableSettingsFolders!=null)
            {
                foreach(string s in GAPPSF.Properties.Settings.Default.AvailableSettingsFolders)
                {
                    AvailableFolders.Add(s);
                }
            }
            if (!string.IsNullOrEmpty(GAPPSF.Properties.Settings.Default.SettingsFolder))
            {
                if (!AvailableFolders.Contains(GAPPSF.Properties.Settings.Default.SettingsFolder))
                {
                    AvailableFolders.Add(GAPPSF.Properties.Settings.Default.SettingsFolder);
                }
                SelectedFolder = GAPPSF.Properties.Settings.Default.SettingsFolder;
            }

            InitializeComponent();
            DataContext = this;
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
            if (!string.IsNullOrEmpty(SelectedFolder))
            {
                AvailableFolders.Remove(SelectedFolder);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SelectedFolder))
            {
                try
                {
                    if (System.IO.Directory.Exists(SelectedFolder))
                    {
                        GAPPSF.Properties.Settings.Default.AvailableSettingsFolders = new System.Collections.Specialized.StringCollection();
                        foreach (string s in AvailableFolders)
                        {
                            GAPPSF.Properties.Settings.Default.AvailableSettingsFolders.Add(s);
                        }
                        GAPPSF.Properties.Settings.Default.Save();
                        SelectedSettingsPath = SelectedFolder;
                        DialogResult = true;
                        Close();
                    }
                }
                catch
                {

                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            FolderPickerDialog dlg = new FolderPickerDialog();
            if (dlg.ShowDialog()==true)
            {
                if (!AvailableFolders.Contains(dlg.SelectedPath))
                {
                    AvailableFolders.Add(dlg.SelectedPath);
                    SelectedFolder = dlg.SelectedPath;
                }
            }
        }

    }
}
