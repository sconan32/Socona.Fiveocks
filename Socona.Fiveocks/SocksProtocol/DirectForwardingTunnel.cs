using Socona.Fiveocks.Services;
using Socona.Fiveocks.Socks;
using Socona.ToolBox.Tools;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Socona.Fiveocks.SocksProtocol
{
    public class DirectForwardingTunnel : ForwardTunnel<SocksInboundEntry, DirectOutboundEntry>
    {
        public DirectForwardingTunnel(SocksRequest request, SocksInboundEntry inbound) : base(request)
        {
            Inbound = inbound;
            Outbound = new DirectOutboundEntry(request);

            if (request.IPAddresses.Count == 0)
            {
                var addresses = DomainResolvingServiceProvider.Shared.CreateDomainResolveingService().ResolveDomain(request.Address);
                foreach (var addr in addresses)
                {
                    request.IPAddresses.Add(addr);
                }
            }
        }
    }
}
