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
    /// Interaction logic for ShortCutAssignmentWindow.xaml
    /// </summary>
    public partial class ShortCutAssignmentWindow : Window
    {
        public class ShortCutInfo
        {
            public static string[] AllowedKeys =
            {
                "None",
                "1",
                "2",
                "3",
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                "0",
                "F1",
                "F2",
                "F3",
                "F4",
                "F5",
                "F6",
                "F7",
                "F8",
                "F9",
                "F10",
                "F11",
                "F12",
                "A",
                "B",
                "C",
                "D",
                "E",
                "F",
                "G",
                "H",
                "I",
                "J",
                "K",
                "L",
                "M",
                "N",
                "O",
                "P",
                "Q",
                "R",
                "S",
                "T",
                "U",
                "V",
                "W",
                "X",
                "Y",
                "Z",
                "Escape",
                "Enter",
            };

            public ShortCutInfo(string mnuName)
            {
                MenuName = mnuName;
                ShortKey = "None";
                ShortKeyOption = AllowedKeys;
            }

            public bool Shift { get; set; }
            public bool Control { get; set; }
            public bool Alt { get; set; }
            public bool Windows { get; set; }
            public string MenuName { get; private set; }
            public string ShortKey { get; set; }
            public string[] ShortKeyOption { get; set; }
        }

        private List<ShortCutInfo> _allMenuItems;

        public ShortCutAssignmentWindow()
        {
            InitializeComponent();

            _allMenuItems = new List<ShortCutInfo>();
            if (!string.IsNullOrEmpty(Core.Settings.Default.MainWindowShortCutKeyAssignment))
            {
                string[] lines = Core.Settings.Default.MainWindowShortCutKeyAssignment.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string l in lines)
                {
                    //Shift|Control|Alt|Windows|MenuName|Key
                    string[] parts = l.Split(new char[] { '|' }, 6);
                    if (parts.Length == 6)
                    {
                        ShortCutInfo sci = new ShortCutInfo(parts[5]);
                        sci.Shift = bool.Parse(parts[0]);
                        sci.Control = bool.Parse(parts[1]);
                        sci.Alt = bool.Parse(parts[2]);
                        sci.Windows = bool.Parse(parts[3]);
                        sci.ShortKey = parts[4];
                    }
                }
            }

            Menu mainMenu = Core.ApplicationData.Instance.MainWindow.mainMenu;
            foreach (var c in mainMenu.Items)
            {
                TreeViewItem tvi = new TreeViewItem();
                MenuItem m = c as MenuItem;
                if (m != null)
                {
                    ShortCutInfo sci = (from a in _allMenuItems where a.MenuName == (m.Header as string) select a).FirstOrDefault();
                    if (sci == null)
                    {
                        sci = new ShortCutInfo(m.Header as string);
                    }
                    tvi.Tag = sci;
                    tvi.Header = m.Header;
                    treeView.Items.Add(tvi);

                    if (m.Items.Count > 0)
                    {
                        AddBranche(tvi, m);
                    }
                }
            }
        }

        private void AddBranche(TreeViewItem tvi, MenuItem mnu)
        {
            foreach (var c in mnu.Items)
            {
                TreeViewItem stvi = new TreeViewItem();
                MenuItem m = c as MenuItem;
                if (m != null)
                {
                    ShortCutInfo sci = (from a in _allMenuItems where a.MenuName == (m.Header as string) select a).FirstOrDefault();
                    if (sci == null)
                    {
                        sci = new ShortCutInfo(m.Header as string);
                    }
                    stvi.Tag = sci;
                    stvi.Header = m.Header;
                    tvi.Items.Add(stvi);

                    if (m.Items.Count > 0)
                    {
                        AddBranche(stvi, m);
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Shift|Control|Alt|Windows|MenuName|Key
            StringBuilder sb = new StringBuilder();
            foreach (var sci in _allMenuItems)
            {
                if (!string.IsNullOrEmpty(sci.ShortKey) && sci.ShortKey!="None")
                {
                    sb.AppendLine(string.Format("{0}|{1}|{2}|{3}|{4}|{5}", sci.Shift, sci.Control,sci.Alt,sci.Windows,sci.MenuName,sci.ShortKey));
                }
            }
            Core.Settings.Default.MainWindowShortCutKeyAssignment = sb.ToString();
            DialogResult = true;
            Close();
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ShortCutInfo sci = null;
            if (treeView.SelectedItem!=null)
            {
                TreeViewItem tvi = treeView.SelectedItem as TreeViewItem;
                if (tvi != null)
                {
                    sci = tvi.Tag as ShortCutInfo;
                }
            }
            DataContext = sci;
        }
    }
}
