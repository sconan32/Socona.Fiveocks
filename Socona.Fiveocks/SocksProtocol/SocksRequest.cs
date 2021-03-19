using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Socona.Fiveocks.Encryption;
using Socona.Fiveocks.Services;
using Socona.Fiveocks.TCP;

namespace Socona.Fiveocks.SocksProtocol
{

    public class SocksRequest
    {
        private List<IPAddress> _ipAddresses;
        public AddressType Type { get; set; }
        public StreamTypes StreamType { get; private set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public SockStatus Error { get; set; }
        public SocksRequest(StreamTypes type, AddressType addrtype, IPAddress ipAddress, string address, int port)
        {
            Type = addrtype;
            StreamType = type;
            Address = address;
            Port = port;
            _ipAddresses = new List<IPAddress>();
            if (ipAddress != null)
            {
                _ipAddresses.Add(ipAddress);
            }
        }

        public async Task ResolveDomainAsync(DomainResolvingService domainResolvingService)
        {
            if (Type == AddressType.Domain)
            {
                var addrs = await domainResolvingService.ResolveDomainAsync(Address);
                foreach (var addr in addrs)
                {
                    _ipAddresses.Add(addr);
                }
            }
        }
        public List<IPAddress> IPAddresses => _ipAddresses;


        public int MakeResponsePackage(Memory<byte> memory)
        {
            //+-------+-------+-------+-------+----------+----------+
            //| VER   |  REP  |  RSV  |  ATYP | DST.ADDR | DST.PORT |
            //+-------+-------+-------+-------+----------+----------+
            //| 1Byte | 1Byte |  0x00 | 1Byte | Variable |  2Bytes  |
            //+-------+-------+-------+-------+----------+----------+
            //REP = SockStatus.XXX
            return MakeSocks5Package(memory, isNetToHost: true, (byte)Error);

        }

        public int MakeRequestPackage(Memory<byte> memory)
        {
            //+-------+-------+-------+-------+----------+----------+
            //| VER   |  CMD  |  RSV  |  ATYP | DST.ADDR | DST.PORT |
            //+-------+-------+-------+-------+----------+----------+
            //| 1Byte | 1Byte |  0x00 | 1Byte | Variable |  2Bytes  |
            //+-------+-------+-------+-------+----------+----------+
            // CMD = StreamType.XXX

            return MakeSocks5Package(memory, isNetToHost: false, (byte)StreamType);
        }
        private int MakeSocks5Package(Memory<byte> memory, bool isNetToHost, byte at1)
        {
            memory.Span[0] = 0x05;
            memory.Span[1] = at1;
            memory.Span[2] = 0x00;
            memory.Span[3] = (byte)Type;

            int headerIdx = 4;
            if (Type == AddressType.Domain)
            {
                //+-------+-------+-------+-------+----------+----------+
                //| VER   |  REP  |  RSV  |  ATYP | DST.ADDR | DST.PORT |
                //+-------+-------+-------+-------+----------+----------+
                //| 1Byte | 1Byte |  0x00 | 1Byte | Variable |  2Bytes  |
                //+-------+-------+-------+-------+----------+----------+

                int bytesNeeded = Encoding.ASCII.GetByteCount(Address);
                memory.Span[headerIdx++] = (byte)bytesNeeded;
                if (MemoryMarshal.TryGetArray(memory.Slice(headerIdx, bytesNeeded), out ArraySegment<byte> arraySeg))
                {
                    Encoding.ASCII.GetBytes(Address, 0, Address.Length, arraySeg.Array, arraySeg.Offset);
                    headerIdx += bytesNeeded;
                }
            }
            else if (IPAddress.TryParse(Address, out IPAddress ipaddr))
            {
                if (Type == AddressType.IP && ipaddr.AddressFamily == AddressFamily.InterNetwork)
                {
                    //+-------+-------+-------+-------+----------+----------+
                    //| VER   |  REP  |  RSV  |  ATYP | DST.ADDR | DST.PORT |
                    //+-------+-------+-------+-------+----------+----------+
                    //| 1Byte | 1Byte |  0x00 | 1Byte |  4Bytes  |  2Bytes  |
                    //+-------+-------+-------+-------+----------+----------+
                    ipaddr.TryWriteBytes(memory.Slice(headerIdx, 4).Span, out _);
                }
                else if (Type == AddressType.IPv6 && ipaddr.AddressFamily == AddressFamily.InterNetworkV6)
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

            short port = isNetToHost ? (short)IPAddress.NetworkToHostOrder((short)Port) : (short)IPAddress.HostToNetworkOrder((short)Port);
            if (BitConverter.TryWriteBytes(memory.Span.Slice(headerIdx, 2), port))
            {
                return headerIdx + 2;
            }
            return -1;

        }
        public byte[] GetData(bool networkToHostOrder)
        {
            byte[] data;
            var port = networkToHostOrder ? (short)IPAddress.NetworkToHostOrder((short)Port) : (short)IPAddress.HostToNetworkOrder(Convert.ToInt16(Port));

            if (Type == AddressType.IP)
            {
                data = new byte[10];
                string[] content = IPAddresses[0].ToString().Split('.');
                for (int i = 4; i < content.Length + 4; i++)
                    data[i] = Convert.ToByte(Convert.ToInt32(content[i - 4]));
                Buffer.BlockCopy(BitConverter.GetBytes(port), 0, data, 8, 2);
            }
            else if (Type == AddressType.Domain)
            {
                data = new byte[Address.Length + 7];
                data[4] = Convert.ToByte(Address.Length);
                Buffer.BlockCopy(Encoding.ASCII.GetBytes(Address), 0, data, 5, Address.Length);
                Buffer.BlockCopy(BitConverter.GetBytes(port), 0, data, data.Length - 2, 2);
            }
            else return null;
            data[0] = 0x05;
            data[1] = (byte)Error;
            data[2] = 0x00;
            data[3] = (byte)Type;
            return data;
        }

        public override string ToString()
        {
            return Type switch
            {
                AddressType.Domain => $"[{Type}] {Address}({IPAddresses?[0]}):{Port}",
                _ => $"[{Type}] {Address}:{Port}",
            };
        }
    }

    public enum AuthTypes
    {
        None = 0x00,            //NO AUTHENTICATION REQUIRED 不需要认证       
        GSSAPI = 0x01,            //GSSAPI, 类似SSH的认证协议   (不支持）
        Login = 0x02,           //USERNAME/PASSWORD 用户名密码认证
                                //0x03-0x7F  IANA ASSIGNED 协会保留方法
                                //0x80-0xFE  自定义方法
        SocksCompress = 0x88,   //
        SocksEncrypt = 0x90,
        SocksBoth = 0xFE,
        Unsupported = 0xFF,     // NO ACCEPTABLE METHODS 没有可接受的方法

    }

    public enum SocksVersionTypes
    {
        Socks5 = 0x05,
        Socks4 = 0x01,  //Unsupported
        Zero = 0x00
    }

    public enum StreamTypes
    {
        Stream = 0x01,
        Bind = 0x02,
        UDP = 0x03
    }

    public enum AddressType
    {
        IP = 0x01,
        Domain = 0x03,
        IPv6 = 0x04
    }

    public enum SockStatus
    {
        Granted = 0x00,
        Failure = 0x01,
        NotAllowed = 0x02,
        Unreachable = 0x03,
        HostUnreachable = 0x04,
        Refused = 0x05,
        Expired = 0x06,
        NotSupported = 0x07,
        AddressNotSupported = 0x08,
        LoginRequired = 0x90
    }
}
