using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GAPPChatService
{
    public class ClientConnectionsHandler: IDisposable
    {
        private List<ClientConnection> _clientConnections = new List<ClientConnection>();
        private SynchronizationContext _context = null;
        private bool _disposed = false;
        private List<string> _availableRooms = new List<string>();

        public ClientConnectionsHandler()
        {
            _context = SynchronizationContext.Current;
            if (_context == null)
            {
                _context = new SynchronizationContext();
            }
        }

        public int GetIPConnectionsCount(string ip)
        {
            int result = 0;
            _context.Send(new SendOrPostCallback(delegate(object state)
            {
                result = (from a in _clientConnections where a.IP==ip select a).Count();
            }), null);
            return result;
        }

        public void AddClientConnection(ClientConnection cc)
        {
            _context.Send(new SendOrPostCallback(delegate(object state)
            {
                _clientConnections.Add(cc);
                cc.ConnectionClosed += new EventHandler<EventArgs>(cc_ConnectionClosed);
                cc.DataReceived += new EventHandler<EventArgs>(cc_DataReceived);
                cc.AuthentificationFinished += new EventHandler<EventArgs>(cc_AuthentificationFinished);
                cc.Start();
            }), null);
        }

        void cc_AuthentificationFinished(object sender, EventArgs e)
        {
            _context.Post(new SendOrPostCallback(delegate(object state)
            {
                ClientConnection cc = sender as ClientConnection;
                if (_clientConnections.Contains(cc))
                {
                    ChatMessage msg = new ChatMessage();
                    msg.ID = cc.ID;
                    if (cc.GeocachingComUserProfile == null)
                    {
                        msg.Name = "signinfailed";
                    }
                    else
                    {
                        cc.Username = cc.GeocachingComUserProfile.User.UserName;
                        msg.Name = "signinsuccess";
                    }
                    cc.SendData(msg.ChatMessageData);
                    if (cc.GeocachingComUserProfile != null)
                    {
                        if (availableRoomsChanged())
                        {
                            updateAvailableRooms(null);
                        }
                        else
                        {
                            updateAvailableRooms(cc);
                        }
                        updateActiveClientsForRoom(cc.Room);
                    }
                }
            }), null);            
        }

        void cc_DataReceived(object sender, EventArgs e)
        {
            _context.Post(new SendOrPostCallback(delegate(object state)
            {
                ClientConnection cc = sender as ClientConnection;
                if (_clientConnections.Contains(cc))
                {
                    try
                    {
                        byte[] data = cc.ReadData();
                        while (data != null && !cc.IsClosed)
                        {
                            //parse data
                            ChatMessage msg = ChatMessage.Parse(data);
                            if (msg != null)
                            {
                                //handle data
                                if (msg.Name == "signon" && !cc.AuthentificationChecked)
                                {
                                    //if there is a client with the same ID, then it is a reconnect
                                    var ccid = (from a in _clientConnections where a.ID == msg.ID select a).FirstOrDefault();
                                    if (ccid != null)
                                    {
                                        ccid.Close();
                                        _clientConnections.Remove(ccid);
                                    }

                                    cc.Room = msg.Room;
                                    cc.ID = msg.ID;
                                    cc.Token = msg.Parameters["token"];
                                    cc.Authenticate();
                                }
                                else if (cc.AuthentificationChecked && !string.IsNullOrEmpty(cc.Username))
                                {
                                    //general chat message, broadcast it
                                    if (msg.Name == "txt")
                                    {
                                        ClientConnection[] clients = GetClientsInRoom(cc.Room);
                                        ChatMessage bmsg = new ChatMessage();
                                        bmsg.ID = cc.ID;
                                        bmsg.Room = cc.Room;
                                        bmsg.Name = "txt";
                                        bmsg.Parameters.Add("msg", msg.Parameters["msg"]);
                                        bmsg.Parameters.Add("color", msg.Parameters["color"]);

                                        sendMessageToClients(clients, bmsg);
                                    }
                                    else if (msg.Name == "follow")
                                    {
                                        cc.ActiveGeocache = msg.Parameters["cache"];
                                        cc.CanBeFollowed = bool.Parse(msg.Parameters["canfollow"]);

                                        ClientConnection[] clients = GetClientsInRoom(cc.Room);
                                        ChatMessage bmsg = new ChatMessage();
                                        bmsg.ID = cc.ID;
                                        bmsg.Room = cc.Room;
                                        bmsg.Name = "follow";
                                        bmsg.Parameters.Add("canfollow", msg.Parameters["canfollow"]);
                                        bmsg.Parameters.Add("cache", msg.Parameters["cache"]);
                                        if (msg.Parameters.ContainsKey("selected"))
                                        {
                                            string s = msg.Parameters["selected"];
                                            if (string.IsNullOrEmpty(s))
                                            {
                                                s = "";
                                                cc.SelectionCount = -1;
                                            }
                                            else
                                            {
                                                int cnt;
                                                if (int.TryParse(s, out cnt))
                                                {
                                                    cc.SelectionCount = cnt;
                                                }
                                                else
                                                {
                                                    s = "";
                                                    cc.SelectionCount = -1;
                                                }
                                            }
                                            bmsg.Parameters.Add("selected", s);
                                        }
                                        else
                                        {
                                            cc.SelectionCount = -1;
                                            bmsg.Parameters.Add("selected", "");
                                        }
                                        sendMessageToClients(clients, bmsg);
                                    }
                                    else if (msg.Name == "room")
                                    {
                                        if (cc.Room!=msg.Room)
                                        {
                                            //leave old
                                            string prevRoom = cc.Room;
                                            cc.Room = msg.Room;
                                            updateActiveClientsForRoom(prevRoom);

                                            if (availableRoomsChanged())
                                            {
                                                updateAvailableRooms(null);
                                            }

                                            //enter new
                                            updateActiveClientsForRoom(cc.Room);
                                        }
                                    }
                                    else if (msg.Name == "byebye")
                                    {
                                        cc.Close();
                                        break;
                                    }
                                    else if (msg.Name == "reqsel")
                                    {
                                        ClientConnection[] clients = GetClientsInRoom(cc.Room);
                                        ClientConnection destClient = (from c in clients where c.ID == msg.Parameters["clientid"] select c).FirstOrDefault();
                                        if (destClient != null)
                                        {
                                            ChatMessage bmsg = new ChatMessage();
                                            bmsg.ID = cc.ID;
                                            bmsg.Room = cc.Room;
                                            bmsg.Name = "reqsel";
                                            bmsg.Parameters.Add("reqid", msg.Parameters["reqid"]);
                                            bmsg.Parameters.Add("clientid", msg.Parameters["clientid"]);

                                            sendMessageToClients(new ClientConnection[] { destClient }, bmsg);
                                        }
                                        else
                                        {
                                            ChatMessage bmsg = new ChatMessage();
                                            bmsg.ID = cc.ID;
                                            bmsg.Room = cc.Room;
                                            bmsg.Name = "reqselresp";
                                            bmsg.Parameters.Add("reqid", msg.Parameters["reqid"]);
                                            bmsg.Parameters.Add("clientid", msg.Parameters["clientid"]);
                                            bmsg.Parameters.Add("selection", "");

                                            sendMessageToClients(new ClientConnection[] { cc }, bmsg);
                                        }
                                    }
                                    else if (msg.Name == "reqselresp")
                                    {
                                        ClientConnection[] clients = GetClientsInRoom(cc.Room);
                                        ClientConnection destClient = (from c in clients where c.ID == msg.Parameters["clientid"] select c).FirstOrDefault();
                                        if (destClient != null)
                                        {
                                            ChatMessage bmsg = new ChatMessage();
                                            bmsg.ID = cc.ID;
                                            bmsg.Room = cc.Room;
                                            bmsg.Name = "reqselresp";
                                            bmsg.Parameters.Add("reqid", msg.Parameters["reqid"]);
                                            bmsg.Parameters.Add("clientid", msg.Parameters["clientid"]);
                                            bmsg.Parameters.Add("selection", msg.Parameters["selection"]);

                                            sendMessageToClients(new ClientConnection[] { destClient }, bmsg);
                                        }
                                    }
                                }
                            }
                            data = cc.ReadData();
                        }
                    }
                    catch
                    {
                        cc.Close();
                    }
                }
            }), null);            
        }

        private ClientConnection[] GetClientsInRoom(string room)
        {
            return (from a in _clientConnections where a.AuthentificationChecked && !a.IsClosed && !string.IsNullOrEmpty(a.Username) && a.Room==room select a).ToArray();
        }

        private void sendMessageToClients(ClientConnection[] clients, ChatMessage msg)
        {
            byte[] data = msg.ChatMessageData;
            foreach (var a in clients)
            {
                byte[] b = new byte[data.Length];
                data.CopyTo(b, 0);
                a.SendData(b);
            }
        }


        private bool availableRoomsChanged()
        {
            bool result = false;
            List<string> actRooms = (from a in _clientConnections where a.AuthentificationChecked && !a.IsClosed && !string.IsNullOrEmpty(a.Username) && a.Room!="Lobby" select a.Room).Distinct().ToList();
            if (actRooms.Count != _availableRooms.Count)
            {
                result = true;
            }
            else
            {
                if ((from a in actRooms join b in _availableRooms on a equals b select a).Count() != actRooms.Count)
                {
                    result = true;
                }
            }
            if (result)
            {
                _availableRooms = actRooms;
            }
            return result;
        }

        private void updateAvailableRooms(ClientConnection cc)
        {
            ClientConnection[] clients;
            if (cc == null)
            {
                clients = (from a in _clientConnections where a.AuthentificationChecked && !a.IsClosed && !string.IsNullOrEmpty(a.Username) select a).ToArray();
            }
            else
            {
                clients = new ClientConnection[] { cc };
            }
            ChatMessage msg = new ChatMessage();
            msg.ID = "";
            msg.Room = "";
            msg.Name = "rooms";
            msg.Parameters.Add(string.Format("name{0}", 0), "Lobby");
            for (int i = 0; i < _availableRooms.Count; i++)
            {
                msg.Parameters.Add(string.Format("name{0}", i+1), _availableRooms[i]);
            }
            sendMessageToClients(clients, msg);
        }

        private void updateActiveClientsForRoom(string room)
        {
            ClientConnection[] clients = GetClientsInRoom(room);
            ChatMessage msg = new ChatMessage();
            msg.ID = "";
            msg.Room = room;
            msg.Name = "usersinroom";
            for (int i = 0; i < clients.Length; i++)
            {
                msg.Parameters.Add(string.Format("name{0}", i), clients[i].Username);
                msg.Parameters.Add(string.Format("id{0}", i), clients[i].ID);
                msg.Parameters.Add(string.Format("cbf{0}", i), clients[i].CanBeFollowed.ToString());
                msg.Parameters.Add(string.Format("agc{0}", i), clients[i].ActiveGeocache);
                msg.Parameters.Add(string.Format("sc{0}", i), clients[i].SelectionCount >= 0 ? clients[i].SelectionCount.ToString() : "");
            }
            sendMessageToClients(clients, msg);
        }

        void cc_ConnectionClosed(object sender, EventArgs e)
        {
            _context.Send(new SendOrPostCallback(delegate(object state)
            {
                ClientConnection cc = sender as ClientConnection;
                if (_clientConnections.Contains(cc))
                {
                    _clientConnections.Remove(cc);
                    updateActiveClientsForRoom(cc.Room);
                }
            }), null);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _context.Send(new SendOrPostCallback(delegate(object state)
                {
                    _disposed = true;
                    foreach (var cc in _clientConnections)
                    {
                        cc.ConnectionClosed -= new EventHandler<EventArgs>(cc_ConnectionClosed);
                        cc.DataReceived -= new EventHandler<EventArgs>(cc_DataReceived);
                        cc.AuthentificationFinished -= new EventHandler<EventArgs>(cc_AuthentificationFinished);
                        cc.Dispose();
                    }
                    _clientConnections.Clear();
                }), null);
            }
        }
    }
}
