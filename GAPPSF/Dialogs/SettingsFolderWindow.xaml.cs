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
    /// Interaction logic for SettingsFolderWindow.xaml
    /// </summary>
    public partial class SettingsFolderWindow : Window
    {
        public string SelectedSettingsPath { get; private set; }

        public SettingsFolderWindow()
        {
            InitializeComponent();
        }
    }
}
