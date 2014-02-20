using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GAPPSF.Chat
{
    public class Manager : INotifyPropertyChanged
    {
        public enum ConnectionStatus
        {
            NotConnected,
            Connecting,
            Connected,
            SigningIn,
            SignedIn
        }
        
        public delegate void TextMessageEventHandler(object sender, string fromid, string msg, Color col);
        public delegate void UserInfoUpdateEventHandler(object sender, UserInRoomInfo usr);

        private static Manager _uniqueInstance = null;
        private static object _lockObject = new object();
        private ServerConnection _serverConnection = null;
        private int _refCount = 0;
        private SynchronizationContext _context = null;
        private System.Media.SoundPlayer _sndWelcome = null;
        private System.Media.SoundPlayer _sndMessage = null;
        private System.Windows.Threading.DispatcherTimer retryTimer = null;
        private int _retryCount = 0;
        private string _currentCopySelectionRequestID = null;

        private Core.Storage.Database _copySelectionDb = null;
        private List<string> _importGeocaches = new List<string>();
        private Utils.DataUpdater _dataUpdater = null;
        private Thread _getGeocacheThread = null;

        private string _id = "";
        public string ID { get { return _id; } }

        private string _room = "";
        public string Room 
        {
            get { return _room; }
            set
            {
                if (_room != value)
                {
                    SetProperty(ref _room, value);
                    if (_serverConnection != null && _connectionStatus == ConnectionStatus.SignedIn)
                    {
                        ChatMessage msg = new ChatMessage();
                        msg.Name = "room";
                        SendMessage(msg);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event TextMessageEventHandler NewTextMessage;
        public event UserInfoUpdateEventHandler UserInfoUpdate;

        public ObservableCollection<UserInRoomInfo> UsersInRoomList { get; set; }
        public ObservableCollection<RoomInfo> RoomList { get; set; }

        private ConnectionStatus _connectionStatus = ConnectionStatus.NotConnected;
        public ConnectionStatus ChatConnectionStatus
        {
            get { return _connectionStatus; }
            set { SetProperty(ref _connectionStatus, value); }
        }

        private bool _iCanBeFollowed = false;
        public bool ICanBeFollowed
        {
            get { return _iCanBeFollowed; }
            set
            {
                if (_iCanBeFollowed!=value)
                {
                    SetProperty(ref _iCanBeFollowed, value);
                    if (_serverConnection!=null &&  _connectionStatus== ConnectionStatus.SignedIn)
                    {
                        ChatMessage msg = new ChatMessage();
                        msg.Name = "follow";
                        msg.Parameters.Add("canfollow", _iCanBeFollowed.ToString());
                        msg.Parameters.Add("cache", _iCanBeFollowed ? Core.ApplicationData.Instance.ActiveGeocache == null ? "" : Core.ApplicationData.Instance.ActiveGeocache.Code : "");
                        msg.Parameters.Add("selected", _iCanBeFollowed ? Core.ApplicationData.Instance.MainWindow.GeocacheSelectionCount.ToString() : "");
                        if (!SendMessage(msg))
                        {
                            ICanBeFollowed = false;
                        }
                    }
                    else
                    {
                        ICanBeFollowed = false;
                    }
                }
            }
        }

        private Manager()
        {
            _id = Guid.NewGuid().ToString("N");
            _room = "Lobby";
            UsersInRoomList = new ObservableCollection<UserInRoomInfo>();
            RoomList = new ObservableCollection<RoomInfo>();

            Core.ApplicationData.Instance.MainWindow.PropertyChanged += MainWindow_PropertyChanged;
            Core.ApplicationData.Instance.PropertyChanged += Instance_PropertyChanged;

            try
            {
                Uri u = Utils.ResourceHelper.GetResourceUri("/Resources/General/welcome.wav");
                System.Windows.Resources.StreamResourceInfo info = System.Windows.Application.GetResourceStream(u);
                _sndWelcome = new System.Media.SoundPlayer(info.Stream);
                u = Utils.ResourceHelper.GetResourceUri("/Resources/General/message.wav");
                info = System.Windows.Application.GetResourceStream(u);
                _sndMessage = new System.Media.SoundPlayer(info.Stream);
            }
            catch (Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }

            _context = SynchronizationContext.Current;
            if (_context == null)
            {
                _context = new SynchronizationContext();
            }
        }

        void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName=="ActiveGeocache")
            {
                if (_serverConnection != null && _connectionStatus == ConnectionStatus.SignedIn && ICanBeFollowed)
                {
                    if (ICanBeFollowed)
                    {
                        ChatMessage msg = new ChatMessage();
                        msg.Name = "follow";
                        msg.Parameters.Add("canfollow", _iCanBeFollowed.ToString());
                        msg.Parameters.Add("cache", _iCanBeFollowed ? Core.ApplicationData.Instance.ActiveGeocache == null ? "" : Core.ApplicationData.Instance.ActiveGeocache.Code : "");
                        msg.Parameters.Add("selected", _iCanBeFollowed ? Core.ApplicationData.Instance.MainWindow.GeocacheSelectionCount.ToString() : "");
                        SendMessage(msg);
                    }
                }
            }
        }

        void MainWindow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "GeocacheSelectionCount")
            {
                if (_serverConnection != null && _connectionStatus == ConnectionStatus.SignedIn && ICanBeFollowed)
                {
                    ChatMessage msg = new ChatMessage();
                    msg.Name = "follow";
                    msg.Parameters.Add("canfollow", _iCanBeFollowed.ToString());
                    msg.Parameters.Add("cache", _iCanBeFollowed ? Core.ApplicationData.Instance.ActiveGeocache == null ? "" : Core.ApplicationData.Instance.ActiveGeocache.Code : "");
                    msg.Parameters.Add("selected", _iCanBeFollowed ? Core.ApplicationData.Instance.MainWindow.GeocacheSelectionCount.ToString() : "");
                    SendMessage(msg);
                }
            }
        }

        public static Manager Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            _uniqueInstance = new Manager();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        public void Attach()
        {
            _refCount++;
            if (_refCount==1)
            {
                _retryCount = 0;
            }
            if (_serverConnection==null)
            {
                openConnection();
            }
        }
        public void Detach()
        {
            _refCount--;
            if (_refCount<=0)
            {
                if (retryTimer!=null)
                {
                    retryTimer.IsEnabled = false;
                }
                _refCount = 0;
                closeConnection();
            }
        }

        public bool BroadcastTextMessage(string txt)
        {
            bool result = false;
            if (_serverConnection != null && _connectionStatus == ConnectionStatus.SignedIn)
            {
                string s = txt.Trim();
                if (s.Length > 0)
                {
                    ChatMessage msg = new ChatMessage();
                    msg.Name = "txt";
                    msg.Parameters.Add("msg", s);
                    System.Windows.Media.Color c = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(Core.Settings.Default.ChatMessageColor);
                    Color col = Color.FromArgb(c.A, c.R, c.G, c.B);
                    msg.Parameters.Add("color", col.ToArgb().ToString());
                    result = SendMessage(msg);
                }
            }
            return result;
        }

        public void RequestCopySelection(UserInRoomInfo usr)
        {
            if (_serverConnection != null && _connectionStatus == ConnectionStatus.SignedIn && usr.CanBeFollowed && usr.SelectionCount > 0)
            {
                //send request
                _currentCopySelectionRequestID = Guid.NewGuid().ToString("N");

                ChatMessage msg = new ChatMessage();
                msg.Name = "reqsel";
                msg.Parameters.Add("reqid", _currentCopySelectionRequestID);
                msg.Parameters.Add("clientid", usr.ID);
                SendMessage(msg);
            }
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


        private bool SendMessage(ChatMessage msg)
        {
            bool result = false;
            msg.ID = _id;
            msg.Room = _room;
            try
            {
                if (_serverConnection != null)
                {
                    _serverConnection.SendData(msg.ChatMessageData);
                    result = true;
                }
            }
            catch
            {
                closeConnection();
            }
            return result;
        }

        private void checkRetry()
        {
            if (_serverConnection == null && _refCount > 0 && _retryCount<3)
            {
                if (retryTimer == null)
                {
                    retryTimer = new System.Windows.Threading.DispatcherTimer();
                    retryTimer.Interval = new TimeSpan(0, 0, 2);
                    retryTimer.Tick += retryTimer_Tick;
                }
                retryTimer.IsEnabled = false;
                retryTimer.IsEnabled = true;
            }
        }

        void retryTimer_Tick(object sender, EventArgs e)
        {
            retryTimer.IsEnabled = false;
            _context.Post(new SendOrPostCallback(delegate(object state)
            {
                if (_refCount > 0)
                {
                    _retryCount++;
                    openConnection();
                }
            }), null);            
        }

        private void _serverConnection_ConnectionClosed(object sender, EventArgs e)
        {
            _context.Post(new SendOrPostCallback(delegate(object state)
            {
                _serverConnection = null;
                ChatConnectionStatus = ConnectionStatus.NotConnected;
                checkRetry();
            }), null);
        }

        void _serverConnection_DataReceived(object sender, EventArgs e)
        {
            _context.Post(new SendOrPostCallback(delegate(object state)
            {
                try
                {
                    if (_serverConnection != null)
                    {
                        byte[] data = _serverConnection.ReadData();
                        while (data != null)
                        {
                            //parse data
                            ChatMessage msg = ChatMessage.Parse(data);
                            if (msg != null)
                            {
                                //handle data
                                if (msg.Name == "signinfailed")
                                {
                                    closeConnection();
                                    break;
                                }
                                else if (msg.Name == "signinsuccess")
                                {
                                    _retryCount = 0;
                                    ChatConnectionStatus = ConnectionStatus.SignedIn;
                                }
                                else if (msg.Name == "txt")
                                {
                                    if (NewTextMessage!=null)
                                    {
                                        NewTextMessage(this, msg.ID, msg.Parameters["msg"], Color.FromArgb(int.Parse(msg.Parameters["color"])));
                                    }
                                    if (Core.Settings.Default.ChatPlaySounds && _sndMessage != null)
                                    {
                                        _sndMessage.Play();
                                    }
                                }
                                else if (msg.Name == "usersinroom")
                                {
                                    bool newUser = false;
                                    int i = 0;
                                    foreach (var x in UsersInRoomList)
                                    {
                                        x.present = false;
                                    }
                                    bool wasempty = UsersInRoomList.Count == 0;
                                    while (msg.Parameters.ContainsKey(string.Format("name{0}", i)))
                                    {
                                        string id = msg.Parameters[string.Format("id{0}", i)];
                                        UserInRoomInfo c = (from x in UsersInRoomList where x.ID == id select x).FirstOrDefault();
                                        if (c == null)
                                        {
                                            c = new UserInRoomInfo();
                                            c.ID = id;
                                            c.Username = msg.Parameters[string.Format("name{0}", i)];
                                            c.present = true;
                                            c.FollowThisUser = false;
                                            c.SelectionCount = -1;
                                            if (msg.Parameters.ContainsKey(string.Format("sc{0}", i)))
                                            {
                                                string selCount = msg.Parameters[string.Format("sc{0}", i)];
                                                if (!string.IsNullOrEmpty(selCount))
                                                {
                                                    c.SelectionCount = int.Parse(selCount);
                                                }
                                            }
                                            c.CanBeFollowed = bool.Parse(msg.Parameters[string.Format("cbf{0}", i)]);
                                            c.ActiveGeocache = msg.Parameters[string.Format("agc{0}", i)];
                                            UsersInRoomList.Add(c);
                                            if (!wasempty)
                                            {
                                                if (NewTextMessage != null)
                                                {
                                                    NewTextMessage(this, "-1", string.Format("{0} {1}", c.Username, Localization.TranslationManager.Instance.Translate("JoinedTheRoom") as string), Color.Black);
                                                }
                                            }
                                            newUser = true;
                                        }
                                        else
                                        {
                                            bool cbf = c.CanBeFollowed;
                                            string agc = c.ActiveGeocache;
                                            c.CanBeFollowed = bool.Parse(msg.Parameters[string.Format("cbf{0}", i)]);
                                            c.ActiveGeocache = msg.Parameters[string.Format("agc{0}", i)];
                                            c.present = true;
                                            c.UpdateText();
                                            if (cbf!=c.CanBeFollowed || agc!=c.ActiveGeocache)
                                            {
                                                if (UserInfoUpdate != null)
                                                {
                                                    UserInfoUpdate(this, c);
                                                }
                                            }
                                        }
                                        i++;
                                    }
                                    var rmList = from a in UsersInRoomList where !a.present select a;
                                    foreach (var x in rmList)
                                    {
                                        UsersInRoomList.Remove(x);
                                    }

                                    if (newUser && Core.Settings.Default.ChatPlaySounds && _sndWelcome != null)
                                    {
                                        _sndWelcome.Play();
                                    }
                                }
                                else if (msg.Name == "follow")
                                {
                                    UserInRoomInfo[] curUsr = UsersInRoomList.ToArray();
                                    UserInRoomInfo c = (from x in curUsr where x.ID == msg.ID select x).FirstOrDefault();
                                    if (c != null)
                                    {
                                        c.CanBeFollowed = bool.Parse(msg.Parameters["canfollow"]);
                                        c.ActiveGeocache = msg.Parameters["cache"];
                                        c.SelectionCount = -1;
                                        if (msg.Parameters.ContainsKey("selected"))
                                        {
                                            string selCount = msg.Parameters["selected"];
                                            if (!string.IsNullOrEmpty(selCount))
                                            {
                                                c.SelectionCount = int.Parse(selCount);
                                            }
                                        }

                                        if (c.FollowThisUser)
                                        {
                                            if (!c.CanBeFollowed)
                                            {
                                                c.FollowThisUser = false;
                                            }
                                        }
                                        c.UpdateText();
                                        if (UserInfoUpdate != null)
                                        {
                                            UserInfoUpdate(this, c);
                                        }
                                    }
                                }
                                else if (msg.Name == "rooms")
                                {
                                    RoomInfo[] curRms = RoomList.ToArray();
                                    foreach (var x in curRms)
                                    {
                                        x.present = false;
                                    }
                                    int i = 0;
                                    while (msg.Parameters.ContainsKey(string.Format("name{0}", i)))
                                    {
                                        string name = msg.Parameters[string.Format("name{0}", i)];
                                        RoomInfo c = (from x in curRms where x.Name == name select x).FirstOrDefault();
                                        if (c == null)
                                        {
                                            c = new RoomInfo();
                                            c.present = true;
                                            c.Name = name;
                                            RoomList.Add(c);
                                        }
                                        else
                                        {
                                            c.present = true;
                                        }
                                        i++;
                                    }
                                    var rmList = from a in RoomList where !a.present select a;
                                    foreach (var x in rmList)
                                    {
                                        RoomList.Remove(x);
                                    }
                                }
                                else if (msg.Name == "reqsel")
                                {
                                    StringBuilder sb = new StringBuilder();
                                    UserInRoomInfo[] curUsr = UsersInRoomList.ToArray();
                                    UserInRoomInfo c = (from x in curUsr where x.ID == msg.ID select x).FirstOrDefault();
                                    if (c != null)
                                    {
                                        if (NewTextMessage != null)
                                        {
                                            NewTextMessage(this, "-1", string.Format("{0} {1}", c.Username, Localization.TranslationManager.Instance.Translate("RequestedSelection") as string), Color.Black);
                                        }
                                        if (Core.ApplicationData.Instance.ActiveDatabase != null)
                                        {
                                            var gsel = from Core.Data.Geocache g in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where g.Selected select g;
                                            foreach (var g in gsel)
                                            {
                                                sb.AppendFormat("{0},", g.Code);
                                            }
                                        }
                                    }

                                    ChatMessage bmsg = new ChatMessage();
                                    bmsg.Name = "reqselresp";
                                    bmsg.Parameters.Add("reqid", msg.Parameters["reqid"]);
                                    bmsg.Parameters.Add("clientid", msg.ID);
                                    bmsg.Parameters.Add("selection", sb.ToString());

                                    SendMessage(bmsg);
                                }
                                else if (msg.Name == "reqselresp")
                                {
                                    if (msg.Parameters["reqid"] == _currentCopySelectionRequestID && _getGeocacheThread==null)
                                    {
                                        //handle response
                                        Core.Storage.Database db = Core.ApplicationData.Instance.ActiveDatabase;
                                        if (db != null)
                                        {
                                            string[] gcCodes = msg.Parameters["selection"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                            using (Utils.DataUpdater upd = new Utils.DataUpdater(db))
                                            {
                                                foreach (var g in db.GeocacheCollection)
                                                {
                                                    g.Selected = gcCodes.Contains(g.Code);
                                                }
                                            }
                                            //are we missing geocaches?
                                            _importGeocaches.Clear();
                                            if (Core.Settings.Default.LiveAPIMemberTypeId > 0)
                                            {
                                                foreach (string s in gcCodes)
                                                {
                                                    if (db.GeocacheCollection.GetGeocache(s) == null && s.StartsWith("GC"))
                                                    {
                                                        _importGeocaches.Add(s);
                                                    }
                                                }
                                                if (_importGeocaches.Count > 0)
                                                {
                                                    _copySelectionDb = db;
                                                    _dataUpdater = new Utils.DataUpdater(db);
                                                    _getGeocacheThread = new Thread(new ThreadStart(getCopiedSelectionGeocacheInbackgroundMethod));
                                                    _getGeocacheThread.IsBackground = true;
                                                    _getGeocacheThread.Start();
                                                }
                                            }
                                        }
                                    }
                                    _currentCopySelectionRequestID = null;
                                }
                            }
                            data = _serverConnection.ReadData();
                        }
                    }
                }
                catch
                {
                    closeConnection();
                }
            }), null);
        }

        private void getCopiedSelectionGeocacheInbackgroundMethod()
        {
            try
            {
                LiveAPI.Import.ImportGeocaches(_copySelectionDb, _importGeocaches);
                foreach (string s in _importGeocaches)
                {
                    var gc = _copySelectionDb.GeocacheCollection.GetGeocache(s);
                    if (gc!=null)
                    {
                        gc.Selected = true;
                    }
                }
            }
            catch(Exception e) 
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
            _context.Post(new SendOrPostCallback(delegate(object state)
            {
                _dataUpdater.Dispose();
                _dataUpdater = null;
                _importGeocaches.Clear();
                _getGeocacheThread = null;
                _copySelectionDb = null;
            }), null);
        }

        private void openConnection()
        {
            if (_serverConnection == null)
            {
                if (!string.IsNullOrEmpty(Core.Settings.Default.LiveAPIToken))
                {
                    try
                    {
                        ChatConnectionStatus = ConnectionStatus.Connecting;

                        TcpClient tcpClient = new TcpClient();

                        //then intialize
                        _serverConnection = new ServerConnection(tcpClient);
                        _serverConnection.ConnectionClosed += new EventHandler<EventArgs>(_serverConnection_ConnectionClosed);
                        _serverConnection.DataReceived += new EventHandler<EventArgs>(_serverConnection_DataReceived);
                        _serverConnection.Start();
                        ChatConnectionStatus = ConnectionStatus.Connected;

                        ChatConnectionStatus = ConnectionStatus.SigningIn;
                        ChatMessage msg = new ChatMessage();
                        msg.ID = _id;
                        msg.Name = "signon";
                        msg.Room = _room;
                        msg.Parameters.Add("username", Core.ApplicationData.Instance.AccountInfos.GetAccountInfo("GC").AccountName ?? "");
                        msg.Parameters.Add("token", Core.Settings.Default.LiveAPIToken);
                        _serverConnection.SendData(msg.ChatMessageData);
                    }
                    catch
                    {
                        closeConnection();
                    }
                }
                else
                {
                    checkRetry();
                }
            }
        }

        private void closeConnection()
        {
            try
            {
                UsersInRoomList.Clear();
                RoomList.Clear();
                if (_serverConnection != null)
                {
                    _serverConnection.ConnectionClosed -= new EventHandler<EventArgs>(_serverConnection_ConnectionClosed);
                    _serverConnection.DataReceived -= new EventHandler<EventArgs>(_serverConnection_DataReceived);
                    _serverConnection.Dispose();
                    _serverConnection = null;
                }
                ChatConnectionStatus = ConnectionStatus.NotConnected;
                checkRetry();
            }
            catch
            {
                _serverConnection = null;
                ChatConnectionStatus = ConnectionStatus.NotConnected;
            }
        }

    }
}
