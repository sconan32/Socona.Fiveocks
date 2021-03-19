using Socona.Fiveocks.SocksProtocol;

namespace Socona.Fiveocks.Plugin
{
    public class LoginHandlerExample : LoginHandler
    {
        public override bool HandleLogin(SocksUser user)
        {
            return true;// (user.Username == "thrdev" && user.Password == "testing1234" ? LoginStatus.Correct : LoginStatus.Denied);
        }
        //Username/Password Table? Endless possiblities for the login system.
        private bool enabled = false;
        public override bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public override string Name { get => nameof(LoginHandlerExample); set => throw new System.NotImplementedException(); }
    }
}
