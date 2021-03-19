using System;

namespace Socona.Fiveocks.Plugin
{
    public class ConnectHandlerExample : ConnectHandler
    {
        public override bool OnConnect(SocksProtocol.SocksRequest Request)
        {
            //Compare data.
            if (Request.Address.Contains("74.125.224")) //Google.com IP
            {
                Console.WriteLine("Redirecting traffic from {0} to yahoo.com.", Request.Address);
                Request.Address = "www.yahoo.com";
                Request.Type = SocksProtocol.AddressType.Domain;
            }
            //Allow the connection.
            return true;
        }
        private bool enabled = false;
        public override bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public override string Name { get => nameof(ConnectHandlerExample); set => throw new NotImplementedException(); }
    }
}
