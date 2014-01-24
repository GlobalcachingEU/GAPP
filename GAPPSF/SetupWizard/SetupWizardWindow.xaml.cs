using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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

namespace GAPPSF.SetupWizard
{
    /// <summary>
    /// Interaction logic for SetupWizardWindow.xaml
    /// </summary>
    public partial class SetupWizardWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public SetupWizardWindow()
        {
            DataContext = this;

            InitializeComponent();

            string l = Localization.TranslationManager.Instance.CurrentLanguage.TwoLetterISOLanguageName.ToLower();
            if (l=="en")
            {
                languageSelectCombo.SelectedIndex = 0;
            }
            else if (l == "de")
            {
                languageSelectCombo.SelectedIndex = 1;
            }
            else if (l == "nl")
            {
                languageSelectCombo.SelectedIndex = 2;
            }
            else if (l == "fr")
            {
                languageSelectCombo.SelectedIndex = 3;
            }
            else
            {
                languageSelectCombo.SelectedIndex = 0;
            }

            Core.ApplicationData.Instance.AccountInfos.CollectionChanged += AccountInfos_CollectionChanged;
            Core.Settings.Default.PropertyChanged += Default_PropertyChanged;
        }

        void Default_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Authorized = string.IsNullOrEmpty(Core.Settings.Default.LiveAPIToken) ? Localization.TranslationManager.Instance.Translate("No").ToString() : Localization.TranslationManager.Instance.Translate("Yes").ToString();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Core.Settings.Default.PropertyChanged -= Default_PropertyChanged;
            Core.ApplicationData.Instance.AccountInfos.CollectionChanged -= AccountInfos_CollectionChanged;
        }

        void AccountInfos_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            GeocachingAcccountName = Core.ApplicationData.Instance.AccountInfos.GetAccountInfo("GC").AccountName;
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

        private string _geocachingComAccountName = Core.ApplicationData.Instance.AccountInfos.GetAccountInfo("GC").AccountName;
        public string GeocachingAcccountName
        {
            get { return _geocachingComAccountName; }
            set
            {
                if (Core.ApplicationData.Instance.AccountInfos.GetAccountInfo("GC").AccountName != value)
                {
                    Core.ApplicationData.Instance.AccountInfos.GetAccountInfo("GC").AccountName = value;
                }
                SetProperty(ref _geocachingComAccountName, value);
            }
        }

        private string _authorized = string.IsNullOrEmpty(Core.Settings.Default.LiveAPIToken) ? Localization.TranslationManager.Instance.Translate("No").ToString() : Localization.TranslationManager.Instance.Translate("Yes").ToString();
        public string Authorized
        {
            get { return _authorized; }
            set
            {
                SetProperty(ref _authorized, value);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (languageSelectCombo.SelectedIndex)
            {
                case 0:
                    Localization.TranslationManager.Instance.CurrentLanguage = new CultureInfo("en");
                    break;
                case 1:
                    Localization.TranslationManager.Instance.CurrentLanguage = new CultureInfo("de");
                    break;
                case 2:
                    Localization.TranslationManager.Instance.CurrentLanguage = new CultureInfo("nl");
                    break;
                case 3:
                    Localization.TranslationManager.Instance.CurrentLanguage = new CultureInfo("fr");
                    break;
                default:
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Utils.DataAccess.SetCenterLocation(Core.ApplicationData.Instance.HomeLocation.Lat, Core.ApplicationData.Instance.HomeLocation.Lon);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (tabControl.SelectedIndex<tabControl.Items.Count-1)
            {
                tabControl.SelectedIndex++;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (tabControl.SelectedIndex > 0)
            {
                tabControl.SelectedIndex--;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Dialogs.GetLocationWindow dlg = new Dialogs.GetLocationWindow(Core.ApplicationData.Instance.HomeLocation);
            if (dlg.ShowDialog()==true)
            {
                Core.ApplicationData.Instance.HomeLocation.Lat = dlg.Location.Lat;
                Core.ApplicationData.Instance.HomeLocation.Lon = dlg.Location.Lon;
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Dialogs.GetLocationWindow dlg = new Dialogs.GetLocationWindow(Core.ApplicationData.Instance.CenterLocation);
            if (dlg.ShowDialog() == true)
            {
                Utils.DataAccess.SetCenterLocation(dlg.Location.Lat, dlg.Location.Lon);
            }
        }

        private void Button_Click_Finish(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            backButton.IsEnabled = tabControl.SelectedIndex > 0;
            nextButton.IsEnabled = tabControl.SelectedIndex < tabControl.Items.Count-1;
            if (nextButton.IsEnabled)
            {
                nextButton.Visibility = System.Windows.Visibility.Visible;
                finishButton.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                nextButton.Visibility = System.Windows.Visibility.Collapsed;
                finishButton.Visibility = System.Windows.Visibility.Visible;
            }
        }

    }
}
