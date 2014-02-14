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

namespace GAPPSF.LiveAPILogGeocaches
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<LogInfo> AvailableLogs { get; set; }

        private LogInfo _selectedLog;
        public LogInfo SelectedLog
        {
            get { return _selectedLog; }
            set { SetProperty(ref _selectedLog, value); }
        }

        public LogWindow()
        {
            AvailableLogs = new ObservableCollection<LogInfo>();
            if (!string.IsNullOrEmpty(Core.Settings.Default.LiveAPILogGeocachesLogs))
            {
                string[] lines = Core.Settings.Default.LiveAPILogGeocachesLogs.Split(new char[]{'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
                foreach(string s in lines)
                {
                    LogInfo li = LogInfo.FromDataString(s);
                    if (li!=null)
                    {
                        AvailableLogs.Add(li);
                    }
                }
            }

            InitializeComponent();

            DataContext = this;
            AvailableLogs.CollectionChanged += AvailableLogs_CollectionChanged;
        }

        void AvailableLogs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var s in AvailableLogs)
            {
                sb.AppendLine(s.ToDataString());
            }
            Core.Settings.Default.LiveAPILogGeocachesLogs = sb.ToString();
        }

        public LogWindow(List<Core.Data.Geocache> gcList)
            : this()
        {
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

    }
}
