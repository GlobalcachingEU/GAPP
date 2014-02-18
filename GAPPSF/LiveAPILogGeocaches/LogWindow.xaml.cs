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
            set 
            {
                saveLogData();
                SetProperty(ref _selectedLog, value);
                restoreLogData();
                if (_selectedLog!=null && Core.ApplicationData.Instance.ActiveDatabase!=null)
                {
                    var gc = Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection.GetGeocache(_selectedLog.GeocacheCode);
                    if (gc!=null)
                    {
                        Core.ApplicationData.Instance.ActiveGeocache = gc;
                    }
                }
                IsLogSelected = _selectedLog != null;
            }
        }

        private bool _isLogSelected;
        public bool IsLogSelected
        {
            get { return _isLogSelected; }
            set { SetProperty(ref _isLogSelected, value); }
        }


        private LogInfo.ImageInfo _selectedLogImage;
        public LogInfo.ImageInfo SelectedLogImage
        {
            get { return _selectedLogImage; }
            set
            {
                SetProperty(ref _selectedLogImage, value);
                IsLogImageSelected = _selectedLogImage != null;
            }
        }


        private bool _isLogImageSelected;
        public bool IsLogImageSelected
        {
            get { return _isLogImageSelected; }
            set { SetProperty(ref _isLogImageSelected, value); }
        }

        private bool _isMultipleLogSelected;
        public bool IsMultipleLogSelected
        {
            get { return _isMultipleLogSelected; }
            set { SetProperty(ref _isMultipleLogSelected, value); }
        }

        private bool _isMoveUpEnabled;
        public bool IsMoveUpEnabled
        {
            get { return _isMoveUpEnabled; }
            set { SetProperty(ref _isMoveUpEnabled, value); }
        }

        private bool _isMoveDownEnabled;
        public bool IsMoveDownEnabled
        {
            get { return _isMoveDownEnabled; }
            set { SetProperty(ref _isMoveDownEnabled, value); }
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
            int[] ids = new int[] { 2, 3, 4, 7, 45, 9, 10 };
            int i = 0;
            while(i < clogtype.AvailableTypes.Count)
            {
                if (!ids.Contains(clogtype.AvailableTypes[i].ID) )
                {
                    clogtype.AvailableTypes.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            DataContext = this;
            AvailableLogs.CollectionChanged += AvailableLogs_CollectionChanged;
        }

        void AvailableLogs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            saveData();
        }

        public LogWindow(List<Core.Data.Geocache> gcList)
            : this()
        {
            if (gcList!=null)
            {
                foreach(var gc in gcList)
                {
                    LogInfo li = (from a in AvailableLogs where a.GeocacheCode == gc.Code select a).FirstOrDefault();
                    if (li==null)
                    {
                        li = new LogInfo();
                        li.GeocacheCode = gc.Code;
                        li.LogText = "";
                        li.LogType = Utils.DataAccess.GetLogType(2);
                        li.VisitDate = DateTime.Now.Date;
                        li.AddToFavorites = false;
                        li.TrackableDrop = false;
                        li.TrackableRetrieve = "";
                        AvailableLogs.Add(li);
                    }
                }
            }
        }

        public LogWindow(List<GeocacheVisitsItem> gcList)
            : this()
        {
            if (gcList != null)
            {
                foreach (var gc in gcList)
                {
                    LogInfo li = (from a in AvailableLogs where a.GeocacheCode == gc.Code select a).FirstOrDefault();
                    if (li == null)
                    {
                        li = new LogInfo();
                        li.GeocacheCode = gc.Code;
                        li.LogText = gc.Comment;
                        li.LogType = Utils.DataAccess.GetLogType(2);
                        li.VisitDate = gc.LogDate;
                        li.AddToFavorites = false;
                        li.TrackableDrop = false;
                        li.TrackableRetrieve = "";
                        AvailableLogs.Add(li);
                    }
                }
            }
        }

        private void restoreLogData()
        {
            if (_selectedLog != null)
            {
                clogtype.SelectedComboItem = _selectedLog.LogType;
                clogtext.Text = _selectedLog.LogText;
                clogdate.SelectedDate = _selectedLog.VisitDate;
                clogdate.DisplayDate = _selectedLog.VisitDate;
                ctbdrop.IsChecked = _selectedLog.TrackableDrop;
                ctbretrieve.Text = _selectedLog.TrackableRetrieve;
                caddfav.IsChecked = _selectedLog.AddToFavorites;
            }
        }

        private void saveLogData()
        {
            if (_selectedLog!=null)
            {
                _selectedLog.LogType = clogtype.SelectedComboItem;
                _selectedLog.LogText = clogtext.Text;
                _selectedLog.VisitDate = (DateTime)clogdate.SelectedDate;
                _selectedLog.TrackableDrop = ctbdrop.IsChecked==true;
                _selectedLog.TrackableRetrieve = ctbretrieve.Text;
                _selectedLog.AddToFavorites = caddfav.IsChecked == true;
            }
        }

        private void saveData()
        {
            saveLogData();
            StringBuilder sb = new StringBuilder();
            foreach (var s in AvailableLogs)
            {
                sb.AppendLine(s.ToDataString());
            }
            Core.Settings.Default.LiveAPILogGeocachesLogs = sb.ToString();
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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            saveData();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<LogInfo> lis = (from LogInfo a in cloginfos.SelectedItems select a).ToList();
            foreach(var l in lis)
            {
                AvailableLogs.Remove(l);
            }
        }

        private void cloginfos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IsMultipleLogSelected = cloginfos.SelectedItems.Count > 1;
            IsMoveUpEnabled = cloginfos.SelectedItems.Count == 1 && cloginfos.SelectedIndex > 0;
            IsMoveDownEnabled = cloginfos.SelectedItems.Count == 1 && cloginfos.SelectedIndex < cloginfos.Items.Count - 1;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (SelectedLog != null)
            {
                //copy active to all other selected (batch logging)
                List<LogInfo> lis = (from LogInfo a in cloginfos.SelectedItems select a).ToList();
                foreach (var l in lis)
                {
                    l.LogType = clogtype.SelectedComboItem;
                    l.LogText = clogtext.Text;
                    l.VisitDate = (DateTime)clogdate.SelectedDate;
                    l.TrackableDrop = ctbdrop.IsChecked == true;
                    //l.TrackableRetrieve = ctbretrieve.Text;
                    l.AddToFavorites = caddfav.IsChecked == true;
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (cloginfos.SelectedIndex>=0)
            {
                int index = cloginfos.SelectedIndex;
                LogInfo l = AvailableLogs[index];
                AvailableLogs.RemoveAt(index);
                AvailableLogs.Insert(index - 1, l);
                SelectedLog = l;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (cloginfos.SelectedIndex < cloginfos.Items.Count - 1)
            {
                int index = cloginfos.SelectedIndex;
                LogInfo l = AvailableLogs[index];
                AvailableLogs.RemoveAt(index);
                AvailableLogs.Insert(index + 1, l);
                SelectedLog = l;
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (SelectedLog!=null)
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.FileName = ""; // Default file name
                dlg.Filter = "*.*|*.*"; // Filter files by extension 

                // Show open file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process open file dialog box results 
                if (result == true)
                {
                    LogInfo.ImageInfo ii = new LogInfo.ImageInfo();
                    ii.Caption = System.IO.Path.GetFileName(dlg.FileName);
                    ii.Description = System.IO.Path.GetFileName(dlg.FileName);
                    ii.Uri = dlg.FileName;
                    ii.RotationDeg = 0;

                    SelectedLog.Images.Add(ii);
                }

            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (SelectedLog != null && SelectedLogImage!=null)
            {
                SelectedLog.Images.Remove(SelectedLogImage);
            }
        }

    }
}
