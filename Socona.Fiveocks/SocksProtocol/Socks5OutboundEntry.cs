using Socona.Fiveocks.Core;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Socona.Fiveocks.SocksProtocol
{
    public class Socks5OutboundEntry : IOutboundEntry
    {
        string ProxyAddress { get; set; }

        int ProxyPort { get; set; }

        public SocksRequest Request { get; set; }

        public Socket Socket { get; private set; }

        public SocksUser User { get; set; }

        public string DisplayName => $"SOCKS5://{ProxyAddress}:{ProxyPort}";

        public Socks5OutboundEntry(SocksRequest request, string proxyAddress, int proxyPort, SocksUser user)
        {
            ProxyAddress = proxyAddress;
            ProxyPort = proxyPort;
            Request = request;
            User = user;
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public Socks5OutboundEntry(string proxyAddress, int proxyPort, string destDomain, int destPort)
        {
            ProxyAddress = proxyAddress;
            ProxyPort = proxyPort;

            Request = new SocksRequest(SocksCommand.TcpStream, SocksAddressType.Domain, null, destDomain, destPort);
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            if (IPAddress.TryParse(ProxyAddress, out var ipAddress))
            {
                await Socket.ConnectAsync(new IPEndPoint(ipAddress, ProxyPort), cancellationToken);
            }
            else
            {
                try
                {
                    foreach (IPAddress ip in Dns.GetHostAddresses(ProxyAddress))
                    {
                        await Socket.ConnectAsync(new IPEndPoint(ip, ProxyPort), cancellationToken);
                    }
                }
                catch (SocketException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            if (Socket.Connected && await InitializeSocks5(cancellationToken))
            {
                return true;
            }
            return false;
        }

        public async Task<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return await Socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
        }

        public async Task<int> SendAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return await Socket.SendAsync(buffer, SocketFlags.None, cancellationToken);
        }


        public async Task<bool> InitializeSocks5(CancellationToken cancellationToken = default)
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent();
            var memory = memoryOwner.Memory;

            int length = SocksPackageBuilder.Shared.BuildGreetRequestPackage(memory);
            await Socket.SendAsync(memory.Slice(0, length), SocketFlags.None, cancellationToken);

            int received = await Socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken);

            bool isAuthenized = false;

            var authMethod = SocksPackageParser.Shared.ParseAuthencatingResponse(memory, received);
            switch (authMethod)
            {
                case SocksAuthencation.None:
                    isAuthenized = true;
                    break;
                case SocksAuthencation.Login:
                    isAuthenized = await AuthenrizeUserAsync(cancellationToken);
                    break;
                default:
                    Console.WriteLine($"Server needs an unrecognized AuthType {(SocksAuthencation)memory.Span[1]} ");
                    break;
            }

            if (isAuthenized)
            {
                length = SocksPackageBuilder.Shared.BuildHandShakeRequestPackage(memory, Request);
                await Socket.SendAsync(memory.Slice(0, length), SocketFlags.None, cancellationToken);
                received = await Socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken);
                var result = SocksPackageParser.Shared.ParseHandShakeResponse(memory, received);
                return result == SocksStatus.Granted;
            }

            return false;
        }

        public async Task<bool> AuthenrizeUserAsync(CancellationToken cancellationToken = default)
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent();
            var memory = memoryOwner.Memory;
            int length = SocksPackageBuilder.Shared.BuildLoginRequestPacage(memory, User);
            await Socket.SendAsync(memory.Slice(0, length), SocketFlags.None, cancellationToken);
            int recv = await Socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken);
            var loginResult = SocksPackageParser.Shared.ParseUserLoginResponse(memory, recv);
            return loginResult == SocksUserLoginResult.Succeed;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Socket != null)
                {
                    Socket.Close();
                    Socket = null;
                }
            }
        }
    }
}
