using Socona.Fiveocks.Services;
using Socona.Fiveocks.SocksProtocol;

namespace Socona.Fiveocks.SocksProtocol
{
    //public class DirectTunnel : ForwardingTunnel<SocksInboundEntry, DirectOutboundEntry>
    //{
    //    public DirectTunnel(SocksRequest request, SocksInboundEntry inbound) : base(request)
    //    {
    //        Inbound = inbound;
    //        Outbound = new DirectOutboundEntry(request);

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
