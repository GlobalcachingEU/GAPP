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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GAPPSF.UIControls.OfflineImages
{
    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl, IDisposable, IUIControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<ImageInfo> ImageInfoCollection { get; private set; }

        public Control()
        {
            ImageInfoCollection = new ObservableCollection<ImageInfo>();

            DataContext = this;

            InitializeComponent();

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
            return Localization.TranslationManager.Instance.Translate("OfflineImages") as string;
        }

        private void updateView()
        {
            ImageInfoCollection.Clear();
            if (Core.ApplicationData.Instance.ActiveGeocache!=null)
            {
                List<string> lim = Utils.DataAccess.GetImagesOfGeocache(Core.ApplicationData.Instance.ActiveGeocache.Database, Core.ApplicationData.Instance.ActiveGeocache.Code);

                Dictionary<string, string> imgs = GAPPSF.ImageGrabber.OfflineImagesManager.Instance.GetImages(Core.ApplicationData.Instance.ActiveGeocache);
                foreach (var kp in imgs)
                {
                    ImageInfo ii = new ImageInfo();
                    ii.Url = kp.Key;
                    ii.FileName = kp.Value;
                    ImageInfoCollection.Add(ii);

                    if (lim.Contains(ii.Url))
                    {
                        lim.Remove(ii.Url);
                    }
                }
                foreach(string s in lim)
                {
                    ImageInfo ii = new ImageInfo();
                    ii.Url = s;
                    ii.FileName = "";
                    ImageInfoCollection.Add(ii);
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


        AsyncDelegateCommand _importCurrentCommand;
        public ICommand ImportCurrentCommand
        {
            get
            {
                if (_importCurrentCommand == null)
                {
                    _importCurrentCommand = new AsyncDelegateCommand(param => this.ImportCurrent(), param => Core.ApplicationData.Instance.ActiveGeocache != null);
                }
                return _importCurrentCommand;
            }
        }

        private async Task ImportCurrent()
        {
            if (Core.ApplicationData.Instance.ActiveGeocache != null)
            {
                List<Core.Data.Geocache> gcList = new List<Core.Data.Geocache>();
                gcList.Add(Core.ApplicationData.Instance.ActiveGeocache);
                await GAPPSF.ImageGrabber.OfflineImagesManager.Instance.DownloadImagesAsync(gcList, true);
            }
        }

        AsyncDelegateCommand _importSelectedCommand;
        public ICommand ImportSelectedCommand
        {
            get
            {
                if (_importSelectedCommand == null)
                {
                    _importSelectedCommand = new AsyncDelegateCommand(param => this.ImportCurrent(), param => Core.ApplicationData.Instance.ActiveDatabase != null && (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Selected select a).FirstOrDefault() != null);
                }
                return _importSelectedCommand;
            }
        }

        private async Task ImportSelected()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                List<Core.Data.Geocache> gcList = (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Selected select a).ToList();
                if (gcList.Count>0)
                {
                    await GAPPSF.ImageGrabber.OfflineImagesManager.Instance.DownloadImagesAsync(gcList, true);
                }
            }
        }

        public int WindowWidth
        {
            get
            {
                return Core.Settings.Default.OfflineImagesWindowWidth;
            }
            set
            {
                Core.Settings.Default.OfflineImagesWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.OfflineImagesWindowHeight;
            }
            set
            {
                Core.Settings.Default.OfflineImagesWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.OfflineImagesWindowLeft;
            }
            set
            {
                Core.Settings.Default.OfflineImagesWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.OfflineImagesWindowTop;
            }
            set
            {
                Core.Settings.Default.OfflineImagesWindowTop = value;
            }
        }


    }
}
