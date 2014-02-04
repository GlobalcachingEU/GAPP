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

namespace GAPPSF.Munzee
{
    /// <summary>
    /// Interaction logic for DfxAtImportWindow.xaml
    /// </summary>
    public partial class DfxAtImportWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _munzeeAccountName;
        public string MunzeeAccountName
        {
            get { return _munzeeAccountName; }
            set
            {
                _munzeeAccountName = value;
                Core.ApplicationData.Instance.AccountInfos.GetAccountInfo("MZ").AccountName = value;
            }
        }

        public ObservableCollection<string> AvailableUrls { get; set; }
        private string _selectedUrl;
        public string SelectedUrl
        {
            get { return _selectedUrl; }
            set
            {
                SetProperty(ref _selectedUrl, value);
                IsUrlSelected = _selectedUrl != null;
            }
        }

        private bool _isUrlSelected = false;
        public bool IsUrlSelected
        {
            get { return _isUrlSelected; }
            set { SetProperty(ref _isUrlSelected, value); }
        }


        private string _newUrl;
        public string NewUrl
        {
            get { return _newUrl; }
            set
            {
                SetProperty(ref _newUrl, value);
                NewUrlValid = _newUrl != null && (_newUrl.ToLower().StartsWith("http://munzee.dfx.at/") || _newUrl.ToLower().StartsWith("http://dfx.at/"));
            }
        }

        private string _newComment;
        public string NewComment
        {
            get { return _newComment; }
            set
            {
                SetProperty(ref _newComment, value);
            }
        }


        private bool _newUrlValid = false;
        public bool NewUrlValid
        {
            get { return _newUrlValid; }
            set { SetProperty(ref _newUrlValid, value); }
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

        public DfxAtImportWindow()
        {
            _munzeeAccountName = Core.ApplicationData.Instance.AccountInfos.GetAccountInfo("MZ").AccountName ?? "";
            AvailableUrls = new ObservableCollection<string>();
            if (!string.IsNullOrEmpty(Core.Settings.Default.MunzeeDFXAtUrls))
            {
                string[] lines = Core.Settings.Default.MunzeeDFXAtUrls.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach(string s in lines)
                {
                    AvailableUrls.Add(s);
                }
            }

            InitializeComponent();

            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SelectedUrl))
            {
                AvailableUrls.Remove(SelectedUrl);
                StringBuilder sb = new StringBuilder();
                foreach(string s in AvailableUrls)
                {
                    sb.AppendLine(s);
                }
                Core.Settings.Default.MunzeeDFXAtUrls = sb.ToString();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://munzee.dfx.at");
            }
            catch (Exception ex)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, ex);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(NewUrl))
            {
                string newItem = string.Format("{0} ({1})", NewUrl, NewComment ?? "");
                AvailableUrls.Add(newItem);
                StringBuilder sb = new StringBuilder();
                foreach (string s in AvailableUrls)
                {
                    sb.AppendLine(s);
                }
                Core.Settings.Default.MunzeeDFXAtUrls = sb.ToString();

                SelectedUrl = newItem;
                NewComment = "";
                NewUrl = "";
            }
        }

        private AsyncDelegateCommand _importCommand;
        public AsyncDelegateCommand ImportCommand
        {
            get
            {
                if (_importCommand==null)
                {
                    _importCommand = new AsyncDelegateCommand(param => ImportSelectedUrl(),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null && IsUrlSelected);
                }
                return _importCommand;
            }
        }
        public async Task ImportSelectedUrl()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                string url = SelectedUrl.Split(' ')[0];
                Import imp = new Import();
                await imp.ImportMunzeesFromDfxAtAsync(Core.ApplicationData.Instance.ActiveDatabase, url);
            }
        }
    }
}
