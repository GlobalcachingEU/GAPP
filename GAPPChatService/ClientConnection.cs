using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using GlobalcachingApplication.Utils.API.LiveV6;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace GAPPChatService
{
    public class ClientConnection: IDisposable
    {
        private TcpClient _tcpClient = null;
        private Thread _sendReceiveThread = null;
        private Thread _authenticateThread = null;

        public event EventHandler<EventArgs> DataReceived;
        public event EventHandler<EventArgs> ConnectionClosed;
        public event EventHandler<EventArgs> AuthentificationFinished;

        private volatile bool _closed = false;
        private volatile bool _authentificationChecked = false;

        public string Username { get; set; }
        public string IP { get; set; }
        public string Room { get; set; }
        public string ID { get; set; }
        public string Token { get; set; }
        public bool CanBeFollowed { get; set; }
        public string ActiveGeocache { get; set; }
        public UserProfile GeocachingComUserProfile { get; private set; }
        public int SelectionCount { get; set; }

        private List<byte[]> _txData = new List<byte[]>();
        private List<byte[]> _rxData = new List<byte[]>();
        private AutoResetEvent _txDataAvailable = new AutoResetEvent(false);

        public ClientConnection(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            GeocachingComUserProfile = null;
            CanBeFollowed = false;
            ActiveGeocache = "";
            SelectionCount = -1;
        }

        public bool IsClosed
        {
            get { return _closed; }
        }
        public bool AuthentificationChecked
        {
            get { return _authentificationChecked; }
        }

        public void Start()
        {
            if (_sendReceiveThread == null)
            {
                _sendReceiveThread = new Thread(new ThreadStart(SendReceiveThreadMethod));
                _sendReceiveThread.IsBackground = true;
                _sendReceiveThread.Start();
            }
        }

        public void Authenticate()
        {
            if (_authenticateThread == null)
            {
                _authenticateThread = new Thread(new ThreadStart(AuthenticateThreadMethod));
                _authenticateThread.IsBackground = true;
                _authenticateThread.Start();
            }
        }

        public void AuthenticateThreadMethod()
        {
            try
            {
                LiveClient client = null;
                BinaryMessageEncodingBindingElement binaryMessageEncoding = new BinaryMessageEncodingBindingElement()
                {
                    ReaderQuotas = new XmlDictionaryReaderQuotas()
                    {
                        MaxStringContentLength = int.MaxValue,
                        MaxBytesPerRead = int.MaxValue,
                        MaxDepth = int.MaxValue,
                        MaxArrayLength = int.MaxValue
                    }
                };

                HttpTransportBindingElement httpTransport = new HttpsTransportBindingElement()
                {
                    MaxBufferSize = int.MaxValue,
                    MaxReceivedMessageSize = int.MaxValue,
                    AllowCookies = false,
                };

                client = new LiveClient(new CustomBinding(binaryMessageEncoding, httpTransport), new EndpointAddress("https://api.groundspeak.com/LiveV6/Geocaching.svc/Silverlightsoap"));
                var req = new GetYourUserProfileRequest();
                req.AccessToken = this.Token;
                req.DeviceInfo = new DeviceData()
                            {
                                DeviceName = "GAPPChatService",
                                DeviceUniqueId = "internal",
                                ApplicationSoftwareVersion = "V1.0.0.0"
                            };

                var resp = client.GetYourUserProfile(req);
                if (resp.Status.StatusCode == 0)
                {
                    GeocachingComUserProfile = resp.Profile;
                }
                client.Close();
            }
            catch
            {
                GeocachingComUserProfile = null;
            }
            //_authenticateThread = null;
            _authentificationChecked = true;
            if (AuthentificationFinished != null && !_closed)
            {
                AuthentificationFinished(this, EventArgs.Empty);
            }
        }

        public byte[] ReadData()
        {
            byte[] data = null;
            lock (_rxData)
            {
                if (_rxData.Count > 0)
                {
                    data = _rxData[0];
                    _rxData.RemoveAt(0);
                }
            }
            return data;
        }

        public void SendData(byte[] data)
        {
            if (!_closed)
            {
                lock (_txData)
                {
                    _txData.Add(data);
                }
                try
                {
                    //could be disposed in the meantime
                    _txDataAvailable.Set();
                }
                catch
                {
                }
            }
        }

        public void SendReceiveThreadMethod()
        {
            DateTime lastTx = DateTime.Now;
            DateTime lastRx = DateTime.Now;
            TimeSpan connectionTimeout = new TimeSpan(0, 1, 0);
            TimeSpan keepAliveInterval = new TimeSpan(0, 0, 20);

            try
            {
                while (!_closed)
                {
                    _tcpClient.Client.SendTimeout = 10000;
                    _tcpClient.Client.ReceiveTimeout = 10000;
                    if (_txDataAvailable.WaitOne(200))
                    {
                        //data available, send
                        byte[] data = null;
                        lock (_txData)
                        {
                            if (_txData.Count > 0)
                            {
                                data = _txData[0];
                                _txData.RemoveAt(0);
                            }
                        }
                        while (data != null)
                        {
                            byte[] b = BitConverter.GetBytes((int)data.Length);
                            if (_tcpClient.Client.Send(b)!=b.Length) break;
                            if (data.Length > 0)
                            {
                                if (_tcpClient.Client.Send(data)!=data.Length) break;
                            }
                            lastTx = DateTime.Now;

                            data = null;
                            lock (_txData)
                            {
                                if (_txData.Count > 0)
                                {
                                    data = _txData[0];
                                    _txData.RemoveAt(0);
                                }
                            }
                        }
                    }
                    if (_tcpClient.Available > 0)
                    {
                        //data available, read first 4 bytes: the size
                        byte[] data = new byte[sizeof(int)];
                        if (_tcpClient.Client.Receive(data, 0, data.Length, SocketFlags.None)!=data.Length) break;
                        int l = BitConverter.ToInt32(data, 0);
                        if (l > 0)
                        {
                            if (l > 100000)
                            {
                                //yeah, right... bye bye
                                break;
                            }
                            else
                            {
                                data = new byte[l];
                                if (_tcpClient.Client.Receive(data, 0, data.Length, SocketFlags.None) != data.Length) break;
                                lock (_rxData)
                                {
                                    _rxData.Add(data);
                                }
                                OnDataReceived();
                            }
                        }
                        lastRx = DateTime.Now;
                    }

                    if (DateTime.Now - lastRx > connectionTimeout)
                    {
                        //bye bye
                        break;
                    }
                    else if (DateTime.Now - lastTx > keepAliveInterval)
                    {
                        SendData(new byte[0]);
                    }
                }
            }
            catch
            {
            }
            try
            {
                _tcpClient.Client.Dispose();
            }
            catch
            {
            }
            _closed = true;
            try
            {
                _txDataAvailable.Dispose();
                _txDataAvailable.Close();
                _txDataAvailable = null;
            }
            catch
            {
            }
            OnConnectionClosed();
        }

        public void OnConnectionClosed()
        {
            if (ConnectionClosed != null)
            {
                ConnectionClosed(this, EventArgs.Empty);
            }
        }

        public void OnDataReceived()
        {
            if (DataReceived != null)
            {
                DataReceived(this, EventArgs.Empty);
            }
        }

        public void Close()
        {
            try
            {
                if (!_closed)
                {
                    _closed = true;
                }
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}
