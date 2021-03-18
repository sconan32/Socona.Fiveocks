using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Socona.Fiveocks.SocksProtocol
{
    public interface INetEntry : IDisposable
    {
        Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
        Task<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default);
        Task<int> SendAsync(Memory<byte> buffer, CancellationToken cancellationToken = default);
    }
}
