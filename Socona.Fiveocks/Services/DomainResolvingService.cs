using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Socona.Fiveocks.Services
{
    public class DomainResolvingService
    {

        public virtual async Task<IPAddress[]> ResolveDomainAsync(string domain)
        {
            return await Dns.GetHostAddressesAsync(domain);
        }
    }
}
