using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Socona.Fiveocks.Encryption;
using Socona.Fiveocks.Plugin;
using Socona.Fiveocks.TCP;

namespace Socona.Fiveocks.SocksProtocol
{
//    class SocksSpecialTunnel : IDisposable
//    {
//        public SocksRequest Req;
//        public SocksRequest ModifiedReq;

//        public SocksInboundEntry Client;
//        public Socket RemoteClient;

//        private List<DataHandler> Plugins = new List<DataHandler>();

//        private int Timeout = 10000;
//        private int PacketSize = 4096;
//        private SocksEncryption se;
//        public event EventHandler TunnelDisposing;
//        private bool isDisposing;
//        public SocksSpecialTunnel(SocksInboundEntry p, SocksEncryption ph, SocksRequest req, SocksRequest req1, int packetSize, int timeout)
//        {          
//            Client = p;
//            Req = req;
//            ModifiedReq = req1;
//            PacketSize = packetSize;
//            Timeout = timeout;
//            se = ph;
//            isDisposing = false;
//        }

//        public void Start()
//        {
//            if (ModifiedReq.Address == null || ModifiedReq.Port <= -1) { Client.Socket.Disconnect(false); return; }
//#if DEBUG
//            Console.WriteLine("{0}:{1}", ModifiedReq.Address, ModifiedReq.Port);
//#endif
//            //foreach (ConnectSocketOverrideHandler conn in PluginLoader.LoadPlugin(typeof(ConnectSocketOverrideHandler)))
//            //    if (conn.Enabled)
//            //    {
//            //        ISocketClient pm = conn.OnConnectOverride(ModifiedReq);
//            //        if (pm != null)
//            //        {
//            //            //check if it's connected.
//            //            if (pm.Socket.Connected)
//            //            {
//            //                RemoteClient = pm;
//            //                //send request right here.
//            //                byte[] shit = Req.GetData(true);
//            //                shit[1] = 0x00;
//            //                //process packet.
//            //                byte[] output = se.ProcessOutputData(shit, 0, shit.Length);
//            //                //gucci let's go.
//            //                Client.SocketClient.Send(output);
//            //                ConnectHandler(null);
//            //                return;
//            //            }
//            //        }
//            //    }
//            var socketArgs = new SocketAsyncEventArgs { RemoteEndPoint = new IPEndPoint(ModifiedReq.IPAddress, ModifiedReq.Port) };
//            socketArgs.Completed += socketArgs_Completed;
           
//            RemoteClient = new Socket(SocketType.Stream, ProtocolType.Tcp);
//            if (!RemoteClient.ConnectAsync(socketArgs))
//                ConnectHandler(socketArgs);
//        }

//        void socketArgs_Completed(object sender, SocketAsyncEventArgs e)
//        {
//            byte[] request = Req.GetData(true); // Client.Client.Send(Req.GetData());
//            if (e.SocketError != SocketError.Success)
//            {
//                Console.WriteLine("Error while connecting: {0}", e.SocketError.ToString());
//                request[1] = (byte)SockStatus.Unreachable;
//            }
//            else
//            {
//                request[1] = 0x00;
//            }

//            byte[] encreq = se.ProcessOutputData(request, 0, request.Length);
//            Client.Socket.Send(encreq);

//            switch (e.LastOperation)
//            {
//                case SocketAsyncOperation.Connect:
//                    //connected;
//                    ConnectHandler(e);
//                    break;
//            }
//        }

//        private void ConnectHandler(SocketAsyncEventArgs e)
//        {
//            //start receiving from both endpoints.
//            try
//            {
//                //all plugins get the event thrown.
//                foreach (DataHandler data in PluginLoader.LoadPlugin(typeof(DataHandler)))
//                    Plugins.Push(data);
//                //Client.SocketClient.DataReceived += Client_onDataReceived;
//                //RemoteClient.DataReceived += RemoteClient_onDataReceived;
//                //RemoteClient.Disconnecting += RemoteClientDisconnected;
//                //Client.SocketClient.Disconnecting += ClientDisconnected;
//               // Client.Client.ReceiveAsyncNew();
//              //  RemoteClient.ReceiveAsyncNew();
//            }
//            catch
//            {
//            }
//        }
//        bool disconnected = false;
//        //  private bool remotedcd = false;
//        void ClientDisconnected(object sender, DisconnectEventArgs e)
//        {
//            if (disconnected) return;
//            disconnected = true;
//            RemoteClient.Disconnect(false);
//            OnTunnelDisposing();
//        }

//        void RemoteClientDisconnected(object sender, DisconnectEventArgs e)
//        {
//#if DEBUG
//            Console.WriteLine("Remote DC'd");
//#endif
//            if (disconnected) return;
//            disconnected = true;
//            Client.Socket.Disconnect(false);
//            OnTunnelDisposing();
//        }

//        void RemoteClient_onDataReceived(object sender, DataEventArgs e)
//        {
//            e.Request = this.ModifiedReq;
//            try
//            {
//                foreach (DataHandler f in Plugins)
//                    if (f.Enabled)
//                        f.OnDataReceived(this, e);
//                //craft headers & shit.
//                byte[] outputdata = se.ProcessOutputData(e.Buffer, e.Offset, e.Count);
//                //send outputdata's length firs.t
//                Client.Socket.Send(BitConverter.GetBytes(outputdata.Length));
//                e.Buffer = outputdata;
//                e.Offset = 0;
//                e.Count = outputdata.Length;
//                //ok now send data.
//                Client.Socket.Send(e.Buffer, e.Count, SocketFlags.None);
//            }
//            catch
//            {
//                Client.Socket.Disconnect(false);
//                RemoteClient.Disconnect(false);
//                OnTunnelDisposing();
//            }
//        }

//        void Client_onDataReceived(object sender, DataEventArgs e)
//        {
//            e.Request = this.ModifiedReq;
//            //this should be packet header.
//            try
//            {
//                int torecv = BitConverter.ToInt32(e.Buffer, e.Offset);
//                byte[] newbuff = new byte[torecv];
//                int recv = Client.Socket.Receive(newbuff,  newbuff.Length, SocketFlags.None);
//                if (recv == torecv)
//                {
//                    //yey
//                    //process packet.
//                    byte[] output = se.ProcessInputData(newbuff, 0, recv);
//                    e.Buffer = output;
//                    e.Offset = 0;
//                    e.Count = output.Length;
//                    //receive full packet.
//                    foreach (DataHandler f in Plugins)
//                        if (f.Enabled)
//                            f.OnDataSent(this, e);
//                    RemoteClient.Send(e.Buffer, e.Count,SocketFlags.None);
//                }
//                else
//                {
//                    throw new Exception();
//                }
//            }
//            catch
//            {
//                //disconnect.
//                Client.Socket.Disconnect(false);
//                RemoteClient.Disconnect(false);
//                OnTunnelDisposing();
//            }
//        }

//        protected void OnTunnelDisposing()
//        {
//            if (this.TunnelDisposing != null)
//            {
//                TunnelDisposing(this, new EventArgs());
//            }
//            Dispose();
//        }
//        public void Dispose()
//        {
//            if (isDisposing)
//            {
//                return;
//            }
//            isDisposing = true;
//            disconnected = true;
//            this.Client = null;
//            this.RemoteClient = null;
//            this.ModifiedReq = null;
//            this.Req = null;
//        }


//    }
}
