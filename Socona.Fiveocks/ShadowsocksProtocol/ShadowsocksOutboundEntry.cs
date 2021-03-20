using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Socona.Fiveocks.Core;
using Socona.Fiveocks.Encryption;
using Socona.Fiveocks.Plugin;
using Socona.Fiveocks.TCP;

namespace Socona.Fiveocks.ShadowsocksProtocol
{
    public class ShadowsocksOutboundEntry : IOutboundEntry
    {
        public string DisplayName => "SS";

        public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> SendAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
