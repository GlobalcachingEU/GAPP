using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace GlobalcachingApplication.Plugins.Chat
{
    public partial class ChatForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Chat";
        public const string STR_NOTCONNECTED = "Not connected";
        public const string STR_CONNECTED = "Connected";
        public const string STR_CONNECTING = "Connecting...";
        public const string STR_FOLLOW = "Follow";
        public const string STR_UNFOLLOW = "Unfollow";
        public const string STR_ICANBEFOLLOWED = "I can be followed";
        public const string STR_TEXTCOLOR = "Text Color";
        public const string STR_USERS = "Users";
        public const string STR_ROOMS = "Rooms";
        public const string STR_CREATEJOIN = "Create/Join";
        public const string STR_PLAYSOUND = "Play sound";
        public const string STR_DECOUPLE = "Decouple window";
        public const string STR_CLEAR = "Clear";
        public const string STR_MISSING = "Missing";
        public const string STR_RETRIEVING = "Retrieving data from geocaching.com...";
        public const string STR_JOINED = "joined the room";
        public const string STR_COPYSELECTION = "Copy selection";
        public const string STR_REQUESTINGSELECTION = "Requesting selection from user... seconds left:";
        public const string STR_REQUESTEDSELECTION = "requested selection";

        private SynchronizationContext _context = null;
        private string _id = "";
        private string _room = "";
        private Color _txtColor = Color.Black;
        private ServerConnection _serverConnection = null;
        private Thread _getGeocacheThread = null;
        private string _codeToGetWithLiveAPI = null;
        private System.Media.SoundPlayer _sndWelcome = null;
        private System.Media.SoundPlayer _sndMessage = null;
        private DateTime _copySelectionRequestStarted = DateTime.MinValue;
        private TimeSpan _copySelectionRequestTimeout = new TimeSpan(0, 0, 30);
        private string _currentCopySelectionRequestID = null;
        private List<string> _importGeocaches = new List<string>();
        private Utils.FrameworkDataUpdater _frameworkUpdater = null;
        private int _retryCount = 0;

        private bool _sizeInitializing = true;

        public class UserInRoomInfo
        {
            public string Username { get; set; }
            public string ID { get; set; }
            public bool present { get; set; }
            public bool CanBeFollowed { get; set; }
            public string ActiveGeocache { get; set; }
            public bool FollowThisUser { get; set; }
            public int SelectionCount { get; set; }

            public override string ToString()
            {
                if (CanBeFollowed)
                {
                    return string.Format("{2}{0} ({1}, {3})", Username, ActiveGeocache ?? "-", FollowThisUser ? "+" : "", SelectionCount>=0?SelectionCount.ToString():"?");
                }
                else
                {
                    return Username;
                }
            }
        }

        public class RoomInfo
        {
            public string Name { get; set; }
            public bool present { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        public ChatForm()
        {
            InitializeComponent();
        }

        public ChatForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            try
            {
                if (Properties.Settings.Default.WindowPos != null && !Properties.Settings.Default.WindowPos.IsEmpty)
                {
                    this.Bounds = Properties.Settings.Default.WindowPos;
                    this.StartPosition = FormStartPosition.Manual;
                }
                splitContainer1.SplitterDistance = splitContainer1.Width - Properties.Settings.Default.RightPanelWidth;
                splitContainer2.SplitterDistance = splitContainer2.Height - Properties.Settings.Default.BottomPanelHeight;
            }
            catch
            {
            }

            _sizeInitializing = false;

            _id = Guid.NewGuid().ToString("N");
            _room = "Lobby";
            labelActiveRoom.Text = _room;
            toolStripStatusLabelGettingGeocache.Visible = false;

            _context = SynchronizationContext.Current;
            if (_context == null)
            {
                _context = new SynchronizationContext();
            }

            SelectedLanguageChanged(this, EventArgs.Empty);
            core.ActiveGeocacheChanged += new Framework.EventArguments.GeocacheEventHandler(core_ActiveGeocacheChanged);
            core.Geocaches.GeocacheSelectedChanged += new Framework.EventArguments.GeocacheEventHandler(Geocaches_GeocacheSelectedChanged);
            core.Geocaches.ListSelectionChanged += new EventHandler(Geocaches_ListSelectionChanged);
        }

        void Geocaches_ListSelectionChanged(object sender, EventArgs e)
        {
            if (checkBoxCanFollow.Checked)
            {
                checkBoxCanFollow_CheckedChanged(this, EventArgs.Empty);
            }
        }

        void Geocaches_GeocacheSelectedChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (checkBoxCanFollow.Checked)
            {
                checkBoxCanFollow_CheckedChanged(this, EventArgs.Empty);
            }            
        }

        void core_ActiveGeocacheChanged(object sender, Framework.EventArguments.GeocacheEventArgs e)
        {
            if (checkBoxCanFollow.Checked)
            {
                checkBoxCanFollow_CheckedChanged(this, EventArgs.Empty);
            }
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            base.SelectedLanguageChanged(sender, e);

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.buttonFollow.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FOLLOW);
            this.buttonCopySelection.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COPYSELECTION);
            this.checkBoxCanFollow.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ICANBEFOLLOWED);
            this.buttonTxtColor.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TEXTCOLOR);
            this.tabControl1.TabPages[0].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_USERS);
            this.tabControl1.TabPages[1].Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ROOMS);
            this.buttonCreateJoinRoom.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CREATEJOIN);
            this.checkBoxPlaySound.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PLAYSOUND);
            this.checkBoxDecoupleWindow.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DECOUPLE);
            this.buttonClear.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CLEAR);
            this.toolStripStatusLabelGettingGeocache.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RETRIEVING);

            if (_serverConnection == null || _serverConnection.IsClosed)
            {
                toolStripStatusLabelConnectionStatus.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NOTCONNECTED);
            }
            else
            {
                toolStripStatusLabelConnectionStatus.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CONNECTED);
            }
            listBox1_SelectedIndexChanged(this, EventArgs.Empty);
        }

        public void UpdateView()
        {
            if (_serverConnection == null)
            {
                try
                {
                    panel1.Enabled = false;
                    splitContainer1.Enabled = false;

                    toolStripStatusLabelConnectionStatus.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CONNECTING);

                    TcpClient tcpClient = new TcpClient();

                    //then intialize
                    _serverConnection = new ServerConnection(tcpClient);
                    _serverConnection.ConnectionClosed += new EventHandler<EventArgs>(_serverConnection_ConnectionClosed);
                    _serverConnection.DataReceived += new EventHandler<EventArgs>(_serverConnection_DataReceived);
                    _serverConnection.Start();

                    ChatMessage msg = new ChatMessage();
                    msg.ID = _id;
                    msg.Name = "signon";
                    msg.Room = _room;
                    msg.Parameters.Add("username", Core.GeocachingComAccount.AccountName);
                    msg.Parameters.Add("token", Core.GeocachingComAccount.APIToken);
                    _serverConnection.SendData(msg.ChatMessageData);
                }
                catch
                {
                    CloseConnection();
                }
            }
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
                                    CloseConnection();
                                    break;
                                }
                                else if (msg.Name == "signinsuccess")
                                {
                                    _retryCount = 0;
                                    panel1.Enabled = true;
                                    splitContainer1.Enabled = true;

                                    toolStripStatusLabelConnectionStatus.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CONNECTED);
                                }
                                else if (msg.Name == "txt")
                                {
                                    AddMessage(msg.ID, msg.Parameters["msg"], Color.FromArgb(int.Parse(msg.Parameters["color"])));
                                    if (checkBoxPlaySound.Checked && _sndWelcome != null)
                                    {
                                        _sndMessage.Play();
                                        //System.Media.SystemSounds.Beep.Play();
                                    }
                                }
                                else if (msg.Name == "usersinroom")
                                {
                                    bool newUser = false;
                                    int i = 0;
                                    UserInRoomInfo[] curUsr = (from UserInRoomInfo a in listBox1.Items select a as UserInRoomInfo).ToArray();
                                    foreach (var x in curUsr)
                                    {
                                        x.present = false;
                                    }
                                    bool wasempty = curUsr.Length == 0;
                                    while (msg.Parameters.ContainsKey(string.Format("name{0}", i)))
                                    {
                                        string id = msg.Parameters[string.Format("id{0}", i)];
                                        UserInRoomInfo c = (from x in curUsr where x.ID == id select x).FirstOrDefault();
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
                                            listBox1.Items.Add(c);
                                            if (!wasempty)
                                            {
                                                AddMessage("-1", string.Format("{0} {1}", c.Username, Utils.LanguageSupport.Instance.GetTranslation(STR_JOINED)), Color.Black);
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
                                    foreach (var x in curUsr)
                                    {
                                        if (!x.present)
                                        {
                                            listBox1.Items.Remove(x);
                                        }
                                    }
                                    typeof(ListBox).InvokeMember("RefreshItems", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, listBox1, new object[] { });
                                    listBox1_SelectedIndexChanged(this, EventArgs.Empty);

                                    if (checkBoxPlaySound.Checked && newUser && _sndWelcome != null)
                                    {
                                        _sndWelcome.Play();
                                        //System.Media.SystemSounds.Beep.Play();
                                    }
                                }
                                else if (msg.Name == "follow")
                                {
                                    UserInRoomInfo[] curUsr = (from UserInRoomInfo a in listBox1.Items select a as UserInRoomInfo).ToArray();
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
                                                linkLabelUnavailableCache.Visible = false;
                                            }
                                            else
                                            {
                                                if (!string.IsNullOrEmpty(c.ActiveGeocache))
                                                {
                                                    Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(Core.Geocaches, c.ActiveGeocache);
                                                    if (gc != null)
                                                    {
                                                        Core.ActiveGeocache = gc;
                                                        linkLabelUnavailableCache.Visible = false;
                                                    }
                                                    else if (c.ActiveGeocache.StartsWith("GC") && Core.GeocachingComAccount.MemberTypeId > 1)
                                                    {
                                                        //offer to download
                                                        linkLabelUnavailableCache.Links.Clear();
                                                        linkLabelUnavailableCache.Text = string.Format("{0}: {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_MISSING), c.ActiveGeocache);
                                                        linkLabelUnavailableCache.Links.Add(linkLabelUnavailableCache.Text.IndexOf(':') + 2, c.ActiveGeocache.Length, c.ActiveGeocache);
                                                        linkLabelUnavailableCache.Visible = true;
                                                    }
                                                    else
                                                    {
                                                        linkLabelUnavailableCache.Visible = false;
                                                    }
                                                }
                                                else
                                                {
                                                    linkLabelUnavailableCache.Visible = false;
                                                }
                                            }
                                        }
                                    }
                                    typeof(ListBox).InvokeMember("RefreshItems", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, listBox1, new object[] { });
                                    listBox1_SelectedIndexChanged(this, EventArgs.Empty);
                                }
                                else if (msg.Name == "rooms")
                                {
                                    RoomInfo[] curRms = (from RoomInfo a in listBox2.Items select a as RoomInfo).ToArray();
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
                                            listBox2.Items.Add(c);
                                        }
                                        else
                                        {
                                            c.present = true;
                                        }
                                        i++;
                                    }
                                    foreach (var x in curRms)
                                    {
                                        if (!x.present)
                                        {
                                            listBox2.Items.Remove(x);
                                        }
                                    }
                                    typeof(ListBox).InvokeMember("RefreshItems", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, listBox2, new object[] { });
                                }
                                else if (msg.Name == "reqsel")
                                {
                                    StringBuilder sb = new StringBuilder();
                                    UserInRoomInfo[] curUsr = (from UserInRoomInfo a in listBox1.Items select a as UserInRoomInfo).ToArray();
                                    UserInRoomInfo c = (from x in curUsr where x.ID == msg.ID select x).FirstOrDefault();
                                    if (c != null)
                                    {
                                        AddMessage("-1", string.Format("{0} {1}", c.Username, Utils.LanguageSupport.Instance.GetTranslation(STR_REQUESTEDSELECTION)), Color.Black);
                                        var gsel = from Framework.Data.Geocache g in Core.Geocaches where g.Selected select g;
                                        foreach (var g in gsel)
                                        {
                                            sb.AppendFormat("{0},", g.Code);
                                        }
                                    }

                                    ChatMessage bmsg = new ChatMessage();
                                    bmsg.Name = "reqselresp";
                                    bmsg.Parameters.Add("reqid", msg.Parameters["reqid"]);
                                    bmsg.Parameters.Add("clientid", msg.ID);
                                    bmsg.Parameters.Add("selection", sb.ToString());

                                    sendMessage(bmsg);
                                }
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
                            }
                            data = _serverConnection.ReadData();
                        }
                    }
                }
                catch
                {
                    CloseConnection();
                }
            }), null);
        }


        private void InsertImage(Image img)
        {
            //Properties.Resources.wink

            //richTextBox1.rtf
            //{\pict\pngblip\picw10449\pich3280\picwgoal5924\pichgoal1860 hex data}
            //{\pict\wmetafile8\picw[N]\pich[N]\picwgoal[N]\pichgoal[N] [BYTES]}
            //1 Twip = 1/20 Point 
            //goal is twips
            //1 Twip = 1/1440 Inch
            //int picwgoal = (int)Math.Round((_image.Width / xDpi) * TWIPS_PER_INCH);

            //string str = BitConverter.ToString(bytes, 0).Replace("-", string.Empty); (bytes is stream to array, byte array

            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                byte[] bytes = stream.ToArray();
                string str = BitConverter.ToString(bytes, 0).Replace("-", string.Empty);
                int width = 400;
                int height = 400;

                string rtf = richTextBox1.Rtf;
                //exclude the final } and anything after it so we can use Append instead of Insert
                StringBuilder richText = new StringBuilder(rtf, 0, rtf.LastIndexOf('}'), rtf.Length /* this capacity should be selected for the specific application */);
                richText.Append(string.Concat(@"{\pict\pngblip\picw", img.Width.ToString(), @"\pich", img.Height.ToString(), @"\picwgoal", width.ToString(), @" \pichgoal", height.ToString(), @" \hex ", str, "}"));
                richText.AppendLine("}");
                richTextBox1.Rtf = richText.ToString();
                //richTextBox1.Rtf += string.Concat(@"{\pict\pngblip\picw", img.Width.ToString(), @"\pich", img.Height.ToString(), @"\picwgoal", width.ToString(), @"\pichgoal", height.ToString(), @"\bin ", str, "}");
            }
        }

        private void AddMessage(string msg, Color col)
        {
            //InsertImage(Properties.Resources.wink);

            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.SelectionLength = 0;

            richTextBox1.SelectionColor = col;
            richTextBox1.AppendText(msg);
            richTextBox1.SelectionColor = richTextBox1.ForeColor;
        }

        private void AddMessage(string fromid, string msg, Color col)
        {
            UserInRoomInfo usr = (from UserInRoomInfo a in listBox1.Items where a.ID == fromid select a as UserInRoomInfo).FirstOrDefault();

            AddMessage(string.Format("{0} [{1}]:", DateTime.Now.ToString("t"), usr==null?"":usr.Username), Color.Black);
            AddMessage(msg, col);

            richTextBox1.AppendText(Environment.NewLine);
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }

        void _serverConnection_ConnectionClosed(object sender, EventArgs e)
        {
            _context.Post(new SendOrPostCallback(delegate(object state)
            {
                panel1.Enabled = false;
                splitContainer1.Enabled = false;
                _serverConnection = null;
                toolStripStatusLabelConnectionStatus.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NOTCONNECTED);
                checkRetry();
            }), null);
        }

        private void CloseConnection()
        {
            try
            {
                listBox1.Items.Clear();
                panel1.Enabled = false;
                splitContainer1.Enabled = false;
                toolStripStatusLabelConnectionStatus.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NOTCONNECTED);
                checkBoxCanFollow.Checked = false;
                if (_serverConnection != null)
                {
                    _serverConnection.ConnectionClosed -= new EventHandler<EventArgs>(_serverConnection_ConnectionClosed);
                    _serverConnection.DataReceived -= new EventHandler<EventArgs>(_serverConnection_DataReceived);
                    _serverConnection.Dispose();
                    _serverConnection = null;
                }
                checkRetry();
            }
            catch
            {
            }
        }

        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            _retryCount = 0;
            CloseConnection();
        }


        private bool sendMessage(ChatMessage msg)
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
                CloseConnection();
            }
            return result;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                string s = textBox1.Text.Trim();
                if (s.Length > 0)
                {
                    ChatMessage msg = new ChatMessage();
                    msg.Name = "txt";
                    msg.Parameters.Add("msg", s);
                    msg.Parameters.Add("color", _txtColor.ToArgb().ToString());
                    if (sendMessage(msg))
                    {
                        textBox1.Text = "";
                    }
                    e.Handled = true;
                }
            }
        }

        private void checkBoxCanFollow_CheckedChanged(object sender, EventArgs e)
        {
            ChatMessage msg = new ChatMessage();
            msg.Name = "follow";
            msg.Parameters.Add("canfollow", checkBoxCanFollow.Checked.ToString());
            msg.Parameters.Add("cache", checkBoxCanFollow.Checked ? Core.ActiveGeocache == null ?"": Core.ActiveGeocache.Code : "");
            msg.Parameters.Add("selected", checkBoxCanFollow.Checked ? (from Framework.Data.Geocache g in Core.Geocaches where g.Selected select g).Count().ToString() : "");
            sendMessage(msg);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UserInRoomInfo c = listBox1.SelectedItem as UserInRoomInfo;
            if (c != null)
            {
                if (c.CanBeFollowed && c.ID!=_id)
                {
                    buttonFollow.Enabled = true;
                    buttonCopySelection.Enabled = c.SelectionCount > 0;
                    if (c.FollowThisUser)
                    {
                        buttonFollow.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_UNFOLLOW);
                    }
                    else
                    {
                        buttonFollow.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FOLLOW);
                    }
                }
                else
                {
                    buttonFollow.Enabled = false;
                    buttonCopySelection.Enabled = false;
                }
            }
            else
            {
                buttonFollow.Enabled = false;
                buttonCopySelection.Enabled = false;
            }
        }

        private void buttonFollow_Click(object sender, EventArgs e)
        {
            UserInRoomInfo c = listBox1.SelectedItem as UserInRoomInfo;
            if (c != null)
            {
                if (c.FollowThisUser)
                {
                    c.FollowThisUser = false;
                }
                else if (c.CanBeFollowed)
                {
                    c.FollowThisUser = true;                   
                }
                typeof(ListBox).InvokeMember("RefreshItems", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, listBox1, new object[] { });
                listBox1_SelectedIndexChanged(this, EventArgs.Empty);
                if (c.FollowThisUser)
                {
                    if (!string.IsNullOrEmpty(c.ActiveGeocache))
                    {
                        Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(Core.Geocaches, c.ActiveGeocache);
                        if (gc != null)
                        {
                            Core.ActiveGeocache = gc;
                        }
                    }
                }
            }
        }

        private void textBoxRoomName_TextChanged(object sender, EventArgs e)
        {
            buttonCreateJoinRoom.Enabled = (textBoxRoomName.Text.Trim().Length > 0 && textBoxRoomName.Text!=_room);
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            RoomInfo ri = listBox2.SelectedItem as RoomInfo;
            if (ri != null)
            {
                textBoxRoomName.Text = ri.Name;
            }
        }

        private void buttonCreateJoinRoom_Click(object sender, EventArgs e)
        {
            _room = textBoxRoomName.Text.Trim();
            ChatMessage msg = new ChatMessage();
            msg.Name = "room";
            sendMessage(msg);
            labelActiveRoom.Text = _room;
        }

        private void buttonTxtColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = _txtColor;
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _txtColor = colorDialog1.Color;
                buttonTxtColor.ForeColor = _txtColor;
            }
        }

        private void listBox2_DoubleClick(object sender, EventArgs e)
        {
            RoomInfo ri = listBox2.SelectedItem as RoomInfo;
            if (ri != null)
            {
                textBoxRoomName.Text = ri.Name;
                if (ri.Name != _room)
                {
                    buttonCreateJoinRoom_Click(this, EventArgs.Empty);
                }
            }
        }

        private void checkBoxDecoupleWindow_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxDecoupleWindow.Checked)
            {
                this.MdiParent = null;
            }
            else
            {
                Framework.Interfaces.IPluginUIMainWindow mainPlugin = (from Framework.Interfaces.IPlugin a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault() as Framework.Interfaces.IPluginUIMainWindow;
                if (mainPlugin != null)
                {
                    this.MdiParent = mainPlugin.MainForm;
                }
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        private bool processRunningInBackground()
        {
            return (_copySelectionRequestStarted != DateTime.MinValue ||  //Copy a selection from another user
                    (_getGeocacheThread != null && _getGeocacheThread.IsAlive) //getting geocache by Live API
                    );
        }

        private void linkLabelUnavailableCache_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //get geocache in background
            if (!processRunningInBackground())
            {
                toolStripStatusLabelGettingGeocache.Visible = true;
                _codeToGetWithLiveAPI = e.Link.LinkData.ToString();
                _getGeocacheThread = new Thread(new ThreadStart(getGeocacheInbackgroundMethod));
                _getGeocacheThread.IsBackground = true;
                _getGeocacheThread.Start();
            }
        }


        private void getCopiedSelectionGeocacheInbackgroundMethod()
        {
            try
            {
                int max = _importGeocaches.Count;
                using (Utils.ProgressBlock prog = new Utils.ProgressBlock(OwnerPlugin as Utils.BasePlugin.Plugin, STR_COPYSELECTION, STR_RETRIEVING, max, 0, true))
                {
                    using (Utils.API.GeocachingLiveV6 client = new Utils.API.GeocachingLiveV6(Core, string.IsNullOrEmpty(Core.GeocachingComAccount.APIToken)))
                    {
                        var req = new Utils.API.LiveV6.SearchForGeocachesRequest();
                        req.AccessToken = client.Token;
                        while (_importGeocaches.Count > 0)
                        {
                            req.CacheCode = new Utils.API.LiveV6.CacheCodeFilter();
                            req.CacheCode.CacheCodes = (from gc in _importGeocaches select gc).Take(10).ToArray();
                            req.GeocacheLogCount = 5;
                            req.IsLite = Core.GeocachingComAccount.MemberTypeId == 1;
                            req.MaxPerPage = req.CacheCode.CacheCodes.Length;
                            var resp = client.Client.SearchForGeocaches(req);
                            if (resp.Status.StatusCode == 0 && resp.Geocaches != null)
                            {
                                _importGeocaches.RemoveRange(0, req.CacheCode.CacheCodes.Length);
                                Utils.API.Import.AddGeocaches(Core, resp.Geocaches);
                            }
                            else
                            {
                                break;
                            }
                            if (!prog.UpdateProgress(STR_COPYSELECTION, STR_RETRIEVING, max, max - _importGeocaches.Count))
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            _context.Send(new SendOrPostCallback(delegate(object state)
            {
                _frameworkUpdater.Dispose();
                _frameworkUpdater = null;
            }), null);
        }

        private void getGeocacheInbackgroundMethod()
        {
            Utils.API.LiveV6.Geocache[] gcList = null;
            try
            {
                using (Utils.API.GeocachingLiveV6 client = new Utils.API.GeocachingLiveV6(Core, string.IsNullOrEmpty(Core.GeocachingComAccount.APIToken)))
                {
                    var req = new Utils.API.LiveV6.SearchForGeocachesRequest();
                    req.AccessToken = client.Token;
                    req.CacheCode = new Utils.API.LiveV6.CacheCodeFilter();
                    req.CacheCode.CacheCodes = new string[] { _codeToGetWithLiveAPI };
                    req.GeocacheLogCount = 5;
                    req.IsLite = Core.GeocachingComAccount.MemberTypeId == 1;
                    req.MaxPerPage = 1;
                    var resp = client.Client.SearchForGeocaches(req);
                    if (resp.Status.StatusCode == 0 && resp.Geocaches != null)
                    {
                        gcList = resp.Geocaches;
                    }
                }
            }
            catch
            {
            }
            _context.Post(new SendOrPostCallback(delegate(object state)
            {
                Framework.Data.Geocache activeGC = null;
                if (gcList != null)
                {
                    using (Utils.FrameworkDataUpdater dupd = new Utils.FrameworkDataUpdater(Core))
                    {
                        List<Framework.Data.Geocache> lst = Utils.API.Import.AddGeocaches(Core, gcList);
                        if (lst != null && lst.Count > 0)
                        {
                            activeGC = lst[lst.Count - 1];
                        }
                    }
                }
                toolStripStatusLabelGettingGeocache.Visible = false;
                Core.ActiveGeocache = activeGC;
                if (activeGC != null)
                {
                    if (linkLabelUnavailableCache.Visible)
                    {
                        if (linkLabelUnavailableCache.Text.IndexOf(activeGC.Code) >= 0)
                        {
                            linkLabelUnavailableCache.Visible = false;
                        }
                    }
                }
            }), null);
        }

        private void ChatForm_Shown(object sender, EventArgs e)
        {
            try
            {
                _sndWelcome = new System.Media.SoundPlayer(Properties.Resources.welcome);
                _sndMessage = new System.Media.SoundPlayer(Properties.Resources.message);
                checkBoxPlaySound.Checked = true;
            }
            catch
            {
                checkBoxPlaySound.Enabled = false;
            }
        }

        private void buttonCopySelection_Click(object sender, EventArgs e)
        {
            if (!processRunningInBackground())
            {
                UserInRoomInfo c = listBox1.SelectedItem as UserInRoomInfo;
                if (c != null)
                {
                    if (c.CanBeFollowed && c.SelectionCount > 0)
                    {
                        //send request
                        _currentCopySelectionRequestID = Guid.NewGuid().ToString("N");
                        
                        ChatMessage msg = new ChatMessage();
                        msg.Name = "reqsel";
                        msg.Parameters.Add("reqid", _currentCopySelectionRequestID);
                        msg.Parameters.Add("clientid", c.ID);
                        sendMessage(msg);

                        _copySelectionRequestStarted = DateTime.Now;
                        int secs = (int)_copySelectionRequestTimeout.TotalSeconds;
                        this.toolStripStatusLabelRequestSelection.Text = string.Format("{0} {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_REQUESTINGSELECTION), secs);
                        this.toolStripStatusLabelRequestSelection.Visible = true;
                        timerCopySelection.Enabled = true;
                    }
                }
            }
        }

        private void timerCopySelection_Tick(object sender, EventArgs e)
        {
            TimeSpan diff = DateTime.Now - _copySelectionRequestStarted;
            if (diff > _copySelectionRequestTimeout)
            {
                this.toolStripStatusLabelRequestSelection.Visible = false;
                timerCopySelection.Enabled = false;
                _copySelectionRequestStarted = DateTime.MinValue;
                _currentCopySelectionRequestID = null;
            }
            else
            {
                int secs = (int)(_copySelectionRequestTimeout.TotalSeconds - diff.TotalSeconds);
                this.toolStripStatusLabelRequestSelection.Text = string.Format("{0} {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_REQUESTINGSELECTION), secs);
            }
        }

        private void ChatForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void ChatForm_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (!_sizeInitializing)
            {
                Properties.Settings.Default.BottomPanelHeight = splitContainer2.Height-splitContainer2.SplitterDistance;
                Properties.Settings.Default.Save();
            }
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (!_sizeInitializing)
            {
                Properties.Settings.Default.RightPanelWidth = splitContainer1.Width-splitContainer1.SplitterDistance;
                Properties.Settings.Default.Save();
            }
        }

        private void checkRetry()
        {
            _retryCount++;
            if (_retryCount < 4 && this.Visible && _serverConnection==null && !timerConnectionRetry.Enabled)
            {
                timerConnectionRetry.Enabled = true;
            }
        }
        private void timerConnectionRetry_Tick(object sender, EventArgs e)
        {
            timerConnectionRetry.Enabled = false;
            if (this.Visible && _serverConnection==null)
            {
                UpdateView();
            }
        }

    }

    public class Chat : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Chat";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(ChatForm.STR_CONNECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ChatForm.STR_CONNECTING));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ChatForm.STR_CREATEJOIN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ChatForm.STR_FOLLOW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ChatForm.STR_ICANBEFOLLOWED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ChatForm.STR_NOTCONNECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ChatForm.STR_ROOMS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ChatForm.STR_TEXTCOLOR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ChatForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ChatForm.STR_UNFOLLOW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ChatForm.STR_USERS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ChatForm.STR_PLAYSOUND));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ChatForm.STR_DECOUPLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ChatForm.STR_JOINED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ChatForm.STR_COPYSELECTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ChatForm.STR_REQUESTINGSELECTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ChatForm.STR_REQUESTEDSELECTION));           

            return base.Initialize(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.UIChildWindow;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new ChatForm(this, core));
        }

        /*
        public override bool ActionEnabled(string action, int selectCount, bool active)
        {
            bool result = base.ActionEnabled(action, selectCount, active);
            if (result)
            {
                result = !string.IsNullOrEmpty(Core.GeocachingComAccount.APIToken);
            }
            return result;
        }
         * */

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (UIChildWindowForm != null)
                {
                    if (action == ACTION_SHOW)
                    {
                        if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, false))
                        {
                            if (!UIChildWindowForm.Visible)
                            {
                                UIChildWindowForm.Show();
                                (UIChildWindowForm as ChatForm).UpdateView();
                            }
                            if (UIChildWindowForm.WindowState == FormWindowState.Minimized)
                            {
                                UIChildWindowForm.WindowState = FormWindowState.Normal;
                            }
                            UIChildWindowForm.BringToFront();
                        }
                    }
                }
                result = true;
            }
            return result;
        }
    }
}
