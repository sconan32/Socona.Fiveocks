using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Socona.Fiveocks.Core;
using Socona.Fiveocks.Encryption;
using Socona.Fiveocks.Services;
using Socona.Fiveocks.TCP;

namespace Socona.Fiveocks.SocksProtocol
{

    public class SocksRequest : IRequest
    {
        private List<IPAddress> _ipAddresses;

        public SocksAddressType AddressType { get; set; }

        public StreamTypes StreamType { get; private set; }

        public string Address { get; set; }

        public int Port { get; set; }

        public EndPoint InboundEndPoint { get; set; }

        public SockStatus Error { get; set; }

        public ProtocolType TransportProtocolType { get; set; } = ProtocolType.Tcp;

        public SocksRequest(StreamTypes type, SocksAddressType addrtype, IPAddress ipAddress, string address, int port)
        {
            AddressType = addrtype;
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
            if (AddressType == SocksAddressType.Domain)
            {
                var addrs = await domainResolvingService.ResolveDomainAsync(Address);
                foreach (var addr in addrs)
                {
                    _ipAddresses.Add(addr);
                }
            }
        }
        public IEnumerable<IPAddress> IPAddresses => _ipAddresses;


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
            memory.Span[3] = (byte)AddressType;

            int headerIdx = 4;
            if (AddressType == SocksAddressType.Domain)
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
                if (AddressType == SocksAddressType.IP && ipaddr.AddressFamily == AddressFamily.InterNetwork)
                {
                    //+-------+-------+-------+-------+----------+----------+
                    //| VER   |  REP  |  RSV  |  ATYP | DST.ADDR | DST.PORT |
                    //+-------+-------+-------+-------+----------+----------+
                    //| 1Byte | 1Byte |  0x00 | 1Byte |  4Bytes  |  2Bytes  |
                    //+-------+-------+-------+-------+----------+----------+
                    ipaddr.TryWriteBytes(memory.Slice(headerIdx, 4).Span, out _);
                }
                else if (AddressType == SocksAddressType.IPv6 && ipaddr.AddressFamily == AddressFamily.InterNetworkV6)
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

        public override string ToString()
        {
            return AddressType switch
            {
                SocksAddressType.Domain => $"{InboundEndPoint}->[{TransportProtocolType}] {Address}({IPAddresses?.FirstOrDefault()}):{Port}",
                _ => $"{InboundEndPoint}->[{TransportProtocolType}] {Address}:{Port}",
            };
        }
    }

}
