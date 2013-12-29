using GAPPSF.Core;
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
using System.Windows.Shapes;

namespace GAPPSF.Dialogs
{
    /// <summary>
    /// Interaction logic for ProgessWindow.xaml
    /// </summary>
    public partial class ProgessWindow : Window
    {
        private static ProgessWindow _uniqueInstance = null;

        public class ProgressData : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

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

            private string _title = "Progress";
            public string Title
            {
                get { return _title; }
                set { SetProperty(ref _title, value); }
            }

            private string _mainAction = "Main action";
            public string MainAction
            {
                get { return _mainAction; }
                set { SetProperty(ref _mainAction, value); }
            }

            private string _subAction = "Sub action";
            public string SubAction
            {
                get { return _subAction; }
                set { SetProperty(ref _subAction, value); }
            }

            private int _mainMax = 100;
            public int MainMax
            {
                get { return _mainMax; }
                set { SetProperty(ref _mainMax, value); }
            }

            private int _mainValue = 100;
            public int MainValue
            {
                get { return _mainValue; }
                set { SetProperty(ref _mainValue, value); }
            }

            private int _subMax = 100;
            public int SubMax
            {
                get { return _subMax; }
                set { SetProperty(ref _subMax, value); }
            }

            private int _subValue = 100;
            public int SubValue
            {
                get { return _subValue; }
                set { SetProperty(ref _subValue, value); }
            }

            private Visibility _subVisible = Visibility.Hidden;
            public Visibility SubVisible
            {
                get { return _subVisible; }
                set { SetProperty(ref _subVisible, value); }
            }

            private Visibility _cancelButton = Visibility.Hidden;
            public Visibility CancelButton
            {
                get { return _cancelButton; }
                set { SetProperty(ref _cancelButton, value); }
            }

        }
        private ProgressData _progressData;

        public bool Canceled { get; set; }

        private ProgessWindow()
        {
            InitializeComponent();

            _progressData = new ProgressData();
            this.DataContext = _progressData;
        }

        public static ProgessWindow Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    _uniqueInstance = new ProgessWindow();
                }
                return _uniqueInstance;
            }
        }

        public void Start(string title, string mainAction, int max, int pos)
        {
            Start(title, mainAction, max, pos, false);
        }
        public void Start(string title, string mainAction, int max, int pos, bool canCancel)
        {
            Canceled = false;
            if (!CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action<string, string, int, int, bool>(Start), new object[] { title, mainAction, max, pos, canCancel });
                //Dispatcher.Invoke(new Action<string, string, int, int, bool>(Start), new object[] { title, mainAction, max, pos, canCancel });
                return;
            }
            _progressData.Title = title;
            _progressData.MainAction = mainAction;
            _progressData.MainMax = max;
            _progressData.MainValue = pos;
            _progressData.SubVisible = System.Windows.Visibility.Hidden;
            if (canCancel)
            {
                _progressData.CancelButton = System.Windows.Visibility.Visible;
            }
            else
            {
                _progressData.CancelButton = System.Windows.Visibility.Hidden;
            }
            this.Visibility = System.Windows.Visibility.Visible;
            this.Owner = ApplicationData.Instance.MainWindow;
        }

        public void Stop()
        {
            if (!CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(Stop), new object[]{});
                //Dispatcher.Invoke(new Action(Stop), new object[] { });
                return;
            }
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        public void ChangeProgress(string mainAction, int value, int max)
        {
            if (!CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action<string, int, int>(ChangeProgress), new object[] { mainAction, value, max });
                //Dispatcher.Invoke(new Action<string, int, int>(ChangeProgress), new object[] { mainAction, value, max });
                return;
            }
            _progressData.MainAction = mainAction;
            _progressData.MainMax = max;
            _progressData.MainValue = value;
        }

        public void ChangeProgressSub(string subAction, int value, int max)
        {
            if (!CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action<string, int, int>(ChangeProgressSub), new object[] { subAction, value, max });
                //Dispatcher.Invoke(new Action<string, int, int>(ChangeProgressSub), new object[] { subAction, value, max });
                return;
            }
            _progressData.SubAction = subAction;
            _progressData.SubMax = max;
            _progressData.SubValue = value;
        }


        public void StartSub(string subAction, int max, int pos)
        {
            if (!CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action<string, int, int>(StartSub), new object[] { subAction, max, pos });
                //Dispatcher.Invoke(new Action<string, int, int>(StartSub), new object[] { subAction, max, pos });
                return;
            }
            Canceled = false;
            _progressData.SubAction = subAction;
            _progressData.SubMax = max;
            _progressData.SubValue = pos;
            _progressData.SubVisible = System.Windows.Visibility.Visible;
        }

        public void StopSub()
        {
            if (!CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(StopSub), new object[] { });
                //Dispatcher.Invoke(new Action(StopSub), new object[] { });
                return;
            }
            _progressData.SubVisible = System.Windows.Visibility.Visible;
        }


        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (Core.ApplicationData.Instance.UIIsIdle)
            {
                Canceled = true;
                base.OnClosing(e);
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Canceled = true;
        }
    }
}
