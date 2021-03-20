using System;
using System.Threading;
using System.Threading.Tasks;

namespace Socona.Fiveocks.Core
{
    public interface INetEntry : IDisposable
    {
        Task<bool> ConnectAsync(CancellationToken cancellationToken = default);

        Task<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default);

        Task<int> SendAsync(Memory<byte> buffer, CancellationToken cancellationToken = default);
    }
}
