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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GAPPSF.UIControls.LogImageViewer
{
    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl, IDisposable, IUIControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<Core.Data.LogImage> AvailableLogImages { get; set; }

        private Core.Data.LogImage _selectedLogImage;
        public Core.Data.LogImage SelectedLogImage
        {
            get { return _selectedLogImage; }
            set
            {
                SetProperty(ref _selectedLogImage, value);
                if (_selectedLogImage==null)
                {
                    LogImageThumbSource = null;
                    LogImageSource = null;
                }
                else
                {
                    try
                    {
                        if (_selectedLogImage.Url.IndexOf("/cache/log/") > 0)
                        {
                            LogImageThumbSource = new BitmapImage(new Uri(_selectedLogImage.Url.Replace("/cache/log", "/cache/log/thumb")));
                        }
                        else
                        {
                            LogImageThumbSource = null;
                        }
                        LogImageSource = new BitmapImage(new Uri(_selectedLogImage.Url));
                    }
                    catch
                    {
                        LogImageThumbSource = null;
                        LogImageSource = null;
                    }
                }
            }
        }

        private BitmapSource _logImageSource;
        public BitmapSource LogImageSource
        {
            get { return _logImageSource; }
            set 
            {
                SetProperty(ref _logImageSource, value);
            }
        }

        private BitmapSource _logImageThumbSource;
        public BitmapSource LogImageThumbSource
        {
            get { return _logImageThumbSource; }
            set
            {
                SetProperty(ref _logImageThumbSource, value);
            }
        }

        public Control()
        {
            AvailableLogImages = new ObservableCollection<Core.Data.LogImage>();
            InitializeComponent();

            DataContext = this;
            updateView();

            Core.ApplicationData.Instance.PropertyChanged += Instance_PropertyChanged;
        }

        void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveGeocache")
            {
                updateView();
            }
        }

        public void Dispose()
        {
            Core.ApplicationData.Instance.PropertyChanged -= Instance_PropertyChanged;
        }

        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate("LogImageViewer") as string;
        }

        private void updateView()
        {
            AvailableLogImages.Clear();
            if (Core.ApplicationData.Instance.ActiveGeocache!=null)
            {
                List<Core.Data.LogImage> lil = Utils.DataAccess.GetLogImages(Core.ApplicationData.Instance.ActiveGeocache);
                foreach(var li in lil)
                {
                    AvailableLogImages.Add(li);
                }
            }
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

        public int WindowWidth
        {
            get
            {
                return Core.Settings.Default.LogImageViewerWindowWidth;
            }
            set
            {
                Core.Settings.Default.LogImageViewerWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.LogImageViewerWindowHeight;
            }
            set
            {
                Core.Settings.Default.LogImageViewerWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.LogImageViewerWindowLeft;
            }
            set
            {
                Core.Settings.Default.LogImageViewerWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.LogImageViewerWindowTop;
            }
            set
            {
                Core.Settings.Default.LogImageViewerWindowTop = value;
            }
        }

    }
}
