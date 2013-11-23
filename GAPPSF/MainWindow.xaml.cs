using GAPPSF.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace GAPPSF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.DataContext = this;

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
                await GAPPDataStorage.Importer.PerformAction(Core.ApplicationData.Instance.Databases[0], dlg.FileName);
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
                Core.Storage.Database db = new Core.Storage.Database(filename);
                await db.InitializeAsync();
                Core.ApplicationData.Instance.Databases.Add(db);
                Core.ApplicationData.Instance.ActiveDatabase = db;
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
                await db.InitializeAsync();
                Core.ApplicationData.Instance.Databases.Add(db);
                Core.ApplicationData.Instance.ActiveDatabase = db;
            }
        }


        private void MenuItem_Click_10(object sender, RoutedEventArgs e)
        {
            Window w = new FeatureWindow(new UIControls.ApplicationDataInfo());
            w.Owner = this;
            w.Show();
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


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var w in this.OwnedWindows)
            {
                if (w is FeatureWindow)
                {
                    sb.AppendLine((w as FeatureWindow).featureContainer.FeatureControl.GetType().ToString());
                }
            }
            Core.Settings.Default.MainWindowWindowFeature = sb.ToString();
        }


        public static T FindChild<T>(DependencyObject parent, string childName)
           where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        private void MenuItem_Click_7(object sender, RoutedEventArgs e)
        {
            Window w = new FeatureWindow(new UIControls.GMap.GoogleMap());
            w.Owner = this;
            w.Show();
        }
    }
}
