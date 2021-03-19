using Socona.Fiveocks.SocksProtocol;
using Socona.ToolBox.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Socona.Fiveocks.SocksProtocol
{
    public interface IForwardingTunnel : IDisposable
    {
        Task<long> ForwardAsync(CancellationToken cancellationToken = default);

        BandwidthCounter InCounter { get; set; }

        BandwidthCounter OutCounter { get; set; }

        IInboundEntry InboundEntry { get; set; }

        IOutboundEntry OutboundEntry { get; set; }

        bool IsCompleted { get; set; }

        SocksRequest Request { get; set; }

    }
}
