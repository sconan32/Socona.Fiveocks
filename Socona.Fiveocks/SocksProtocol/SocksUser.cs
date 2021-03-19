using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Socona.Fiveocks.Encryption;
using Socona.Fiveocks.Plugin;
using Socona.Fiveocks.TCP;

namespace Socona.Fiveocks.SocksProtocol
{
    
    public class SocksUser
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public IPEndPoint IP { get; private set; }
        public SocksUser(string un, string pw, IPEndPoint ip)
        {
            Username = un;
            Password = pw;
            IP = ip;
        }
    }
}
