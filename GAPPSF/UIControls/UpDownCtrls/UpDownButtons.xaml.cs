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

namespace GAPPSF.UIControls
{
    /// <summary>
    /// Interaction logic for UpDownButtons.xaml
    /// </summary>
    public partial class UpDownButtons : UserControl
    {
        public static readonly RoutedEvent UpClickEvent = EventManager.RegisterRoutedEvent("UpClick",
                                     RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UpDownButtons));

        public event RoutedEventHandler UpClick
        {
            add { AddHandler(UpClickEvent, value); }
            remove { RemoveHandler(UpClickEvent, value); }
        }

        public static readonly RoutedEvent DownClickEvent = EventManager.RegisterRoutedEvent("DownClick",
                             RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UpDownButtons));

        public event RoutedEventHandler DownClick
        {
            add { AddHandler(DownClickEvent, value); }
            remove { RemoveHandler(DownClickEvent, value); }
        }
        public UpDownButtons()
        {
            InitializeComponent();
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            RoutedEventArgs UpClickEventArgs = new RoutedEventArgs(UpClickEvent);
            RaiseEvent(UpClickEventArgs);
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            RoutedEventArgs DownClickEventArgs = new RoutedEventArgs(DownClickEvent);
            RaiseEvent(DownClickEventArgs);
        }
    }
}