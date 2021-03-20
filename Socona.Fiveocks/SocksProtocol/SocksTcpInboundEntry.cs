using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Socona.Fiveocks.SocksProtocol
{
    public class SocksTcpInboundEntry : SocksInboundEntry
    {
        public SocksTcpInboundEntry(Socket socket) : base(socket)
        {
            EndPoint = (IPEndPoint)socket.RemoteEndPoint;
        }

        public override async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            // We get this socket from a TcpListener 
            // It is already connected.
            return await ValueTask.FromResult(true);
        }
    }
}
