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

namespace GAPPSF.UIControls.Trackables
{
    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl, IDisposable, IUIControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<TrackableGroup> AvailableTrackableGroups { get; private set; }
        public ObservableCollection<TrackableItem> AvailableTrackableItems { get; private set; }

        private TrackableGroup _selectedTrackableGroup;
        public TrackableGroup SelectedTrackableGroup
        {
            get { return _selectedTrackableGroup; }
            set
            {
                if (_selectedTrackableGroup != value)
                {
                    SetProperty(ref _selectedTrackableGroup, value);
                    IsTrackableGroupSelected = _selectedTrackableGroup != null;

                    AvailableTrackableItems.Clear();
                    if (_selectedTrackableGroup!=null)
                    {
                        List<TrackableItem> trks = Core.Settings.Default.GetTrackables(_selectedTrackableGroup);
                        foreach (var g in trks)
                        {
                            AvailableTrackableItems.Add(g);
                        }
                    }
                }
            }
        }

        private bool _isTrackableGroupSelected;
        public bool IsTrackableGroupSelected
        {
            get { return _isTrackableGroupSelected; }
            set { SetProperty(ref _isTrackableGroupSelected, value); }
        }

        private TrackableItem _selectedTrackableItem;
        public TrackableItem SelectedTrackableItem
        {
            get { return _selectedTrackableItem; }
            set
            {
                SetProperty(ref _selectedTrackableItem, value);
                IsTrackableItemSelected = _selectedTrackableItem != null;
            }
        }

        private bool _isTrackableItemSelected;
        public bool IsTrackableItemSelected
        {
            get { return _isTrackableItemSelected; }
            set { SetProperty(ref _isTrackableItemSelected, value); }
        }


        public Control()
        {
            AvailableTrackableGroups = new ObservableCollection<TrackableGroup>();
            AvailableTrackableItems = new ObservableCollection<TrackableItem>();

            try
            {
                List<TrackableGroup> grps = Core.Settings.Default.GetTrackableGroups();
                foreach(var g in grps)
                {
                    AvailableTrackableGroups.Add(g);
                }
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }

            InitializeComponent();

            DataContext = this;
        }

        public void Dispose()
        {
        }

        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate("TrackableGroups") as string;
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
                return Core.Settings.Default.TrackableGroupWindowWidth;
            }
            set
            {
                Core.Settings.Default.TrackableGroupWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.TrackableGroupWindowHeight;
            }
            set
            {
                Core.Settings.Default.TrackableGroupWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.TrackableGroupWindowLeft;
            }
            set
            {
                Core.Settings.Default.TrackableGroupWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.TrackableGroupWindowTop;
            }
            set
            {
                Core.Settings.Default.TrackableGroupWindowTop = value;
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
                        //add groep
                        int maxId = AvailableTrackableGroups.Max(x => x.ID);
                        maxId++;
                        TrackableGroup tg = new TrackableGroup();
                        tg.ID = maxId;
                        tg.Name = s;
                        try
                        {
                            Core.Settings.Default.AddTrackableGroup(tg);
                            AvailableTrackableGroups.Add(tg);
                            SelectedTrackableGroup = tg;
                        }
                        catch(Exception ex)
                        {
                            Core.ApplicationData.Instance.Logger.AddLog(this, ex);
                        }
                    }
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (SelectedTrackableGroup!=null)
            {
                try
                {
                    TrackableGroup tg = SelectedTrackableGroup;
                    Core.Settings.Default.DeleteTrackableGroup(tg);
                    AvailableTrackableGroups.Remove(tg);
                }
                catch (Exception ex)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, ex);
                }
            }
        }

        private AsyncDelegateCommand _addOwnTrackablesCommand;
        public AsyncDelegateCommand AddOwnTrackablesCommand
        {
            get
            {
                if (_addOwnTrackablesCommand==null)
                {
                    _addOwnTrackablesCommand = new AsyncDelegateCommand(param => AddOwnTrackables(),
                        param => SelectedTrackableGroup != null);
                }
                return _addOwnTrackablesCommand;
            }
        }
        public async Task AddOwnTrackables()
        {
            if (SelectedTrackableGroup != null)
            {
                var tbg = SelectedTrackableGroup;
                Import imp = new Import();
                await imp.AddOwnTrackablesAsync(SelectedTrackableGroup);
                SelectedTrackableGroup = null;
                SelectedTrackableGroup = tbg;
            }
        }

        private AsyncDelegateCommand _addNewTrackablesCommand;
        public AsyncDelegateCommand AddNewTrackablesCommand
        {
            get
            {
                if (_addNewTrackablesCommand == null)
                {
                    _addNewTrackablesCommand = new AsyncDelegateCommand(param => AddNewTrackables(),
                        param => SelectedTrackableGroup != null);
                }
                return _addNewTrackablesCommand;
            }
        }
        public async Task AddNewTrackables()
        {
            if (SelectedTrackableGroup != null && !string.IsNullOrEmpty(addTrackables.Text))
            {
                var tbg = SelectedTrackableGroup;
                string[] parts = addTrackables.Text.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                List<string> tbcodes = (from a in parts where a.ToUpper().StartsWith("TB") select a.ToUpper()).ToList();
                Import imp = new Import();
                await imp.AddUpdateTrackablesAsync(SelectedTrackableGroup, tbcodes);
                SelectedTrackableGroup = null;
                SelectedTrackableGroup = tbg;
            }
        }

    }
}
