using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socona.Fiveocks.Core
{
    interface IServer
    {

        void Start();
        void Stop();

        Task StartAsync();

        Task StopAsync();
    }
}
