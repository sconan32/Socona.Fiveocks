using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Socona.Fiveocks.SocksProtocol
{
    public class DirectOutboundEntry : IOutboundEntry
    {
        public SocksRequest Request { get; set; }

        public Socket Socket { get; private set; }


        public string DisplayName { get; } = "DIRECT";

        public DirectOutboundEntry(SocksRequest request)
        {
            Request = request;
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }


        public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            foreach (var ipaddr in Request.IPAddresses)
            {
                try
                {
                    await Socket.ConnectAsync(new IPEndPoint(ipaddr, Request.Port), cancellationToken);
                    if (Socket.Connected)
                    {
                        return true;
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return false;
        }

        public async Task<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return await Socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
        }

        public async Task<int> SendAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return await Socket.SendAsync(buffer, SocketFlags.None, cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Socket != null)
                {
                    Socket.Close();
                    Socket = null;
                }
            }
        }
    }
}
