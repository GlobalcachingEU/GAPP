using System;
using System.Collections.Generic;
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

namespace GAPPSF.UIControls.NotesEditor
{
    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl, IDisposable, IUIControl, INotifyPropertyChanged
    {
        private Core.Data.Geocache _currentGeocache = null;

        public event PropertyChangedEventHandler PropertyChanged;
        private string _notesText;
        public string NotesText
        {
            get 
            { 
                //get notes from control
                _notesText = editor.mainRTB.Tag as string;
                //strip head/body tags
                if (!string.IsNullOrEmpty(_notesText))
                {
                    string s = Utils.Conversion.StripHtmlTags(_notesText);
                    if (s.Trim().Length > 0)
                    {
                        if (_notesText.Length > 26 && _notesText.StartsWith("<HTML><BODY>"))
                        {
                            _notesText = _notesText.Substring(12, _notesText.Length - 26);
                        }
                        if (_notesText == "<DIV />")
                        {
                            _notesText = "";
                        }
                    }
                    else
                    {
                        _notesText = "";
                    }
                }
                return _notesText; 
            }
            set 
            { 
                SetProperty(ref _notesText, value);
                //add head/body tags
                string html = string.Format("<HTML><BODY>{0}</BODY></HTML>", _notesText ?? "");
                //set control text
                editor.Text = "";
                editor.mainRTB.Tag = html;
                editor.Text = html;
            }
        }

        public Control()
        {
            InitializeComponent();
            DataContext = this;
            Core.ApplicationData.Instance.PropertyChanged += Instance_PropertyChanged;

            UpdateView();
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
            saveNotes();
            Core.ApplicationData.Instance.PropertyChanged -= Instance_PropertyChanged;
        }

        private void UpdateView()
        {
            saveNotes();
            _currentGeocache = Core.ApplicationData.Instance.ActiveGeocache;
            if (_currentGeocache!=null)
            {
                NotesText = _currentGeocache.Notes;
            }
            else
            {
                NotesText = null;
            }
        }

        private void saveNotes()
        {
            if (_currentGeocache != null)
            {
                _currentGeocache.Notes = NotesText;
            }
        }

        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate("NotesEditor") as string;
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
                return Core.Settings.Default.NotesEditorWindowWidth;
            }
            set
            {
                Core.Settings.Default.NotesEditorWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.NotesEditorWindowHeight;
            }
            set
            {
                Core.Settings.Default.NotesEditorWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.NotesEditorWindowLeft;
            }
            set
            {
                Core.Settings.Default.NotesEditorWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.NotesEditorWindowTop;
            }
            set
            {
                Core.Settings.Default.NotesEditorWindowTop = value;
            }
        }

    }
}
