using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Socona.Fiveocks.SocksProtocol
{
    public class SocksPackageBuilder
    {
        public static SocksPackageBuilder Shared { get; } = new SocksPackageBuilder();
        private SocksPackageBuilder() { }

        public int BuildGreetRequestPackage(Memory<byte> memory)
        {
            //+-------+----------+----------+
            //|  VER  | NMETHODS |  METHODS |
            //+-------+----------+----------+
            //| 1Byte |   1Byte  | 1 to 255 |
            //+-------+----------+----------+
            //Ver Socks4=0x01 Sock5=0x05
            memory.Span[0] = (byte)SocksVersions.Socks5;
            memory.Span[1] = 0x02;
            memory.Span[2] = (byte)SocksAuthencation.None;
            memory.Span[3] = (byte)SocksAuthencation.Login;
            return 4;
        }


        public int BuildGreetResponsePackage(Memory<byte> memory, SocksAuthencation method)
        {
            //+------+--------+
            //| VER  | METHOD |
            //+------+--------+
            //| 0x05 |  1Byte |
            //+------+--------+          
            memory.Span[0] = (byte)SocksVersions.Socks5;
            memory.Span[1] = (byte)method;
            return 2;
        }

        public int BuildLoginRequestPacage(Memory<byte> memory, SocksUser user)
        {
            // .------.------------------.-----------.------------------.-----------.
            // | VER  | USERNAME_LENGTH  |  USERNAME |  PASSWORD_LNEGTH | PASSWORD  |
            // :------+------------------+-----------+------------------+-----------:
            // | 0x01 |      1Byte       |  Variable |     1Byte        | Variable  |
            // '------'------------------'-----------'------------------'-----------'

            memory.Span[0] = 0x01;
            int usernameLength = Encoding.ASCII.GetByteCount(user.Username);
            int passwordLength = Encoding.ASCII.GetByteCount(user.Password);
            if (usernameLength >= 128 || passwordLength >= 128)
            {
                throw new ArgumentOutOfRangeException();
            }
            memory.Span[1] = (byte)usernameLength;
            if (MemoryMarshal.TryGetArray(memory.Slice(2, usernameLength), out ArraySegment<byte> arraySeg1))
            {
                Encoding.ASCII.GetBytes(user.Username, 0, user.Username.Length, arraySeg1.Array, arraySeg1.Offset);
            }

            memory.Span[2 + usernameLength] = (byte)passwordLength;
            if (MemoryMarshal.TryGetArray(memory.Slice(3 + usernameLength, passwordLength), out ArraySegment<byte> arraySeg2))
            {
                Encoding.ASCII.GetBytes(user.Password, 0, user.Password.Length, arraySeg2.Array, arraySeg2.Offset);
            }

            return 3 + usernameLength + passwordLength;
        }
        public int BuildLoginResponsePackage(Memory<byte> memory, bool authenized)
        {
            //.-------.----------.
            //|  VER  |  STATUS  |
            //:-------+----------:
            //| 0x01  |  1Byte   |
            //'-------'--------- '
            // STATUS 0x00 = Succeed 0x01 = Denied
            memory.Span[0] = 0x01;
            if (authenized)
            {
                memory.Span[1] = 0x00;
            }
            else
            {
                memory.Span[1] = 0x01;
            }
            return 2;

        }


        public int BuildHandShakeResponsePackage(Memory<byte> memory, SocksRequest request)
        {
            //+-------+-------+-------+-------+----------+----------+
            //| VER   |  REP  |  RSV  |  ATYP | DST.ADDR | DST.PORT |
            //+-------+-------+-------+-------+----------+----------+
            //| 1Byte | 1Byte |  0x00 | 1Byte | Variable |  2Bytes  |
            //+-------+-------+-------+-------+----------+----------+
            //REP = SockStatus.XXX
            return MakeSocks5Package(memory, isNetToHost: true, (byte)request.Error, request);

        }

        public int BuildHandShakeRequestPackage(Memory<byte> memory, SocksRequest request)
        {
            //+-------+-------+-------+-------+----------+----------+
            //| VER   |  CMD  |  RSV  |  ATYP | DST.ADDR | DST.PORT |
            //+-------+-------+-------+-------+----------+----------+
            //| 1Byte | 1Byte |  0x00 | 1Byte | Variable |  2Bytes  |
            //+-------+-------+-------+-------+----------+----------+
            // CMD = StreamType.XXX

            return MakeSocks5Package(memory, isNetToHost: false, (byte)request.StreamType, request);
        }

        private int MakeSocks5Package(Memory<byte> memory, bool isNetToHost, byte at1, SocksRequest request)
        {
            memory.Span[0] = 0x05;
            memory.Span[1] = at1;
            memory.Span[2] = 0x00;
            memory.Span[3] = (byte)request.AddressType;

            int headerIdx = 4;
            if (request.AddressType == SocksAddressType.Domain)
            {
                //+-------+-------+-------+-------+----------+----------+
                //| VER   |  REP  |  RSV  |  ATYP | DST.ADDR | DST.PORT |
                //+-------+-------+-------+-------+----------+----------+
                //| 1Byte | 1Byte |  0x00 | 1Byte | Variable |  2Bytes  |
                //+-------+-------+-------+-------+----------+----------+

                int bytesNeeded = Encoding.ASCII.GetByteCount(request.Address);
                memory.Span[headerIdx++] = (byte)bytesNeeded;
                if (MemoryMarshal.TryGetArray(memory.Slice(headerIdx, bytesNeeded), out ArraySegment<byte> arraySeg))
                {
                    Encoding.ASCII.GetBytes(request.Address, 0, request.Address.Length, arraySeg.Array, arraySeg.Offset);
                    headerIdx += bytesNeeded;
                }
            }
            else if (IPAddress.TryParse(request.Address, out IPAddress ipaddr))
            {
                if (request.AddressType == SocksAddressType.IP && ipaddr.AddressFamily == AddressFamily.InterNetwork)
                {
                    //+-------+-------+-------+-------+----------+----------+
                    //| VER   |  REP  |  RSV  |  ATYP | DST.ADDR | DST.PORT |
                    //+-------+-------+-------+-------+----------+----------+
                    //| 1Byte | 1Byte |  0x00 | 1Byte |  4Bytes  |  2Bytes  |
                    //+-------+-------+-------+-------+----------+----------+
                    ipaddr.TryWriteBytes(memory.Slice(headerIdx, 4).Span, out _);
                }
                else if (request.AddressType == SocksAddressType.IPv6 && ipaddr.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    //+-------+-------+-------+-------+----------+----------+
                    //| VER   |  REP  |  RSV  |  ATYP | DST.ADDR | DST.PORT |
                    //+-------+-------+-------+-------+----------+----------+
                    //| 1Byte | 1Byte |  0x00 | 1Byte | 16Bytes  |  2Bytes  |
                    //+-------+-------+-------+-------+----------+----------+
                    ipaddr.TryWriteBytes(memory.Slice(headerIdx, 16).Span, out _);
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                return -1;
            }

            short port = isNetToHost ? (short)IPAddress.NetworkToHostOrder((short)request.Port) : (short)IPAddress.HostToNetworkOrder((short)request.Port);
            if (BitConverter.TryWriteBytes(memory.Span.Slice(headerIdx, 2), port))
            {
                return headerIdx + 2;
            }
            return -1;

        }
    }
}