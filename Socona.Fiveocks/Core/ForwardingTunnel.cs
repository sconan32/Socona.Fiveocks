using Socona.Fiveocks.SocksProtocol;
using Socona.ToolBox.Tools;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Socona.Fiveocks.Core
{

    public class ForwardingTunnel : IForwardingTunnel
    {


        public IInboundEntry InboundEntry { get; set; }

        public IOutboundEntry OutboundEntry { get; set; }

        public BandwidthCounter OutCounter { get; set; }

        public BandwidthCounter InCounter { get; set; }

        public bool IsCompleted { get; set; } = false;

        public ForwardingTunnel()
        {
          
        }

        public virtual async Task<long> ForwardAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var localMemoryOwner = MemoryPool<byte>.Shared.Rent();
                var localMemory = localMemoryOwner.Memory;

                using var remoteMemoryOwner = MemoryPool<byte>.Shared.Rent();
                var remoteMemory = remoteMemoryOwner.Memory;

                var downloadingTask = Task.Run(async () =>
                {
                    var remoteCount = await OutboundEntry.ReceiveAsync(remoteMemory, cancellationToken);
                    while (remoteCount > 0)
                    {
                        InCounter?.AddBytes(remoteCount);
                        await InboundEntry.SendAsync(remoteMemory.Slice(0, remoteCount), cancellationToken);
                        remoteCount = await OutboundEntry.ReceiveAsync(remoteMemory, cancellationToken);
                    }

                }, cancellationToken);

                var uploadingTask = Task.Run(async () =>
                {
                    var localCount = await InboundEntry.ReceiveAsync(localMemory, cancellationToken);
                    while (localCount > 0)
                    {
                        OutCounter?.AddBytes(localCount);
                        await OutboundEntry.SendAsync(localMemory.Slice(0, localCount), cancellationToken);
                        localCount = await InboundEntry.ReceiveAsync(localMemory, cancellationToken);
                    }
                }, cancellationToken);
                await Task.WhenAll(uploadingTask, downloadingTask);
            }
            catch (SocketException ex)
            {
                //do nothing 
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                IsCompleted = true;
            }
            return InCounter?.TotalBytes ?? 0 + OutCounter?.TotalBytes ?? 0;

        }

        public void Dispose()
        {
            IsCompleted = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (InboundEntry != null)
                {
                    InboundEntry.Dispose();
                    InboundEntry = default;
                }
                if (OutboundEntry != null)
                {
                    OutboundEntry.Dispose();
                    OutboundEntry = default;
                }

            }
        }
    }
}

