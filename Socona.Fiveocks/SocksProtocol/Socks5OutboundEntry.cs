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

        public string DisplayName => $"SOCKS5 {ProxyAddress}:{ProxyPort}";

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

            Request = new SocksRequest(StreamTypes.Stream, SocksAddressType.Domain, null, destDomain, destPort);          
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

            //+-------+----------+----------+
            //|  VER  | NMETHODS |  METHODS |
            //+-------+----------+----------+
            //| 1Byte |   1Byte  | 1 to 255 |
            //+-------+----------+----------+
            //Ver Socks4=0x01 Sock5=0x05
            memory.Span[0] = (byte)SocksVersions.Socks5;
            memory.Span[1] = 0x02;
            memory.Span[2] = (byte)AuthencationMethods.None;
            memory.Span[3] = (byte)AuthencationMethods.Login;
            await Socket.SendAsync(memory.Slice(0, 4), SocketFlags.None, cancellationToken);

            //+-------+----------+
            //|  VER  |  METHOD  | 
            //+-------+----------+
            //| 1Byte |   1Byte  | 
            //+-------+----------+
            //Ver Socks4=0x01 Sock5=0x05
            int received = await Socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken);
            if (received <= 0)
            {
                return false;
            }

            bool isAuthenized = false;
            //check for server version.
            if (memory.Span[0] == 0x05)
            {
                switch ((AuthencationMethods)memory.Span[1])
                {
                    case AuthencationMethods.None:
                        isAuthenized = true;
                        break;
                    case AuthencationMethods.Login:
                        isAuthenized = await AuthenrizeUserAsync(cancellationToken);
                        break;
                    default:
                        Console.WriteLine($"Server needs an unrecognized AuthType {(AuthencationMethods)memory.Span[1]} ");
                        break;
                }
            }
            if (isAuthenized)
            {
                int length = Request.MakeRequestPackage(memory);
                await Socket.SendAsync(memory.Slice(0, length), SocketFlags.None, cancellationToken);
                received = await Socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken);
                if (received > 0 &&
                    memory.Span[0] == (byte)SocksVersions.Socks5 &&
                    memory.Span[1] == (byte)SockStatus.Granted)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> AuthenrizeUserAsync(CancellationToken cancellationToken = default)
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent();
            var memory = memoryOwner.Memory;

            // .------.------------------.-----------.------------------.-----------.
            // | VER  | USERNAME_LENGTH  |  USERNAME |  PASSWORD_LNEGTH | PASSWORD  |
            // :------+------------------+-----------+------------------+-----------:
            // | 0x01 |      1Byte       |  Variable |     1Byte        | Variable  |
            // '------'------------------'-----------'------------------'-----------'

            if (User == null)
            {
                return false;
            }

            memory.Span[0] = 0x01;
            int usernameLength = Encoding.ASCII.GetByteCount(User.Username);
            int passwordLength = Encoding.ASCII.GetByteCount(User.Password);
            if (usernameLength >= 128 || passwordLength >= 128)
            {
                return false;
            }
            memory.Span[1] = (byte)usernameLength;
            if (MemoryMarshal.TryGetArray(memory.Slice(2, usernameLength), out ArraySegment<byte> arraySeg1))
            {
                Encoding.ASCII.GetBytes(User.Username, 0, User.Username.Length, arraySeg1.Array, arraySeg1.Offset);
            }

            memory.Span[2 + usernameLength] = (byte)passwordLength;
            if (MemoryMarshal.TryGetArray(memory.Slice(3 + usernameLength, passwordLength), out ArraySegment<byte> arraySeg2))
            {
                Encoding.ASCII.GetBytes(User.Password, 0, User.Password.Length, arraySeg2.Array, arraySeg2.Offset);
            }
            await Socket.SendAsync(memory.Slice(0, 3 + usernameLength + passwordLength), SocketFlags.None, cancellationToken);

            //.-------.----------.
            //|  VER  |  STATUS  |
            //:-------+----------:
            //| 0x01  |  1Byte   |
            //'-------'--------- '
            // STATUS 0x00 = Succeed 0x01 = Denied
            int recv = await Socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken);
            if (recv > 0 && memory.Span[0] == 0x01 && memory.Span[1] == 0x00)
            {
                return true;
            }

            return false;
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
