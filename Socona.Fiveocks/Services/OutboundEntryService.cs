using Socona.Fiveocks.Core;
using Socona.Fiveocks.Plugin;
using Socona.Fiveocks.SocksProtocol;
using System.Threading.Tasks;

namespace Socona.Fiveocks.Services
{
    public class OutboundEntryService
    {

        FuckGfwPlugin fgp = new FuckGfwPlugin();
        public Task<IOutboundEntry> CreateOutBoundEntryAsync(IRequest request)
        {
            if (fgp.ShallUseProxy(request))
            {
                var outbound = new Socks5OutboundEntry(request as SocksRequest, "210.30.97.227", 10089, null);

                return Task.FromResult<IOutboundEntry>(outbound);
            }
            return Task.FromResult<IOutboundEntry>(null);
        }
    }
}
