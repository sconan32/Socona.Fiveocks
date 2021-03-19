using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socona.Fiveocks.Services
{
    public class DomainResolvingServiceProvider
    {

        public  static DomainResolvingServiceProvider Shared { get; } = new DomainResolvingServiceProvider();
        private DomainResolvingServiceProvider() { }

        public DomainResolvingService CreateDomainResolvingService()
        {
            return new DomainResolvingService();
        }


    }
}
