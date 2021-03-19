using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socona.Fiveocks.SocksProtocol
{
    //class Socks5Tunnel : ForwardingTunnel<SocksInboundEntry, Socks5OutboundEntry>
    //{
    //    public Socks5Tunnel(SocksRequest request, SocksInboundEntry inbound, string proxyAddress, int proxyPort, SocksUser user = null) : base(request)
    //    {
    //        Inbound = inbound;
    //        Outbound = Outbound = new Socks5OutboundEntry(request, "210.30.97.227", 10089, user)

    //        if (request.IPAddresses.Count == 0)
    //        {
    //            var addresses = DomainResolvingServiceProvider.Shared.CreateDomainResolveingService().ResolveDomain(request.Address);
    //            foreach (var addr in addresses)
    //            {
    //                request.IPAddresses.Add(addr);
    //            }
    //        }
    //    }
    //}
}
