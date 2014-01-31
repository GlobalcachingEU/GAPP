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
                CollectionSelected = _selectedCollection != null;
            }
        }

        private string _selectedGeocacheCode;
        public string SelectedGeocacheCode
        {
            get { return _selectedGeocacheCode; }
            set
            {
                SetProperty(ref _selectedGeocacheCode, value);
                if (_selectedGeocacheCode != null)
                {
                    if (Core.ApplicationData.Instance.ActiveGeocache != null && Core.ApplicationData.Instance.ActiveGeocache.Code == _selectedGeocacheCode)
                    {
                        //already active
                    }
                    else if (Core.ApplicationData.Instance.ActiveDatabase!=null)
                    {
                        Core.Data.Geocache g = Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection.GetGeocache(_selectedGeocacheCode);
                        if (g != null)
                        {
                            Core.ApplicationData.Instance.ActiveGeocache = g;
                        }
                    }
                }
            }
        }

        private bool _collectionSelected;
        public bool CollectionSelected
        {
            get { return _collectionSelected; }
            set { SetProperty(ref _collectionSelected, value); }
        }

        public Control()
        {
            AvailableCollections = new ObservableCollection<string>();
            GeocachesInCollections = new ObservableCollection<string>();

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
            if (Core.ApplicationData.Instance.ActiveGeocache != null)
            {
                if (GeocachesInCollections.Contains(Core.ApplicationData.Instance.ActiveGeocache.Code))
                {
                    SelectedGeocacheCode = Core.ApplicationData.Instance.ActiveGeocache.Code;
                }
            }
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            inputDialog.Show(Localization.TranslationManager.Instance.Translate("Name").ToString());
            inputDialog.DialogClosed += newDialog_DialogClosed;
        }

        private void newDialog_DialogClosed(object sender, EventArgs e)
        {
            inputDialog.DialogClosed -= newDialog_DialogClosed;
            if (inputDialog.DialogResult)
            {
                if (!string.IsNullOrEmpty(inputDialog.InputText))
                {
                    string s = inputDialog.InputText.Trim();
                    if (s.Length > 0)
                    {
                        int id = Core.Settings.Default.GetCollectionID(s);
                        if (id < 0)
                        {
                            Core.Settings.Default.AddCollection(s);
                            AvailableCollections.Add(s);
                            SelectedCollection = s;
                        }
                    }
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (SelectedCollection != null && AvailableCollections.Contains(SelectedCollection))
            {
                Core.Settings.Default.DeleteCollection(SelectedCollection);
                AvailableCollections.Remove(SelectedCollection);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (Core.ApplicationData.Instance.ActiveDatabase!=null && SelectedCollection!=null)
            {
                var gcList = from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection
                             join b in GeocachesInCollections on a.Code equals b
                             select a;
                using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
                {
                    foreach(var gc in gcList)
                    {
                        gc.Selected = true;
                    }
                }
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (SelectedCollection != null)
            {
                int id = Core.Settings.Default.GetCollectionID(SelectedCollection);
                if (id >= 0)
                {
                    List<string> sl = new List<string>();
                    foreach (string s in gcInCollection.SelectedItems)
                    {
                        sl.Add(s);
                    }
                    foreach(string s in sl)
                    {
                        Core.Settings.Default.RemoveFromCollection(id, s);
                        GeocachesInCollections.Remove(s);
                    }
                }
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (SelectedCollection != null && Core.ApplicationData.Instance.ActiveGeocache!=null)
            {
                int id = Core.Settings.Default.GetCollectionID(SelectedCollection);
                if (id >= 0)
                {
                    if (!Core.Settings.Default.InCollection(id, Core.ApplicationData.Instance.ActiveGeocache.Code))
                    {
                        Core.Settings.Default.AddToCollection(id, Core.ApplicationData.Instance.ActiveGeocache.Code);
                        GeocachesInCollections.Add(Core.ApplicationData.Instance.ActiveGeocache.Code);
                        SelectedGeocacheCode = Core.ApplicationData.Instance.ActiveGeocache.Code;
                    }
                }
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (SelectedCollection != null && Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                int id = Core.Settings.Default.GetCollectionID(SelectedCollection);
                if (id >= 0)
                {
                    foreach (var g in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection)
                    {
                        if (g.Selected)
                        {
                            if (!Core.Settings.Default.InCollection(id, g.Code))
                            {
                                Core.Settings.Default.AddToCollection(id, g.Code);
                                GeocachesInCollections.Add(g.Code);
                            }
                        }
                    }
                }
            }
        }

    }
}
