using Socona.Fiveocks.Socks;

namespace Socona.Fiveocks.Plugin
{
    public enum LoginStatus
    {
        Denied = 0xFF,
        Correct = 0x00
    }
    public abstract class LoginHandler : PluginBase
    {
        public abstract bool HandleLogin(SocksUser user);
     
    }
}
