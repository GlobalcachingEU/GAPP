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
using System.Windows.Shapes;

namespace GAPPSF.Dialogs
{
    /// <summary>
    /// Interaction logic for SelectActionWindow.xaml
    /// </summary>
    public partial class SelectActionWindow : Window
    {
        public MenuItem SelectedMenuItem { get; set; }
        public string[] SelectedPath { get; set; }
        public string SelectedPathFormat { get; set; }

        public SelectActionWindow()
        {
            InitializeComponent();

            Menu mainMenu = Core.ApplicationData.Instance.MainWindow.mainMenu;
            foreach (var c in mainMenu.Items)
            {
                MenuItem m = c as MenuItem;
                if (m != null)
                {
                    TreeViewItem tvi = new TreeViewItem();

                    tvi.Tag = m;
                    tvi.Header = m.Header;
                    treeView.Items.Add(tvi);

                    if (m.Items.Count > 0)
                    {
                        tvi.IsExpanded = true;
                        AddBranche(tvi, m);
                    }
                }
            }

            okButton.IsEnabled = false;
        }

        private void AddBranche(TreeViewItem tvi, MenuItem mnu)
        {
            foreach (var c in mnu.Items)
            {
                MenuItem m = c as MenuItem;
                if (m != null)
                {
                    TreeViewItem stvi = new TreeViewItem();

                    stvi.Tag = m;
                    stvi.Header = m.Header;
                    tvi.Items.Add(stvi);

                    if (m.Items.Count > 0)
                    {
                        stvi.IsExpanded = true;
                        AddBranche(stvi, m);
                    }
                }
            }
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem tvi = treeView.SelectedItem as TreeViewItem;
            if (tvi != null)
            {
                MenuItem mi = tvi.Tag as MenuItem;
                okButton.IsEnabled = (mi != null && !string.IsNullOrEmpty(mi.Name));
            }
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = treeView.SelectedItem as TreeViewItem;
            if (tvi != null)
            {
                MenuItem mi = tvi.Tag as MenuItem;
                if (mi != null && !string.IsNullOrEmpty(mi.Name))
                {
                    SelectedMenuItem = mi;
                    List<string> p = new List<string>();
                    while(mi is MenuItem)
                    {
                        p.Add(mi.Header as string);
                        mi = mi.Parent as MenuItem;
                    }
                    p.Reverse();
                    SelectedPath = p.ToArray();
                    SelectedPathFormat = SelectedPath[0];
                    for (int i = 1; i < SelectedPath.Length; i++ )
                    {
                        SelectedPathFormat = string.Format("{0} -> {1}", SelectedPathFormat, SelectedPath[i]);
                    }

                    DialogResult = true;
                    Close();
                }
            }
        }

    }
}
