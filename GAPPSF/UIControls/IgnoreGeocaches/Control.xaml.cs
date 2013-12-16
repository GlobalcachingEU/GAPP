using GAPPSF.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace GAPPSF.UIControls.IgnoreGeocaches
{
    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl, IUIControl, IDisposable
    {
        public List<IgnoreCategory> IgnoreCategories { get; private set; }
        public Control()
        {
            InitializeComponent();

            IgnoreCategories = new List<IgnoreCategory>();
            IgnoreCategories.Add(new IgnoredGeocacheCodes());
            IgnoreCategories.Add(new IgnoredGeocacheOwners());
            IgnoreCategories.Add(new IgnoredGeocacheNames());
            DataContext = this;

            Core.Settings.Default.PropertyChanged += Default_PropertyChanged;
            Localization.TranslationManager.Instance.LanguageChanged += Instance_LanguageChanged;
        }

        void Instance_LanguageChanged(object sender, EventArgs e)
        {
            foreach (var c in IgnoreCategories)
            {
                c.UpdateText();
            }
        }

        public override string ToString()
        {
            return "IgnoreGeocaches";
        }

        void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName=="IgnoreGeocachesUpdateCounter")
            {
                foreach(var c in IgnoreCategories)
                {
                    c.UpdateItems();
                }
            }
        }

        public void Dispose()
        {
            Localization.TranslationManager.Instance.LanguageChanged -= Instance_LanguageChanged;
            Core.Settings.Default.PropertyChanged -= Default_PropertyChanged;
        }

        private RelayCommand _addItemCommand = null;
        public RelayCommand AddItemCommand
        {
            get
            {
                if (_addItemCommand==null)
                {
                    _addItemCommand = new RelayCommand(param => AddItem(), param => itemEdit.Text.Trim().Length > 0);
                }
                return _addItemCommand;
            }
        }
        public void AddItem()
        {
            IgnoreCategory cat = catCombo.SelectedItem as IgnoreCategory;
            if (cat!=null && itemEdit.Text.Trim().Length > 0)
            {
                cat.AddItem(itemEdit.Text.Trim());
            }
        }

        private RelayCommand _removeItemCommand = null;
        public RelayCommand RemoveItemCommand
        {
            get
            {
                if (_removeItemCommand == null)
                {
                    _removeItemCommand = new RelayCommand(param => RemoveItem(), param => itemList.SelectedItems.Count > 0);
                }
                return _removeItemCommand;
            }
        }
        public void RemoveItem()
        {
            IgnoreCategory cat = catCombo.SelectedItem as IgnoreCategory;
            if (cat != null && itemList.SelectedItems.Count > 0)
            {
                cat.RemoveItems((from string a in itemList.SelectedItems select a).ToList());
            }
        }

        public int WindowWidth
        {
            get
            {
                return Core.Settings.Default.IgnoreGeocachesWindowWidth;
            }
            set
            {
                Core.Settings.Default.IgnoreGeocachesWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.IgnoreGeocachesWindowHeight;
            }
            set
            {
                Core.Settings.Default.IgnoreGeocachesWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.IgnoreGeocachesWindowLeft;
            }
            set
            {
                Core.Settings.Default.IgnoreGeocachesWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.IgnoreGeocachesWindowTop;
            }
            set
            {
                Core.Settings.Default.IgnoreGeocachesWindowTop = value;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Core.Settings.Default.ClearGeocacheIgnoreFilters();
        }


    }
}
