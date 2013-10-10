using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;

namespace GlobalcachingApplication.Plugins.LogView
{
    /// <summary>
    /// Interaction logic for LogViewControl.xaml
    /// </summary>
    public partial class LogViewControl : UserControl
    {
        public LogViewControl()
        {
            InitializeComponent();
        }
    }

    public class PathConverter : IValueConverter
    {
        public PathConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Framework.Data.LogType gct = value as Framework.Data.LogType;
            if (gct != null)
            {
                return Utils.ImageSupport.Instance.GetImagePath(LogViewerForm.FixedCore, Framework.Data.ImageSize.Small, gct);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

}
