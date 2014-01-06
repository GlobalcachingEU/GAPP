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

namespace GAPPSF.UIControls.Attachement
{
    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl, IUIControl, INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<GAPPSF.Attachement.Item> Attachements { get; set; }

        private GAPPSF.Attachement.Item _editingForComments = null;

        private GAPPSF.Attachement.Item _selectedItem;
        public GAPPSF.Attachement.Item SelectedItem
        {
            get { return _selectedItem; }
            set 
            { 
                SetProperty(ref _selectedItem, value);
                IsItemSelected = value != null;
            }
        }

        private bool _isItemSelected;
        public bool IsItemSelected
        {
            get { return _isItemSelected; }
            set { SetProperty(ref _isItemSelected, value); }
        }

        public Control()
        {
            Attachements = new ObservableCollection<GAPPSF.Attachement.Item>();

            InitializeComponent();

            DataContext = this;
            UpdateView();

            Core.ApplicationData.Instance.PropertyChanged += Instance_PropertyChanged;
        }

        void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName=="ActiveGeocache")
            {
                UpdateView();
            }
        }

        public void Dispose()
        {
            Core.ApplicationData.Instance.PropertyChanged -= Instance_PropertyChanged;
        }

        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate("Attachements") as string;
        }

        private void UpdateView()
        {
            Attachements.Clear();
            if (Core.ApplicationData.Instance.ActiveGeocache!=null)
            {
                List<GAPPSF.Attachement.Item> its = GAPPSF.Attachement.Manager.Instance.GetAttachements(Core.ApplicationData.Instance.ActiveGeocache.Code);
                foreach(var it in its)
                {
                    Attachements.Add(it);
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
                return Core.Settings.Default.AttachementWindowWidth;
            }
            set
            {
                Core.Settings.Default.AttachementWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.AttachementWindowHeight;
            }
            set
            {
                Core.Settings.Default.AttachementWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.AttachementWindowLeft;
            }
            set
            {
                Core.Settings.Default.AttachementWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.AttachementWindowTop;
            }
            set
            {
                Core.Settings.Default.AttachementWindowTop = value;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Core.ApplicationData.Instance.ActiveGeocache != null)
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.FileName = ""; // Default file name
                dlg.DefaultExt = ".*"; // Default file extension
                dlg.Filter = "All files (*.*)|*.*"; // Filter files by extension 

                // Show open file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process open file dialog box results 
                if (result == true)
                {
                    //add
                    _editingForComments = new GAPPSF.Attachement.Item();
                    _editingForComments.GeocacheCode = Core.ApplicationData.Instance.ActiveGeocache.Code;
                    _editingForComments.FileName = dlg.FileName;
                    _editingForComments.Comment = "";

                    inputDialog.Show(Localization.TranslationManager.Instance.Translate("Comment").ToString());
                    inputDialog.DialogClosed += newDialog_DialogClosed;

                }
            }
        }

        private void newDialog_DialogClosed(object sender, EventArgs e)
        {
            inputDialog.DialogClosed -= newDialog_DialogClosed;
            if (inputDialog.DialogResult)
            {
                string s = inputDialog.InputText.Trim();
                if (_editingForComments != null)
                {
                    _editingForComments.Comment = s;
                    GAPPSF.Attachement.Manager.Instance.AddAttachement(_editingForComments);
                    Attachements.Add(_editingForComments);
                    SelectedItem = _editingForComments;
                }
            }
            _editingForComments = null;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (SelectedItem!=null)
            {
                var it = SelectedItem;
                Attachements.Remove(it);
                GAPPSF.Attachement.Manager.Instance.DeleteAttachement(it);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (SelectedItem != null)
            {
                try
                {
                    System.Diagnostics.Process.Start(SelectedItem.FileName);
                }
                catch(Exception ex)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, ex);
                }
            }
        }

    }
}
