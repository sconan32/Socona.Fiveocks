using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socona.Fiveocks.Services
{
    public class OutboundEntryServiceProvider
    {
        public static OutboundEntryServiceProvider Shared { get; } = new OutboundEntryServiceProvider();

        private OutboundEntryService _fts { get; } = new OutboundEntryService();
        private OutboundEntryServiceProvider() { }

        public OutboundEntryService CreateService()
        {
            return _fts;
        }
    }
}
