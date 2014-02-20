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

        private string _followActiveGeocacheText = "";
        public string FollowActiveGeocacheText
        {
            get { return _followActiveGeocacheText; }
            set { SetProperty(ref _followActiveGeocacheText, value); }
        }

        private GAPPSF.Chat.UserInRoomInfo _selectedUser;
        public GAPPSF.Chat.UserInRoomInfo SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                SetProperty(ref _selectedUser, value);
            }
        }

        private GAPPSF.Chat.RoomInfo _selectedRoom;
        public GAPPSF.Chat.RoomInfo SelectedRoom
        {
            get { return _selectedRoom; }
            set
            {
                SetProperty(ref _selectedRoom, value);
                if (_selectedRoom!=null)
                {
                    txtroom.Text = _selectedRoom.Name ?? "";
                }
            }
        }

        private bool _chatEnabled;
        public bool ChatEnabled
        {
            get { return _chatEnabled; }
            set { SetProperty(ref _chatEnabled, value); }
        }

        public Control()
        {
            InitializeComponent();

            colorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(Core.Settings.Default.ChatMessageColor);

            StatusBarConnectionText = Localization.TranslationManager.Instance.Translate(GAPPSF.Chat.Manager.Instance.ChatConnectionStatus.ToString()) as string;
            GAPPSF.Chat.Manager.Instance.PropertyChanged += Instance_PropertyChanged;
            Core.Settings.Default.PropertyChanged += Default_PropertyChanged;
            GAPPSF.Chat.Manager.Instance.NewTextMessage += Instance_NewTextMessage;
            GAPPSF.Chat.Manager.Instance.UserInfoUpdate += Instance_UserInfoUpdate;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                GAPPSF.Chat.Manager.Instance.Attach();
                _attached = true;
            }));

            ChatEnabled = GAPPSF.Chat.Manager.Instance.ChatConnectionStatus == GAPPSF.Chat.Manager.ConnectionStatus.SignedIn;

            DataContext = this;
        }

        void Instance_UserInfoUpdate(object sender, GAPPSF.Chat.UserInRoomInfo usr)
        {
            if (usr.FollowThisUser)
            {
                if (!string.IsNullOrEmpty(usr.ActiveGeocache))
                {
                    if (Core.ApplicationData.Instance.ActiveDatabase != null)
                    {
                        Core.Data.Geocache gc = Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection.GetGeocache(usr.ActiveGeocache);
                        if (gc != null)
                        {
                            Core.ApplicationData.Instance.ActiveGeocache = gc;
                            FollowActiveGeocacheText = gc.Code;
                        }
                        else if (usr.ActiveGeocache.StartsWith("GC") && Core.Settings.Default.LiveAPIMemberTypeId > 0)
                        {
                            //offer to download
                            FollowActiveGeocacheText = string.Format("{0} ({1})", usr.ActiveGeocache, Localization.TranslationManager.Instance.Translate("Missing") as string);
                        }
                        else
                        {
                            FollowActiveGeocacheText = gc.Code;
                        }
                    }
                }
                else
                {
                    FollowActiveGeocacheText = "";
                }
            }
        }

        void Default_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ChatMessageColor")
            {
                colorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(Core.Settings.Default.ChatMessageColor);
            }            
        }

        void Instance_NewTextMessage(object sender, string fromid, string msg, System.Drawing.Color col)
        {
            GAPPSF.Chat.UserInRoomInfo usr = (from a in GAPPSF.Chat.Manager.Instance.UsersInRoomList where a.ID == fromid select a).FirstOrDefault();

            AddMessage(string.Format("{0} [{1}]:", DateTime.Now.ToString("t"), usr == null ? "" : usr.Username), System.Drawing.Color.Black);
            AddMessage(msg, col);

            txtBox.AppendText("\r");
            txtBox.ScrollToEnd();            
        }

        void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ChatConnectionStatus")
            {
                StatusBarConnectionText = Localization.TranslationManager.Instance.Translate(GAPPSF.Chat.Manager.Instance.ChatConnectionStatus.ToString()) as string;
                ChatEnabled = GAPPSF.Chat.Manager.Instance.ChatConnectionStatus == GAPPSF.Chat.Manager.ConnectionStatus.SignedIn;
            }
            else if (e.PropertyName == "Room")
            {
            }
        }

        public void Dispose()
        {
            Core.Settings.Default.PropertyChanged -= Default_PropertyChanged;
            GAPPSF.Chat.Manager.Instance.NewTextMessage -= Instance_NewTextMessage;
            GAPPSF.Chat.Manager.Instance.PropertyChanged -= Instance_PropertyChanged;
            GAPPSF.Chat.Manager.Instance.UserInfoUpdate -= Instance_UserInfoUpdate;
            if (_attached)
            {
                GAPPSF.Chat.Manager.Instance.Detach();
                _attached = false;
            }
        }


        private void AddMessage(string msg, System.Drawing.Color col)
        {
            //InsertImage(Properties.Resources.wink);

            TextRange tr = new TextRange(txtBox.Document.ContentEnd, txtBox.Document.ContentEnd);
            tr.Text = msg;
            var b = new SolidColorBrush(Color.FromArgb(col.A, col.R, col.G, col.B));
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, b);
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

        private void colorPicker_ColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            Core.Settings.Default.ChatMessageColor = colorPicker.SelectedColor.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            txtBox.Document.Blocks.Clear();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                bool sent = GAPPSF.Chat.Manager.Instance.BroadcastTextMessage(txtEntry.Text);
                if (sent)
                {
                    txtEntry.Text = "";
                    e.Handled = true;
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (SelectedUser != null)
            {
                if (SelectedUser.FollowThisUser)
                {
                    SelectedUser.FollowThisUser = false;
                }
                else if (SelectedUser.CanBeFollowed)
                {
                    SelectedUser.FollowThisUser = true;
                }
                SelectedUser.UpdateText();
                if (SelectedUser.FollowThisUser)
                {
                    Instance_UserInfoUpdate(null, SelectedUser);
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtroom.Text.Trim()))
            {
                GAPPSF.Chat.Manager.Instance.Room = txtroom.Text.Trim();
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (SelectedUser != null && SelectedUser.CanBeFollowed && SelectedUser.SelectionCount>0)
            {
                GAPPSF.Chat.Manager.Instance.RequestCopySelection(SelectedUser);
            }
        }

        private async void TextBlock_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(FollowActiveGeocacheText) && Core.ApplicationData.Instance.ActiveDatabase!=null)
            {
                string gcCode = FollowActiveGeocacheText.Split(new char[] { ' ' })[0];
                var agc = Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection.GetGeocache(gcCode);
                if (agc == null)
                {
                    if (gcCode.StartsWith("GC"))
                    {
                        using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
                        {
                            await Task.Run(() =>
                            {
                                LiveAPI.Import.ImportGeocaches(Core.ApplicationData.Instance.ActiveDatabase, new string[] { gcCode }.ToList());
                            });
                        }
                        var gc = Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection.GetGeocache(gcCode);
                        if (gc != null)
                        {
                            Core.ApplicationData.Instance.ActiveGeocache = gc;
                        }
                    }
                }
                else
                {
                    Core.ApplicationData.Instance.ActiveGeocache = agc;
                }
            }
        }

    }
}
