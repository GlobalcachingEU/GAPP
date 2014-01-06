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

namespace GAPPSF.UIControls
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler DialogClosed;

        public InputDialog()
        {
            InitializeComponent();

            DataContext = this;
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

        private bool _dialogResult;
        public bool DialogResult
        {
            get { return _dialogResult; }
            set { SetProperty(ref _dialogResult, value); }
        }

        private string _inputText;
        public string InputText
        {
            get { return _inputText ?? ""; }
            set { SetProperty(ref _inputText, value); }
        }

        private string _captionText;
        public string CaptionText
        {
            get { return _captionText; }
            set { SetProperty(ref _captionText, value); }
        }

        private Visibility _dialogVisibility = Visibility.Collapsed;
        public Visibility DialogVisibility
        {
            get { return _dialogVisibility; }
            set 
            { 
                SetProperty(ref _dialogVisibility, value);
                if (_dialogVisibility== System.Windows.Visibility.Collapsed)
                {
                    if (DialogClosed!=null)
                    {
                        DialogClosed(this, EventArgs.Empty);
                    }
                }
            }
        }

        public void Show(string captext)
        {
            CaptionText = captext;
            DialogResult = false;
            DialogVisibility = System.Windows.Visibility.Visible;
            FocusManager.SetFocusedElement(this, InputTextBox);
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogVisibility = System.Windows.Visibility.Collapsed;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            DialogVisibility = System.Windows.Visibility.Collapsed;
        }

        private void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key== Key.Enter)
            {
                FocusManager.SetFocusedElement(this, YesButton);
                YesButton_Click(this, null);
                e.Handled = true;
            }
            else if (e.Key== Key.Escape)
            {
                FocusManager.SetFocusedElement(this, NoButton);
                NoButton_Click(this, null);
                e.Handled = true;
            }
        }
    }
}
