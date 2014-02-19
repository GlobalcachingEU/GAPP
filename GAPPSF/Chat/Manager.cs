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

        private static Manager _uniqueInstance = null;
        private static object _lockObject = new object();
        private ServerConnection _serverConnection = null;
        private int _refCount = 0;
        private SynchronizationContext _context = null;

        private string _id = "";
        public string ID { get { return _id; } }

        private string _room = "";
        public string Room 
        { 
            get { return _id; }
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

        public ObservableCollection<UserInRoomInfo> UsersInRoomList { get; set; }
        public ObservableCollection<RoomInfo> RoomList { get; set; }

        private ConnectionStatus _connectionStatus = ConnectionStatus.NotConnected;
        public ConnectionStatus ChatConnectionStatus
        {
            get { return _connectionStatus; }
            set { SetProperty(ref _connectionStatus, value); }
        }

        private Manager()
        {
            _id = Guid.NewGuid().ToString("N");
            _room = "Lobby";
            UsersInRoomList = new ObservableCollection<UserInRoomInfo>();
            RoomList = new ObservableCollection<RoomInfo>();

            _context = SynchronizationContext.Current;
            if (_context == null)
            {
                _context = new SynchronizationContext();
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
                _refCount = 0;
                closeConnection();
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
            if (_serverConnection == null && _refCount > 0)
            {
                //todo
            }
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
                                    ChatConnectionStatus = ConnectionStatus.SignedIn;
                                }
                                else if (msg.Name == "txt")
                                {
                                    if (NewTextMessage!=null)
                                    {
                                        NewTextMessage(this, msg.ID, msg.Parameters["msg"], Color.FromArgb(int.Parse(msg.Parameters["color"])));
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
                                                //todo
                                                //AddMessage("-1", string.Format("{0} {1}", c.Username, Utils.LanguageSupport.Instance.GetTranslation(STR_JOINED)), Color.Black);
                                            }
                                            newUser = true;
                                        }
                                        else
                                        {
                                            c.CanBeFollowed = bool.Parse(msg.Parameters[string.Format("cbf{0}", i)]);
                                            c.ActiveGeocache = msg.Parameters[string.Format("agc{0}", i)];
                                            c.present = true;
                                        }
                                        i++;
                                    }
                                    var rmList = from a in UsersInRoomList where !a.present select a;
                                    foreach (var x in rmList)
                                    {
                                        UsersInRoomList.Remove(x);
                                    }

                                    //if (checkBoxPlaySound.Checked && newUser && _sndWelcome != null)
                                    //{
                                    //    _sndWelcome.Play();
                                        //System.Media.SystemSounds.Beep.Play();
                                    //}
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
                                            else
                                            {
                                                if (!string.IsNullOrEmpty(c.ActiveGeocache))
                                                {
                                                    if (Core.ApplicationData.Instance.ActiveDatabase != null)
                                                    {
                                                        Core.Data.Geocache gc = Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection.GetGeocache(c.ActiveGeocache);
                                                        if (gc != null)
                                                        {
                                                            Core.ApplicationData.Instance.ActiveGeocache = gc;
                                                        }
                                                        else if (c.ActiveGeocache.StartsWith("GC") && Core.Settings.Default.LiveAPIMemberTypeId > 0)
                                                        {
                                                            //offer to download
                                                        }
                                                        else
                                                        {
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                }
                                            }
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
                                        //AddMessage("-1", string.Format("{0} {1}", c.Username, Utils.LanguageSupport.Instance.GetTranslation(STR_REQUESTEDSELECTION)), Color.Black);
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
                                    /*
                                else if (msg.Name == "reqselresp")
                                {
                                    if (msg.Parameters["reqid"] == _currentCopySelectionRequestID)
                                    {
                                        //handle response
                                        string[] gcCodes = msg.Parameters["selection"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                        Core.Geocaches.BeginUpdate();
                                        foreach (Framework.Data.Geocache g in Core.Geocaches)
                                        {
                                            g.Selected = gcCodes.Contains(g.Code);
                                        }
                                        Core.Geocaches.EndUpdate();
                                        //are we missing geocaches?
                                        _importGeocaches.Clear();
                                        if (Core.GeocachingComAccount.MemberTypeId > 1)
                                        {
                                            foreach (string s in gcCodes)
                                            {
                                                if (Utils.DataAccess.GetGeocache(Core.Geocaches, s) == null && s.StartsWith("GC"))
                                                {
                                                    _importGeocaches.Add(s);
                                                }
                                            }
                                            if (_importGeocaches.Count > 0)
                                            {
                                                _frameworkUpdater = new Utils.FrameworkDataUpdater(Core);
                                                _getGeocacheThread = new Thread(new ThreadStart(getCopiedSelectionGeocacheInbackgroundMethod));
                                                _getGeocacheThread.IsBackground = true;
                                                _getGeocacheThread.Start();
                                            }
                                        }
                                        //reset request prop.
                                        timerCopySelection.Enabled = false;
                                        _currentCopySelectionRequestID = null;
                                        _copySelectionRequestStarted = DateTime.MinValue;
                                        toolStripStatusLabelRequestSelection.Visible = false;
                                    }
                                }
                                     */
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
