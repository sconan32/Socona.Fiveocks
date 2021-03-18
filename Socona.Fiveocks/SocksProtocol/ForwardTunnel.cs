using Socona.Fiveocks.Socks;
using Socona.ToolBox.Tools;
using System;
using System.Buffers;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Socona.Fiveocks.SocksProtocol
{

    public class ForwardTunnel<TInbound, TOutbound> : IForwardingTunnel
        where TInbound : IInboundEntry
        where TOutbound : IOutboundEntry
    {
        public SocksRequest Request { get; set; }

        public TInbound Inbound { get; set; }

        public TOutbound Outbound { get; set; }

        public BandwidthCounter OutCounter { get; set; }

        public BandwidthCounter InCounter { get; set; }

        public bool IsCompleted { get; set; } = false;

        public ForwardTunnel(SocksRequest request)
        {
            Request = request;
        }

        public virtual async Task<long> ForwardAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (!await Outbound.ConnectAsync(cancellationToken))
                {
                    Request.Error = SockStatus.HostUnreachable;
                }
                using var localMemoryOwner = MemoryPool<byte>.Shared.Rent();
                var localMemory = localMemoryOwner.Memory;

                var requestLength = Request.MakeSocks5Request(localMemory, true);
                await Inbound.SendAsync(localMemory.Slice(0, requestLength), cancellationToken);

                if (Request.Error != SockStatus.Granted)
                {
                    return 0;
                }

                using var remoteMemoryOwner = MemoryPool<byte>.Shared.Rent();
                var remoteMemory = remoteMemoryOwner.Memory;

                var downloadingTask = Task.Run(async () =>
                {
                    var remoteCount = await Outbound.ReceiveAsync(remoteMemory, cancellationToken);
                    while (remoteCount > 0)
                    {
                        InCounter?.AddBytes(remoteCount);
                        await Inbound.SendAsync(remoteMemory.Slice(0, remoteCount), cancellationToken);
                        remoteCount = await Outbound.ReceiveAsync(remoteMemory, cancellationToken);
                    }

                }, cancellationToken);

                var uploadingTask = Task.Run(async () =>
                {
                    var localCount = await Inbound.ReceiveAsync(localMemory, cancellationToken);
                    while (localCount > 0)
                    {
                        OutCounter?.AddBytes(localCount);
                        await Outbound.SendAsync(localMemory.Slice(0, localCount), cancellationToken);
                        localCount = await Inbound.ReceiveAsync(localMemory, cancellationToken);
                    }
                }, cancellationToken);
                await Task.WhenAll(uploadingTask, downloadingTask);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                IsCompleted = true;
            }
            return InCounter?.TotalBytes ?? 0 + OutCounter?.TotalBytes ?? 0;
        }
    }
}

