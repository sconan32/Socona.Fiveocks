using Socona.Fiveocks.Plugin;
using Socona.Fiveocks.TCP;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Socona.Fiveocks.Services;
using System.Diagnostics;
using Socona.Fiveocks.Core;
using Socona.Fiveocks.Tools;

namespace Socona.Fiveocks.SocksProtocol
{
    public class Socks5Server
    {
        private TcpListener _tcpListener;

        private CancellationTokenSource _cancellation = new CancellationTokenSource();

        public NetworkStats Stats { get; private set; }

        private bool _started;

        public int Timeout { get; set; }

        public int PacketSize { get; set; }

        public bool LoadPluginsFromDisk { get; set; }

        public Socks5Server(IPAddress ip, int port)
        {
            this.Timeout = 5000;
            this.PacketSize = 65535;
            this.LoadPluginsFromDisk = false;
            this.Stats = new NetworkStats();
            this._tcpListener = TcpListener.Create(port);
        }

        public void Start()
        {
            if (this._started)
            {
                return;
            }
            PluginLoader.LoadPluginsFromDisk = this.LoadPluginsFromDisk;
            PluginLoader.LoadPlugins();
            this._started = true;
            HandleTcpSocketLoop();
        }

        public void Stop()
        {
            if (!this._started)
            {
                return;
            }
            this._started = false;
        }

        private async void HandleTcpSocketLoop()
        {
            //wait for a socket connection       
            try
            {
                this._tcpListener.Start();
                while (!_cancellation.Token.IsCancellationRequested)
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
            string tunnelDesc = string.Empty;
            try
            {
                this.Stats.AddClient();
                using SocksInboundEntry socksInboundEntry = new SocksInboundEntry(socket);
                var request = await socksInboundEntry.RetrieveSocksRequestAsync(_cancellation.Token);
           
                using var outboundEntry = await RoutingServiceProvider.Shared.CreateService().CreateOutBoundEntryAsync(request) ?? new DirectOutboundEntry(request);             

                var domainResolvingService = DomainResolvingServiceProvider.Shared.CreateService() ?? new DomainResolvingService();            
                await request.ResolveDomainAsync(domainResolvingService);

                tunnelDesc = $"{request} <{outboundEntry.DisplayName}> ";

                Debug.WriteLine($"+ {tunnelDesc}");
                Console.WriteLine($"+ {tunnelDesc}");               

                if (await socksInboundEntry.ShakeHandAsync(request, outboundEntry, _cancellation.Token))
                {
                    using IForwardingTunnel forwardingTunnel = new ForwardingTunnel();
                    forwardingTunnel.InCounter = Stats.DownCounter;
                    forwardingTunnel.OutCounter = Stats.UpCounter;
                    forwardingTunnel.InboundEntry = socksInboundEntry;
                    forwardingTunnel.OutboundEntry = outboundEntry;
                    
                    await forwardingTunnel.ForwardAsync(_cancellation.Token);                  
                    forwardingTunnel.Dispose();                   
                }

                Debug.WriteLine($"- {tunnelDesc}");
                Console.WriteLine($"- {tunnelDesc}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                this.Stats.DecreaseClient();
              
            }

        }
    }
}
