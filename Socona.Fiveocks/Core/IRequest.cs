using Socona.Fiveocks.SocksProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Socona.Fiveocks.Core
{
    public interface IRequest
    {
        IEnumerable<IPAddress> IPAddresses { get; }

        string Address { get; set; }

        int Port { get; set; }

        SocksAddressType AddressType { get; set; }

        ProtocolType TransportProtocolType { get; set; }

    }
}
