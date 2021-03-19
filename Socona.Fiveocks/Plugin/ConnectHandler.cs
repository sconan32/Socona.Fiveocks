using Socona.Fiveocks.SocksProtocol;

namespace Socona.Fiveocks.Plugin
{
    public abstract class ConnectHandler : PluginBase
    {
        /// <summary>
        /// Handle request callback.
        /// </summary>
        /// <param name="Request"></param>
        /// <returns>Return true to allow the connection, return false to deny it.</returns>
        public abstract bool OnConnect(SocksRequest Request);
    }
}
