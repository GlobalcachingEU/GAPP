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

namespace GAPPSF.UIControls.GeocacheCollection
{
    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl, IUIControl, INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> AvailableCollections { get; set; }
        public ObservableCollection<string> GeocachesInCollections { get; set; }

        private string _selectedCollection;
        public string SelectedCollection
        {
            get { return _selectedCollection; }
            set
            {
                if (_selectedCollection != value)
                {
                    SetProperty(ref _selectedCollection, value);
                    GeocachesInCollections.Clear();
                    if (!string.IsNullOrEmpty(_selectedCollection))
                    {
                        List<string> gcl = Core.Settings.Default.GetGeocachesInCollection(_selectedCollection);
                        foreach(string s in gcl)
                        {
                            GeocachesInCollections.Add(s);
                        }
                        UpdateView();
                    }
                }
            }
        }

        private string _selectedGeocacheCode;
        public string SelectedGeocacheCode
        {
            get { return _selectedGeocacheCode; }
            set
            {
                SetProperty(ref _selectedGeocacheCode, value);
                //todo: make active if exists and not active already
            }
        }

        public Control()
        {
            InitializeComponent();

            Core.ApplicationData.Instance.PropertyChanged += Instance_PropertyChanged;
            List<string> ac = Core.Settings.Default.AvailableCollections();
            foreach(string c in ac)
            {
                AvailableCollections.Add(c);
            }
            DataContext = this;
        }

        void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveGeocache")
            {
                UpdateView();
            }
        }

        public void Dispose()
        {
            Core.ApplicationData.Instance.PropertyChanged -= Instance_PropertyChanged;
        }

        private void UpdateView()
        {
            //todo: set SelectedGeocacheCode if in list (activegeocache)
        }

        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate("GeocacheCollections") as string;
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
                return Core.Settings.Default.GCEditorWindowWidth;
            }
            set
            {
                Core.Settings.Default.GCEditorWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.GCEditorWindowHeight;
            }
            set
            {
                Core.Settings.Default.GCEditorWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.GCEditorWindowLeft;
            }
            set
            {
                Core.Settings.Default.GCEditorWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.GCEditorWindowTop;
            }
            set
            {
                Core.Settings.Default.GCEditorWindowTop = value;
            }
        }

    }
}
