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

        public SocksCommand StreamType { get; private set; }

        public string Address { get; set; }

        public int Port { get; set; }

        public EndPoint InboundEndPoint { get; set; }

        public SocksStatus Error { get; set; }

        public ProtocolType TransportProtocolType { get; set; } = ProtocolType.Tcp;

        public SocksRequest(SocksCommand type, SocksAddressType addrtype, IPAddress ipAddress, string address, int port)
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
