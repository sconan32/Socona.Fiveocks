﻿using Socona.Fiveocks.Plugin;
using Socona.Fiveocks.TCP;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Socona.Fiveocks.SocksProtocol;

namespace Socona.Fiveocks.SocksServer
{
    public class Socks5Server
    {
        private TcpListener _tcpListener;

        private List<IForwardingTunnel> _clients = new List<IForwardingTunnel>();

        private CancellationTokenSource cancellation = new CancellationTokenSource();

        public CancellationToken CancellationToken => cancellation.Token;

        public Stats Stats;

        private bool started;
        
        public int Timeout { get; set; }

        public int PacketSize { get; set; }

        public bool LoadPluginsFromDisk { get; set; }

        public Socks5Server(IPAddress ip, int port)
        {
            this.Timeout = 5000;
            this.PacketSize = 65535;
            this.LoadPluginsFromDisk = false;
            this.Stats = new Stats();
            this._tcpListener = TcpListener.Create(port);
        }

        public void Start()
        {
            if (this.started)
            {
                return;
            }
            PluginLoader.LoadPluginsFromDisk = this.LoadPluginsFromDisk;
            PluginLoader.LoadPlugins();
            this.started = true;
            HandleTcpSocketLoop();
        }

        public void Stop()
        {
            if (!this.started)
            {
                return;
            }
            this.started = false;
            this._clients.Clear();
        }

        private async void HandleTcpSocketLoop()
        {
            //wait for a socket connection       
            try
            {
                this._tcpListener.Start();
                while (!CancellationToken.IsCancellationRequested)
                {
                    Socket socket = await _tcpListener.AcceptSocketAsync();
                    var task = OnClientConnected(socket);
                }
                this._tcpListener.Stop();
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

        }
        private async Task OnClientConnected(Socket socket)
        {
            IForwardingTunnel forwardingTunnel = null;
            try
            {
                using SocksProtocol.SocksInboundEntry socksInboundEntry = new SocksProtocol.SocksInboundEntry(socket);
                forwardingTunnel = await socksInboundEntry.CreateForwardingTunnelAsync( CancellationToken);
                if(forwardingTunnel==null)
                {
                    return;
                }
                forwardingTunnel.InCounter = Stats.DownCounter;
                forwardingTunnel.OutCounter = Stats.UpCounter;
                _clients.Add(forwardingTunnel);
                this.Stats.AddClient();
                await forwardingTunnel.ForwardAsync(CancellationToken);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _clients.Remove(forwardingTunnel);
                this.Stats.DecreaseClient();
            }
         
        }     
    }
}
