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
    public abstract class SocksInboundEntry : IInboundEntry
    {
        public Socket Socket { get; set; }

        public IPEndPoint EndPoint { get; set; }


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
            socksRequest.InboundEndPoint = EndPoint;
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
                    request.Error = SocksStatus.HostUnreachable;
                }

                var length = SocksPackageBuilder.Shared.BuildHandShakeResponsePackage(memory, request);
                await SendAsync(memory.Slice(0, length), cancellationToken);

                if (request.Error == SocksStatus.Granted)
                {
                    return true;
                }
            }
            catch (SocketException)
            { }

            return false;
        }


        public virtual async Task<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return await Socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
        }

        public virtual async Task<int> SendAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return await Socket.SendAsync(buffer, SocketFlags.None, cancellationToken);
        }

        public abstract Task<bool> ConnectAsync(CancellationToken cancellationToken = default);


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

            if ((SocksCommand)memory.Span[1] != SocksCommand.TcpStream)
            {
                // not supported;
                Console.WriteLine($"ERROR: Command {(SocksCommand)memory.Span[1]} is Not Supported");
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

            return new SocksRequest(SocksCommand.TcpStream, addressType, ipAddress, domain, port);
        }

        /// <summary>
        /// Step2.1: Send Selected AuthType
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task SendAuthTypeAsync(SocksAuthencation authType, CancellationToken cancellationToken = default)
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(2);
            var memory = memoryOwner.Memory;
            int length = SocksPackageBuilder.Shared.BuildGreetResponsePackage(memory, authType);
            await Socket.SendAsync(memory.Slice(0, length), SocketFlags.None, cancellationToken);
        }

        /// <summary>
        /// Step1: Recv Requested AuthTypes
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<SocksAuthencation>> ReceiveAuthTypesAsync(CancellationToken cancellationToken = default)
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
                List<SocksAuthencation> types = new List<SocksAuthencation>(5);
                for (int i = 2; i < methods + 2; i++)
                {
                    switch ((SocksAuthencation)memory.Span[i])
                    {
                        case SocksAuthencation.Login:
                            types.Add(SocksAuthencation.Login);
                            break;
                        case SocksAuthencation.None:
                            types.Add(SocksAuthencation.None);
                            break;
                        case SocksAuthencation.SocksBoth:
                            types.Add(SocksAuthencation.SocksBoth);
                            break;
                        case SocksAuthencation.SocksEncrypt:
                            types.Add(SocksAuthencation.SocksEncrypt);
                            break;
                        case SocksAuthencation.SocksCompress:
                            types.Add(SocksAuthencation.SocksCompress);
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
        public async Task<bool> AuthenticateClientAsync(List<SocksAuthencation> authTypes, CancellationToken cancellationToken = default)
        {

            if (authTypes == null || authTypes.Count == 0)
            {
                await SendAuthTypeAsync(SocksAuthencation.Unsupported, cancellationToken);
                Console.WriteLine("FATAL: No Acceptable AuthType. [AuthTypes.None] needed");
                return false;
            }

            var isServerSideLoginEnabled = UserLoginServiceProvider.Shared.IsUserLoginEnabled;

            if (isServerSideLoginEnabled && authTypes.Contains(SocksAuthencation.Login))
            {
                await SendAuthTypeAsync(SocksAuthencation.Login, cancellationToken);
                return await CheckUsernamePasswordAsync(cancellationToken);
            }
            else if (authTypes.Contains(SocksAuthencation.None))
            {
                await SendAuthTypeAsync(SocksAuthencation.None, cancellationToken);
                return true;
            }
            else
            {
                await SendAuthTypeAsync(SocksAuthencation.Unsupported, cancellationToken);
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

            int recv = await Socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken);
            var user = SocksPackageParser.Shared.ParseUserLoginRequest(memory, recv);

            bool authenized = UserLoginServiceProvider.Shared.CreateHandler().HandleLogin(user);
            int length = SocksPackageBuilder.Shared.BuildLoginResponsePackage(memory, authenized);
            await Socket.SendAsync(memory.Slice(0, length), SocketFlags.None, cancellationToken);
            return authenized;
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
