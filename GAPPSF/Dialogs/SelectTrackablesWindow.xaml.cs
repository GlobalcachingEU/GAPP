using GAPPSF.UIControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace GAPPSF.Dialogs
{
    /// <summary>
    /// Interaction logic for SelectTrackablesWindow.xaml
    /// </summary>
    public partial class SelectTrackablesWindow : Window
    {
        public ObservableCollection<CheckedListItem<LiveAPI.LiveV6.Trackable>> Trackables { get; set; }

        public List<LiveAPI.LiveV6.Trackable> SelectedTrackables
        {
            get
            {
                List<LiveAPI.LiveV6.Trackable> result = new List<LiveAPI.LiveV6.Trackable>();
                foreach(var t in Trackables)
                {
                    if (t.IsChecked)
                    {
                        result.Add(t.Item);
                    }
                }
                return result;
            }
        }

        public SelectTrackablesWindow()
        {
            InitializeComponent();
        }

        public SelectTrackablesWindow(List<LiveAPI.LiveV6.Trackable> tbList)
        {
            Trackables = new ObservableCollection<CheckedListItem<LiveAPI.LiveV6.Trackable>>();
            foreach (var tb in tbList)
            {
                Trackables.Add(new CheckedListItem<LiveAPI.LiveV6.Trackable>(tb, false));
            }
            InitializeComponent();
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
