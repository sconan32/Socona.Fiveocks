﻿using Socona.Fiveocks.Encryption;
using Socona.Fiveocks.Services;
using Socona.Fiveocks.Socks;
using System;
using System.Buffers;
using System.Collections.Generic;
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

        SocksRequest Request { get; set; }


        public SocksInboundEntry(Socket socket)
        {
            Socket = socket;
        }

        public async Task<IForwardingTunnel> CreateForwardingTunnelAsync(CancellationToken token = default)
        {

            var authTypes = await ReceiveAuthTypesAsync(token);

            if (!await AuthenticateClientAsync(authTypes, token))
            {
                return null;
            }
            var socksRequest = await ReceiveSocksRequestAsync(token);

            if (socksRequest == null)
            {
                return null;
            }
           


            return new DirectForwardingTunnel(socksRequest, this);
        }

        public async Task<int> ReceiveAsync(Memory<byte> buffer, CancellationToken token = default)
        {
            return await Socket.ReceiveAsync(buffer, SocketFlags.None, token);
        }

        public async Task<int> SendAsync(Memory<byte> buffer, CancellationToken token = default)
        {
            return await Socket.SendAsync(buffer, SocketFlags.None, token);
        }

        public async Task<bool> ConnectAsync(CancellationToken token = default)
        {
            return await ValueTask.FromResult(true);
        }
        /// <summary>
        /// Step3: Get Client Request
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<SocksRequest> ReceiveSocksRequestAsync(CancellationToken token = default)
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent();
            var memory = memoryOwner.Memory;
            int recv = await Socket.ReceiveAsync(memory, SocketFlags.None, token);

            //+-------+-------+-------+-------+----------+----------+
            //| VER   |  CMD  |  RSV  |  ATYP | DST.ADDR | DST.PORT |
            //+-------+-------+-------+-------+----------+----------+
            //| 1Byte | 1Byte |  0x00 | 1Byte | Variable |  2Bytes  |
            //+-------+-------+-------+-------+----------+----------+
            // CMD: 0x01 = CONNECT 0x02 = BIND 0x03 = UDP forwarding
            // ATYP: 0x01 = IPV4 0x03 = Domain 0x04 = IPV6

            if (recv <= 0 || (HeaderTypes)memory.Span[0] != HeaderTypes.Socks5)
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

            var addressType = (AddressType)memory.Span[3];
            if (addressType == AddressType.IP)
            {
                ipAddress = new IPAddress(memory.Slice(4, 4).Span);
                domain = ipAddress.ToString();
                fwd += 4;
            }
            else if (addressType == AddressType.IPv6)
            {
                ipAddress = new IPAddress(memory.Slice(4, 16).Span);
                domain = ipAddress.ToString();
                fwd += 16;
            }
            else if (addressType == AddressType.Domain)
            {
                int domainlen = memory.Span[4];
                domain = Encoding.ASCII.GetString(memory.Slice(5, domainlen).Span);
                fwd += domainlen + 1;
            }
            else
            {
                return null;
            }
            short portOriginal = BitConverter.ToInt16(memory.Slice(fwd, 2).Span);
            int port = IPAddress.NetworkToHostOrder(portOriginal);

            return new SocksRequest(StreamTypes.Stream, addressType, ipAddress, domain, port);
        }

        /// <summary>
        /// Step2.1: Send Selected AuthType
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task SendAuthTypeAsync(AuthTypes authType, CancellationToken token = default)
        {
            //+------+--------+
            //| VER  | METHOD |
            //+------+--------+
            //| 0x05 |  1Byte |
            //+------+--------+
            using var memoryOwner = MemoryPool<byte>.Shared.Rent(2);
            var memory = memoryOwner.Memory;
            memory.Span[0] = (byte)HeaderTypes.Socks5;
            memory.Span[1] = (byte)authType;
            await Socket.SendAsync(memory.Slice(0, 2), SocketFlags.None, token);
        }

        /// <summary>
        /// Step1: Recv Requested AuthTypes
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<AuthTypes>> ReceiveAuthTypesAsync(CancellationToken token = default)
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent();
            var memory = memoryOwner.Memory;
            int recv = await Socket.ReceiveAsync(memory, SocketFlags.None, token);
            // +----+----------+----------+
            // |VER | NMETHODS | METHODS  |
            // +----+----------+----------+
            // | 1B |    1B    | 1 to 255 |
            // +----+----------+----------+
            HeaderTypes headerType = (HeaderTypes)memory.Span[0];
            if (headerType == HeaderTypes.Socks5)
            {
                int methods = memory.Span[1];
                List<AuthTypes> types = new List<AuthTypes>(5);
                for (int i = 2; i < methods + 2; i++)
                {
                    switch ((AuthTypes)memory.Span[i])
                    {
                        case AuthTypes.Login:
                            types.Add(AuthTypes.Login);
                            break;
                        case AuthTypes.None:
                            types.Add(AuthTypes.None);
                            break;
                        case AuthTypes.SocksBoth:
                            types.Add(AuthTypes.SocksBoth);
                            break;
                        case AuthTypes.SocksEncrypt:
                            types.Add(AuthTypes.SocksEncrypt);
                            break;
                        case AuthTypes.SocksCompress:
                            types.Add(AuthTypes.SocksCompress);
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
        public async Task<bool> AuthenticateClientAsync(List<AuthTypes> authTypes, CancellationToken token = default)
        {

            if (authTypes == null || authTypes.Count == 0)
            {
                await SendAuthTypeAsync(AuthTypes.Unsupported, token);
                Console.WriteLine("FATAL: No Acceptable AuthType. [AuthTypes.None] needed");
                return false;
            }

            var isServerSideLoginEnabled = UserLoginServiceProvider.Shared.IsUserLoginEnabled;

            if (isServerSideLoginEnabled && authTypes.Contains(AuthTypes.Login))
            {
                await SendAuthTypeAsync(AuthTypes.Login, token);
                return await CheckUsernamePasswordAsync(token);
            }
            else if (authTypes.Contains(AuthTypes.None))
            {
                await SendAuthTypeAsync(AuthTypes.None, token);
                return true;
            }
            else
            {
                await SendAuthTypeAsync(AuthTypes.Unsupported, token);
                return false;
            }
        }

        /// <summary>
        /// Step 2.2: 验证登录用户名和密码
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> CheckUsernamePasswordAsync(CancellationToken token = default)
        {
            using var memoryOwner = MemoryPool<byte>.Shared.Rent();
            var memory = memoryOwner.Memory;

            // .------.------------------.-----------.------------------.-----------.
            // | VER  | USERNAME_LENGTH  |  USERNAME |  PASSWORD_LNEGTH | PASSWORD  |
            // :------+------------------+-----------+------------------+-----------:
            // | 0x01 |      1Byte       |  Variable |     1Byte        | Variable  |
            // '------'------------------'-----------'------------------'-----------'
            int recv = await Socket.ReceiveAsync(memory, SocketFlags.None, token);
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
                await Socket.SendAsync(memory.Slice(0, 2), SocketFlags.None, token);
                return true;
            }
            memory.Span[1] = 0x01;
            await Socket.SendAsync(memory.Slice(0, 2), SocketFlags.None, token);
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