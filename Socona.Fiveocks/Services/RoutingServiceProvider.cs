using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socona.Fiveocks.Services
{
    public class RoutingServiceProvider
    {
        public static RoutingServiceProvider Shared { get; } = new RoutingServiceProvider();

        private OutboundEntryService _fts { get; } = new OutboundEntryService();
        private RoutingServiceProvider() { }

        public OutboundEntryService CreateService()
        {
            return _fts;
        }
    }
}
