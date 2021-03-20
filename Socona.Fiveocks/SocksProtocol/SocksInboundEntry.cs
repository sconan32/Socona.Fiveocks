using Socona.Fiveocks.Core;
using Socona.Fiveocks.Encryption;
using Socona.Fiveocks.Services;
using Socona.Fiveocks.SocksProtocol;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Socona.Fiveocks.SocksProtocol
{
    public class SocksInboundEntry : IInboundEntry
    {
        Socket Socket { get; set; }

        public SocksInboundEntry(Socket socket)
        {
            Socket = socket;
        }

        public async Task<SocksRequest> RetrieveSocksRequestAsync(CancellationToken cancellationToken = default)
        {
            var authTypes = await ReceiveAuthTypesAsync(cancellationToken);
            if (!await AuthenticateClientAsync(authTypes, cancellationToken))
            {
                return null;
            }
            var socksRequest = await ReceiveSocksRequestAsync(cancellationToken);
            socksRequest.InboundEndPoint = Socket.RemoteEndPoint;
            return socksRequest;
        }

        public async Task<bool> ShakeHandAsync(SocksRequest request, IOutboundEntry outboundEntry, CancellationToken cancellationToken = default)
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent();
            var memory = memoryOwner.Memory;
            try
            {
                if (!await outboundEntry.ConnectAsync(cancellationToken))
                {
                    request.Error = SockStatus.HostUnreachable;
                }

                var requestLength = request.MakeResponsePackage(memory);
                await SendAsync(memory.Slice(0, requestLength), cancellationToken);

                if (request.Error == SockStatus.Granted)
                {
                    return true;
                }
            }
            catch (SocketException)
            { }

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

        public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            return await ValueTask.FromResult(true);
        }

        /// <summary>
        /// Step3: Get Client Request
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<SocksRequest> ReceiveSocksRequestAsync(CancellationToken cancellationToken = default)
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent();
            var memory = memoryOwner.Memory;
            int recv = await Socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken);

            //+-------+-------+-------+-------+----------+----------+
            //| VER   |  CMD  |  RSV  |  ATYP | DST.ADDR | DST.PORT |
            //+-------+-------+-------+-------+----------+----------+
            //| 1Byte | 1Byte |  0x00 | 1Byte | Variable |  2Bytes  |
            //+-------+-------+-------+-------+----------+----------+
            // CMD: 0x01 = CONNECT 0x02 = BIND 0x03 = UDP forwarding
            // ATYP: 0x01 = IPV4 0x03 = Domain 0x04 = IPV6

            if (recv <= 0 || (SocksVersions)memory.Span[0] != SocksVersions.Socks5)
                return null;

            if ((StreamTypes)memory.Span[1] != StreamTypes.Stream)
            {
                // not supported;
                Console.WriteLine($"ERROR: Command {(StreamTypes)memory.Span[1]} is Not Supported");
                return null;
            }

            IPAddress ipAddress = null;
            string domain = null;
            int fwd = 4;

            var addressType = (SocksAddressType)memory.Span[3];
            if (addressType == SocksAddressType.IP)
            {
                ipAddress = new IPAddress(memory.Slice(4, 4).Span);
                domain = ipAddress.ToString();
                fwd += 4;
            }
            else if (addressType == SocksAddressType.IPv6)
            {
                ipAddress = new IPAddress(memory.Slice(4, 16).Span);
                domain = ipAddress.ToString();
                fwd += 16;
            }
            else if (addressType == SocksAddressType.Domain)
            {
                int domainlen = memory.Span[4];
                domain = Encoding.ASCII.GetString(memory.Slice(5, domainlen).Span);
                fwd += domainlen + 1;
                if (IPAddress.TryParse(domain, out ipAddress))
                {
                    addressType = ipAddress.AddressFamily switch
                    {
                        AddressFamily.InterNetwork => SocksAddressType.IP,
                        AddressFamily.InterNetworkV6 => SocksAddressType.IPv6,
                        _ => 0x00,
                    };

                }
            }
            else
            {
                return null;
            }

            short portOriginal = BitConverter.ToInt16(memory.Slice(fwd, 2).Span);
            int port = (ushort)IPAddress.NetworkToHostOrder(portOriginal);

            return new SocksRequest(StreamTypes.Stream, addressType, ipAddress, domain, port);
        }

        /// <summary>
        /// Step2.1: Send Selected AuthType
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task SendAuthTypeAsync(AuthencationMethods authType, CancellationToken cancellationToken = default)
        {
            //+------+--------+
            //| VER  | METHOD |
            //+------+--------+
            //| 0x05 |  1Byte |
            //+------+--------+
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(2);
            var memory = memoryOwner.Memory;
            memory.Span[0] = (byte)SocksVersions.Socks5;
            memory.Span[1] = (byte)authType;
            await Socket.SendAsync(memory.Slice(0, 2), SocketFlags.None, cancellationToken);
        }

        /// <summary>
        /// Step1: Recv Requested AuthTypes
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<AuthencationMethods>> ReceiveAuthTypesAsync(CancellationToken cancellationToken = default)
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent();
            var memory = memoryOwner.Memory;
            int recv = await Socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken);
            // +----+----------+----------+
            // |VER | NMETHODS | METHODS  |
            // +----+----------+----------+
            // | 1B |    1B    | 1 to 255 |
            // +----+----------+----------+
            SocksVersions headerType = (SocksVersions)memory.Span[0];
            if (headerType == SocksVersions.Socks5)
            {
                int methods = memory.Span[1];
                List<AuthencationMethods> types = new List<AuthencationMethods>(5);
                for (int i = 2; i < methods + 2; i++)
                {
                    switch ((AuthencationMethods)memory.Span[i])
                    {
                        case AuthencationMethods.Login:
                            types.Add(AuthencationMethods.Login);
                            break;
                        case AuthencationMethods.None:
                            types.Add(AuthencationMethods.None);
                            break;
                        case AuthencationMethods.SocksBoth:
                            types.Add(AuthencationMethods.SocksBoth);
                            break;
                        case AuthencationMethods.SocksEncrypt:
                            types.Add(AuthencationMethods.SocksEncrypt);
                            break;
                        case AuthencationMethods.SocksCompress:
                            types.Add(AuthencationMethods.SocksCompress);
                            break;
                    }
                }
                return types;
            }
            return null;
        }

        /// <summary>
        /// Step 2: 验证客户端
        /// </summary>
        /// <param name="authTypes"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> AuthenticateClientAsync(List<AuthencationMethods> authTypes, CancellationToken cancellationToken = default)
        {

            if (authTypes == null || authTypes.Count == 0)
            {
                await SendAuthTypeAsync(AuthencationMethods.Unsupported, cancellationToken);
                Console.WriteLine("FATAL: No Acceptable AuthType. [AuthTypes.None] needed");
                return false;
            }

            var isServerSideLoginEnabled = UserLoginServiceProvider.Shared.IsUserLoginEnabled;

            if (isServerSideLoginEnabled && authTypes.Contains(AuthencationMethods.Login))
            {
                await SendAuthTypeAsync(AuthencationMethods.Login, cancellationToken);
                return await CheckUsernamePasswordAsync(cancellationToken);
            }
            else if (authTypes.Contains(AuthencationMethods.None))
            {
                await SendAuthTypeAsync(AuthencationMethods.None, cancellationToken);
                return true;
            }
            else
            {
                await SendAuthTypeAsync(AuthencationMethods.Unsupported, cancellationToken);
                return false;
            }
        }

        /// <summary>
        /// Step 2.2: 验证登录用户名和密码
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> CheckUsernamePasswordAsync(CancellationToken cancellationToken = default)
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent();
            var memory = memoryOwner.Memory;

            // .------.------------------.-----------.------------------.-----------.
            // | VER  | USERNAME_LENGTH  |  USERNAME |  PASSWORD_LNEGTH | PASSWORD  |
            // :------+------------------+-----------+------------------+-----------:
            // | 0x01 |      1Byte       |  Variable |     1Byte        | Variable  |
            // '------'------------------'-----------'------------------'-----------'
            int recv = await Socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken);
            if (recv <= 0 || memory.Span[0] != 0x01)
            {
                return false;
            }
            int numusername = memory.Span[1];
            int numpassword = memory.Span[numusername + 2];

            string username = Encoding.ASCII.GetString(memory.Slice(2, numusername).Span);
            string password = Encoding.ASCII.GetString(memory.Slice(numusername + 3, numpassword).Span);
            var user = new SocksUser(username, password, (IPEndPoint)Socket.RemoteEndPoint);

            //.-------.----------.
            //|  VER  |  STATUS  |
            //:-------+----------:
            //| 0x01  |  1Byte   |
            //'-------'--------- '
            // STATUS 0x00 = Succeed 0x01 = Denied
            memory.Span[0] = 0x01;
            if (UserLoginServiceProvider.Shared.CreateHandler().HandleLogin(user))
            {
                memory.Span[1] = 0x00;
                await Socket.SendAsync(memory.Slice(0, 2), SocketFlags.None, cancellationToken);
                return true;
            }
            memory.Span[1] = 0x01;
            await Socket.SendAsync(memory.Slice(0, 2), SocketFlags.None, cancellationToken);
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
                    Socket.Dispose();
                    Socket = null;
                }
            }
        }
    }
}
