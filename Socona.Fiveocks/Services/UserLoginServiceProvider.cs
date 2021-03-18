using Socona.Fiveocks.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socona.Fiveocks.Services
{
    public  class UserLoginServiceProvider
    {
        public readonly static UserLoginServiceProvider Shared = new UserLoginServiceProvider();
        private UserLoginServiceProvider() { }

        public  LoginHandler CreateHandler()
        {
            return new LoginHandlerExample();
        }

        public  bool IsUserLoginEnabled
        {
            get
            {
                return false;
            }
        }
    }
}
