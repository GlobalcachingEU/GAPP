using GAPPSF.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GAPPSF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Core.Storage.Database _currentConnectedDatabase = null;

        private string _popUpText = "";
        public string PopUpText 
        {
            get { return _popUpText; }
            set { SetProperty(ref _popUpText, value); } 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            CurrentConnectedDatabase = Core.ApplicationData.Instance.ActiveDatabase;

            this.DataContext = this;

            GridLength rpgl = Core.Settings.Default.MainWindowRightPanelWidth;
            GridLength blgl = Core.Settings.Default.MainWindowBottomLeftPanelWidth;
            GridLength bpgl = Core.Settings.Default.MainWindowBottomPanelHeight;

            Core.ApplicationData.Instance.MainWindow = this;
            Dialogs.ProgessWindow prog = Dialogs.ProgessWindow.Instance;
            InitializeComponent();

            SetFeatureControl(leftPanelContent, Core.Settings.Default.MainWindowLeftPanelFeature, "GAPPSF.UIControls.ApplicationDataInfo");
            SetFeatureControl(topPanelContent, Core.Settings.Default.MainWindowTopPanelFeature, "GAPPSF.UIControls.CacheList");
            SetFeatureControl(bottomLeftPanelContent, Core.Settings.Default.MainWindowBottomLeftPanelFeature, "GAPPSF.UIControls.GeocacheViewer");
            SetFeatureControl(bottomRightPanelContent, Core.Settings.Default.MainWindowBottomRightPanelFeature, "");
            SetFeatureControl(rightPanelContent, Core.Settings.Default.MainWindowRightPanelFeature, "");
            SetFeatureControl(expandedPanelContent, Core.Settings.Default.MainWindowExpandedPanelFeature, "");

            leftPanelContent.PropertyChanged += leftPanelContent_PropertyChanged;
            topPanelContent.PropertyChanged += topPanelContent_PropertyChanged;
            bottomLeftPanelContent.PropertyChanged += bottomLeftPanelContent_PropertyChanged;
            bottomRightPanelContent.PropertyChanged += bottomRightPanelContent_PropertyChanged;
            rightPanelContent.PropertyChanged += rightPanelContent_PropertyChanged;
            expandedPanelContent.PropertyChanged += expandedPanelContent_PropertyChanged;

            leftPanelContent.WindowStateButtonClick += panelContent_WindowStateButtonClick;
            topPanelContent.WindowStateButtonClick += panelContent_WindowStateButtonClick;
            bottomLeftPanelContent.WindowStateButtonClick += panelContent_WindowStateButtonClick;
            bottomRightPanelContent.WindowStateButtonClick += panelContent_WindowStateButtonClick;
            rightPanelContent.WindowStateButtonClick += panelContent_WindowStateButtonClick;
            expandedPanelContent.WindowStateButtonClick += panelContent_WindowStateButtonClick;

            if (expandedPanelContent.FeatureControl!=null)
            {
                normalView.Visibility = System.Windows.Visibility.Collapsed;
                expandedView.Visibility = System.Windows.Visibility.Visible;
            }

            Core.ApplicationData.Instance.PropertyChanged += Instance_PropertyChanged;
            Core.Settings.Default.PropertyChanged += Default_PropertyChanged;
            Core.ApplicationData.Instance.Logger.LogAdded += Logger_LogAdded;

            rightPanelColumn.Width = rpgl;
            bottomLeftPanelColumn.Width = blgl;
            bottomPanelsRow.Height = bpgl;

            updateShortCutKeyAssignment();

            UIControls.ActionBuilder.Manager mng = UIControls.ActionBuilder.Manager.Instance;
            ActionSequence.Manager mng2 = ActionSequence.Manager.Instance;
            //popup.IsOpen = true;
        }

        void Logger_LogAdded(object sender, Core.Logger.LogEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (e.Level == Core.Logger.Level.Error)
                {
                    PopUpText = e.Message;
                    popup.IsOpen = true;
                }
            }));
        }

        void Default_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MainWindowShortCutKeyAssignment")
            {
                updateShortCutKeyAssignment();
            }
        }

        private void clearShortCutKey(MenuItem mi)
        {
            mi.InputGestureText = "";
            foreach (var m in mi.Items)
            {
                if (m is MenuItem) clearShortCutKey(m as MenuItem);
            }
        }
        private void updateShortCutKeyAssignment()
        {
            this.InputBindings.Clear();
            //Shift|Control|Alt|Windows|MenuName|Key

            foreach(var m in mainMenu.Items)
            {
                if (m is MenuItem) clearShortCutKey(m as MenuItem);
            }

            if (!string.IsNullOrEmpty(Core.Settings.Default.MainWindowShortCutKeyAssignment))
            {
                string[] lines = Core.Settings.Default.MainWindowShortCutKeyAssignment.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string l in lines)
                {
                    string[] parts = l.Split(new char[] { '|' }, 6);
                    if (parts.Length == 6)
                    {
                        MenuItem mi = FindName(parts[4]) as MenuItem;
                        if (mi != null)
                        {
                            string decoration = "";
                            ModifierKeys mk = ModifierKeys.None;
                            if (bool.Parse(parts[0]))
                            {
                                mk |= ModifierKeys.Shift;
                                decoration = string.Concat(decoration, "Shift");
                            }
                            if (bool.Parse(parts[1]))
                            {
                                mk |= ModifierKeys.Control;
                                if (decoration.Length > 0)
                                {
                                    decoration = string.Concat(decoration, "+");
                                }
                                decoration = string.Concat(decoration, "Ctrl");
                            }
                            if (bool.Parse(parts[2]))
                            {
                                mk |= ModifierKeys.Alt;
                                if (decoration.Length > 0)
                                {
                                    decoration = string.Concat(decoration, "+");
                                }
                                decoration = string.Concat(decoration, "Alt");
                            }
                            if (bool.Parse(parts[3]))
                            {
                                mk |= ModifierKeys.Windows;
                                if (decoration.Length > 0)
                                {
                                    decoration = string.Concat(decoration, "+");
                                }
                                decoration = string.Concat(decoration, "Win");
                            }
                            if (decoration.Length > 0)
                            {
                                decoration = string.Concat(decoration, "+");
                            }
                            decoration = string.Concat(decoration, parts[5]);
                            Key k = (Key)Enum.Parse(typeof(Key), parts[5]);
                            this.InputBindings.Add(new KeyBinding(ShortCutKeyCommand, new KeyGesture(k, mk)) { CommandParameter = mi });
                            mi.InputGestureText = decoration;
                        }
                    }
                }
            }
        }

        private RelayCommand _shortCutKeyCommand = null;
        public RelayCommand ShortCutKeyCommand
        {
            get
            {
                if (_shortCutKeyCommand==null)
                {
                    _shortCutKeyCommand = new RelayCommand(param => handleShortCutKey(param));
                }
                return _shortCutKeyCommand;
            }
        }
        private void handleShortCutKey(object e)
        {
            MenuItem mni = e as MenuItem;
            if (mni != null)
            {
                MenuItemAutomationPeer p = new MenuItemAutomationPeer(mni);
                IInvokeProvider ip = p.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                ip.Invoke();
            }
        }

        private string _statusBarBackgroundColor = "#FF007ACC";
        public string StatusBarBackgroundColor
        {
            get { return _statusBarBackgroundColor; }
            set { SetProperty(ref _statusBarBackgroundColor, value); }
        }

        void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveDatabase")
            {
                CurrentConnectedDatabase = Core.ApplicationData.Instance.ActiveDatabase;
            }
            else if (e.PropertyName == "UIIsIdle")
            {
                StatusBarBackgroundColor = Core.ApplicationData.Instance.UIIsIdle ? "#FF007ACC" : "#FFE9760E";
            }
        }

        private async Task initializeApplicationAsync()
        {
            if (Core.Settings.Default.FirstStart)
            {
                Core.Settings.Default.FirstStart = false;
                SetupWizard.SetupWizardWindow dlg = new SetupWizard.SetupWizardWindow();
                dlg.ShowDialog();
            }
            else
            {
                Core.ApplicationData.Instance.BeginActiviy();

                bool autoLoad = Core.Settings.Default.AutoLoadDatabases;
                string dbs = Core.Settings.Default.OpenedDatabases;
                string actDb = Core.Settings.Default.ActiveDatabase;
                string actGc = Core.Settings.Default.ActiveGeocache;
                Core.Settings.Default.OpenedDatabases = "";
                if (autoLoad && !string.IsNullOrEmpty(dbs))
                {
                    string[] lines = dbs.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    int index = 0;
                    using (Utils.ProgressBlock prog = new Utils.ProgressBlock("LoadingDatabases", "LoadingDatabases", lines.Length, 0, true))
                    {
                        foreach (string s in lines)
                        {
                            prog.Update(s, lines.Length, index);

                            Core.Storage.Database db = new Core.Storage.Database(s);
                            bool success = await db.InitializeAsync();
                            if (success)
                            {
                                Core.ApplicationData.Instance.Databases.Add(db);
                            }
                            else
                            {
                                db.Dispose();
                            }

                            index++;
                            if (!prog.Update(s, lines.Length, index))
                            {
                                break;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(actDb))
                    {
                        Core.ApplicationData.Instance.ActiveDatabase = (from a in Core.ApplicationData.Instance.Databases where a.FileName == actDb select a).FirstOrDefault();
                    }
                    if (Core.ApplicationData.Instance.ActiveDatabase != null && !string.IsNullOrEmpty(actGc))
                    {
                        Core.ApplicationData.Instance.ActiveGeocache = (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Code == actGc select a).FirstOrDefault();
                    }
                }

                Core.ApplicationData.Instance.EndActiviy();
            }
        }

        private Core.Storage.Database CurrentConnectedDatabase
        {
            get { return _currentConnectedDatabase; }
            set
            {
                if (_currentConnectedDatabase != value)
                {
                    if (_currentConnectedDatabase != null)
                    {
                        _currentConnectedDatabase.GeocacheCollection.CollectionChanged -= GeocacheCollection_CollectionChanged;
                        _currentConnectedDatabase.GeocacheCollection.GeocacheDataChanged -= GeocacheCollection_GeocacheDataChanged;
                        _currentConnectedDatabase.GeocacheCollection.GeocachePropertyChanged -= GeocacheCollection_GeocachePropertyChanged;
                    }
                    _currentConnectedDatabase = value;
                    if (_currentConnectedDatabase != null)
                    {
                        _currentConnectedDatabase.GeocacheCollection.CollectionChanged += GeocacheCollection_CollectionChanged;
                        _currentConnectedDatabase.GeocacheCollection.GeocacheDataChanged += GeocacheCollection_GeocacheDataChanged;
                        _currentConnectedDatabase.GeocacheCollection.GeocachePropertyChanged += GeocacheCollection_GeocachePropertyChanged;
                    }
                    UpdateView();
                }
            }
        }
        void GeocacheCollection_GeocachePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Core.Data.Geocache gc = sender as Core.Data.Geocache;
            if (gc != null)
            {
                if (gc == Core.ApplicationData.Instance.ActiveGeocache || e.PropertyName == "Selected")
                {
                    if (e.PropertyName == "Name" ||
                        e.PropertyName == "Selected")
                    {
                        UpdateView();
                    }
                }
            }
        }
        void GeocacheCollection_GeocacheDataChanged(object sender, EventArgs e)
        {
            Core.Data.Geocache gc = sender as Core.Data.Geocache;
            if (gc != null)
            {
                if (gc == Core.ApplicationData.Instance.ActiveGeocache)
                {
                    UpdateView();
                }
            }
        }
        void GeocacheCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateView();
        }

        public void UpdateView()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase==null)
            {
                GeocacheSelectionCount = 0;
            }
            else
            {
                GeocacheSelectionCount = (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Selected select a).Count();
            }
        }

        private int _geocacheSelectionCount = 0;
        public int GeocacheSelectionCount
        {
            get { return _geocacheSelectionCount; }
            set { SetProperty(ref _geocacheSelectionCount, value); }
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

        void panelContent_WindowStateButtonClick(object sender, EventArgs e)
        {
            UIControls.UIControlContainer ucc = sender as UIControls.UIControlContainer;
            if (ucc != null)
            {
                ucc.DisposeOnClear = false;
                if (ucc == expandedPanelContent)
                {
                    //minimize
                    UserControl uc = ucc.FeatureControl;
                    ucc.FeatureControl = null;
                    UIControls.UIControlContainer targetUc = normalView.FindName(Core.Settings.Default.MainWindowMiximizedPanelName) as UIControls.UIControlContainer;
                    if (targetUc != null)
                    {
                        targetUc.DisposeOnClear = true;
                        targetUc.FeatureControl = uc;
                        targetUc.DisposeOnClear = false;
                    }
                    expandedView.Visibility = System.Windows.Visibility.Collapsed;
                    normalView.Visibility = System.Windows.Visibility.Visible;

                    Core.Settings.Default.MainWindowMiximizedPanelName = "";
                }
                else
                {
                    //maximize
                    Core.Settings.Default.MainWindowMiximizedPanelName = ucc.Name;
                    UserControl uc = ucc.FeatureControl;
                    ucc.FeatureControl = null;
                    expandedPanelContent.DisposeOnClear = false;
                    expandedPanelContent.FeatureControl = uc;
                    expandedPanelContent.DisposeOnClear = true;
                    normalView.Visibility = System.Windows.Visibility.Collapsed;
                    expandedView.Visibility = System.Windows.Visibility.Visible;
                }
                ucc.DisposeOnClear = true;
            }
        }

        private bool _shown;
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (_shown)
                return;

            _shown = true;

            string s = Core.Settings.Default.MainWindowWindowFeature;
            if (!string.IsNullOrEmpty(s))
            {
                string[] parts = s.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string p in parts)
                {
                    Type t = Type.GetType(p);
                    if (t != null)
                    {
                        ConstructorInfo constructor = t.GetConstructor(Type.EmptyTypes);
                        UserControl uc = (UserControl)constructor.Invoke(Type.EmptyTypes);
                        FeatureWindow w = new FeatureWindow(uc);
                        w.Owner = this;
                        w.Show();
                    }
                }
            }
        }

        void rightPanelContent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FeatureControl")
            {
                Core.Settings.Default.MainWindowRightPanelFeature = rightPanelContent.FeatureControl == null ? "" : rightPanelContent.FeatureControl.GetType().ToString();
            }
        }

        void expandedPanelContent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FeatureControl")
            {
                Core.Settings.Default.MainWindowExpandedPanelFeature = expandedPanelContent.FeatureControl == null ? "" : expandedPanelContent.FeatureControl.GetType().ToString();
            }
        }


        void bottomRightPanelContent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FeatureControl")
            {
                Core.Settings.Default.MainWindowBottomRightPanelFeature = bottomRightPanelContent.FeatureControl == null ? "" : bottomRightPanelContent.FeatureControl.GetType().ToString();
            }
        }

        void bottomLeftPanelContent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FeatureControl")
            {
                Core.Settings.Default.MainWindowBottomLeftPanelFeature = bottomLeftPanelContent.FeatureControl == null ? "" : bottomLeftPanelContent.FeatureControl.GetType().ToString();
            }
        }

        void topPanelContent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FeatureControl")
            {
                Core.Settings.Default.MainWindowTopPanelFeature = topPanelContent.FeatureControl == null ? "" : topPanelContent.FeatureControl.GetType().ToString();
            }
        }

        void leftPanelContent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FeatureControl")
            {
                Core.Settings.Default.MainWindowLeftPanelFeature = leftPanelContent.FeatureControl == null ? "" : leftPanelContent.FeatureControl.GetType().ToString();
            }
        }

        public void SetFeatureControl(UIControls.UIControlContainer container, string setting, string defaultFeature)
        {
            if (setting == null)
            {
                setting = defaultFeature;
            }
            if (!string.IsNullOrEmpty(setting))
            {
                Type t = Type.GetType(setting);
                if (t != null)
                {
                    ConstructorInfo constructor = t.GetConstructor(Type.EmptyTypes);
                    UserControl uc = (UserControl)constructor.Invoke(Type.EmptyTypes);
                    container.FeatureControl = uc;
                }
            }

        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Core.Settings.Default.MainWindowRightPanelWidth = rightPanelColumn.Width;
            Core.Settings.Default.MainWindowBottomLeftPanelWidth = bottomLeftPanelColumn.Width;
            Core.Settings.Default.MainWindowBottomPanelHeight = bottomPanelsRow.Height;
            Dialogs.ProgessWindow.Instance.Close();
            base.OnClosing(e);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Window w = new FeatureWindow(new UIControls.CacheList());
            w.Owner = this;
            w.Show();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Window w = new FeatureWindow(new UIControls.GeocacheViewer());
            w.Owner = this;
            w.Show();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            Localization.TranslationManager.Instance.CurrentLanguage = CultureInfo.InvariantCulture;
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            Localization.TranslationManager.Instance.CurrentLanguage = new CultureInfo("en-US");
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            Localization.TranslationManager.Instance.CurrentLanguage = new CultureInfo("de-DE");
        }

        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            Localization.TranslationManager.Instance.CurrentLanguage = new CultureInfo("fr-FR");
        }

        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            Localization.TranslationManager.Instance.CurrentLanguage = new CultureInfo("nl-NL");
        }

        AsyncDelegateCommand _deleteActiveCommand;
        public ICommand DeleteActiveCommand
        {
            get
            {
                if (_deleteActiveCommand == null)
                {
                    _deleteActiveCommand = new AsyncDelegateCommand(param => this.DeleteActiveGeocache(),
                        param => Core.ApplicationData.Instance.ActiveGeocache != null);
                }
                return _deleteActiveCommand;
            }
        }

        async public Task DeleteActiveGeocache()
        {
            if (Core.ApplicationData.Instance.ActiveGeocache != null)
            {
                Core.Data.Geocache gc = Core.ApplicationData.Instance.ActiveGeocache;
                Core.ApplicationData.Instance.ActiveGeocache = null;
                using (Utils.DataUpdater upd = new Utils.DataUpdater(gc.Database))
                {
                    await Task.Run(() =>
                    {
                        Utils.DataAccess.DeleteGeocache(gc);
                    });
                }
            }
        }

        AsyncDelegateCommand _deleteSelectionCommand;
        public ICommand DeleteSelectionCommand
        {
            get
            {
                if (_deleteSelectionCommand == null)
                {
                    _deleteSelectionCommand = new AsyncDelegateCommand(param => this.DeleteSelectionGeocache(),
                        param => GeocacheSelectionCount>0);
                }
                return _deleteSelectionCommand;
            }
        }

        async public Task DeleteSelectionGeocache()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                if (Core.ApplicationData.Instance.ActiveGeocache != null && Core.ApplicationData.Instance.ActiveGeocache.Selected)
                {
                    Core.ApplicationData.Instance.ActiveGeocache = null;
                }
                using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
                {
                    await Task.Run(() =>
                    {
                        int index = 0;
                        DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                        List<Core.Data.Geocache> gcList = (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Selected select a).ToList();
                        using (Utils.ProgressBlock prog = new Utils.ProgressBlock("DeletingGeocaches", "DeletingGeocaches", gcList.Count, 0, true))
                        {
                            foreach (var gc in gcList)
                            {
                                Utils.DataAccess.DeleteGeocache(gc);
                                index++;

                                if (DateTime.Now >= nextUpdate)
                                {
                                    if (!prog.Update("Deleting geocaches...", gcList.Count, index))
                                    {
                                        break;
                                    }
                                    nextUpdate = DateTime.Now.AddSeconds(1);
                                }
                            }
                        }
                    });
                }
            }
        }


        AsyncDelegateCommand _importGpxCommand;
        public ICommand ImportGPXCommand
        {
            get
            {
                if (_importGpxCommand == null)
                {
                    _importGpxCommand = new AsyncDelegateCommand(param => this.ImportGPXFile(),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null);
                }
                return _importGpxCommand;
            }
        }

        async private Task ImportGPXFile()
        {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".gpx"; // Default file extension
            dlg.Filter = "GPX files (.gpx)|*.gpx|Pocket Query files (.zip)|*.zip|Garmin GGZ files (.ggz)|*.ggz"; // Filter files by extension 

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                GPX.Import imp = new GPX.Import();
                await imp.ImportFilesAsync(dlg.FileNames);
            }
        }


        AsyncDelegateCommand _updateStatusActiveCommand;
        public ICommand UpdateStatusActiveCommand
        {
            get
            {
                if (_updateStatusActiveCommand == null)
                {
                    _updateStatusActiveCommand = new AsyncDelegateCommand(param => this.UpdateStatusActive(),
                        param => Core.ApplicationData.Instance.ActiveGeocache != null && Core.Settings.Default.LiveAPIMemberTypeId > 0);
                }
                return _updateStatusActiveCommand;
            }
        }
        private async Task UpdateStatusActive()
        {
            if (Core.ApplicationData.Instance.ActiveGeocache != null)
            {
                await UpdateStatusGeocaches(new Core.Data.Geocache[] { Core.ApplicationData.Instance.ActiveGeocache }.ToList());
            }
        }
        AsyncDelegateCommand _updateStatusSelectedCommand;
        public ICommand UpdateStatusSelectedCommand
        {
            get
            {
                if (_updateStatusSelectedCommand == null)
                {
                    _updateStatusSelectedCommand = new AsyncDelegateCommand(param => this.UpdateStatusSelected(),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null && this.GeocacheSelectionCount > 0 && Core.Settings.Default.LiveAPIMemberTypeId > 0);
                }
                return _updateStatusSelectedCommand;
            }
        }
        private async Task UpdateStatusSelected()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                await UpdateStatusGeocaches((from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Selected select a).ToList());
            }
        }
        AsyncDelegateCommand _updateStatusAllCommand;
        public ICommand UpdateStatusAllCommand
        {
            get
            {
                if (_updateStatusAllCommand == null)
                {
                    _updateStatusAllCommand = new AsyncDelegateCommand(param => this.UpdateStatusAll(),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null);
                }
                return _updateStatusAllCommand;
            }
        }
        private async Task UpdateStatusAll()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                await UpdateStatusGeocaches(Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection);
            }
        }
        private async Task UpdateStatusGeocaches(List<Core.Data.Geocache> gcList)
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
                {
                    await Task.Run(() =>
                    {
                        LiveAPI.Import.ImportGeocacheStatus(Core.ApplicationData.Instance.ActiveDatabase, (from a in gcList select a.Code).ToList());
                    });
                }
            }
        }



        AsyncDelegateCommand _updateActiveCommand;
        public ICommand UpdateActiveCommand
        {
            get
            {
                if (_updateActiveCommand == null)
                {
                    _updateActiveCommand = new AsyncDelegateCommand(param => this.UpdateActive(),
                        param => Core.ApplicationData.Instance.ActiveGeocache != null && Core.Settings.Default.LiveAPIMemberTypeId>0);
                }
                return _updateActiveCommand;
            }
        }
        private async Task UpdateActive()
        {
            if (Core.ApplicationData.Instance.ActiveGeocache != null)
            {
                await UpdateGeocaches(new Core.Data.Geocache[] { Core.ApplicationData.Instance.ActiveGeocache }.ToList());
            }
        }
        AsyncDelegateCommand _updateSelectedCommand;
        public ICommand UpdateSelectedCommand
        {
            get
            {
                if (_updateSelectedCommand == null)
                {
                    _updateSelectedCommand = new AsyncDelegateCommand(param => this.UpdateSelected(),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null && this.GeocacheSelectionCount > 0 && Core.Settings.Default.LiveAPIMemberTypeId > 0);
                }
                return _updateSelectedCommand;
            }
        }
        private async Task UpdateSelected()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                await UpdateGeocaches((from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Selected select a).ToList());
            }
        }
        AsyncDelegateCommand _updateAllCommand;
        public ICommand UpdateAllCommand
        {
            get
            {
                if (_updateAllCommand == null)
                {
                    _updateAllCommand = new AsyncDelegateCommand(param => this.UpdateAll(),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null);
                }
                return _updateAllCommand;
            }
        }
        private async Task UpdateAll()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                await UpdateGeocaches(Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection);
            }
        }
        private async Task UpdateGeocaches(List<Core.Data.Geocache> gcList)
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
                {
                    await Task.Run(() =>
                        {
                            LiveAPI.Import.ImportGeocaches(Core.ApplicationData.Instance.ActiveDatabase, (from a in gcList select a.Code).ToList());
                        });
                }
            }
        }



        RelayCommand _exportIGKActiveCommand;
        public ICommand ExportIGKActiveCommand
        {
            get
            {
                if (_exportIGKActiveCommand == null)
                {
                    _exportIGKActiveCommand = new RelayCommand(param => this.ExportIGKActive(),
                        param => Core.ApplicationData.Instance.ActiveGeocache != null);
                }
                return _exportIGKActiveCommand;
            }
        }
        private void ExportIGKActive()
        {
            if (Core.ApplicationData.Instance.ActiveGeocache != null)
            {
                ExportIGK(new Core.Data.Geocache[] { Core.ApplicationData.Instance.ActiveGeocache }.ToList());
            }
        }
        RelayCommand _exportIGKSelectedCommand;
        public ICommand ExportIGKSelectedCommand
        {
            get
            {
                if (_exportIGKSelectedCommand == null)
                {
                    _exportIGKSelectedCommand = new RelayCommand(param => this.ExportIGKSelected(),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null && this.GeocacheSelectionCount > 0);
                }
                return _exportIGKSelectedCommand;
            }
        }
        private void ExportIGKSelected()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                ExportIGK((from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Selected select a).ToList());
            }
        }
        RelayCommand _exportIGKAllCommand;
        public ICommand ExportIGKAllCommand
        {
            get
            {
                if (_exportIGKAllCommand == null)
                {
                    _exportIGKAllCommand = new RelayCommand(param => this.ExportIGKAll(),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null);
                }
                return _exportIGKAllCommand;
            }
        }
        private void ExportIGKAll()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                ExportIGK(Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection);
            }
        }
        private void ExportIGK(List<Core.Data.Geocache> gcList)
        {
            iGeoKnife.ExportWindow dlg = new iGeoKnife.ExportWindow(gcList);
            dlg.ShowDialog();
        }




        RelayCommand _exportOV2ActiveCommand;
        public ICommand ExportOV2ActiveCommand
        {
            get
            {
                if (_exportOV2ActiveCommand == null)
                {
                    _exportOV2ActiveCommand = new RelayCommand(param => this.ExportOV2Active(),
                        param => Core.ApplicationData.Instance.ActiveGeocache != null);
                }
                return _exportOV2ActiveCommand;
            }
        }
        private void ExportOV2Active()
        {
            if (Core.ApplicationData.Instance.ActiveGeocache != null)
            {
                ExportOV2(new Core.Data.Geocache[] { Core.ApplicationData.Instance.ActiveGeocache }.ToList());
            }
        }
        RelayCommand _exportOV2SelectedCommand;
        public ICommand ExportOV2SelectedCommand
        {
            get
            {
                if (_exportOV2SelectedCommand == null)
                {
                    _exportOV2SelectedCommand = new RelayCommand(param => this.ExportOV2Selected(),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null && this.GeocacheSelectionCount > 0);
                }
                return _exportOV2SelectedCommand;
            }
        }
        private void ExportOV2Selected()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                ExportOV2((from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Selected select a).ToList());
            }
        }
        RelayCommand _exportOV2AllCommand;
        public ICommand ExportOV2AllCommand
        {
            get
            {
                if (_exportOV2AllCommand == null)
                {
                    _exportOV2AllCommand = new RelayCommand(param => this.ExportOV2All(),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null);
                }
                return _exportOV2AllCommand;
            }
        }
        private void ExportOV2All()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                ExportOV2(Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection);
            }
        }
        private void ExportOV2(List<Core.Data.Geocache> gcList)
        {
            OV2.ExportWindow dlg = new OV2.ExportWindow(gcList);
            dlg.ShowDialog();
        }



        RelayCommand _exportGPXActiveCommand;
        public ICommand ExportGPXActiveCommand
        {
            get
            {
                if (_exportGPXActiveCommand == null)
                {
                    _exportGPXActiveCommand = new RelayCommand(param => this.ExportGPXActive(),
                        param => Core.ApplicationData.Instance.ActiveGeocache != null);
                }
                return _exportGPXActiveCommand;
            }
        }
        private void ExportGPXActive()
        {
            if (Core.ApplicationData.Instance.ActiveGeocache != null)
            {
                ExportGPX(new Core.Data.Geocache[] { Core.ApplicationData.Instance.ActiveGeocache }.ToList());
            }
        }
        RelayCommand _exportGPXSelectedCommand;
        public ICommand ExportGPXSelectedCommand
        {
            get
            {
                if (_exportGPXSelectedCommand == null)
                {
                    _exportGPXSelectedCommand = new RelayCommand(param => this.ExportGPXSelected(),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null && this.GeocacheSelectionCount > 0);
                }
                return _exportGPXSelectedCommand;
            }
        }
        private void ExportGPXSelected()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                ExportGPX((from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Selected select a).ToList());
            }
        }
        RelayCommand _exportGPXAllCommand;
        public ICommand ExportGPXAllCommand
        {
            get
            {
                if (_exportGPXAllCommand == null)
                {
                    _exportGPXAllCommand = new RelayCommand(param => this.ExportGPXAll(),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null);
                }
                return _exportGPXAllCommand;
            }
        }
        private void ExportGPXAll()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                ExportGPX(Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection);
            }
        }
        private void ExportGPX(List<Core.Data.Geocache> gcList)
        {
            GPX.ExportWindow dlg = new GPX.ExportWindow(gcList);
            dlg.ShowDialog();
        }


        AsyncDelegateCommand _exportKMLActiveCommand;
        public ICommand ExportKMLActiveCommand
        {
            get
            {
                if (_exportKMLActiveCommand == null)
                {
                    _exportKMLActiveCommand = new AsyncDelegateCommand(param => this.ExportKMLActive(),
                        param => Core.ApplicationData.Instance.ActiveGeocache != null);
                }
                return _exportKMLActiveCommand;
            }
        }
        async private Task ExportKMLActive()
        {
            if (Core.ApplicationData.Instance.ActiveGeocache != null)
            {
                await ExportKML(new Core.Data.Geocache[] { Core.ApplicationData.Instance.ActiveGeocache }.ToList());
            }
        }
        AsyncDelegateCommand _exportKMLSelectedCommand;
        public ICommand ExportKMLSelectedCommand
        {
            get
            {
                if (_exportKMLSelectedCommand == null)
                {
                    _exportKMLSelectedCommand = new AsyncDelegateCommand(param => this.ExportKMLSelected(),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null && this.GeocacheSelectionCount>0);
                }
                return _exportKMLSelectedCommand;
            }
        }
        async private Task ExportKMLSelected()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                await ExportKML((from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Selected select a).ToList());
            }
        }
        AsyncDelegateCommand _exportKMLAllCommand;
        public ICommand ExportKMLAllCommand
        {
            get
            {
                if (_exportKMLAllCommand == null)
                {
                    _exportKMLAllCommand = new AsyncDelegateCommand(param => this.ExportKMLAll(),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null);
                }
                return _exportKMLAllCommand;
            }
        }
        async private Task ExportKMLAll()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                await ExportKML(Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection);
            }
        }
        async private Task ExportKML(List<Core.Data.Geocache> gcList)
        {
            KML.Export p = new KML.Export();
            await p.PerformExportAsync(gcList);
        }


        AsyncDelegateCommand _importGSAKCommand;
        public ICommand ImportGSAKCommand
        {
            get
            {
                if (_importGSAKCommand == null)
                {
                    _importGSAKCommand = new AsyncDelegateCommand(param => this.ImportGSAKDatabase(),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null);
                }
                return _importGSAKCommand;
            }
        }

        async private Task ImportGSAKDatabase()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.FileName = ""; // Default file name
                dlg.DefaultExt = ".db3"; // Default file extension
                dlg.Filter = "GSAK database (sqlite.db3)|sqlite.db3"; // Filter files by extension 

                // Show open file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process open file dialog box results 
                if (result == true)
                {
                    // Open document 
                    await GSAK.Importer.PerformAction(Core.ApplicationData.Instance.ActiveDatabase, dlg.FileName);
                }
            }
        }


        AsyncDelegateCommand _importGappCommand;
        public ICommand ImportGAPPCommand
        {
            get
            {
                if (_importGappCommand == null)
                {
                    _importGappCommand = new AsyncDelegateCommand(param => this.ImportGAPPDatabase(),
                        param => Core.ApplicationData.Instance.ActiveDatabase!=null);
                }
                return _importGappCommand;
            }
        }

        async private Task ImportGAPPDatabase()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.FileName = ""; // Default file name
                dlg.DefaultExt = ".gpp"; // Default file extension
                dlg.Filter = "GAPP Data Storage (.gpp)|*.gpp"; // Filter files by extension 

                // Show open file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process open file dialog box results 
                if (result == true)
                {
                    // Open document 
                    await GAPPDataStorage.Importer.PerformAction(Core.ApplicationData.Instance.ActiveDatabase, dlg.FileName);
                }
            }
        }

        AsyncDelegateCommand _newCommand;
        public ICommand NewCommand
        {
            get
            {
                if (_newCommand == null)
                {
                    _newCommand = new AsyncDelegateCommand(param => this.NewDatabase());
                }
                return _newCommand;
            }
        }

        async private Task NewDatabase()
        {
            try
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = ""; // Default file name
                dlg.DefaultExt = ".gsf"; // Default file extension
                dlg.Filter = "GAPP SF (.gsf)|*.gsf"; // Filter files by extension 

                // Show open file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process open file dialog box results 
                if (result == true)
                {
                    // Open document 
                    string filename = dlg.FileName;
                    if (File.Exists(filename))
                    {
                        File.Delete(filename);
                    }
                    Core.Storage.Database db = new Core.Storage.Database(filename);
                    bool success = await db.InitializeAsync();
                    if (success)
                    {
                        Core.ApplicationData.Instance.Databases.Add(db);
                        Core.ApplicationData.Instance.ActiveDatabase = db;
                    }
                    else
                    {
                        db.Dispose();
                    }
                }
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
        }

        RelayCommand _liveApiAuthorizeCommand;
        public ICommand LiveApiAuthorizeCommand
        {
            get
            {
                if (_liveApiAuthorizeCommand == null)
                {
                    _liveApiAuthorizeCommand = new RelayCommand(param => this.LiveAPIAuthorize());
                }
                return _liveApiAuthorizeCommand;
            }
        }
        public void LiveAPIAuthorize()
        {
            LiveAPI.GeocachingLiveV6.Authorize(true);
        }


        AsyncDelegateCommand _openCommand;
        public ICommand OpenCommand
        {
            get
            {
                if (_openCommand == null)
                {
                    _openCommand = new AsyncDelegateCommand(param => this.OpenDatabase());
                }
                return _openCommand;
            }
        }

        async private Task OpenDatabase()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".gsf"; // Default file extension
            dlg.Filter = "GAPP SF (.gsf)|*.gsf"; // Filter files by extension 

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                Core.Storage.Database db = new Core.Storage.Database(filename);
                bool success = await db.InitializeAsync();
                if (success)
                {
                    Core.ApplicationData.Instance.Databases.Add(db);
                    Core.ApplicationData.Instance.ActiveDatabase = db;
                }
                else
                {
                    db.Dispose();
                }
            }
        }


        private void MenuItem_Click_10(object sender, RoutedEventArgs e)
        {
            Window w = new FeatureWindow(new UIControls.ApplicationDataInfo());
            w.Owner = this;
            w.Show();
        }



        ForAllGeocachesCommand _selectWithUserWPCommand;
        public ICommand SelectWithUserWPCommand
        {
            get
            {
                if (_selectWithUserWPCommand == null)
                {
                    _selectWithUserWPCommand = new ForAllGeocachesCommand(param => this.selectWithUserWP(param));
                }
                return _selectWithUserWPCommand;
            }
        }
        private void selectWithUserWP(Core.Data.Geocache gc)
        {
           gc.Selected = Utils.DataAccess.GetUserWaypointsFromGeocache(gc.Database,gc.Code).Count() > 0;
        }


        ForAllGeocachesCommand _selectMultipleFoundsCommand;
        public ICommand SelectMultipleFoundsCommand
        {
            get
            {
                if (_selectMultipleFoundsCommand == null)
                {
                    _selectMultipleFoundsCommand = new ForAllGeocachesCommand(param => this.selectMultipleFounds(param));
                }
                return _selectMultipleFoundsCommand;
            }
        }
        private void selectMultipleFounds(Core.Data.Geocache gc)
        {
            if (gc.Found)
            {
                Core.Data.AccountInfo ai = Core.ApplicationData.Instance.AccountInfos.GetAccountInfo(gc.Code.Substring(0, 2));
                if (ai != null)
                {
                    string me = ai.AccountName;
                    gc.Selected = (from a in Utils.DataAccess.GetLogs(gc.Database, gc.Code) where a.LogType.AsFound && a.Finder == me select a).Count() > 1;
                }
            }
            else
            {
                gc.Selected = false;
            }
        }


        ForAllGeocachesCommand _selectOwnCommand;
        public ICommand SelectOwnCommand
        {
            get
            {
                if (_selectOwnCommand == null)
                {
                    _selectOwnCommand = new ForAllGeocachesCommand(param => this.selectOwn(param));
                }
                return _selectOwnCommand;
            }
        }
        private void selectOwn(Core.Data.Geocache gc)
        {
            gc.Selected = gc.IsOwn;
        }


        ForAllGeocachesCommand _selectCorrectCoordsCommand;
        public ICommand SelectCorrectCoordsCommand
        {
            get
            {
                if (_selectCorrectCoordsCommand == null)
                {
                    _selectCorrectCoordsCommand = new ForAllGeocachesCommand(param => this.selectCorrectCoords(param));
                }
                return _selectCorrectCoordsCommand;
            }
        }
        private void selectCorrectCoords(Core.Data.Geocache gc)
        {
            gc.Selected = gc.ContainsCustomLatLon;
        }



        ForAllGeocachesCommand _selectNotesCommand;
        public ICommand SelectNotesCommand
        {
            get
            {
                if (_selectNotesCommand == null)
                {
                    _selectNotesCommand = new ForAllGeocachesCommand(param => this.selectNotes(param));
                }
                return _selectNotesCommand;
            }
        }
        private void selectNotes(Core.Data.Geocache gc)
        {
            gc.Selected = gc.ContainsNote;
        }


        ForAllGeocachesCommand _selectFlaggedCommand;
        public ICommand SelectFlaggedCommand
        {
            get
            {
                if (_selectFlaggedCommand == null)
                {
                    _selectFlaggedCommand = new ForAllGeocachesCommand(param => this.selectFlagged(param));
                }
                return _selectFlaggedCommand;
            }
        }
        private void selectFlagged(Core.Data.Geocache gc)
        {
            gc.Selected = gc.Flagged;
        }

        ForAllGeocachesCommand _selectAvailableCommand;
        public ICommand SelectAvailableCommand
        {
            get
            {
                if (_selectAvailableCommand == null)
                {
                    _selectAvailableCommand = new ForAllGeocachesCommand(param => this.selectAvailable(param));
                }
                return _selectAvailableCommand;
            }
        }
        private void selectAvailable(Core.Data.Geocache gc)
        {
            gc.Selected = gc.Available;
        }

        ForAllGeocachesCommand _selectFoundCommand;
        public ICommand SelectFoundCommand
        {
            get
            {
                if (_selectFoundCommand == null)
                {
                    _selectFoundCommand = new ForAllGeocachesCommand(param => this.selectFound(param));
                }
                return _selectFoundCommand;
            }
        }
        private void selectFound(Core.Data.Geocache gc)
        {
            gc.Selected = gc.Found;
        }

        ForAllGeocachesCommand _selectNotFoundCommand;
        public ICommand SelectNotFoundCommand
        {
            get
            {
                if (_selectNotFoundCommand == null)
                {
                    _selectNotFoundCommand = new ForAllGeocachesCommand(param => this.selectNotFound(param));
                }
                return _selectNotFoundCommand;
            }
        }
        private void selectNotFound(Core.Data.Geocache gc)
        {
            gc.Selected = !gc.Found;
        }


        ForAllGeocachesCommand _selectInvertCommand;
        public ICommand SelectInvertCommand
        {
            get
            {
                if (_selectInvertCommand == null)
                {
                    _selectInvertCommand = new ForAllGeocachesCommand(param => this.selectInvert(param));
                }
                return _selectInvertCommand;
            }
        }
        private void selectInvert(Core.Data.Geocache gc)
        {
            gc.Selected = !gc.Selected;
        }

        ForAllGeocachesCommand _selectArchivedCommand;
        public ICommand SelectArchivedCommand
        {
            get
            {
                if (_selectArchivedCommand == null)
                {
                    _selectArchivedCommand = new ForAllGeocachesCommand(param => this.selectIfArchived(param));
                }
                return _selectArchivedCommand;
            }
        }
        private void selectIfArchived(Core.Data.Geocache gc)
        {
            gc.Selected = gc.Archived;
        }

        ForAllGeocachesCommand _selectAllCommand;
        public ICommand SelectAllCommand
        {
            get
            {
                if (_selectAllCommand == null)
                {
                    _selectAllCommand = new ForAllGeocachesCommand(param => this.selectAll(param));
                }
                return _selectAllCommand;
            }
        }
        private void selectAll(Core.Data.Geocache gc)
        {
            gc.Selected = true;
        }

        ForAllGeocachesCommand _selectNoneCommand;
        public ICommand SelectNoneCommand
        {
            get
            {
                if (_selectNoneCommand == null)
                {
                    _selectNoneCommand = new ForAllGeocachesCommand(param => this.selectNone(param));
                }
                return _selectNoneCommand;
            }
        }
        private void selectNone(Core.Data.Geocache gc)
        {
            gc.Selected = false;
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Core.ApplicationData.Instance.UIIsIdle)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var w in this.OwnedWindows)
                {
                    if (w is FeatureWindow)
                    {
                        sb.AppendLine((w as FeatureWindow).featureContainer.FeatureControl.GetType().ToString());
                    }
                }
                Core.Settings.Default.MainWindowWindowFeature = sb.ToString();
            }
            else
            {
                e.Cancel = true;
            }
        }


        private void MenuItem_Click_7(object sender, RoutedEventArgs e)
        {
            Window w = new FeatureWindow(new UIControls.GMap.GoogleMap());
            w.Owner = this;
            w.Show();
        }

        private void MenuItem_Click_8(object sender, RoutedEventArgs e)
        {
            Shapefiles.SettingsWindow dlg = new Shapefiles.SettingsWindow();
            dlg.ShowDialog();
        }

        private void MenuItem_Click_9(object sender, RoutedEventArgs e)
        {
            Window w = new FeatureWindow(new UIControls.OfflineImages.Control());
            w.Owner = this;
            w.Show();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(async () => await initializeApplicationAsync()), DispatcherPriority.ContextIdle, null);
        }

        private void TextBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Core.ApplicationData.Instance.ActiveGeocache!=null && !string.IsNullOrEmpty(Core.ApplicationData.Instance.ActiveGeocache.Url))
            {
                System.Diagnostics.Process.Start(Core.ApplicationData.Instance.ActiveGeocache.Url);
            }
        }

        private void MenuItem_Click_11(object sender, RoutedEventArgs e)
        {
            Window w = new FeatureWindow(new UIControls.GeocacheFilter.Control());
            w.Owner = this;
            w.Show();
        }

        private void MenuItem_Click_12(object sender, RoutedEventArgs e)
        {
            Dialogs.ShortCutAssignmentWindow dlg = new Dialogs.ShortCutAssignmentWindow();
            dlg.ShowDialog();
        }

        private void menu27_Click(object sender, RoutedEventArgs e)
        {
            Window w = new FeatureWindow(new UIControls.IgnoreGeocaches.Control());
            w.Owner = this;
            w.Show();
        }

        private void menua27_Click(object sender, RoutedEventArgs e)
        {
            Window w = new FeatureWindow(new UIControls.DebugLogView.Control());
            w.Owner = this;
            w.Show();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = false;
        }

        private void menub27_Click(object sender, RoutedEventArgs e)
        {
            Window w = new FeatureWindow(new UIControls.ActionBuilder.Control());
            w.Owner = this;
            w.Show();
        }

        private void menud27_Click(object sender, RoutedEventArgs e)
        {
            Dialogs.ActionSequenceWindow dlg = new Dialogs.ActionSequenceWindow();
            dlg.ShowDialog();
        }

        private void menud37_Click(object sender, RoutedEventArgs e)
        {
            Dialogs.GCComBookmarkWindow dlg = new Dialogs.GCComBookmarkWindow();
            dlg.ShowDialog();
        }

        private void menux27_Click(object sender, RoutedEventArgs e)
        {
            Dialogs.AboutWindow dlg = new Dialogs.AboutWindow(this);
            dlg.ShowDialog();
        }

        private void menuexit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
