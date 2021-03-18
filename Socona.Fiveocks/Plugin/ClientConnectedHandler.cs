using System.Net;
using System.Net.Sockets;
using Socona.Fiveocks.TCP;

namespace Socona.Fiveocks.Plugin
{
    public abstract class ClientConnectedHandler : PluginBase
    {
        /// <summary>
        /// Handle Client connected callback. Useful for IPblocking.
        /// </summary>
        /// <param name="socketClient"></param>
        /// <returns>Return true to allow the connection, return false to deny it.</returns>
        public abstract bool OnConnect(Socket socketClient, IPEndPoint IP);
        
      

       
    }
}
