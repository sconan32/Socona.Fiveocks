using Socona.Fiveocks.Socks;
using Socona.Fiveocks.SocksProtocol;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Socona.Fiveocks.Plugin
{
    public abstract class ConnectSocketOverrideHandler : PluginBase
    {
        /// <summary>
        /// Override the connection, to do whatever you want with it. Client is a wrapper around a socket.
        /// </summary>
        /// <param name="sr">The original request params.</param>
        /// <returns></returns>
        public abstract Socket OnConnectOverride(SocksRequest sr);

        public abstract Task<bool> OnConnectOverrideAsync(IInboundEntry local, SocksRequest request, CancellationToken cancellationToken);
    }
}
