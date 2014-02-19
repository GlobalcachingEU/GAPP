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

namespace GAPPSF.UIControls.Chat
{
    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl, IUIControl, INotifyPropertyChanged, IDisposable
    {
        private bool _attached = false;
        public event PropertyChangedEventHandler PropertyChanged;

        private string _statusBarConnectionText;
        public string StatusBarConnectionText
        {
            get { return _statusBarConnectionText; }
            set { SetProperty(ref _statusBarConnectionText, value); }
        }

        public Control()
        {
            InitializeComponent();

            StatusBarConnectionText = Localization.TranslationManager.Instance.Translate(GAPPSF.Chat.Manager.Instance.ChatConnectionStatus.ToString()) as string;
            GAPPSF.Chat.Manager.Instance.PropertyChanged += Instance_PropertyChanged;
            GAPPSF.Chat.Manager.Instance.NewTextMessage += Instance_NewTextMessage;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                GAPPSF.Chat.Manager.Instance.Attach();
                _attached = true;
            }));

            DataContext = this;
        }

        void Instance_NewTextMessage(object sender, string fromid, string msg, System.Drawing.Color col)
        {
            GAPPSF.Chat.UserInRoomInfo usr = (from a in GAPPSF.Chat.Manager.Instance.UsersInRoomList where a.ID == fromid select a).FirstOrDefault();

            AddMessage(string.Format("{0} [{1}]:", DateTime.Now.ToString("t"), usr == null ? "" : usr.Username), System.Drawing.Color.Black);
            AddMessage(msg, col);

            txtBox.AppendText(Environment.NewLine);
            //txtBox.Selection.Start = txtBox.Document.Text.Length;
            //txtBox.ScrollToCaret();            
        }

        void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ChatConnectionStatus")
            {
                StatusBarConnectionText = Localization.TranslationManager.Instance.Translate(GAPPSF.Chat.Manager.Instance.ChatConnectionStatus.ToString()) as string;
            }
            else if (e.PropertyName == "Room")
            {
            }
        }

        public void Dispose()
        {
            GAPPSF.Chat.Manager.Instance.NewTextMessage -= Instance_NewTextMessage;
            GAPPSF.Chat.Manager.Instance.PropertyChanged -= Instance_PropertyChanged;
            if (_attached)
            {
                GAPPSF.Chat.Manager.Instance.Detach();
                _attached = false;
            }
        }


        private void AddMessage(string msg, System.Drawing.Color col)
        {
            //InsertImage(Properties.Resources.wink);
            /*
            txtBox.SelectionStart = txtBox.TextLength;
            txtBox.SelectionLength = 0;

            txtBox.SelectionColor = col;
            txtBox.AppendText(msg);
            txtBox.SelectionColor = txtBox.ForeColor;
             * */
        }

        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate("Chat") as string;
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

        public int WindowWidth
        {
            get
            {
                return Core.Settings.Default.ChatWindowWidth;
            }
            set
            {
                Core.Settings.Default.ChatWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.ChatWindowHeight;
            }
            set
            {
                Core.Settings.Default.ChatWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.ChatWindowLeft;
            }
            set
            {
                Core.Settings.Default.ChatWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.ChatWindowTop;
            }
            set
            {
                Core.Settings.Default.ChatWindowTop = value;
            }
        }

    }
}
