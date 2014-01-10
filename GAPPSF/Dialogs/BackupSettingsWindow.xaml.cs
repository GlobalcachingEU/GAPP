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
    /// Interaction logic for BackupSettingsWindow.xaml
    /// </summary>
    public partial class BackupSettingsWindow : Window
    {
        private int _days;
        public int Days 
        { 
            get {return _days;}
            set { _days = value; updateInterval(); } 
        }

        private int _hours;
        public int Hours
        {
            get { return _hours; }
            set { _hours = value; updateInterval(); }
        }

        private int _minutes;
        public int Minutes
        {
            get { return _minutes; }
            set { _minutes = value; updateInterval(); }
        }

        public BackupSettingsWindow()
        {
            InitializeComponent();

            _days = Core.Settings.Default.DatabaseBackupAutomaticInterval.Days;
            _hours = Core.Settings.Default.DatabaseBackupAutomaticInterval.Hours;
            _minutes = Core.Settings.Default.DatabaseBackupAutomaticInterval.Minutes;

            DataContext = this;
        }

        private void updateInterval()
        {
            if (_days == 0 && _hours==0)
            {
                _minutes = Math.Max(1, _minutes);
            }
            Core.Settings.Default.DatabaseBackupAutomaticInterval = new TimeSpan(_days, _hours, _minutes, 0);
        }
    }
}
