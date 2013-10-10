using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;


namespace GAPPChatService
{
	public class ChatService: IDisposable
	{
        Thread _listenThread = null;
        ClientConnectionsHandler _clientHandler = null;
        private volatile bool _doaccept = true;

        public ChatService() 
        {
            _clientHandler = new ClientConnectionsHandler();
        }

        public void Start()
        {
            _listenThread = new Thread(new ThreadStart(TcpListenerMethod));
            _listenThread.Name = "TcpListener";
            _listenThread.IsBackground = true;
            _listenThread.Start();
        }

        private void TcpListenerMethod()
        {

            TcpListener _TcpListener = new TcpListener(IPAddress.Any, 8542);
            _TcpListener.Start();
            while (_doaccept)
            {
                TcpClient tcpClient = _TcpListener.AcceptTcpClient();

                bool allowed = false;

                lock (this)
                {
                    if (_doaccept)
                    {
                        string ip = tcpClient.Client.RemoteEndPoint.ToString();
                        ip = ip.Substring(0, ip.IndexOf(':'));

                        //todo: check connection requests per IP
                        allowed = (_clientHandler.GetIPConnectionsCount(ip)<10);

                        if (allowed)
                        {
                            ClientConnection cc = new ClientConnection(tcpClient);
                            cc.IP = ip;
                            _clientHandler.AddClientConnection(cc);
                        }
                    }
                }

                if (!allowed)
                {
                    try
                    {
                        tcpClient.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_clientHandler != null)
            {
                lock (this)
                {
                    _doaccept = false;
                }
                _clientHandler.Dispose();
                _clientHandler = null;
            }
        }
    }
}
