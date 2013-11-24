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
    /// Interaction logic for SelectionContext.xaml
    /// </summary>
    public partial class SelectionContext : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public enum Context
        {
            NewSelection,
            WithinSelection,
            AddToSelection
        }
        public SelectionContext()
        {
            InitializeComponent();

            SelectedContext = Context.NewSelection;
            DataContext = this;
        }

        public Context _selectedContext = Context.NewSelection;
        public Context SelectedContext 
        {
            get { return _selectedContext; }
            set { SetProperty(ref _selectedContext, value); }
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

    }
}
