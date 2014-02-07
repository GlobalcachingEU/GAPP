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
using System.Windows.Shapes;

namespace GAPPSF.Dialogs
{
    /// <summary>
    /// Interaction logic for RestoreSettingsWindow.xaml
    /// </summary>
    public partial class RestoreSettingsWindow : Window
    {
        public List<string> AvailableBackups { get; private set; }

        private string _selectedBackup;
        public string SelectedBackup
        {
            get { return _selectedBackup; }
            set
            {
                _selectedBackup = value;
                okButton.IsEnabled = !string.IsNullOrEmpty(_selectedBackup);
            }
        }

        public RestoreSettingsWindow()
        {
            AvailableBackups = Core.Settings.Default.AvailableBackups;

            InitializeComponent();
            okButton.IsEnabled = false;

            DataContext = this;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_selectedBackup))
            {
                if (Core.Settings.Default.PrepareRestoreBackup(_selectedBackup))
                {
                    DialogResult = true;
                    Close();
                }
            }
        }
    }
}
