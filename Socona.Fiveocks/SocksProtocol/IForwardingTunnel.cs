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
    public interface IForwardingTunnel
    {
        Task<long> ForwardAsync(CancellationToken cancellationToken = default);

        BandwidthCounter InCounter { get; set; }

        BandwidthCounter OutCounter { get; set; }

        bool IsCompleted { get; set; }

    }
}
