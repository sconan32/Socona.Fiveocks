using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socona.Fiveocks.Services
{
    public class DomainResolvingServiceProvider
    {

        public readonly static DomainResolvingServiceProvider Shared = new DomainResolvingServiceProvider();
        private DomainResolvingServiceProvider() { }

        public DomainResolvingService CreateDomainResolveingService()
        {
            return new DomainResolvingService();
        }


    }
}
