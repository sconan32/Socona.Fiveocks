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
    public class SocksUdpInboundEntry : SocksInboundEntry
    {

        public SocksUdpInboundEntry(IPEndPoint endPoint) 
            : base(new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
        {
            EndPoint = endPoint;
            Socket.DontFragment = true;
        }

        public override async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await Socket.ConnectAsync(EndPoint, cancellationToken);
                return true;
            }
            catch(SocketException)
            {

            }
            return false;
        }


      

        public override Task<int> SendAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return base.SendAsync(buffer, cancellationToken);
        }

        public override Task<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return base.ReceiveAsync(buffer, cancellationToken);
        }
       
    }
}
