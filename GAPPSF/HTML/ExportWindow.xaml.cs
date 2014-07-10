using GAPPSF.Commands;
using GAPPSF.UIControls;
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
using System.Windows.Shapes;

namespace GAPPSF.HTML
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Sheet> Sheets { get; private set; }
        public ObservableCollection<CheckedListItem<PropertyItem>> Fields { get; private set; }

        private bool _initializingSheet = false;
        private Sheet _activeSheet;
        public Sheet ActiveSheet 
        {
            get { return _activeSheet; }
            set 
            {
                _initializingSheet = true;
                SetProperty(ref _activeSheet, value);
                if (_activeSheet==null)
                {
                    foreach(var f in Fields)
                    {
                        f.IsChecked = false;
                    }
                }
                else
                {
                    fillFields();
                }
                _initializingSheet = false;
                IsRemoveSheetEnabled = _activeSheet != null;
            } 
        }

        private CheckedListItem<PropertyItem> _selectedField;
        public CheckedListItem<PropertyItem> SelectedField
        {
            get { return _selectedField; }
            set 
            { 
                SetProperty(ref _selectedField, value);
                IsMoveUpEnabled = _selectedField != null && _selectedField.IsChecked && Fields.IndexOf(_selectedField) > 0;
                IsMoveDownEnabled = _selectedField != null && _selectedField.IsChecked && Fields.IndexOf(_selectedField) < Fields.Count - 1 && Fields[Fields.IndexOf(_selectedField)+1].IsChecked;
            }

        }

        private bool _isRemoveSheetEnabled = false;
        public bool IsRemoveSheetEnabled
        {
            get { return _isRemoveSheetEnabled; }
            set { SetProperty(ref _isRemoveSheetEnabled, value);}
        }
        private bool _isMoveUpEnabled = false;
        public bool IsMoveUpEnabled
        {
            get { return _isMoveUpEnabled; }
            set { SetProperty(ref _isMoveUpEnabled, value); }
        }
        private bool _isMoveDownEnabled = false;
        public bool IsMoveDownEnabled
        {
            get { return _isMoveDownEnabled; }
            set { SetProperty(ref _isMoveDownEnabled, value); }
        }

        private List<Core.Data.Geocache> _gcList;

        public ExportWindow()
        {
            InitializeComponent();
        }

        public ExportWindow(List<Core.Data.Geocache> gcList):this()
        {
            _gcList = gcList;

            if (PropertyItem.PropertyItems.Count==0)
            {
                //fill the available items
                PropertyItem ppi;
                ppi = new PropertyItemCode();
                ppi = new PropertyItemName();
                ppi = new PropertyItemPublished();
                ppi = new PropertyItemLat();
                ppi = new PropertyItemLon();
                ppi = new PropertyItemCoordinate();
                ppi = new PropertyItemAvailable();
                ppi = new PropertyItemArchived();
                ppi = new PropertyItemCountry();
                ppi = new PropertyItemState();
                ppi = new PropertyItemMunicipality();
                ppi = new PropertyItemCity();
                ppi = new PropertyItemType();
                ppi = new PropertyItemPlacedBy();
                ppi = new PropertyItemOwner();
                ppi = new PropertyItemContainer();
                ppi = new PropertyItemTerrain();
                ppi = new PropertyItemDifficulty();
                ppi = new PropertyItemDescriptionText();
                ppi = new PropertyItemDescriptionHTML();
                ppi = new PropertyItemUrl();
                ppi = new PropertyItemMemberOnly();
                ppi = new PropertyItemCustomLat();
                ppi = new PropertyItemCustomLon();
                ppi = new PropertyItemCustomCoordinate();
                ppi = new PropertyItemAutoCoordinate();
                ppi = new PropertyItemFavorites();
                ppi = new PropertyItemPersonalNotes();
                ppi = new PropertyItemNotes();
                ppi = new PropertyItemFlagged();
                ppi = new PropertyItemFound();
                ppi = new PropertyItemFoundDate();
                ppi = new PropertyItemHints();
                //ppi = new PropertyItemGCVote(core);
#if DEBUG
                ppi = new PropertyItemRDx();
                ppi = new PropertyItemRDy();
                ppi = new PropertyItemEnvelopAreaOther();
                ppi = new PropertyItemInAreaOther();
                ppi = new PropertyItemGlobalcachingUrl();
#endif
            }

            Sheets = new ObservableCollection<Sheet>();
            Fields = new ObservableCollection<CheckedListItem<PropertyItem>>();

            if (!string.IsNullOrEmpty(Core.Settings.Default.HTMLSheets))
            {
                string[] lines = Core.Settings.Default.HTMLSheets.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in lines)
                {
                    //fielscount
                    //fields
                    //name
                    string[] parts = s.Split(new char[] { '|' }, 2);
                    int cnt = int.Parse(parts[0]);
                    parts = s.Split(new char[] { '|' }, cnt + 2);
                    Sheet sheet = new Sheet();
                    sheet.Name = parts[parts.Length - 1];
                    for (int i = 1; i < parts.Length - 1; i++)
                    {
                        sheet.SelectedItems.Add((from p in PropertyItem.PropertyItems where p.GetType().ToString() == parts[i] select p).FirstOrDefault());
                    }
                    Sheets.Add(sheet);
                }
            }

            foreach (var fi in PropertyItem.PropertyItems)
            {
                CheckedListItem<PropertyItem> cli = new CheckedListItem<PropertyItem>(fi, false);
                Fields.Add(cli);
                cli.PropertyChanged += cli_PropertyChanged;
            }

            DataContext = this;
        }

        private void fillFields()
        {
            if (_activeSheet != null)
            {
                int insertAt = 0;
                foreach (var f in _activeSheet.SelectedItems)
                {
                    CheckedListItem<PropertyItem> fitem = (from a in Fields where a.Item == f select a).FirstOrDefault();
                    int index = Fields.IndexOf(fitem);
                    if (index>insertAt)
                    {
                        Fields.RemoveAt(index);
                        Fields.Insert(insertAt, fitem);
                        fitem.IsChecked = true;
                    }
                    else
                    {
                        fitem.IsChecked = true;
                    }
                    insertAt++;
                }
                for (int i = insertAt; i < Fields.Count; i++)
                {
                    Fields[i].IsChecked = false;
                }
            }
            else
            {
                for (int i = 0; i < Fields.Count; i++)
                {
                    Fields[i].IsChecked = false;
                }
            }
        }

        void cli_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_initializingSheet)
            {
                if (_activeSheet != null && sender is CheckedListItem<PropertyItem>)
                {
                    CheckedListItem<PropertyItem> pi = sender as CheckedListItem<PropertyItem>;
                    if (pi.IsChecked && !_activeSheet.SelectedItems.Contains(pi.Item))
                    {
                        _activeSheet.SelectedItems.Add(pi.Item);
                    }
                    else if (!pi.IsChecked && _activeSheet.SelectedItems.Contains(pi.Item))
                    {
                        _activeSheet.SelectedItems.Remove(pi.Item);
                    }
                }
                saveSheets();
                fillFields();
                SelectedField = sender as CheckedListItem<PropertyItem>;
            }
        }

        private void saveSheets()
        {
            StringBuilder sc = new StringBuilder();
            foreach (Sheet sheet in Sheets)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(sheet.SelectedItems.Count.ToString());
                foreach (PropertyItem pi in sheet.SelectedItems)
                {
                    sb.AppendFormat("|{0}", pi.GetType().ToString());
                }
                sb.AppendFormat("|{0}", sheet.Name);
                sc.AppendLine(sb.ToString());
            }
            Core.Settings.Default.HTMLSheets = sc.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Dialogs.FolderPickerDialog();

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                Core.Settings.Default.HTMLTargetPath = dlg.SelectedPath;
            }
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
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
                        Sheet sh = new Sheet();
                        sh.Name = s;
                        Sheets.Add(sh);
                        ActiveSheet = sh;
                        saveSheets();
                    }
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

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (ActiveSheet!=null)
            {
                Sheets.Remove(ActiveSheet);
                saveSheets();
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (ActiveSheet!=null && SelectedField != null && SelectedField.IsChecked && Fields.IndexOf(SelectedField) > 0)
            {
                var rmb = SelectedField;
                int index = Fields.IndexOf(rmb);
                Fields.RemoveAt(index);
                Fields.Insert(index - 1, rmb);
                SelectedField = rmb;

                ActiveSheet.SelectedItems.Clear();
                foreach (var fi in Fields)
                {
                    if (fi.IsChecked)
                    {
                        ActiveSheet.SelectedItems.Add(fi.Item);
                    }
                }
                saveSheets();
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (ActiveSheet!=null && SelectedField != null && SelectedField.IsChecked && Fields.IndexOf(SelectedField) < Fields.Count - 1 && Fields[Fields.IndexOf(SelectedField) + 1].IsChecked)
            {
                var rmb = SelectedField;
                int index = Fields.IndexOf(rmb);
                Fields.RemoveAt(index);
                Fields.Insert(index + 1, rmb);
                SelectedField = rmb;

                ActiveSheet.SelectedItems.Clear();
                foreach (var fi in Fields)
                {
                    if (fi.IsChecked)
                    {
                        ActiveSheet.SelectedItems.Add(fi.Item);
                    }
                }
                saveSheets();
            }

        }

        private AsyncDelegateCommand _exportHTMLCommand;
        public AsyncDelegateCommand ExportHTMLCommand
        {
            get
            {
                if (_exportHTMLCommand == null)
                {
                    _exportHTMLCommand = new AsyncDelegateCommand(param => ExportHTMLAsync(),
                        param => !string.IsNullOrEmpty(Core.Settings.Default.HTMLTargetPath));
                }
                return _exportHTMLCommand;
            }
        }
        public async Task ExportHTMLAsync()
        {
            Export exp = new Export();
            await exp.ExportToHTML(Core.Settings.Default.HTMLTargetPath, Sheets.ToList(), _gcList);
            Close();
        }

    }
}
