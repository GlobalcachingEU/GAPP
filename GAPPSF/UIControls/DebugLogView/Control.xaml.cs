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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GAPPSF.UIControls.DebugLogView
{
    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl, IUIControl, IDisposable
    {
        public List<Core.Logger.Level> LogLevels { get; set; }

        public Control()
        {
            LogLevels = (Enum.GetValues(typeof(Core.Logger.Level)) as Core.Logger.Level[]).ToList();

            InitializeComponent();

            DataContext = this;

            Core.ApplicationData.Instance.Logger.LogAdded += Logger_LogAdded;
        }

        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate("DebugLogView") as string;
        }

        void Logger_LogAdded(object sender, Core.Logger.LogEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (e.Level <= Core.Settings.Default.DebugLogViewLevel)
                {
                    logText.Text += string.Format("{0} [{1}] => {2}\r\n", DateTime.Now.ToString("HH:mm:ss:FFF"), e.ObjType, e.Message);
                    if (e.Exception != null && !string.IsNullOrEmpty(e.Exception.StackTrace))
                    {
                        logText.Text += e.Exception.StackTrace + "\r\n";
                    }
                }
            })); 
            
        }

        public void Dispose()
        {
            Core.ApplicationData.Instance.Logger.LogAdded -= Logger_LogAdded;
        }

        public int WindowWidth
        {
            get
            {
                return Core.Settings.Default.DebugLogViewWindowWidth;
            }
            set
            {
                Core.Settings.Default.DebugLogViewWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.DebugLogViewWindowHeight;
            }
            set
            {
                Core.Settings.Default.DebugLogViewWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.DebugLogViewWindowLeft;
            }
            set
            {
                Core.Settings.Default.DebugLogViewWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.DebugLogViewWindowTop;
            }
            set
            {
                Core.Settings.Default.DebugLogViewWindowTop = value;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            logText.Text = "";
        }

    }
}
