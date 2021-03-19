using System;
using System.Net;
using Socona.Fiveocks.SocksProtocol;

namespace Socona.Fiveocks.TCP
{
   
    public class SocksClientEventArgs : EventArgs
    {
        public IInboundEntry Client { get; private set; }
        public SocksClientEventArgs(IInboundEntry client)
        {
            Client = client;
        }
    }

    public class DisconnectEventArgs : EventArgs
    {
        public EndPoint LocalEnd { get; set; }
        public EndPoint RemoteEnd { get; set; }      

        public DisconnectEventArgs(EndPoint local, EndPoint remote)
        {
            this.LocalEnd = local;
            this.RemoteEnd = remote;
        }
    }
}
